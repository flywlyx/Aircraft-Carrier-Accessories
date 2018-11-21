using UnityEngine;

public class OLS : PartModule
{
    [KSPField]
    public float maxlength = 90f;
    public float TargetDA = 0f;
    public float BlockA = 0f;
    private Transform VerticalTransform = null;
    private Transform HorizontalTransform = null;
    private Transform IdealTransform = null;
    private Transform IndicatorTransform = null;
    private Transform TargetTransform = null;
    private Transform DirectorTransform = null;
    private Transform OLSZoneTransform = null;
    private Transform LEDLightTransform = null;
    private Transform LightsTransform = null;
    public bool IsTriggered = false;
    public bool IsWorking = false;
    public MeshRenderer IndicatorColor = null;
    public float time = 0f;
    public Vessel TargetVessel = null;
    public LineRenderer Director = null;
    public LineRenderer IdealLine= null;
    public LineRenderer Vertical = null;
    public LineRenderer Horizontal = null;
    public MeshRenderer LightColor = null;
    public float vtimer = 0f;
    public float htimer = 0f;
    public float vflash = 0f;
    public float hflash = 0f;
    Color VerticalColor = Color.yellow;
    Color HorizontalColor = Color.yellow;

    [KSPField(isPersistant = true)]
    public bool IsActivated = true;


