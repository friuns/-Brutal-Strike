using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class PhotonView : MonoBehaviour
{
    // public int viewIdField ;
    public List<Component> ObservedComponents;
}

public class Destructable : ItemBase
{
	public int lifeDef = 100;
    public override void Reset()
    {
        base.Reset();
        if (!GetComponent<PhotonView>())
            gameObject.AddComponent<PhotonView>();
        
    }
}