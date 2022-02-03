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
using VTNetworking;
using VTOLVR.Multiplayer;

[HarmonyPatch(typeof(VTMPMainMenu), "LaunchMPGameForScenario")]
public class Custom_LobbyData
{
    public static void Postfix()
    {
        if (VTOLMPLobbyManager.isLobbyHost)
            VTOLMPLobbyManager.currentLobby.SetData("WMD", (CustomWeaponHelper.allowWMDS ? 1 : 0).ToString());
        else if (VTOLMPLobbyManager.isInLobby)
        {
            string wmdAllowed = VTOLMPLobbyManager.currentLobby.GetData("WMD");
            CustomWeaponHelper.allowWMDS = int.Parse(wmdAllowed) != 0;
        }
    }
}