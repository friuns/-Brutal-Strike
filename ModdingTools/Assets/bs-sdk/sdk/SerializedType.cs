using System;
using System.Reflection;
using UnityEngine;
using Object = UnityEngine.Object;

[Serializable]
public class SerializedType:ISerializationCallbackReceiver
{
    public string m_type;
    // public void OnBeforeSerialize() //executed often
    // {
    //     if (m_type == null) 
    //         m_type = type?.AssemblyQualifiedName;
    // }
    // public void OnAfterDeserialize()
    // {
    //     if (!string.IsNullOrEmpty(m_type))
    //     {
    //         type = Type.GetType(m_type);
    //         if (type == null)
    //             type = typeof(Object);
    //     }
    //
    // }
    
    public void OnBeforeSerialize()
    {
        if (m_type == null && type != null)
            m_type = type?.AssemblyQualifiedName;
    }
    public static Type GetType(string name)
    {
        return Type.GetType(name, null, (assembly, typeName, arg3) => assembly.GetType(typeName) ?? DefaultAssemblies.csharp.GetType(typeName) ?? DefaultAssemblies.csharpPlusin.GetType(typeName)?? typeof(Object));
    }

    public static class DefaultAssemblies
    {
        public static Assembly csharp = Assembly.Load("Assembly-CSharp");
        public static Assembly csharpPlusin = Assembly.Load("Assembly-CSharp-firstpass");
    }

    public void OnAfterDeserialize()
    {
        if (!string.IsNullOrEmpty(m_type))
            try
            {
                type = GetType(m_type);
            }
            catch (Exception)
            {
                // ignored
            }
        // System.AppDomain.CurrentDomain.GetAssemblies();
    }

    private Type backfield;
    public Type type
    {
        get => backfield;
        set
        {
            m_type = null;
            backfield = value;
        }
    }
}