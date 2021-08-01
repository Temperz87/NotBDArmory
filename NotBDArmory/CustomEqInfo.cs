using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
public class CustomEqInfo
{
    public CustomEqInfo(GameObject weaponObject, VTOLVehicles compatability, bool exclude)
    {
        this.weaponObject = weaponObject;
        this.compatability = compatability;
        this.isExclude = exclude;
    }
    public CustomEqInfo(GameObject weaponObject) : this(weaponObject, VTOLVehicles.None, false) { } // thir arg here doesn't matter
    public bool CompareTo(VTOLVehicles vehicle)
    {
        Debug.Log($"compatabillity == none: {compatability == VTOLVehicles.None}");
        Debug.Log($"vehicle == compatability {vehicle == compatability}");
        Debug.Log($"vehicle == compatability ^ is exclude {(vehicle == compatability) ^ isExclude}");
        Debug.Log($"whole thing  {compatability == VTOLVehicles.None || ((vehicle == compatability) ^ isExclude)}");
        return compatability == VTOLVehicles.None || ((vehicle == compatability) ^ isExclude); // i love hate xor
    }
    public GameObject weaponObject;
    public VTOLVehicles compatability;
    public bool isExclude;
}
