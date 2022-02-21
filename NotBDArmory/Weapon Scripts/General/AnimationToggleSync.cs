using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Reflection;
using Harmony;
using VTNetworking;
using VTOLVR.Multiplayer;

class AnimationToggleSync : VTNetSyncRPCOnly
{
    private AnimationToggle toggle;
    private Traverse toggleTraverse;
    private bool lastDeployed;

    protected override void OnNetInitialized()
    {
        if (netEntity == null)
            Debug.LogError("AnimationToggleSync has no netEntity!");
        toggle = GetComponentInChildren<AnimationToggle>(true);
        if (toggle == null)
        {
            Debug.LogError("No animation toggle on " + gameObject.name);
            return;
        }
        toggleTraverse = Traverse.Create(toggle);
        lastDeployed = (bool)toggleTraverse.Field("deployed").GetValue();
    }

    private void Update()
    {
        if (!isMine)
            return;
        bool deployed = (bool)toggleTraverse.Field("deployed").GetValue();
        if (deployed != lastDeployed)
        {
            lastDeployed = deployed;
            SendRPC("Toggle", new object[] { lastDeployed ? 1 : 0 });
        }
    }

    [VTRPC]
    public void Toggle(int state)
    {
        bool deployed = state == 1;
        if (deployed != lastDeployed)
        {
            lastDeployed = deployed;
            toggle.Toggle();
        }
    }
}

