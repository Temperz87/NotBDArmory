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

/*[HarmonyPatch(typeof(WeaponManager))] WIP MP Integration
[HarmonyPatch("EquipWeapons")]
public class EquipCustomWeapons
{
    public static void Postfix(WeaponManager __instance, Loadout loadout)
    {
        if (handler == null)
            handler = VRHead.instance.gameObject.AddComponent<CustomWeaponHandler>(); // just a handy place to put it
        handler.EquipCustomWeapons(loadout, __instance);
    }
    private static CustomWeaponHandler handler;
}*/