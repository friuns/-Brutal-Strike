using UnityEngine;

public class SimpleRagdoll:MonoBehaviour
{
    internal Vector3 velocity;
    public Vector3 offset;
    private Vector3 oldPos;
    public bool applyGravity;
    public Vector3 constantMove;
    #if game
    public void Update()
    {
        if (applyGravity)
            velocity += Physics.gravity * TimeCached.deltaTime;
        
        transform.position += (velocity + constantMove) * TimeCached.deltaTime;RaycastHit h;
        if (oldPos != Vector3.zero && Physics.Linecast(oldPos + offset, transform.position + offset, out  h, Layer.levelMaskFull))
        {
            transform.position = h.point + offset;
            transform.up = h.normal;
            enabled = false;
        }
        oldPos = transform.position;
        
    }
#endif
}