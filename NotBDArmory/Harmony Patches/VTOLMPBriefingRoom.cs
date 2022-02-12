using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Harmony;
using UnityEngine;
using VTOLVR.Multiplayer;

//[HarmonyPatch(typeof(VTOLMPBriefingRoom), "OpenEquipConfig")]
//public class Ensure_NoCustomWeapons
//{
//    public static bool Prefix()
//    {
//        List<GameObject> toRemove = new List<GameObject>();
//        foreach (GameObject gameObject in PilotSaveManager.currentVehicle.allEquipPrefabs)
//            if (Armory.CheckCustomWeapon(gameObject.name))
//                toRemove.Add(gameObject);
//            else
//                Debug.Log("Not removing object " + gameObject.name);
//        foreach (GameObject go in toRemove)
//        {
//            Debug.Log("removing object " + go.name);
//            PilotSaveManager.currentVehicle.allEquipPrefabs.Remove(go);
//        }
//        return true;
//    }
//}

