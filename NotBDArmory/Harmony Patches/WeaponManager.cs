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

[HarmonyPatch(typeof(WeaponManager))] //WIP MP Integration
[HarmonyPatch("EquipWeapons")]
public class EquipCustomWeapons
{
    [HarmonyPatch(typeof(WeaponManager))]
    [HarmonyPatch(nameof(WeaponManager.EquipWeapons))]
    public class EquipCustomWeaponsPatch
    {
        public static void Postfix(WeaponManager __instance, Loadout loadout)
        {
            CustomWeaponHelper.EquipCustomWeapons(__instance, loadout);
        }
    }
}