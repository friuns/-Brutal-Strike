using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using Random = System.Random;

public static class ext
{
    public static IEnumerable<TSource> Concat2<TSource>(this IEnumerable<TSource> first, TSource second)
    {
        foreach (var a in first)
            yield return a;
        yield return second;
    }
public static List<Transform> GetTransforms(this Transform t)
    {
        var list = new List<Transform>();
        for (int i = 0; i < t.childCount; i++)
            list.Add(t.GetChild(i));
        return list;
    }    
    public  static bool hasAlpha(Texture2D txt)
    {
        return GraphicsFormatUtility.HasAlphaChannel(txt.graphicsFormat);
        // return txt.format == TextureFormat.Alpha8 || txt.format == TextureFormat.ARGB4444 || txt.format == TextureFormat.RGBA32|| txt.format == TextureFormat.ARGB32 || txt.format == TextureFormat.DXT5 || txt.format == TextureFormat.PVRTC_RGBA2 || txt.format == TextureFormat.PVRTC_RGBA4 || txt.format == TextureFormat.ETC2_RGBA8 || txt.format == TextureFormat.ETC2_RGBA1 || txt.format == TextureFormat.ETC2_RGBA8;
    }
    public static Vector3 SetY(this Vector3 v, float a)
    {
        v.y = a;
        return v;
    }
    public static T Get<T>(this IList<T> a, int i)
    {
        return a.Count <= i || i < 0 ? default : a[i];
    }
public static T Component<T>(this Component a) where T : Component
    {
        return Component<T>(a.gameObject);
    }
    public static T Component<T>(this GameObject g) where T : Component
    {
//        return g.GetComponent<T>() ?? g.AddComponent<T>(); // do not work with <guilayer>
        
        var addComponent = g.GetComponent<T>();
        var addComponent2 = (Component)addComponent ? addComponent : g.AddComponent<T>();
//        if (ac != null)
//            ac(addComponent2);
        return addComponent2;

    }
    
    public static Texture2D CloneTexture(Texture2D txt)
    {
        
        var w = txt.width;
        var h = txt.height;
        var rt = RenderTexture.GetTemporary(w, h);
        rt.Create();
        Graphics.Blit(txt, rt);
        
        Texture2D nwT = new Texture2D(w, h, hasAlpha(txt) ? TextureFormat.ARGB32: TextureFormat.RGB24, false);
        nwT.name = "Clone of " + txt.name;
        nwT.ReadPixels(new Rect(0, 0, w, h), 0, 0, false);
        
        nwT.Apply(false);
        RenderTexture.active = null;
        RenderTexture.ReleaseTemporary(rt);
        Debug.Log("Cloning: " + txt.name);
        return nwT;
    }
    
    public static IList<T> Shuffle<T>(this IList<T> array, Random mr=null)
    {
        if (mr == null) mr = new Random();
        int n = array.Count;
        while (n > 1)
        {
            int k = mr.Next(0, n--);
            T temp = array[n];
            array[n] = array[k];
            array[k] = temp;
        }
        return array;
    }

    public static bool Contains(this string s, string s2, StringComparison so)
    {
        return s.IndexOf(s2, so) != -1;
    }
    public static T Component<T>(this GameObject g, Action<T> ac = null) where T : Component
    {
        var addComponent = g.GetComponent<T>();
        var addComponent2 = addComponent ? addComponent : g.AddComponent<T>();
        if (ac != null)
            ac(addComponent2);
        return addComponent2;

    }    public static T GetClampedSQ<T>(this IList<T> a, int i)
    {
        return a.Count == 0 ? default : a[i % (a.Count - 1)];
    }
    public static Material[] GetSharedMaterials(this Renderer o)
    {
        return o.sharedMaterials;
    }
    public static int lastRandom;
    public static T Random<T>(this IList<T> ts)
    {
        if (ts.Count == 0) return default(T);
        return ts[lastRandom = UnityEngine.Random.Range(0, ts.Count)];
    }
    public static T[] Fill<T>(this T[] ar, Func<int, T> ac)
    {
        for (int i = 0; i < ar.Length; i++)
            ar[i] = ac(i);
        return ar;
    }
   
}