using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class Tiedown : PartModule
{
    [KSPField]
    public float wiretension = 1f;
    private Transform TieTransform = null;
    private Transform FPTransform = null;
    private Transform triggerTransform = null;
    public bool IsHooked = false;
    public Part FP = null;
    [KSPField(isPersistant = true)]
    public bool Ispassive = true;
    [KSPField(isPersistant = true)]
    public bool IsActivated = true;

    [KSPField(guiActive = true, guiActiveEditor = true, guiName = "Force Ratio", isPersistant = true), UI_FloatRange(minValue = 1f, maxValue = 10f, stepIncrement = 1f)]
    public float ForceRatio = 1f;


    [KSPField(guiActive = true, guiActiveEditor = true, guiName = "WireLength", isPersistant = true), UI_FloatRange(minValue = 1f, maxValue = 10f, stepIncrement = 1f)]
    public float MaxWireLength = 1f;

    [KSPAction("Toggle TieDown", KSPActionGroup.None)]
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
        TieTransform = null;
        IsHooked = false;
    }

    public override void OnStart(PartModule.StartState state)
    {
        this.enabled = true;
        this.part.force_activate();
        TieTransform = base.part.FindModelTransform("TieTransform");
        triggerTransform = base.part.FindModelTransform("triggerTransform");
    }

    public override void OnFixedUpdate()
    {
        if (IsActivated)
        {
            triggerTransform.localScale = Vector3.one * MaxWireLength;
        }
        else
        {
            triggerTransform.localScale = Vector3.zero;
        }

        if (IsHooked)
        {
            LineRenderer rope = this.part.gameObject.GetComponent<LineRenderer>();
            if (rope == null)
            {
                rope = this.part.gameObject.AddComponent<LineRenderer>();
                Color ropeColor = Color.grey;
                ropeColor.a = ropeColor.a / 2;
                rope.material = new Material(Shader.Find("KSP/Alpha/Cutoff"));
                //rope.material.SetColor("_TintColor", ropeColor);
                rope.material.mainTexture = GameDatabase.Instance.GetTexture("KFC/Textures/chain", false);
                rope.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                rope.receiveShadows = false;
                rope.widthCurve = new AnimationCurve(new Keyframe(0, 0.1f), new Keyframe(1, 0.1f));
                rope.positionCount = 2;
                rope.SetPosition(0, TieTransform.position + part.rb.velocity * Time.fixedDeltaTime);
                rope.SetPosition(1, FPTransform.position + part.rb.velocity * Time.fixedDeltaTime);
                rope.useWorldSpace = true;
                rope.enabled = true;

            }
            else
            {
                rope.SetPosition(0, TieTransform.position + part.rb.velocity * Time.fixedDeltaTime);
                rope.SetPosition(1, FPTransform.position + part.rb.velocity * Time.fixedDeltaTime);
                rope.enabled = true;
            }
        }
        else
        {
            LineRenderer hit = this.part.gameObject.GetComponent<LineRenderer>();
            if (hit != null)
            {
                Destroy(hit);
            }
        }
    }
    void OnTriggerStay(Collider other)
    {
        //if (!IsHooked)
        //{
        //    Part IRpart = other.gameObject.GetComponentInParent<Part>();
        //    if (IRpart)
        //    {
        //        float RelV = TieTransform.InverseTransformDirection(this.part.rb.velocity - IRpart.rb.velocity).magnitude;
        //        if (IRpart && IRpart.vessel && IRpart.vessel != this.vessel && RelV < 0.2f)
        //        {
        //        }
        //    }
        //}

    }

}
