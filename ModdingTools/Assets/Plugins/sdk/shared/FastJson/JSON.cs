#define SILVERLIGHT
using System;
using System.Collections;
using System.Collections.Generic;
#if !SILVERLIGHT
using System.Data;
#endif
using System.Globalization;
using System.IO;
using System.Collections.Specialized;
using System.Diagnostics;
using UnityEngine;
#if game
using Debug = UnityEngine.Debug;
#endif
public class JsonNonSerialized:Attribute{}

namespace fastJSON
{
    //public delegate object Serialize(object data);
    //public delegate object Deserialize(string data);

public static class JSON
    {
        /// <summary>
        /// Globally set-able parameters for controlling the serializer
        /// </summary>
        public static JSONParameters Parameters = new JSONParameters();
        /// <summary>
        /// Create a formatted json string (beautified) from an object
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string ToNiceJSON(object obj)
        {
            string s = ToJSON(obj, new JSONParameters(){ EnableAnonymousTypes =true}); // use default params

            return Beautify(s);
        }
        /// <summary>
        /// Create a formatted json string (beautified) from an object
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public static string ToNiceJSON(object obj, JSONParameters param)
        {
            string s = ToJSON(obj, param);

            return Beautify(s, param.FormatterIndentSpaces);
        }
        /// <summary>
        /// Create a json representation for an object
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string ToJSON(object obj)
        {
            return ToJSON(obj, Parameters);
        }
        /// <summary>
        /// Create a json representation for an object with parameter override on this call
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public static string ToJSON(object obj, JSONParameters param)
        {
            param.FixValues();
            Type t = null;

            if (obj == null)
                return "null";

            if (obj.GetType().IsGenericType)
                t = Reflection.Instance.GetGenericTypeDefinition(obj.GetType());
            if (t == typeof(Dictionary<,>) || t == typeof(List<>))
                param.UsingGlobalTypes = false;

            // FEATURE : enable extensions when you can deserialize anon types
            if (param.EnableAnonymousTypes) { param.UseExtensions = false; param.UsingGlobalTypes = false; }
            return new JSONSerializer(param).ConvertToJSON(obj);
        }
        /// <summary>
        /// Parse a json string and generate a Dictionary&lt;string,object&gt; or List&lt;object&gt; structure
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public static object Parse(string json)
        {
            return new JsonParser(json).Decode();
        }
#if net4
        /// <summary>
        /// Create a .net4 dynamic object from the json string
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public static dynamic ToDynamic(string json)
        {
            return new DynamicJson(json);
        }
#endif
        /// <summary>
        /// Create a typed generic object from the json
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="json"></param>
        /// <returns></returns>
        public static T ToObject<T>(string json)
        {
            return new deserializer(Parameters).ToObject<T>(json);
        }
        /// <summary>
        /// Create a typed generic object from the json with parameter override on this call
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="json"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public static T ToObject<T>(string json, JSONParameters param)
        {
            return new deserializer(param).ToObject<T>(json);
        }
        /// <summary>
        /// Create an object from the json
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public static object ToObject(string json)
        {
            return new deserializer(Parameters).ToObject(json, null);
        }
        /// <summary>
        /// Create an object from the json with parameter override on this call
        /// </summary>
        /// <param name="json"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public static object ToObject(string json, JSONParameters param)
        {
            return new deserializer(param).ToObject(json, null);
        }
        /// <summary>
        /// Create an object of type from the json
        /// </summary>
        /// <param name="json"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static object ToObject(string json, Type type)
        {
            return new deserializer(Parameters).ToObject(json, type);
        }
        /// <summary>
        /// Create an object of type from the json with parameter override on this call
        /// </summary>
        /// <param name="json"></param>
        /// <param name="type"></param>
        /// <param name="par"></param>
        /// <returns></returns>
        public static object ToObject(string json, Type type, JSONParameters par)
        {
            return new deserializer(par).ToObject(json, type);
        }
        /// <summary>
        /// Fill a given object with the json represenation
        /// </summary>
        /// <param name="input"></param>
        /// <param name="json"></param>
        /// <returns></returns>
        public static Dictionary<string, object> ht;
        public static object FillObject(object input, string json)
        {
            ht = new JsonParser(json).Decode() as Dictionary<string, object>;
            if (ht == null) throw new Exception("couldn not map json");
            return FillObject2(input, ht);
        }
        public static object FillObject2(object input, Dictionary<string, object> ht)
        {
            return new deserializer(Parameters).ParseDictionary(ht, null, input.GetType(), input);
        }
        /// <summary>
        /// Deep copy an object i.e. clone to a new object
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static object DeepCopy(object obj)
        {
            return new deserializer(Parameters).ToObject(ToJSON(obj));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static T DeepCopy<T>(T obj)
        {
            return new deserializer(Parameters).ToObject<T>(ToJSON(obj));
        }

        /// <summary>
        /// Create a human readable string from the json 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string Beautify(string input)
        {
            var i = new string(' ', JSON.Parameters.FormatterIndentSpaces);
            return Formatter.PrettyPrint(input, i);
        }
        /// <summary>
        /// Create a human readable string from the json with specified indent spaces
        /// </summary>
        /// <param name="input"></param>
        /// <param name="spaces"></param>
        /// <returns></returns>
        public static string Beautify(string input, byte spaces)
        {
            var i = new string(' ', spaces);
            return Formatter.PrettyPrint(input, i);
        }
        /// <summary>
        /// Register custom type handlers for your own types not natively handled by fastJSON
        /// </summary>
        /// <param name="type"></param>
        /// <param name="serializer"></param>
        /// <param name="deserializer"></param>
        public static void RegisterCustomType<T1, T2>(Func<T1, T2> serializer, Func<T2, T1> deserializer)
        {
            Reflection.Instance.RegisterCustomType(typeof(T1), typeof(T2), a => serializer((T1)a), a => deserializer((T2)a));
        }
        /// <summary>
        /// Clear the internal reflection cache so you can start from new (you will loose performance)
        /// </summary>
        public static void ClearReflectionCache()
        {
            Reflection.Instance.ClearReflectionCache();
        }

        //internal static long CreateLong(string s, int index, int count)
        //{
        //    return deserializer.CreateLong(s, index, count);
        //}
    }

