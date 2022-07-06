using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Reflection;
using Harmony;

class HPEquipLaserTurret : HPEquipLaser
{
    public HPEquipLaserTurret()
    {
        fullName = "Vampiric Laser System";
        shortName = "VLS";
        unitCost = 1500f;
        description = "Originally made to defeat an ancient enemy, a crystal was dug up by archaeologists and mounted onto an aircraft. The crystal exhibits alchemical properties.";
        subLabel = "LASER";
        armable = true;
        armed = true;
        jettisonable = false;
        allowedHardpoints = "15";
        baseRadarCrossSection = .75f;
    }
    
    private ModuleTurret turret;

    protected override void Awake()
    {
        base.Awake();
        turret = GetComponent<ModuleTurret>();
    }

    protected override void LateUpdate()
    {
        if (itemActivated)
            turret.AimToTarget(VRHead.instance.transform.position + VRHead.instance.transform.forward * laseDistance, true, true, false);
        base.LateUpdate();
    }
}