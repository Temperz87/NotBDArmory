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

[HarmonyPatch(typeof(VTNetworkManager), "GetInstantiatePrefab")]
public static class Ensure_PrefabNotEnabled
{
    public static void Postfix(string resourcePath, ref GameObject __result)
    {
        if (resourcePath.Contains("NotBDArmory"))
        {
            Debug.Log("re instantiate prefab " + resourcePath + " and __result active is " + __result.activeSelf);
            if (__result.activeSelf)
                Debug.LogError("Why are you already active?");
            GameObject old = __result;
            __result = GameObject.Instantiate(__result);
            if (__result.activeSelf)
                Debug.LogError("Instantiation made new prefab active.");
            if (old.activeSelf)
                Debug.LogError("Instantiation made old prefab active.");
            if (old == __result)
                Debug.LogError("Your instances are fucked.");
        }
    }
}
