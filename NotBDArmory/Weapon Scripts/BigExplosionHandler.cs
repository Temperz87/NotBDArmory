using Harmony;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


static class BigExplosionHandler
{
    private static List<RoutineHandler> allHandlers = new List<RoutineHandler>();
    private static List<HandlerQSData> quickSavedHandlers = new List<HandlerQSData>();
    static BigExplosionHandler()
    {
        if (j4ShipPrefab == null)
        {
            Debug.LogWarning("j4ShipPrefab was somehow null?");
            j4ShipPrefab = Resources.Load<GameObject>("units/enemy/mothership");
        }
        //allHandlers = new List<RoutineHandler>();
        QuicksaveManager.instance.OnQuicksave += CreateQuickSavedRoutines;
        QuicksaveManager.instance.OnQuickloadLate += ResumeRoutines;
    }

    public static void DoNukeExplode(Vector3 pos) // This is a bad solution
    {
        NukeExplodeHandler handler = GameObject.Instantiate(new GameObject()).AddComponent<NukeExplodeHandler>();
        handler.gameObject.SetActive(true);
        handler.targetPos = pos;
        allHandlers.Add(handler);
        handler.StartExplode();
    }
    public static void DoEmpExplode(Vector3 pos)
    {
        EmpExplodeHandler handler = GameObject.Instantiate(new GameObject()).AddComponent<EmpExplodeHandler>();
        handler.gameObject.SetActive(true);
        handler.targetPos = pos;
        allHandlers.Add(handler);
        handler.StartExplode();
    }

    private static void CreateQuickSavedRoutines(ConfigNode _)
    {
        return;
        Debug.Log("Try quicksave routines.");
        quickSavedHandlers = new List<HandlerQSData>();
        foreach (RoutineHandler handler in allHandlers)
            quickSavedHandlers.Add(new HandlerQSData(handler));
    }
    private static void ResumeRoutines(ConfigNode _)
    {
        return;
        allHandlers = new List<RoutineHandler>();
        if (messages == null)
            messages = VTOLAPI.GetPlayersVehicleGameObject().GetComponent<VehicleMaster>().hudMessages;
        foreach (HandlerQSData handlerQSData in quickSavedHandlers)
        {
            RoutineHandler newHandler = null;
            if (handlerQSData.isnuke)
                newHandler = GameObject.Instantiate(new GameObject()).AddComponent<NukeExplodeHandler>();
            else
                newHandler = GameObject.Instantiate(new GameObject()).AddComponent<EmpExplodeHandler>();
            newHandler.CopyFields(handlerQSData);
            newHandler.StartExplode();
            allHandlers.Add(newHandler);
        }
    }

    private struct HandlerQSData
    {
        public HandlerQSData(RoutineHandler toCopy)
        {
            this.targetPos = toCopy.targetPos;
            this.r = toCopy.r;
            this.criticality = toCopy.criticality;
            this.isnuke = toCopy.isnuke;
        }
        public Vector3 targetPos;
        public float r;
        public int criticality; // incase i wanna reuse it
        public bool isnuke;
    }
    private class RoutineHandler : MonoBehaviour // OH GOD WHY MUST YOU BE LIKE THIS
    {
        public static int nukeIdx;
        public Vector3 targetPos;
        public float r = 0f;
        public int criticality = 6; // incase i wanna reuse it
        public bool isnuke = false;
        public GameObject explosionObject;

        public RoutineHandler() { }
        /*public RoutineHandler(Vector3 targetPos)
        {
            this.targetPos = targetPos;
        }*/
        public RoutineHandler(RoutineHandler toCopy)
        {
            this.targetPos = toCopy.targetPos;
            this.r = toCopy.r;
            this.criticality = toCopy.criticality;
            this.isnuke = toCopy.isnuke;
            QuicksaveManager.instance.OnQuickloadEarly += Destroy;
            VTOLAPI.MissionReloaded += Destroy;
        }
        public void CopyFields(HandlerQSData toCopy)
        {
            this.targetPos = toCopy.targetPos;
            this.r = toCopy.r;
            this.criticality = toCopy.criticality;
        }

        public void StartExplode()
        {
            StartCoroutine(ExplodeRoutine());
        }

        internal virtual IEnumerator ExplodeRoutine()
        {
            yield break;
        }

        public virtual void Destroy(ConfigNode _) => Destroy();

