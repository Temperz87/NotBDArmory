using Harmony;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

class Nuke : MonoBehaviour
{
    private void Awake()
    {
        GetComponent<Missile>().OnMissileDetonated += DoNukeExplode;
        //if (QuicksaveManager.instance != null)
        //    QuicksaveManager.instance.OnQuickloadedMissiles += ResubEvent;
    }
    public static void DoNukeExplode(Missile missile)
    {
        Debug.Log("DoNukeExplode invoked.");
        BigExplosionHandler.DoNukeExplode(missile.transform.position);
    }
    //private void ResubEvent(ConfigNode _)
    //{
    //    GetComponent<Missile>().OnMissileDetonated += DoNukeExplode;
    //}
}