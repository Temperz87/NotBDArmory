using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Reflection;
using Harmony;

class RailGun : MonoBehaviour
{
    private Animator animationController;
    private HPEquipGun equip;
    private WeaponManager wm;
    private bool deployed = false;
    private string boop;
    private void Awake()
    {
        this.equip = gameObject.GetComponent<HPEquipGun>();
        if (equip == null)
            Debug.LogError("equip is null.");
        equip.OnEquipped += yoinkWM;
        this.animationController = GetComponentInChildren<Animator>();
    }
    private void Update()
    {
        if (equip.itemActivated != deployed)
            toggleAnimation();
        if (Input.GetKeyDown(KeyCode.L))
            toggleAnimation();
        if (Input.GetKeyDown(KeyCode.G))
        {
            animationController.SetFloat("direction", 1);
            animationController.SetTrigger("animate!");
        }
        if (Input.GetKeyDown(KeyCode.H))
        {
            animationController.SetFloat("direction", -1);
            animationController.SetTrigger("animate!");
        }
    }
    private void yoinkWM()
    {
        Debug.Log("try yoink wm");
        this.wm = equip.weaponManager;
        if (wm == null)
        {
            Debug.LogError("null wm on 45 rail gun");
            return;
        }
        else
            Debug.Log("Got wm for 45 rail gun.");
        if (wm.OnWeaponChanged == null)
        {
            Debug.Log("on weapon changed is null, remaking it now.");
            wm.OnWeaponChanged = new UnityEvent(); 
        }
        equip.triggerOpenBay = false;
    }
    private void toggleAnimation()
    {
        animationController.SetFloat("direction", deployed ? -1 : 1);
        animationController.SetTrigger("animate!");
        deployed = !deployed;
    }
    private void OnGUI()
    {
        return;
        GUI.Label(new Rect(36, 36, 1000f, 36), "Deployed: " + deployed.ToString());
        GUI.Label(new Rect(36, 72, 1000f, 36), (boop != null)? boop : "null");
    }
}