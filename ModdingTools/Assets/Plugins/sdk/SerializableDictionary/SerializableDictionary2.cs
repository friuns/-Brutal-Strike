




using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.TextCore;

public interface ISerializableDictionary2
{
    
}
[Serializable]
public class SerializableDictionary2<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>,ISerializableDictionary2
{
    [NonSerialized]
    public bool throwOnKeyNotFoundException=true;
    public List<TKey> Keys = new List<TKey>();
    public List<TValue> Values = new List<TValue>();
    public Transform leftHand;
    public Transform rightHand;

    public int Count {get {return Keys.Count; } }

    public TValue this[TKey key]
    {
        get
        {
            var id = Keys.IndexOf(key);
            if (id == -1)
            {
                if (throwOnKeyNotFoundException)
                {
                    throw new KeyNotFoundException("key not found " + id);
                }
                else
                {
                    Keys.Add(key);
                    Values.Add(default(TValue));
                    id = Count - 1;
                }
            }
            return Values[id];
        }
        set
        {
            var v = Keys.IndexOf(key);
            if (v != -1)
                Values[v] = value;
            else
            {
                Keys.Add(key);
                Values.Add(value);
            }
        }
    }

    public void Add(TKey key, TValue value)
    {
        if (Keys.Contains(key)) throw new Exception("key already exists");
        Keys.Add(key);
        Values.Add(value);
    }
    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        for (int i = 0; i < Keys.Count; i++)
            yield return new KeyValuePair<TKey, TValue>(Keys[i], Values[i]);
    }
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
    public bool TryGetValue(TKey key, out TValue value)
    {

        var i = Keys.IndexOf(key);
        if (i == -1)
        {
            value = default(TValue);
            return false;
        }
        value = Values[i];
        return true;
    }
    public bool ContainsKey(TKey key)
    {
        return Keys.Contains(key);
    }
}