    public class deserializer
    {
        public deserializer(JSONParameters param)
        {
            _params = param;
        }

        private JSONParameters _params;
        private bool _usingglobals = false;
        private Dictionary<object, int> _circobj = new Dictionary<object, int>();
        private Dictionary<int, object> _cirrev = new Dictionary<int, object>();

        public T ToObject<T>(string json)
        {
            Type t = typeof(T);
            var o = ToObject(json, t);

            if (t.IsArray)
            {
                if ((o as ICollection).Count == 0) // edge case for "[]" -> T[]
                {
                    Type tt = t.GetElementType();
                    object oo = Array.CreateInstance(tt, 0);
                    return (T)oo;
                }
                else
                    return (T)o;
            }
            else
                return (T)o;
        }

        public object ToObject(string json)
        {
            return ToObject(json, null);
        }

        public object ToObject(string json, Type type)
        {
            //_params = Parameters;
            _params.FixValues();
            Type t = null;
            if (type != null && type.IsGenericType)
                t = Reflection.Instance.GetGenericTypeDefinition(type);
            if (t == typeof(Dictionary<,>) || t == typeof(List<>))
                _params.UsingGlobalTypes = false;
            _usingglobals = _params.UsingGlobalTypes;

            object o = new JsonParser(json).Decode();
            if (o == null)
                return null;
#if !SILVERLIGHT
            if (type != null && type == typeof(DataSet))
                return CreateDataset(o as Dictionary<string, object>, null);
            else if (type != null && type == typeof(DataTable))
                return CreateDataTable(o as Dictionary<string, object>, null);
#endif
            if (o is IDictionary)
            {
                if (type != null && t == typeof(Dictionary<,>)) // deserialize a dictionary
                    return RootDictionary(o, type);
                else // deserialize an object
                {
                    return ParseDictionary(o as Dictionary<string, object>, null, type, null);
                }
            }
            else if (o is List<object>)
            {
                if (type != null && t == typeof(Dictionary<,>)) // kv format
                    return RootDictionary(o, type);
                else if (type != null && t == typeof(List<>)) // deserialize to generic list
                    return RootList(o, type);
                else if (type != null && type.IsArray)
                    return RootArray(o, type);
                else if (type == typeof(Hashtable))
                    return RootHashTable((List<object>)o);
                else if (type == null)
                {
                    List<object> l = (List<object>)o;
                    if (l.Count > 0 && l[0].GetType() == typeof(Dictionary<string, object>))
                    {
                        Dictionary<string, object> globals = new Dictionary<string, object>();
                        List<object> op = new List<object>();
                        // try to get $types 
                        foreach (var i in l)
                            op.Add(ParseDictionary((Dictionary<string, object>)i, globals, null, null));
                        return op;
                    }
                    return l.ToArray();
                }
            }
            else if (type != null && o.GetType() != type)
                return ChangeType(o, type);

