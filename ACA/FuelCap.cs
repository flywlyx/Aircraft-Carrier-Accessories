using System;
using System.Collections.Generic;
using UnityEngine;

public class FuelCap: PartModule
{
    [KSPField]
    private Transform PipeTransform = null;
    private Transform ConnectorTransform = null;
    private Transform CapTransform = null;
    private Transform RopeTransform = null;
    public bool IsHooked = false;
    public bool IsBreak = false;
    public Part LinkedPart = null;
    public float CableLength = 0f;
    public FuelNozzle Nozzle = null;
    public MeshRenderer RopeRender = null;
    private List<PartResource> ResourceList = null;

    [KSPField(isPersistant = true)]
    public bool IsActivated = true;

    [KSPField(isPersistant = true)]
    public float Speed = 20f;

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
        PipeTransform = null;
        IsHooked = false;
        LinkedPart = null;
        Nozzle = null;
    }

    public override void OnStart(PartModule.StartState state)
    {
        this.enabled = true;
        this.part.force_activate();
        CapTransform = base.part.FindModelTransform("CapTransform");
        ConnectorTransform = base.part.FindModelTransform("Connector");
        RopeTransform = base.part.FindModelTransform("Rope");
        RopeRender = RopeTransform.gameObject.GetComponent(typeof(MeshRenderer)) as MeshRenderer;
    }

    public override void OnFixedUpdate()
    {

        if (IsActivated && LinkedPart != null && Nozzle.IsActivated)
        {
            if (!IsConnected(LinkedPart))
            {
                PipeTransform = null;
                LinkedPart = null;
                Nozzle = null;
                ConnectorTransform.localScale = Vector3.zero;
            }

            else
            {
                float RelV = Vector3.Magnitude(this.part.rb.velocity - LinkedPart.rb.velocity);
                CableLength = Vector3.Distance(PipeTransform.position + LinkedPart.vel * Time.fixedDeltaTime, CapTransform.position);
                if (this.vessel.ActionGroups.groups[5])
                {                    
                    IsBreak = true;
                    if (!IsHooked)
                    {
                        if (RelV < 0.2f)
                        {
                            IsHooked = true;
                           
                        }
                    }
                    else
                    {

                        //Debug.Log(CableLength);
                        if (PipeTransform != null)
                        {
                            ConnectorTransform.localScale = Vector3.one * 1.6f;
                            Vector3 ReP = PipeTransform.position + LinkedPart.vel * Time.fixedDeltaTime - CapTransform.position;
                            Quaternion to = Quaternion.FromToRotation(CapTransform.up, ReP.normalized);
                            CapTransform.rotation = Quaternion.Slerp(CapTransform.rotation, to * CapTransform.rotation, 1);
                            CapTransform.localScale = new Vector3(1, CableLength * 2.5f, 1);
                            RopeRender.material.SetTextureScale("_MainTex", new Vector2(1f, CableLength * 2.5f));
                            Nozzle.IsHooked = true;
                            if (ResourceList == null)
                            {
                                ResourceList = new List<PartResource>();
                            }

                            foreach (Part p in this.vessel.Parts)
                            {
                                if (p.Resources.Count!= 0)
                                {
                                    foreach (PartResource r in p.Resources)
                                    {
                                        if (!ResourceList.Contains(r))
                                        {
                                            ResourceList.Add(r);
                                        }
                                    }
                                }
                                else
                                if(p.children.Count ==0)
                                {
                                    foreach (PartModule m in p.Modules)
                                    {
                                        if (m.moduleName == "ReloadableRail")
                                        {
                                            ReloadableRail RL = (ReloadableRail)m;
                                            RL.Resupply();
                                        }
                                    }
                                }
                            }
                            foreach (PartResource r in ResourceList)
                            {
                                Double targetr = 0;
                                Double targetmaxr = 0;
                                Double requestr = 0;
                                Double requestmaxr = 0;
                                LinkedPart.GetConnectedResourceTotals(r.info.id,out targetr, out targetmaxr);
                                if (targetr != 0)
                                {
                                    this.part.GetConnectedResourceTotals(r.info.id, out requestr, out requestmaxr);
                                    if(requestr< requestmaxr)
                                    {
                                        Double Rout= LinkedPart.RequestResource(r.info.id, (Double)Mathf.Min((float)requestmaxr- (float)requestr, (float)targetr, Speed * Time.fixedDeltaTime));
                                        this.part.RequestResource(r.info.id, -Rout);
                                    }
                                }
                            }
                        }
                        else
                        {
                            ConnectorTransform.localScale = Vector3.zero;
                        }
                    }
                }
                else
                {
                    IsBreak = false;
                    if (IsHooked || Vector3.Distance(LinkedPart.rb.position, this.part.rb.position) > Nozzle.MaxWireLength)
                    {
                        IsHooked = false;
                        PipeTransform = null;
                        LinkedPart = null;
                        Nozzle = null;
                        ConnectorTransform.localScale = Vector3.zero;
                    }
                }

                //IsHooked = true;

            }

        }
        else
        {
            ConnectorTransform.localScale = Vector3.zero;
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
                    if (IsNozzle(IRpart))
                    {
                        if (LinkedPart == null)
                        {
                            LinkedPart = IRpart;
                            PipeTransform = LinkedPart.FindModelTransform("PipeTransform");
                        }

                        else if (Vector3.Distance(IRpart.transform.position, this.part.transform.position) < Vector3.Distance(LinkedPart.transform.position, this.part.transform.position))
                        {
                            LinkedPart = IRpart;
                            PipeTransform = LinkedPart.FindModelTransform("PipeTransform");
                        }
                    }
                }
            }
        }

    }

    private bool IsNozzle(Part IRpart)
    {
        foreach (PartModule m in IRpart.Modules)
        {
            if (m.moduleName == "FuelNozzle")
            {
                Nozzle = (FuelNozzle)m;
                if (IsConnected(IRpart)&& Nozzle.IsActivated)
                {
                    return true;
                }
            }
        }
        return false;
    }

    private bool IsConnected(Part IRpart)
    {
                Ray ray = new Ray(CapTransform.position, IRpart.transform.position - CapTransform.position);
                RaycastHit rayHit;
                if (Physics.Raycast(ray, out rayHit, Vector3.Magnitude(CapTransform.position - IRpart.transform.position), 2097153))
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
