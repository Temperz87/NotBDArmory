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
        //VTNetEntity entity = GetComponent<VTNetEntity>();
        //if (entity == null || entity.isMine)
        GetComponent<Missile>().OnMissileDetonated += DoNukeExplode;
    }
    public static void DoNukeExplode(Missile missile)
    {
        BigExplosionHandler.DoNukeExplode(missile.transform.position);
    }
}