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
        //if (QuicksaveManager.instance != null)
        //    QuicksaveManager.instance.OnQuickloadedMissiles += ResubEvent;
    }
    public static void DoEmpExplode(Missile missile)
    {
        Debug.Log("DoEmpExplode invoked.");
        BigExplosionHandler.DoEmpExplode(missile.transform.position);
    }
    private void ResubEvent(ConfigNode _)
    {
        //GetComponent<Missile>().OnMissileDetonated += DoEmpExplode;
    }
}