using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Reflection;
using Harmony;
class HPEquipHeadtrackOML : HPEquipOpticalML // WIP
{
    protected override void Awake()
    {
        base.Awake();
        OpticalTargeter targeter = GetComponentInChildren<OpticalTargeter>();
        oml.SetTargeter(GetComponentInChildren<OpticalTargeter>());
    }
}
