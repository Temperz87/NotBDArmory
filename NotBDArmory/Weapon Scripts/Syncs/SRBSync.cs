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
	protected override void Awake()
	{
		base.Awake();
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
		SendRPC("RPC_FiredSRB");
	}


	[VTRPC]
	public void RPC_FiredSRB()
	{
		srb.OnStartFire();
	}

	protected override void OnNetInitialized()
	{
		base.OnNetInitialized();
	}
}
