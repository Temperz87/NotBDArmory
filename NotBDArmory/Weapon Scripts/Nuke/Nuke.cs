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
        if (VTScenario.current != null)
            VTScenario.current.qsMode = QuicksaveManager.QSModes.None;
    }
    public static void DoNukeExplode(Missile missile)
    {
        BigExplosionHandler.DoNukeExplode(missile.transform.position);
    }
}