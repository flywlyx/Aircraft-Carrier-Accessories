using UnityEngine;
using System;

public class CLLS : PartModule
{
    [KSPField]
    public float maxlength = 200f;
    public float width = 10f;
    public float length = 20f;
    public float TargetDA = 0f;
    public float PrepareP = 0f;
    public Vector3 ReP = Vector3.zero;
    public Vector3 ReV = Vector3.zero;
    private Vessel Target = null;
    private Transform CoreTransform = null;
    private Transform Guide1Transform = null;
    private Transform Guide2Transform = null;
    private Transform InstructionTransform = null;
    private Transform BaseTransform = null;
    private Transform TriggerTransform = null;
    private Transform ColliderTransform = null;
    public MeshRenderer Guide1Color = null;
    public MeshRenderer Guide2Color = null;
    public MeshRenderer InstructionColor = null;
    public MeshRenderer BaseRenderer = null;
    public float time = 0f;
    private PidController xPid = new PidController(10F, 0.005F, 0.002F, 50, 10);
    private PidController yPid = new PidController(10F, 0.005F, 0.002F, 50, 10);
    private PidController zPid = new PidController(10F, 0.005F, 0.002F, 50, 10);
    [KSPField(isPersistant = true)]
    public bool IsActivated = false;
    public bool IsReseting = false;
    public bool IsGrabing = false;
    public bool IsReady = false;
    public bool IsLaunching = false;
    public bool IsBreak = false;
    [KSPField(isPersistant = true)]
    public bool IsLanding = false;


    [KSPField(guiActive = true, guiActiveEditor = true, guiName = "Acceleration", isPersistant = true), UI_FloatRange(minValue = 10f, maxValue = 60f, stepIncrement = 10f)]
    public float Acceleration = 30f;

    [KSPAction("Toggle System", KSPActionGroup.None)]
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

    [KSPEvent(name = "Activate", guiName = "Activate", active = true, guiActive = true)]
    public void Activate()
    {
        IsActivated = true;
        Events["Deactivate"].guiActive = true;
        Events["Activate"].guiActive = false;
        InstructionTransform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        Guide1Color.material.SetTextureScale("_MainTex", new Vector2(length, 1f));
        Guide2Color.material.SetTextureScale("_MainTex", new Vector2(length, 1f));
        Guide1Transform.localScale = new Vector3(length, 1f, 1f) / 10f;
        Guide1Transform.localPosition = new Vector3(width, length, 0f) / 2f;
        Guide2Transform.localScale = new Vector3(length, 1f, 1f) / 10f;
        Guide2Transform.localPosition = new Vector3(-width, length, 0f) / 2f;
    }

    [KSPEvent(name = "Deactivate", guiName = "Deactivate", active = true, guiActive = false)]
    public void Deactivate()
    {
        IsActivated = false;
        Events["Deactivate"].guiActive = false;
        Events["Activate"].guiActive = true;
        Target = null;
        InstructionTransform.localScale = new Vector3(0f, 0f, 0f);
        Guide1Color.material.SetTextureScale("_MainTex", new Vector2(length, 1f));
        Guide2Color.material.SetTextureScale("_MainTex", new Vector2(length, 1f));
        Guide1Transform.localScale = new Vector3(length, 1f, 1f) / 10f;
        Guide1Transform.localPosition = new Vector3(width, length, 0f) / 2f;
        Guide2Transform.localScale = new Vector3(length, 1f, 1f) / 10f;
        Guide2Transform.localPosition = new Vector3(-width, length, 0f) / 2f;
    }


    [KSPAction("Toggle", KSPActionGroup.None, guiName = "Transform")]
    private void ActionTransform(KSPActionParam param)
    {
        if (IsLanding)
        {
            Launch();
        }
        else
        {
            Land();
        }
    }

