using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Reflection;
using Harmony;

class MissilePSOnDeath : MonoBehaviour
{
    private ParticleSystem[] deathSystems;
    private Missile missile;

    private void Awake()
    {
        Transform tf = transform.Find("deathSystems");
        deathSystems = tf.GetComponentsInChildren<ParticleSystem>(true);
        missile = GetComponent<Missile>();
        missile.OnDetonate.AddListener(delegate
        {
            tf.SetParent(null);
            deathSystems.Play();
        });
    }
}