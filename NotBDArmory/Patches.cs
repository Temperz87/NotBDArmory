using Harmony;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


[HarmonyPatch(typeof(PlayerVehicleSetup))]
[HarmonyPatch("SetupForFlight")]
public class Patch2
{
    [HarmonyPrefix]
    public static bool Prefix(PlayerVehicle __instance)
    {
        Loadout notStaticLoadout = new Loadout();
        notStaticLoadout.hpLoadout = (string[])VehicleEquipper.loadout.hpLoadout.Clone();
        notStaticLoadout.cmLoadout = (int[])VehicleEquipper.loadout.cmLoadout.Clone();
        notStaticLoadout.normalizedFuel = VehicleEquipper.loadout.normalizedFuel;
        Armory.sloadout = notStaticLoadout;
        Debug.Log("Before prefix loadout");
        foreach (var equip in VehicleEquipper.loadout.hpLoadout)
        {
            Debug.Log(equip);
        }
        Debug.Log("Before prefix sloadout (already newed the shit)");
        foreach (var equip in Armory.sloadout.hpLoadout)
        {
            Debug.Log(equip);
        }
        Debug.Log("Changing shit.");
        for (int i = 0; i < VehicleEquipper.loadout.hpLoadout.Length; i++)
        {
            if (VehicleEquipper.loadout.hpLoadout[i] == "" || Armory.CheckCustomWeapon(VehicleEquipper.loadout.hpLoadout[i]))
            {
                VehicleEquipper.loadout.hpLoadout[i] = null;
            }
        };
        Debug.Log("After prefix loadout");
        foreach (var equip in VehicleEquipper.loadout.hpLoadout)
        {
            Debug.Log(equip);
        }
        Debug.Log("After prefix sloadout");
        foreach (var equip in Armory.sloadout.hpLoadout)
        {
            Debug.Log(equip);
        }
        return true;
    }
}

[HarmonyPatch(typeof(LoadoutConfigurator), nameof(LoadoutConfigurator.Initialize))]
public class InjectCustomWeapons
{
    [HarmonyPostfix]
    public static void Postfix(LoadoutConfigurator __instance)
    {
        Debug.Log("Doing postfix.");
        Traverse traverse = Traverse.Create(__instance);
        Debug.Log("Start eq info");
        Dictionary<string, EqInfo> unlockedWeaponPrefabs = (Dictionary<string, EqInfo>)traverse.Field("unlockedWeaponPrefabs").GetValue();
        //Debug.Log("Got eq info.");
        GameObject holocamDummy = Resources.FindObjectsOfTypeAll<HPEquippable>().FirstOrDefault().gameObject;
        if (holocamDummy == null)
            Debug.LogError("Dumb");
        foreach (string name in Armory.customweaponames)
        {
            try
            {
                Debug.Log("Try add " + name + " to loadout configurator.");
                CustomEqInfo info = Armory.TryGetWeapon(name);
                if (!info.CompareTo(VTOLAPI.GetPlayersVehicleEnum()))
                    return;
                if (info == null)
                {
                    Debug.LogError(name + " was not found in all custom weaopns.");
                    continue;
                }
                GameObject customWeapon = info.weaponObject;
                EqInfo eq = new EqInfo(GameObject.Instantiate(customWeapon), name);
                if (holocamDummy != null)
                    eq.eq.transform.position = holocamDummy.transform.position;
                unlockedWeaponPrefabs.Add(name, eq); // Sadly can't prefix a method so shitty shit 
                Debug.Log("Added weapon to list " + name);
            }
            catch (KeyNotFoundException)
            {
                Debug.LogError("Couldn't find weapon " + name);
                continue;
            }
        }
        traverse.Field("unlockedWeaponPrefabs").SetValue(unlockedWeaponPrefabs);
        traverse.Field("allWeaponPrefabs").SetValue(unlockedWeaponPrefabs);
        Debug.Log("Postfix 200.");
    }
}

[HarmonyPatch(typeof(LoadoutConfigurator), "Attach")]
public static class PatchAttachCustomWeapon
{
    public static bool Prefix(string weaponName, int hpIdx, LoadoutConfigurator __instance)
    {
        if (helper == null)
        {
            helper = __instance.gameObject.AddComponent<MonoHelper>();
        }
        if (Armory.CheckCustomWeapon(weaponName))
        {
            __instance.Detach(hpIdx);
            Debug.Log("attaching custom weapon " + weaponName);
            helper.StartConfigRoutine(hpIdx, weaponName, __instance);
            return false;
        }
        else
            Debug.Log(weaponName + " is not a custom weapon.");
        return true;
    }
    private static MonoHelper helper = null;
}

