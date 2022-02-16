using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Reflection;
using Harmony;

class HPEquipLaser : HPEquippable, IMassObject
{
    private bool fire;
    private LineRenderer laserRenderer;
    private Transform fireTf;
    private AudioSource source;
    private static int layerMask = LayerMask.GetMask("Hitbox");
    private const float laseDistance = 7500f;
    private int charge = 500;

    public BoolEvent firedEvent = new BoolEvent();

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
        {
            if (charge < 500)
                charge++;
            return;
        }
        if (charge <= 0)
            return;
        charge--;
        laserRenderer.SetPosition(0, fireTf.position);
        if (Physics.SphereCast(fireTf.position, 10f, fireTf.forward, out RaycastHit info, laseDistance, layerMask))
        {
            Debug.Log("Laser hit.");
            laserRenderer.SetPosition(1, fireTf.position + info.distance * fireTf.forward);
            Hitbox box = info.collider.GetComponent<Hitbox>();
            if (box != null)
                box.Damage(70f, info.point, Health.DamageTypes.Impact, null, "Laser");
        }
        else
            laserRenderer.SetPosition(1, GetAimPoint());
    }

    public void Fire(bool fire)
    {
        if (fire)
            OnStartFire();
        else
            OnStopFire();
    }

    public override void OnStartFire()      
    {
        base.OnStartFire();
        fire = true;
        laserRenderer.gameObject.SetActive(true);
        source.Play();
        firedEvent.Invoke(true);
    }
    public override void OnStopFire()
    {
        base.OnStopFire();
        fire = false;
        laserRenderer.gameObject.SetActive(false);
        source.Stop();
        firedEvent.Invoke(false);
    }

    public override int GetCount()
    {
        return charge;
    }

    public Transform GetFireTransform()
    {
        return fireTf;
    }

    public override Vector3 GetAimPoint()
    {
        return fireTf.position + laseDistance * fireTf.forward;
    }

    public float GetMass()
    {
        return 0.025f;
    }
}