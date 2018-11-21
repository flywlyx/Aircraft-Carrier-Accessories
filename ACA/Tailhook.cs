using UnityEngine;
using System.Collections.Generic;
using System;

public class Tailhook : PartModule
{
    [KSPField]
    public float maxwiretension = 100f;
    public float wiretension = 100f;
    public float TargetDA = 0f;
    public float BlockA = 0f;
    private Transform HookTransform = null;
    private Transform ARP1Transform = null;
    private Transform ARP2Transform = null;
    public bool IsDeploy = false;
    public bool IsHooked = false;
    public bool IsBlocked = false;
    private Arrestwire AW = null;
    public AnimationState[] deployStates;
    [KSPField(isPersistant = false)]
    public string deployAnimName = "HookAnim";
    [KSPField(isPersistant = true)]
    public float MaxDA = 60f;


    [KSPField(guiActive = true, guiActiveEditor = false, guiName = "Deploy Angle", isPersistant = true), UI_FloatRange(minValue = 0f, maxValue = 90f, stepIncrement = 5f)]
    public float DeployAngle = 45f;

    [KSPAction("Toggle Hook", KSPActionGroup.None)]
    private void ActionActivate(KSPActionParam param)
    {
        if (IsDeploy == false)
        {
            HookDeploy();
        }
        else
        {
            HookRetract();
        }
    }

    [KSPEvent(name = "HookDeploy", guiName = "Deploy Hook", active = true, guiActive = true)]
    public void HookDeploy()
    {
        IsDeploy = true;
        Events["HookRetract"].guiActive = true;
        Events["HookDeploy"].guiActive = false;
        wiretension = maxwiretension;
        Debug.Log("Hook Deployed");
    }

    [KSPEvent(name = "HookRetract", guiName = "Retract Hook", active = true, guiActive = false)]
    public void HookRetract()
    {
        IsDeploy = false;
        Events["HookRetract"].guiActive = false;
        Events["HookDeploy"].guiActive = true;
        IsHooked = false;
        wiretension = maxwiretension;
        LineRenderer rope = this.part.gameObject.GetComponent<LineRenderer>();
        if (rope)
        {
            Destroy(rope);
        }
        if (AW)
        {
            AW.IsHooked = false;
            AW = null;
        }
    }

    public override void OnStart(PartModule.StartState state)
    {
        this.enabled = true;
        this.part.force_activate();
        if(base.part.FindModelTransform("HookTransform"))
        {
            HookTransform = base.part.FindModelTransform("HookTransform");
        }
        else
        {
            Debug.Log("Missing HookTransform");
        }

        if (deployAnimName != "")
        {
            deployStates = SetUpAnimation(deployAnimName, this.part);
            foreach (AnimationState anim in deployStates)
            {
                anim.enabled = false;
                anim.speed = 0;
                anim.normalizedTime = 0;
            }
            
        }
    }

