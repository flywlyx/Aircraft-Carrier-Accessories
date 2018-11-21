using UnityEngine;

public class FixPoint : PartModule
{
    [KSPField]
    private Transform TieTransform = null;
    private Transform FPTransform = null;
    public bool IsHooked = false;
    public bool IsBreak = false;
    public Part LinkedPart = null;
    public float CableLength = 0f;
    public Tiedown Tiedown = null;
    [KSPField(isPersistant = true)]
    public bool IsActivated = true;

    [KSPField(guiActive = true, guiActiveEditor = true, guiName = "Cable Preload", isPersistant = true), UI_FloatRange(minValue = 0f, maxValue = 5f, stepIncrement = 0.25f)]
    public float Preload = 0f;



    [KSPAction("Toggle FixPoint", KSPActionGroup.None)]
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
        LinkedPart = null;
        Tiedown = null;
    }

    public override void OnStart(PartModule.StartState state)
    {
        this.enabled = true;
        this.part.force_activate();
        FPTransform = base.part.FindModelTransform("FixPointTransform");
    }

    public override void OnFixedUpdate()
    {


        if (IsActivated && LinkedPart != null && Tiedown.IsActivated)
        {
            if (!IsConnected(LinkedPart))
            {
                TieTransform = null;
                LinkedPart = null;
                Tiedown = null;
            }

            else
            {
                Color ropeColor = Color.green;
                float RelV = Vector3.Magnitude(this.part.rb.velocity - LinkedPart.rb.velocity);
                if (this.vessel.ActionGroups.groups[5])
                {                    
                    IsBreak = true;
                    if (!IsHooked)
                    {
                        if (RelV < 0.2f)
                        {
                            IsHooked = true;
                            CableLength = Vector3.Distance(LinkedPart.rb.position, this.part.rb.position);
                        }
                    }
                    else
                    {
                        ropeColor = new Color(1, 0.6f, 1, 1);
                        //Debug.Log(CableLength);
                        float LengthDif = Vector3.Distance(LinkedPart.rb.position, this.part.rb.position) - CableLength;
                        if (LengthDif > 0)
                        {
                            //Debug.Log(LengthDif);
                            this.part.AddForce((LinkedPart.rb.position - this.part.rb.position).normalized * (LengthDif * 1000+Preload * 10) * (float)this.vessel.totalMass);
                            LinkedPart.AddForce(-(LinkedPart.rb.position - this.part.rb.position).normalized * (LengthDif * 1000 + Preload * 10) * (float)this.vessel.totalMass);
                        }
                        else
                        {
                            CableLength = Mathf.Max(1,Vector3.Distance(LinkedPart.rb.position, this.part.rb.position));
                        }
                    }
                }
                else
                {
                    IsBreak = false;
                    ropeColor = Color.green;
                    if (IsHooked || Vector3.Distance(LinkedPart.rb.position, this.part.rb.position) > Tiedown.MaxWireLength)
                    {
                        IsHooked = false;
                        TieTransform = null;
                        LinkedPart = null;
                        Tiedown = null;
                    }
                }

                //IsHooked = true;
                LineRenderer rope = this.part.gameObject.GetComponent<LineRenderer>();
                if (TieTransform != null)
                {
                    if (rope == null)
                    {
                        rope = this.part.gameObject.AddComponent<LineRenderer>();
                        ropeColor.a = ropeColor.a / 2;
                        rope.material = new Material(Shader.Find("KSP/Particles/Alpha Blended"));
                        rope.material.SetColor("_TintColor", ropeColor);
                        rope.material.mainTexture = GameDatabase.Instance.GetTexture("KFC/Textures/chain", false);
                        rope.material.SetTextureScale("_MainTex", new Vector2(Vector3.Distance(LinkedPart.rb.position, this.part.rb.position) * 4, 1f));
                        rope.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                        rope.receiveShadows = false;
                        rope.startWidth=0.25f;
                        rope.endWidth = 0.25f;
                        rope.SetPosition(0, TieTransform.position + LinkedPart.rb.velocity * Time.fixedDeltaTime);
                        rope.SetPosition(1, FPTransform.position + part.rb.velocity * Time.fixedDeltaTime);
                        rope.useWorldSpace = true;
                        rope.enabled = true;

                    }
                    else
                    {
                        rope.SetPosition(0, TieTransform.position + LinkedPart.rb.velocity * Time.fixedDeltaTime);
                        rope.SetPosition(1, FPTransform.position + part.rb.velocity * Time.fixedDeltaTime);
                        ropeColor.a = ropeColor.a / 2;
                        rope.material.SetColor("_TintColor", ropeColor);
                        rope.material.SetTextureScale("_MainTex", new Vector2(Vector3.Distance(LinkedPart.rb.position, this.part.rb.position) * 4, 1f));
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
        if (!IsHooked && IsActivated)
        {
            Part IRpart = other.gameObject.GetComponentInParent<Part>();
            if (IRpart)
            {
                float RelV = Vector3.Magnitude(this.part.rb.velocity - IRpart.rb.velocity);
                if (IRpart.vessel && IRpart.vessel != this.vessel && RelV < 5)
                {
                    if (IsTiedown(IRpart))
                    {
                        if (LinkedPart == null)
                        {
                            LinkedPart = IRpart;
                            TieTransform = LinkedPart.FindModelTransform("TieTransform");
                        }

                        else if (Vector3.Distance(IRpart.transform.position, this.part.transform.position) < Vector3.Distance(LinkedPart.transform.position, this.part.transform.position))
                        {
                            LinkedPart = IRpart;
                            TieTransform = LinkedPart.FindModelTransform("TieTransform");
                        }
                    }
                }
            }
        }

    }

    private bool IsTiedown(Part IRpart)
    {
        foreach (PartModule m in IRpart.Modules)
        {
            if (m.moduleName == "Tiedown")
            {
                Tiedown = (Tiedown)m;
                if (IsConnected(IRpart)&&Tiedown.IsActivated)
                {
                    return true;
                }
            }
        }
        return false;
    }

    private bool IsConnected(Part IRpart)
    {
                Ray ray = new Ray(FPTransform.position, IRpart.transform.position - FPTransform.position);
                RaycastHit rayHit;
                if (Physics.Raycast(ray, out rayHit, Vector3.Magnitude(FPTransform.position - IRpart.transform.position), 2097153))
                {
                    Part Obstacle = rayHit.collider.GetComponentInParent<Part>();
                    if (Obstacle == IRpart)
                    {
                        return true;
                    }
                }
        return false;
    }
}

