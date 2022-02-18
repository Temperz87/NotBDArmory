using Harmony;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using VTNetworking;
using VTOLVR.Multiplayer;
using Valve.Newtonsoft.Json;
using Valve.Newtonsoft.Json.Linq;

public class Armory : VTOLMOD
{
    private static bool patched = false;
    private static string StreamingAssetsPath;
    public static Loadout sloadout;
    public static Dictionary<string, CustomEqInfo> allCustomWeapons = null;
    public static string[] customweaponames = {
        "AIM-7x1",
        "AIM-7x3",
        "AIM-7x5",
        //"AIM-7x8", no
        //"ADMM", so much code
        "B61x1",
        "AIM-280x1",
        "AIM-280x3",
        "45 Rail Gun",
        "Howitzer",
        "TacticalLaserSystem",
        "Flight Assist Solid Rocket Booster",
        "Mistake Gun",
        "MK84x1",
        "MK85x1",
        "MOABx1",
        "ConformalGunTank"
        //"HYCMx1" this one is shelved until awacs missile gets implemented
        //"CJSM-69_Geiravorx1" this one is just tedious :P
    };

    public override void ModLoaded()
    {
        if (!patched)
        {
            VTNetworkManager.verboseLogs = true;
            StreamingAssetsPath = Directory.GetCurrentDirectory() + @"\VTOLVR_ModLoader\mods\";
            HarmonyInstance.Create("tempy.notbdarmory").PatchAll();
            Debug.Log("Try load NBDA prefabs...");
            allCustomWeapons = new Dictionary<string, CustomEqInfo>();
            StartCoroutine(LoadStockWeaponsAsync());
            StartCoroutine(LoadCustomCustomBundlesAsync()); 
            patched = true;
            VTOLAPI.SceneLoaded += SceneChanged; // So when the scene is changed SceneChanged is called
            SceneChanged(VTOLScenes.ReadyRoom);
        }
        base.ModLoaded();
    }

    private IEnumerator LoadCustomCustomBundlesAsync() // Special thanks to https://github.com/THE-GREAT-OVERLORD-OF-ALL-CHEESE/Custom-Scenario-Assets/ for this code
    {
        if (Directory.Exists(StreamingAssetsPath)) 
        {
            DirectoryInfo info = new DirectoryInfo(StreamingAssetsPath);
            foreach (DirectoryInfo directory in info.GetDirectories())
            {
                //Debug.Log("Searching " + directory + " for .nbda custom weapons");
                foreach (FileInfo file in info.GetFiles("*.nbda"))
                {
                    Debug.Log("Found nbda " + file.FullName);
                    StartCoroutine(LoadStreamedWeapons(file));
                }
            }
        }
        yield break;
    }

    private IEnumerator LoadStreamedWeapons(FileInfo info)
    {

        AssetBundleCreateRequest request = AssetBundle.LoadFromFileAsync(info.FullName);
        yield return request;

        if (request.assetBundle != null)
        {
            AssetBundleRequest requestjson = request.assetBundle.LoadAssetAsync("manifest.json");
            yield return requestjson;
            if (requestjson.asset == null)
            {
                Debug.LogError("Couldn't find manifest.json in " + info.FullName);
                yield break;
            }
            TextAsset manifest = requestjson.asset as TextAsset;
            JObject o = JObject.Parse(manifest.text);
            Dictionary<string, string> jsonLines = o.ToObject<Dictionary<string, string>>();
            foreach (string weaponName in jsonLines.Keys)
            {
                AssetBundleRequest requestGun = request.assetBundle.LoadAssetAsync(weaponName + ".prefab");
                yield return requestGun;
                if (requestGun.asset == null)
                {
                    Debug.LogError("Couldn't load asset " + weaponName);
                    continue;
                }
                GameObject gun = requestGun.asset as GameObject;
                LoadGeneric(gun, weaponName, jsonLines[weaponName]);
            }
        }
        else
        {
            Debug.Log("Couldn't load streamed bundle " + info.FullName);
        }
        yield break;
    }

    private void SceneChanged(VTOLScenes scenes)
    {
        if (scenes == VTOLScenes.ReadyRoom)
        {
            CustomWeaponHelper.allowWMDS = false;
            Inject_ArmorySettings.doneSettings = false;
        }
    }

