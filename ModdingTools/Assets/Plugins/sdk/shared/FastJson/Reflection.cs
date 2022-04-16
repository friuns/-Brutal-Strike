using System;
using System.Collections.Generic;
using System.Reflection;
using System.Collections;
using System.Text;
using System.Runtime.Serialization;
#if !SILVERLIGHT
using System.Reflection.Emit;
using System.Data;
#endif
using System.Collections.Specialized;
using System.Linq;
using GenericGetter = System.Func<object,object>;
using GenericSetter = System.Func<object,object,object>;
namespace fastJSON
{
    internal struct Getters
    {
        public string Name;
        public GenericGetter Getter;
    }

    internal enum myPropInfoType
    {
        Int,
        Long,
        String,
        Bool,
        DateTime,
        Enum,
        Guid,

        Array,
        ByteArray,
        Dictionary,
        StringKeyDictionary,
        NameValue,
        StringDictionary,
        Hashtable,
#if !SILVERLIGHT
        DataSet,
        DataTable,
#endif
        //Custom,
        Unknown,
    }

    internal struct myPropInfo
    {
        public Type pt;
        public Type bt;
        public Type changeType;
        public GenericSetter setter;
        public GenericGetter getter;
        public Type[] GenericTypes;
        public string Name;
#if net4
        public string memberName;
#endif
        public myPropInfoType Type;
        public bool CanWrite;

        public bool IsClass;
        public bool IsValueType;
        public bool IsGenericType;
        public bool IsStruct;
        public bool IsInterface;
    }
    //public delegate object Serialize(object data);
    //public delegate object Deserialize(object data);
    public class CustomConverter
    {
        public Type t;
        public Func<object, object> serialize;
        public Func<object, object> deserialize;
    }

    internal sealed class Reflection
    {
        // Sinlgeton pattern 4 from : http://csharpindepth.com/articles/general/singleton.aspx
        private static readonly Reflection instance = new Reflection();
        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit
        static Reflection()
        {
        }
        private Reflection()
        {
        }
        public static Reflection Instance { get { return instance; } }

        // public delegate object GenericSetter(object target, object value);
        // public delegate object GenericGetter(object obj);
        private delegate object CreateObject();

        private SafeDictionary<Type, string> _tyname = new SafeDictionary<Type, string>();
        private SafeDictionary<string, Type> _typecache = new SafeDictionary<string, Type>();
        private SafeDictionary<Type, Getters[]> _getterscache = new SafeDictionary<Type, Getters[]>();
        private SafeDictionary<string, Dictionary<string, myPropInfo>> _propertycache = new SafeDictionary<string, Dictionary<string, myPropInfo>>();
        private SafeDictionary<Type, Type[]> _genericTypes = new SafeDictionary<Type, Type[]>();
        private SafeDictionary<Type, Type> _genericTypeDef = new SafeDictionary<Type, Type>();

        #region bjson custom types
        internal UnicodeEncoding unicode = new UnicodeEncoding();
        internal UTF8Encoding utf8 = new UTF8Encoding();
        #endregion

        #region json custom types
        // JSON custom
        //internal SafeDictionary<Type, Serialize> _customSerializer = new SafeDictionary<Type, Serialize>();
        //internal SafeDictionary<Type, Deserialize> _customDeserializer = new SafeDictionary<Type, Deserialize>();
        internal Dictionary<Type, CustomConverter> customTypes = new Dictionary<Type, CustomConverter>();

        internal void RegisterCustomType(Type type, Type t, Func<object, object> serializer, Func<object, object> deserializer)
        {
            if (type != null && serializer != null && deserializer != null)
            {
                customTypes.Add(type, new CustomConverter() { deserialize = deserializer, serialize = serializer, t = t });
                // reset property cache
                Instance.ResetPropertyCache();
            }
        }

        internal bool IsTypeRegistered(Type t)
        {
            if (customTypes.Count == 0)
                return false;
            CustomConverter s;
            return customTypes.TryGetValue(t, out s);
        }
        #endregion

        public Type GetGenericTypeDefinition(Type t)
        {
            Type tt = null;
            if (_genericTypeDef.TryGetValue(t, out tt))
                return tt;
            else
            {
                tt = t.GetGenericTypeDefinition();
                _genericTypeDef.Add(t, tt);
                return tt;
            }
        }

        public Type[] GetGenericArguments(Type t)
        {
            Type[] tt = null;
            if (_genericTypes.TryGetValue(t, out tt))
                return tt;
            else
            {
                tt = t.GetGenericArguments();
                _genericTypes.Add(t, tt);
                return tt;
            }
        }