    [KSPEvent(name = "Launch", guiName = "Launcher", active = true, guiActive = true)]
    public void Launch()
    {
        IsLanding = false;
        Events["Land"].guiActive = true;
        Events["Launch"].guiActive = false;
        Guide1Color.material.SetTextureScale("_MainTex", new Vector2(length, 1f));
        Guide2Color.material.SetTextureScale("_MainTex", new Vector2(length, 1f));
        Guide1Transform.localScale = new Vector3(length, 1f, 1f) / 10f;
        Guide1Transform.localPosition = new Vector3(width, length, 0f) / 2f;
        Guide2Transform.localScale = new Vector3(length, 1f, 1f) / 10f;
        Guide2Transform.localPosition = new Vector3(-width, length, 0f) / 2f;
        CoreTransform.localPosition = new Vector3(0f, length, 0f) / 2f;
        InstructionTransform.localPosition = new Vector3(0f, length, 0f);
    }

    [KSPEvent(name = "Land", guiName = "Arrester", active = true, guiActive = false)]
    public void Land()
    {
        IsLanding = true;
        Events["Land"].guiActive = false;
        Events["Launch"].guiActive = true;
    }

    public override void OnStart(PartModule.StartState state)
    {
        this.enabled = true;
        this.part.force_activate();
        CoreTransform = base.part.FindModelTransform("Core");
        Guide1Transform = base.part.FindModelTransform("Guide1");
        Guide2Transform = base.part.FindModelTransform("Guide2");
        InstructionTransform = base.part.FindModelTransform("Instruction");
        BaseTransform = base.part.FindModelTransform("Base");
        TriggerTransform = base.part.FindModelTransform("Trigger");
        ColliderTransform = base.part.FindModelTransform("Collider");
        ColliderTransform.localScale = Vector3.zero;
        Guide1Transform.localScale = new Vector3(length, 1f, 1f) / 10f;
        Guide1Transform.localPosition = new Vector3(width, length, 0f) / 2f;
        Guide2Transform.localScale = new Vector3(length, 1f, 1f) / 10f;
        Guide2Transform.localPosition = new Vector3(-width, length, 0f) / 2f;
        CoreTransform.localPosition = new Vector3(0f, length, 0f) / 2f;
        InstructionTransform.localPosition = new Vector3(0f, length, 0f);
        Guide1Color = Guide1Transform.gameObject.GetComponent(typeof(MeshRenderer)) as MeshRenderer;
        Guide1Color.material.SetTextureScale("_MainTex", new Vector2(length, 1f));
        Guide2Color = Guide2Transform.gameObject.GetComponent(typeof(MeshRenderer)) as MeshRenderer;
        Guide2Color.material.SetTextureScale("_MainTex", new Vector2(length, 1f));
        InstructionColor = InstructionTransform.gameObject.GetComponent(typeof(MeshRenderer)) as MeshRenderer;
        BaseRenderer = BaseTransform.gameObject.GetComponent(typeof(MeshRenderer)) as MeshRenderer;
        if (HighLogic.LoadedSceneIsFlight)
        {
            this.enabled = true;
            this.part.force_activate();
            BaseRenderer.enabled = false;
        }

        if (!IsActivated)
        {
            Activate();
        }
        else
        {
            Deactivate();
        }

        if (IsLanding)
        {
            Launch();
        }
        else
        {
            Land();
        }
    }

