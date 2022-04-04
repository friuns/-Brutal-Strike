using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;
#pragma warning disable 618



 
public class TypeData : ScriptableObject
{
    [Serializable]
    public class ListSerializedMember
    {
        public List<SerializedMember> list = new List<SerializedMember>();
    }
    [Serializable]
    public class TypeDict:SerializableDictionary<string,ListSerializedMember>
    {}

    private HashSet<Type> handled = new HashSet<Type>();
    public List<SerializedMember> Get(Type type)
    {
        #if game
        if (!types.TryGetValue(type.Name, out var actions) || handled.Add(type))
        {
            actions = types[type.Name] = new ListSerializedMember();
            Fill(type, new List<SerializedType>(), actions.list);
            SetDirty();
        }
        #endif
        if(types.TryGetValue(type.Name,out var o))
            return o.list;
        throw new Exception("key not found " + type.Name);
    }
    public TypeDict types = new TypeDict();
    #if game 
    private void Fill(Type type, List<SerializedType> extraParams, List<SerializedMember> actions, string path = "", string code = "")
    {
        
        // extraParams = new List<Type>();


        //cast
        foreach (var t in type.Assembly.GetTypes().Where(a => type.IsAssignableFrom(a) && a != type))
        foreach (var method in GetMethods(t))
            if (method.GetBaseDefinition()?.DeclaringType != type)
                actions.Add(new SerializedMember(method, path + t.Name + "/") { code = code, indexes = extraParams });

        //fields
        var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly;
        if (path == "")
        {
            path ="/";
            foreach (FieldInfo a in type.GetFields(flags).Where(f => !f.IsSpecialName && f.DeclaringType == type && !f.FieldType.IsValueType))
            {
                var nwList = extraParams.ToList();
                var fieldType = a.FieldType;
                var fieldTypeName = fieldType.Name;
                var extraParamsCount = nwList.Count;
                var code1 = code + "." + a.Name;
                if (typeof(IDictionary).IsAssignableFrom(fieldType) || typeof(ISerializableDictionary2).IsAssignableFrom(fieldType))
                {
                    if (!fieldType.IsGenericType)
                        fieldType = fieldType.BaseType;
                    nwList.Add(new SerializedType() { type = fieldType.GenericTypeArguments[0] });
                    code1 += "[{" + extraParamsCount + "}]";
                    fieldType = fieldType.GenericTypeArguments[1];
                    fieldTypeName = fieldType.Name + "[]";
                }

                if (typeof(IList).IsAssignableFrom(fieldType))
                {
                    fieldType = fieldType.GetElementType() ?? fieldType.GenericTypeArguments[0];
                    code1 += "[{" + extraParamsCount + "}]";
                    fieldTypeName = fieldType.Name + "[]";
                    nwList.Add(new SerializedType() { type = typeof(int) });
                }

                Fill(fieldType, nwList,actions, $"{path}{a.Name} {fieldTypeName}/", code1);
            }
        }


        foreach (var method in GetMethods(type))
            actions.Add(new SerializedMember(method, path) { code = code, indexes = extraParams });


        // foreach (var method in Base.GetMethods(type).Where(a => a.CustomAttributes.Any(b => b.AttributeType.Name.StartsWith("PunRPC"))))
        // members.Add(new Member(method,path));
    }
    public static List<MethodInfo>  GetMethods(Type type)
    {
        var list = new List<MethodInfo>();
        GetMethods(type, list);
        return list;

    } 
    public static void GetMethods(Type type,List<MethodInfo> list)
    {
        // IEnumerable<MethodInfo> methodInfos = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Where(a => a.GetCustomAttributes(typeof(Expose)).Any());
        // list.AddRange(methodInfos.OrderBy(a=> a.Name));
        
        IEnumerable<MethodInfo> methodInfos = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Where(a => !a.Name.StartsWithFast("Start","Update","Awake") && !a.IsSpecialName && !a.IsGenericMethodDefinition && a.DeclaringType == type  && a.GetCustomAttribute(typeof(CompilerGeneratedAttribute)) == null);
        list.AddRange(methodInfos.OrderBy(a => a.Name));
        
    }
 #endif   
}

[Serializable]
public class SerializedMember:IEquatable<SerializedMember>
{
    [Serializable]
    public class ParameterInfo:SerializedType
    {
        public Type ParameterType { get { return type; } set { type = value; } }
        public string Name;
    }
    public SerializedMember()
    {
    }
    
    public bool exposed;
    public List<SerializedType> indexes = new List<SerializedType>();
    public string fullName;
    public string name;
    // public MemberInfo info;
    public List<ParameterInfo> parameters = new List<ParameterInfo>();
    public string path;
    public string code;
    public SerializedMember(MethodInfo method, string Path, string postFix = "")
    {
        exposed = method.GetCustomAttributes(typeof(Expose)).Any();


        foreach (var a in method.GetParameters())
            parameters.Add(new ParameterInfo() { ParameterType = a.ParameterType, Name = a.Name });
        fullName = TriggerEvent.MethodName(method);
        name = method.Name;
        path = $"{Path}{fullName} {postFix}";
    }
    public Type targetType;
    public SerializedMember Clone(string c, Type t)
    {
        var clone = (SerializedMember)base.MemberwiseClone();
        clone.code = c + clone.code;
        clone.path = c +"/"+t.Name+ clone.path;
        clone.targetType = t;
        return clone;
    }
    public SerializedMember Clone(string c)
    {
        var clone = (SerializedMember)base.MemberwiseClone();
        clone.code = c + clone.code;
        clone.path = c + clone.path;
        return clone;

    }
    public bool Equals(SerializedMember other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return path == other.path;
    }
    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((SerializedMember)obj);
    }
    public override int GetHashCode()
    {
        return (path != null ? path.GetHashCode() : 0);
    }
}