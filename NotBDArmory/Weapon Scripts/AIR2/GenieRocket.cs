using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Reflection;
using Harmony;

class GenieRocket : MonoBehaviour
{
    private Rocket rocket;
    private ParticleSystem cloud;
    private Traverse rocketTraverse;
    private bool hasFired = false;
    private Actor myActor;
    private RotationToggle toggle;

    private void Awake()
    {
        rocket = GetComponent<Rocket>();
        rocketTraverse = Traverse.Create(rocket);
        toggle = GetComponent<RotationToggle>();
        cloud = transform.Find("Mach Diamond").GetComponent<ParticleSystem>();
    }

    private void Update()
    {
        if (!hasFired && !(bool)rocketTraverse.Field("fired").GetValue() )
            return;
        if (!hasFired)
        {
            hasFired = true;
            myActor = rocketTraverse.Field("sourceActor").GetValue() as Actor;
            toggle.Toggle();
            transform.Find("AIR2").Find("Rope").gameObject.SetActive(false);
        }
        float mach = MeasurementManager.SpeedToMach(rocket.GetVelocity().magnitude, WaterPhysics.GetAltitude(transform.position));
        cloud.SetEmissionRate(Mathf.Lerp(0f, 150f, Mathf.Clamp01(2f * (mach - .8f))));
        //Debug.Log("Mach on genie is now " + mach + " and emission rate is " + Mathf.Lerp(0f, 150f, Mathf.Clamp01(2f * (mach - .8f))));
        List<Actor> detectedActors = new List<Actor>();
        Actor.GetActorsInRadius(transform.position, rocket.radius/10f, myActor.team, TeamOptions.OtherTeam, detectedActors);
        foreach (Actor a in detectedActors)
            if (a.role == Actor.Roles.Air)
            {
                //Debug.Log("Proxy detonate rocket.");
                rocket.Detonate();
            }
    }
}
