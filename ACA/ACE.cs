using System;
using System.Collections.Generic;
using UnityEngine;

public class ACE : PartModule
{
    [KSPField]
    public float maxlength = 90f;
    public float TargetDA = 0f;
    public float torque = 10000f;
    public float BlockA = 0f;
    public float maxgee = 50f;
    public bool engineIgnited = true;
    private PidController grabPid = new PidController(10F, 0.005F, 0.005F, 50, 5);
    List<KSPParticleEmitter> acEmitters = new List<KSPParticleEmitter>();
    public string Liftcoefficient = "0,0;0.5,1;1,0";
    protected FloatCurve lccurve = new FloatCurve();

    [KSPAction("Toggle Engine", KSPActionGroup.None)]
    private void ActionActivate(KSPActionParam param)
    {
        if (engineIgnited == true)
        {
            Deactivate();
        }
        else
        {
            Activate();
        }
    }

    [KSPEvent(name = "Activate", guiName = "Activate Engine", active = true, guiActive = true)]
    public void Activate()
    {
            engineIgnited = true;
            Events["Deactivate"].guiActive = true;
            Events["Activate"].guiActive = false;
    }

    [KSPEvent(name = "Deactivate", guiName = "Deactivate Engine", active = true, guiActive = false)]
    public void Deactivate()
    {
        engineIgnited = false;
        Events["Deactivate"].guiActive = false;
        Events["Activate"].guiActive = true;
    }

    [KSPField(guiActive = true, guiActiveEditor = true, guiName = "Target Velocity", isPersistant = true), UI_FloatRange(minValue = 0f, maxValue = 3f, stepIncrement = 1f)]
    public float Overload = 1f;

    public override void OnStart(PartModule.StartState state)
    {
        this.enabled = true;
        this.part.force_activate();
        this.vessel.SetReferenceTransform(this.part);
        foreach (Transform aceTransform in part.FindModelTransforms("aceTransform"))
        {
            KSPParticleEmitter pEmitter = aceTransform.gameObject.GetComponent<KSPParticleEmitter>();
            acEmitters.Add(pEmitter);
        }

    }

    public override void OnFixedUpdate()
    {
        if (engineIgnited)
        {
            Activate();
            float pitch = vessel.ctrlState.pitch;
            float roll = vessel.ctrlState.roll;
            float yaw = vessel.ctrlState.yaw;
            float y = -vessel.ctrlState.Y * Overload * 10;
            float x = -vessel.ctrlState.X * Overload * 10;
            float z = - vessel.ctrlState.Z * Overload * 10;

            Vector3 gee = FlightGlobals.getGeeForceAtPosition(transform.position);
            Vector3 Breakforce = this.vessel.ActionGroups.groups[5] ? (-this.vessel.GetSrfVelocity()).normalized * Mathf.Min(this.vessel.GetSrfVelocity().magnitude / Time.fixedDeltaTime, Overload * 10f) - gee * 0.5f : Vector3.zero;
            Vector3 controlforce = vessel.ReferenceTransform.up * z + vessel.ReferenceTransform.forward * y + vessel.ReferenceTransform.right * x + Breakforce;
            Vector3 controltorque = (-vessel.transform.right * pitch - vessel.transform.up * roll - vessel.transform.forward * yaw)*torque;
            this.part.AddForce(controlforce * this.part.rb.mass);
            this.part.AddTorque(controltorque * this.part.rb.mass);

            if (pitch != 0 || roll != 0 || yaw != 0 || y != 0 || x != 0 || z != 0)
                foreach (KSPParticleEmitter pEmitter in acEmitters)
                {
                    pEmitter.emit = true;
                    pEmitter.maxEmission = (int)(5000 * Mathf.Max(Mathf.Abs(pitch*10) ,Mathf.Abs(roll*10) , Mathf.Abs(yaw*10) , Mathf.Abs(x) , Mathf.Abs(y) , Mathf.Abs(z),Mathf.Abs(Breakforce.magnitude)) / 10);
                    pEmitter.minEmission = (int)(4000 * Mathf.Max(Mathf.Abs(pitch * 10), Mathf.Abs(roll * 10), Mathf.Abs(yaw * 10), Mathf.Abs(x), Mathf.Abs(y), Mathf.Abs(z), Mathf.Abs(Breakforce.magnitude)) / 10);
                }
            else
            {
                foreach (KSPParticleEmitter pEmitter in acEmitters)
                {
                    pEmitter.emit = false;
                }
            }
        }
        else
        {
            Deactivate();
           foreach(KSPParticleEmitter pEmitter in acEmitters)
           {
            pEmitter.emit = false;
            }
        }
        lccurve = stringToFloatCurve(Liftcoefficient);
        Vector3 srfVelocity = vessel.GetSrfVelocity();
        Vector3 Airspeed = part.transform.InverseTransformDirection(srfVelocity);
        Vector3 VesselPosition = part.partTransform.position;
        double temp = FlightGlobals.getExternalTemperature(VesselPosition, vessel.mainBody);
        double pressure = FlightGlobals.getStaticPressure(VesselPosition);
        double rho = FlightGlobals.getAtmDensity(pressure, temp, vessel.mainBody);
        Vector3 yawsForce = (float)rho * Airspeed.x * this.part.rb.mass * -vessel.transform.right;
        if (yawsForce.magnitude > this.vessel.totalMass * maxgee)
        {
            yawsForce *= (float)(this.vessel.totalMass * maxgee / yawsForce.magnitude);
        }
        Vector3 pitchsForce = (float)rho * Airspeed.z * this.part.rb.mass *  -vessel.transform.forward;
        if (pitchsForce.magnitude > this.vessel.totalMass * maxgee)
        {
            pitchsForce *= (float)(this.vessel.totalMass * maxgee / pitchsForce.magnitude);
        }
        part.AddForce(yawsForce);
        part.AddForce(pitchsForce);
    }
    public static FloatCurve stringToFloatCurve(string curveString)
    {
        FloatCurve resultCurve = new FloatCurve();

        string[] keyString = curveString.Split(';');
        for (int i = 0; i < keyString.Length; i++)
        {
            string[] valueString = keyString[i].Split(',');
            if (valueString.Length >= 2)
            {
                Vector4 key = Vector4.zero;
                float.TryParse(valueString[0], out key.x);
                float.TryParse(valueString[1], out key.y);
                if (valueString.Length == 4)
                {
                    float.TryParse(valueString[2], out key.z);
                    float.TryParse(valueString[3], out key.w);
                }

                resultCurve.Add(key.x, key.y, key.z, key.w);
            }
        }
        return resultCurve;
    }
}