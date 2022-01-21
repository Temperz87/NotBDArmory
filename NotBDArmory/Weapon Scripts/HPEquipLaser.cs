using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Reflection;
using Harmony;

class HPEquipLaser : HPEquippable, IGDSCompatible // This is a work in progress and waas intended as a fun side side project for me, might not be done for a bit
{
    public HPEquipLaser()
    {
        name = "TacticalLaserSystem";
        shortName = "TLS";
        unitCost = 2000f;
        description = "pew pew";
        subLabel = "LASER";
        armable = true;
        armed = true;
        allowedHardpoints = "15";
        baseRadarCrossSection = .75f;
    }

    protected override void Awake()
    {
        if (fireTf == null)
        {
            fireTf = transform.Find("TacticalLaserSystem").Find("Glass");
            laserRenderer = transform.Find("Laser").GetComponent<LineRenderer>();
            laserRenderer.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (!fire)
            return;
        Debug.Log("Laser updated.");
        Ray toCast = new Ray();
        toCast.origin = fireTf.position;
        toCast.direction = fireTf.forward;
        
        Physics.Raycast(toCast, out RaycastHit info, 100, 10);
        laserRenderer.SetPosition(0, fireTf.position);
        laserRenderer.SetPosition(1, info.point);
        if (info.collider != null)
        {
            Hitbox box = info.collider.GetComponent<Hitbox>();
            if (box != null)
                box.Damage(100, info.point, Health.DamageTypes.Impact, null, "Laser");
        }
    }

    public override void OnStartFire()
    {
        base.OnStartFire();
        fire = true;
        laserRenderer.gameObject.SetActive(true);
    }
    public override void OnStopFire()
    {
        base.OnStopFire();
        fire = false;
        laserRenderer.gameObject.SetActive(false);
    }

    public Transform GetFireTransform()
    {
        return fireTf;
    }

    public float GetMuzzleVelocity()
    {
        return 999999;
    }

    public override Vector3 GetAimPoint()
    {
        return fireTf.position + 100f * fireTf.forward;
    }

    private bool fire;
    private LineRenderer laserRenderer;
    private Transform fireTf;
}