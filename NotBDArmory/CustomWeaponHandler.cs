using Harmony;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using System;


public static class CustomWeaponHelper
{
    public static void EquipCustomWeapons(WeaponManager wm, Loadout loadout, bool quickloaded = false)
    {
        if (loadout == null)
            return;
        if (wm == null)
        {
            Debug.LogError("Weapn manager is null, returning.");
            return;
        }
        Traverse traverse = Traverse.Create(wm);
        HPEquippable[] equips = (HPEquippable[])traverse.Field("equips").GetValue();
        //MassUpdater component = wm.vesselRB.GetComponent<MassUpdater>();
        string[] hpLoadout = (string[])loadout.hpLoadout.Clone();
        ExternalOptionalHardpoints extHp = wm.GetComponentInChildren<ExternalOptionalHardpoints>(true);
        for (int idx = 0; idx < wm.hardpointTransforms.Length && idx < hpLoadout.Length; idx++)
        {
            if ((!string.IsNullOrEmpty(hpLoadout[idx]) && Armory.CheckCustomWeapon(hpLoadout[idx])))
            {
                Debug.Log(hpLoadout[idx] + " will be tried to be loaded on hp " + idx);
                GameObject customObject = GameObject.Instantiate(Armory.TryGetWeapon(hpLoadout[idx]).weaponObject, wm.hardpointTransforms[idx]);
                customObject.SetActive(true);
                HPEquippable HPEquipper = customObject.GetComponent<HPEquippable>();
                HPEquipper.SetWeaponManager(wm);
                equips[idx] = HPEquipper;
                traverse.Field("equips").SetValue(equips);
                HPEquipper.wasPurchased = true;
                HPEquipper.hardpointIdx = idx;
                HPEquipper.Equip();
                customObject.transform.localPosition = wm.hardpointTransforms[idx].localPosition;
                foreach (Component component in HPEquipper.gameObject.GetComponentsInChildren<Component>())
                {
                    if (component is IParentRBDependent)
                    {
                        ((IParentRBDependent)component).SetParentRigidbody(wm.vesselRB);
                    }
                    if (component is IRequiresLockingRadar)
                    {
                        ((IRequiresLockingRadar)component).SetLockingRadar(wm.lockingRadar);
                    }
                    if (component is IRequiresOpticalTargeter)
                    {
                        ((IRequiresOpticalTargeter)component).SetOpticalTargeter(wm.opticalTargeter);
                    }
                }
                if (HPEquipper is HPEquipIRML || HPEquipper is HPEquipRadarML)
                {
                    if (HPEquipper.dlz)
                    {
                        DynamicLaunchZone.LaunchParams dynamicLaunchParams = HPEquipper.dlz.GetDynamicLaunchParams(wm.transform.forward * 343f, wm.transform.position + wm.transform.forward * 10000f, Vector3.zero);
                        traverse.Field("maxAntiAirRange").SetValue(Mathf.Max(dynamicLaunchParams.maxLaunchRange, wm.maxAntiAirRange));
                    }
                }
                else if (HPEquipper is HPEquipARML)
                {
                    if (HPEquipper.dlz)
                    {
                        DynamicLaunchZone.LaunchParams dynamicLaunchParams2 = HPEquipper.dlz.GetDynamicLaunchParams(wm.transform.forward * 343f, wm.transform.position + wm.transform.forward * 10000f, Vector3.zero);
                        traverse.Field("maxAntiRadRange").SetValue(Mathf.Max(dynamicLaunchParams2.maxLaunchRange, wm.maxAntiRadRange));
                    }
                }
                else if (HPEquipper is HPEquipOpticalML && HPEquipper.dlz)
                {
                    DynamicLaunchZone.LaunchParams dynamicLaunchParams3 = HPEquipper.dlz.GetDynamicLaunchParams(wm.transform.forward * 280f, wm.transform.position + wm.transform.forward * 10000f, Vector3.zero);
                    traverse.Field("maxAGMRange").SetValue(Mathf.Max(dynamicLaunchParams3.maxLaunchRange, wm.maxAGMRange));
                }
                wm.ReportWeaponArming(HPEquipper);
                wm.ReportEquipJettisonMark(HPEquipper);
                if (HPEquipper.armable)
                    HPEquipper.armed = true;
                if (extHp != null)
                    extHp.Wm_OnWeaponEquippedHPIdx(idx);
                //if (extHp != null)
                //extHp.Wm_OnWeaponEquippedHPIdx(idx);
                Debug.Log("Equipped custom weapon " + hpLoadout[idx] + "!");
            }
        }
        wm.RefreshWeapon();
        wm.gameObject.AddComponent<CustomWeaponQuicksaveHandler>();
        Debug.Log("End try equipping weapon.");
    }

    class CustomWeaponQuicksaveHandler : MonoBehaviour
    {
        private void Awake()
        {
            QuicksaveManager.instance.OnQuickloadedMissiles += OnQuickload; // ensures that we quickload after the weaopn manager so our equips aren't overwritten by its equip funciton
            QuicksaveManager.instance.OnQuicksave += OnQuicksave;
            wm = GetComponent<WeaponManager>();
        }


        public void OnQuicksave(ConfigNode qsNode)
        {
            Debug.Log("Quicksaving custom weapons");
            ConfigNode node = new ConfigNode("CustomWeapons");
            for (int i = 0; i < wm.equipCount; i++)
            {
                HPEquippable equip = wm.GetEquip(i);
                if (equip != null)
                {
                    if (Armory.CheckCustomWeapon(equip.name))
                    {
                        Debug.Log("Found custom weapon " + equip.name);
                        ConfigNode equipNode = new ConfigNode("CustomEquip");
                        equipNode.SetValue<int>("hpIdx", i);
                        equipNode.SetValue<string>("name", equip.name);
                        equip.OnQuicksaveEquip(equipNode);
                        node.AddNode(equipNode);
                    }
                    else
                    {
                        Debug.Log(equip.name + " is not a custom equip.");
                    }
                }
            }
            qsNode.AddNode(node);
        }

        public void OnQuickload(ConfigNode qsNode)
        {
            Debug.Log("Quickloading custom weapons");
            Loadout loadout = new Loadout();
            loadout.hpLoadout = new string[wm.hardpointTransforms.Length];
            Dictionary<int, ConfigNode> customHardpoints = new Dictionary<int, ConfigNode>();
            if (qsNode.HasNode("CustomWeapons"))
            {
                foreach (ConfigNode configNode in qsNode.GetNode("CustomWeapons").GetNodes("CustomEquip"))
                {
                    string weaponName = configNode.GetValue("name");
                    if (!Armory.CheckCustomWeapon(weaponName))
                    {
                        Debug.Log(weaponName + " is not a custom weapon.");
                        continue;
                    }
                    Debug.Log("Found custom weapon " + weaponName);
                    int hpIdx = ConfigNodeUtils.ParseInt(configNode.GetValue("hpIdx"));
                    loadout.hpLoadout[hpIdx] = weaponName;
                    customHardpoints.Add(hpIdx, configNode);
                }
            }
            CustomWeaponHelper.EquipCustomWeapons(wm, loadout);
            foreach (int key in customHardpoints.Keys)
            {
                wm.GetEquip(key).OnQuickloadEquip(customHardpoints[key]);
            }
            Debug.Log("End quickload custom weapons.");
        }
        private WeaponManager wm;
    }
}