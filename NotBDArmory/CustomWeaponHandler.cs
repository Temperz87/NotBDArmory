using Harmony;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using System;


class CustomWeaponHandler : MonoBehaviour
{
    private WeaponManager wm;
    private ExternalOptionalHardpoints extHp;
    private static CustomWeaponHandler instance;
    public CustomWeaponHandler()
    {
        wm = base.GetComponent<WeaponManager>();
        extHp = base.GetComponent<ExternalOptionalHardpoints>();
    }
    private void Awake()
    {
        if (wm == null)
            Debug.LogError("No weaponmanger on custom weapon handler");
        QuicksaveManager.instance.OnQuickloadedMissiles += OnQuickload; // ensures that we quickload after the weaopn manager so our equips aren't overwritten by its equip funciton
        QuicksaveManager.instance.OnQuicksave += OnQuicksave;
        instance = this;
    }

    public void EquipCustomWeapons(Loadout loadout, bool quickloaded = false)
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
        for (int idx = 0; idx < hpLoadout.Length; idx++)
        {
            if ((!string.IsNullOrEmpty(hpLoadout[idx]) && Armory.CheckCustomWeapon(hpLoadout[idx])))
            {
                Debug.Log(hpLoadout[idx] + " will be tried to be loaded on hp " + idx);
                GameObject customObject = Instantiate(Armory.TryGetWeapon(hpLoadout[idx]).weaponObject, wm.hardpointTransforms[idx]);
                customObject.SetActive(true);
                HPEquippable HPEquipper = customObject.GetComponent<HPEquippable>();
                HPEquipper.SetWeaponManager(wm);
                equips[idx] = HPEquipper;
                traverse.Field("equips").SetValue(equips);
                HPEquipper.wasPurchased = true;
                HPEquipper.hardpointIdx = idx;
                HPEquipper.Equip();
                customObject.transform.localPosition = wm.hardpointTransforms[idx].localPosition;
                if (HPEquipper.armable)
                    HPEquipper.armed = true;
                if (extHp != null)
                    extHp.Wm_OnWeaponEquippedHPIdx(idx);
                Debug.Log("Equipped custom weapon " + hpLoadout[idx] + "!");
                MonoHelper.FindAllChildrenRecursively(HPEquipper.transform);
            }
        }
        wm.RefreshWeapon();
        wm.SetLockingRadar(wm.lockingRadar); // to make sure that anything that needs radar will have it available, the func uses an interface to make it work
        Debug.Log("End try equipping weapon.");
    }

    public static void OnQuicksave(ConfigNode qsNode)
    {
        Debug.Log("Quicksaving custom weapons");
        ConfigNode node = new ConfigNode("CustomWeapons");
        for (int i = 0; i < instance.wm.equipCount; i++)
        {
            HPEquippable equip = instance.wm.GetEquip(i);
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

    public static void OnQuickload(ConfigNode qsNode)
    {
        if (instance == null)
            instance = VTOLAPI.GetPlayersVehicleGameObject().AddComponent<CustomWeaponHandler>();
        Debug.Log("Quickloading custom weapons");
        Loadout loadout = new Loadout();
        loadout.hpLoadout = new string[instance.wm.hardpointTransforms.Length];
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
        instance.EquipCustomWeapons(loadout);
        foreach (int key in customHardpoints.Keys)
        {
            instance.wm.GetEquip(key).OnQuickloadEquip(customHardpoints[key]);
        }
        Debug.Log("End quickload custom weapons.");
    }
}
