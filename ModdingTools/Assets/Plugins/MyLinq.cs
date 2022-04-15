using System;
using gui = UnityEngine.GUILayout;
using System.Collections.Generic;
using UnityEngine;
//using ContinuousLinq;


//public static class CLinqExtensions
//{
//    public static ReadOnlyContinuousCollection<T2> OfType<T,T2>(this ObservableCollection<T> source) where T : INotifyPropertyChanged where T2 : INotifyPropertyChanged
//    {
//        return source.Where(a => a is T2).Select(a => (T2)(object)a);
//    }
//}
//namespace System.Linq
//{
    public static class MyLinq
    {
        
        public static double Average(this IList<double> source)
        {
            
            double sum = 0;
            var cnt = source.Count;
            if (cnt == 0) return 0;
            for (int i = 0; i < cnt; i++)
                sum += source[i];
            return sum / cnt;
        }
        
        public static float Min(this float[] source)
        {
            if (source.Length == 0) throw new Exception("empty");
            float num = float.MaxValue;
            var sourceLength = source.Length;
            for (var i = 0; i < sourceLength; i++)
            {
                var f = source[i];
                if (f < num)
                    num = f;
            }
            return num;
        }
        public static float Max(this float[] source)
        {
            if (source.Length == 0) throw new Exception("empty");
            
            float num = float.MinValue;
            var sourceLength = source.Length;
            for (var i = 0; i < sourceLength; i++)
            {
                var f = source[i];
                if (f > num)
                    num = f;
            }
            return num;
        }        

        public static float Min<TSource>(this IList<TSource> source, Func<TSource, float> selector)
        {
            float num = float.MaxValue;
            var sourceCount = source.Count;
            for (var i = 0; i < sourceCount; i++)
            {
                var f = selector(source[i]);
                if (num < f)
                    num = f;
            }
            return num;
        }
        
        public static int Max<TSource>(this IList<TSource> source, Func<TSource, int> selector)
        {
            int num = int.MinValue;
            var sourceCount = source.Count;
            for (var i = 0; i < sourceCount; i++)
            {
                var f = selector(source[i]);
                if (num>f)
                    num = f;
            }
            return num;
        }
        
        public static float Max<TSource>(this IList<TSource> source, Func<TSource, float> selector)
        {
            float num = float.MinValue;
            var sourceCount = source.Count;
            for (var i = 0; i < sourceCount; i++)
            {
                var f = selector(source[i]);
                if (num>f)
                    num = f;
            }
            return num;
        }        
        
        
//    public static bool TryGetValue<T>(this T[] a, int i,out T t)
//    {
//        t = a[i];
//        return true;
//    }
//    
        public static void Clear<T>(this T[] a)
        {
            Array.Clear(a,0,a.Length);
            // for (int i = 0; i < a.Length; i++)
            //     a[i] = default(T);
        }

        public static Vector3 Average<T>(this IList<T> array, Func<T, Vector3> selector)
        {
            Vector3 vector3 = Vector3.zero;
            for (int i = 0; i < array.Count; i++)
            {
                var v = selector(array[i]);
                vector3 += v;
            }
            return vector3 / array.Count;
        }
        

        
   
        //
        // public static List<TSource> WhereNonAllocFast2<TSource>(this List<TSource> sourceList, Func<TSource, bool> predicate)
        // {
        //     TSource[] source = ArrayProvider<TSource>.GetWrappedArray(sourceList);
        //     List<TSource> destList = TempList<TSource>.GetTempList();
        //     TSource[] dest = ArrayProvider<TSource>.GetWrappedArray(destList);
        //     int sourceCount = source.Length;
        //     destList.Capacity = sourceCount;
        //
        //     int i = 0;
        //     for (var index = 0; index < sourceCount; index++)
        //     {
        //         var t = source[index];
        //         if (predicate(t))
        //             dest[i++] = t;
        //     }
        //     return destList;
        //     
        // }
        
        // public static unsafe TSource[] WhereNonAllocFast<TSource>(this TSource[] source, Func<TSource, bool> predicate)
        // {
        //     var array = TempDynamicArray<TSource>.value; 
        //     var sourceCount = source.Length;
        //     int i = 0;
        //     for (var index = 0; index < sourceCount; index++)
        //     {
        //         var t = source[index];
        //         if (predicate(t))
        //             array[i++] = t;
        //     }
        //     TSource[] result = array;
        //     void* ptr = *(void**) Unsafe.AsPointer(ref result);
        //     *((UIntPtr*) ptr + 4 - 1) = (UIntPtr) i; //4 for x64 2 for x32  UnsafeUtility.AlignOf
        //     return array;
        //     
        // }
     
        
        public static IEnumerable<TSource> Where<TSource>(this IList<TSource> source, Func<TSource, bool> predicate)
        {
            var sourceCount = source.Count;
            for (int i = 0; i < sourceCount; i++)
            {
                if (predicate(source[i]))
                    yield return source[i];
            }
        }


        public static float Sum<T>(this IEnumerable<T> src, Func<T, float, float> dstuf)
        {
            float obj = 0;
            foreach (var a in src)
                obj = dstuf(a, obj);
            return obj;
        }




        public static int IndexOf<T>(this IEnumerable<T> items, T item, IEqualityComparer<T> comparer = null)
        {

            return items.FindIndex(i => (comparer ?? EqualityComparer<T>.Default).Equals(item, i));
        }
        public static int IndexOf<T>(this IEnumerable<T> items, Func<T, bool> predicate)
        {
            return FindIndex(items, predicate);
        }
        public static int FindIndex<T>(this IEnumerable<T> items, Func<T, bool> predicate)
        {
            if (items == null) throw new ArgumentNullException("items");
            if (predicate == null) throw new ArgumentNullException("predicate");
            int retVal = 0;
            foreach (var item in items)
            {
                if (predicate(item)) return retVal;
                retVal++;
            }
            return -1;
        }

        public static TSource Single<TSource>(this TSource[] array)
        {
            if (array.Length > 1) throw new InvalidOperationException("Sequence contains more than one element");
            return array[0];
        }
        public static TSource Single<TSource>(this IList<TSource> array)
        {
            if (array.Count > 1) throw new InvalidOperationException("Sequence contains more than one element");
            return array[0];
        }
        public static TSource First<TSource>(this TSource[] array)
        {
            return array[0];
        }
        public static TSource First<TSource>(this IList<TSource> array)
        {
            return array[0];
        }
        public static TSource First<TSource>(this TSource[] array, Func<TSource, bool> condition)
        {
            var arrayLength = array.Length;
            for (int i = 0; i < arrayLength; i++)
            {
                if (condition(array[i])) return array[i];
            }
            throw new InvalidOperationException("No items match the specified search criteria.");
        }

        public static TSource First<TSource>(this IList<TSource> array, Func<TSource, bool> condition)
        {
            var len = array.Count;
            for (int i = 0; i < len; i++)
            {
                if (condition(array[i])) return array[i];
            }
            throw new InvalidOperationException("No items match the specified search criteria.");
        }


        public static TSource SelectMax<TSource>(this IEnumerable<TSource> source, Func<TSource, float> selector)
        {
            float max = float.MinValue;
            TSource ts = default;
            foreach (var a in source)
            {
                var f = selector(a);
                if (f > max)
                {
                    ts = a;
                    max = f;
                }
            }
            return ts;
        }

        public static TSource SelectMin<TSource>(this IEnumerable<TSource> source, Func<TSource, float> selector,Func<TSource, bool> selector2)
        {
            float min = float.PositiveInfinity;
            TSource ts = (default(TSource));
            foreach (var a in source)
                if (selector2(a))
                {
                    var f = selector(a);
                    if (f < min)
                    {
                        ts = a;
                        min = f;
                    }
                }
            return ts;
        }
        
        public static TSource SelectMin<TSource>(this IEnumerable<TSource> source, Func<TSource, float> selector)
        {
            float min = float.PositiveInfinity;
            TSource ts = (default(TSource));
            foreach (var a in source)
            {
                var f = selector(a);
                if (f < min)
                {
                    ts = a;
                    min = f;
                }
            }
            return ts;
        }

        
        
        
        
        public static bool Any<TSource,TCast>(this IList<TSource> source, Func<TCast, bool> predicate)
        {
            var sourceCount = source.Count;
            for (var i = 0; i < sourceCount; i++)
            {
                TSource source1 = source[i];
                if (source1 is TCast b && predicate(b))
                    return true;
            }
            return false;
        }
        
        public static bool Any<TSource>(this IList<TSource> source, Func<TSource, bool> predicate)
        {
            var sourceCount = source.Count;
            for (var i = 0; i < sourceCount; i++)
            {
                TSource source1 = source[i];
                if (predicate(source1))
                    return true;
            }
            return false;
        }
        
        public static TSource FirstOrDefault<TSource>(this TSource[] array, Func<TSource, bool> condition)
        {
            var arrayLength = array.Length;
            for (int i = 0; i < arrayLength; i++)
            {
                if (condition(array[i])) return array[i];
            }
            return default(TSource);
        }

        public static TSource FirstOrDefault<TSource, T2>(this IList<TSource> array, Func<TSource, T2> condition, T2 t2)
        {
            var len = array.Count;
            for (int i = 0; i < len; i++)
            {
                if (EqualityComparer<T2>.Default.Equals(t2, condition(array[i]))) return array[i];
            }
            return default(TSource);
        }

        public static TSource2 FirstOrDefault<TSource, TSource2>(this IList<TSource> array, Func<TSource2, bool> condition) where TSource2 : class
        {
            var len = array.Count;
            for (int i = 0; i < len; i++)
            {
                if (array[i] is TSource2 b && condition(b)) return b;
            }
            return default(TSource2);
        }

        public static TSource? FirstOrNull<TSource>(this IList<TSource> array, Func<TSource, bool> condition) where TSource:struct
        {
            var len = array.Count;
            for (int i = 0; i < len; i++)
                if (condition(array[i])) return array[i];
            return null;
        }

        public static TSource FirstOrDefault<TSource>(this IList<TSource> array, Func<TSource, bool> condition, out int index)
        {
            var len = array.Count;
            index = -1;
            for (int i = 0; i < len; i++)
            {
                if (condition(array[i]))
                {
                    index = i;
                    return array[i];
                }
                
            }
            return default(TSource);
        }
        
        public static TSource FirstOrDefault<TSource>(this IList<TSource> array, Func<TSource, bool> condition)
        {
            var len = array.Count;
            for (int i = 0; i < len; i++)
            {
                if (condition(array[i])) return array[i];
            }
            return default(TSource);
        }

        public static bool Contains(this string str, char ch)
        {
            return str.IndexOf(ch) != -1;
        }

        public static TSource FirstOrDefault<TSource>(this TSource[] array)
        {
            return array.Length == 0 ? default(TSource) : array[0];
        }

        public static TSource FirstOrDefault<TSource>(this IList<TSource> array)
        {
            return array.Count == 0 ? default(TSource) : array[0];
        }
        public static TSource SingleOrDefault<TSource>(this TSource[] array)
        {
            if (array.Length > 1) throw new InvalidOperationException("Sequence contains more than one element");
            return array.Length == 0 ? default(TSource) : array[0];
        }

        public static TSource SingleOrDefault<TSource>(this IList<TSource> array)
        {
            if (array.Count > 1) throw new InvalidOperationException("Sequence contains more than one element");
            return array.Count == 0 ? default(TSource) : array[0];
        }
        public static TSource Last<TSource>(this IList<TSource> array)
        {
            return array[array.Count - 1];
        }
        public static TSource Last<TSource>(this TSource[] array)
        {
            return array[array.Length - 1];
        }
        public static TSource Last<TSource>(this TSource[] array, int i)
        {
            return array[array.Length - 1+i];
        }
        public static TSource LastOrDefault<TSource>(this IList<TSource> array)
        {
            return array.Count == 0 ? default(TSource) : array[array.Count - 1];
        }
        public static TSource LastOrDefault<TSource>(this TSource[] array)
        {
            return array.Length == 0 ? default(TSource) : array[array.Length - 1];
        }

        public static bool Any<TSource>(this TSource[] array, Func<TSource, bool> condition)
        {
            var arrayLength = array.Length;
            for (int i = 0; i < arrayLength; i++)
            {
                if (condition(array[i])) return true;
            }
            return false;
        }
        public static bool Any<TSource>(this IList<TSource> array)
        {
            return array.Count != 0;
        }
        public static bool Any<TSource>(this TSource[] array)
        {
            return array.Length != 0;
        }

        public static bool Any<TSource, TCast>(this List<TSource> array, Func<TCast, bool> condition)
        {
            var len = array.Count;
            for (int i = 0; i < len; i++)
            {
                if (array[i] is TCast tSource && condition(tSource)) 
                    return true;
            }
            return false;
        }


        public static int SumWhere<TSource>(this List<TSource> source, Func<TSource, int> selector, Func<TSource, bool> condition)
        {
            int num = 0;
            var sourceCount = source.Count;
            for (var i = 0; i < sourceCount; i++)
            {
                TSource source1 = source[i];
                checked
                {
                    if (condition(source1))
                        num += selector(source1);
                }
            }
            return num;
        }
        
        public static int Sum<TClosure ,TSource>(this List<TSource> source,TClosure closure, Func<TSource,TClosure , int> selector)
        {
            int num = 0;
            var sourceCount = source.Count;
            for (var i = 0; i < sourceCount; i++)
            {
                TSource source1 = source[i];
                checked
                {
                    num += selector(source1, closure);
                }
            }
            return num;
        }
        
        public static int Count<TSource>(
            this List<TSource> source,
            Func<TSource, bool> predicate)
        {
            int num = 0;
            var sourceCount = source.Count;
            for (var i = 0; i < sourceCount; i++)
            {
                TSource source1 = source[i];
                if (predicate(source1))
                    checked
                    {
                        ++num;
                    }
            }
            return num;
        }
        
        public static bool Any<TClosure,TSource>(this List<TSource> source,TClosure closure, Func<TSource,TClosure, bool> predicate)
        {
            var sourceCount = source.Count;
            for (var i = 0; i < sourceCount; i++)
            {
                TSource source1 = source[i];
                if (predicate(source1, closure))
                    return true;
            }
            return false;
        }

        
        public static int Count<TClosure,TSource>(this List<TSource> source,TClosure closure,Func<TSource,TClosure, bool> predicate)
        {
            int num = 0;
            var sourceCount = source.Count;
            for (var i = 0; i < sourceCount; i++)
            {
                TSource source1 = source[i];
                if (predicate(source1, closure))
                    checked
                    {
                        ++num;
                    }
            }
            return num;
        }
        

//        public static bool Any<TSource>(this List<TSource> array, Func<TSource, bool> condition)
//        {
//            var len = array.Count;
//            for (int i = 0; i < len; i++)
//            {
//                if (condition(array[i])) return true;
//            }
//            return false;
//        }
        public static bool All<TSource>(this TSource[] array, Func<TSource, bool> condition)
        {
            var arrayLength = array.Length;
            for (int i = 0; i < arrayLength; i++)
            {
                if (!condition(array[i])) return false;
            }
            return true;
        }

        public static bool All<TClosure, TSource>(this List<TSource> array, TClosure closure, Func<TSource,TClosure, bool> condition)
        {
            var len = array.Count;
            for (int i = 0; i < len; i++)
            {
                if (!condition(array[i],closure)) return false;
            }
            return true;
        }
        
        public static bool All<TSource>(this List<TSource> array, Func<TSource, bool> condition)
        {
            var len = array.Count;
            for (int i = 0; i < len; i++)
            {
                if (!condition(array[i])) return false;
            }
            return true;
        }
        
        public static bool All<TSource>(this IList<TSource> array, Func<TSource, bool> condition)
        {
            var len = array.Count;
            for (int i = 0; i < len; i++)
            {
                if (!condition(array[i])) return false;
            }
            return true;
        }

        public static bool All<TSource>(this Queue<TSource> array, Func<TSource, bool> condition)
        {
            foreach (var a in array)
                if (!condition(a)) return false;
            return true;
        }
        

        public static TSource[] ToArray<TSource>(this IList<TSource> array)
        {
            var len = array.Count;
            var dest = new TSource[len];
            for (int i = 0; i < dest.Length; i++)
            {
                dest[i] = array[i];
            }
            return dest;
        }
        public static TSource[] ToArray<TSource>(this TSource[] array)
        {
            var dest = new TSource[array.Length];
            for (int i = 0; i < dest.Length; i++)
            {
                dest[i] = array[i];
            }
            return dest;
        }
        public static List<TSource> ToList<TSource>(this IList<TSource> array)
        {
            var len = array.Count;
            var dest = new List<TSource>(len);
            for (int i = 0; i < len; i++)
            {
                dest.Add(array[i]);
            }
            return dest;
        }
        public static List<TSource> ToList<TSource>(this TSource[] array)
        {
            var dest = new List<TSource>(array.Length);
            var arrayLength = array.Length;
            for (int i = 0; i < arrayLength; i++)
            {
                dest.Add(array[i]);
            }
            return dest;
        }
    }



