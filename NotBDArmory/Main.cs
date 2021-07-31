using Harmony;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using System;

public class Armory : VTOLMOD
{
    private static bool patched = false;
    public static Loadout sloadout;
    private static Loadout RestartedLoadout;
    private static Dictionary<string, CustomEqInfo> allCustomWeapons = null;
    public static string[] customweaponames = {
        "AIM-7",
        "B61x1",
        "AIM-280",
        "45 Rail Gun"
    };
    public void BeginEquipCustomWeapons()
    {

        CustomWeaponHandler handler = null;
        handler = VTOLAPI.GetPlayersVehicleGameObject().GetComponent<CustomWeaponHandler>();
        if (handler == null)
        {
            handler = VTOLAPI.GetPlayersVehicleGameObject().AddComponent<CustomWeaponHandler>();
            if (RestartedLoadout == null)
            {
                RestartedLoadout = new Loadout();
                RestartedLoadout.hpLoadout = (string[])sloadout.hpLoadout.Clone();
                handler.EquipCustomWeapons(sloadout);
                sloadout = null;
            }
            else
                handler.EquipCustomWeapons(RestartedLoadout);
        }
    }
    private void Start()
    {
        Debug.Log("Start called");
    }
    public override void ModLoaded()
    {
        if (!patched)
        {
            Debug.Log("Firstime start called.");
            HarmonyInstance.Create("Tempy.notbdarmory").PatchAll();
            Debug.Log("Try loading prefabs...");
            AssetBundle assBundel = AssetBundle.LoadFromFile(ModFolder + "/customweapon.assets");

            allCustomWeapons = new Dictionary<string, CustomEqInfo>();

            if (assBundel == null)
                throw new NullReferenceException("Could not find/load customweapon.assets, path should be " + ModFolder + "/customweapon.assets.");
            StartCoroutine(LoadWeaponsAsync(assBundel));
            patched = true;
            VTOLAPI.SceneLoaded += SceneChanged; // So when the scene is changed SceneChanged is called
            VTOLAPI.MissionReloaded += BeginEquipCustomWeapons;
            if (BOOM.instance == null)
            {
                GameObject BOOMObject = Instantiate(new GameObject()); // stupid solution time
                BOOMObject.AddComponent<BOOM>();
                DontDestroyOnLoad(BOOMObject);
            }
        }
        base.ModLoaded();
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
            GameObject weaponObject = Instantiate(handler.asset as GameObject);
            if (weaponObject == null)
            {
                Debug.Log("weaponObject is null for asset " + weaponName + "?");
                continue;
            }
            switch (weaponName)
            {
                case "AIM-7":
                    StartCoroutine(LoadAim7(weaponObject));
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
                case "AIM-280":
                    StartCoroutine(LoadAim280(weaponObject));
                    break;
                case "45 Rail Gun":
                    StartCoroutine(LoadRailGun(weaponObject));
                    break;
            }
        }

