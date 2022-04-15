using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

public class Expose : Attribute
{
    
}
public interface ISerializableDictionary2
{
        
}
public static class Temp
{
    private static StringBuilder sb = new StringBuilder();
    public static StringBuilder StringBuilder(string s)
    {
        sb.Clear();
        sb.Append(s);
        return sb;

    }
    public static StringBuilder StringBuilder()
    {
        sb.Clear();
        return sb;
    }
}
[SelectionBase]
public class bs : Base,IOnInspectorGUI
{
    public static bool insideGUI;
    public static bool HasChanged<T>(Func<T, T> f, T a)
    {
        return !EqualityComparer<T>.Default.Equals(a, f(a));
    }
    
    public static bool HasChanged<T>(T a,ref T b)
    {
        if (!EqualityComparer<T>.Default.Equals(a, b))
        {
            b = a;
            return true;
        }
        return false;
    }
    public static void LabelError(string s)
    {
        var guiStyle = new GUIStyle(GUI.skin.label);
        guiStyle.normal.textColor = Color.red;
        GUILayout.Label(s, guiStyle);
    }
    public static object DrawObject(object v2, string name)
    {
#if UNITY_EDITOR
        if (v2 is Enum e)
            v2 = EditorGUILayout.EnumPopup(name, e);
        else if (v2 is int)
            v2 = EditorGUILayout.IntField(name, (int)v2);
        else if (v2 is double)
            v2 = EditorGUILayout.FloatField(name, (float)(double)v2);
        else if (v2 is float)
            v2 = EditorGUILayout.FloatField(name, (float)v2);
        else if (v2 is bool)
            v2 = EditorGUILayout.Toggle((bool)v2, name);
        else if (v2 is string)
            v2 = EditorGUILayout.TextField(name, (string)v2);
        else if (v2 is Vector3)
            v2 = EditorGUILayout.Vector3Field(name, (Vector3)v2);
        else if (v2 is Vector2)
            v2 = EditorGUILayout.Vector2Field(name, (Vector2)v2);
#endif
        return v2;
    }
    public const bool sdk =
#if game
        false;
#else
        true;
#endif
    public virtual void OnValidate()
    {
    }
    protected ObsCamera _ObsCamera; 
    protected Animator m_Animator;
    protected  Animator animator { get { return m_Animator ?? (m_Animator = GetComponentInChildren<Animator>()); } set { m_Animator = value; } }
    public static Vector3 ZeroY(Vector3 v, float a = 0)
    {
        v.y *= a;
        return v;
    }
    public void SetDirty(MonoBehaviour g = null)
    {
#if (UNITY_EDITOR)
        UnityEditor.EditorUtility.SetDirty(g ? g : this);
#endif
    }
    public virtual void OnInspectorGUI()
    {
    }
    public static RoomSettings roomSettings=new RoomSettings();
    protected static Player _Player;
    protected static Game _Game;
    protected static Hud _Hud;
    internal List<Collider> levelColliders;
    public static bool insideEditor;

    public virtual void OnEditorGUI()
    {

    }
    public virtual void OnEditorSelected()
    {
    }
}
// public class RoomSettings
// {
//     public bool enableBotSupport=true;
//     public bool enableKnocking;
// }
public class PosRot
{
    public List<PosRot> child = new List<PosRot>();
    public string name;

    public Vector3 pos;
    public Quaternion rot;
    public Vector3 scale = Vector3.one;
    public PosRot(Transform self)
    {
     
        //t = self;
        //parent = self.parent;
        pos = self.localPosition;
        rot = self.localRotation;
        scale = self.localScale;
        name = self.name;
        foreach (Transform t in self)
        {
            child.Add(new PosRot(t));
        }
    }
    public void Restore(Transform t)
    {
       
        if (t == null) return;
        if (t.localPosition != pos || t.localRotation!= rot || t.localScale != t.localScale)
        {
            t.localScale = scale;
            t.localPosition = pos;
            t.localRotation = rot;
            Debug.Log(name + " changed", t);
        }
        List<Transform> used = new List<Transform>();
        foreach(var a in child)
        {
            var nw = t.Cast<Transform>().FirstOrDefault(b => b.name == a.name && !used.Contains(b));
            if (nw)
            {
                used.Add(nw);
                a.Restore(nw);
            }
            else
            {
                Debug.Log("not found " + a.name);
            }
        }
        
    }
}


public class ObjectBase : bs
{
    
}
public class bsNetwork : bs
{
    
}
public class AssetBase : bs
{
    
}