        public Dictionary<string, myPropInfo> Getproperties(Type type, string typename)
        {
            Dictionary<string, myPropInfo> sd = null;
            if (_propertycache.TryGetValue(typename, out sd))
            {
                return sd;
            }
            else
            {
                var bf = BindingFlags.Public | BindingFlags.Instance;
                sd = new Dictionary<string, myPropInfo>();
                if (JSON.Parameters.enableProperties)
                {
                    PropertyInfo[] pr = type.GetProperties(bf);
                    foreach (PropertyInfo p in pr)
                    {
                        if (p.GetIndexParameters().Length > 0)// Property is an indexer
                            continue;
                    
                        myPropInfo d = CreateMyProp(p.PropertyType, p.Name);
                        d.setter = CreateSetMethod(type, p);
                        if (d.setter != null)
                            d.CanWrite = true;
                        d.getter = CreateGetMethod(type, p);
                    
                        sd.Add(p.Name, d);
                    }
                }


#if new 
                var cached = new TypeCached(type, type.GetFields(bf));
                 
                // FieldInfo[] fi = type.GetFields(bf);
                var fi = cached.memberInfos;
                foreach (var f in fi)
                {
                    myPropInfo d = CreateMyProp(f.type, f.Name);
                    if (f.fi.IsLiteral == false)
                    {
                        
                        d.setter = (a, b) => { f.SetObject(a, b); return a; };
                        // d.setter = CreateSetField(type, f);
                        // if (d.setter != null)
                            d.CanWrite = true;
                            d.getter = f.GetObject;
                        // d.getter = CreateGetField(type, f); 
                        sd.Add(f.Name, d);
                    }
                }
                #else
                FieldInfo[] fi = type.GetFields(bf);
                foreach (var f in fi)
                {
                    myPropInfo d = CreateMyProp(f.FieldType, f.Name);
                    if (f.IsLiteral == false)
                    {
                        d.setter = CreateSetField(type, f);
                        if (d.setter != null)
                            d.CanWrite = true;
                        d.getter = CreateGetField(type, f); 
                        sd.Add(f.Name, d);
                    }
                }
                #endif

                _propertycache.Add(typename, sd);
                return sd;
            }
        }

        private myPropInfo CreateMyProp(Type t, string name)
        {
            myPropInfo d = new myPropInfo();
            myPropInfoType d_type = myPropInfoType.Unknown;

            d.pt = t;
            CustomConverter cc;
            if (customTypes.TryGetValue(t, out cc))
                t = cc.t;

            if (t == typeof(int) || t == typeof(int?)) d_type = myPropInfoType.Int;
            else if (t == typeof(long) || t == typeof(long?)) d_type = myPropInfoType.Long;
            else if (t == typeof(string)) d_type = myPropInfoType.String;
            else if (t == typeof(bool) || t == typeof(bool?)) d_type = myPropInfoType.Bool;
            else if (t == typeof(DateTime) || t == typeof(DateTime?)) d_type = myPropInfoType.DateTime;
            else if (t.IsEnum) d_type = myPropInfoType.Enum;
            else if (t == typeof(Guid) || t == typeof(Guid?)) d_type = myPropInfoType.Guid;
            else if (t == typeof(StringDictionary)) d_type = myPropInfoType.StringDictionary;
            else if (t == typeof(NameValueCollection)) d_type = myPropInfoType.NameValue;
            else if (t.IsArray)
            {
                d.bt = t.GetElementType();
                if (t == typeof(byte[]))
                    d_type = myPropInfoType.ByteArray;
                else
                    d_type = myPropInfoType.Array;
            }
            else if (t.Name.Contains("Dictionary") || t.GetInterface("IDictionary") != null)
            {

                d.GenericTypes = Reflection.Instance.GetGenericArguments(t);
                if (d.GenericTypes.Length > 0 && d.GenericTypes[0] == typeof(string))
                    d_type = myPropInfoType.StringKeyDictionary;
                else
                    d_type = myPropInfoType.Dictionary;
            }
            else if (t == typeof(Hashtable)) d_type = myPropInfoType.Hashtable;


            if (t.IsValueType && !t.IsPrimitive && !t.IsEnum && t != typeof(decimal))
                d.IsStruct = true;

            d.IsInterface = t.IsInterface;
            d.IsClass = t.IsClass;
            d.IsValueType = t.IsValueType;
            if (t.IsGenericType)
            {
                d.IsGenericType = true;
                d.bt = t.GetGenericArguments()[0];
            }

            d.Name = name;
            d.changeType = GetChangeType(t);
            d.Type = d_type;

            return d;
        }

