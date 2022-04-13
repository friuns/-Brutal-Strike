using UnityEditor;
using UnityEngine;

public class Base : MonoBehaviour
{
    public virtual void Awake()
    {
        
    }
    #if UNITY_EDITOR
    public void OnSceneGui(SceneView scene)
    {
    }
    public void OnSceneUpdate(SceneView scene)
    {
    }
    #endif
}