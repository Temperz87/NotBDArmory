using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using VTNetworking;

public class CustomEqInfo
{
    public GameObject weaponObject;
    public VehicleCompat compatability;
    public HPEquippable equip;
    public bool isExclude;
    public bool isWMD;
    public bool mpReady = false;

    public CustomEqInfo(GameObject weaponObject, VehicleCompat compatability, bool exclude, bool WMD, string equips = null, bool mpReady = false)
    {
        this.weaponObject = weaponObject;
        equip = weaponObject.GetComponent<HPEquippable>();
        this.compatability = compatability;
        this.isExclude = exclude;
        this.isWMD = WMD;
        this.mpReady = mpReady;
        if (equips != null)
            weaponObject.GetComponent<HPEquippable>().allowedHardpoints = equips;
        string name = weaponObject.name;
        if (name.Contains("(Clone)"))
            name = weaponObject.name.Substring(0, weaponObject.name.IndexOf("(Clone)"));
        VTNetworkManager.RegisterOverrideResource("NotBDArmory/" + name, weaponObject);
        if (equip is HPEquipMissileLauncher)
        {
            try
            {
                HPEquipMissileLauncher launcher = equip as HPEquipMissileLauncher;
                if (!launcher.missileResourcePath.Contains("NotBDArmory/"))
                    launcher.missileResourcePath = "NotBDArmory/" + name + "Missile";
                VTNetworkManager.RegisterOverrideResource(launcher.missileResourcePath, launcher.ml.missilePrefab);
            }
            catch (NullReferenceException e)
            {
                Debug.Log("Caught NRE while trying to add missile launcher path to override resources, of launcher " + name + " stack trace,");
            }
        }
    }
    public CustomEqInfo(GameObject weaponObject) : this(weaponObject, VehicleCompat.None, false, true, null, false) { }

    public bool CompareTo(VTOLVehicles vehicle) // is this overengineered? yes. Do I care? Yes ;(
    {
        VehicleCompat bitMask = convert(vehicle);
        bool compatFlag = ((int)(compatability & bitMask) != 0) ^ isExclude; // we compare & and if it isn't 0 we can assume that one of our selected masks is true, and since don't EVER want to flag and exclude to be the same so xor 
        return compatFlag && (!isWMD || (isWMD && CustomWeaponHelper.allowWMDS) || !VTNetworkManager.hasInstance || VTNetworkManager.instance.connectionState != VTNetworkManager.ConnectionStates.Connected);
    }

    public static VehicleCompat convert(VTOLVehicles v)
    {
        if (v == VTOLVehicles.AV42C)
            return VehicleCompat.AV42C;
        else if (v == VTOLVehicles.FA26B)
            return VehicleCompat.FA26B;
        else if (v == VTOLVehicles.F45A)
            return VehicleCompat.F45A;
        else if (v == VTOLVehicles.AH94)
            return VehicleCompat.AH94;
        return VehicleCompat.None;
    }
}

public enum VehicleCompat // I'm just gonna leave this here, yes this is bad practice but come on
{
    None = 0,
    AV42C = 1,
    FA26B = 2,
    F45A = 4,
    AH94 = 8
}