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


/*[HarmonyPatch(typeof(MissileLauncher), "OnLoadedMissile")]
public static class Ensure_PS
{
    public static void Postfix(MissileLauncher __instance, Missile m)
    {
        if (!Armory.CheckCustomWeapon(__instance.name))
        {
            Debug.Log(__instance.name + " is not a custom weapon.");
            return;
        }
        Traverse missileTraverse = Traverse.Create(m);
        ParticleSystem[] systems = (ParticleSystem[])missileTraverse.Field("ps").GetValue();
        if (systems == null || systems.Length == 0)
        {
            Debug.Log("Try resetup systems for missile of launcher " + __instance.name);
            if (m.exhaustTransform == null)
            {
                Debug.LogError("Exhaust transform is null.");
                //m.exhaustTransform = 
                return;
            }
            systems = m.exhaustTransform.GetComponentsInChildren<ParticleSystem>();
            missileTraverse.Field("ps").SetValue(systems);
        }
    }
}*/

[HarmonyPatch(typeof(Missile), "Awake")]
public static class Redo_Transform
{
    public static void Postfix(Missile __instance)
    {
        if (__instance.exhaustTransform == null)
        {
            Debug.LogWarning("exhaustTransform is null on missile: " + __instance.name);
            return;
        }
        Traverse missileTraverse = Traverse.Create(__instance);
        ParticleSystem[] systems = (ParticleSystem[])missileTraverse.Field("ps").GetValue();
        bool redops = false;
        if (systems == null)
        {
            Debug.LogError("Systems are null for missile: " + __instance.name);
            redops = true;
        }
        else if (systems.Length == 0)
        {
            Debug.LogError("Systems are 0 for missile: " + __instance.name);
            redops = true;
        }
        if (redops)
        {
            missileTraverse.Field("ps").SetValue(__instance.exhaustTransform.GetComponentsInChildren<ParticleSystem>(true));
            Debug.Log("Redo ps for missile: " + __instance.name);
        }
        return;
    }
}