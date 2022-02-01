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
 * maddog esque where missiles will fire then seek out the nearest enemy, most anime best option so far most op love it 10/10 but too op imo AND HOW TRACK THIS DOESNT ANSWER THE QUESTION, AND NO GROUND RADAR :(
 * custom mfd page to select target (please oh god no please fuck fuck no please) 
 * IRST
 * idfk man
*/
class HPEquipAllDirectionalMissileLauncher : HPEquipMissileLauncher // This is a work in progress and waas intended as a fun side side project for me, might not be done for a bit
{
    VisualTargetFinder irst;
    private List<Actor> attackingTargets = new List<Actor>();
    private List<Actor> point = new List<Actor>();
    private const float fireCheck = .1f;
    private float firedTime = 0;
    private List<TrackIcon> icons = new List<TrackIcon>();
    private List<HUDRadarTrackIcon> seenIcons = new List<HUDRadarTrackIcon>();

    public HPEquipAllDirectionalMissileLauncher()
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
        irst = GetComponent<VisualTargetFinder>();
        irst.enabled = false;
        ml = GetComponent<MissileLauncher>();
        if (ml == null)
            Debug.LogError("ADMM has no missile launcher.");
    }

    protected override void OnEquip()
    {
        base.OnEquip();
        ml.LoadAllMissiles();
        irst.enabled = true;
        StartCoroutine(MakeIcons());
    }

    private IEnumerator MakeIcons() // next frame cuz reasons
    {
        yield return null;
        HUDRadarTrackIcon iconTemplate = Resources.FindObjectsOfTypeAll<HUDRadarLock>()[0].trackTemplate.GetComponent<HUDRadarTrackIcon>();
        yield return null;
        for (int i = 0; i < 12; i++)
        {
            Debug.Log("Making hud icon " + i);
            HUDRadarTrackIcon newIcon = Instantiate(iconTemplate.gameObject, iconTemplate.gameObject.transform).GetComponent<HUDRadarTrackIcon>();
            newIcon.SetTrack(Vector3.zero, false);
            newIcon.trackText.text = "ADMM LOCK";
            seenIcons.Add(newIcon);
        }
    }

    private void Update() 
    {
        foreach (HUDRadarTrackIcon icon in seenIcons)
            icon.SetTrack(Vector3.zero, false);
        for (int i = 0; i < seenIcons.Count && i < irst.targetsSeen.Count; i++)
        {
            Debug.Log("Setting icon.");
            seenIcons[i].SetTrack(irst.targetsSeen[i].position, true);
        }
    }

    public override void OnStartFire()
    {
        if (Time.time - firedTime < fireCheck)
            return;
        firedTime = Time.time;
        for (int i = 0; i < 12; i++)
        {
            Actor toShootAt = GetNextTarget();
            if (toShootAt != null)
            {
                if ((toShootAt.role & (Actor.Roles.Air | Actor.Roles.Missile)) != 0) // BIT WISEEEEEEEEE
                {
                    Debug.Log("Shooting at air target " + toShootAt.name);
                    ShootAtAirTarget(toShootAt);
                }
                else
                {
                    Debug.Log("Shooting at ground target " + toShootAt.name);
                    ShootAtGroundTarget(toShootAt);
                }
            }
            else
                break;
        }
    }

    private void ShootAtAirTarget(Actor target)
    {
        Missile next = ml.GetNextMissile();
        if (next == null)
        {
            Debug.LogError("Couldn't get next missile.");
            return;
        }
        next.guidanceMode = Missile.GuidanceModes.Heat;
        next.heatSeeker.seekerEnabled = true;
        next.heatSeeker.RemoteSetHardLock(target.transform.position);
        ml.FireMissile();
    }

    private void ShootAtGroundTarget(Actor target)
    {
        Missile next = ml.GetNextMissile();
        if (next == null)
        {
            Debug.LogError("Couldn't get next missile.");
            return;
        }
        next.guidanceMode = Missile.GuidanceModes.Optical;
        next.SetOpticalTarget(null, target);
        ml.FireMissile();
    }

    private Actor GetNextTarget()
    {
        attackingTargets.RemoveAll((Actor a) => { return a == null; });
        if (attackingTargets.Count < irst.targetsSeen.Count) // if we have already locked every target, we just reassign them
        {
            attackingTargets.Clear();
        }
        foreach (Actor actor in irst.targetsSeen)
        {
            if (!attackingTargets.Contains(actor))
            {
                attackingTargets.Add(actor);
                return actor;
            }
        }
        return null;
    }

    private struct TrackIcon
    {
        public HUDRadarTrackIcon icon;
        public Actor actor;

        public TrackIcon(Actor actor, HUDRadarTrackIcon icon)
        {
            this.actor = actor;
            this.icon = icon;
        }
    }
}
