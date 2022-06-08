using System;
using System.Collections.Generic;

namespace fastJSON
{
public sealed class JSONParameters
{
    /// <summary>
    /// Use the fast GUID format (default = True)
    /// </summary>
    public bool UseFastGuid = true;
    /// <summary>
    /// Serialize null values to the output (default = True)
    /// </summary>
    public bool SerializeNullValues = false;
    /// <summary>
    /// Use the UTC date format (default = True)
    /// </summary>
    public bool UseUTCDateTime = true;
    /// <summary>
    /// Show the readonly properties of types in the output (default = False)
    /// </summary>
    public bool ShowReadOnlyProperties = false;
    /// <summary>
    /// Use the $types extension to optimise the output json (default = True)
    /// </summary>
    public bool UsingGlobalTypes = false;
    /// <summary>
    /// Ignore case when processing json and deserializing 
    /// </summary>
    [Obsolete("Not needed anymore and will always match")]
    public bool IgnoreCaseOnDeserialize = false;
    /// <summary>
    /// Anonymous types have read only properties 
    /// </summary>
    public bool EnableAnonymousTypes = false;
    /// <summary>
    /// Enable fastJSON extensions $types, $type, $map (default = True)
    /// </summary>
    public bool UseExtensions = true;
    /// <summary>
    /// Use escaped unicode i.e. \uXXXX format for non ASCII characters (default = True)
    /// </summary>
    public bool UseEscapedUnicode = true;
    /// <summary>
    /// Output string key dictionaries as "k"/"v" format (default = False) 
    /// </summary>
    public bool KVStyleStringDictionary = false;
    /// <summary>
    /// Output Enum values instead of names (default = False)
    /// </summary>
    public bool UseValuesOfEnums = false;
    /// <summary>
    /// Ignore attributes to check for (default : XmlIgnoreAttribute, NonSerialized)
    /// </summary>
    public List<Type> IgnoreAttributes = new List<Type> { typeof(System.Xml.Serialization.XmlIgnoreAttribute), typeof(NonSerializedAttribute),typeof(JsonNonSerialized) };
    /// <summary>
    /// If you have parametric and no default constructor for you classes (default = False)
    /// 
    /// IMPORTANT NOTE : If True then all initial values within the class will be ignored and will be not set
    /// </summary>
    public bool ParametricConstructorOverride = false;
    /// <summary>
    /// Serialize DateTime milliseconds i.e. yyyy-MM-dd HH:mm:ss.nnn (default = false)
    /// </summary>
    public bool DateTimeMilliseconds = false;
    /// <summary>
    /// Maximum depth for circular references in inline mode (default = 20)
    /// </summary>
    public int SerializerMaxDepth = 20;
    /// <summary>
    /// Inline circular or already seen objects instead of replacement with $i (default = False) 
    /// </summary>
    public bool InlineCircularReferences = true; //score.pos broken 
    /// <summary>
    /// Formatter indent spaces (default = 3)
    /// </summary>
    public byte FormatterIndentSpaces = 3;
    public bool SkipDefaultValues=true;
    public bool enableProperties;

    public void FixValues()
    {
        if (UseExtensions == false) // disable conflicting params
        {
            UsingGlobalTypes = false;
            InlineCircularReferences = true;
        }
        if (EnableAnonymousTypes)
            ShowReadOnlyProperties = true;
    }
}
}