    public override void OnFixedUpdate()
    {
        if (IsDeploy)
        {          
                if (IsHooked)
            {
                TargetDA = 15;

                if (ARP1Transform != null && ARP2Transform != null)
                {
                    //if (!IsConenected(HookTransform, ARP1Transform) || !IsConenected(HookTransform, ARP2Transform))
                    //{
                    //    HookRetract();
                    //}
                    LineRenderer rope = this.part.gameObject.GetComponent<LineRenderer>();
                    Vector3 Rope1 = ARP1Transform.position - HookTransform.position;
                    Vector3 Rope2 = ARP2Transform.position - HookTransform.position;
                    Vector3 ReV = AW.part.vessel.GetSrfVelocity() - this.part.vessel.GetSrfVelocity();
                    Vector3 Forcedirection = Vector3.Normalize(ReV);
                    if (Vector3.Dot(ReV, Rope1 + Rope2) < 0)
                    {
                        wiretension = 0.5F;
                    }
                    else
                    {
                        wiretension = maxwiretension;
                    }
                    part.AddForce(Forcedirection * Mathf.Min(wiretension * Vector3.Magnitude(Rope1 + Rope2) * AW.ForceRatio, (float)this.vessel.totalMass * 42f * AW.ForceRatio));
                    //Debug.Log(wiretension);
                    //Debug.Log(Vector3.Magnitude(Forcedirection * Mathf.Min(wiretension*Vector3.Magnitude(Rope1 + Rope2), (float)this.vessel.totalMass*42)));
                    if (!rope)
                    {
                        rope = this.part.gameObject.AddComponent<LineRenderer>();
                        Color ropeColor = Color.white;
                        rope.material = new Material(Shader.Find("KSP/Diffuse"));
                        rope.material.SetColor("_BurnColor", ropeColor);
                        rope.material.mainTexture = GameDatabase.Instance.GetTexture("KFC/Textures/SteelWire", false);
                        rope.material.mainTextureScale = Vector2.right;
                        rope.shadowCastingMode = 0;
                        rope.receiveShadows = false;
                        rope.widthCurve = new AnimationCurve(new Keyframe(0, 0.1f), new Keyframe(1, 0.1f));
                        rope.positionCount = 3;
                        rope.SetPosition(0, ARP1Transform.position + AW.part.rb.velocity * Time.fixedDeltaTime);
                        rope.SetPosition(1, HookTransform.position + part.rb.velocity * Time.fixedDeltaTime);
                        rope.SetPosition(2, ARP2Transform.position + AW.part.rb.velocity * Time.fixedDeltaTime);
                        rope.material.SetTextureScale("_MainTex", new Vector2((Vector3.Distance(ARP1Transform.position, HookTransform.position) + Vector3.Distance(ARP2Transform.position, HookTransform.position)) * 20, 1f));
                        rope.useWorldSpace = true;
                        rope.enabled = true;
                    }
                    else
                    {
                        rope.positionCount = 3;
                        rope.SetPosition(0, ARP1Transform.position + AW.part.rb.velocity * Time.fixedDeltaTime);
                        rope.SetPosition(1, HookTransform.position + part.rb.velocity * Time.fixedDeltaTime);
                        rope.SetPosition(2, ARP2Transform.position + AW.part.rb.velocity * Time.fixedDeltaTime);
                        rope.material.SetTextureScale("_MainTex", new Vector2((Vector3.Distance(ARP1Transform.position, HookTransform.position) + Vector3.Distance(ARP2Transform.position, HookTransform.position)) * 20, 1f));
                        rope.enabled = true;
                    }
                }
            }
                else
            {
                TargetDA = DeployAngle;
            }
            if (deployAnimName != "")
            {
                foreach (AnimationState anim in deployStates)
                {
                    //animation clamping
                    anim.enabled = true;
                    if (anim.normalizedTime < 0.5f + TargetDA / 180 && anim.speed < 1)
                    {
                        anim.speed = 1;
                    }
                    if (anim.normalizedTime == 0.5f + TargetDA / 180)
                    {
                        anim.enabled = false;
                    }
                    if (anim.normalizedTime > 0.5f + TargetDA / 180)
                    {
                        anim.speed = 0;
                        anim.normalizedTime = 0.5f + TargetDA / 180;
                        anim.enabled = false;
                    }
                    if (anim.normalizedTime < 0)
                    {
                        anim.speed = 0;
                        anim.normalizedTime = 0;
                    }

                    //deploying

                }
            }
        }
        else
        {
            foreach (AnimationState anim in deployStates)
            {
                //animation clamping
                if (anim.normalizedTime > 1)
                {
                    anim.speed = 0;
                    anim.normalizedTime = 1;
                }
                if (anim.normalizedTime < 0)
                {
                    anim.speed = 0;
                    anim.normalizedTime = 0;
                }
                anim.enabled = true;
                if (anim.normalizedTime > 0 && anim.speed > -1)
                {
                    anim.speed = -1;
                }
                if (anim.normalizedTime == 0)
                {
                    anim.enabled = false;
                }
            }
        }

    }

