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
        Debug.Log("Awoke animation toggle sync.");
        if (netEntity == null)
            Debug.LogError("AnimationToggleSync has no netEntity!");
        toggle = GetComponent<AnimationToggle>();
        toggleTraverse = Traverse.Create(toggle);
        lastDeployed = (bool)toggleTraverse.Field("deployed").GetValue();

        if (!isMine)
        {
            HeatGlow toOverride = GetComponent<HeatGlow>();
            if (toOverride)
                toOverride.overriden = true;
        }
    }

    private void Update()
    {
        if (!isMine)
            return;
        bool deployed = (bool)toggleTraverse.Field("deployed").GetValue();
        if (deployed != lastDeployed)
        {
            lastDeployed = deployed;
            Debug.Log("Sending Toggle RPC.");
            SendRPC("Toggle", new object[] { lastDeployed ? 1 : 0 });
        }
    }

    [VTRPC]
    public void Toggle(int state)
    {
        Debug.Log("Received Toggle RPC.");
        bool deployed = state == 1;
        if (deployed != lastDeployed)
        {
            lastDeployed = deployed;
            toggle.Toggle();
        }
    }
}

