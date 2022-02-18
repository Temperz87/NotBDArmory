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
        {
            Debug.Log("Sending RPC srb fired.");
            SendRPC("RPC_FiredSRB");
        }
        else
            Debug.Log("This isn't my SRB, so I'm not net firing it.");
    }


    [VTRPC]
    public void RPC_FiredSRB()
    {
        Debug.Log("Recieved SRB fired.");
        if (!base.isMine) // yes, i am adding in checks for the unmp mod inside of a mod that no one will ever try to hack
        {
            Debug.Log("Firing!");
            srb.OnStartFire();
        }
        else
            Debug.Log("Not firing, as this is mine.");
    }
}
