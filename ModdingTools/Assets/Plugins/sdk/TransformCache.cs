//using System.Diagnostics;

using System;
using System.Collections.Generic;
#if game 
using cakeslice;
#endif
//using NetFabric.Hyperlinq;
using UnityEngine;
//using Debug=UnityEngine.Debug;

public interface IDontDisable
{
}

public class TransformCache : Base
{
    internal List<TransformCache> prefabInstances = new List<TransformCache>();
    public TransformCache prefab;
#if game 
    internal bool destroyedDext
    {
        get { return parent2.IsNull(); }
        set
        {
            if (value) parent2 = null;
        }
    }
    
    public Parent parent2;

    public List<Component> components = new List<Component>(20);
    public bool[] oldComponents;

    public TreePrivate treeActive = new TreePrivate();
    public TreePrivate treeVisible = new TreePrivate();
    public override void OnInspectorGUI()
    {
        treeActive.active = GUILayout.Toggle(treeActive.active, "Active");
        treeVisible.active = GUILayout.Toggle(treeVisible.active, "Visible");
        GUILayout.Toggle(treeActive.activeInHierarchy, "Active In Hierarchy");
        GUILayout.Toggle(treeVisible.activeInHierarchy, "Visible In Hierarchy");
        base.OnInspectorGUI();
    }
    public bool autoPopulate = true; //autopopulate disabled by player in inspector 
    public bool inited;
    public override void Awake()
    {
        inited = true;
        base.Awake();
        Populate();
        treeVisible.name = "TreeVisible " + name;
        treeVisible.changed = VisibleChanged;
        treeActive.name = "TreeActive " + name;
        treeActive.changed = ActiveChanged;
        treeActive.Add(treeVisible);
    }
    [ContextMenu("IncludeColliders")]
    public void IncludeColliders()
    {
        var list = TempList<Collider>.GetTempList();
        GetComponentsInChildren2(transform,list);
        foreach (var a in list)
            if (!components.Contains(a))
                components.Add(a);
            // else
            //     Debug.LogError("already added "+name, a);
    }
    
