using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Harmony;
using UnityEngine;
using UnityEngine.UI;
using VTOLVR.Multiplayer;

[HarmonyPatch(typeof(VTMPScenarioSettings), "SetupScenarioSettings")]
public class Inject_ArmorySettings
{
    public static void Postfix(VTMPScenarioSettings __instance)
    {
        if (!doneSettings)
        {
            GameObject settingsTemplate = __instance.iconsIndicator.transform.parent.gameObject;
            GameObject wmd = GameObject.Instantiate(settingsTemplate, settingsTemplate.transform.parent);
            wmd.SetActive(true);
            wmd.transform.localPosition = new Vector3(377.200012f, -120, 0);

            Transform wmdCheckmark = wmd.transform.Find(__instance.iconsIndicator.name);

            Debug.Log("Try setup vrInt");
            VRInteractable vrInt = wmd.GetComponent<VRInteractable>();
            vrInt.interactableName = "Allow NotBDArmory WMDS";
            vrInt.OnInteract = new UnityEngine.Events.UnityEvent();
            vrInt.OnInteract.AddListener(delegate
            {
                CustomWeaponHelper.allowWMDS = !CustomWeaponHelper.allowWMDS;
                wmdCheckmark.gameObject.SetActive(CustomWeaponHelper.allowWMDS);
            });
            vrInt.enabled = true;
            vrInt.sqrRadius = 0.0121f;

            wmdCheckmark.gameObject.SetActive(false);
            doneSettings = true;

            wmd.GetComponentInChildren<Text>().text = "WMDS";
            Debug.Log("WMD setting made.");

            wmd.GetComponentInParent<VRPointInteractableCanvas>().RefreshInteractables();
        }
    }
    public static bool doneSettings = false;
}
