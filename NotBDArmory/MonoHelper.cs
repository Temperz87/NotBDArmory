using Harmony;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MonoHelper : MonoBehaviour
{
    private static ExternalOptionalHardpoints extHardpoint;
    private void Awake()
    {
        if (extHardpoint == null)
        {
            GameObject playerObject = VTOLAPI.GetPlayersVehicleGameObject();
            if (playerObject)
                extHardpoint = playerObject.GetComponent<ExternalOptionalHardpoints>();
        }
    }
    public void StartConfigRoutine(int hpIdx, string weaponName, LoadoutConfigurator __instance)
    {
        try
        {
            StartCoroutine(attachRoutine(hpIdx, weaponName, __instance));
        }
        catch (NullReferenceException e)
        {
            if (weaponName != null && __instance != null)
                Debug.LogError("TEMPERZ YOU PARENTED IT TO THE WRONG THING AGAIN YOU NIMWIT");
            throw e;
        }
    }

    public void AttachImmediate(int hpIdx, string weaponName, LoadoutConfigurator __instance)
    {
        __instance.DetachImmediate(hpIdx);
        if (__instance.uiOnly)
        {
            __instance.equips[hpIdx] = Armory.TryGetWeapon(weaponName).equip;
            __instance.equips[hpIdx].OnConfigAttach(__instance);
        }
        else
        {
            GameObject instantiated = Instantiate(Armory.TryGetWeapon(weaponName).weaponObject);
            instantiated.name = weaponName;
            instantiated.SetActive(true);
            Transform transform = instantiated.transform;
            __instance.equips[hpIdx] = transform.GetComponent<HPEquippable>();
            Traverse LCPTraverse = Traverse.Create(__instance);
            Transform[] allTfs = (Transform[])(LCPTraverse.Field("hpTransforms").GetValue());
            Transform hpTf = allTfs[hpIdx];
            transform.SetParent(hpTf);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;
            __instance.equips[hpIdx].OnConfigAttach(__instance);
            if (extHardpoint != null)
                extHardpoint.Wm_OnWeaponEquippedHPIdx(hpIdx);
        }
        __instance.UpdateNodes();
    }

    private static IEnumerator attachRoutine(int hpIdx, string weaponName, LoadoutConfigurator __instance)
    {
        Debug.Log("Starting attach routine.");
        Traverse LCPTraverse = Traverse.Create(__instance);
        Coroutine[] detachRoutines = (Coroutine[])LCPTraverse.Field("detachRoutines").GetValue();
        if (detachRoutines[hpIdx] != null)
        {
            yield return detachRoutines[hpIdx];
            detachRoutines = (Coroutine[])LCPTraverse.Field("detachRoutines").GetValue();
        }
        GameObject instantiated = Instantiate(Armory.TryGetWeapon(weaponName).weaponObject);
        instantiated.name = weaponName;
        instantiated.SetActive(true);
        if (extHardpoint != null)
            extHardpoint.Wm_OnWeaponEquippedHPIdx(hpIdx);
        InternalWeaponBay iwb = __instance.GetWeaponBay(hpIdx);
        if (iwb)
            iwb.Open();

        Transform weaponTf = instantiated.transform;
        Transform[] allTfs = (Transform[])(LCPTraverse.Field("hpTransforms").GetValue());
        Transform hpTf = allTfs[hpIdx];
        __instance.equips[hpIdx] = weaponTf.GetComponent<HPEquippable>();
        __instance.equips[hpIdx].OnConfigAttach(__instance);
        weaponTf.rotation = hpTf.rotation;
        Vector3 localPos = new Vector3(0f, -4f, 0f);
        weaponTf.position = hpTf.TransformPoint(localPos);
        __instance.UpdateNodes();
        Vector3 tgt = new Vector3(0f, 0f, 0.5f);
        while ((localPos - tgt).sqrMagnitude > 0.01f)
        {
            localPos = Vector3.Lerp(localPos, tgt, 5f * Time.deltaTime);
            weaponTf.position = hpTf.TransformPoint(localPos);
            yield return null;
        }
        weaponTf.parent = hpTf;
        weaponTf.localPosition = tgt;
        weaponTf.localRotation = Quaternion.identity;
        __instance.vehicleRb.AddForceAtPosition(Vector3.up * __instance.equipImpulse, __instance.wm.hardpointTransforms[hpIdx].position, ForceMode.Impulse);
        AudioSource[] allAudioTfs = (AudioSource[])(LCPTraverse.Field("hpAudioSources").GetValue());
        allAudioTfs[hpIdx].PlayOneShot(__instance.attachAudioClip);
        __instance.attachPs.transform.position = hpTf.position;
        __instance.attachPs.FireBurst();
        yield return new WaitForSeconds(0.2f);
        while (weaponTf.localPosition.sqrMagnitude > 0.001f)
        {
            weaponTf.localPosition = Vector3.MoveTowards(weaponTf.localPosition, Vector3.zero, 4f * Time.deltaTime);
            yield return null;
        }
        weaponTf.localPosition = Vector3.zero;
        __instance.UpdateNodes();
        if (iwb)
            iwb.Close();
        Coroutine[] attachRoutines = (Coroutine[])LCPTraverse.Field("attachRoutines").GetValue();
        attachRoutines[hpIdx] = null;
        LCPTraverse.Field("attachRoutines").SetValue(attachRoutines);
        Debug.Log("Ended attach routine.");
        //FindAllChildrenRecursively(instantiated.transform);
        yield break;
    }
    public static void FindAllChildrenRecursively(Transform parent) // debug function :P
    {
        return;
        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);
            if (child == null)
                continue;
            Debug.Log("Found child of parent " + parent.name + " called " + child);
            FindAllChildrenRecursively(child);
        }
    }
}
