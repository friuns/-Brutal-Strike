using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Microsoft.Win32;
#if !game
using RealtimeCSG;
using RealtimeCSG.Components;
using RealtimeCSG.Foundation;
#endif
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;
public partial class BuildToolsSDK : EditorWindow
{
    // [MenuItem("Assets/Create Asset")] //use ctrl + D
     public static void SaveAsset(){}
 

#if !game
   
    // [InitializeOnLoadMethod]
    // public static void PreBake()
    // {
    //     Lightmapping.bakeStarted+= delegate
    //     {
    //         CSGModelManager.BuildLightmapUvs(true);
    //         EditorSceneManager.MarkAllScenesDirty();    
    //     }; 
    // }
    [MenuItem("Brutal Strike/Bake Lightmap", false) ]
    public static void BakeLightmap()
    {
        var csg = CSGSettings.EnableRealtimeCSG;
        CSGSettings.SetRealtimeCSGEnabled(true);
        EditorApplication.delayCall += delegate
        {
            foreach (var a in FindObjectsOfType<Light>())
            {
                a.shadows = LightShadows.Soft;
                a.lightmapBakeType = a.type == LightType.Directional ? LightmapBakeType.Mixed : LightmapBakeType.Baked;
            }

            Lightmapping.realtimeGI = false;
            LightmapSettings.lightmapsMode = LightmapsMode.NonDirectional;
            LightmapEditorSettings.lightmapper = LightmapEditorSettings.Lightmapper.ProgressiveCPU;
            LightmapEditorSettings.bakeResolution = 6;
            LightmapEditorSettings.mixedBakeMode = MixedLightingMode.Subtractive;
            foreach (var m in FindObjectsOfType<CSGModel>())
                m.ReceiveGI = ReceiveGI.Lightmaps;
            foreach (var r in FindObjectsOfType<Renderer>())
            {
                r.receiveShadows = true;
                r.shadowCastingMode = ShadowCastingMode.On;

                GameObjectUtility.SetStaticEditorFlags(r.gameObject, StaticEditorFlags.ContributeGI |
                                                                     // StaticEditorFlags.BatchingStatic |
                                                                     StaticEditorFlags.NavigationStatic |
                                                                     StaticEditorFlags.OccludeeStatic |
                                                                     StaticEditorFlags.OffMeshLinkGeneration |
                                                                     StaticEditorFlags.ReflectionProbeStatic);
            }


            CSGModelManager.BuildLightmapUvs(true);
            EditorSceneManager.MarkAllScenesDirty();

            EditorApplication.delayCall += delegate
            {
                Lightmapping.BakeAsync(); 
                CSGSettings.SetRealtimeCSGEnabled(csg);
                
            };
        };
    }
#endif
    
    [MenuItem("Brutal Strike/Build Menu", false)]
    public static void BuildBundleMenu()
    {
        foreach (var a in buildPlatforms)
            BuildLevel(a, menusDir);
        OpenExplorer();
    }

    
    
    [MenuItem("Brutal Strike/Build Mod", false)]
    public static void BuildBundleAll()
    {
   
            foreach (var a in buildPlatforms)
                BuildLevel(a, bundlesDir);
        OpenExplorer();
    }
    
    [MenuItem("Brutal Strike/Build Mod PC only", false)]
    public static void BuildBundlePC()
    {
        
        BuildLevel(EditorUserBuildSettings.activeBuildTarget, bundlesDir);
        OpenExplorer();
    }

    [MenuItem("Brutal Strike/Build Map", false)]
    public static void BuildLevelAll()
    {
        foreach (var a in buildPlatforms)
            BuildLevel(a, mapsDir);
        OpenExplorer();
    }
    
    [MenuItem("Brutal Strike/Build Map PC Only", false)]
    public static void BuildLevelPC()
    {
      
        BuildLevel(EditorUserBuildSettings.activeBuildTarget, mapsDir);
        OpenExplorer();
    }
    

    

    public static string mapsDir = RootDir.GetPath("Maps");
    public static string bundlesDir = RootDir.GetPath("AssetBundles");
    public static string menusDir = RootDir.GetPath("Menus");
    public static string skinsDir = RootDir.GetPath("Skins");
    
