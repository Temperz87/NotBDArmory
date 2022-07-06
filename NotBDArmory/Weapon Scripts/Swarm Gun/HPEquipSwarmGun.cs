using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Reflection;
using Harmony;
using VTOLVR.Multiplayer;

class HPEquipSwarmGun : HPEquippable, IMassObject
{
    private Gun[] guns;
    private ModuleTurret[] turrets;
    private VisualTargetFinder irst;

    public HPEquipSwarmGun()
    {
        fullName = "Swarm Gun";
        shortName = "SWG";
        unitCost = 1500f;
        description = "WIP";
        subLabel = "SWARM";
        armable = true;
        armed = true;
        jettisonable = false;
        allowedHardpoints = "0";
        baseRadarCrossSection = .75f;
    }

    protected override void Awake()
    {
        base.Awake();
        guns = GetComponentsInChildren<Gun>(true);
        turrets = new ModuleTurret[guns.Length];
        for (int i = 0; i < turrets.Length; i++)
            turrets[i] = guns[i].GetComponent<ModuleTurret>();
    }

    private void Update()
    {
        if (!itemActivated)
            return;

    }

    protected override void OnEquip()
    {
        base.OnEquip();
        irst = GetComponent<VisualTargetFinder>();
    }

    public float GetMass()
    {
        float mass = 0f;
        foreach (Gun gun in guns)
            mass += gun.GetMass();
        return mass;
    }
}