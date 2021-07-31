using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Reflection;
using Harmony;

class HPEquipSwarmMissileLauncher : HPEquippable // This is a work in progress and waas intended as a fun side side project for me, might not be done for a bit
{
    private List<Vector3> lockedPositions;
    private List<HeatSeeker> allHeatSeekers;
    private List<lockData> missilesLocked;
    private IRMissileLauncher irml;
    private Dictionary<HeatSeeker, Traverse> seekerTraverses;
    private Dictionary<HeatSeeker, Missile> seekerToMissile;
    private Transform[] reticles;
    private Traverse irmltraverse;
    private bool raisehell;
    private HUDWeaponInfo hudReticle;
    private CollimatedHUDUI hud;
    private void Awake()
    {
        throw new NotImplementedException("Sorry, not done yet.");
        lockedPositions = new List<Vector3>(); // TODO : Add to the list
        allHeatSeekers = new List<HeatSeeker>();
        irml = base.GetComponent<IRMissileLauncher>();
        seekerTraverses = new Dictionary<HeatSeeker, Traverse>();
        seekerToMissile = new Dictionary<HeatSeeker, Missile>();
        missilesLocked = new List<lockData>();
        foreach (Missile missile in irml.missiles)
        {
            allHeatSeekers.Add(missile.heatSeeker);
            seekerTraverses.Add(missile.heatSeeker, Traverse.Create(missile.heatSeeker)); // I don't know how fast creating traverses are, but this should be fine and faster   
            seekerToMissile.Add(missile.heatSeeker, missile);
        }
        HPEquippable.EquipFunction equipFunction = new HPEquippable.EquipFunction();
        equipFunction.optionName = "Cycle Mode";
        equipFunction.optionReturnLabel = raisehell ? "Multi-Lock" : "Single-Lock";
        equipFunction.optionEvent = new EquipFunction.OptionEvent(CycleMode);
        irmltraverse = Traverse.Create(irml);
        reticles = new Transform[irml.missileCount];
        hudReticle = VTOLAPI.GetPlayersVehicleGameObject().GetComponentInChildren<HUDWeaponInfo>();
        hud = VTOLAPI.GetPlayersVehicleGameObject().GetComponentInChildren<CollimatedHUDUI>();
        Transform reticleBase = hudReticle.reticleTransforms[3];
        for (int i = 0; i < reticles.Length; i++)
        {
            GameObject newReticle = Instantiate(reticleBase.gameObject, reticleBase.parent);
            reticles[i] = newReticle.transform;
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
            for (int i = 0; i < irml.missileCount; i++)
            {
                Missile missile = irml.missiles[i];
                HeatSeeker seeker = missile.heatSeeker;
                Traverse thisTraverse;
                if (!seekerTraverses.TryGetValue(seeker, out thisTraverse))
                {
                    Debug.LogError("Couldn't get a seeker traverse.");
                    continue;
                }
                Vector3 seekingVector = seeker.targetPosition;
                if (AlreadyLocked(missile))
                    continue;
                if (EnsureNotConflictedLock(seekingVector))
                {
                    lockedPositions.Add(seekingVector);
                    missilesLocked.Add(new lockData(missile, seeker));
                }
                else
                {
                    HeatSeeker.SeekerModes lastMode = seeker.seekerMode;
                    seeker.SetSeekerMode(HeatSeeker.SeekerModes.Caged);
                    seeker.SetSeekerMode(lastMode); // this should unlcok it?
                }
            }
        }
        AssignIcons();
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
            aimPoints.Add();
        }
        for (int i = 0; i < aimPoints.Count; i++)
        {
            reticles[i].position = aimPoints[i] + VRHead.position * hud.depth;
            reticles[i].rotation = Quaternion.LookRotation(aimPoints[i], reticles[i].parent.up);
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

    public override void OnStartFire()
    {
        for (int i = 0; i < irml.missiles.Length; i++)
        {
            if (irml.missiles[i].hasTarget)
            {
                irmltraverse.Field("missileIdx").SetValue(i);
                irml.TryFireMissile();
            }
        }
    }
    public override void OnEnableWeapon()
    {
        base.OnEnableWeapon();
        irml.EnableWeapon(); // this will set 1 seeker to be active, we want all to be active
        foreach (HeatSeeker seeker in allHeatSeekers)
        {
            seeker.enabled = true;
            seeker.seekerEnabled = true;
            seeker.EnableAudio();
        }
        foreach (Transform reticle in reticles)
            reticle.gameObject.SetActive(true);
    }
    public override void OnDisableWeapon()
    {
        base.OnDisableWeapon();
        irml.DisableWeapon(); // this will set 1 seeker to be active, we want all to be active
        foreach (HeatSeeker seeker in allHeatSeekers) // TODO : Add to the list
        {
            seeker.enabled = false;
            seeker.seekerEnabled = false;
            seeker.DisableAudio();
        }
        foreach (Transform reticle in reticles)
            reticle.gameObject.SetActive(false);
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