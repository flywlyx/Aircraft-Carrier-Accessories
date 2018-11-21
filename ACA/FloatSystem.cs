using UnityEngine;

public class Floatsystem : PartModule
{
    [KSPField]
    public float hoverForce = 0;
    [KSPField]
    public float Draftoffset = 5;
    [KSPField]
    public float maxav = 1;
    [KSPField]
    bool anchored = false;
    [KSPField(isPersistant = false)]
    public float Stability = 1;
    [KSPField(isPersistant = false)]
    public float yaws = 1;
    [KSPField(isPersistant = false)]
    public float yawc = 5;

    //[KSPEvent(name = "Anchor", guiName = "Drop Anchor", active = true, guiActive = true)]
    //public void Anchor()
    //{
    //    if (this.vessel.Splashed == true)
    //    {
    //        this.part.mass = 10;
    //        this.part.rb.mass = 10;
    //        this.part.rigidbody.mass = 10;
    //        this.part.Rigidbody.mass = 10;
    //        this.part.buoyancy = 0;
    //        this.vessel.rigidbody.mass = 10;
    //        anchored = true;
    //        Events["Deanchor"].guiActive = true;
    //        Events["Anchor"].guiActive = false;
    //    }
    //}

    //[KSPEvent(name = "Deanchor", guiName = "Raise Anchor", active = true, guiActive = false)]
    //public void Deanchor()
    //{
    //    if (this.vessel.Splashed == true)
    //    {
    //        this.part.mass = 40000;
    //        this.part.rb.mass = 40000;
    //        this.part.rigidbody.mass = 40000;
    //        this.part.Rigidbody.mass = 40000;
    //        this.vessel.rigidbody.mass = this.vessel.GetTotalMass();
    //        anchored = false;
    //        Events["Deanchor"].guiActive = false;
    //        Events["Anchor"].guiActive = true;
    //    }
    //}

    protected Transform DraftTransform = null;
    protected Transform rudderTransform = null;

    public override void OnStart(PartModule.StartState state)
    {
        base.OnStart(state);
        {
            if (state != StartState.Editor && state != StartState.None)
            {
                this.enabled = true;
                this.part.force_activate();
            }
            else
            {
                this.enabled = false;
            }

            if (base.part.FindModelTransform("Draft") != null)
            {
                DraftTransform = base.part.FindModelTransform("Draft");
            } 
            if (base.part.FindModelTransform("rudder") != null)
            {
                rudderTransform = base.part.FindModelTransform("rudder");
            }

        }

    }


    public override void OnFixedUpdate()
    {
        if (!HighLogic.LoadedSceneIsFlight) return;
        if (this.vessel.mainBody.ocean)
        {
            float pitch = vessel.ctrlState.pitch;
            float roll = vessel.ctrlState.roll;
            float yaw = vessel.ctrlState.yaw;

            Vector3 srfVelocity = vessel.GetSrfVelocity();
            float VerticalV = (float)vessel.verticalSpeed;
            Vector3 Speed = part.transform.InverseTransformDirection(srfVelocity);
            Vector3 gee = FlightGlobals.getGeeForceAtPosition(this.transform.position);
            float height = FlightGlobals.getAltitudeAtPos(DraftTransform.position);
            Vector3 yawsForce = Vector3.SqrMagnitude(Speed) * Speed.x * System.Math.Abs(Speed.y) * yaws * -vessel.transform.right;
            rudderTransform.localEulerAngles = Vector3.down * yaw * 10f;
            Vector3 VerPosition = DraftTransform.InverseTransformDirection(gee);
            Vector3 rollControl = vessel.transform.up * Stability * VerPosition.x * (float)this.vessel.totalMass;
            Vector3 pitchControl = -vessel.transform.right * 5f * Stability * VerPosition.y * (float)this.vessel.totalMass;
            Vector3 buoyancy = (float)this.vessel.totalMass * gee * (VerticalV + height);


            //if (anchored)
            //{
            //    this.vessel.verticalSpeed = 0;
            //    this.vessel.angularVelocity = Vector3.zero;
            //    this.vessel.srf_velocity = Vector3.zero;
            //}

            //if (part.rb.angularVelocity.magnitude > maxav)
            //{
            //    part.rb.angularVelocity = maxav * part.vessel.rigidbody.angularVelocity / part.vessel.rigidbody.angularVelocity.magnitude;
            //}

            if (height < 3f)
            {

                part.AddTorque(rollControl);
                part.AddTorque(pitchControl);
                part.AddForceAtPosition(yawsForce, vessel.transform.position);
                part.AddForce(buoyancy);
                this.part.AddTorque(-this.part.transform.forward * (float)this.vessel.totalMass * yaw * yawc * Vector3.SqrMagnitude(Speed));
                //Debug.Log(rollControl);
                //Debug.Log(Stability);
            }
        }
    }
}

