using Harmony;
using UnityEngine;

[HarmonyPatch(typeof(Missile), "Awake")]
public static class Debug_MissileAwake
{
    public static void Postfix(Missile __instance)
    {
        Collider[] colliders = Traverse.Create(__instance).Field("colliders").GetValue() as Collider[];
        if (colliders == null)
        {
            Debug.LogError("Colliders null on misisle " + __instance.name + " in awake.");
        }
        else
            foreach (Collider collider in colliders)
                if (collider == null)
                    Debug.LogError("A collider in the array of colliders is null in awake for missile " + __instance.name);

    }
}

[HarmonyPatch(typeof(Missile), "Fire")]
public static class Debug_MissileFire
{
    public static bool Prefix(Missile __instance)
    {
        Collider[] colliders = Traverse.Create(__instance).Field("colliders").GetValue() as Collider[];
        if (colliders == null)
        {
            Debug.LogError("Colliders null on misisle " + __instance.name + " in awakfire.");
        }
        else
            foreach (Collider collider in colliders)
                if (collider == null)
                    Debug.LogError("A collider in the array of colliders is null in fire for missile " + __instance.name);
        return true;
    }
}