    private IEnumerator LoadStockWeaponsAsync()
    {
        AssetBundleCreateRequest request = AssetBundle.LoadFromFileAsync(ModFolder + "/armory.assets");
        yield return request;
        AssetBundle bundle = request.assetBundle;
        if (bundle == null)
            throw new FileNotFoundException("Could not find/load armory.assets, path should be " + ModFolder + "/armory.assets.");
        foreach (string weaponName in customweaponames)
        {
            Debug.Log("Try load asset " + weaponName);
            AssetBundleRequest handler = bundle.LoadAssetAsync(weaponName + ".prefab");
            yield return handler;
            if (handler.asset == null)
            {
                Debug.Log("Couldn't find custom weapon " + weaponName);
                continue;
            }
            GameObject weaponObject = handler.asset as GameObject;
            if (weaponObject == null)
            {
                Debug.Log("weaponObject is null for asset " + weaponName + "?");
                continue;
            }
            switch (weaponName)
            {
                case "AIM-7x1":
                case "AIM-7x3":
                    LoadGeneric(weaponObject, weaponName, VehicleCompat.AV42C | VehicleCompat.AH94, true, false);
                    break;
                case "AIM-7x5":
                    LoadGeneric(weaponObject, "FA26B_" + weaponName, VehicleCompat.FA26B, false, false, "1,2,3,4,5,6,7,8,9,10");
                    GameObject f45Prefab = Instantiate(weaponObject);
                    LoadGeneric(f45Prefab, "F45_" + weaponName, VehicleCompat.F45A, false, false, "5,6,7,8,9,10");
                    break;
                case "AIM-7x8":
                    LoadGeneric(weaponObject, weaponName, VehicleCompat.F45A, false, false);
                    break;
                case "B61x1":
                    handler = bundle.LoadAssetAsync("B61 Bomb.prefab");
                    yield return handler;
                    if (handler.asset == null)
                    {
                        Debug.Log("Couldn't find custom weapon B61 Bomb");
                        break;
                    }
                    StartCoroutine(LoadB61(weaponObject, handler.asset as GameObject));
                    break;
                case "AIM-280x1":
                case "AIM-280x3":
                    LoadGeneric(weaponObject, weaponName, VehicleCompat.AH94, true, false).GetComponent<MissileLauncher>().missilePrefab.AddComponent<EMP>();
                    break;
                case "45 Rail Gun":
                    GameObject rg = LoadGeneric(weaponObject, weaponName, VehicleCompat.F45A, false, false);
                    rg.AddComponent<RailGun>();
                    VTNetEntity entity = rg.GetComponent<VTNetEntity>();
                    entity.netSyncs.Add(rg.AddComponent<AnimationToggleSync>());
                    break;
                case "ADMM":
                    break;
                case "Howitzer":
                    LoadGeneric(weaponObject, weaponName, VehicleCompat.AH94, false, false).AddComponent<Howitzer>();
                    break;
                case "TacticalLaserSystem":
                    StartCoroutine(LoadLaser(weaponObject));
                    break;
                case "Flight Assist Solid Rocket Booster":
                    StartCoroutine(LoadSRB(weaponObject));
                    break;
                case "Mistake Gun":
                    LoadGeneric(weaponObject, "Mistake Gun", VehicleCompat.AH94, false, true);
                    break;
                case "MK84x1":
                    LoadGeneric(weaponObject, weaponName, VehicleCompat.FA26B, false, false);
                    break;
                case "MK85x1":
                    LoadGeneric(weaponObject, weaponName, VehicleCompat.FA26B, false, false);
                    break;
                case "MOABx1":
                    LoadGeneric(weaponObject, weaponName, VehicleCompat.FA26B, false, true).GetComponent<MissileLauncher>().missilePrefab.AddComponent<AirburstMissile>();
                    break;
                case "CJSM-69_Geiravorx1":
                    LoadGeneric(weaponObject, weaponName, VehicleCompat.FA26B, false, false);
                    break;
                case "ConformalGunTank":
                    LoadCGT(weaponObject);
                    break;
                case "HYCMx1":
                    LoadGeneric(weaponObject, weaponName, VehicleCompat.F45A, false, false);
                    break;
                default:
                    Debug.LogError(name + " has not been implemented yet but is inside of custom weapon names."); // this should never occur
                    break;
            }
            weaponObject.SetActive(false);
        }
        Debug.Log("Done loading prefabs.");
    }

