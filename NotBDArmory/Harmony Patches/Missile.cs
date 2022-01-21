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


[HarmonyPatch(typeof(Missile), "Awake")]
public static class Inject_Redo_Transform
{
    public static void Postfix(Missile __instance)
    {
        if (__instance.exhaustTransform == null)
            return;
        Traverse missileTraverse = Traverse.Create(__instance);
        ParticleSystem[] systems = (ParticleSystem[])missileTraverse.Field("ps").GetValue();
        if (__instance.exhaustTransform != null && (systems == null || systems.Length == 0))
        {
            missileTraverse.Field("ps").SetValue(__instance.exhaustTransform.GetComponentsInChildren<ParticleSystem>(true));
            Debug.Log("Redid ps for missile: " + __instance.name);
        }
        return;
    }
}

[HarmonyPatch(typeof(Missile), "UpdateTargetData")]
public static class Check_UpdateData
{
    public static bool Prefix(Missile __instance)
    {
        if (__instance.name.Contains("ADMM"))
            Patch_LOALToAny.skipNext = true;
        return true;
    }
}

[HarmonyPatch(typeof(Actor), "GetRoleMask")] // this is in missile because only one function calls it, and it's in missile
public static class Patch_LOALToAny
{
    public static bool Prefix(ref int __result)
    {
        __result = 286; // i did the | manually :P
        bool skip = skipNext;
        skipNext = false;
        return !skip;
    }
    public static bool skipNext = false;
}