            return o;
        }
        public static object CheckCallBack(object o)
        {
            if(o is ISerializationCallbackReceiver dd)
                dd.OnAfterDeserialize();
            return o;
        }

        #region [   p r i v a t e   m e t h o d s   ]
        private object RootHashTable(List<object> o)
        {
            Hashtable h = new Hashtable();

            foreach (Dictionary<string, object> values in o)
            {
                object key = values["k"];
                object val = values["v"];
                if (key is Dictionary<string, object>)
                    key = ParseDictionary((Dictionary<string, object>)key, null, typeof(object), null);

                if (val is Dictionary<string, object>)
                    val = ParseDictionary((Dictionary<string, object>)val, null, typeof(object), null);

                h.Add(key, val);
            }

            return h;
        }

        public object ChangeType(object value, Type conversionType)
        {
            CustomConverter cc;
            if (conversionType == typeof(int))
            {
                string s = value as string;
                if (s == null)
                    return (int)value;
                else
                    return CreateInteger(s, 0, s.Length);
            }
            else if (conversionType == typeof(long))
            {
                string s = value as string;
                if (s == null)
                    return (long)value;
                else
                    return CreateLong(s, 0, s.Length);
            }
            else if (conversionType == typeof(string))
                return (string)value;

            else if (conversionType.IsEnum)
                return CreateEnum(conversionType, value);

            else if (conversionType == typeof(DateTime))
                return CreateDateTime((string)value);

            else if (conversionType == typeof(DateTimeOffset))
                return CreateDateTimeOffset((string)value);

            else if (Reflection.Instance.customTypes.TryGetValue(conversionType, out cc))
                return cc.deserialize(value);

            // 8-30-2014 - James Brooks - Added code for nullable types.
            if (IsNullable(conversionType))
            {
                if (value == null)
                    return value;
                conversionType = UnderlyingTypeOf(conversionType);
            }

            // 8-30-2014 - James Brooks - Nullable Guid is a special case so it was moved after the "IsNullable" check.
            if (conversionType == typeof(Guid))
                return CreateGuid((string)value);

            // 2016-04-02 - Enrico Padovani - proper conversion of byte[] back from string
            if (conversionType == typeof(byte[]))
                return Convert.FromBase64String((string)value);

            if (conversionType == typeof(TimeSpan))
                return new TimeSpan((long)value);

            return Convert.ChangeType(value, conversionType, CultureInfo.InvariantCulture);
        }

        private object CreateDateTimeOffset(string value)
        {
            //                   0123456789012345678 9012 9/3 0/4  1/5
            // datetime format = yyyy-MM-ddTHH:mm:ss .nnn  _   +   00:00

            // ISO8601 roundtrip formats have 7 digits for ticks, and no space before the '+'
            // datetime format = yyyy-MM-ddTHH:mm:ss .nnnnnnn  +   00:00  
            // datetime format = yyyy-MM-ddTHH:mm:ss .nnnnnnn  Z  

            int year;
            int month;
            int day;
            int hour;
            int min;
            int sec;
            int ms = 0;
            int usTicks = 0; // ticks for xxx.x microseconds
            int th = 0;
            int tm = 0;

            year = CreateInteger(value, 0, 4);
            month = CreateInteger(value, 5, 2);
            day = CreateInteger(value, 8, 2);
            hour = CreateInteger(value, 11, 2);
            min = CreateInteger(value, 14, 2);
            sec = CreateInteger(value, 17, 2);

            int p = 20;

            if (value.Length > 21 && value[19] == '.')
            {
                ms = CreateInteger(value, p, 3);
                p = 23;

                // handle 7 digit case
                if (value.Length > 25 && char.IsDigit(value[p]))
                {
                    usTicks = CreateInteger(value, p, 4);
                    p = 27;
                }
            }

