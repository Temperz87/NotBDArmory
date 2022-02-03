using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Reflection;
using Harmony;

class HPEquipSRB : HPEquippable, IMassObject, IParentRBDependent
{
    public static SRBFiredEvent firedSRB = new SRBFiredEvent();
    public UnityEvent thisFired = new UnityEvent();
    public SolidBooster srb;
    public bool isLocal;
    private bool fired = false;
    private Coroutine messageRoutine;

    public HPEquipSRB()
    {
        name = "Flight Assist Solid Rocket Booster";
        shortName = "SRB";
        unitCost = 1500f;
        description = "A one-time use booster that's used to increase the total thrust of your aircraft once ignited. Once it's activated, it cannot be stopped.";
        subLabel = "Solid Rocket Booster";
        //allowedHardpoints = "0,1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20,21,22,23,24,25,26,27,28,29,30";
        armable = true;
        armed = true;
        jettisonable = true;
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
                else
                    weaponManager.JettisonEq(hardpointIdx);
                thisFired.Invoke();
            }
        });
    }

    private void Update()
    {
        if (fired && srb.transform.parent == null)
            weaponManager.JettisonEq(hardpointIdx);
    }

    protected override void OnJettison()
    {
        base.OnJettison();
        if (messageRoutine != null)
        {
            StopCoroutine(messageRoutine);
            //weaponManager.vm.hudMessages.RemoveMessage("SRB");
        }
        if (srb != null)
        {
            Vector3 savedVelocity = srb.rb.velocity;
            srb.transform.parent = null;
            if (srb.rb == null)
                srb.rb = srb.gameObject.AddComponent<Rigidbody>();
            srb.rb.interpolation = RigidbodyInterpolation.Interpolate;
            srb.rb.mass = srb.boosterMass;
            srb.rb.velocity = savedVelocity;
            srb.gameObject.AddComponent<FloatingOriginTransform>().SetRigidbody(srb.rb);
        }
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
        firedSRB.Invoke(weaponManager);
        StartCoroutine(hudMessagesRoutine());
    }

    private IEnumerator hudMessagesRoutine()
    {
        yield break; // this sucks :(
        HUDMessages messages = weaponManager.vm.hudMessages;
        if (weaponManager.vm == null || messages == null)
            yield break;
        float time = Time.time;
        while (Time.time - time < srb.burnTime)
        {
            messages.SetMessage("SRB", $"SRB Jettison in: {(int)(srb.burnTime - (Time.time - time))}");
            yield return new WaitForSeconds(1f);
            time--;
        }
    }

    public override int GetCount()
    {
        return 1;
    }
    public override float GetEstimatedMass()
    {
        return GetMass();
    }

    public float GetMass()
    {
        if (srb == null)
            srb = GetComponent<SolidBooster>();
        return ((IMassObject)srb).GetMass();
    }

    public void SetParentRigidbody(Rigidbody rb)
    {
        srb.SetParentRigidbody(rb);
    }

    public override void OnQuicksaveEquip(ConfigNode eqNode)
    {
        base.OnQuicksaveEquip(eqNode);
        srb.OnQuicksavedMissile(eqNode, 0f);
    }
    public override void OnQuickloadEquip(ConfigNode eqNode)
    {
        base.OnQuickloadEquip(eqNode);
        srb.OnQuickloadedMissile(eqNode, 0f);
    }

    public class SRBFiredEvent : UnityEvent<WeaponManager> { }
}