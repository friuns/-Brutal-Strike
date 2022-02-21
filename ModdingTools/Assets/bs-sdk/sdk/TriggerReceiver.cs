using UnityEngine;

public class TriggerReceiver:Trigger
{
    public bool debug;
    #if game
    public void OnTriggerEnter(Collider c)
    {
        if(debug && IsLogging(LogTypes.rayCast))
            Debug.Log("collider entered: " + c.name, gameObject);
        Trigger other = c.GetComponentNonAlloc<Trigger>();
        if ((object) other != null)
        {
            if (other.handler != this.handler)
            {
                if (!triggers.Contains(other))
                {
                    triggers.Add(other);
                    (handler as IOnTriggerEnter)?.OnTriggerEnterOrExit(this, other,true);
                }
                if (!other.triggers.Contains(this))
                {
                    other.triggers.Add(this);
                    (other.handler as IOnTriggerEnter)?.OnTriggerEnterOrExit(other, this, true);
                }

            }
        }
    }

    public override void OnDisable()
    {
        
        if (bs.exiting) return;
        foreach (Trigger other in triggers)
        {
            this.OnTriggerExit2(other);
            if (other is TriggerReceiver r)
                r.OnTriggerExit2(this);
        }
    }
    public void OnTriggerExit2(Trigger other)
    {
        if(triggers.Remove(other))
            (handler as IOnTriggerEnter)?.OnTriggerEnterOrExit(this,other,false);
        if(other.triggers.Remove(this))
            (other.handler as IOnTriggerEnter)?.OnTriggerEnterOrExit(other, this, false);
        
    }
    
    public void OnTriggerExit(Collider c)
    {
        var other = c.GetComponent<Trigger>();
        if ((object) other != null)
            OnTriggerExit2(other);
        
    }
    public override void Clear()
    {
    }
#endif
}