            if (value[p] == 'Z')
                // UTC
                return CreateDateTimeOffset(year, month, day, hour, min, sec, ms, usTicks, TimeSpan.Zero);

            if (value[p] == ' ')
                ++p;

            // +00:00
            th = CreateInteger(value, p + 1, 2);
            tm = CreateInteger(value, p + 1 + 2 + 1, 2);

            if (value[p] == '-')
                th = -th;

            return CreateDateTimeOffset(year, month, day, hour, min, sec, ms, usTicks, new TimeSpan(th, tm, 0));
        }

        private static DateTimeOffset CreateDateTimeOffset(
            int year, int month, int day, int hour, int min, int sec, int milli, int extraTicks, TimeSpan offset)
        {
            var dt = new DateTimeOffset(year, month, day, hour, min, sec, milli, offset);

            if (extraTicks > 0)
                dt += TimeSpan.FromTicks(extraTicks);

            return dt;
        }

        private bool IsNullable(Type t)
        {
            if (!t.IsGenericType) return false;
            Type g = t.GetGenericTypeDefinition();
            return (g.Equals(typeof(Nullable<>)));
        }

        private Type UnderlyingTypeOf(Type t)
        {
            return t.GetGenericArguments()[0];
        }

        private object RootList(object parse, Type type)
        {
            Type[] gtypes = Reflection.Instance.GetGenericArguments(type);
            IList o = (IList)Reflection.Instance.FastCreateInstance(type);
            DoParseList(parse, gtypes[0], o);
            return o;
        }

        private void DoParseList(object parse, Type it, IList o)
        {
            Dictionary<string, object> globals = new Dictionary<string, object>();
            foreach (var k in (IList)parse)
            {
                _usingglobals = false;
                object v = k;
                if (k is Dictionary<string, object>)
                    v = ParseDictionary(k as Dictionary<string, object>, globals, it, null);
                else
                    v = ChangeType(k, it);

                o.Add(v);
            }
        }

        private object RootArray(object parse, Type type)
        {
            Type it = type.GetElementType();
            IList o = (IList)Reflection.Instance.FastCreateInstance(typeof(List<>).MakeGenericType(it));
            DoParseList(parse, it, o);
            var array = Array.CreateInstance(it, o.Count);
            o.CopyTo(array, 0);

            return array;
        }

        private object RootDictionary(object parse, Type type)
        {
            Type[] gtypes = Reflection.Instance.GetGenericArguments(type);
            Type t1 = null;
            Type t2 = null;
            if (gtypes != null)
            {
                t1 = gtypes[0];
                t2 = gtypes[1];
            }
            var arraytype = t2.GetElementType();
            if (parse is Dictionary<string, object>)
            {
                IDictionary o = (IDictionary)Reflection.Instance.FastCreateInstance(type);

                foreach (var kv in (Dictionary<string, object>)parse)
                {
                    object v;
                    object k = ChangeType(kv.Key, t1);

                    if (kv.Value is Dictionary<string, object>)
                        v = ParseDictionary(kv.Value as Dictionary<string, object>, null, t2, null);

                    else if (t2.IsArray && t2 != typeof(byte[]))
                        v = CreateArray((List<object>)kv.Value, t2, arraytype, null);

                    else if (kv.Value is IList)
                        v = CreateGenericList((List<object>)kv.Value, t2, t1, null);

                    else
                        v = ChangeType(kv.Value, t2);

                    o.Add(k, v);
                }

                return o;
            }
            if (parse is List<object>)
                return CreateDictionary(parse as List<object>, type, gtypes, null);

            return null;
        }

        internal object ParseDictionary(Dictionary<string, object> d, Dictionary<string, object> globaltypes, Type type, object input)
        {
            object tn = "";
            if (type == typeof(NameValueCollection))
                return CreateNV(d);
            if (type == typeof(StringDictionary))
                return CreateSD(d);

            if (d.TryGetValue("$i", out tn))
            {
                object v = null;
                _cirrev.TryGetValue((int)tn, out v);
                return v;
            }

            if (d.TryGetValue("$types", out tn))
            {
                _usingglobals = true;
                if (globaltypes == null)
                    globaltypes = new Dictionary<string, object>();
                foreach (var kv in (Dictionary<string, object>)tn)
                {
                    globaltypes.Add((string)kv.Value, kv.Key);
                }
            }
            if (globaltypes != null)
                _usingglobals = true;

