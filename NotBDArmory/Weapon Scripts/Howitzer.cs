using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Reflection;
using Harmony;

class Howitzer : MonoBehaviour
{
    private HPEquipGun gun;
    private AnimationToggle toggle;
    private Transform ejectTf;

    private void Awake()
    {
        gun = GetComponent<HPEquipGun>();
        toggle = GetComponent<AnimationToggle>();
        ejectTf = transform.Find("Howitzer").Find("Azimuth").Find("eject tf");
        gun.gun.OnFiredBullet += delegate
        {
            StartCoroutine(ejectBullet());
        };
    }

    private IEnumerator ejectBullet()
    {
        toggle.Toggle();
        yield return new WaitForSeconds(.5f);
        EjectedShell.Eject(ejectTf.position, ejectTf.rotation, 10f, 3f, gun.weaponManager.actor.velocity);
        yield return new WaitForSeconds(.5f);
        toggle.Toggle();
    }
}