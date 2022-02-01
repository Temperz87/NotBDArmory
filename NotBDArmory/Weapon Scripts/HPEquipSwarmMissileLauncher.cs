using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Reflection;
using Harmony;

/* IDEAS FOR THIS/Brainstorming
 * Have A master optical targeter for each side (maybe 6?) that can assign missile locks, doable
 * Current system of each missile unlocks if they've already locked a target another missile has, downsides it's fucking shit and gives me a headahce
 * steal C's radar code, upsides I DONT GOTTA DO SHIT WOOOOOOOO
 * use a optical/visual/heat finder to paint targets then tell the missiles to track to them via a super radar or something when fire ml
 * headtrack paint, upsides pretty pog downsides I HAVE TO TURN MY HEAD AAAAAAAAAAAAA
 * tgp paint (ew please no)
 * maddog esque where missiles will fire then seek out the nearest enemy, most anime best option so far most op love it 10/10 but too op imo AND HOW TRACK THIS DOESNT ANSWER THE QUESTION
 * custom mfd page to select target (please oh god no please fuck fuck no please) 
 *
 * idfk man
*/
class HPEquipSwarmMissileLauncher : HPEquippable // This is a work in progress and waas intended as a fun side side project for me, might not be done for a bit
{
    public IRMissileLauncher irml;

    private List<Vector3> lockedPositions;
    private List<lockData> missilesLocked;
    private List<Transform> reticles;

    private Traverse rmltraverse;
    private HUDWeaponInfo hudReticle;
    private CollimatedHUDUI hud;
    private bool fire = false;
    RadarLockData rdata;
    private bool raisehell;

    public HPEquipSwarmMissileLauncher()
    {
        this.fullName = "All-Directional-Multi-Missile-Launcher";
        this.shortName = "ADMM";
        this.subLabel = "I DONT KNOW WHAT FIRE MOD THIS WILL HAVE SWARM";
        this.allowedHardpoints = "15";
        this.description = "A weapon to allow you to become death...";
        this.armable = true;
        this.armed = true;
    }

    protected override void Awake()
    {
        base.Awake();
        lockedPositions = new List<Vector3>();
        irml = base.GetComponent<IRMissileLauncher>();
        missilesLocked = new List<lockData>();
        HPEquippable.EquipFunction equipFunction = new HPEquippable.EquipFunction();
        equipFunction.optionName = "Cycle Mode";
        equipFunction.optionReturnLabel = raisehell ? "Multi-Lock" : "Single-Lock";
        equipFunction.optionEvent = new EquipFunction.OptionEvent(CycleMode);
        rmltraverse = Traverse.Create(irml);
        Debug.Log("Awake hell incarnate jesus christ this thing is powerful...");
    }       
    private void MakeReticles()
    {
        if (reticles != null)
        {
            Transform[] destroyReticles = reticles.ToArray();
            for (int i = 0; i < reticles.Count; i++)
            {
                Destroy(destroyReticles[i]);
            }
        }
        reticles = new List<Transform>();
        hudReticle = VTOLAPI.GetPlayersVehicleGameObject().GetComponentInChildren<HUDWeaponInfo>();
        hud = VTOLAPI.GetPlayersVehicleGameObject().GetComponentInChildren<CollimatedHUDUI>();
        Transform reticleBase = hudReticle.reticleTransforms[3];
        for (int i = 0; i < irml.missileCount; i++)
        {
            GameObject newReticle = Instantiate(reticleBase.gameObject, reticleBase.parent);
            reticles.Add(newReticle.transform);
            newReticle.SetActive(itemActivated);
            newReticle.SetActive(false);
        }
    }
    private string CycleMode()
    {
        raisehell = !raisehell;
        return raisehell ? "Multi-Lock" : "Single-Lock"; ;
    }
    private void Update()
    {
        if (raisehell)
            Debug.Log("Not implemented :P");
        //AssignUnfiredMissilesToOneLock();
        else
        {
            if (!fire)
                return;

            for (int i = 0; i < irml.missileCount; i++)
            {
                Missile missile = irml.missiles[i];
                HeatSeeker seeker = missile.heatSeeker;
                Vector3 seekingVector = seeker.targetPosition;
                if (AlreadyLocked(missile))
                    continue;
                if (missile.hasTarget && EnsureNotConflictedLock(seekingVector))
                {
                    lockedPositions.Add(seekingVector);
                    missilesLocked.Add(new lockData(missile, seeker));
                }
                else if (missile.hasTarget)
                {
                    HeatSeeker.SeekerModes lastMode = seeker.seekerMode;
                    seeker.SetSeekerMode(HeatSeeker.SeekerModes.Uncaged);
                    seeker.SetSeekerMode(lastMode); // this should unlock it?
                }
            }
            AssignIcons();
        }
    }


