



using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
#if game
using UnityStandardAssets.ImageEffects;
#endif
public class LightSun : ItemBase //,ItemBaseVarParseEnable
{  
    
#if game
    
    public override void OnGameEnabled()
    {
        base.OnGameEnabled();
        
        // if (enabled)
        {
        
            if (light == null) Debug.LogError("directional light not found");
            else
            {
                if (loaded2)
                {
                    light.transform.position = pos;
                    light.transform.rotation = rot;
                }
                else
                {
                    pos = light.transform.position;
                    rot = light.transform.rotation;
                }

                light.transform.SetParent(transform, true);
                
            }
            //    if (RenderSettings.skybox && RenderSettings.skybox.HasProperty("_Tex"))
            //    {
            //        RenderSettings.skybox .SetFloat("_Rotation",eulerAngles.y + 180);
            //        //SetRotation(RenderSettings.skybox, Quaternion.Euler(0, eulerAngles.y + 180, 0));
            //    }
            if (transform.forward.y > 0)
            {
                print("light is inverted, flipping");
                transform.forward = transform.forward.ZeroY(-1);
                
                
            }
        }
    }
    private new Light m_light2;
    public new Light light { get { return m_light2 ? m_light2 : (m_light2 = FindObjectsOfType<Light>().FirstOrDefault(a => a.type == LightType.Directional)); } }
    public float intensivity
    {
        get
        {
            return light ? light.intensity : 1;
        }
        set
        {
            if (light) light.intensity = value;
            RenderSettings.skybox?.SetFloat("_Exposure", value);
        }
    }
    public float reflectionsAmbient { get { return RenderSettings.reflectionIntensity; } set { RenderSettings.reflectionIntensity = value; } }
    public float fogDensity { get { return RenderSettings.fogDensity; } set { RenderSettings.fogDensity = value; } }
    
    
    public float ambient
    {
        get
        {
            if (RenderSettings.ambientMode == AmbientMode.Flat)
            {
                Color.RGBToHSV(RenderSettings.ambientLight, out _, out  _, out float v);
                return v;
            }
            return RenderSettings.ambientIntensity;
        }
        set
        {
            if (RenderSettings.ambientMode == AmbientMode.Flat)
            {
                Color.RGBToHSV(RenderSettings.ambientLight, out float h, out float s, out _);
                RenderSettings.ambientLight = Color.HSVToRGB(h, s, Mathf.Max(0.01f, value));
            }
            RenderSettings.ambientIntensity = value;
        }
    }
    private Material[] skyboxes = new Material[0];
    public override void OnLevelEditorGUI()
    {
        base.OnLevelEditorGUI();
        intensivity = FloatField("intensivity", intensivity);
        ambient = FloatField("ambient", ambient);
        
        reflectionsAmbient = FloatField("reflectionsAmbient", reflectionsAmbient);
        fogDensity = FloatField("fog", fogDensity);
        RenderSettings.fog = fogDensity > 0;
        
        Label("skyboxes");
        foreach (var a in skyboxes)
        {
            if (Button(a.name))
                RenderSettings.skybox = a;
        }
    }
    public override void Awake()
    {
        // enabled = false;
        RenderSettings.skybox = Instantiate(RenderSettings.skybox);
        base.Awake();
        skyboxes = Resources.FindObjectsOfTypeAll<Material>().Where(a => a.renderQueue <= 1000).ToArray();
    }
    private bool loaded2;
    public override void Load(BinaryReader br)  
    {
        base.Load(br);
        loaded2 = true;
        pos = br.ReadVector3();
        rot = br.ReadQuaternion();
        intensivity = br.ReadSingle();
        reflectionsAmbient = br.ReadSingle();
        ambient = br.ReadSingle();
        fogDensity = br.ReadSingle();
        //todo fog
    }

    public override void Save(BinaryWriter bw)
    {
        base.Save(bw);
        bw.Write(pos);
        bw.Write(rot);
        bw.Write(intensivity);
        bw.Write(reflectionsAmbient);
        bw.Write(ambient);
        bw.Write(fogDensity);
    }
    public override ItemBase Instantiate(Vector3 readVector, Quaternion lookRotation, bool local = false, bool pernament = true, bool scene = true,string name = null)
    {
        // enabled = true;
        transform.position = readVector;
        transform.rotation = lookRotation;
        
        return this;
    }
    //public static void SetRotation(Material material, Quaternion q)
    //{
    //    var m = Matrix4x4.TRS(Vector3.zero, q, Vector3.one);
    //    material.SetVector("_Rotation1", m.GetRow(0));
    //    material.SetVector("_Rotation2", m.GetRow(1));
    //    material.SetVector("_Rotation3", m.GetRow(2));
    //}
#endif
}
