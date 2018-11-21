//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using UnityEngine;

//public class LandZone : PartModule
//{
//    [KSPField]
//    public float maxlength = 90f;
//    public float TargetDA = 0f;
//    public float BlockA = 0f;
//    private Transform LandZoneTransform = null;
//    public bool IsActivated = true;
//    public bool IsReseting = false;
//    public bool IsGrabing = false;
//    public bool IsReady = false;
//    public bool IsLaunching = false;
//    public bool IsBreak = false;
//    public ModuleWheelBase Wheel = null;
//    public MeshRenderer CatapultColor = null;
//    public float time = 0f;
//    private PidController xPid = new PidController(50F, 0.005F, 0.005F, 50, 50);
//    private PidController yPid = new PidController(50F, 0.005F, 0.005F, 50, 50);
//    private PidController zPid = new PidController(50F, 0.005F, 0.005F, 50, 50);

//    [KSPAction("Toggle", KSPActionGroup.None, guiName = "Toggle Zone")]
//    private void ActionActivate(KSPActionParam param)
//    {
//        if (!IsActivated)
//        {
//            Activate();
//        }
//        else
//        {
//            Deactivate();
//        }
//    }

//    [KSPEvent(name = "Activate", guiName = "Activate", active = true, guiActive = false)]
//    public void Activate()
//    {
//        IsActivated = true;
//        Events["Deactivate"].guiActive = true;
//        Events["Activate"].guiActive = false;
//    }

//    [KSPEvent(name = "Deactivate", guiName = "Deactivate", active = true, guiActive = true)]
//    public void Deactivate()
//    {
//        IsActivated = false;
//        Events["Deactivate"].guiActive = false;
//        Events["Activate"].guiActive = true;
//        IsReseting = false;
//        IsGrabing = false;
//        IsReady = false;
//        IsLaunching = false;
//        IsBreak = false;
//    }


//    public override void OnStart(PartModule.StartState state)
//    {
//        if (HighLogic.LoadedSceneIsFlight)
//        {
//            this.enabled = true;
//            this.part.force_activate();
//            LandZoneTransform = base.part.FindModelTransform("LandZone");
//        }        
//    }

//    public override void OnFixedUpdate()
//    {
//        if (HighLogic.LoadedSceneIsFlight && !LandZoneTransform)
//        {
//            LandZoneTransform = base.part.FindModelTransform("LandZone");
//        }
//        Color LZColor = Color.green;
//        if (IsActivated)
//        {
//            if (Wheel)
//            {
//                if (Wheel.vessel.ActionGroups.groups[5])
//                {
//                    IsBreak = true;
//                }
//                else
//                {
//                    IsBreak = false;
//                }
//            }

//            if (!CatapultColor)
//            {
//                CatapultColor = LandZoneTransform.gameObject.GetComponent(typeof(MeshRenderer)) as MeshRenderer;
//            }

//            if (IsGrabing)
//            {
//                //Debug.Log(LandZoneTransform.localPosition);
                
//                LZColor = Color.yellow;
//                if (!IsLaunching)
//                {
//                    foreach (Part p in Wheel.vessel.parts)
//                    {
//                        if ((p.physicalSignificance == Part.PhysicalSignificance.FULL) && (p.rb != null))
//                        {
//                            p.AddForce(GrabForce * 10f * p.rb.mass);
//                        }
//                    }
//                }
//                else
//                {
//                    Wheel.part.AddForceAtPosition(GrabForce * VesselMass * 10f, WheelTransform.position);
//                }

//                if (IsBreak && !IsReady && !IsLaunching)
//                {
//                    IsReady = true;
//                }
//            }

//            if (IsReady)
//            {
//                LZColor = Color.yellow;
//                if (!IsBreak && !IsLaunching)
//                {
//                    IsLaunching = true;
//                    IsGrabing = false;
//                }
//            }

//            if (IsLaunching)
//            {
//                float Acceleration = Mathf.Pow(TargetV, 2f) / (2f * maxlength);
//                //Debug.Log(Acceleration);
//                float VesselMass = Wheel.vessel.GetTotalMass();
//                time += Time.fixedDeltaTime;
//                LZColor = Color.green;
//                LandZoneTransform.localPosition = LandZoneTransform.localPosition + Vector3.right * Acceleration * time * Time.fixedDeltaTime;
//                Vector3 LaunchReP = LandZoneTransform.InverseTransformDirection(LandZoneTransform.position - WheelTransform.position);
//                Vector3 LaunchReV = LandZoneTransform.InverseTransformDirection(this.part.rb.velocity - WheelTransform.gameObject.GetComponentInParent<Part>().rb.velocity);
//                Vector3 LaunchReC = LaunchReV + LaunchReP;
//                Vector3 LaunchGrabForce = LandZoneTransform.TransformDirection(0f, LaunchReC.y, 0f);
//                Vector3 AccForce = LandZoneTransform.right * Acceleration;
//                Wheel.part.AddForceAtPosition((AccForce + LaunchGrabForce) * VesselMass, WheelTransform.position);

//                if (LandZoneTransform.localPosition.x > maxlength || IsBreak)
//                {
//                    IsReseting = true;
//                    IsGrabing = false;
//                    IsReady = false;
//                    IsLaunching = false;
//                    Wheel = null;
//                    WheelTransform = null;
//                    LZColor = Color.white;
//                }
//            }

//            if (IsReseting)
//            {
//                LandZoneTransform.localPosition = Vector3.zero;
//                IsReseting = false;
//                time = 0f;
//            }
//        }
//        else
//        {
//            LZColor = Color.grey;
//        }
//        if (CatapultColor)
//        {
//            CatapultColor.material.SetColor("_Color", LZColor);
//        }
//    }

//    void OnTriggerStay(Collider other)
//    {
//        //Debug.Log("Arrive");
//        if (IsActivated)
//        {
//            Part IRpart = other.gameObject.GetComponentInParent<Part>();
//            if (IRpart && IRpart.vessel && IRpart.vessel != this.vessel && IsWheel(IRpart) == true)
//            {
//                //Debug.Log("Enter");
//                IsGrabing = true;
//                Transform WheelTransform = IRpart.FindModelTransform(Wheel.wheelTransformName);
//                float VesselMass = Wheel.vessel.GetTotalMass();
//                Vector3 ReP = LandZoneTransform.InverseTransformDirection(LandZoneTransform.position - WheelTransform.position);
//                Vector3 ReV = LandZoneTransform.InverseTransformDirection(this.part.rb.velocity - WheelTransform.gameObject.GetComponentInParent<Part>().rb.velocity);
//                if (IsLaunching)
//                {
//                    ReV = Vector3.zero;
//                }
//                Vector3 GrabForce = LandZoneTransform.TransformDirection(ReV.x, -1f, ReV.z);

//            }
//        }
//    }

//    private bool IsWheel(Part IRpart)
//    {
//        foreach (PartModule m in IRpart.Modules)
//        {
//            if (m.moduleName == "ModuleWheelBase")
//            {
//                Wheel = (ModuleWheelBase)m;
//                if (Wheel.isGrounded)
//                {
//                    //Debug.Log("Contact");
//                    return true;
//                }
//            }
//            else
//                if (m.moduleName == "KSPWheelBase")
//                {

//                }
//        }
//        return false;
//    }
//}
