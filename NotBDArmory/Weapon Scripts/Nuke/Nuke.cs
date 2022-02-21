using Harmony;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using VTNetworking;

class Nuke : MonoBehaviour
{
    private void Awake()
    {
        if (VTScenario.current != null)
            VTScenario.current.qsMode = QuicksaveManager.QSModes.None;
        Missile ms = GetComponent<Missile>();
        if (ms != null)
            ms.OnMissileDetonated += DoNukeExplode;
        else
            GetComponent<Rocket>().OnDetonated += DoNukeExplode;
    }
    public static void DoNukeExplode(Missile missile)
    {
        BigExplosionHandler.DoNukeExplode(missile.transform.position, missile.explodeRadius);
    }
    public static void DoNukeExplode(Rocket rocket)
    {
        BigExplosionHandler.DoNukeExplode(rocket.transform.position, rocket.radius);
    }
}