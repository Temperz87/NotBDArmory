using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

class CGT : MonoBehaviour
{
    HPEquipGun equip;
    private void Awake()
    {
        equip = gameObject.GetComponent<HPEquipGun>();
        if (equip == null)
            Debug.LogError("equip is null on cgt.");
        equip.OnEquipped += yoinkWM;
    }
    private void yoinkWM()
    {
        Debug.Log("Try yoink cgt material.");
        Material ext = equip.weaponManager.GetComponent<AircraftLiveryApplicator>().materials[0];

        MaterialPropertyBlock block = new MaterialPropertyBlock();

        equip.weaponManager.transform.Find("sevtf_layer_2").Find("VertStabLeftPart").Find("vertStabLeft").Find("vertStabLeft_mdl").GetComponent<MeshRenderer>().GetPropertyBlock(block);

        MeshRenderer mat1 = transform.Find("ConformalGunTank").Find("SEVTF 1").Find("WeaponBays").Find("GunDoor").Find("HP_Gun").Find("ConformalGunTank").GetComponent<MeshRenderer>();
        mat1.material = ext;
        mat1.SetPropertyBlock(block);

        MeshRenderer mat2 = transform.Find("ConformalGunTank").Find("SEVTF 1").Find("WeaponBays").Find("GunDoor").Find("HP_Gun").Find("ConformalGunTank").Find("LeftGunDoorGroup").Find("LeftGunDoor").GetComponent<MeshRenderer>();
        mat2.material = ext;
        mat2.SetPropertyBlock(block);

        MeshRenderer mat3 = transform.Find("ConformalGunTank").Find("SEVTF 1").Find("WeaponBays").Find("GunDoor").Find("HP_Gun").Find("ConformalGunTank").Find("RightGunDoorGroup").Find("RightGunDoor").GetComponent<MeshRenderer>();
        mat3.material = ext;
        mat3.SetPropertyBlock(block);


        Debug.Log("Set Material CGT");
    }
}