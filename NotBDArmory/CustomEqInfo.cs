using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
public class CustomEqInfo
{
    public CustomEqInfo(GameObject weaponObject, VTOLVehicles compatability)
    {
        this.weaponObject = weaponObject;
        this.compatability = compatability;
    }
    public CustomEqInfo(GameObject weaponObject) : this(weaponObject, VTOLVehicles.None) { }
    public bool CompareTo(VTOLVehicles vehicle)
    {
        return compatability == VTOLVehicles.None || vehicle == compatability;
    }
    public GameObject weaponObject;
    public VTOLVehicles compatability;
}