    [KSPAction("Toggle", KSPActionGroup.None, guiName = "Toggle OLS")]
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
        IsTriggered = false;
        IsWorking = false;
    }

    [KSPField(guiActive = true, guiActiveEditor = true, guiName = "Range", isPersistant = true), UI_FloatRange(minValue = 1f, maxValue = 10f, stepIncrement = 1f)]
    public float ZRange = 1f;


    public override void OnStart(PartModule.StartState state)
    {
        this.enabled = true;
        this.part.force_activate();
        vtimer = 0;
        htimer = 0;
        VerticalTransform = base.part.FindModelTransform("Vertical");
        HorizontalTransform = base.part.FindModelTransform("Horizontal");
        IdealTransform = base.part.FindModelTransform("IdealTransform");
        IndicatorTransform = base.part.FindModelTransform("IndicatorTransform");
        DirectorTransform = base.part.FindModelTransform("Director");
        OLSZoneTransform = base.part.FindModelTransform("OLSZone");
        LEDLightTransform = base.part.FindModelTransform("LEDLight");
        LightsTransform = base.part.FindModelTransform("Lights");
        Director = DirectorTransform.gameObject.GetComponent<LineRenderer>();
        if (Director == null)
        {
            Director = DirectorTransform.gameObject.AddComponent<LineRenderer>();
            Color DirectorColor = Color.cyan;
            DirectorColor.a = DirectorColor.a / 2;
            Director.material = new Material(Shader.Find("KSP/Particles/Alpha Blended"));
            Director.material.SetColor("_TintColor", DirectorColor);
            Director.material.mainTexture = GameDatabase.Instance.GetTexture("KFC/Textures/wire", false);
            Director.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            Director.receiveShadows = false;
            Director.startWidth=0.5f;
            Director.endWidth=0.1f;
            Director.positionCount = 2;
            Director.SetPosition(0, DirectorTransform.position);
            Director.SetPosition(1, DirectorTransform.position);
            Director.useWorldSpace = true;
            Director.enabled = true;
        }
        else
        {
            Director.SetPosition(0, DirectorTransform.position);
            Director.SetPosition(1, DirectorTransform.position);
            Director.enabled = true;
        }
        //GameObject IdealLiner;
        //IdealLiner = new GameObject("IdealLiner");
        //IdealLine = IdealLiner.GetComponent<LineRenderer>();
        if (IdealLine == null)
        {
            IdealLine = IdealTransform.gameObject.AddComponent<LineRenderer>();
            Color IdealLineColor = Color.red;
            IdealLineColor.a = IdealLineColor.a / 2;
            IdealLine.material = new Material(Shader.Find("KSP/Particles/Alpha Blended"));
            IdealLine.material.SetColor("_TintColor", IdealLineColor);
            IdealLine.material.mainTexture = GameDatabase.Instance.GetTexture("KFC/Textures/wire", false);
            IdealLine.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            IdealLine.receiveShadows = false;
            IdealLine.startWidth = 0.5f;
            IdealLine.startWidth = 0.1f;
            IdealLine.positionCount = 2;
            IdealLine.SetPosition(0, IdealTransform.position);
            IdealLine.SetPosition(1, IdealTransform.position);
            IdealLine.useWorldSpace = true;
            IdealLine.enabled = true;
        }
        else
        {
            IdealLine.SetPosition(0, IdealTransform.position);
            IdealLine.SetPosition(1, IdealTransform.position);
            IdealLine.enabled = true;
        }

        if (Vertical == null)
        {
            Vertical = VerticalTransform.gameObject.AddComponent<LineRenderer>();
            VerticalColor.a = VerticalColor.a / 2;
            Vertical.material = new Material(Shader.Find("KSP/Particles/Alpha Blended"));
            Vertical.material.SetColor("_TintColor", VerticalColor);
            Vertical.material.mainTexture = GameDatabase.Instance.GetTexture("KFC/Textures/wire", false);
            Vertical.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            Vertical.receiveShadows = false;
            Vertical.startWidth=0.5f;
            Vertical.endWidth = 0.5f;
            Vertical.positionCount = 2;
            Vertical.SetPosition(0, VerticalTransform.position);
            Vertical.SetPosition(1, VerticalTransform.position);
            Vertical.useWorldSpace = true;
            Vertical.enabled = true;
        }
        else
        {
            Vertical.SetPosition(0, VerticalTransform.position);
            Vertical.SetPosition(1, VerticalTransform.position);
            Vertical.enabled = true;
        }

        if (Horizontal == null)
        {
            HorizontalColor.a = HorizontalColor.a / 2;
            Horizontal = HorizontalTransform.gameObject.AddComponent<LineRenderer>();
            Horizontal.material = new Material(Shader.Find("KSP/Particles/Alpha Blended"));
            Horizontal.material.SetColor("_TintColor", HorizontalColor);
            Horizontal.material.SetTextureScale("_MainTex", new Vector2(20f, 1f));
            Horizontal.material.mainTexture = GameDatabase.Instance.GetTexture("KFC/Textures/dashline", false);
            Horizontal.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            Horizontal.receiveShadows = false;
            Horizontal.startWidth = 0.25f;
            Horizontal.endWidth = 0.25f;
            Horizontal.positionCount = 2;
            Horizontal.SetPosition(0, HorizontalTransform.position);
            Horizontal.SetPosition(1, HorizontalTransform.position);
            Horizontal.useWorldSpace = true;
            Horizontal.enabled = true;
        }
        else
        {
            Horizontal.SetPosition(0, HorizontalTransform.position);
            Horizontal.SetPosition(1, HorizontalTransform.position);
            Horizontal.enabled = true;
        }
    }

    public override void OnFixedUpdate()
    {
        if (IsActivated)
        {
            OLSZoneTransform.localScale = new Vector3(ZRange * 50f, ZRange * 50f, ZRange * 50f);

            Color LEDColor = Color.yellow;
            if (!LightColor)
            {
                LightColor = LEDLightTransform.gameObject.GetComponent(typeof(MeshRenderer)) as MeshRenderer;
            }
            if (LightColor)
            {
                LightColor.material.SetColor("_EmissiveColor", LEDColor);
            }

            LightsTransform.gameObject.SetActive(true);
        }
        else
        {
            OLSZoneTransform.localScale = Vector3.zero;
            Color LEDColor = Color.black;
            if (!LightColor)
            {
                LightColor = LEDLightTransform.gameObject.GetComponent(typeof(MeshRenderer)) as MeshRenderer;
            }
            if (LightColor)
            {
                LightColor.material.SetColor("_EmissiveColor", LEDColor);
            }
            LightsTransform.gameObject.SetActive(false);
        }
        if (IsTriggered)
        {
            if (TargetVessel.isActiveVessel)
            {
                TargetTransform = TargetVessel.transform;
                Vector3 ReP = IdealTransform.position - TargetTransform.position;
                Vector3 ReV = this.part.vessel.GetSrfVelocity() - TargetVessel.GetSrfVelocity();
                if (Vector3.Dot(ReV, ReP) < 0 && ReP.magnitude > 5 && ReV.magnitude >5)
                {
                    //Debug.Log("working");
                    IsWorking = true;
                }
                else
                {
                    IsWorking = false;
                }
                if (IsWorking)
                {
                    Vector3 RePProject = IdealTransform.InverseTransformDirection(ReP.normalized);

                    Director.SetPosition(0, TargetVessel.transform.position);
                    Director.SetPosition(1, TargetVessel.transform.position + TargetVessel.GetSrfVelocity());
                    IdealLine.SetPosition(0, IdealTransform.position);
                    IdealLine.SetPosition(1, TargetVessel.transform.position);
                    IndicatorTransform.localScale = Vector3.one;
                    IndicatorTransform.position = IdealTransform.position + IdealTransform.forward * (ReP.magnitude - 20f);
                    Horizontal.SetPosition(0, TargetVessel.transform.position - IdealTransform.forward * 20f - IdealTransform.right * 10f);
                    Horizontal.SetPosition(1, TargetVessel.transform.position - IdealTransform.forward * 20f + IdealTransform.right * 10f);
                    Vertical.SetPosition(0, TargetVessel.transform.position - IdealTransform.forward * 20f - IdealTransform.up * RePProject.y *30 - IdealTransform.right * 0.5f);
                    Vertical.SetPosition(1, TargetVessel.transform.position - IdealTransform.forward * 20f - IdealTransform.up * RePProject.y *30 + IdealTransform.right * 0.5f);

                    if (RePProject.y < -Mathf.Sin(Mathf.Deg2Rad * 3f))
                    {
                        VerticalColor = Color.red;
                    }
                    else
                        if (RePProject.y >= -Mathf.Sin(Mathf.Deg2Rad * 3f) && RePProject.y <= Mathf.Sin(Mathf.Deg2Rad * 5f))
                        {
                            VerticalColor = Color.yellow;
                        }
                        else
                            if (RePProject.y > Mathf.Sin(Mathf.Deg2Rad * 3f))
                            {
                                VerticalColor = Color.red;
                            }
                    if (Mathf.Abs(RePProject.y) < Mathf.Sin(Mathf.Deg2Rad * 3f))
                    {
                        vflash = 0f;
                    }
                    else
                        if (Mathf.Abs(RePProject.y) < Mathf.Sin(Mathf.Deg2Rad * 5f))
                        {
                            vflash = 0.5f;
                        }
                        else
                            if (Mathf.Abs(RePProject.y) >= Mathf.Sin(Mathf.Deg2Rad * 5))
                            {
                                vflash = 0.1f;
                            }

                    if (RePProject.x < -Mathf.Sin(Mathf.Deg2Rad * 0.25f))
                    {
                        HorizontalColor = Color.green;
                    }
                    else
                        if (RePProject.x >= -Mathf.Sin(Mathf.Deg2Rad * 0.25f) && RePProject.x <= Mathf.Sin(Mathf.Deg2Rad * 0.25f))
                        {
                            HorizontalColor = Color.yellow;
                        }
                        else
                            if (RePProject.x > Mathf.Sin(Mathf.Deg2Rad * 0.25f))
                            {
                                HorizontalColor = Color.red;
                            }
                    if (Mathf.Abs(RePProject.x) < Mathf.Sin(Mathf.Deg2Rad * 1f))
                    {
                        hflash = 0f;
                    }
                    else
                        if (Mathf.Abs(RePProject.x) < Mathf.Sin(Mathf.Deg2Rad * 5f))
                        {
                            hflash = 0.5f;
                        }
                        else
                            if (Mathf.Abs(RePProject.x) >= Mathf.Sin(Mathf.Deg2Rad * 5f))
                            {
                                hflash = 0.1f;
                            }

                    if (Horizontal.material.GetColor("_TintColor") == Color.clear && hflash != 0f)
                    {
                        if (Time.time < htimer + hflash)
                        {
                            HorizontalColor = Color.clear;
                        }
                        else
                        {
                            htimer = Time.time + hflash;
                        }
                    }
                    if (Horizontal.material.GetColor("_TintColor") != Color.clear && hflash != 0f)
                    {
                        if (Time.time > htimer + hflash)
                        {
                            HorizontalColor = Color.clear;
                            htimer = Time.time + hflash;
                        }
                    }

                    if (Vertical.material.GetColor("_TintColor") == Color.clear && vflash != 0f)
                    {
                        if (Time.time < vtimer + vflash)
                        {
                            VerticalColor = Color.clear;
                        }
                        else
                        {
                            vtimer = Time.time + vflash;
                        }
                    }
                    if (Vertical.material.GetColor("_TintColor") != Color.clear && vflash !=0f)
                    {
                        if (Time.time > vtimer + vflash)
                        {
                            VerticalColor = Color.clear;
                            vtimer = Time.time + vflash;
                        }
                    }

                    Vertical.material.SetColor("_TintColor", VerticalColor);
                    Horizontal.material.SetColor("_TintColor", HorizontalColor);
                }
                else
                {
                    Horizontal.SetPosition(0, HorizontalTransform.position);
                    Horizontal.SetPosition(1, HorizontalTransform.position);
                    Vertical.SetPosition(0, VerticalTransform.position);
                    Vertical.SetPosition(1, VerticalTransform.position);
                    IdealLine.SetPosition(0, IdealTransform.position);
                    IdealLine.SetPosition(1, IdealTransform.position);
                    Director.SetPosition(0, DirectorTransform.position);
                    Director.SetPosition(1, DirectorTransform.position);
                    IndicatorTransform.localScale = Vector3.zero;
                    TargetVessel = null;
                }
            }
            else
                if (!TargetVessel || !TargetVessel.isActiveVessel)
                {
                    IsTriggered = false;
                    TargetVessel = null;
                    Horizontal.SetPosition(0, HorizontalTransform.position);
                    Horizontal.SetPosition(1, HorizontalTransform.position);
                    Vertical.SetPosition(0, VerticalTransform.position);
                    Vertical.SetPosition(1, VerticalTransform.position);
                    IdealLine.SetPosition(0, IdealTransform.position);
                    IdealLine.SetPosition(1, IdealTransform.position);
                    Director.SetPosition(0, DirectorTransform.position);
                    Director.SetPosition(1, DirectorTransform.position);
                    IndicatorTransform.localScale = Vector3.zero;
                }

        }
        else
        {
            TargetVessel = null;
            Horizontal.SetPosition(0, HorizontalTransform.position);
            Horizontal.SetPosition(1, HorizontalTransform.position);
            Vertical.SetPosition(0, VerticalTransform.position);
            Vertical.SetPosition(1, VerticalTransform.position);
            IdealLine.SetPosition(0, IdealTransform.position);
            IdealLine.SetPosition(1, IdealTransform.position);
            Director.SetPosition(0, DirectorTransform.position);
            Director.SetPosition(1, DirectorTransform.position);
            IndicatorTransform.localScale = Vector3.zero;
            vtimer = 0;
            htimer = 0;
        }
        IsTriggered = false;
    }

    void OnTriggerStay(Collider other)
    {
        if (!IsTriggered && IsActivated)
        {
            Part IRpart = other.gameObject.GetComponentInParent<Part>();
            if (IRpart && IRpart.vessel && IRpart.vessel != this.vessel && IRpart.vessel.isActiveVessel)
            {
                //Debug.Log("Enter");
                IsTriggered = true;
                TargetVessel = IRpart.vessel;
            }
        }
    }
}