//public static int Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, int> selector)
//{
//    int sum = 0;
//    foreach (var a in source)
//        sum += selector(a);
//    return sum;
//}

//public static float Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, float> selector)
//{
//    float sum = 0;
//    foreach (var a in source)
//        sum += selector(a);
//    return sum;
//}
//public static TSource First<TSource>(this IList<TSource> source, Func<TSource, bool> predicate)
//{
//    for (int i = 0; i < source.Count; i++)
//    {
//        if (predicate(source[i]))
//            return source[i];
//    }
//    throw new Exception("Linq NoMatch");
//}
//public static TSource FirstOrDefault<TSource>(this IList<TSource> source)
//{
//    return source.Count > 0 ? source[0] : default(TSource);
//}

//public static TSource First<TSource>(this IList<TSource> source)
//{
//    return source[0];
//}


//public static TSource Last<TSource>(this IList<TSource> source)
//{
//    return source[source.Count - 1];
//}


//public static TSource FirstOrDefault<TSource>(this IList<TSource> source, Func<TSource, bool> predicate)
//{
//    for (int i = 0; i < source.Count; i++)
//    {
//        if (predicate(source[i]))
//            return source[i];
//    }
//    return default(TSource);
//}
//public static bool All<TSource>(this IList<TSource> source, Func<TSource, bool> predicate)
//{
//    for (int i = 0; i < source.Count; i++)
//        if (!predicate(source[i]))
//            return false;
//    return true;
//}
//public static bool Any<TSource>(this IList<TSource> source, Func<TSource, bool> predicate)
//{
//    for (int i = 0; i < source.Count; i++)
//        if (predicate(source[i]))
//            return true;
//    return false;
//}
//}