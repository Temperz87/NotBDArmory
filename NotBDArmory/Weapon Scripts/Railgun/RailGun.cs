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
        this.emisiveMaterials = GetComponentsInChildren<MeshRenderer>();
        foreach (MeshRenderer renderer in emisiveMaterials)
            renderer.material.EnableKeyword("_EMISSION");
        setRgEmission(Color.black);
        equip.gun.OnFired += delegate { StartCoroutine(rampDownIntensity()); };
        gameObject.AddComponent<AnimateOnEquip>();
    }

    private void setRgEmission(Color color)
    {
        foreach (MeshRenderer renderer in emisiveMaterials)
            renderer.material.SetColor("_EmissionColor", color);
    }

    private IEnumerator rampDownIntensity()
    {
        float intensity;
        float timeElapsed = 0;
        while ((int)timeElapsed < 6)
        {
            if (timeElapsed < .5)
                intensity = Mathf.SmoothStep(0, 4, timeElapsed / .35f);
            else
                intensity = Mathf.SmoothStep(4, 0, timeElapsed / 5.65f);
            timeElapsed += Time.deltaTime;
            setRgEmission((Color)(emissionColor) * intensity);
            if (wm.vm && wm.vm.hudMessages)
                wm.vm.hudMessages.SetMessage("RG", $"RG COOL: {Math.Round(6 - timeElapsed, 2)}");
            yield return null;
        }
        wm.vm.hudMessages.RemoveMessage("RG");
        setRgEmission((Color)(emissionColor) * (0));
    }

    private void yoinkWM()
    {
        this.wm = equip.weaponManager;
        if (wm == null)
        {
            Debug.LogError("null wm on 45 rail gun");
            return;
        }
        if (wm.OnWeaponChanged == null)
        {
            wm.OnWeaponChanged = new UnityEvent();
        }
    }
}