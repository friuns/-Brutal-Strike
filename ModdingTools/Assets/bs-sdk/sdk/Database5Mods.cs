

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using EnumsNET;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;


[Serializable]
public class WalkSoundTags: SerializableDictionary< string,AudioClip2> 
{

}


[Serializable]
public class AnimationOffsetDict : SerializableDictionary<Anims, Vector3>
{

}

[Serializable]
public class AnimationDict2 : SerializableDictionary<string, AnimationClip>
{

}

[Serializable]
public class AnimationSoundDict : SerializableDictionary<Anims, AudioClip2>
{

}


[Serializable]
public class AnimationDict : SerializableDictionary<Anims, AnimationClip>
{

}
public enum AnimLayer
{
    none=-1,run,jump,upper,Override
}
public enum Anims
{
    shoot, shoot2, reload, endReload, draw, startShoot, endThrow, pressButton, fly,parachute,Executing,idle,Die,Damage,
    dodgeLeft,
    dodgeRight,
    dodgeBack,
    jump,block,
    jumpAttack,
    runAttack

}

[Serializable]
public class BodyToTransform : SerializableDictionary<HumanBodyBones , Transform> { }

[Serializable]
public class BodyDamage : SerializableDictionary<HumanBodyBones, float> { }


#if game
public static class ext3
{
    public static void InvokeSafe(this Hook a,params object[] o)
    {
        try
        {
            a.Invoke(o);
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            throw;
        }
    }

    public static T Validate<T>(this T target,PlayerBot pl) where T : Target
    {
        return target.IsValid(pl) ? target : null;
    }
    public static T WeightedRandom<T>(this List<T> GunBases,Func<T,float> f,MyRandom random=null) //bigger value bigger probability
    {
        if (random == null) random = bs.random;
        float randomNumber = random.Range(0, GunBases.Sum(a => f(a)));
        foreach (T g in GunBases)
        {
            float number = f(g);
            if (randomNumber < number)
                return g;
            randomNumber -= number;
        }
        return default(T);
    }
    
    public static void CrossFade2(this Animator animator, Anims anim, AnimLayer animLayer)
    {
        if (!animator.IsInTransition((int) animLayer)) return;
        animator.CrossFade(anim.GetName(), 0, (int) animLayer);
    }
    public static void CrossFadeState(this Animator animator,Anims anim,Anims def, AnimLayer animLayer, bool b = true)
    {
        if (animator.IsInTransition((int) animLayer)) return;
        
        if (b)
        {
            if (!animator.GetNextAnimatorStateInfo((int) animLayer).IsName(anim.GetName()))
                animator.CrossFade(anim.GetName(), .3f, (int) animLayer);
        }
        else if (!animator.IsInTransition((int) animLayer) && animator.GetCurrentAnimatorStateInfo((int) animLayer).IsName(anim.GetName()))
            animator.CrossFade(def.GetName(), .3f, (int) animLayer);
//            animator.SetTrigger(AnimParams.Interrupt.ToStringC());
    }
    
    public static bool IsDead(this PlayerBotBase a)
    {
        return a == null || !a.player || a.player.deadOrKnocked;
    }
    
    public static bool IsAlive2(this PlayerBotBase a)
    {
        return a != null && a.player && a.player.alive && !a.player.knocked;
    }
    
    public static bool IsAlive(this PlayerBotBase a)
    {
        return a!=null && a.player && a.player.alive;
    }
    
    public static bool IsAlive(this PlayerBot a)
    {
        return a!=null && a.player && a.player.alive;
    }
    
    public static bool IsAlive(this Player a)
    {
        return a && a.alive;
    }
    public static bool HasFlag2(this WeaponSetId variable, WeaponSetId  value)
    {
        return (variable & value) == value;
    }
    public static bool HasFlag2(this CollisionFlags variable, CollisionFlags value)
    {
        return (variable & value) == value;
    }


    
//    public static T Component<T>(this GameObject g) where T : Component
//    {
////        return g.GetComponent<T>() ?? g.AddComponent<T>(); // do not work with <guilayer>
//        
//        var addComponent = g.GetComponentNonAlloc<T>();
//        var addComponent2 = addComponent ? addComponent : g.AddComponent<T>();
////        if (ac != null)
////            ac(addComponent2);
//        return addComponent2;
//
//    }
//
//    public static T Component<T>(this Component a) where T : Component
//    {
//        return Component<T>(a.gameObject);
//    }

}
#endif



