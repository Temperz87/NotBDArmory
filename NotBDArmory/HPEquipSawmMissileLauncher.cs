using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Reflection;
using Harmony;

class HPEquipSawmMissileLauncher : HPEquippable // This is a work in progress and waas intended as a fun side side project for me, might not be done for a bit
{
    List<Actor> lockedActors;
    List<HeatSeeker> allHeatSeekers;
    private void Awake()
    {
        lockedActors = new List<Actor>();
        allHeatSeekers = new List<HeatSeeker>();
    }
}