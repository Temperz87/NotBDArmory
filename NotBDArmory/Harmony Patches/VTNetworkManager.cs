using Harmony;
using UnityEngine;
using VTNetworking;

[HarmonyPatch(typeof(VTNetworkManager), "NetInstantiate")]
public static class Instantiate_CustomPrefab
{
    public static bool Prefix(ref string resourcePath)
    {
        if (!Armory.CheckCustomWeapon(resourcePath) && !resourcePath.Contains("NotBDArmory"))
        {
            //Debug.Log(resourcePath + " is not a custom weapon to instantiate.");
            return true;
        }

        Debug.Log("Try net instantiate custom weapon " + resourcePath);
        resourcePath = "NotBDArmory/" + resourcePath.Substring(resourcePath.LastIndexOf("/") + 1);
        if (resourcePath.Contains("(Clone)"))
            resourcePath = resourcePath.Substring(0, resourcePath.IndexOf("(Clone)"));
        Debug.Log("Resource path is now " + resourcePath);
        return true;
    }
}