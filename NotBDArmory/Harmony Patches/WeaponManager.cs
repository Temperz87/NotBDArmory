using Harmony;

[HarmonyPatch(typeof(WeaponManager))]
[HarmonyPatch("EquipWeapons")]
public class EquipCustomWeapons
{
    [HarmonyPatch(typeof(WeaponManager))]
    [HarmonyPatch(nameof(WeaponManager.EquipWeapons))]
    public class EquipCustomWeaponsPatch
    {
        public static void Postfix(WeaponManager __instance, Loadout loadout)
        {
            CustomWeaponHelper.EquipCustomWeapons(__instance, loadout);
        }
    }
}