public class MonoHelper : MonoBehaviour
{
    public void StartConfigRoutine(int hpIdx, string weaponName, LoadoutConfigurator __instance)
    {
        try
        {
            StartCoroutine(attachRoutine(hpIdx, weaponName, __instance));
        }
        catch (NullReferenceException e)
        {
            if (weaponName != null && __instance != null)
                Debug.LogError("TEMPERZ YOU PARENTED IT TO THE WRONG THING AGAIN YOU NIMWIT");
            throw e;
        }
    }
    private static IEnumerator attachRoutine(int hpIdx, string weaponName, LoadoutConfigurator __instance)
    {
        Debug.Log("Starting attach routine.");
        Traverse LCPTraverse = Traverse.Create(__instance);
        Coroutine[] detachRoutines = (Coroutine[])LCPTraverse.Field("detachRoutines").GetValue();
        if (detachRoutines[hpIdx] != null)
        {
            yield return detachRoutines[hpIdx];
        }
        GameObject instantiated = Instantiate(Armory.TryGetWeapon(weaponName).weaponObject);
        instantiated.name = weaponName;
        instantiated.SetActive(true);
        InternalWeaponBay iwb = __instance.GetWeaponBay(hpIdx);
        if (iwb)
            iwb.Open();

        Transform weaponTf = instantiated.transform;
        Transform[] allTfs = (Transform[])(LCPTraverse.Field("hpTransforms").GetValue());
        Transform hpTf = allTfs[hpIdx];
        __instance.equips[hpIdx] = weaponTf.GetComponent<HPEquippable>();
        __instance.equips[hpIdx].OnConfigAttach(__instance);
        weaponTf.rotation = hpTf.rotation;
        Vector3 localPos = new Vector3(0f, -4f, 0f);
        weaponTf.position = hpTf.TransformPoint(localPos);
        __instance.UpdateNodes();
        Vector3 tgt = new Vector3(0f, 0f, 0.5f);
        if (VTOLAPI.GetPlayersVehicleEnum() == VTOLVehicles.F45A)
        {
            if (hpIdx >= 5 && hpIdx <= 10)
                GameObject.Find("extPylon" + (hpIdx - 4).ToString())?.SetActive(true); // fucking hacks amirite?
        }
        while ((localPos - tgt).sqrMagnitude > 0.01f)
        {
            localPos = Vector3.Lerp(localPos, tgt, 5f * Time.deltaTime);
            weaponTf.position = hpTf.TransformPoint(localPos);
            yield return null;
        }
        weaponTf.parent = hpTf;
        weaponTf.localPosition = tgt;
        weaponTf.localRotation = Quaternion.identity;
        __instance.vehicleRb.AddForceAtPosition(Vector3.up * __instance.equipImpulse, __instance.wm.hardpointTransforms[hpIdx].position, ForceMode.Impulse);
        AudioSource[] allAudioTfs = (AudioSource[])(LCPTraverse.Field("hpAudioSources").GetValue());
        allAudioTfs[hpIdx].PlayOneShot(__instance.attachAudioClip);
        __instance.attachPs.transform.position = hpTf.position;
        __instance.attachPs.FireBurst();
        yield return new WaitForSeconds(0.2f);
        while (weaponTf.localPosition.sqrMagnitude > 0.001f)
        {
            weaponTf.localPosition = Vector3.MoveTowards(weaponTf.localPosition, Vector3.zero, 4f * Time.deltaTime);
            yield return null;
        }
        weaponTf.localPosition = Vector3.zero;
        __instance.UpdateNodes();
        if (iwb)
            iwb.Close();
        Coroutine[] attachRoutines = (Coroutine[])LCPTraverse.Field("attachRoutines").GetValue();
        attachRoutines[hpIdx] = null;
        LCPTraverse.Field("attachRoutines").SetValue(attachRoutines);
        Debug.Log("Ended attach routine.");
        FindAllChildrenRecursively(instantiated.transform);
        yield break;
    }
    public static void FindAllChildrenRecursively(Transform parent)
    {
        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);
            if (child == null)
                continue;
            Debug.Log("Found child of parent " + parent.name + " called " + child);
            FindAllChildrenRecursively(child);
        }
    }
}

[HarmonyPatch(typeof(HPConfiguratorFullInfo), nameof(HPConfiguratorFullInfo.AttachSymmetry))]
public static class Ensure_Symmetry
{
    public static bool Prefix(string weaponName, int hpIdx, HPConfiguratorFullInfo __instance)
    {
        if (!Armory.CheckCustomWeapon(weaponName))
            return true;
        GameObject equip = Armory.TryGetWeapon(weaponName).weaponObject;
        if (equip == null)
            return false;
        int weirdbitwiseshit = LoadoutConfigurator.EquipCompatibilityMask(equip.GetComponent<HPEquippable>());
        Debug.Log("tried get bitwise op.");
        if (__instance.configurator.wm.symmetryIndices != null && hpIdx < __instance.configurator.wm.symmetryIndices.Length) // I have no idea how baha did this at all
        {
            int num = __instance.configurator.wm.symmetryIndices[hpIdx];
            if (num < 0 || __instance.configurator.lockedHardpoints.Contains(num))
            {
                return false;
            }
            int WHATISTHISVARIABLE = 1 << num;

            if ((WHATISTHISVARIABLE & weirdbitwiseshit) == WHATISTHISVARIABLE)
            {
                __instance.configurator.Attach(weaponName, num);
            }
        }
        return false;
    }
}

[HarmonyPatch(typeof(SMSInternalWeaponAnimator), "SetupForWeapon")]
public static class DontAnimateCustomWeapns
{
    public static bool Prefix(HPEquippable eq)
    {
        return !Armory.CheckCustomWeapon(eq.name);
    }
}