    public void Populate()
    {
        if (!autoPopulate)
            return;
        autoPopulate = false;
        Populate2();
    }
    [ContextMenu("populate")]
    public void Populate2()
    {
        var tempList = TempList<Component>.GetTempList();
        GetComponentsInChildren2(transform, tempList);

        foreach (var a in tempList)
            if (a is MonoBehaviour && !(a is IDontDisable) || a is TriggerBase || a is Animator || a is Rigidbody || a is LODGroup|| a is Renderer || a is CharacterController || a is Light || a is Animation|| a is ParticleSystem)
            {
                if (!components.Contains(a))
                    components.Add(a);
                else
                    Debug.LogError("already added", a);
            }
    }
    private void GetComponentsInChildren2<T>(Transform tr, List<T> saveTo) where T : Component
    {
        var tmp1 = tr.GetComponentsNonAlloc<T>();
        foreach (var a in tmp1)
            if (a != this && a is TransformCache tc)
            { 
                Add(tc);
                return;
            }

        foreach (var a in tmp1)
            if (a!=this && a is T component)
                saveTo.Add(component);

        for (int i = 0; i < tr.childCount; i++)
        {
            Transform child = tr.GetChild(i);
            GetComponentsInChildren2(child, saveTo);
        }
    }
    public List<TransformCache> tcs = new List<TransformCache>();
    public void Clear()
    {
        for (int i = tcs.Count - 1; i >= 0; i--)
            Remove(tcs[i]);
    }
    public void Remove(TransformCache tc)
    {
        tcs.Remove(tc);
        treeActive.Remove(tc.treeActive);
        treeVisible.Remove(tc.treeVisible);
    }
    public void Add(TransformCache tc)
    {
        if (tcs.Contains(tc)) return;
        tcs.Add(tc);
        treeActive.Add(tc.treeActive);
        treeVisible.Add(tc.treeVisible);
    }
    private void ActiveChanged(bool Active)
    {
        Init();


        for (int i = 0; i < components.Count; i++)
        {
            {
                Behaviour r = components[i] as Behaviour;
                if ((object) r != null && !(r is OutlineRenderer))
                    if (r is MonoBehaviour || r is Animator || r is Animation)
                    {
                        if (Active)
                            r.enabled = oldComponents[i];
                        else
                        {
                            oldComponents[i] = r.enabled;
                            r.enabled = false;
                        }
                    }
            }
            {
                var r = components[i] as Rigidbody;
                if ((object) r != null)
                {
                    if (Active)
                    {
                        r.detectCollisions = true;
                        r.SetKinematic(oldComponents[i]);
                        r.velocity = Vector3.zero;
                        r.angularVelocity = Vector3.zero;
                    }
                    else
                    {
                        oldComponents[i] = r.isKinematic;
                        r.detectCollisions = false;
                        r.SetKinematic(true);
                    }
                    
                }
            }

            {
                var r = components[i] as Collider;
                if ((object) r != null)
                    if (Active)
                        r.enabled = oldComponents[i];
                    else
                    {
                        oldComponents[i] = r.enabled;
                        r.enabled = false;
                    }
            }

            {
                var r = components[i] as CharacterController;
                if ((object) r != null)
                    if (Active)
                        r.enabled = oldComponents[i];
                    else
                    {
                        oldComponents[i] = r.enabled;
                        r.enabled = false;
                    }
            }
        }
    }
    private void VisibleChanged(bool Active)
    {
        Init();
        for (int i = 0; i < components.Count; i++)
        {
            
            {
                var r = components[i] as ParticleSystem;
                if ((object) r != null)
                {
                    
                    var emis = r.emission;
                    if (Active)
                    {
                        emis.enabled = oldComponents[i];
                        if(r.main.playOnAwake)
                            r.Play();
                    }
                    else
                    {
                        oldComponents[i] = emis.enabled;
                        emis.enabled = false;
                    }
                }
            }
            
            {
                var r = components[i] as Renderer;
                if ((object)r != null)
                    if (Active)
                        r.enabled = oldComponents[i];
                    else
                    {
                        oldComponents[i] = r.enabled;
                        r.enabled = false;
                    }
            }
            {
                var r = components[i] as LODGroup;
                if ((object)r != null)
                    if (Active)
                        r.enabled = oldComponents[i];
                    else
                    {
                        oldComponents[i] = r.enabled;
                        r.enabled = false;
                    }
            }

            {
                var r = components[i] as Light;
                if ((object)r != null)
                    if (Active)
                        r.enabled = oldComponents[i];
                    else
                    {
                        oldComponents[i] = r.enabled;
                        r.enabled = false;
                    }
            }
            
            {
                var r = components[i] as OutlineRenderer;
                if ((object)r != null)
                    if (Active)
                        r.enabled = oldComponents[i];
                    else
                    {
                        oldComponents[i] = r.enabled;
                        r.enabled = false;
                    }
            }
        }
    }


    void Init()
    {
        if (destroyedByPool)
            Debug.LogError("Modifying Active while in Destroyed by Pool " + gameObject.GetHashCode(), gameObject);
        if (oldComponents == null || oldComponents.Length == 0)
            oldComponents = new bool[components.Count];
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        if (exiting) return;
        Debug2.Log(LogTypes.pool, gameObject, "Destroying ", name);
        if (prefab)
            prefab.prefabInstances.Remove(this);

        //foreach (var child in tcs)
        //    child.Remove(this);
        Clear();
    }
    internal bool destroyedByPool;
    public MyDisp<TransformCache> DisableCheck()
    {
        destroyedByPool = false;
        return new MyDisp<TransformCache>(a => a.destroyedByPool = true, this);
    }
    internal float timeDestroyed = float.MinValue;
    
//    bool isChecked;
    [System.Diagnostics.Conditional(Tag.logging)]
    private void Check()
    {
        if (!inited) UnityEngine.Debug.LogError("not inited " + name, this);
//        if(!isChecked && transform.parent)
//        {
//            var tc = transform.parent.GetComponentInParent<TransformCache>();
//            if (tc && !tc.tcs.Contains(this)) UnityEngine.Debug.LogError("found parent tcs "+name, gameObject);
//        }
//        isChecked = true;
    }
    public bool visible { get { return treeVisible.activeInHierarchy; } set { Check(); treeVisible.active = value; } } //does not affect animator because renderers only
    public bool active { get { return treeActive.activeInHierarchy; } set { Check(); treeActive.active = value; } }
    public bool visibleInHierarchy { get { return treeVisible.activeInHierarchy; } }
#endif
}