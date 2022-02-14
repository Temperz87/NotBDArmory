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

class LaserSync : VTNetSyncRPCOnly
{
	private HPEquipLaser laser;
	protected override void Awake()
	{
		base.Awake();
		if (netEntity == null)
			Debug.LogError("Laserync has no netEntity!");
		if (!VTOLMPUtils.IsMultiplayer())
		{
			enabled = false;
			return;
		}
		laser = GetComponent<HPEquipLaser>();
		if (isMine)
			laser.firedEvent.AddListener(new UnityAction<bool>(firedLaser));
	}

	private void firedLaser(bool fired)
	{
		if (base.isMine)
		{
			Debug.Log("Sending RPC Laser fired.");
			SendRPC("RPC_LaserChanged", new object[] { fired? 1 : 0 });
		}
		else
			Debug.Log("This isn't my Laser, so I'm not net firing it.");
	}


	[VTRPC]
	public void RPC_LaserChanged(int fired)
	{
		bool shouldFire = fired == 1;
		Debug.Log("Recieved Laser changed.");
		if (!base.isMine) // yes, i am adding in checks for the unmp mod inside of a mod that no one will ever try to hack
		{
			laser.Fire(shouldFire);	
		}
		else
			Debug.Log("Not firing, as this is mine.");
	}

	protected override void OnNetInitialized()
	{
		base.OnNetInitialized();
	}
}
