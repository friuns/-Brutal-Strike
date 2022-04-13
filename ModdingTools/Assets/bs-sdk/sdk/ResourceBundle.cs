using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Win32;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
[DefaultExecutionOrder(ExecutionOrder.ResourceBundle)]
public class ResourceBundle : bs
{
    //public int id = 1;
    public Transform[] roots;
#if game
    public bool dontDeactive;
    public override void Awake()
    {
        base.Awake();
        print("ResourceBundle Awake " + gameObject.scene.name);
        _Game.resources.Add(this);
        if(!dontDeactive)
            gameObject.SetActive(false);


        foreach (Transform t2 in roots.Concat2(transform))
        {
            t2.GetComponentsInChildren<IOnLoadAssetChild>(true).ForEachTryCatch(a => a.OnLoadAssetChild());
            
            foreach (Transform t in t2) //do not need array because handles generated objects 
            {
                if (!dontDeactive)
                    t.gameObject.SetActive(true);

                t.GetComponentsNonAlloc<IPreLoadAsset>().ForEachTryCatch(a => a.OnPreLoadAsset());
                t.GetComponentsNonAlloc<IOnLoadAsset>().ForEachTryCatch(a => a.OnLoadAsset());

            }
        }
        
        foreach (var view in GetComponentsInChildren<PhotonView>(true))
        {
            view.viewID = 0;
            view.prefixBackup = -1;
            view.instantiationId = -1;
        }
    }
#else 
    // public override void Awake()
    // {
    //     base.Awake();
    //     print("ResourceBundle Awake " + gameObject.scene.name);
    //     gameObject.SetActive(false);
    //
    //
    //     foreach (Transform t2 in roots.Concat2(transform))
    //     {
    //         t2.GetComponentsInChildren<IOnLoadAssetChild>(true).ForEachTryCatch(a => a.OnLoadAssetChild());
    //         
    //         foreach (Transform t in t2) //do not need array because handles generated objects 
    //         {
    //             if (!dontDeactive)
    //                 t.gameObject.SetActive(true);
    //
    //             t.GetComponentsNonAlloc<IPreLoadAsset>().ForEachTryCatch(a => a.OnPreLoadAsset());
    //             t.GetComponentsNonAlloc<IOnLoadAsset>().ForEachTryCatch(a => a.OnLoadAsset());
    //
    //         }
    //     }
    //     
    //     foreach (var view in GetComponentsInChildren<PhotonView>(true))
    //     {
    //         view.viewID = 0;
    //         view.prefixBackup = -1;
    //         view.instantiationId = -1;
    //     }
    // }
    //
#endif
    void OnReset()
    {
        name = "Resources";
    }

}