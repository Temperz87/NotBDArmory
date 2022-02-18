using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

class AirburstMissile : MonoBehaviour
{
    private Missile missile;
    private float airbustAlt;

    public void Awake()
    {
        missile = base.GetComponent<Missile>();
        if (missile == null)
            Debug.LogError(gameObject.name + " has no missile to airburst!");
        airbustAlt = missile.proxyDetonateRange;
        missile.proxyDetonateRange = 0f;
    }

    private void Update()
    {
        if (!missile.fired)
            return;
        if (Physics.Raycast(transform.position, Vector3.down, out _, airbustAlt, 1, QueryTriggerInteraction.Ignore))
        {
            Debug.Log("Airbursting missile " + gameObject.name);
            missile.Detonate();
        }
    }
}