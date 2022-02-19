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

	protected override void OnNetInitialized()
	{
		if (netEntity == null)
			Debug.LogError("Laserync has no netEntity!");
		laser = GetComponent<HPEquipLaser>();
		if (isMine)
			laser.firedEvent.AddListener(new UnityAction<bool>(firedLaser));
	}

	private void firedLaser(bool fired)
	{
		if (base.isMine)
		{
			SendRPC("RPC_LaserChanged", new object[] { fired? 1 : 0 });
		}
	}


	[VTRPC]
	public void RPC_LaserChanged(int fired)
	{
		bool shouldFire = fired == 1;
		if (!isMine) // yes, i am adding in checks for the unmp mod inside of a mod that no one will ever try to hack
			laser.Fire(shouldFire);	
	}
}
