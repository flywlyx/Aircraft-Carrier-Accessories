using UnityEngine;

public class Catapult : PartModule
{
    [KSPField]
    public float maxlength = 90f;
    public float TargetDA = 0f;
    public float BlockA = 0f;
    private Transform CatapultTransform = null;
    private Transform WheelTransform = null;
    private Transform RailTransform = null;
    public bool IsReseting = false;
    public bool IsGrabing = false;
    public bool IsReady = false;
    public bool IsLaunching = false;
    public bool IsBreak = false;
    public Part Wheel = null;
    public Rigidbody WheelBody = null;
    public ModuleWheelBase WheelModule = null;
    public MeshRenderer CatapultColor = null;
    public float time = 0f;
    private PidController xPid = new PidController(50F, 0.005F, 0.005F, 50, 50);
    private PidController yPid = new PidController(50F, 0.005F, 0.005F, 50, 50);
    private PidController zPid = new PidController(50F, 0.005F, 0.005F, 50, 50);
    [KSPField(isPersistant = true)]
    public bool IsActivated = true;


    [KSPAction("Toggle Catapult", KSPActionGroup.None)]
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
        CatapultTransform = base.part.FindModelTransform("Catapult");
        RailTransform = base.part.FindModelTransform("Rail");        
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
            if (WheelTransform)
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
                Vector3 ReV = CatapultTransform.InverseTransformDirection(this.part.rb.velocity - WheelBody.velocity);
                if (IsLaunching)
                {
                    ReV = Vector3.zero;
                }
                Vector3 ReC = ReV + ReP*5;
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
                    Wheel.AddForceAtPosition(GrabForce * VesselMass * 10f, WheelTransform.position);
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
                Vector3 LaunchReP = CatapultTransform.InverseTransformDirection(CatapultTransform.position - WheelTransform.position - CatapultTransform.right * Acceleration * time * Time.fixedDeltaTime);
                Vector3 LaunchReV = CatapultTransform.InverseTransformDirection(CatapultTransform.right * Acceleration * time + this.part.rb.velocity - WheelBody.velocity);
                Vector3 LaunchReC = LaunchReV + LaunchReP*10;
                Vector3 LaunchGrabForce = CatapultTransform.TransformDirection(LaunchReC)*10;
                Vector3 AccForce = CatapultTransform.right * Acceleration ;
                Wheel.AddForceAtPosition((AccForce + LaunchGrabForce) * VesselMass, WheelTransform.position);

                if (CatapultTransform.localPosition.x > maxlength || IsBreak)
                {
                    IsReseting = true;
                    IsGrabing = false;
                    IsReady = false;
                    IsLaunching = false;
                    WheelModule = null;
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
                Wheel = IRpart;
                WheelBody = other.attachedRigidbody;
                if (!WheelTransform)
                {
                    WheelTransform = other.transform;
                    Debug.Log("Wheel");
                    if (!Wheel.rb)
                    {
                        Debug.Log("Null");
                    }
                }
                IsGrabing = true;
            }
        }
    }

    private bool IsWheel(Part IRpart)
    {
        foreach (PartModule m in IRpart.Modules)
        {
            if (m.moduleName == "ModuleWheelBase")
            {
                WheelModule = (ModuleWheelBase)m;
                WheelTransform = IRpart.FindModelTransform(WheelModule.wheelTransformName);
                if (WheelModule.isGrounded)
                {
                    //Debug.Log("Contact");
                    return true;
                }
            }
            else
                if (m.moduleName == "KSPWheelBase")
            {
                //WheelTransform = IRpart.FindModelTransform(Wheel.wheelTransformName);
                return true;
            }
        }
        return false;
    }
}
