using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Reflection;
using Harmony;

class HPEquipJetEngine : HPEquippable, IMassObject // This is a work in progress and waas intended as a fun side side project for me, might not be done for a bit
{
    private SolidBooster srb;
    public static SRBFiredEvent firedSRB = new SRBFiredEvent();
    private bool fired = false;

    public HPEquipJetEngine()
    {
        name = "Flight Assist Solid Rocket Booster";
        shortName = "SRB";
        unitCost = 1500f;
        description = "Why?";
        subLabel = "Solid Rocket Booster";
        //allowedHardpoints = "0,1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20,21,22,23,24,25,26,27,28,29,30";
        armable = true;
        armed = true;
        baseRadarCrossSection = 1.05f;
    }

    protected override void Awake()
    {
        base.Awake();
        srb = GetComponent<SolidBooster>();
        firedSRB.AddListener(delegate (WeaponManager wm)
       {
           if (wm == this.weaponManager)
           {
               if (!fired)
               {
                   this.srb.Fire();
                   fired = true;
               }
           }
       });
    }

    private void Update()
    {
        if (fired && srb.transform.parent == null)
            weaponManager.JettisonEq(hardpointIdx);
    }

    public override void Equip()
    {
        base.Equip();
        srb.SetParentRigidbody(weaponManager.vesselRB);
        srb.rb.mass += srb.boosterMass;
    }

    public override void OnStartFire()
    {
        base.OnStartFire();
        if (!fired)
        {
            srb.Fire();
            fired = true;
            firedSRB.Invoke(weaponManager);
        }
    }

    public override float GetEstimatedMass()
    {
        return GetMass();
    }

    public float GetMass()
    {
        return ((IMassObject)srb).GetMass();
    }

    public class SRBFiredEvent : UnityEvent<WeaponManager> { }
}