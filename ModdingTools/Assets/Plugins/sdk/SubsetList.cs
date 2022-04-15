using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EnumsNET;
using UnityEngine;

public class SubsetList
{
    [FieldAtr(inherit = true, dontDraw = true)]
    public int selected = ~0;
    [NonSerialized]
    public string[] names = new string[] { "a", "b", "c", "d" };
}
[Serializable]
public class EnumSubsetList<T> : SubsetList<T> where T:struct,Enum, IConvertible //better than enum because no [flags] needed
{
    public EnumSubsetList(IEnumerable<T> t)
    {
        InitValues(Enum<T>.values);
        Clear();
        foreach(var a in t)
            Add(a);
    }
    
    public EnumSubsetList():base()
    {
        InitValues(Enum<T>.values);
    }
    
    public int[] indexes = new int[20]; //enum does not always corespond to its position 
    protected override void InitValues(T[] t)
    {
        
        base.InitValues(t);

        T[] convertibles = Enums.GetValues<T>().ToArray();
        for (var i = 0; i < convertibles.Length; i++)
            indexes[Enums.ToInt32(convertibles[i])] = i;
    }
    protected override int IndexOf(T t)
    {
        if (indexes.Length == 0) //weird constructor bug
        {
            indexes = new int[20];
            InitValues(Enum<T>.values);
        }
        return indexes[Enums.ToInt32(t)];
    }
    
    public override string ToString()
    {
        var sb = new StringBuilder();
        
        foreach(var a in allPossible)
            if (Contains(a))
                sb.Append(a).Append("|");
        return sb.ToString();
    }
}
[Serializable]
public class SubsetList<T> : SubsetList, IVarParseDraw, IEnumerable<T>, IVarParseSkipIdent //its a list with togable values
{
    [NonSerialized]
    public T[] allPossible;
    // public IEqualityComparer<T> comparer = EqualityComparer<T>.Default;

    public SubsetList()
    {

    }
    public SubsetList(T[] t) //all possible values
    {
        InitValues(t);
    }
    protected virtual void InitValues(T[] t)
    {
        allPossible = t;
        names = t.Select(a => a + "").ToArray();
    }

    public virtual bool Contains(T t)
    {
        var i = IndexOf(t);
        if (i != -1)
            return (selected & Math2.IntPow(2, i)) != 0;
        return false;
    }
    protected virtual int IndexOf(T t)
    {
        return Array.IndexOf(allPossible,t);
    }
    public void Add(T t)
    {
        int index = IndexOf(t);
        if (index != -1)
            selected |= Math2.IntPow(2, index);
        else
            Debug.LogError("couldnt find " + t);
    }

    public void Remove(T t)
    {
        int index = IndexOf(t);
        if (index != -1)
            selected &= ~Math2.IntPow(2, index);
        else
            Debug.LogError("couldnt find " + t);
    }

    public void Toggle(T t)
    {
        int index = IndexOf(t);
        if (index != -1)
            selected ^= Math2.IntPow(2, index);
        else
            Debug.LogError("couldnt find " + t);
    }

    public void Clear()
    {
        selected = 0;
    }
    public void SelectAll()
    {
        selected = ~0;
    }
    #if game
    public bool OnVarParseDraw(FieldCache fieldCache)
    {
        GuiClasses.SubSetListDraw(allPossible, this);
        return false;
    }
#endif
    public IEnumerator<T> GetEnumerator()
    {
        int i = 0;
        foreach (var a in allPossible)
        {
            if ((selected & Math2.IntPow(2, i)) != 0)
                yield return a;
            i++;
        }
    }
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}