            bool found = d.TryGetValue("$type", out tn);

            if (found)
            {
                if (_usingglobals)
                {
                    object tname = "";
                    if (globaltypes != null && globaltypes.TryGetValue((string)tn, out tname))
                        tn = tname;
                }
                type = Reflection.Instance.GetTypeFromCache((string)tn);
            }

            if (type == null)
            {
                d.Remove("$type");
                Debug.LogError("Cannot determine type " + input + " " + tn);
                return d;
                throw new InvalidCastException("Cannot determine type " + input + " " + tn); //2do throw exception earler when parsing types
            }

            string typename = type.FullName;
            object o = input;
            if (o == null)
            {
                if (_params.ParametricConstructorOverride)
                    o = System.Runtime.Serialization.FormatterServices.GetUninitializedObject(type);
                else
                    o = Reflection.Instance.FastCreateInstance(type);
            }
            int circount = 0;
            if (_circobj.TryGetValue(o, out circount) == false)
            {
                circount = _circobj.Count + 1;
                _circobj.Add(o, circount);
                _cirrev.Add(circount, o);
            }

            Dictionary<string, myPropInfo> props = Reflection.Instance.Getproperties(type, typename);
            foreach (var kv in d)
            {
                var n = kv.Key;
                var v = kv.Value;

                string name = n;//.ToLower();
                if (name == "$map")
                {
                    ProcessMap(o, props, (Dictionary<string, object>)d[name]);
                    continue;
                }
                myPropInfo pi;
                // if (props.TryGetValue(name.ToLower(), out pi) == false)
                    if (props.TryGetValue(name, out pi) == false)
                        continue;

                if (pi.CanWrite)
                {
                    if (v != null)
                    {
                        try
                        {
                            object oset = null;

                            switch (pi.Type)
                            {
                                case myPropInfoType.Int:
                                    oset = (int) AutoConv(v);
                                    break;
                                case myPropInfoType.Long:
                                    oset = AutoConv(v);
                                    break;
                                case myPropInfoType.String:
                                    oset = (string) v;
                                    break;
                                case myPropInfoType.Bool:
                                    oset = (bool) v;
                                    break;
                                case myPropInfoType.DateTime:
                                    oset = CreateDateTime((string) v);
                                    break;
                                case myPropInfoType.Enum:
                                    oset = CreateEnum(pi.pt, v);
                                    break;
                                case myPropInfoType.Guid:
                                    oset = CreateGuid((string) v);
                                    break;

                                case myPropInfoType.Array:
                                    if (!pi.IsValueType)
                                        oset = CreateArray((List<object>) v, pi.pt, pi.bt, globaltypes);
                                    // what about 'else'?
                                    break;
                                case myPropInfoType.ByteArray:
                                    oset = Convert.FromBase64String((string) v);
                                    break;
#if !SILVERLIGHT
                            case myPropInfoType.DataSet: oset = CreateDataset((Dictionary<string, object>)v, globaltypes); break;
                            case myPropInfoType.DataTable: oset = CreateDataTable((Dictionary<string, object>)v, globaltypes); break;
#endif
                                case myPropInfoType.Hashtable: // same case as Dictionary
                                case myPropInfoType.Dictionary:
                                    oset = CreateDictionary((List<object>) v, pi.pt, pi.GenericTypes, globaltypes);
                                    break;
                                case myPropInfoType.StringKeyDictionary:
                                    oset = CreateStringKeyDictionary((Dictionary<string, object>) v, pi.pt, pi.GenericTypes, globaltypes, (IDictionary) pi.getter(o));
                                    break;
                                case myPropInfoType.NameValue:
                                    oset = CreateNV((Dictionary<string, object>) v);
                                    break;
                                case myPropInfoType.StringDictionary:
                                    oset = CreateSD((Dictionary<string, object>) v);
                                    break;
                                //case myPropInfoType.Custom: oset = Reflection.Instance.CreateCustom((string)v, pi.pt); break;
                                default:
                                {
                                    if (pi.IsGenericType && pi.IsValueType == false && v is List<object>)
                                        oset = CreateGenericList((List<object>) v, pi.pt, pi.bt, globaltypes);

                                    else if ((pi.IsClass || pi.IsStruct || pi.IsInterface) && v is Dictionary<string, object>)
                                        oset = ParseDictionary((Dictionary<string, object>) v, globaltypes, pi.pt, pi.getter(o));

                                    else if (v is List<object>)
                                        oset = CreateArray((List<object>) v, pi.pt, typeof(object), globaltypes);

                                    else if (pi.IsValueType)
                                        oset = ChangeType(v, pi.changeType);

                                    else
                                        oset = v;
                                }
                                    break;
                            }
                            CustomConverter cc;
                            if (Reflection.Instance.customTypes.TryGetValue(pi.pt, out cc))
                                oset = cc.deserialize(oset);

                            o = pi.setter(o, oset);
                        }
                        catch (Exception e)
                        {
                            // Debugger.Break();
                            
                            #if game
                            UnityEngine.Debug.LogError("Failed to parse " + pi.Name);
                            UnityEngine.Debug.LogException(e);
                            #else
                            //Trace.Fail(e.Message);
                            // Debug.Fail(e.Message);
                            Console.WriteLine(e.Message);
                            #endif
                            // throw;

                        }
                    }
                }
            }
            CheckCallBack(o);
            return o;
        }

