using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Reflection;
using Harmony;

class HPEquipJetEngine : HPEquippable // This is a work in progress and waas intended as a fun side side project for me, might not be done for a bit
{
    private SolidBooster srb;
    
    protected override void Awake()
    {
        srb = GetComponent<SolidBooster>();
    }
    public override void Equip()
    {
        base.Equip();
        srb.rb = weaponManager.vesselRB;
    }

    public void ActiveSRB()
    {
        srb.Fire();
    }
}