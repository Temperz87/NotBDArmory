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
    private AnimationToggle toggle;
    private Traverse toggleTraverse;
    private HPEquipGun equip;
    private WeaponManager wm;
    private MeshRenderer[] emisiveMaterials;
    private static Color32 emissionColor = new Color32(191, 191, 191, 255);

    private void Awake()
    {
        this.equip = gameObject.GetComponent<HPEquipGun>();
        if (equip == null)
            Debug.LogError("equip is null.");
        equip.OnEquipped += yoinkWM;
        this.toggle = GetComponent<AnimationToggle>();
        this.toggleTraverse = Traverse.Create(toggle);
        this.emisiveMaterials = GetComponentsInChildren<MeshRenderer>();
        foreach (MeshRenderer renderer in emisiveMaterials)
            renderer.material.EnableKeyword("_EMISSION");
        setRgEmission(Color.black);
        equip.gun.OnFired += delegate { StartCoroutine(rampDownIntensity()); };
        //equip.subLabel = ("RDY");
    }

    private void setRgEmission(Color color)
    {
        foreach (MeshRenderer renderer in emisiveMaterials)
            renderer.material.SetColor("_EmissionColor", color); ;
    }

    private void Update()
    {
        toggleAnimation();
        if (Input.GetKeyDown(KeyCode.M))
            setRgEmission((Color)(emissionColor) * 4);
        if (Input.GetKeyDown(KeyCode.N))
            setRgEmission(new Color(0, 0, 0, 0));
    }

    private IEnumerator rampDownIntensity()
    {
        float intensity;
        float timeElapsed = 0;
        while ((int)timeElapsed < 10)
        {
            intensity = Mathf.SmoothStep(4, 0, timeElapsed / 10);
            timeElapsed += Time.deltaTime;
            setRgEmission((Color)(emissionColor) * intensity);
            if (wm.vm.hudMessages)
            {
                wm.vm.hudMessages.SetMessage("RG", $"RG COOL: {Math.Round(10 - timeElapsed, 2)}");
                //equip.subLabel = ($"COOL: {Math.Round(10 - timeElapsed, 2)}");
                //wm.ui.hudInfo.subLabelText.text = equip.subLabel;
            }
            yield return null;
        }
        //equip.subLabel = ("RDY");
        wm.vm.hudMessages.RemoveMessage("RG");
        setRgEmission((Color)(emissionColor) * (0));
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
    }

    private void toggleAnimation()
    {
        bool deployed = (bool)toggleTraverse.Field("deployed").GetValue();
        if (equip.itemActivated != deployed)
        {
            toggle.Toggle();
        }
    }
}