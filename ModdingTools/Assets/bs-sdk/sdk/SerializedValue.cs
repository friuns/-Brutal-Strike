using System;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Text.RegularExpressions;
using fastJSON;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;


[Serializable]
public class SerializedValue:SerializedType 
{
    public string reference; //trigger event code naming
    public Object objectReference;
    public SerializedValue(){}
    public SerializedValue(object o)
    {
        SetValue(o);
    }
    public SerializedValue(object o,Type Type)
    {
        SetValue(o, Type);
    }
    
    public override string ToString()
    {
        var name = value is Enum ? value.GetType().Name + "." + value : value is string ? "\"" + value + "\"" : value is null ? "null" : value.GetType().IsPrimitive ? value.ToString().ToLower() : "new " + value.GetType().Name + Regex.Replace(value.ToString(), @"(\.\d*)", "$&f");
        return name;
    }
    public string json;
    private void SetValue(object o,Type t=null)
    {
        if (type == null)
            type = t ?? o?.GetType();
        if (isReference)
            objectReference = (Object)o;
        else
            json = JSON.ToJSON(o);

    }
    private bool isReference => typeof(Object).IsAssignableFrom(type);
    public object value { get { return GetValue(); } set { SetValue(value); } }
    public object GetValue()
    {
        if (isReference)
            return objectReference;
        else
            return JSON.ToObject(json,type);
    }
    
}