        private long AutoConv(object value)
        {
            if (value is string)
            {
                string s = (string)value;
                return CreateLong(s, 0, s.Length);
            }
            else if (value is long)
                return (long)value;
            else
                return Convert.ToInt64(value);
        }

        private StringDictionary CreateSD(Dictionary<string, object> d)
        {
            StringDictionary nv = new StringDictionary();

            foreach (var o in d)
                nv.Add(o.Key, (string)o.Value);

            return nv;
        }

        private NameValueCollection CreateNV(Dictionary<string, object> d)
        {
            NameValueCollection nv = new NameValueCollection();

            foreach (var o in d)
                nv.Add(o.Key, (string)o.Value);

            return nv;
        }

        private void ProcessMap(object obj, Dictionary<string, myPropInfo> props, Dictionary<string, object> dic)
        {
            foreach (KeyValuePair<string, object> kv in dic)
            {
                myPropInfo p = props[kv.Key];
                object o = p.getter(obj);
                Type t = Reflection.GetType((string)kv.Value);
                if (t == typeof(Guid))
                    p.setter(obj, CreateGuid((string)o));
            }
        }

        internal static long CreateLong(string s, int index, int count)
        {
            long num = 0;
            bool neg = false;
            for (int x = 0; x < count; x++, index++)
            {
                char cc = s[index];

                if (cc == '-')
                    neg = true;
                else if (cc == '+')
                    neg = false;
                else
                {
                    num *= 10;
                    num += (int)(cc - '0');
                }
            }
            if (neg) num = -num;

            return num;
        }

        internal static int CreateInteger(string s, int index, int count)
        {
            int num = 0;
            bool neg = false;
            for (int x = 0; x < count; x++, index++)
            {
                char cc = s[index];

                if (cc == '-')
                    neg = true;
                else if (cc == '+')
                    neg = false;
                else
                {
                    num *= 10;
                    num += (int)(cc - '0');
                }
            }
            if (neg) num = -num;

            return num;
        }

        private object CreateEnum(Type pt, object v)
        {
            // FEATURE : optimize create enum
#if !SILVERLIGHT2
            if (v is int i)
                return Enum.ToObject(pt, i);
            
            var value = v.ToString();
            value = value == "Survival" ? "BattleRoyale" : value == "RunMode" ? "Racing" : value;
            return Enum.Parse(pt, value);
#else
            return Enum.Parse(pt, v, true);
#endif
        }

        private Guid CreateGuid(string s)
        {
            if (s.Length > 30)
                return new Guid(s);
            else
                return new Guid(Convert.FromBase64String(s));
        }

        private DateTime CreateDateTime(string value)
        {
            if (value.Length < 19)
                return DateTime.MinValue;

            bool utc = false;
            //                   0123456789012345678 9012 9/3
            // datetime format = yyyy-MM-ddTHH:mm:ss .nnn  Z
            int year;
            int month;
            int day;
            int hour;
            int min;
            int sec;
            int ms = 0;

            year = CreateInteger(value, 0, 4);
            month = CreateInteger(value, 5, 2);
            day = CreateInteger(value, 8, 2);
            hour = CreateInteger(value, 11, 2);
            min = CreateInteger(value, 14, 2);
            sec = CreateInteger(value, 17, 2);
            if (value.Length > 21 && value[19] == '.')
                ms = CreateInteger(value, 20, 3);

            if (value[value.Length - 1] == 'Z')
                utc = true;

            if (_params.UseUTCDateTime == false && utc == false)
                return new DateTime(year, month, day, hour, min, sec, ms);
            else
                return new DateTime(year, month, day, hour, min, sec, ms, DateTimeKind.Utc).ToLocalTime();
        }

