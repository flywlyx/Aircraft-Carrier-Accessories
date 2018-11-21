using UnityEngine;

public class Arrestwire : PartModule
{
    [KSPField]
    public float wiretension = 1f;
    private Transform ARP1Transform = null;
    private Transform ARP2Transform = null;
    private Transform WireTransform = null;
    public bool IsHooked = false;

    [KSPField(guiActive = true, guiActiveEditor = true, guiName = "Force Ratio", isPersistant = true), UI_FloatRange(minValue = 1f, maxValue = 10f, stepIncrement = 1f)]
    public float ForceRatio = 1f;

    public override void OnStart(PartModule.StartState state)
    {
        this.enabled = true;
        this.part.force_activate();
        ARP1Transform = base.part.FindModelTransform("ARP1");
        ARP2Transform = base.part.FindModelTransform("ARP2");
        WireTransform = base.part.FindModelTransform("Wire");
    }

    public override void OnFixedUpdate()
    {
        if (!IsHooked)
        {
            //LineRenderer rope = this.part.gameObject.GetComponent<LineRenderer>();
            //if (rope == null)
            //{
            //    rope = this.part.gameObject.AddComponent<LineRenderer>();
            //    Color ropeColor = Color.white;
            //    rope.material = new Material(Shader.Find("KSP/Diffuse"));
            //    rope.material.SetColor("_BurnColor", ropeColor);
            //    rope.material.mainTexture = GameDatabase.Instance.GetTexture("KFC/Textures/SteelWire", false);
            //    rope.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            //    rope.receiveShadows = false;
            //    rope.startWidth=(0.05f);
            //    rope.endWidth = (0.05f);
            //    rope.positionCount = 2;
            //    rope.SetPosition(0, ARP1Transform.position+part.rb.velocity*Time.fixedDeltaTime);
            //    rope.SetPosition(1, ARP2Transform.position + part.rb.velocity * Time.fixedDeltaTime);
            //    rope.material.SetTextureScale("_MainTex", new Vector2(Vector3.Distance(ARP1Transform.position, ARP2Transform.position) * 20, 1f));
            //    rope.useWorldSpace = true;
            //    rope.enabled = true;
            //}
            //else
            //{
            //    rope.SetPosition(0, ARP1Transform.position + part.rb.velocity * Time.fixedDeltaTime);
            //    rope.SetPosition(1, ARP2Transform.position + part.rb.velocity * Time.fixedDeltaTime);
            //    rope.material.SetTextureScale("_MainTex", new Vector2(Vector3.Distance(ARP1Transform.position, ARP2Transform.position) * 20, 1f));
            //    rope.enabled = true;
            //}
            WireTransform.localScale = Vector3.one;
        }
        else
        {
            //LineRenderer hit = this.part.gameObject.GetComponent<LineRenderer>();
            //if (hit != null)
            //{
            //    Destroy(hit);
            //}
            WireTransform.localScale = Vector3.zero;
        }
    }
}
