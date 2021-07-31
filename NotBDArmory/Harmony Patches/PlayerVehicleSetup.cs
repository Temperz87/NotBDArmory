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

[HarmonyPatch(typeof(PlayerVehicleSetup))]
[HarmonyPatch("SetupForFlight")]
public class Patch2
{
    [HarmonyPrefix]
    public static bool Prefix(PlayerVehicle __instance)
    {
        Loadout notStaticLoadout = new Loadout();
        notStaticLoadout.hpLoadout = (string[])VehicleEquipper.loadout.hpLoadout.Clone();
        notStaticLoadout.cmLoadout = (int[])VehicleEquipper.loadout.cmLoadout.Clone();
        notStaticLoadout.normalizedFuel = VehicleEquipper.loadout.normalizedFuel;
        Armory.sloadout = notStaticLoadout;
        Debug.Log("Before prefix loadout");
        foreach (var equip in VehicleEquipper.loadout.hpLoadout)
        {
            Debug.Log(equip);
        }
        Debug.Log("Before prefix sloadout (already newed the shit)");
        foreach (var equip in Armory.sloadout.hpLoadout)
        {
            Debug.Log(equip);
        }
        Debug.Log("Changing shit.");
        for (int i = 0; i < VehicleEquipper.loadout.hpLoadout.Length; i++)
        {
            if (VehicleEquipper.loadout.hpLoadout[i] == "" || Armory.CheckCustomWeapon(VehicleEquipper.loadout.hpLoadout[i]))
            {
                VehicleEquipper.loadout.hpLoadout[i] = null;
            }
        };
        Debug.Log("After prefix loadout");
        foreach (var equip in VehicleEquipper.loadout.hpLoadout)
        {
            Debug.Log(equip);
        }
        Debug.Log("After prefix sloadout");
        foreach (var equip in Armory.sloadout.hpLoadout)
        {
            Debug.Log(equip);
        }
        return true;
    }
}
