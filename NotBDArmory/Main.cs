using Harmony;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using VTOLVR.Multiplayer;

public class Armory : VTOLMOD
{
    private static bool patched = false;
    public static Loadout sloadout;
    public static Dictionary<string, CustomEqInfo> allCustomWeapons = null;
    public static string[] customweaponames = {
        "AIM-7x1",
        "ADMM",
        "B61x1",
        "AIM-280x1",
        "45 Rail Gun",
        "Howitzer",
        "TacticalLaserSystem",
        "Flight Assist Solid Rocket Booster",
        "Mistake Gun",
        "MK84",
        "MK85",
        "MOABx1"
    };

    public override void ModLoaded()
    {
        if (!patched)
        {
            Debug.Log("Firstime start called.");
            HarmonyInstance.Create("Tempy.notbdarmory").PatchAll();
            Debug.Log("Try loading prefabs...");
            AssetBundle assBundel = AssetBundle.LoadFromFile(ModFolder + "/armory.assets");

            allCustomWeapons = new Dictionary<string, CustomEqInfo>();

            if (assBundel == null)
                throw new FileNotFoundException("Could not find/load armory.assets, path should be " + ModFolder + "/armory.assets.");
            StartCoroutine(LoadWeaponsAsync(assBundel));
            patched = true;
            if (BOOM.instance == null)
            {
                GameObject BOOMObject = Instantiate(new GameObject()); // stupid solution time
                BOOMObject.AddComponent<BOOM>();
                DontDestroyOnLoad(BOOMObject);
            }
            VTOLAPI.SceneLoaded += SceneChanged; // So when the scene is changed SceneChanged is called
            SceneChanged(VTOLScenes.ReadyRoom);
        }
        base.ModLoaded();
    }

    private void SceneChanged(VTOLScenes scenes)
    {
        if (scenes == VTOLScenes.ReadyRoom)
        {
            CustomWeaponHelper.allowWMDS = false;
            Inject_ArmorySettings.doneSettings = false;
        }
    }

