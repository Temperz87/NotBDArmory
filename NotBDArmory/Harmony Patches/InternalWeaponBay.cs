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

[HarmonyPatch(typeof(SMSInternalWeaponAnimator), "SetupForWeapon")]
public static class DontAnimateCustomWeapons
{
    public static bool Prefix(HPEquippable eq)
    {
        return !Armory.CheckCustomWeapon(eq.name);
    }
}
