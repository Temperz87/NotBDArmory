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
        if (QuicksaveManager.instance != null)
            QuicksaveManager.instance.OnQuickloadedMissiles += ResubEvent;
    }
    public static void DoEmpExplode(Missile missile)
    {
        Debug.Log("DoEmpExplode invoked.");
        BOOM.instance.DoEmpExplode(missile.transform.position);
    }
    private void ResubEvent(ConfigNode _)
    {
        GetComponent<Missile>().OnMissileDetonated += DoEmpExplode;
    }
}
class Nuke : MonoBehaviour
{
    private void Awake()
    {
        GetComponent<Missile>().OnMissileDetonated += DoNukeExplode;
        if (QuicksaveManager.instance != null)
            QuicksaveManager.instance.OnQuickloadedMissiles += ResubEvent;
    }
    public static void DoNukeExplode(Missile missile)
    {
        Debug.Log("DoNukeExplode invoked.");
        BOOM.instance.DoNukeExplode(missile.transform.position);
    }
    private void ResubEvent(ConfigNode _)
    {
        GetComponent<Missile>().OnMissileDetonated += DoNukeExplode;
    }
}
