using Harmony;
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