using Harmony;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using System;

class Prefab_Loader : MonoBehaviour
{
    public static Dictionary<string, GameObject> allCustomWeapons = null;
    public void LoadWeapons(AssetBundle bundle)
    {
        StartCoroutine(LoadWeaponsAsync(bundle));
    }
    private IEnumerator LoadWeaponsAsync(AssetBundle bundle)
    {
        foreach (string weaponName in Armory.customweaponames)
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
                case "Rail Gun":
                    StartCoroutine(LoadRailGun(weaponObject));
                    break;
            }
        }

        Debug.Log("Done loading prefabs.");
    }
    private IEnumerator LoadRailGun(GameObject weaponObject)
    {
        GameObject rg = Instantiate(weaponObject);
        HPEquipGun gun = rg.GetComponent<HPEquipGun>();
        Animation animation = rg.GetComponent<Animation>();
        //rg.AddComponent<RailGun>();
        //rg.animation = animation;
        DontDestroyOnLoad(rg);
        allCustomWeapons.Add("Rail Gun", rg);
        rg.SetActive(false);
        yield break;
    }
    private IEnumerator LoadAim280(GameObject weaponObject)
    {
        GameObject missile = Instantiate(weaponObject);
        GameObject equipper = Instantiate(Resources.Load("hpequips/afighter/af_aim9") as GameObject);
        equipper.gameObject.name = "AIM-280";
        HPEquipIRML irml = equipper.GetComponent<HPEquipIRML>();
        Transform parent = missile.transform.Find("SeekerParent");
        GameObject dummyMissile = Instantiate(irml.ml.missilePrefab);
        AudioSource copiedSeeker = parent.Find("SeekerAudio").GetComponent<AudioSource>();
        AudioSource originalSeeker = dummyMissile.transform.Find("SeekerParent").Find("SeekerAudio").GetComponent<AudioSource>();
        copiedSeeker.clip = Instantiate(originalSeeker.clip);
        copiedSeeker.outputAudioMixerGroup = originalSeeker.outputAudioMixerGroup;

        AudioSource copiedLock = parent.Find("LockToneAudio").GetComponent<AudioSource>();
        AudioSource originalLock = dummyMissile.transform.Find("SeekerParent").Find("LockToneAudio").GetComponent<AudioSource>();
        copiedLock.clip = Instantiate(originalLock.clip);
        copiedLock.outputAudioMixerGroup = originalLock.outputAudioMixerGroup;
        parent = missile.transform.Find("exhaustTransform");
        foreach (Transform children in dummyMissile.transform.Find("exhaustTransform").GetComponentsInChildren<Transform>())
        {
            if (children.name != "exhaustTransform")
                continue;
            transform.SetParent(parent);
            transform.position = Vector3.zero;
            break;
        }
        Debug.Log("I AM NOW DESTROYING THE DUMMY MISSILE");
        dummyMissile.SetActive(false);
        Debug.Log("SET INACTIVE");
        if (!this.isActiveAndEnabled)
            Debug.Log("HOW IS THE OBJECT INSTANTE SET TO THE WRONG INSTANCE WHAT THE FUCK IS WRONG WEITH YOU WHY ARE YOU LIKE THIS.");
        Destroy(dummyMissile);
        Debug.Log("I TRIED TO DESTROY THE DUMMY MISSILE");
        irml.fullName = "AIM-280 Electrozapper";
        irml.shortName = "AIM-280";
        irml.description = "An IR guided Air to Air missile that fries any electronics in its blast radius.";
        irml.ml.missilePrefab = missile;
        irml.allowedHardpoints = "0,1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,18,20";
        irml.ml.loadOnStart = true;
        missile.AddComponent<EMP>();
        DontDestroyOnLoad(missile);
        DontDestroyOnLoad(equipper);
        irml.ml.RemoveAllMissiles();
        allCustomWeapons.Add("AIM-280", equipper);
        equipper.SetActive(false);
        missile.SetActive(false);
        Debug.Log("Loaded AIM-280");
        /*if (missile.transform.Find("fireTf") != null)
            if (missile.transform.Find("fireTf").Find("AIM-9") != null)
                Destroy(missile.transform.Find("fireTf").Find("AIM-9"));*/
        yield break;
    }
    private IEnumerator LoadB61(GameObject equipObject, GameObject nukeObject)
    {
        GameObject b61 = Instantiate(nukeObject);
        b61.GetComponentInChildren<Missile>().gameObject.AddComponent<Warhead>();
        GameObject newEquip = Instantiate(equipObject);
        MissileLauncher launcher = newEquip.GetComponentInChildren<MissileLauncher>();
        launcher.missilePrefab = b61;
        launcher.RemoveAllMissiles();
        DontDestroyOnLoad(newEquip);
        DontDestroyOnLoad(b61);
        allCustomWeapons.Add("B61x1", newEquip);
        b61.SetActive(false);
        //nukeObject.SetActive(false);
        newEquip.SetActive(false);
        Debug.Log("Loaded b61");
        yield break;
    }
    private IEnumerator LoadAim7(GameObject weaponObject)
    {
        GameObject yoinkedEquipper = Instantiate(Resources.Load("hpequips/afighter/af_amraam") as GameObject);
        yoinkedEquipper.SetActive(true);
        HPEquipRadarML yoinkedMore = yoinkedEquipper.GetComponentInChildren<HPEquipRadarML>();
        yoinkedMore.fullName = "AIM-7 Sparrow";
        yoinkedMore.shortName = "AIM-7";
        yoinkedMore.description = "Semi-Active radar long range air to air missile.";
        yoinkedMore.subLabel = "SEMI-RADAR AAM";
        yoinkedMore.allowedHardpoints = "0,1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,18,20";
        GameObject dummyMissile = Instantiate(yoinkedMore.ml.missilePrefab);
        Transform particleSystemYoinked = dummyMissile.transform.Find("exhaustTransform").Find("MissileTrail");
        particleSystemYoinked.SetParent(weaponObject.transform.Find("exhaustTransform"));
        particleSystemYoinked.localPosition = Vector3.zero;
        weaponObject.GetComponent<AudioSource>().clip = Instantiate(dummyMissile.GetComponent<AudioSource>().clip);
        foreach (Transform children in dummyMissile.transform.Find("exhaustTransform").GetComponentsInChildren<Transform>())
        {
            if (children.name != "exhaustTransform")
                continue;
            transform.SetParent(weaponObject.transform.Find("exhaustTransform"));
            transform.position = Vector3.zero;
            break;
        }
        Destroy(dummyMissile);
        yoinkedMore.gameObject.name = "AIM-7";
        yoinkedMore.ml.missilePrefab = weaponObject;
        yoinkedMore.ml.RemoveAllMissiles();
        DontDestroyOnLoad(weaponObject);
        DontDestroyOnLoad(yoinkedMore);
        allCustomWeapons.Add("AIM-7", yoinkedMore.gameObject);
        weaponObject.SetActive(false);
        yoinkedEquipper.SetActive(false);
        Debug.Log("Loaded Aim7");
        yield break;
    }
}