    private IEnumerator LoadWeaponsAsync(AssetBundle bundle)
    {
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
            //GameObject weaponObject = Instantiate(handler.asset as GameObject);
            GameObject weaponObject = handler.asset as GameObject;
            if (weaponObject == null)
            {
                Debug.Log("weaponObject is null for asset " + weaponName + "?");
                continue;
            }
            switch (weaponName)
            {
                case "AIM-7x1":
                    //yield return StartCoroutine(LoadAim7(weaponObject));
                    LoadGeneric(weaponObject, weaponName, VehicleCompat.AV42C | VehicleCompat.AH94, true, false);
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
                    //StartCoroutine(LoadAim280(weaponObject));
                    LoadGeneric(weaponObject, weaponName, VehicleCompat.AH94, true, false).GetComponent<MissileLauncher>().missilePrefab.AddComponent<EMP>();
                    break;
                case "45 Rail Gun":
                    LoadGeneric(weaponObject, weaponName, VehicleCompat.F45A, false, false).AddComponent<RailGun>();
                    break;
                case "ADMM":
                    StartCoroutine(LoadADMM(weaponObject));
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
                case "MK84":
                    StartCoroutine(LoadBombGeneric(weaponObject, "MK84", VehicleCompat.FA26B, false, false, "MK84", "MK84", "Standard un-guided 2000lb bomb.", "1,4,5,6,7,10,11,12,13", 100f));
                    break;
                case "MK85":
                    StartCoroutine(LoadBombGeneric(weaponObject, "MK85", VehicleCompat.FA26B, false, false, "MK85", "MK85", "Standard un-guided 4000lb bomb.", "4,7,11,12,13", 200f));
                    break;
                case "MOABx1":
                    //StartCoroutine(LoadBombGeneric(weaponObject, "Mother of all Bombs", VehicleCompat.FA26B, false, true, "Mother of all Bombs", "MOAB", "Sends out an airburst comparable to that of a nuke, obliterating anything in a 1 mile radius, usually dropped from a high altitude to ensure the deployer's survival.", "13", 500f, "hpequips/afighter/fa26_gbu38x1", "GPS BOMB
                    LoadGeneric(weaponObject, weaponName, VehicleCompat.FA26B, false, true);
                    break;
                default:
                    Debug.LogError(name + " has not been implemented yet but is inside of custom weapon names."); // this should never occur
                    break;
            }
            weaponObject.SetActive(false);
        }
        Debug.Log("Done loading prefabs.");
    }

    private IEnumerator LoadSRB(GameObject weaponObject)
    {
        GameObject SRB94 = weaponObject;
        SRB94.AddComponent<HPEquipSRB>();
        SRB94.GetComponent<VTNetworking.VTNetEntity>().netSyncs.Add(SRB94.AddComponent<SRBSync>());
        SRB94.name = "AH-94 Flight Assist Solid Rocket Booster";
        SRB94.GetComponentInChildren<AudioSource>().outputAudioMixerGroup = VTResources.GetExteriorMixerGroup();
        GameObject SRB26 = Instantiate(SRB94);
        //SRB26.AddComponent<HPEquipSRB>();
        //SRB26.AddComponent<SRBSync>();
        SRB26.name = "FA-26B Flight Assist Solid Rocket Booster";
        GameObject SRB45 = Instantiate(SRB94);
        //SRB45.AddComponent<HPEquipSRB>();
        //SRB45.AddComponent<SRBSync>();
        SRB45.name = "F-45A Flight Assist Solid Rocket Booster";
        GameObject SRB42 = Instantiate(SRB94);
        //SRB42.AddComponent<HPEquipSRB>();
        //SRB42.AddComponent<SRBSync>();
        SRB42.name = "AV-42C Flight Assist Solid Rocket Booster";
        DontDestroyOnLoad(SRB94);
        DontDestroyOnLoad(SRB26);
        DontDestroyOnLoad(SRB45);
        DontDestroyOnLoad(SRB42);


        allCustomWeapons.Add("AH-94 Flight Assist Solid Rocket Booster", new CustomEqInfo(SRB94, VehicleCompat.AH94, false, false, "1,2,3,4"));
        allCustomWeapons.Add("FA-26B Flight Assist Solid Rocket Booster", new CustomEqInfo(SRB26, VehicleCompat.FA26B, false, false, "11,12,13"));
        allCustomWeapons.Add("F-45A Flight Assist Solid Rocket Booster", new CustomEqInfo(SRB45, VehicleCompat.F45A, false, false, "5,6,9,10"));
        allCustomWeapons.Add("AV-42C Flight Assist Solid Rocket Booster", new CustomEqInfo(SRB42, VehicleCompat.AV42C, false, false, "1,2,3,4"));
        //allCustomWeapons.Add("AH-94 Flight Assist Solid Rocket Booster", new CustomEqInfo(SRB, VehicleCompat.None, false, null));
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
        DontDestroyOnLoad(TLS);
        allCustomWeapons.Add("TacticalLaserSystem", new CustomEqInfo(TLS, VehicleCompat.FA26B, false, false));
        TLS.SetActive(false);
        Debug.Log("Loaded Tactical Laser System");
        yield break;
    }

    private GameObject LoadGeneric(GameObject weaponObject, string name, VehicleCompat vehicle, bool isExclude, bool isWMD, string equipPoints = null)
    {
        //GameObject weaponToInject = Instantiate(weaponObject);
        GameObject weaponToInject = weaponObject;
        DontDestroyOnLoad(weaponToInject);
        allCustomWeapons.Add(name, new CustomEqInfo(weaponToInject, vehicle, isExclude, isWMD, null));
        weaponToInject.SetActive(false);
        Debug.Log("Loaded " + name);
        foreach (AudioSource source in weaponToInject.GetComponentsInChildren<AudioSource>(true))
            source.outputAudioMixerGroup = VTResources.GetExteriorMixerGroup();
        return weaponToInject;
    }

    private IEnumerator LoadBombGeneric(GameObject weaponObject, string name, VehicleCompat compat, bool isExclude, bool isWMD, string weaponName, string shortName, string description, string equipPoints, float cost, string yoinkedEquipper = "hpequips/afighter/fa26_mk83x1", string sublabel = "BOMB")
    {
        GameObject newEquip = (GameObject)Instantiate(Resources.Load(yoinkedEquipper));
        newEquip.name = name;
        HPEquippable bomb = newEquip.GetComponent<HPEquippable>();
        bomb.fullName = name;
        bomb.shortName = shortName;
        bomb.name = weaponName;
        bomb.subLabel = sublabel;
        bomb.description = description;
        bomb.allowedHardpoints = equipPoints;
        bomb.unitCost = cost;
        Debug.Log("Try do missile launcher");
        MissileLauncher launcher = newEquip.GetComponentInChildren<MissileLauncher>();
        launcher.RemoveAllMissiles();
        launcher.missilePrefab = weaponObject;
        launcher.loadOnStart = true;
        launcher.RemoveAllMissiles();
        DontDestroyOnLoad(newEquip);
        DontDestroyOnLoad(weaponObject);
        allCustomWeapons.Add(name, new CustomEqInfo(newEquip, compat, isExclude, isWMD));
        weaponObject.SetActive(false);
        newEquip.SetActive(false);
        Debug.Log("Loaded generic bomb " + name);
        yield break;
    }

    private IEnumerator LoadADMM(GameObject weaponObject)
    {
        //GameObject launcher = Instantiate(weaponObject);
        GameObject launcher = weaponObject;
        launcher.AddComponent<HPEquipAllDirectionalMissileLauncher>();
        allCustomWeapons.Add("ADMM", new CustomEqInfo(launcher, VehicleCompat.FA26B, false, true));
        DontDestroyOnLoad(launcher);
        launcher.SetActive(false);
        Debug.Log("Loaded ADMM");
        yield break;
    }

    private IEnumerator LoadRailGun(GameObject weaponObject)
    {
        GameObject rg = weaponObject;
        rg.AddComponent<RailGun>();
        DontDestroyOnLoad(rg);
        allCustomWeapons.Add("45 Rail Gun", new CustomEqInfo(rg, VehicleCompat.F45A, false, true));
        rg.SetActive(false);
        weaponObject.GetComponentInChildren<AudioSource>().outputAudioMixerGroup = VTResources.GetExteriorMixerGroup();
        Debug.Log("Loaded Rail Gun");
        yield break;
    }

    private IEnumerator LoadAim280(GameObject weaponObject)
    {
        //GameObject missileObject = Instantiate(weaponObject);
        GameObject missileObject = weaponObject;
        GameObject dummyEquipper = Instantiate(Resources.Load("hpequips/afighter/af_aim9")) as GameObject;
        dummyEquipper.gameObject.name = "AIM-280";
        HPEquipIRML irml = dummyEquipper.GetComponent<HPEquipIRML>();
        GameObject dummyMissile = irml.ml.missilePrefab; // dummyMissile is the aim9 we are copying

        AudioSource ab = missileObject.GetComponent<AudioSource>();
        AudioSource YOINKED = dummyMissile.GetComponent<AudioSource>();
        ab.clip = Instantiate(YOINKED.clip);
        ab.outputAudioMixerGroup = YOINKED.outputAudioMixerGroup;

        Transform parent = missileObject.transform.Find("SeekerParent");
        AudioSource copiedSeeker = parent.Find("SeekerAudio").GetComponent<AudioSource>();
        AudioSource originalSeeker = dummyMissile.transform.Find("SeekerParent").Find("SeekerAudio").GetComponent<AudioSource>();
        copiedSeeker.clip = Instantiate(originalSeeker.clip);
        copiedSeeker.outputAudioMixerGroup = Instantiate(originalSeeker.outputAudioMixerGroup);

        AudioSource copiedLock = parent.Find("LockToneAudio").GetComponent<AudioSource>();
        AudioSource originalLock = dummyMissile.transform.Find("SeekerParent").Find("LockToneAudio").GetComponent<AudioSource>();
        copiedLock.clip = Instantiate(originalLock.clip);
        copiedLock.outputAudioMixerGroup = originalLock.outputAudioMixerGroup;
        //parent = missileObject.transform.Find("exhaustTransform");

        missileObject.AddComponent<EMP>();
        Destroy(dummyMissile);
        irml.fullName = "AIM-280 Electrozapper";
        irml.shortName = "AIM-280";
        irml.description = "An IR guided Air to Air missile that fries any electronics in its blast radius.";
        irml.ml.missilePrefab = missileObject;
        irml.allowedHardpoints = "1,2,3,4,5,6,7,8,9,10";
        irml.ml.loadOnStart = true;
        DontDestroyOnLoad(missileObject);
        DontDestroyOnLoad(dummyEquipper);
        irml.ml.RemoveAllMissiles();
        allCustomWeapons.Add("AIM-280", new CustomEqInfo(dummyEquipper, VehicleCompat.AH94, true, false));
        //allCustomWeapons.Add("AIM-280 Helicopter Variant", new CustomEqInfo(dummyEquipper, VehicleCompat.AH94, false, false, "5,6"));
        dummyEquipper.SetActive(false);
        missileObject.SetActive(false);
        Debug.Log("Loaded AIM-280");

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
        Transform hp0 = newEquip.transform.GetChild(0);
        allCustomWeapons.Add("B61", new CustomEqInfo(newEquip, VehicleCompat.AH94, true, true));
        b61.SetActive(false);
        newEquip.SetActive(false);
        Debug.Log("Loaded b61");
        yield break;
    }

    private IEnumerator LoadAim7(GameObject weaponObject)
    {
        GameObject missileObject = weaponObject;
        GameObject yoinkedEquipper = Instantiate(Resources.Load("hpequips/afighter/af_amraam")) as GameObject;
        HPEquipRadarML yoinkedMore = yoinkedEquipper.GetComponentInChildren<HPEquipRadarML>();
        yoinkedMore.fullName = "AIM-7 Sparrow";
        yoinkedMore.shortName = "AIM-7";
        yoinkedMore.description = "An antiquated Semi-Active radar long range air to air missile.";
        yoinkedMore.subLabel = "SEMI-RADAR AAM";
        yoinkedMore.allowedHardpoints = "1,2,3,4,5,6,7,8,9,10";
        GameObject dummyMissile = yoinkedMore.ml.missilePrefab;

        AudioSource ab = missileObject.GetComponent<AudioSource>();
        AudioSource YOINKED = dummyMissile.GetComponent<AudioSource>();
        if (YOINKED != null && YOINKED.clip != null)
        {
            ab.clip = Instantiate(YOINKED.clip);
            ab.outputAudioMixerGroup = YOINKED.outputAudioMixerGroup;
        }
        Destroy(dummyMissile);
        yoinkedMore.gameObject.name = "AIM-7";
        yoinkedMore.ml.missilePrefab = missileObject;
        yoinkedMore.ml.RemoveAllMissiles();
        DontDestroyOnLoad(missileObject);
        DontDestroyOnLoad(yoinkedMore);
        allCustomWeapons.Add("AIM-7", new CustomEqInfo(yoinkedEquipper, VehicleCompat.AV42C | VehicleCompat.AH94, true, false));
        missileObject.SetActive(false);
        yoinkedEquipper.SetActive(false);
        Debug.Log("Loaded Aim7");
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