        private object CreateArray(List<object> data, Type pt, Type bt, Dictionary<string, object> globalTypes)
        {
            if (bt == null)
                bt = typeof(object);

            Array col = Array.CreateInstance(bt, data.Count);
            var arraytype = bt.GetElementType();
            // create an array of objects
            for (int i = 0; i < data.Count; i++)
            {
                object ob = data[i];
                if (ob == null)
                {
                    continue;
                }
                if (ob is IDictionary)
                    col.SetValue(ParseDictionary((Dictionary<string, object>)ob, globalTypes, bt, null), i);
                else if (ob is ICollection)
                    col.SetValue(CreateArray((List<object>)ob, bt, arraytype, globalTypes), i);
                else
                    col.SetValue(ChangeType(ob, bt), i);
            }

            return col;
        }

        private object CreateGenericList(List<object> data, Type pt, Type bt, Dictionary<string, object> globalTypes)
        {
            if (pt != typeof(object))
            {
                IList col = (IList)Reflection.Instance.FastCreateInstance(pt);
                var it = pt.GetGenericArguments()[0];
                // create an array of objects
                foreach (object ob in data)
                {
                    if (ob is IDictionary)
                        col.Add(ParseDictionary((Dictionary<string, object>)ob, globalTypes, it, null));

                    else if (ob is List<object>)
                    {
                        if (bt.IsGenericType)
                            col.Add((List<object>)ob);//).ToArray());
                        else
                            col.Add(((List<object>)ob).ToArray());
                    }
                    else
                        col.Add(ChangeType(ob, it));
                }
                return col;
            }
            return data;
        }

        private object CreateStringKeyDictionary(Dictionary<string, object> reader, Type pt, Type[] types, Dictionary<string, object> globalTypes,IDictionary src)
        {
            var col = src ?? (IDictionary) Reflection.Instance.FastCreateInstance(pt);
            Type arraytype = null;
            Type t2 = null;
            if (types != null)
                t2 = types[1];

            Type generictype = null;
            var ga = t2.GetGenericArguments();
            if (ga.Length > 0)
                generictype = ga[0];
            arraytype = t2.GetElementType();

            foreach (KeyValuePair<string, object> values in reader)
            {
                var key = values.Key;
                object val = null;

                if (values.Value is Dictionary<string, object>)
                    val = ParseDictionary((Dictionary<string, object>)values.Value, globalTypes, t2, null);

                else if (types != null && t2.IsArray)
                {
                    if (values.Value is Array)
                        val = values.Value;
                    else
                        val = CreateArray((List<object>)values.Value, t2, arraytype, globalTypes);
                }
                else if (values.Value is IList)
                    val = CreateGenericList((List<object>)values.Value, t2, generictype, globalTypes);

                else
                    val = ChangeType(values.Value, t2);

                col[key] = val;
            }

            return col;
        }

        private object CreateDictionary(List<object> reader, Type pt, Type[] types, Dictionary<string, object> globalTypes)
        {
            if (types.Length == 0) types = new Type[2] { typeof(object), typeof(object) };
            IDictionary col = (IDictionary)Reflection.Instance.FastCreateInstance(pt);
            Type t1 = null;
            Type t2 = null;
            if (types != null)
            {
                t1 = types[0];
                t2 = types[1];
            }

            foreach (Dictionary<string, object> values in reader)
            {
                object key = values["k"];
                object val = values["v"];

                if (key is Dictionary<string, object>)
                    key = ParseDictionary((Dictionary<string, object>)key, globalTypes, t1, null);
                else
                    key = ChangeType(key, t1);

                if (typeof(IDictionary).IsAssignableFrom(t2))
                    val = RootDictionary(val, t2);
                else if (val is Dictionary<string, object>)
                    val = ParseDictionary((Dictionary<string, object>)val, globalTypes, t2, null);
                else
                    val = ChangeType(val, t2);

                col.Add(key, val);
            }

            return col;
        }


        #endregion
    }

}