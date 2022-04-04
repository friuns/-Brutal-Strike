using System;
using UnityEngine;
using UnityEngine.Playables;

[CreateAssetMenu(menuName = "Create SkyboxAnimation", fileName = "SkyboxAnimation", order = 0)]
// [TrackBindingType(typeof(Light))]
public class SkyboxAnimation : PlayableAsset
{
    public SkyboxAnimationHandler template = new SkyboxAnimationHandler();
    private static readonly int Tint = Shader.PropertyToID("_Tint");
    private static readonly int SkyTint = Shader.PropertyToID("_SkyTint");
    private static readonly int Exposure = Shader.PropertyToID("_Exposure");
    private static readonly int Intensivity = Shader.PropertyToID("_Intensivity");

    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        return ScriptPlayable<SkyboxAnimationHandler>.Create(graph);
    }
    
    [Serializable]
    public class SkyboxAnimationHandler : PlayableBehaviour //cant set properties 
    {
        public float ambientIntensity=1;
        public float reflectionIntensity=1;
        public float fogDensity=0;
        public Color skyboxColor = Color.white;
        public float intensivity=1;

        
        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            var d = RenderSettings.skybox;
            if (skyboxColor != Color.white)
            {
                d.color = skyboxColor;
                d.SetColor(SkyTint, skyboxColor);
                d.SetColor(Tint, skyboxColor);
            }
            if (intensivity != 1)
            {
                d.SetFloat(Intensivity, intensivity);
                d.SetFloat(Exposure, intensivity);
            }
            if (ambientIntensity != 1)
                RenderSettings.ambientIntensity = ambientIntensity;
            if (reflectionIntensity != 1)
                RenderSettings.reflectionIntensity = reflectionIntensity;
            if (fogDensity != 0)
                RenderSettings.fogDensity = fogDensity;
            RenderSettings.fog = fogDensity != 0;
        }
    }
    
}









// [Serializable]
// public class FloatDictionary : SerializableDictionary<string, float>
// {
//     
// }

//         public FloatDictionary floats = new FloatDictionary();

// var material = RenderSettings.skybox;
//             
// for (int i = 0; i < material.shader.GetPropertyCount(); i++)
// {
//     var name = material.shader.GetPropertyName(i);
//     if (material.shader.GetPropertyType(i) == ShaderPropertyType.Float)
//     {
//         if (!floats.TryGetValue(name, out float f))
//             floats[name] = material.GetFloat(name);
//         else
//             material.SetFloat(name,f);
//     }
// }
