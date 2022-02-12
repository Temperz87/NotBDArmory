using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Reflection;
using Harmony;

class AnimateOnEquip : MonoBehaviour
{
    private AnimationToggle toggle;
    private Traverse toggleTraverse;
    private HPEquipGun equip;
    private WeaponManager wm;

    private void Awake()
    {
        this.equip = gameObject.GetComponent<HPEquipGun>();
        if (equip == null)
            Debug.LogError("equip is null on object " + gameObject.name);
        equip.OnEquipped += yoinkWM;
        this.toggle = GetComponent<AnimationToggle>();
        this.toggleTraverse = Traverse.Create(toggle);
    }

    private void Update()
    {
        bool deployed = (bool)toggleTraverse.Field("deployed").GetValue();
        if (equip.itemActivated != deployed)
            toggle.Toggle();
    }

    private void yoinkWM()
    {
        this.wm = equip.weaponManager;
        if (wm == null)
        {
            Debug.LogError("null wm on " + gameObject.name);
            return;
        }
        if (wm.OnWeaponChanged == null)
        {
            Debug.Log("on weapon changed is null, remaking it now.");
            wm.OnWeaponChanged = new UnityEvent();
        }
    }
}