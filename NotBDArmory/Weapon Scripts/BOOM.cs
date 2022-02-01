using Harmony;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


class BOOM : MonoBehaviour
{
    public static BOOM instance { get; private set; }
    private static List<RoutineHandler> allHandlers = new List<RoutineHandler>();
    private static List<HandlerQSData> quickSavedHandlers = new List<HandlerQSData>();
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            Debug.Log("Set boom instance.");
        }
        if (j4ShipPrefab == null)
        {
            Debug.LogWarning("j4ShipPrefab was somehow null?");
            j4ShipPrefab = Resources.Load<GameObject>("units/enemy/mothership");
        }
        allHandlers = new List<RoutineHandler>();
        QuicksaveManager.instance.OnQuicksave += CreateQuickSavedRoutines;
        QuicksaveManager.instance.OnQuickloadEarly += DestroyRoutines;
        QuicksaveManager.instance.OnQuickloadLate += ResumeRoutines;
        VTOLAPI.MissionReloaded += DestroyRoutines;
    }
    private void Update()
    {
        if (Input.GetKey(KeyCode.K))
        {
            //DoEmpExplode(VTOLAPI.GetPlayersVehicleGameObject().transform);
        }
        else if (Input.GetKey(KeyCode.P))
        {
            //DoNukeExplode(VTOLAPI.GetPlayersVehicleGameObject().transform.position);
        }
        else if (Input.GetKeyDown(KeyCode.O))
            Resources.FindObjectsOfTypeAll<ParticleSystem>().SetEmissionAndActive(true);
    }
    public void DoNukeExplode(Vector3 pos)
    {
        NukeExplodeHandler handler = Instantiate(new GameObject()).AddComponent<NukeExplodeHandler>();
        handler.targetPos = pos;
        allHandlers.Add(handler); 
        StartCoroutine(handler.ExplodeRoutine());
    }
    public void DoEmpExplode(Vector3 pos)
    {
        EmpExplodeHandler handler = Instantiate(new GameObject()).AddComponent<EmpExplodeHandler>(); // WHY DO YOU AND ONLY YOU NEED A MISSILE I HATE THAT I MUST DO THIS
        handler.targetPos = pos;
        allHandlers.Add(handler);
        StartCoroutine(handler.ExplodeRoutine());
    }

    private void CreateQuickSavedRoutines(ConfigNode _)
    {
        quickSavedHandlers = new List<HandlerQSData>();
        foreach (RoutineHandler handler in allHandlers)
        {
            RoutineHandler newHandler = new RoutineHandler(handler);
            quickSavedHandlers.Add(new HandlerQSData(newHandler));
        }
    }
    private void DestroyRoutines(ConfigNode _) => DestroyRoutines();
    private void DestroyRoutines()
    {
        foreach (RoutineHandler routine in allHandlers)
        {
            routine.stopCoroutine = true;
        }
    }
    private void ResumeRoutines(ConfigNode _)
    {
        if (messages == null)
            messages = VTOLAPI.GetPlayersVehicleGameObject().GetComponent<VehicleMaster>().hudMessages;
        foreach (HandlerQSData handlerQSData in quickSavedHandlers)
        {
            RoutineHandler newHandler;
            if (handlerQSData.isnuke)
                newHandler = Instantiate(new GameObject()).AddComponent<NukeExplodeHandler>();
            else
                newHandler = Instantiate(new GameObject()).AddComponent<EmpExplodeHandler>();
            newHandler.CopyFields(handlerQSData);
            StartCoroutine(newHandler.ExplodeRoutine());
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
        public bool stopCoroutine = false;
        public bool isnuke = false;
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
        }
        public void CopyFields(HandlerQSData toCopy)
        {
            this.targetPos = toCopy.targetPos;
            this.r = toCopy.r;
            this.criticality = toCopy.criticality;
        }
        public virtual IEnumerator ExplodeRoutine()
        {
            yield break;
        }
    }
    private class NukeExplodeHandler : RoutineHandler
    {
        //public NukeExplodeHandler(Vector3 targetPos) : base(targetPos) { }
        public NukeExplodeHandler() { }
        public NukeExplodeHandler(RoutineHandler handler) : base(handler) { this.isnuke = true; }
        public override IEnumerator ExplodeRoutine()
        {
            if (base.targetPos == null)
            {
                Debug.LogError("Target pos null, nuke will not work.");
            }
            GameObject j4Ship = Instantiate(j4ShipPrefab);
            J4Mothership jM = j4Ship.GetComponent<J4Mothership>();
            GameObject explosionObject = jM.explosionObject;
            explosionObject.transform.parent = null;
            explosionObject.transform.position = targetPos;
            Transform explodeSphereTf = jM.explodeSphereTf;
            explodeSphereTf.parent = explosionObject.transform;
            explodeSphereTf.position = targetPos;
            Destroy(j4Ship);
            int curr_idx = nukeIdx++;
            if (BOOM.messages == null)
                BOOM.messages = VTOLAPI.GetPlayersVehicleGameObject().GetComponent<VehicleMaster>().hudMessages;
            if (BOOM.messages != null)
                for (; criticality > 0; criticality--)
                {
                    BOOM.messages.SetMessage("nuke criticality " + (curr_idx), "Nuke critical in: " + criticality);
                    yield return new WaitForSeconds(1f);
                }
            else
                yield return new WaitForSeconds(6f);
            BOOM.messages.SetMessage("nuke criticality " + (curr_idx), "BOOM!");
            explosionObject.SetActive(true);
            bool turnedoffmessage = false;
            while (r < 20000f)
            {
                if (stopCoroutine)
                {
                    Destroy(explosionObject);
                    //explodeSphereTf.gameObject.SetActive(false);
                    allHandlers.Remove(this);
                    Destroy(this);
                    yield break;
                }
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
                if (!turnedoffmessage)
                {
                    messages.RemoveMessage("nuke criticality " + (curr_idx));
                }
            }
            explodeSphereTf.gameObject.SetActive(false);
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
        public override IEnumerator ExplodeRoutine()
        {
            if (targetPos == null)
            {
                Debug.LogError("Target pos null, emp will not work.");
            }
            GameObject j4Ship = Instantiate(Resources.Load<GameObject>("units/enemy/mothership"));
            J4Mothership jM = j4Ship.GetComponent<J4Mothership>();
            GameObject explosionObject = jM.explosionObject;
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
            while (r < 100f)
            {
                if (stopCoroutine)
                {
                    Destroy(explosionObject);   
                    //explodeSphereTf.gameObject.SetActive(false);
                    allHandlers.Remove(this);
                    Destroy(this);
                    yield break;
                }
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
                                foreach (ModuleEngine engine in pilot.autoPilot.engines)
                                {
                                    engine.StopImmediate();
                                    engine.FailEngine();
                                }
                            }
                        }
                        Debug.Log("EMP'd actor " + actor.name);
                        empedActors.Add(actor);
                    }
                    else
                    {
                        //Debug.Log("Not emping actor " + actor.actorName);
                        //Debug.Log($"{actor.alive} {(actor.position - explosionObject.transform.position).sqrMagnitude < num} {actor.transform != explosionObject.transform} {!empedActors.Contains(actor)}");
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
