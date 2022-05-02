using System;
using EnumsNET;
using UnityEngine;

public struct Layer
{
    public static int ignoreRayCast = 2;
    public static int ui = LayerMask.NameToLayer("UI");
    public static int Invisible = LayerMask.NameToLayer("Invisible");
    public static int def = LayerMask.NameToLayer("Default");
    public static int transparentFX = LayerMask.NameToLayer("TransparentFX");
    public static int level = LayerMask.NameToLayer("level");
    public static int grenade = LayerMask.NameToLayer("grenade");
    public static int pickable = LayerMask.NameToLayer("Pickups");
    public static int dragger = LayerMask.NameToLayer("dragger");
    public static int trigger = LayerMask.NameToLayer("trigger");
    public static int playerTrigger = LayerMask.NameToLayer("playerTrigger");
    public static int water = LayerMask.NameToLayer("Water");
    public static int tree = LayerMask.NameToLayer("Tree");
    public static int glass = LayerMask.NameToLayer(Tag.Glass);
    public static int door = LayerMask.NameToLayer("door");
    public static int car = LayerMask.NameToLayer("Car");
    public static int player = LayerMask.NameToLayer("Player");
    public static int hands = LayerMask.NameToLayer("Hands");
    public static int ground = LayerMask.NameToLayer("Ground");
    public static int EditorLayer = LayerMask.NameToLayer("EditorLayer");
    public static int ragdoll = LayerMask.NameToLayer("Ragdoll");
    public static int Physics = LayerMask.NameToLayer("Physics");
    public static int botVisibility = LayerMask.NameToLayer("botVisibility");
    public static int groundSoundFx= LayerMask.NameToLayer("groundSoundFx");
    public static int enemy= LayerMask.NameToLayer("Enemy");
    public static int terrain = LayerMask.NameToLayer("Terrain");

    //public static int dragger = LayerMask.NameToLayer("dragger");
    //public static int particles = 2;
    //public static int dontSpawn = LayerMask.NameToLayer("DontSpawn");
    //public static int car = LayerMask.NameToLayer("car");
    public static Layer triggersMask = (1 << trigger) | (1 << pickable) | (1 << Physics);
    public static Layer physicsMask = (1 << Physics) | (1 << ragdoll)|(1 << grenade);
    public static Layer placerMask = 1<<terrain;
    public static Layer levelBoundsMask =  (1 << level)| (1 << door)| (1 << glass) | (1<<tree) | placerMask;
    public static Layer levelMask = (1 << def) | levelBoundsMask | (1 << tree) ;
    public static Layer levelMask2= (1 << def) | levelBoundsMask | (1 << tree) | 1<<pickable;
    public static Layer levelMaskFull = (1<<terrain) | (1 << def) | (1 << level)| (1 << water);
    public static Layer allmask = levelMask | (1 << player) | physicsMask | (1 << ragdoll)  |1<<pickable|(1<<trigger);// |1<<playerTrigger ; bullets using allmask
    public static Layer allmaskWithoutRagdoll = allmask & ~(1 << ragdoll | 1 <<player|1 <<playerTrigger );
    // public static int playerMask = /*(1 << ragdoll) | not a player part*/ (1 << player) | (1 << playerTrigger); does not work cc can be disabled bones can be disable 
    public static Layer botVisionMask = levelMask | (1 << botVisibility) | 1<<trigger;
    public static int editorMask = levelMask | (1 << EditorLayer);

    public bool Contains(int layer)
    {
        return ((1 << layer) & this) != 0; // Pow(2,layer) & this
    }
    public int value;
    public static implicit operator int(Layer o)
    {
        return o.value;
    }
    public static implicit operator Layer(int o)
    {
        return new Layer() { value = o };
    }

#if game
    public static void IgnoreAllExcept(int layer1, params int[] layer2)
    {
        
        for (int j = 0; j < 32; j++)
            UnityEngine.Physics.IgnoreLayerCollision(layer1, j, !layer2.Contains(j));
    }
    
    public static void CollideMask(int layer1, int layer2)
    {
        for (int j = 0; j < 32; j++)
            if (((Layer)layer2).Contains(j))
                UnityEngine.Physics.IgnoreLayerCollision(layer1, j, false);
    }
    public static void FixLayers()
    {
//        IgnoreAllExcept(door, player, playerTrigger);
        // IgnoreAllExcept(botVisibility);
        
        for (int i = 0; i < 32; i++) //disable all
            for (int j = 0; j < 32; j++)
                UnityEngine.Physics.IgnoreLayerCollision(i, j, true);
        CollideMask(player, levelMask | (1 << ground) | (1 << Physics) | (1 << player));
        CollideMask(playerTrigger, triggersMask);

        CollideMask(Physics, (1 << Physics)| (1 << player) | (1 << level) | (1 << playerTrigger) | (1 << def));
        CollideMask(ragdoll, (1 << level) | (1 << playerTrigger) | (1 << def));
        
        // IgnoreAllExcept(ragdoll, level,playerTrigger, def);
    }
#endif


}