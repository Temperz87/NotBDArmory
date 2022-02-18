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
    private float charge = 350f;
    //private HeatGlow glow;
    //private AnimationToggle toggle;
    private bool lastDeployed = false;
    private HeatEmitter emitter;
    private ParticleSystem[] systems;

    public BoolEvent firedEvent = new BoolEvent();

    public HPEquipLaser()
    {
        fullName = "Tactical Laser System";
        shortName = "TLS";
        unitCost = 2000f;
        description = "Shoots a high powered laser capable of ripping through airplanes and ground units alike.";
        subLabel = "LASER";
        armable = true;
        armed = true;
        allowedHardpoints = "15";
        baseRadarCrossSection = .75f;
    }

    protected override void Awake()
    {
        base.Awake();
        if (fireTf == null)
        {
            fireTf = transform.Find("TLS").Find("Emitter");
            laserRenderer = transform.Find("Laser").GetComponent<LineRenderer>();
            laserRenderer.gameObject.SetActive(false);
            source = fireTf.GetComponent<AudioSource>();
        }

        //toggle = GetComponent<AnimationToggle>();

        //glow = GetComponent<HeatGlow>();
        //glow.headAdd = 4f;
        //glow.emissionColor = new Color32(191, 92, 0, 255);

        emitter = GetComponentInChildren<HeatEmitter>();

        systems = fireTf.GetComponentsInChildren<ParticleSystem>(false);
        systems.SetEmission(false);
    }

    public override void SetWeaponManager(WeaponManager wm)
    {
        base.SetWeaponManager(wm);
        AeroController.ControlSurfaceTransform atf = wm.gameObject.GetComponent<AeroController>().controlSurfaces[14];
        atf.transform = transform.Find("TLS").Find("AirBrake");
        atf.axis = new Vector3(1f, 0f, 0f);
        MaterialPropertyBlock block = new MaterialPropertyBlock();
        AircraftLiveryApplicator applicator = wm.gameObject.GetComponent<AircraftLiveryApplicator>(); // we take the materials here to get a livery as well, not done for the 45a cause it doesn't have a livery
        Material livery = applicator.materials[0];

        wm.transform.Find("aFighter2").Find("body").GetComponent<MeshRenderer>().GetPropertyBlock(block);


        Debug.Log("mat1");
        MeshRenderer tls = transform.Find("TLS").GetComponent<MeshRenderer>();
        tls.material = livery;
        tls.SetPropertyBlock(block);
        Debug.Log("mat2");
        MeshRenderer airbrake = transform.Find("TLS").Find("AirBrake").GetComponent<MeshRenderer>();
        airbrake.material = livery;
        airbrake.SetPropertyBlock(block);
        Debug.Log("mat3");
        MeshRenderer heatSink = transform.Find("TLS").Find("HeatSink").GetComponent<MeshRenderer>();
        heatSink.material = livery;
        heatSink.SetPropertyBlock(block);
    }

    public override void OnUnequip()
    {
        base.OnUnequip();
        AeroController.ControlSurfaceTransform atf = weaponManager.gameObject.GetComponent<AeroController>().controlSurfaces[14];
        atf.transform = weaponManager.gameObject.GetComponentInChildren<AirBrakeController>().transform.Find("airBrake");
        atf.axis = new Vector3(-1f, 0f, 0f);
    }

    private void LateUpdate()
    { 
        if (!fire)
        {
            if (charge < 350)
                charge = Mathf.Clamp(charge + (350 * Time.deltaTime)/4f, 0, 350); // recharges over 4 seconds
            //if (lastDeployed && glow.temperature == 0f)
            //{
            //    toggle.Toggle();
            //    lastDeployed = false;
            //}    
            return;
        }
        if (charge <= 0)
        {
            if (source.isPlaying)
            {
                fire = false;
                laserRenderer.gameObject.SetActive(false);
                source.Stop();
                firedEvent.Invoke(false);
                systems.SetEmission(false);
            }
            return;
        }
        charge = Mathf.Clamp(charge - (350 * Time.deltaTime)/4.5f, 0, 350);
        laserRenderer.SetPosition(0, fireTf.position);
        if (Physics.SphereCast(fireTf.position, .2f, fireTf.forward, out RaycastHit info, laseDistance, layerMask))
        {
            laserRenderer.SetPosition(1, fireTf.position + info.distance * fireTf.forward);
            Hitbox box = info.collider.GetComponent<Hitbox>();
            if (box != null && !(box.actor && box.actor == weaponManager.actor))
                box.Damage((30f * Time.deltaTime)/.12f, info.point, Health.DamageTypes.Impact, null, "Laser");
            else
                laserRenderer.SetPosition(1, GetAimPoint());
        }
        else
            laserRenderer.SetPosition(1, GetAimPoint());
        //glow.AddHeat();
        emitter.AddHeat(Time.deltaTime * .025f);
        if (!lastDeployed)
        {
            //toggle.Toggle();
            lastDeployed = true;
        }
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
        systems.SetEmission(true);
        LateUpdate(); // so the laser will be in the correct spot on fire
    }

    public override void OnStopFire()
    {
        base.OnStopFire();
        fire = false;
        laserRenderer.gameObject.SetActive(false);
        source.Stop();
        systems.SetEmission(false);
        firedEvent.Invoke(false);
    }

    public override int GetCount()
    {
        return (int)charge;
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
        return 0.049f;
    }
}