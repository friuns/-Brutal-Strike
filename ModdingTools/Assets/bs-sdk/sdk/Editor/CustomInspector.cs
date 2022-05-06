using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using com.spacepuppyeditor;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = System.Object;

namespace InspectorSearch
{
[CustomEditor(typeof(Base), true)]
[CanEditMultipleObjects]
[InitializeOnLoad]
public partial class CustomInspector : Editor
{
    static CustomInspector()
    {
        SceneView.duringSceneGui += OnSceneUpdate;
        Selection.selectionChanged += SelectionChanged;
        #if !game
        EditorApplication.hierarchyChanged += EditorApplicationOnhierarchyChanged;
#endif
        EditorSceneManager.sceneOpened+= EditorSceneManagerOnsceneOpened;
    }
    private static void EditorSceneManagerOnsceneOpened(Scene scene, OpenSceneMode mode)
    {
        // if (RenderSettings.skybox && RenderSettings.skybox.hideFlags == HideFlags.NotEditable)
            // RenderSettings.skybox = Instantiate(RenderSettings.skybox);
    }
    private static void EditorApplicationOnhierarchyChanged()
    {
        var anim = Selection.activeGameObject?.GetComponent<Animation>();
        if (anim && anim.clip == null)
        {
            var clip = new AnimationClip() { legacy = true, name = anim.name };
            anim.clip = clip;
            anim.AddClip(clip, clip.name);
                
        }
        
        var re = Selection.activeGameObject?.GetComponent<MeshRenderer>();
        if (re && (re.sharedMaterial == null||re.sharedMaterial.hideFlags == HideFlags.NotEditable))
            re.sharedMaterial = Instantiate(re.sharedMaterial ?? new Material(Shader.Find("Standard")));
    }
  
    private static void SelectionChanged()
    {
        
        var gameObject = Selection.activeGameObject;
        if (gameObject)
        {
            
            gameObject.GetComponentNonAlloc<bs>()?.OnEditorSelected();
            
        }
    }


    private static void OnSceneUpdate(SceneView scene)
    {
        var go = Selection.activeGameObject;
        if (go)
            foreach (var ba in go.GetComponents<Base>())
                ba.OnSceneUpdate(scene);
//            var ba = go != null ? go.GetComponent<Base>() : null;

        Handles.BeginGUI();
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Cam"))
            scene.AlignViewToObject(Camera.main?.transform ?? Camera.allCameras[0].transform);


        if (go)
            foreach (var ba in go.GetComponents<Base>())
                ba.OnSceneGui(scene);
//                    

        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();


        Handles.EndGUI();

        Handles.DrawLine(Vector3.zero, Vector3.up / 3);
    }


    void OnEnable()
    {
        EditorApplication.contextualPropertyMenu += OnPropertyContextMenu;
        contextItems.Clear();
        foreach (MemberInfo member in target.GetType().GetMembers(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).OrderBy(a => a.MetadataToken))
        {
            var method = member as MethodInfo;
            if (method != null)
            {
                var customAttributes = method.GetCustomAttributes(true);
                foreach (var _ in customAttributes.OfType<ValidateAttribute>())
                    getters.Add(new Getter {func = Target => method.Invoke(target, null), o = target, name = method.Name});

                foreach (var atr in customAttributes.OfType<ContextMenu>())
                    if (atr.menuItem != "copy ID" && atr.menuItem != "break")
                        contextItems.Add(new ContextItem {method = method, name = atr.menuItem});
            }
            var field = member as FieldInfo;
            if (field != null)
            {
                var customAttributes = field.GetCustomAttributes(true);
                foreach (var _ in customAttributes.OfType<ValidateAttribute>())
                    getters.Add(new Getter {func = field.GetValue, o = target, name = field.Name});
            }
        }
    }
    class ContextItem
    {
        public string name;
        public MethodInfo method;
    }
    public class Getter
    {
        public string name;
        public Func<object, object> func;
        public object o;
    }
    private List<Getter> getters = new List<Getter>();

    List<ContextItem> contextItems = new List<ContextItem>();

    object GetDefaultValue(Type t)
    {
        if (t.IsValueType)
            return Activator.CreateInstance(t);
        return null;
    }
    public static string search { get { return ext.search; } set { ext.search = value; } }

