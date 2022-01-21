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
        foreach (string name in Armory.allCustomWeapons.Keys)
        {
            try
            {
                Debug.Log("Try add " + name + " to loadout configurator.");
                CustomEqInfo info = Armory.TryGetWeapon(name);
                if (!info.CompareTo(VTOLAPI.GetPlayersVehicleEnum()))
                    continue; // this used to be a return and it grinded my gears trying to find out why nothing worked in the av42c, found out why
                if (info == null)
                {
                    Debug.LogError(name + " was not found in all custom weaopns.");
                    continue;
                }
                GameObject customWeapon = info.weaponObject;
                   EqInfo eq = new EqInfo(GameObject.Instantiate(customWeapon), name);
                if (holocamDummy != null)
                    eq.eq.transform.position = holocamDummy.transform.position; // this doesn't work
                unlockedWeaponPrefabs.Add(name, eq); 
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
    public static bool Prefix(string weaponName, int hpIdx, LoadoutConfigurator __instance) // this entire functions existance stems from not being able to patch out get instantiated
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