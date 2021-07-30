﻿using Harmony;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


class BOOM : MonoBehaviour
{
    public static BOOM instance { get; private set; }
    private static List<Coroutine> allCoroutines = new List<Coroutine>();
    private static List<RoutineHandler> allHandlers = new List<RoutineHandler>();
    private static List<RoutineHandler> quickSavedHandlers = new List<RoutineHandler>();
    private GameObject routineSlave;
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
        messages = VTOLAPI.GetPlayersVehicleGameObject().GetComponent<VehicleMaster>().hudMessages;
        allCoroutines = new List<Coroutine>();
        allHandlers = new List<RoutineHandler>();
        QuicksaveManager.instance.OnQuicksave += CreateQuickSavedRoutines;
        QuicksaveManager.instance.OnQuickloadEarly += DestroyRoutines;
        QuicksaveManager.instance.OnQuickloadLate += ResumeRoutines;
        if (routineSlave == null)
        {
            routineSlave = Instantiate(new GameObject());
            DontDestroyOnLoad(routineSlave);
        }
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
        NukeExplodeHandler handler = routineSlave.AddComponent<NukeExplodeHandler>();
        handler.targetPos = pos;
        allHandlers.Add(handler);
        allCoroutines.Add(StartCoroutine(handler.ExplodeRoutine()));
    }
    public void DoEmpExplode(Transform missile)
    {
        EmpExplodeHandler handler = routineSlave.AddComponent<EmpExplodeHandler>(); // WHY DO YOU AND ONLY YOU NEED A MISSILE I HATE THAT I MUST DO THIS
        allHandlers.Add(handler);
        allCoroutines.Add(StartCoroutine(handler.ExplodeRoutine()));
    }

    private void CreateQuickSavedRoutines(ConfigNode _)
    {
        quickSavedHandlers = new List<RoutineHandler>();
        foreach (RoutineHandler handler in allHandlers)
            quickSavedHandlers.Add(handler);
    }
    private void DestroyRoutines(ConfigNode _) => DestroyRoutines();
    private void DestroyRoutines()
    {
        foreach (Coroutine routine in allCoroutines)
            StopCoroutine(routine);
    }
    private void ResumeRoutines(ConfigNode _)
    {
        if (messages == null)
            messages = VTOLAPI.GetPlayersVehicleGameObject().GetComponent<VehicleMaster>().hudMessages;
        foreach (RoutineHandler handler in allHandlers)
        {
            allCoroutines.Add(StartCoroutine(handler.ExplodeRoutine()));
        }
    }

    private class RoutineHandler : MonoBehaviour // OH GOD WHY MUST YOU BE LIKE THIS
    {
        public static int nukeIdx;
        public Vector3 targetPos;
        public float r = 0f;
        public int criticality = 6; // incase i wanna reuse it
        /*public RoutineHandler(Vector3 targetPos)
        {
            this.targetPos = targetPos;
        }
        public RoutineHandler(RoutineHandler toCopy)
        {
            this.targetPos = toCopy.targetPos;
            this.r = toCopy.r;
            this.criticality = toCopy.criticality;
        }*/
        public void CopyFields(RoutineHandler toCopy)
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
        //public NukeExplodeHandler(RoutineHandler handler) : base(handler) { }
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
        //public EmpExplodeHandler(RoutineHandler handler) : base(handler) { }
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
            Debug.Log(targetPos.ToString() + " EMPing...");
            List<Actor> empedActors = new List<Actor>();
            while (r < 20f)
            {
                float num = r * r;
                for (int i = 0; i < TargetManager.instance.allActors.Count; i++) // baha if you're reading this just dm me to take down the mod no need for a cease and desist and/or loiyers
                {
                    Actor actor = TargetManager.instance.allActors[i];
                    if (actor.alive && (actor.position - explosionObject.transform.position).sqrMagnitude < num && actor.transform != explosionObject.transform && !empedActors.Contains(actor))
                    {
                        foreach (var component in actor.GetComponentsInChildren<Battery>())
                        {
                            component.Drain(component.maxCharge);
                            component.ToggleConnection();
                            component.Kill();
                        }
                        AIPilot pilot = actor.GetComponent<AIPilot>();
                        if (pilot)
                        {
                            pilot.enabled = false;
                            pilot.autoPilot.enabled = false;
                        }
                        foreach (ModuleEngine engine in actor.GetComponentsInChildren<ModuleEngine>())
                            engine.FailEngine();
                        //foreach (var poweredObj in actor.GetComponentsInChildren<>)
                    }
                    Debug.Log("EMP'd actor " + actor.name);
                    empedActors.Add(actor);
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
