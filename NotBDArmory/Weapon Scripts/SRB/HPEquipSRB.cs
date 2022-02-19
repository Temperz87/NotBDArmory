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
    private int currentWeaponIdx;

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
        srb = GetComponentInChildren<SolidBooster>(true);
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
            }
        });
        GameObject srbGo = srb.gameObject;
        FlightSceneManager.instance.OnExitScene += delegate
        {
            Destroy(srbGo);
            Destroy(this.gameObject);
        };
    }

    private void Update()
    {
        if (srb == null)
        {
            srb = GetComponentInChildren<SolidBooster>(true);
            if (srb == null)
            {
                this.enabled = false;
                return;
            }
        }
        if (fired && srb.transform.parent == null)
            weaponManager.JettisonEq(hardpointIdx);
    }

    public override void Jettison()
    {
        base.Jettison();
        currentWeaponIdx = weaponManager.currentEquip.hardpointIdx;
    }

    protected override void OnJettison()
    {
        base.OnJettison();
        if (messageRoutine != null)
        {
            StopCoroutine(messageRoutine);
        }
        if (srb != null)
            srb.Detach();
        if (currentWeaponIdx != hardpointIdx && currentWeaponIdx != 0)
            weaponManager.SetWeapon(currentWeaponIdx);
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
        thisFired.Invoke();
        if (weaponManager.isPlayer)
            messageRoutine = StartCoroutine(hudMessagesRoutine());
    }

    private IEnumerator hudMessagesRoutine()
    {
        yield break; // kinda not sucky?
        HUDMessages messages = weaponManager.vm.hudMessages;
        if (weaponManager.vm == null || messages == null)
            yield break;
        float dt = 0f;
        while (dt < 30)
        {
            dt += Time.deltaTime;
            messages.SetMessage("SRB", $"SRB Burn For: {(int)(30f - dt)}");
            yield return new WaitForSeconds(1f);
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
            srb = GetComponentInChildren<SolidBooster>();
        return ((IMassObject)srb).GetMass();
    }

    public void SetParentRigidbody(Rigidbody rb)
    {
        srb.SetParentRigidbody(rb);
    }

    public override void OnQuicksaveEquip(ConfigNode eqNode)
    {
        base.OnQuicksaveEquip(eqNode);
        srb.OnQuicksavedMissile(eqNode, 0f); // the second param is never used
    }
    public override void OnQuickloadEquip(ConfigNode eqNode)
    {
        base.OnQuickloadEquip(eqNode);
        srb.OnQuickloadedMissile(eqNode, 0f);
    }

    public class SRBFiredEvent : UnityEvent<WeaponManager> { }
}