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

class SRBSync : VTNetSyncRPCOnly
{
    private HPEquipSRB srb;

    protected override void OnNetInitialized()
    {
        if (netEntity == null)
            Debug.LogError("SRBSync has no netEntity!");
        if (!VTOLMPUtils.IsMultiplayer())
        {
            enabled = false;
            return;
        }
        srb = base.GetComponent<HPEquipSRB>();
        srb.isLocal = netEntity.isMine;
        if (srb.isLocal)
            srb.thisFired.AddListener(new UnityAction(srbNetFired));
    }

    private void srbNetFired()
    {
        if (base.isMine)
            SendRPC("RPC_FiredSRB");
    }


    [VTRPC]
    public void RPC_FiredSRB()
    {
        if (!base.isMine) // yes, i am adding in checks for the unmp mod inside of a mod that no one will ever try to hack
            srb.OnStartFire();
    }
}