        public virtual void Destroy()
        {
            Destroy(explosionObject);
            //explodeSphereTf.gameObject.SetActive(false);
            allHandlers.Remove(this);
            Destroy(this);
        }

    }
    private class NukeExplodeHandler : RoutineHandler
    {
        //public NukeExplodeHandler(Vector3 targetPos) : base(targetPos) { }
        public NukeExplodeHandler() { }
        public NukeExplodeHandler(RoutineHandler handler) : base(handler) { this.isnuke = true; }
        internal override IEnumerator ExplodeRoutine()
        {
            if (base.targetPos == null)
            {
                Debug.LogError("Target pos null, nuke will not work.");
            }
            GameObject j4Ship = Instantiate(j4ShipPrefab);
            J4Mothership jM = j4Ship.GetComponent<J4Mothership>();
            explosionObject = jM.explosionObject;
            explosionObject.transform.parent = null;
            explosionObject.transform.position = targetPos;
            Transform explodeSphereTf = jM.explodeSphereTf;
            explodeSphereTf.parent = explosionObject.transform;
            explodeSphereTf.position = targetPos;
            Destroy(j4Ship);
            int curr_idx = nukeIdx++;
            if (BigExplosionHandler.messages == null)
                BigExplosionHandler.messages = VTOLAPI.GetPlayersVehicleGameObject().GetComponent<VehicleMaster>().hudMessages;
            if (BigExplosionHandler.messages != null)
                for (; criticality > 0; criticality--)
                {
                    BigExplosionHandler.messages.SetMessage("nuke criticality " + (curr_idx), "Nuke critical in: " + criticality);
                    yield return new WaitForSeconds(1f);
                }
            else
                yield return new WaitForSeconds(6f);
            explosionObject.SetActive(true);
            messages.RemoveMessage("nuke criticality " + (curr_idx));
            while (r < 20000f && explosionObject != null)
            {
                float num = r * r;
                for (int i = 0; i < TargetManager.instance.allActors.Count; i++)
                {
                    Actor actor = TargetManager.instance.allActors[i];
                    if (actor.alive && (actor.position - explosionObject.transform.position).sqrMagnitude < num && actor.transform != explosionObject.transform)
                    {
                        Health[] component = actor.GetComponentsInChildren<Health>();
                        if (component != null)
                            foreach (var health in component)
                            {
                                health.Kill();
                            }
                    }
                }
                explodeSphereTf.localScale = 2f * r * Vector3.one;
                r += 343f * Time.deltaTime;
                yield return new WaitForSeconds(0.01f);
            }
            float t = 0;
            while (r > 20000f && explosionObject != null)
            {
                t += Time.deltaTime;
                explodeSphereTf.localScale = Vector3.Slerp(explodeSphereTf.localScale, Vector3.zero, t);
                yield return null;
            }
            Destroy(explodeSphereTf.gameObject);
            allHandlers.Remove(this);
            Destroy(this);
            yield break;
        }
    }
    private class EmpExplodeHandler : RoutineHandler
    {
        //public EmpExplodeHandler(Vector3 targetPos) : base(targetPos) { }
        public EmpExplodeHandler() { }
        public EmpExplodeHandler(RoutineHandler handler) : base(handler) { this.isnuke = false; }
        internal override IEnumerator ExplodeRoutine()
        {
            if (targetPos == null)
            {
                Debug.LogError("Target pos null, emp will not work.");
            }
            GameObject j4Ship = Instantiate(Resources.Load<GameObject>("units/enemy/mothership"));
            J4Mothership jM = j4Ship.GetComponent<J4Mothership>();
            explosionObject = jM.explosionObject;
            explosionObject.transform.parent = null;
            explosionObject.transform.position = targetPos;
            explosionObject.GetComponent<ParticleSystem>().startColor = new Color(131f, 255f, 254f, 1f);
            foreach (var sys in explosionObject.GetComponentsInChildren<ParticleSystem>())
            {
                if (sys.name == "mainFlame" || sys.name == "smokeColumn" || sys.name == "smokeHead" || sys.name == "groundDust")
                {
                    sys.gameObject.SetActive(false);
                }
            }
            Transform explodeSphereTf = jM.explodeSphereTf;
            explodeSphereTf.parent = explosionObject.transform;
            explodeSphereTf.position = targetPos;
            explodeSphereTf.localScale = Vector3.one;
            Destroy(j4Ship);
            //yield return new WaitForSeconds(6.6f);
            explosionObject.SetActive(true);
            explosionObject.transform.parent = null;
            float r = 0f;
            List<Actor> empedActors = new List<Actor>();
            while (r < 100f && explosionObject != null)
            {
                float num = r * r;
                for (int i = 0; i < TargetManager.instance.allActors.Count; i++) // baha if you're reading this just dm me to take down the mod no need for a cease and desist and/or loiyers
                {
                    Actor actor = TargetManager.instance.allActors[i];
                    if (actor.alive && (actor.position - explosionObject.transform.position).sqrMagnitude < num && actor.transform != explosionObject.transform && !empedActors.Contains(actor))
                    {
                        Debug.Log("Found actor to emp " + actor.actorName);
                        foreach (var battery in actor.GetComponentsInChildren<Battery>())
                        {
                            battery.Drain(battery.maxCharge);
                            battery.SetConnection(0);
                            battery.Kill();
                        }
                        AIPilot pilot = actor.GetComponent<AIPilot>();
                        if (pilot)
                        {
                            pilot.enabled = false;
                            if (pilot.autoPilot)
                            {
                                pilot.autoPilot.enabled = false;
                                pilot.aeroController?.SetRandomInputs();
                            }
                        }
                        foreach (ModuleEngine engine in actor.GetComponentsInChildren<ModuleEngine>(true))
                        {
                            engine.StopImmediate();
                            engine.FailEngine();
                            engine.GetComponent<Health>()?.Kill();
                        }
                        Debug.Log("EMP'd actor " + actor.name);
                        empedActors.Add(actor);
                    }
                }
                explodeSphereTf.localScale = 2f * r * Vector3.one;
                r += 343f * Time.deltaTime;
                yield return new WaitForSeconds(0.01f);
            }
            explodeSphereTf.gameObject.SetActive(false);
            allHandlers.Remove(this);
            Destroy(this);
            yield break;
        }
    }
    private static GameObject j4ShipPrefab = Resources.Load<GameObject>("units/enemy/mothership");
    public static HUDMessages messages = null;
}
