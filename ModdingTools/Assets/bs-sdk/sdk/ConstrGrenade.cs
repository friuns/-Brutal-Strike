using UnityEngine;

public class ConstrGrenade : GrenadeBullet
{
    
    private bool exploded ;
    #if game
    [PunRPC]
    public override void Explode()
    {
        if (exploded)
            return;
        exploded = true;
        if (wep.IsMine)
            StartCoroutine(AddMethod(delegate { explosion.GetComponent<Destructable>().Instantiate(pos + Vector3.down, Quaternion.identity, pernament: false); }, new WaitUntil(() => rigidbody.IsSleeping())));
        
    }
#endif
}