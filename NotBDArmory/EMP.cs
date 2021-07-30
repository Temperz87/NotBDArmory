using Harmony;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;


class EMP : MonoBehaviour
{
    private void Awake()
    {
        GetComponent<Missile>().OnMissileDetonated += DoEmpExplode;
    }
    public static void DoEmpExplode(Missile missile)
    {
        Debug.Log("DoEmpExplode invoked.");
        //for (int i = 0; i < 5; i++)
        BOOM.instance.DoEmpExplode(missile.transform);
    }

}
