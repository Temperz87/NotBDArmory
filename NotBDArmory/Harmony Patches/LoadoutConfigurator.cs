using Harmony;
using System.Collections.Generic;
using UnityEngine;



[HarmonyPatch(typeof(LoadoutConfigurator), nameof(LoadoutConfigurator.Initialize))]
public class Inject_CustomWeapons
{
    [HarmonyPostfix]
    public static void Postfix(LoadoutConfigurator __instance, bool useMidflightEquips)
    {
        Traverse traverse = Traverse.Create(__instance);
        Dictionary<string, EqInfo> unlockedWeaponPrefabs = (Dictionary<string, EqInfo>)traverse.Field("unlockedWeaponPrefabs").GetValue();
        foreach (string name in Armory.allCustomWeapons.Keys)
        {
            if (name == null || Armory.allCustomWeapons[name] == null)
                continue;
            try
            {
                Debug.Log("Try add " + name + " to loadout configurator.");
                CustomEqInfo info = Armory.allCustomWeapons[name];
                if (__instance.uiOnly && !info.mpReady)
                    continue;
                if (!info.CompareTo(VTOLAPI.GetPlayersVehicleEnum()))
                    continue; // this used to be a return and it grinded my gears trying to find out why nothing worked in the av42c, found out why
                if (info == null)
                {
                    Debug.LogError(name + " was not found in all custom weaopns.");
                    continue;
                }
                if (info.weaponObject == null)
                {
                    Debug.LogError("info's weapon object is null.");
                    continue;
                }
                GameObject customWeapon = info.weaponObject;
                EqInfo eq = new EqInfo(GameObject.Instantiate(customWeapon), name);
                unlockedWeaponPrefabs.Add(name, eq);
                __instance.availableEquipStrings.Add(name);

                Debug.Log("Added weapon to list " + name);
            }
            catch (KeyNotFoundException)
            {
                Debug.LogError("Couldn't find weapon " + name);
                continue;
            }
        }

        if (useMidflightEquips && __instance.equips != null) // ensure that reloading works
        {
            foreach (HPEquippable equip in __instance.equips)
            {
                if (equip == null)
                    continue;
                if (!unlockedWeaponPrefabs.ContainsKey(equip.gameObject.name))
                    unlockedWeaponPrefabs.Add(equip.gameObject.name, new EqInfo(GameObject.Instantiate(Armory.TryGetWeapon(equip.gameObject.name).equip.gameObject), equip.gameObject.name));
            }
        }
        traverse.Field("unlockedWeaponPrefabs").SetValue(unlockedWeaponPrefabs);
        traverse.Field("allWeaponPrefabs").SetValue(unlockedWeaponPrefabs);
    }
}

[HarmonyPatch(typeof(LoadoutConfigurator), "Attach")]
public static class Patch_AttachCustomWeapon
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
            if (!__instance.uiOnly)
                helper.StartConfigRoutine(hpIdx, weaponName, __instance);
            else
                helper.AttachImmediate(hpIdx, weaponName, __instance);
            return false;
        }
        else
            Debug.Log(weaponName + " is not a custom weapon.");
        return true;
    }
    public static MonoHelper helper = null;
}
[HarmonyPatch(typeof(LoadoutConfigurator), "AttachImmediate")]
public static class Patch_AttachCustomWeaponImmediate
{
    public static bool Prefix(string weaponName, int hpIdx, LoadoutConfigurator __instance) // this entire functions existance stems from not being able to patch out get instantiated
    {
        if (Patch_AttachCustomWeapon.helper == null)
            Patch_AttachCustomWeapon.helper = __instance.gameObject.AddComponent<MonoHelper>();
        if (Armory.CheckCustomWeapon(weaponName))
        {
            __instance.Detach(hpIdx);
            Debug.Log("attaching immediate custom weapon " + weaponName);
            Patch_AttachCustomWeapon.helper.AttachImmediate(hpIdx, weaponName, __instance);
            return false;
        }
        else
            Debug.Log(weaponName + " is not a custom weapon.");
        return true;
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

/*[HarmonyPatch(typeof(LoadoutConfigurator), "CalculateTotalThrust")]
public static class Patch_IncludeSRBInTWR
{
    public static void Postfix(LoadoutConfigurator __instance)
    {
        Traverse lcTraverse = Traverse.Create(__instance);
        float totalThrust = (float)lcTraverse.Field("totalThrust").GetValue();
        foreach (HPEquipSRB srb in __instance.wm.GetComponentsInChildren<HPEquipSRB>(true))
        {
            if (srb.srb == null)
            {
                Debug.LogError("Srb.srb is null.");
                continue;
            }
            totalThrust += srb.srb.thrust;
        }
        lcTraverse.Field("totalThrust").SetValue(totalThrust);
    }
}*/