public class LockAttribute : Attribute
{}
public class CacheAttribute : Attribute
{
    public int minutes;
    
}

public class CacheOnce : Attribute
{
    public int frames=1;
}




[Serializable]
public class SubsetListWeaponType: EnumSubsetList<WeaponType>
{
   
    public SubsetListWeaponType(WeaponType[] t) : base(t)
    {
    }
    public SubsetListWeaponType():base()
    {
    }
}

[Serializable]
public class SubsetListAttachments : EnumSubsetList<AttachmentSlots>
{
    public SubsetListAttachments(IEnumerable<AttachmentSlots> t) : base(t)
    {
    }
    public SubsetListAttachments():base()
    {
    }
}



[Serializable]
public class SubsetListTeamEnum : EnumSubsetList<TeamEnum>
{
    public SubsetListTeamEnum(Func<IEnumerable<TeamEnum>> t) : base()
    {
        
    }
    public SubsetListTeamEnum(IEnumerable<TeamEnum> t) : base(t)
    {
    }
    
}

public enum AttachmentSlots { scope, silencerSlot, StockAttachment, grip,clip }

public enum AttachmentType { scope, silencer, flashHider, Compressor, StockAttachment, VerticalForegrip,HorizontalGrip,ExtendedClip } //flashHider vs Compressor
public class Database2
{

}

public struct AnimParams
{
    //cant use state because no enum support
  public static AnimParams  Horizontal;
  public static AnimParams  Vertical;
  public static AnimParams  grounded;
  public static AnimParams  TurnLeft;
  public static AnimParams  TurnRight;
  public static AnimParams  Interrupt;
  public static AnimParams  speed;
  public static AnimParams  running;
  public static AnimParams  crouch;
  public static AnimParams  prone;
  public static AnimParams  knocked;
  public static AnimParams  IsRU;
  public static AnimParams  state;
  public static AnimParams  attachmentUI;
  public static AnimParams  idle;
  public static AnimParams  deathAnim;
  public static AnimParams  motionTime;
  public static AnimParams  jump;
  public static AnimParams  Curve;
  public static AnimParams  lastKey;
  public static AnimParams  DamageCurve;
  public static Dictionary<string, AnimParams> dict = new Dictionary<string, AnimParams>(StringComparer.OrdinalIgnoreCase);
  public int value;
  // private string name;

  public static implicit operator int(AnimParams value)
  {
      return value.value;
  }

  static AnimParams()
  {
      foreach (FieldInfo f in typeof(AnimParams).GetFields(BindingFlags.Static | BindingFlags.Public))
      {
          if (f.FieldType == typeof(AnimParams))
          {
              var anim = new AnimParams() {value = Animator.StringToHash(f.Name)};
              f.SetValue(null, anim);
              dict.Add(f.Name, anim);
          }
      }
  }
}


[Serializable]
public class PlayerSkinDict : SerializableDictionary<int, PlayerSkin> { }

[Serializable]
public class PlayerClassDict : SerializableDictionary<int, PlayerClass> { }


[Serializable]
public class BoneOffsets : SerializableDictionary<HumanBodyBones, Vector3>
{

}

[Serializable]
public class GunDict : SerializableDictionary2<int, GunBase>
{

}

#if game
interface ISetLife
{
    void RPCDamageAddLife(float damage, int pv = -1, int weapon = -1, HumanBodyBones colliderId = 0, Vector3 hitPos = default);
}
#endif
//public static class PhotonGroup
//{
//    public static byte bomb = 1;
//    public static byte player = 2;
//}

public enum EventGroup
{
    Other, SiteOld,
    Maps,
    Fps,
    playedTime,
    LoadedIn,
    Site,
    GameType,
    LevelEditor,
    Debug,
    Shop,
    Ban,
    Level,Kick,Notification
}
