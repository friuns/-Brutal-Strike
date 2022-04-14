using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace EditorRuntime
{
public class EditorGUILayout : bs
{
    public static int Popup(string label, int index, string[] paths, params GUILayoutOption[] prms)
    {
        #if UNITY_EDITOR
        if (bs.insideEditor)
            return UnityEditor.EditorGUILayout.Popup(label, index, paths, prms);
        #endif
        #if game
        RootFolder data = GetControlUIData<RootFolder>(paths);
        data.index = data.setIndex ??index;
        data.setIndex = null;
        if (data.rootFolder == null)
        {
            Folder rootFolder = new Folder();
            for (var j = 0; j < paths.Length; j++)
            {
                var path = paths[j];
                var ss = path.Split('/');

                var folder = rootFolder;
                for (int i = 0; i < ss.Length; i++)
                {
                    var name = ss[i];
                    var k = folder.child.FirstOrDefault(a => a.name == name);
                    if (k == null) folder.child.Add(k = new Folder() { name = name });
                    k.parentFolder = folder;
                    k.index = j;
                    folder = k;
                }
            }
            data.currentFolder = data.rootFolder = rootFolder;
        }

        using (BeginHorizontal())
        {
            if (Button(t + label + ":" + paths[index]))
            {
                string search = "";
                ShowWindow(delegate
                {
                    win.SetupWindow(1000);
                    win.isEditorSkin = true;
                    if (paths.Length >3)
                        search = TextField("Search", search);

                    if (data.currentFolder.parentFolder != null && Button("Back"))
                        data.currentFolder = data.currentFolder.parentFolder;

                    foreach (var a in data.currentFolder.child)
                    {
                        if (string.IsNullOrEmpty(search) || a.name.ContainsFastIc(search) || a.child.Any(b=>b.name.ContainsFastIc(search)))
                        {
                            if (a.child.Count > 0 && bs.Button(a.name + " (" + a.child.Count + ")"))
                                data.currentFolder = a;

                            if (a.child.Count == 0 && GlowButton(a.name, index == a.index))
                            {
                                data.setIndex = a.index;
                                Back();
                            }
                        }
                    }
                });
            }
        }

        return data.index;
        #else
        return 0;
#endif
    }
    public class RootFolder
    {
        public Folder currentFolder;
        public Folder rootFolder;
        public int index;
        public int? setIndex;
    }
    public class Folder
    {
        public string name;
        public int index;
        public Folder parentFolder;
        public List<Folder> child = new List<Folder>();
    }
    public class ObjectFieldData
    {
        public Object o;
    }
    public static Object ObjectField(string label, Object value, Type type, bool allowSceneObjects=true)
    {
        #if UNITY_EDITOR
        if (bs.insideEditor)
            return UnityEditor.EditorGUILayout.ObjectField(label, value, type, allowSceneObjects);
        #endif
        #if game
        var d = bs.GetControlUIData<ObjectFieldData>(label);
        if (d.o != null)
            value = d.o;
        d.o = null;
        
        if (GUILayout.Button(label + ":" + ((value as Component)?.GetComponent<ItemBase>()??value)))
        {
            var list = GameObject.FindObjectsOfType<ItemBase>().Where(a => a.GetComponent(type) && a.pernament);
            string search = "";
            ShowWindow(delegate
            {
                win.isEditorSkin = true;
                search = TextField("search", search);
                foreach (ItemBase a in list.Where(a => string.IsNullOrEmpty(search) || (a.ToString().ContainsFastIc(search))).Take(20))
                {
                    if (Button(a.ToString()))
                    {
                        d.o = a.GetComponent(type);
                        Back();
                    }
                }
            });
            
            
        }
        return value;
#else
        return null;
        #endif
    }
    public static object EnumPopup(string label, Enum e)
    {
        #if UNITY_EDITOR
        if (bs.insideEditor)
            return UnityEditor.EditorGUILayout.EnumPopup(label,e);
        #endif
        #if game
        return bs.Toolbar2(label, e);
#else
        return null;
#endif
    }
}
}
