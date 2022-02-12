using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Reflection;
using Harmony;

class HPEquipLaser : HPEquippable, IGDSCompatible, IMassObject
{
    private bool fire;
    private LineRenderer laserRenderer;
    private Transform fireTf;
    private AudioSource source;

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
            source = fireTf.GetComponent<AudioSource>();
        }
    }

    private void FixedUpdate()
    {
        if (!fire)
            return;
        laserRenderer.SetPosition(0, fireTf.position);
        if (Physics.Linecast(fireTf.position, GetAimPoint(), out RaycastHit info, 8))
        {
            laserRenderer.SetPosition(1, info.point);
            Hitbox box = info.collider.GetComponent<Hitbox>();
            if (box != null)
                box.Damage(140f, info.point, Health.DamageTypes.Impact, null, "Laser");
        }
        else
            laserRenderer.SetPosition(1, GetAimPoint());
    }

    public override void OnStartFire()      
    {
        base.OnStartFire();
        fire = true;
        laserRenderer.gameObject.SetActive(true);
        source.Play();
    }
    public override void OnStopFire()
    {
        base.OnStopFire();
        fire = false;
        laserRenderer.gameObject.SetActive(false);
        source.Stop();
    }

    public override int GetCount()
    {
        return 1;
    }

    public Transform GetFireTransform()
    {
        return fireTf;
    }

    public float GetMuzzleVelocity()
    {
        return Mathf.Infinity;
    }

    public override Vector3 GetAimPoint()
    {
        return fireTf.position + 100f * fireTf.forward;
    }

    public float GetMass()
    {
        return 0.025f;
    }
}