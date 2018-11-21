using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class LandZone : PartModule
{
    [KSPField]
    public float maxlength = 90f;
    public float TargetDA = 0f;
    public float BlockA = 0f;
    private Transform LandZoneTransform = null;
    public bool IsActivated = true;
    public bool IsReseting = false;
    public bool IsGrabing = false;
    public bool IsReady = false;
    public bool IsLaunching = false;
    public bool IsBreak = false;
    public ModuleWheelBase Wheel = null;
    public MeshRenderer CatapultColor = null;
    public float time = 0f;
    private PidController xPid = new PidController(50F, 0.005F, 0.005F, 50, 50);
    private PidController yPid = new PidController(50F, 0.005F, 0.005F, 50, 50);
    private PidController zPid = new PidController(50F, 0.005F, 0.005F, 50, 50);

    [KSPAction("Toggle", KSPActionGroup.None, guiName = "Toggle Zone")]
    private void ActionActivate(KSPActionParam param)
    {
        if (!IsActivated)
        {
            Activate();
        }
        else
        {
            Deactivate();
        }
    }

    [KSPEvent(name = "Activate", guiName = "Activate", active = true, guiActive = false)]
    public void Activate()
    {
        IsActivated = true;
        Events["Deactivate"].guiActive = true;
        Events["Activate"].guiActive = false;
    }

    [KSPEvent(name = "Deactivate", guiName = "Deactivate", active = true, guiActive = true)]
    public void Deactivate()
    {
        IsActivated = false;
        Events["Deactivate"].guiActive = false;
        Events["Activate"].guiActive = true;
        IsReseting = false;
        IsGrabing = false;
        IsReady = false;
        IsLaunching = false;
        IsBreak = false;
    }

    [KSPField(guiActive = true, guiActiveEditor = true, guiName = "Target Velocity", isPersistant = true), UI_FloatRange(minValue = 30f, maxValue = 100f, stepIncrement = 10f)]
    public float TargetV = 60f;

    public override void OnStart(PartModule.StartState state)
    {
        this.enabled = true;
        this.part.force_activate();
        CatapultTransform = base.part.FindModelTransform("LandZone");
    }

    public override void OnFixedUpdate()
    {
        if (HighLogic.LoadedSceneIsFlight)
        {
            RailTransform.localScale = Vector3.zero;
        }
        Color CataColor = Color.white;
        if (IsActivated)
        {
            if (Wheel)
            {
                if (Wheel.vessel.ActionGroups.groups[5])
                {
                    IsBreak = true;
                }
                else
                {
                    IsBreak = false;
                }
            }

            if (!CatapultColor)
            {
                CatapultColor = CatapultTransform.gameObject.GetComponent(typeof(MeshRenderer)) as MeshRenderer;
            }

            if (IsGrabing)
            {
                //Debug.Log(CatapultTransform.localPosition);
                float VesselMass = Wheel.vessel.GetTotalMass();
                Vector3 ReP = CatapultTransform.InverseTransformDirection(CatapultTransform.position - WheelTransform.position);
                Vector3 ReV = CatapultTransform.InverseTransformDirection(this.part.rb.velocity - WheelTransform.gameObject.GetComponentInParent<Part>().rb.velocity);
                if (IsLaunching)
                {
                    ReV = Vector3.zero;
                }
                Vector3 ReC = ReV + ReP;
                Vector3 GrabForce = CatapultTransform.TransformDirection(ReC.x, -1f, ReC.z);
                CataColor = Color.blue;
                if (!IsLaunching)
                {
                    foreach (Part p in Wheel.vessel.parts)
                    {
                        if ((p.physicalSignificance == Part.PhysicalSignificance.FULL) && (p.rb != null))
                        {
                            p.AddForce(GrabForce * 10f * p.rb.mass);
                        }
                    }
                }
                else
                {
                    Wheel.part.AddForceAtPosition(GrabForce * VesselMass * 10f, WheelTransform.position);
                }

                if (IsBreak && !IsReady && !IsLaunching)
                {
                    IsReady = true;
                }
            }

            if (IsReady)
            {
                CataColor = Color.yellow;
                if (!IsBreak && !IsLaunching)
                {
                    IsLaunching = true;
                    IsGrabing = false;
                }
            }

            if (IsLaunching)
            {
                float Acceleration = Mathf.Pow(TargetV, 2f) / (2f * maxlength);
                //Debug.Log(Acceleration);
                float VesselMass = Wheel.vessel.GetTotalMass();
                time += Time.fixedDeltaTime;
                CataColor = Color.green;
                CatapultTransform.localPosition = CatapultTransform.localPosition + Vector3.right * Acceleration * time * Time.fixedDeltaTime;
                Vector3 LaunchReP = CatapultTransform.InverseTransformDirection(CatapultTransform.position - WheelTransform.position);
                Vector3 LaunchReV = CatapultTransform.InverseTransformDirection(this.part.rb.velocity - WheelTransform.gameObject.GetComponentInParent<Part>().rb.velocity);
                Vector3 LaunchReC = LaunchReV + LaunchReP;
                Vector3 LaunchGrabForce = CatapultTransform.TransformDirection(0f, LaunchReC.y, 0f);
                Vector3 AccForce = CatapultTransform.right * Acceleration;
                Wheel.part.AddForceAtPosition((AccForce + LaunchGrabForce) * VesselMass, WheelTransform.position);

                if (CatapultTransform.localPosition.x > maxlength || IsBreak)
                {
                    IsReseting = true;
                    IsGrabing = false;
                    IsReady = false;
                    IsLaunching = false;
                    Wheel = null;
                    WheelTransform = null;
                    CataColor = Color.white;
                }
            }

            if (IsReseting)
            {
                CatapultTransform.localPosition = Vector3.zero;
                IsReseting = false;
                time = 0f;
            }
        }
        else
        {
            CataColor = Color.grey;
        }
        if (CatapultColor)
        {
            CatapultColor.material.SetColor("_Color", CataColor);
        }
    }

    void OnTriggerStay(Collider other)
    {
        //Debug.Log("Arrive");
        if (!IsReseting && !IsReady && !IsGrabing && !IsLaunching && IsActivated)
        {
            Part IRpart = other.gameObject.GetComponentInParent<Part>();
            if (IRpart && IRpart.vessel && IRpart.vessel != this.vessel && IsWheel(IRpart) == true)
            {
                //Debug.Log("Enter");
                IsGrabing = true;
                WheelTransform = IRpart.FindModelTransform(Wheel.wheelTransformName);
            }
        }
    }

    private bool IsWheel(Part IRpart)
    {
        foreach (PartModule m in IRpart.Modules)
        {
            if (m.moduleName == "ModuleWheelBase")
            {
                Wheel = (ModuleWheelBase)m;
                if (Wheel.isGrounded)
                {
                    //Debug.Log("Contact");
                    return true;
                }
            }
            else
                if (m.moduleName == "KSPWheelBase")
                {

                }
        }
        return false;
    }
}
