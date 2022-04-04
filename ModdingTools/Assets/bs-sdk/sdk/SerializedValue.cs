using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

public enum PropertyType
{
    Generic = -1,
    Integer = 0,
    Boolean = 1,
    Float = 2,
    String = 3,
    Color = 4,
    ObjectReference = 5,
    LayerMask = 6,
    Enum = 7,
    Vector2 = 8,
    Vector3 = 9,
    Vector4 = 10,
    Rect = 11,
    ArraySize = 12,
    Character = 13,
    AnimationCurve = 14,
    Bounds = 15,
    Gradient = 16,
    Quaternion = 17
}

[Serializable]
public class SerializedValue:SerializedType
{
    public string reference;
    public PropertyType Type;
    
    public AnimationCurve animationCurveValue;
    public bool boolValue;
    public Bounds boundsValue;
    public Color colorValue;
    public double doubleValue;
    public int enumValue;
    public float floatValue;
    public int intValue;
    public long longValue;
    public Object objectReferenceValue;
    public Quaternion quaternionValue;
    public Rect rectValue;
    public string stringValue;
    public Vector2 vector2Value;
    public Vector3 vector3Value;
    public Vector4 vector4Value;
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
        var name = value is Enum ? value.GetType().Name + "." + value : value is string ? "\"" + value + "\"" : value?.ToString() ?? "null";
        return name;
    }
    private void SetValue(object o,Type t=null)
    {
        if (type == null)
            type = t ?? o?.GetType();
        
        if (o is float f)
        {
            floatValue = f;
            Type = PropertyType.Float;
        }
        if (o is int i)
        {
            intValue = i;
            Type = PropertyType.Integer;
        }
        else if (o is Enum e)
        {
            enumValue = Convert.ToInt32(e);
            Type = PropertyType.Enum;
        }
        else
        {
            objectReferenceValue = o as Object;
            Type = PropertyType.ObjectReference;
            if (type == null)
                type = objectReferenceValue?.GetType();
        }
            
    }
    public object value { get { return GetValue(); } set{SetValue(value);}}
    public object GetValue()
    {
        switch (Type)
        {
            case PropertyType.Integer:
                return intValue;
            case PropertyType.Boolean:
                return boolValue;
            case PropertyType.Float:
                return floatValue;
            case PropertyType.String:
                return stringValue;
            case PropertyType.Color:
                return colorValue;
            case PropertyType.ObjectReference:
                return objectReferenceValue;
            case PropertyType.LayerMask:
                // ?
                break;
            case PropertyType.Enum:
            {
                var value = Enum.ToObject(type, enumValue);
                return value;
            }
            case PropertyType.Vector2:
                return vector2Value;
            case PropertyType.Vector3:
                return vector3Value;
            case PropertyType.Vector4:
                return vector4Value;
            case PropertyType.Rect:
                return rectValue;
            case PropertyType.ArraySize:
                // ?
                break;
            case PropertyType.Character:
                // ?
                break;
            case PropertyType.AnimationCurve:
                return animationCurveValue;
            case PropertyType.Bounds:
                return boundsValue;
            case PropertyType.Gradient:
                break;
            case PropertyType.Quaternion:
                return quaternionValue;
        }

        return null;
    }
    
}