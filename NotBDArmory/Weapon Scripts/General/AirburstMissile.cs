using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

class AirburstMissile : MonoBehaviour
{
    public Missile missile;

    public void Awake()
    {
        missile = base.GetComponent<Missile>();
    }

    private void Update()
    {
    }
}
