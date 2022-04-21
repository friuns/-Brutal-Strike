using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

[Serializable]
public class SerializedMember:SerializedType, IEquatable<SerializedMember>
{
    public override string ToString()
    {
        return path;
    }
    [Serializable]
    public class ParameterInfo:SerializedType
    {
        public Type ParameterType { get { return type; } set { type = value; } }
        public string Name;
    }
    public SerializedMember()
    {
    }
    public Type DeclaringType => type;
    public string Prefix = "_";
    public bool exposed;
    public List<SerializedType> indexes = new List<SerializedType>();
    public string fullName;
    public string name;
    // public MemberInfo info;
    public List<ParameterInfo> parameters = new List<ParameterInfo>();
    public string path;
    public string code="";
    public SerializedMember(MethodInfo method, string Path, string postFix = "")
    {
        exposed = method.GetCustomAttributes(typeof(Expose)).Any();


        foreach (var a in method.GetParameters())
            parameters.Add(new ParameterInfo() { ParameterType = a.ParameterType, Name = a.Name });
        name = method.Name;
        fullName = TriggerEvent.MethodName(this);
        type = method.DeclaringType;
        path = $"{Path}{fullName} {postFix}";
    }
    public Type targetType; //target gameobjectonly
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
    
    public IEnumerable<ParameterInfo> GetParameters()
    {
        return parameters;
    }
    public bool Equals(SerializedMember other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return fullName == other.fullName && code == other.code;
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
        unchecked
        {
            return ((fullName != null ? fullName.GetHashCode() : 0) * 397) ^ (code != null ? code.GetHashCode() : 0);
        }
    }
}