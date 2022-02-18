using Harmony;

[HarmonyPatch(typeof(SMSInternalWeaponAnimator), "SetupForWeapon")]
public static class DontAnimateCustomWeapons
{
    public static bool Prefix(HPEquippable eq)
    {
        return !Armory.CheckCustomWeapon(eq.name);
    }
}
