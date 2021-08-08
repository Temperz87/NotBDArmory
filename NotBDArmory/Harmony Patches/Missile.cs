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
public static class Redo_Transform
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