    private void LoadCGT(GameObject weaponObject)
    {
        GameObject cgt = LoadGeneric(weaponObject, "ConformalGunTank", VehicleCompat.F45A, false, false);
        cgt.AddComponent<AnimateOnEquip>();
        VTNetEntity entity = cgt.GetComponent<VTNetEntity>();
        entity.netSyncs.Add(cgt.AddComponent<AnimationToggleSync>());
        cgt.AddComponent<HeatGlow>();
        Debug.Log("Try yoink cgt material.");
        GameObject sevtf = VTResources.GetPlayerVehicle("F-45A").vehiclePrefab;
        MeshRenderer toYoink = sevtf.transform.Find("sevtf_layer_2").Find("VertStabLeftPart").Find("vertStabLeft").Find("vertStabLeft_mdl").GetComponent<MeshRenderer>();
        Material ext = toYoink.sharedMaterial;

        cgt.transform.Find("ConformalGunTank").GetComponent<MeshRenderer>().material = ext;
        cgt.transform.Find("ConformalGunTank").Find("LeftGunDoorGroup").Find("LeftGunDoor").GetComponent<MeshRenderer>().material = ext;
        cgt.transform.Find("ConformalGunTank").Find("RightGunDoorGroup").Find("RightGunDoor").GetComponent<MeshRenderer>().material = ext;
        Debug.Log("Set Material CGT");
    }

    private IEnumerator LoadSRB(GameObject weaponObject)
    {
        GameObject SRB94 = weaponObject;
        SRB94.AddComponent<HPEquipSRB>();
        SRB94.GetComponent<VTNetEntity>().netSyncs.Add(SRB94.AddComponent<SRBSync>());
        SRB94.name = "AH-94 Flight Assist Solid Rocket Booster";
        SRB94.GetComponentInChildren<AudioSource>().outputAudioMixerGroup = VTResources.GetExteriorMixerGroup();
        SRB94.SetActive(false); // just in case, so we don't destroy our net entity on it
        GameObject SRB26 = Instantiate(SRB94);
        SRB26.name = "FA-26B Flight Assist Solid Rocket Booster";
        GameObject SRB45 = Instantiate(SRB94);
        SRB45.name = "F-45A Flight Assist Solid Rocket Booster";
        GameObject SRB42 = Instantiate(SRB94);
        SRB42.name = "AV-42C Flight Assist Solid Rocket Booster";
        DontDestroyOnLoad(SRB94);
        DontDestroyOnLoad(SRB26);
        DontDestroyOnLoad(SRB45);
        DontDestroyOnLoad(SRB42);


        allCustomWeapons.Add("AH-94 Flight Assist Solid Rocket Booster", new CustomEqInfo(SRB94, VehicleCompat.AH94, false, false, "1,2,3,4"));
        allCustomWeapons.Add("FA-26B Flight Assist Solid Rocket Booster", new CustomEqInfo(SRB26, VehicleCompat.FA26B, false, false, "11,12,13"));
        allCustomWeapons.Add("F-45A Flight Assist Solid Rocket Booster", new CustomEqInfo(SRB45, VehicleCompat.F45A, false, false, "5,6,9,10"));
        allCustomWeapons.Add("AV-42C Flight Assist Solid Rocket Booster", new CustomEqInfo(SRB42, VehicleCompat.AV42C, false, false, "1,2,3,4"));
        SRB94.SetActive(false);
        SRB26.SetActive(false);
        SRB45.SetActive(false);
        SRB42.SetActive(false);
        Debug.Log("Loaded Flight Assist Solid Rocket Booster");
        yield break;
    }

    private IEnumerator LoadLaser(GameObject weaponObject)
    {
        GameObject TLS = weaponObject;
        TLS.AddComponent<HPEquipLaser>();
        //TLS.AddComponent<HeatGlow>();
        VTNetEntity entity = TLS.GetComponent<VTNetEntity>();
        entity.netSyncs = new List<VTNetSync>
        {
            TLS.AddComponent<LaserSync>()
        };
        //entity.netSyncs.Add(TLS.AddComponent<AnimationToggleSync>());
        foreach (AudioSource source in TLS.GetComponentsInChildren<AudioSource>(true))
        {
            source.outputAudioMixerGroup = VTResources.GetExteriorMixerGroup();
        }
        DontDestroyOnLoad(TLS);
        allCustomWeapons.Add("TacticalLaserSystem", new CustomEqInfo(TLS, VehicleCompat.FA26B, false, false));
        TLS.SetActive(false);

        Debug.Log("Loaded Tactical Laser System");
        yield break;
    }

    private GameObject LoadGeneric(GameObject weaponObject, string name, string compatability)
    {
        int mask = 0;
        if (compatability.Contains("42"))
            mask |= (int)VehicleCompat.AV42C;
        if (compatability.Contains("26"))
            mask |= (int)VehicleCompat.FA26B;
        if (compatability.Contains("45"))
            mask |= (int)VehicleCompat.F45A;
        if (compatability.Contains("94"))
            mask |= (int)VehicleCompat.AH94;

        VehicleCompat compat = (VehicleCompat)mask;
        return LoadGeneric(weaponObject, name, compat, false, false);
    }

