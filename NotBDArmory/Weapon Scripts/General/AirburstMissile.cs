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
        airbustAlt = missile.proxyDetonateRange;
        missile.proxyDetonateRange = 0f;
    }

    private void Update()
    {
        Vector3 a = transform.position + new Vector3(0, WaterPhysics.instance.height, 0);
        float terrainAlt = VTMapGenerator.fetch.GetHeightmapAltitude(transform.position);
        float agl = a.y + terrainAlt;

        if (agl <= airbustAlt)
            missile.Detonate();
    }
}