    private void AssignIcons()
    {
        List<Vector3> aimPoints = new List<Vector3>();
        foreach (lockData data in missilesLocked)
        {
            Missile missile = data.ms;
            Transform tf = missile.heatSeeker.transform;
            Vector3 aimPoint = tf.position + transform.forward * 4000f;
            aimPoint = (aimPoint - VRHead.position).normalized;
            Debug.Log("Created aimpoint.");
            aimPoints.Add(aimPoint);
        }
        foreach (Transform reticle in reticles)
            reticle.gameObject.SetActive(false);
        for (int i = 0; i < irml.missileCount; i++)
        {
            if (irml.missiles[i].hasTarget)
                rmltraverse.Field("missileIdx").SetValue(i);
            aimPoints.Add((irml.GetAimPoint() - VRHead.position).normalized);
        }
        for (int i = 0; i < aimPoints.Count; i++)
        {
            reticles[i].position = VRHead.position + aimPoints[i] * hud.depth;
            reticles[i].rotation = Quaternion.LookRotation(aimPoints[i], reticles[i].parent.up);
            reticles[i].gameObject.SetActive(true);
        }
    }

    private bool AlreadyLocked(Missile missile) // this function is ass
    {
        foreach (lockData data in missilesLocked)
            if (data.ms == missile)
                return true;
        return false;
    }

    private bool EnsureNotConflictedLock(Vector3 checkAgainst)
    {
        if (checkAgainst == Vector3.zero)
            return false;
        foreach (Vector3 heatLock in lockedPositions)
        {
            if (heatLock == checkAgainst)
                continue;
            if (Math.Abs(checkAgainst.x - heatLock.x) < 1 && Math.Abs(checkAgainst.y - heatLock.y) < 1 && Math.Abs(checkAgainst.z - heatLock.z) < 1)
            {
                return false;
            }
        }
        return true;
    }

    public override int GetCount()
    {
        return irml.GetAmmoCount();
    }
    public override int GetMaxCount() => 333; // this works???

    public override void OnStartFire()
    {

        this.fire = true;
        /*for (int i = 0; i < irml.missiles.Length; i++)
        {
            if (irml.missiles[i].hasTarget)
            {
                rmltraverse.Field("missileIdx").SetValue(i);
                if (irml.TryFireMissile())
                    foreach (lockData data in missilesLocked)
                        if (data.ms == irml.missiles[i])
                        {
                            missilesLocked.Remove(data);
                            allHeatSeekers.Remove(data.seeker);
                            MakeReticles(); // we have lost a missile so we reassign the transforms
                            break;
                        }
            }
        }*/
    }
    public override void OnStopFire()
    {
        base.OnStopFire();
        this.fire = false;
    }
    public override void OnEnableWeapon()
    {
        base.OnEnableWeapon();
        this.enabled = true;
        //rml.EnableWeapon(); // this will set 1 seeker to be active, we want all to be active
        /*foreach (HeatSeeker seeker in allHeatSeekers)
        {
            seeker.enabled = true;
            seeker.seekerEnabled = true;
            seeker.gimbalFOV = 15f; // we shal track em no matter where they are
            seeker.seekerFOV = 15f;
            seeker.SetSeekerMode(HeatSeeker.SeekerModes.HeadTrack);
            seeker.EnableAudio();
        }
        if (reticles == null)
            MakeReticles();
        foreach (Transform reticle in reticles)
            reticle?.gameObject.SetActive(true);*/
    }
    public override void OnDisableWeapon()
    {
        base.OnDisableWeapon();
        /*irml.DisableWeapon(); // this will set 1 seeker to be active, we want all to be active
        foreach (HeatSeeker seeker in allHeatSeekers) // TODO : Add to the list
        {
            seeker.enabled = false;
            seeker.seekerEnabled = false;
            seeker.DisableAudio();
        }
        foreach (Transform reticle in reticles)
            reticle?.gameObject.SetActive(false);*/
    }

    private struct lockData
    {
        public Missile ms;
        public HeatSeeker seeker;
        public lockData(Missile missile, HeatSeeker seeker)
        {
            this.ms = missile;
            this.seeker = seeker;
        }
    }
}