    private GameObject LoadGeneric(GameObject weaponObject, string name, VehicleCompat vehicle, bool isExclude, bool isWMD, string equipPoints = null)
    {
        GameObject weaponToInject = weaponObject;
        weaponToInject.name = name;
        DontDestroyOnLoad(weaponToInject);
        foreach (AudioSource source in weaponToInject.GetComponentsInChildren<AudioSource>(true))
        {
            source.outputAudioMixerGroup = VTResources.GetExteriorMixerGroup();
        }
        HPEquipMissileLauncher launcher = weaponObject.GetComponent<HPEquipMissileLauncher>();
        if (launcher) // I know this solution isn't optimized in the slightest, but meh
        {
            GameObject dummyEquipper = Resources.Load("hpequips/afighter/fa26_iris-t-x1") as GameObject; // can't async this as we still need the return so rip
            HPEquipIRML irml = dummyEquipper.GetComponent<HPEquipIRML>();
            launcher.ml.launchAudioClips[0] = Instantiate(irml.ml.launchAudioClips[0]);
            launcher.ml.launchAudioSources[0].outputAudioMixerGroup = irml.ml.launchAudioSources[0].outputAudioMixerGroup;

            Missile missile = launcher.ml.missilePrefab.GetComponent<Missile>();
            if (missile.guidanceMode == Missile.GuidanceModes.Heat || missile.guidanceMode == Missile.GuidanceModes.Radar)
            {
                Missile dummyMissile = irml.ml.missilePrefab.GetComponent<Missile>();
                AudioSource source = missile.GetComponent<AudioSource>();
                AudioSource dummySource = dummyMissile.GetComponent<AudioSource>();
                source.outputAudioMixerGroup = dummySource.outputAudioMixerGroup;
                source.clip = Instantiate(dummySource.clip);

                if (missile.heatSeeker != null)
                {
                    Transform parent = missile.gameObject.transform.Find("SeekerParent");
                    AudioSource copiedSeeker = parent.Find("SeekerAudio").GetComponent<AudioSource>();
                    AudioSource originalSeeker = dummyMissile.transform.Find("SeekerParent").Find("SeekerAudio").GetComponent<AudioSource>();
                    copiedSeeker.clip = Instantiate(originalSeeker.clip);
                    copiedSeeker.outputAudioMixerGroup = Instantiate(originalSeeker.outputAudioMixerGroup);

                    AudioSource copiedLock = parent.Find("LockToneAudio").GetComponent<AudioSource>();
                    AudioSource originalLock = dummyMissile.transform.Find("SeekerParent").Find("LockToneAudio").GetComponent<AudioSource>();
                    copiedLock.clip = Instantiate(originalLock.clip);
                    copiedLock.outputAudioMixerGroup = originalLock.outputAudioMixerGroup;
                }

            }

        }
        allCustomWeapons.Add(name, new CustomEqInfo(weaponToInject, vehicle, isExclude, isWMD, equipPoints));
        weaponToInject.SetActive(false);
        Debug.Log("Loaded " + name);
        return weaponToInject;
    }

    private IEnumerator LoadADMM(GameObject weaponObject)
    {
        GameObject launcher = weaponObject;
        launcher.AddComponent<HPEquipAllDirectionalMissileLauncher>();
        allCustomWeapons.Add("ADMM", new CustomEqInfo(launcher, VehicleCompat.FA26B, false, true));
        DontDestroyOnLoad(launcher);
        launcher.SetActive(false);
        Debug.Log("Loaded ADMM");
        yield break;
    }

    private IEnumerator LoadB61(GameObject equipObject, GameObject nukeObject)
    {
        GameObject b61 = nukeObject;
        b61.AddComponent<Nuke>();
        GameObject newEquip = equipObject;
        MissileLauncher launcher = newEquip.GetComponentInChildren<MissileLauncher>();
        launcher.RemoveAllMissiles();
        launcher.missilePrefab = b61;
        launcher.loadOnStart = true;
        launcher.RemoveAllMissiles();
        DontDestroyOnLoad(newEquip);
        DontDestroyOnLoad(b61);
        allCustomWeapons.Add("B61x1", new CustomEqInfo(newEquip, VehicleCompat.AH94, true, true));
        b61.SetActive(false);
        newEquip.SetActive(false);
        Debug.Log("Loaded b61");
        yield break;
    }

    public static bool CheckCustomWeapon(string name)
    {
        if (String.IsNullOrEmpty(name))
            return false;
        foreach (string customName in allCustomWeapons.Keys)
            if (name.ToLower().Contains(customName.ToLower()))
                return true;
        return false;
    }
    public static CustomEqInfo TryGetWeapon(string name)
    {
        foreach (string customName in allCustomWeapons.Keys)
            if (name.ToLower().Contains(customName.ToLower()))
                return allCustomWeapons[customName];
        return null;
    }
}