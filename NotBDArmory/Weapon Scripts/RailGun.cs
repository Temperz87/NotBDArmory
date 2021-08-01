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
    private float intensity = 4f;
    private MeshRenderer[] emisiveMaterials;
    private static Color32 emissionColor = new Color32(191, 191, 191, 255);
    ParticleSystem system;
    private string boop; //debug gui var
    private void Awake()
    {
        this.equip = gameObject.GetComponent<HPEquipGun>();
        if (equip == null)
            Debug.LogError("equip is null.");
        equip.OnEquipped += yoinkWM;
        this.animationController = GetComponentInChildren<Animator>();
        this.emisiveMaterials = GetComponentsInChildren<MeshRenderer>();
        foreach (MeshRenderer renderer in emisiveMaterials)
            renderer.material.EnableKeyword("_EMISSION");
        setRgEmission(Color.black);
        equip.gun.OnFired += delegate { StartCoroutine(rampUpIntensity()); };
        system = equip.gun.fireTransforms[0].gameObject.GetComponentInChildren<ParticleSystem>();
        equip.gun.OnFired += delegate { system.FireBurst(); }; // i did it properly baha :P
    }
    private void setRgEmission(Color color)
    {
        foreach (MeshRenderer renderer in emisiveMaterials)
            renderer.material.SetColor("_EmissionColor", color); ;
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
        if (Input.GetKeyDown(KeyCode.M))
            setRgEmission((Color)(emissionColor) * 4);
        if (Input.GetKeyDown(KeyCode.N))
            setRgEmission(new Color(0, 0, 0, 0));

    }
    private IEnumerator rampUpIntensity() // is this right?
    {
        float timeElapsed = 0;
        while (timeElapsed < 10)
        {
            intensity = Mathf.Lerp(0, 4, timeElapsed / 10);
            timeElapsed += Time.deltaTime;
            setRgEmission((Color)(emissionColor) * (deployed ? intensity : 0));
            yield return null;
        }
        intensity = 4;
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
        setRgEmission((Color)(emissionColor) * (deployed ? intensity : 0));
    }
    private void OnGUI()
    {
        return;
        GUI.Label(new Rect(36, 36, 1000f, 36), "Deployed: " + deployed.ToString());
        GUI.Label(new Rect(36, 72, 1000f, 36), (boop != null) ? boop : "null");
    }
}