        Debug.Log("Done loading prefabs.");
    }
    private IEnumerator LoadRailGun(GameObject weaponObject)
    {
        GameObject rg = Instantiate(weaponObject);
        rg.AddComponent<RailGun>();
        DontDestroyOnLoad(rg);
        allCustomWeapons.Add("45 Rail Gun", new CustomEqInfo(rg, VTOLVehicles.F45A));
        rg.SetActive(false);
        GameObject gau22dumby = Instantiate(Resources.Load("hpequips/f45a/f45_gun") as GameObject);
        weaponObject.GetComponentInChildren<AudioSource>().outputAudioMixerGroup = gau22dumby.GetComponentInChildren<Gun>().fireAudioSource.outputAudioMixerGroup;
        Destroy(gau22dumby);
        Debug.Log("Loaded Rail Gun");
        yield break;
    }
    private IEnumerator LoadAim280(GameObject weaponObject)
    {
        GameObject missileObject = Instantiate(weaponObject);
        GameObject dummyEquipper = Instantiate(Resources.Load("hpequips/afighter/af_aim9") as GameObject);
        dummyEquipper.gameObject.name = "AIM-280";
        HPEquipIRML irml = dummyEquipper.GetComponent<HPEquipIRML>();
        GameObject dummyMissile = Instantiate(irml.ml.missilePrefab); // dummyMissile is the aim9 we are copying

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
        irml.allowedHardpoints = "1,2,3,4,5,6,7,8,9,10"; ;
        irml.ml.loadOnStart = true;
        DontDestroyOnLoad(missileObject);
        DontDestroyOnLoad(dummyEquipper);
        irml.ml.RemoveAllMissiles();
        allCustomWeapons.Add("AIM-280", new CustomEqInfo(dummyEquipper));
        dummyEquipper.SetActive(false);
        missileObject.SetActive(false);
        Debug.Log("Loaded AIM-280");

        yield break;
    }
    private IEnumerator LoadB61(GameObject equipObject, GameObject nukeObject)
    {
        GameObject b61 = Instantiate(nukeObject);
        b61.AddComponent<Nuke>();
        GameObject newEquip = Instantiate(equipObject);
        MissileLauncher launcher = newEquip.GetComponentInChildren<MissileLauncher>();
        launcher.RemoveAllMissiles();
        launcher.missilePrefab = b61;
        launcher.loadOnStart = true;
        launcher.RemoveAllMissiles();
        DontDestroyOnLoad(newEquip);
        DontDestroyOnLoad(b61);
        Transform hp0 = newEquip.transform.GetChild(0);
        /*for (int i = 0; i < hp0.childCount; i++)
        {
            Transform childtf = hp0.GetChild(i);
            Debug.Log("Destroying child " + childtf.name);
            Destroy(hp0.GetChild(i).gameObject); // why is the nuke like this
        }*/
        //MonoHelper.FindAllChildrenRecursively(b61.transform);
        MonoHelper.FindAllChildrenRecursively(newEquip.transform);
        allCustomWeapons.Add("B61x1", new CustomEqInfo(newEquip));
        b61.SetActive(false);
        //nukeObject.SetActive(false);
        newEquip.SetActive(false);
        Debug.Log("Loaded b61");
        yield break;
    }
    private IEnumerator LoadAim7(GameObject weaponObject)
    {

        GameObject missileObject = Instantiate(weaponObject);
        GameObject yoinkedEquipper = Instantiate(Resources.Load("hpequips/afighter/af_amraam") as GameObject);
        yoinkedEquipper.SetActive(true);
        HPEquipRadarML yoinkedMore = yoinkedEquipper.GetComponentInChildren<HPEquipRadarML>();
        yoinkedMore.fullName = "AIM-7 Sparrow";
        yoinkedMore.shortName = "AIM-7";
        yoinkedMore.description = "Semi-Active radar long range air to air missile.";
        yoinkedMore.subLabel = "SEMI-RADAR AAM";
        yoinkedMore.allowedHardpoints = "1,2,3,4,5,6,7,8,9,10";
        GameObject dummyMissile = Instantiate(yoinkedMore.ml.missilePrefab);

        AudioSource ab = missileObject.GetComponent<AudioSource>();
        AudioSource YOINKED = dummyMissile.GetComponent<AudioSource>();
        ab.clip = Instantiate(YOINKED.clip);
        ab.outputAudioMixerGroup = YOINKED.outputAudioMixerGroup;

        Destroy(dummyMissile);
        yoinkedMore.gameObject.name = "AIM-7";
        yoinkedMore.ml.missilePrefab = missileObject;
        yoinkedMore.ml.RemoveAllMissiles();
        DontDestroyOnLoad(missileObject);
        DontDestroyOnLoad(yoinkedMore);
        allCustomWeapons.Add("AIM-7", new CustomEqInfo(yoinkedEquipper));
        missileObject.SetActive(false);
        yoinkedEquipper.SetActive(false);
        Debug.Log("Loaded Aim7");
        yield break;
    }

    private void SceneChanged(VTOLScenes scenes)
    {
        if (scenes == VTOLScenes.Akutan || scenes == VTOLScenes.CustomMapBase) // If inside of a scene that you can fly in
        {
            BeginEquipCustomWeapons();
        }
        else
            RestartedLoadout = null;
    }

    public static bool CheckCustomWeapon(string name)
    {
        if (String.IsNullOrEmpty(name))
            return false;
        foreach (string customName in customweaponames)
            if (name.ToLower().Contains(customName.ToLower()))
                return true;
        return false;
    }
    public static CustomEqInfo TryGetWeapon(string name)
    {
        if (String.IsNullOrEmpty(name))
        {
            Debug.Log("name is null in try get weapon.");
            return null;
        }
        foreach (string customName in customweaponames)
            if (name.ToLower().Contains(customName.ToLower()))
                return allCustomWeapons[customName];
        Debug.LogError("Couldn't retrieve custom weapon " + name);
        return null;
    }

}