    public override void OnFixedUpdate()
    {
        ColliderTransform.localScale = Vector3.zero;
        if (IsActivated)
        {
            Color LaunchColor = Color.red;
            if (Target)
            {
                if (Target.ActionGroups.groups[5])
                {
                    IsBreak = true;
                }
                else
                {
                    IsBreak = false;
                }
            }

            if (!Guide1Color)
            {
                Guide1Color = Guide1Transform.gameObject.GetComponent(typeof(MeshRenderer)) as MeshRenderer;
            }

            if (!Guide2Color)
            {
                Guide2Color = Guide2Transform.gameObject.GetComponent(typeof(MeshRenderer)) as MeshRenderer;
            }

            if (!InstructionColor)
            {
                InstructionColor = InstructionTransform.gameObject.GetComponent(typeof(MeshRenderer)) as MeshRenderer;
            }
            if (Target)
            {
                ReP = CoreTransform.InverseTransformDirection(CoreTransform.position - Target.transform.position);
                ReV = CoreTransform.InverseTransformDirection(this.part.rb.velocity - Target.rootPart.rb.velocity);
            }
            else
            {
                IsGrabing = false;
                IsReady = false;
                IsLaunching = false;
                IsReseting = false;
            }

            if (!IsLanding)
            {
                TriggerTransform.localScale = new Vector3(width, length, 2f);
                TriggerTransform.localPosition = new Vector3(0f, length / 2f, 0f);
                if (IsGrabing)
                {
                    //Debug.Log("Grab");
                    Vector3 RPCancel = ReV + ReP;
                    Vector3 GrabForce = CoreTransform.TransformDirection(RPCancel);
                    LaunchColor = Color.blue;
                    InstructionColor.material.mainTexture = GameDatabase.Instance.GetTexture("KFC/Textures/StandBy", false);
                    if (time < maxlength)
                    {
                        //Debug.Log("Extend");
                        time += 1;
                        Guide1Color.material.SetTextureScale("_MainTex", new Vector2(time, 1f));
                        Guide2Color.material.SetTextureScale("_MainTex", new Vector2(time, 1f));
                        Guide1Transform.localScale = new Vector3(time, 1f, 1f) / 10f;
                        Guide1Transform.localPosition = new Vector3(width, time, 0f) / 2f;
                        Guide2Transform.localScale = new Vector3(time, 1f, 1f) / 10f;
                        Guide2Transform.localPosition = new Vector3(-width, time, 0f) / 2f;
                    }
                    else
                        if (time == maxlength && !IsReady && !IsLaunching && IsBreak)
                        {
                            IsReady = true;
                        }
                    if (!IsLaunching)
                    {
                        float totalmass = 0f;
                        foreach (Part p in Target.parts)
                        {
                            if ((p.physicalSignificance == Part.PhysicalSignificance.FULL) && (p.rb != null))
                            {
                                p.AddForce(GrabForce * p.rb.mass);
                                totalmass +=p.rb.mass;
                            }
                        }

                        foreach (Part p in this.vessel.parts)
                        {
                            if ((p.physicalSignificance == Part.PhysicalSignificance.FULL) && (p.rb != null))
                            {
                                p.AddForce(-GrabForce * p.rb.mass/totalmass);//Reaction
                            }
                        }
                        double consumption = GrabForce.magnitude * totalmass * Time.fixedDeltaTime;
                        double ECconsumtion = this.part.RequestResource("ElectricCharge", consumption);
                        if (Math.Round(consumption,4) != 0 && Math.Round(ECconsumtion,4) < Math.Round(consumption,4))
                        {
                            Deactivate();
                        }
                    }
                }

                if (IsReady)
                {
                    //Debug.Log("Ready");
                    LaunchColor = Color.yellow;
                    InstructionColor.material.mainTexture = GameDatabase.Instance.GetTexture("KFC/Textures/Ready", false);
                    if (!IsBreak && !IsLaunching)
                    {
                        IsLaunching = true;
                    }
                }

                if (IsLaunching)
                {
                    //Debug.Log(Acceleration);
                    LaunchColor = Color.green;
                    Vector3 AccForce = CoreTransform.TransformDirection(Vector3.up) * Acceleration * Time.fixedDeltaTime;
                    InstructionColor.material.mainTexture = GameDatabase.Instance.GetTexture("KFC/Textures/Launch", false);
                    float totalmass = 0f;
                    foreach (Part p in Target.parts)
                    {
                        if ((p.physicalSignificance == Part.PhysicalSignificance.FULL) && (p.rb != null))
                        {
                            p.AddForce(AccForce * p.rb.mass * 10f);
                            totalmass += p.rb.mass;
                        }
                    }

                    foreach (Part p in this.vessel.parts)
                    {
                        if ((p.physicalSignificance == Part.PhysicalSignificance.FULL) && (p.rb != null))
                        {
                            p.AddForce(-AccForce * (float)this.vessel.totalMass *10 / totalmass);//Reaction
                        }
                    }

                    double consumption = AccForce.magnitude * totalmass * Time.fixedDeltaTime;
                    double ECconsumtion = this.part.RequestResource("ElectricCharge", consumption);
                    if (Math.Round(consumption,4) != 0 && Math.Round(ECconsumtion,4) < Math.Round(consumption,4))
                    {
                        Deactivate();
                    }

                    if (ReP.magnitude > maxlength || IsBreak)
                    {
                        IsReseting = true;
                    }
                }

                if (IsReseting)
                {
                    CoreTransform.localPosition = Vector3.zero;
                    IsReseting = false;
                    IsGrabing = false;
                    IsReady = false;
                    IsLaunching = false;
                    Target = null;
                    LaunchColor = Color.red;
                    time = 0f;
                    Guide1Color.material.SetTextureScale("_MainTex", new Vector2(length, 1f));
                    Guide2Color.material.SetTextureScale("_MainTex", new Vector2(length, 1f));
                    Guide1Transform.localScale = new Vector3(length, 1f, 1f) / 10f;
                    Guide1Transform.localPosition = new Vector3(width, length, 0f) / 2f;
                    Guide2Transform.localScale = new Vector3(length, 1f, 1f) / 10f;
                    Guide2Transform.localPosition = new Vector3(-width, length, 0f) / 2f;
                    CoreTransform.localPosition = new Vector3(0f, length, 0f) / 2f;
                    InstructionTransform.localPosition = new Vector3(0f, length, 0f);
                }
            }
            else
            {
                TriggerTransform.localScale = new Vector3(width, maxlength, 5f);
                TriggerTransform.localPosition = new Vector3(0f, maxlength / 2f, 0f);
                Guide1Color.material.SetTextureScale("_MainTex", new Vector2(-maxlength, 1f));
                Guide2Color.material.SetTextureScale("_MainTex", new Vector2(-maxlength, 1f));
                Guide1Transform.localScale = new Vector3(maxlength, 1f, 1f) / 10f;
                Guide1Transform.localPosition = new Vector3(width, maxlength, 0f) / 2f;
                Guide2Transform.localScale = new Vector3(maxlength, 1f, 1f) / 10f;
                Guide2Transform.localPosition = new Vector3(-width, maxlength, 0f) / 2f;
                if (Target)
                {
                    Vector3 RPCancel = ReP + ReV;
                    RPCancel.y = Mathf.Max(RPCancel.y, 0f);
                    Vector3 GrabForce = CoreTransform.TransformDirection(RPCancel) * 10f;
                    float totalmass = 0f;
                    foreach (Part p in Target.parts)
                    {
                        if ((p.physicalSignificance == Part.PhysicalSignificance.FULL) && (p.rb != null))
                        {
                            p.AddForce(GrabForce * p.rb.mass);
                            totalmass += p.rb.mass;
                        }
                    }

                    foreach (Part p in this.vessel.parts)
                    {
                        if ((p.physicalSignificance == Part.PhysicalSignificance.FULL) && (p.rb != null))
                        {
                            p.AddForce(-GrabForce * (float)this.vessel.totalMass / totalmass);//Reaction
                        }
                    }

                    double consumption = GrabForce.magnitude * totalmass * Time.fixedDeltaTime;
                    double ECconsumtion = this.part.RequestResource("ElectricCharge", consumption);

                    if (Math.Round(consumption,4) != 0 && Math.Round(ECconsumtion,4) < Math.Round(consumption,4))
                    {
                        Deactivate();
                    }

                    if (!IsBreak)
                    {
                        Deactivate();
                    }

                    if (ReP.magnitude <= 0.1f && ReV.magnitude <= 0.5f)
                    {
                        Deactivate();
                    }
                }
            }
            Guide1Color.material.SetColor("_TintColor", LaunchColor);
            Guide2Color.material.SetColor("_TintColor", LaunchColor);
            InstructionColor.material.SetColor("_TintColor", LaunchColor);
        }
        else
        {
            Guide1Color.material.SetColor("_TintColor", Color.black);
            Guide2Color.material.SetColor("_TintColor", Color.black);
            InstructionColor.material.SetColor("_TintColor", Color.black);
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (!IsReseting && !IsReady && !IsGrabing && !IsLaunching && IsActivated)
        {
            Part IRpart = other.gameObject.GetComponentInParent<Part>();
            if (IRpart && IRpart.vessel && IRpart.vessel != this.vessel)
            {
                if (!IsLanding)
                {
                    //Debug.Log("Enter");
                    IsGrabing = true;
                    Target = IRpart.vessel;
                }
                else if (IRpart.vessel.ActionGroups.groups[5])
                {
                    IsGrabing = true;
                    Target = IRpart.vessel;
                }
            }
        }
    }
}
