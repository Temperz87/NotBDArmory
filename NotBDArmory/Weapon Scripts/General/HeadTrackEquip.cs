using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Reflection;
using Harmony;

class HeadTrackEquip : MonoBehaviour
{

    private ModuleTurret turret;
    private HPEquippable equip;

    private void Awake()
    {
        turret = GetComponent<ModuleTurret>();
        equip = GetComponent<HPEquippable>();
    }

    private void LateUpdate()
    {
        if (equip.itemActivated)
            turret.AimToTarget(VRHead.instance.transform.position + VRHead.instance.transform.forward * 2000f, true, true, false);
    }
}