        private Type GetChangeType(Type conversionType)
        {
            if (conversionType.IsGenericType && conversionType.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
                return Reflection.Instance.GetGenericArguments(conversionType)[0];

            return conversionType;
        }

        #region [   PROPERTY GET SET   ]

        public static Type GetType(string fullName)
        {

            foreach (var a in AppDomain.CurrentDomain.GetAssemblies())
            {
                var type = a.GetType(fullName);
                if (type != null)
                    return type;
            }
            return null;
        }

        internal string GetTypeAssemblyName(Type t)
        {
            string val = "";
            if (_tyname.TryGetValue(t, out val))
                return val;
            else
            {
                string s = t.FullName;
                _tyname.Add(t, s);
                return s;
            }
        }

        internal Type GetTypeFromCache(string typename)
        {
            Type val = null;
            if (_typecache.TryGetValue(typename, out val))
                return val;
            else
            {
                Type t = GetType(typename);
                //if (t == null) // RaptorDB : loading runtime assemblies
                //{
                //    t = Type.GetType(typename, (name) => {
                //        return AppDomain.CurrentDomain.GetAssemblies().Where(z => z.FullName == name.FullName).FirstOrDefault();
                //    }, null, true);
                //}
                _typecache.Add(typename, t);
                return t;
            }
        }


        internal object FastCreateInstance(Type objtype)
        {
            return Activator.CreateInstance(objtype);
        }
        internal static GenericSetter CreateSetField(Type type, FieldInfo field)
        {
            return (a, b) => { field.SetValue(a, b); return a; };
        }
        internal static GenericGetter CreateGetField(Type type, FieldInfo fieldInfo)
        {
            return fieldInfo.GetValue;
        }

        internal static GenericSetter CreateSetMethod(Type type, PropertyInfo propertyInfo)
        {
            return (a, b) => { propertyInfo.SetValue(a, b, null); return a; };
        }

        internal static GenericGetter CreateGetMethod(Type type, PropertyInfo propertyInfo)
        {
            return a => propertyInfo.GetValue(a, null);
        }







        internal Getters[] GetGetters(Type type, bool ShowReadOnlyProperties, List<Type> IgnoreAttributes)
        {
            Getters[] val = null;
            if (_getterscache.TryGetValue(type, out val))
                return val;
            //bool isAnonymous = IsAnonymousType(type);

            var bf = BindingFlags.Public | BindingFlags.Instance ;
           
            List<Getters> getters = new List<Getters>();
            if (JSON.Parameters.enableProperties)
            {
                if (ShowReadOnlyProperties)
                    bf |= BindingFlags.NonPublic;
                PropertyInfo[] props = type.GetProperties(bf);
                foreach (PropertyInfo p in props)
                {
                    if (p.GetIndexParameters().Length > 0)
                    {
                        // Property is an indexer
                        continue;
                    }
                    if (!p.CanWrite && (ShowReadOnlyProperties == false)) //|| isAnonymous == false))
                        continue;
                    if (IgnoreAttributes != null)
                    {
                        bool found = false;
                        foreach (var ignoreAttr in IgnoreAttributes)
                        {
                            if (p.IsDefined(ignoreAttr, false))
                            {
                                found = true;
                                break;
                            }
                        }
                        if (found)
                            continue;
                    }

                    GenericGetter g = CreateGetMethod(type, p);
                    if (g != null)
                        getters.Add(new Getters {Getter = g, Name = p.Name});
                }
            }

            FieldInfo[] fi = type.GetFields(bf);
            foreach (var f in fi)
            {
                if (IgnoreAttributes != null)
                {
                    bool found = false;
                    foreach (var ignoreAttr in IgnoreAttributes)
                    {
                        if (f.IsDefined(ignoreAttr, false))
                        {
                            found = true;
                            break;
                        }
                    }
                    if (found)
                        continue;
                }

                if (f.IsLiteral == false)
                {
                    GenericGetter g = CreateGetField(type, f);
                    if (g != null)
                        getters.Add(new Getters { Getter = g, Name = f.Name});
                }
            }
            val = getters.ToArray();
            _getterscache.Add(type, val);
            return val;
        }

        //private static bool IsAnonymousType(Type type)
        //{
        //    // may break in the future if compiler defined names change...
        //    const string CS_ANONYMOUS_PREFIX = "<>f__AnonymousType";
        //    const string VB_ANONYMOUS_PREFIX = "VB$AnonymousType";

        //    if (type == null)
        //        throw new ArgumentNullException("type");

        //    if (type.Name.StartsWith(CS_ANONYMOUS_PREFIX, StringComparison.Ordinal) || type.Name.StartsWith(VB_ANONYMOUS_PREFIX, StringComparison.Ordinal))
        //    {
        //        return type.IsDefined(typeof(CompilerGeneratedAttribute), false);
        //    }

        //    return false;
        //}
        #endregion

        internal void ResetPropertyCache()
        {
            _propertycache = new SafeDictionary<string, Dictionary<string, myPropInfo>>();
        }

        internal void ClearReflectionCache()
        {
            _tyname = new SafeDictionary<Type, string>();
            _typecache = new SafeDictionary<string, Type>();
            _getterscache = new SafeDictionary<Type, Getters[]>();
            _propertycache = new SafeDictionary<string, Dictionary<string, myPropInfo>>();
            _genericTypes = new SafeDictionary<Type, Type[]>();
            _genericTypeDef = new SafeDictionary<Type, Type>();
        }
    }
}