    void OnTriggerStay(Collider other)
    {
        if (IsDeploy && !IsHooked && other.gameObject.GetComponentInParent<Part>())
        {
            Part IRpart = other.gameObject.GetComponentInParent<Part>();
            //Debug.Log("Trigger touch" + IRpart.name);
            if (IRpart && IRpart.vessel && IRpart.vessel != this.vessel && IsWire(IRpart) == true)
            {
                Debug.Log("Hook Wire!");
                IsHooked = true;
                ARP1Transform = IRpart.FindModelTransform("ARP1");
                ARP2Transform = IRpart.FindModelTransform("ARP2");
                LineRenderer rope = this.part.gameObject.GetComponent<LineRenderer>();
                if (!rope)
                {
                    rope = this.part.gameObject.AddComponent<LineRenderer>();
                    Color ropeColor = Color.white;
                    rope.material = new Material(Shader.Find("KSP/Diffuse"));
                    rope.material.SetColor("_BurnColor", ropeColor);
                    rope.material.mainTexture = GameDatabase.Instance.GetTexture("KFC/Textures/SteelWire", false);
                    rope.shadowCastingMode = 0;
                    rope.receiveShadows = false;
                    rope.startWidth = 0.05f;
                    rope.endWidth = 0.05f;
                    rope.positionCount = 3;
                    rope.SetPosition(0, ARP1Transform.position);
                    rope.SetPosition(1, HookTransform.position);
                    rope.SetPosition(2, ARP2Transform.position);
                    rope.material.SetTextureScale("_MainTex", new Vector2((Vector3.Distance(ARP1Transform.position, HookTransform.position) + Vector3.Distance(ARP2Transform.position, HookTransform.position)) * 20, 1f));
                    rope.useWorldSpace = true;
                    rope.enabled = true;
                }
                else
                {
                    rope.positionCount = 3;
                    rope.SetPosition(0, ARP1Transform.position);
                    rope.SetPosition(1, HookTransform.position);
                    rope.SetPosition(2, ARP2Transform.position);
                    rope.material.SetTextureScale("_MainTex", new Vector2((Vector3.Distance(ARP1Transform.position, HookTransform.position) + Vector3.Distance(ARP2Transform.position, HookTransform.position)) * 20, 1f));
                    rope.enabled = true;
                }
            }
            else
                if (IRpart && IRpart.vessel && IRpart.vessel != this.vessel && !IsWire(IRpart))
                {
                    IsBlocked = true;
                }
        }

      }

    private bool IsWire(Part IRpart)
    {
        foreach (PartModule m in IRpart.Modules)
            {
                if (m.moduleName == "Arrestwire")
                {
                AW = (Arrestwire)m;
                    if (!AW.IsHooked)
                    {
                        AW.IsHooked = true;
                        Debug.Log("Hook");
                        return true;
                    }
                }
            }
        return false;
    }

    public static AnimationState[] SetUpAnimation(string animationName, Part part)  //Thanks Majiir!
    {
        var states = new List<AnimationState>();
        foreach (var animation in part.FindModelAnimators(animationName))
        {
            var animationState = animation[animationName];
            animationState.speed = 0;
            animationState.enabled = true;
            animationState.wrapMode = WrapMode.ClampForever;
            animation.Blend(animationName);
            states.Add(animationState);
        }
        return states.ToArray();
    }
    //private bool IsConenected (Transform Hook, Transform Pile)
    //{
    //    Ray ray = new Ray(Hook.position, Pile.position - Hook.position);
    //    RaycastHit rayHit;
    //    if (Physics.Raycast(ray, out rayHit, Vector3.Magnitude(Pile.position - Hook.position), 557057))
    //    {
    //        Part Obstacle = rayHit.collider.GetComponentInParent<Part>();
    //        if (Obstacle && Obstacle.vessel != this.part.vessel && Obstacle.vessel && !IsWire(Obstacle))
    //        {
    //            if (Obstacle.crashTolerance > 6400f)
    //            {
    //                return false;
    //            }
    //            else
    //            {
    //                Obstacle.temperature = Obstacle.maxTemp + 100;
    //            }

    //        }
    //        else
    //            if(!Obstacle.vessel && !Obstacle)
    //            {
    //                return false;
    //            }
    //        return true;

    //    }
    //    return true;


    //}
}