    public override void OnInspectorGUI()
    {
        search = EditorGUILayout.TextField("search", search);

        bs.insideGUI = true;
        bs.insideEditor = true;
        if (search=="" || target is IOnInspectorGUISearch)
        {
            if (target is IOnInspectorGUI io)
                using (new GUILayout.VerticalScope(style: GUI.skin.box))
                    try
                    {
                        io.OnInspectorGUI();
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                    }

            foreach (Getter getter in getters)
            {
                object v = getter.func(getter.o);
                if (IsDefault(v))
                    bs.LabelError(getter.name + " is invalid");
            }
            foreach (ContextItem a in contextItems)
                if (GUILayout.Button(a.name, GUILayout.ExpandWidth(false)))
                {
                    foreach (var b in targets)
                    {
                        a.method.Invoke(b, null);
                        EditorUtility.SetDirty(b);
                    }
                }
        }

        var hide = target is IOnInspectorGUIHide;
        if (hide)
            showAllFields = GUILayout.Toggle(showAllFields, "Show All fields");

        if (showAllFields || !hide)
            DoDrawDefaultInspector(serializedObject);
        
        bs.insideGUI = false;
        bs.insideEditor = false;
        // base.OnInspectorGUI();
    }
    public static bool showAllFields;

    public static Dictionary<string, Object> modified = new Dictionary<string, object>();


    internal bool DoDrawDefaultInspector(SerializedObject obj)
    {
        EditorGUI.BeginChangeCheck();
        obj.UpdateIfRequiredOrScript();
        SerializedProperty sprop = obj.GetIterator();
        for (bool enterChildren = true; sprop.NextVisible(enterChildren); enterChildren = false)
        {
            using (new EditorGUI.DisabledScope("m_Script" == sprop.propertyPath))
            {
                var path = sprop.serializedObject.targetObject.name + "." + sprop.propertyPath;
                GUI.changed = false;
                var fnd = sprop.name.Contains(search, StringComparison.OrdinalIgnoreCase);
                if (search!="" && sprop.hasChildren && !fnd) //draw children manually of serializable if searching 
                {
                    var prop = sprop.Copy();
                    SerializedProperty endProperty = prop.GetEndProperty();
                    while (prop.NextVisible(true) && !SerializedProperty.EqualContents(prop, endProperty))
                    {
                        if (prop.name.Contains(search, StringComparison.OrdinalIgnoreCase))
                            EditorGUILayout.PropertyField(prop, true);
                    }
                }
                else if (search=="" || sprop.hasChildren || fnd)
                {
                    EditorGUILayout.PropertyField(sprop, true);

                    if (GUI.changed)
                    {
                        obj.ApplyModifiedProperties();
                        var value = sprop.GetTargetObjectOfProperty();
                        if (value != null)
                            modified[path] = value;
                    }
                }
            }
        }
        
        obj.ApplyModifiedProperties();
        
        if(search!="")
            foreach (FieldInfo a in obj.targetObject.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
            {
                bs.DrawObject(a.GetValue(obj.targetObject), a.Name);
            }
        return EditorGUI.EndChangeCheck();
    }

    private bool IsDefault(object o)
    {
        if (o == null)
            return true;
        if (o is AnimationCurve)
            return ((AnimationCurve) o).keys.Length == 0;
        if (o is UnityEngine.Object)
            return !(UnityEngine.Object) o;
        if (o is string)
            return (string) o == "";
        if (o is IList)
            return ((IList) o).Count == 0;
        if (Equals(GetDefaultValue(o.GetType()), o))
            return true;
        return false;
    }
    
    

    void OnDestroy()
    {
        EditorApplication.contextualPropertyMenu -= OnPropertyContextMenu;
    }

    void OnPropertyContextMenu(GenericMenu menu, SerializedProperty property)
    {
        // // show a custom menu item only for Vector3 properties
        // if (property.propertyType != SerializedPropertyType.Vector3)
        //     return;
        //
        // // and only when called on a Transform component
        // if (property.serializedObject.targetObject.GetType() != typeof(Transform))
        //     return;

        
        
        if(property.propertyType != SerializedPropertyType.Generic)
            return;
        
        menu.AddItem(new GUIContent("Reset"), false, () =>
        {
            
            object def = Activator.CreateInstance(property.serializedObject.targetObject.GetType());
            // propertyCopy.serializedObject.targetObject.
            // property.serializedObject.targetObject.GetType().GetFields()
            var defVaule = def.GetType().GetField(property.propertyPath).GetValue(def);
            property.SetTargetObjectOfProperty(defVaule);
            property.serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(property.serializedObject.targetObject);
        });
    }
}
}