    public static BuildTargetGroup ConvertBuildTarget(BuildTarget buildTarget)
    {
        switch (buildTarget)
        {
            case BuildTarget.iOS:
                return BuildTargetGroup.iOS;
            case BuildTarget.Android:
                return BuildTargetGroup.Android;
            case BuildTarget.WebGL:
                return BuildTargetGroup.WebGL;
            case BuildTarget.WSAPlayer:
                return BuildTargetGroup.WSA;
            case BuildTarget.PS4:
                return BuildTargetGroup.PS4;
            case BuildTarget.XboxOne:
                return BuildTargetGroup.XboxOne;
            case BuildTarget.tvOS:
                return BuildTargetGroup.tvOS;
            case BuildTarget.Switch:
                return BuildTargetGroup.Switch;
            default:
                return BuildTargetGroup.Standalone;
        }
    }
    [MenuItem("Tools/GenerateChecks", false)]
    public static void GenerateChecks()
    {
        foreach (var a in Directory.GetFiles(skinsDir))
        {
            if (Path.GetExtension(a).Contains("unity3d"))
            {
                var hash = MD5.Create().ComputeHash(File.ReadAllBytes(a));
                File.WriteAllBytes(a + ".check", hash);
                Debug.Log(a);
            }
        }

    }
    [MenuItem("Assets/Build Skin", false)]
    public static void BuildSkin()
    {
        var random = Random.Range(1, 9999999);
        foreach (var target in buildPlatforms)
        {
            var gs = Selection.objects;
            string[] assetPath = gs.Select(AssetDatabase.GetAssetPath).ToArray();
            var name = Path.GetFileName(Path.GetDirectoryName(assetPath[0]));
            var path = name + random + "." + GetExt(target);
            var path2 = name + "." + GetExt(target);
            var build = new AssetBundleBuild() {assetNames = assetPath, assetBundleName = path};
            
            AssetBundleManifest man = BuildPipeline.BuildAssetBundles(skinsDir, new[] {build}, BuildAssetBundleOptions.ForceRebuildAssetBundle, target);
            RenameFile(skinsDir + path, skinsDir + path2);
            var hash = MD5.Create().ComputeHash(File.ReadAllBytes(skinsDir + path2));
            File.WriteAllBytes(skinsDir + path2+".check",hash);
            Debug.Log("success");
        }
        
        CleanupDir(skinsDir);

    }
    private static void RenameFile(string a, string b)
    {
        FileDelete(b);
        File.Move(a, b);
    }
    private static BuildTarget[] buildPlatforms { get
    {
        // if (Environment.UserName == "friuns")
            // return new[] {EditorUserBuildSettings.activeBuildTarget};

        return new[]
        {
             BuildTarget.Android, 
            BuildTarget.StandaloneWindows,
             // BuildTarget.WebGL,
        };
    } }
    public static void CleanupDir(string dir)
    {
        foreach (var a in Directory.GetFiles(dir))
            if (File.Exists(a) && (!Path.HasExtension(a) || a.EndsWith(".manifest", StringComparison.OrdinalIgnoreCase)))
                File.Delete(a);
    }

    public static void BuildLevel(BuildTarget target, string dir)
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene d = SceneManager.GetSceneAt(i);
            if (d.isLoaded)
            {
                
                SupportedPlatforms ext = GetExt(target);
                var assetBundleName = d.name + "." + ext;
                var build = new AssetBundleBuild() {assetNames = new[] {d.path}, assetBundleName = assetBundleName};
                Directory.CreateDirectory(dir);
                FileDelete(dir+assetBundleName);
                FileDelete(dir + assetBundleName + ".manifest");
                
                
                PlayerSettings.WebGL.useEmbeddedResources = true; //enabling StringComparison.IgnoreCase

                AssetBundleManifest man = BuildPipeline.BuildAssetBundles(dir, new[] {build}, BuildAssetBundleOptions.ForceRebuildAssetBundle, target);
                //RenameFile(dir+assetBundleName, dir+ Path.GetFileName(Path.GetDirectoryName(d.path))+ "." + ext); rename to folder name

                if(man==null)
                    Debug.Log("build failed ");
                else
                {
                    Debug.Log("build success3 " + man + " ");
                    foreach (var a in man.GetAllAssetBundles())
                        Debug.Log(a);
                }
                
#if sdk || game
                try
                {
                    PlayerPrefs.SetString("assetPath", Path.GetFullPath(bundlesDir + "/" + build.assetBundleName));
                    var key = Registry.CurrentUser.OpenSubKey(@"Software\Unity\UnityEditor\Phaneron\", true);
                    RegistryUtilities.CopyKey(key, "Brutal Strike", Registry.CurrentUser.OpenSubKey(@"Software\Phaneron\", true));
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
#endif

            }
        }
        
        

    }
    private static SupportedPlatforms GetExt(BuildTarget target)
    {
        return target == BuildTarget.WebGL ? SupportedPlatforms.unity3dwebgl : target == BuildTarget.Android ? SupportedPlatforms.unity3dandroid : target == BuildTarget.iOS ? SupportedPlatforms.unity3dios : SupportedPlatforms.unity3dwindows;
    }
    private static void FileDelete(string assetBundleName)
    {
        if (File.Exists(assetBundleName)) File.Delete(assetBundleName);
    }
    private static void OpenExplorer()
    {
        
        var proc = new ProcessStartInfo();
        proc.FileName = Path.GetFullPath(bundlesDir);
        proc.Verb = "open";
        proc.WindowStyle = ProcessWindowStyle.Hidden;
        Process.Start(proc );
    }
}