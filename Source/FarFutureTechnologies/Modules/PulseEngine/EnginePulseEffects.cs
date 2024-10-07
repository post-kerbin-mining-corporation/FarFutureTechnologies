using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Waterfall;

namespace FarFutureTechnologies
{
  public class PulseEngineEffect
  {
    public string name;
    public Part part;
    public FloatCurve timeIntensityCurve = new FloatCurve();
    public bool isSequenced = false;
    public int sequenceID = -1;

    public PulseEngineEffect() { }
    public PulseEngineEffect(ConfigNode node, Part hostPart)
    {
      Load(node);
      part = hostPart;
    }
    public virtual void Load(ConfigNode node)
    {
      node.TryGetValue("name", ref name);
      node.TryGetValue("sequential", ref isSequenced);
      node.TryGetValue("sequenceID", ref sequenceID);
      if (node.HasNode("timeIntensityCurve"))
      {
        timeIntensityCurve.Load(node.GetNode("timeIntensityCurve"));
      }
    }
    public virtual IEnumerator Pulse() { yield return 0; }
    public virtual void Update(float time) { }
  }
  public class PulseEngineWaterfallEffect : PulseEngineEffect
  {
    protected string controllerName = "";
    protected string effectName = "";
    protected string moduleID = "";

    public ModuleWaterfallFX WaterfallModule
    {
      get
      {
        if (_waterfallModule != null)
        {
          return _waterfallModule;
        }
        else
        {
          if (moduleID != "")
          {
            _waterfallModule = part.GetComponents<ModuleWaterfallFX>().ToList().Find(x => x.moduleID == moduleID);
          }
          else
          {
            _waterfallModule = part.GetComponents<ModuleWaterfallFX>().ToList().First();
          }
          return _waterfallModule;
        }
      }
    }

    protected ModuleWaterfallFX _waterfallModule;
    protected WaterfallEffect _waterfallEffect;
    public PulseEngineWaterfallEffect() { }
    public PulseEngineWaterfallEffect(ConfigNode node, Part hostPart) : base(node, hostPart) { }

    public override void Load(ConfigNode node)
    {
      base.Load(node);
      node.TryGetValue("waterfallModuleID", ref moduleID);
      node.TryGetValue("waterfallControllerName", ref controllerName);
      node.TryGetValue("waterfallEffectName", ref effectName);
    }
  }
  public class PulseEnginePulseEffect : PulseEngineWaterfallEffect
  {
    public bool fakeWorldSpace = false;

    private Transform[] effectTransforms;
    private Transform[] parents;
    private Vector3[] positions;
    private Quaternion[] rotations;

    private bool setupComplete = false;
    private Rigidbody[] rbs;
    physicalObject[] pos;
    private float lifeTimer;

    public PulseEnginePulseEffect() { }
    public PulseEnginePulseEffect(ConfigNode node, Part hostPart) : base(node, hostPart) { }
    public override void Load(ConfigNode node)
    {
      base.Load(node);
      node.TryGetValue("fakeWorldSpace", ref fakeWorldSpace);
    }

    public override IEnumerator Pulse()
    {
      if (_waterfallEffect == null)
      {
        _waterfallEffect = WaterfallModule.FindEffect(effectName);
      }
      if (effectName != "" && _waterfallEffect != null)
      {
        _waterfallEffect.Reset(true);

        if (fakeWorldSpace)
        {
          if (!setupComplete)
          {
            // detach and save the positions
            effectTransforms = _waterfallEffect.GetEffectTransforms().ToArray();
            parents = new Transform[effectTransforms.Length];
            positions = new Vector3[effectTransforms.Length];
            rotations = new Quaternion[effectTransforms.Length];
            rbs = new Rigidbody[effectTransforms.Length];
            pos = new physicalObject[effectTransforms.Length];

            for (int i = 0; i < effectTransforms.Length; i++)
            {
              parents[i] = effectTransforms[i].parent;
              positions[i] = effectTransforms[i].localPosition;
              rotations[i] = effectTransforms[i].localRotation;
              effectTransforms[i].parent = null;

              rbs[i] = effectTransforms[i].gameObject.AddComponent<Rigidbody>();
              pos[i] = effectTransforms[i].gameObject.AddComponent<physicalObject>();
              rbs[i].useGravity = false;
              pos[i].origDrag = 1f;
              pos[i].maxDistance = 50000f;
              rbs[i].velocity = part.vessel.rb_velocity;
            }
            setupComplete = true;
          }
          else
          {
            /// reattach to parent and restore the positions
            for (int i = 0; i < effectTransforms.Length; i++)
            {
              effectTransforms[i].SetParent(parents[i], true);
              effectTransforms[i].localPosition = positions[i];
              effectTransforms[i].localRotation = rotations[i];
              effectTransforms[i].parent = null;
              rbs[i].velocity = part.vessel.rb_velocity;
            }
          }
          yield return 0;
          for (int i = 0; i < pos.Length; i++)
          {
            pos[i].enabled = false;
          }
        }
      }
    }

    IEnumerator CleanupTimer()
    {
      while (lifeTimer <= 5f)
      {
        lifeTimer += 0.1f;
        yield return new WaitForSeconds(0.1f);
      }

      for (int i = 0; i < effectTransforms.Length; i++)
      {
        effectTransforms[i].SetParent(parents[i], true);
        effectTransforms[i].localPosition = positions[i];
        effectTransforms[i].localRotation = rotations[i];
        GameObject.Destroy(pos[i]);
        GameObject.Destroy(rbs[i]);

        setupComplete = false;
      }
    }
  }
  public class PulseEnginePlumeEffect : PulseEngineWaterfallEffect
  {

    public PulseEnginePlumeEffect() { }
    public PulseEnginePlumeEffect(ConfigNode node, Part hostPart) : base(node, hostPart) { }
    public override void Load(ConfigNode node)
    {
      base.Load(node);
    }
    public override void Update(float time)
    {
      if (timeIntensityCurve != null && controllerName != "")
      {
        WaterfallModule.SetControllerValue(controllerName, timeIntensityCurve.Evaluate(time));
      }
    }

    //public void FixedSet()
    //{
    //  if (fakeWorldSpace)
    //  {
    //    if (_waterfallEffect != null && effectName != null)
    //    {
    //      Vector3 deltaPosition;
    //      Vector3 frameVelocity = Krakensbane.GetFrameVelocity();
    //      Vector3 frameGravVel = part.vessel.graviticAcceleration * TimeWarp.fixedDeltaTime;

    //      Debug.Log($"g={FlightGlobals.getGeeForceAtPosition(part.vessel.GetWorldPos3D(), part.vessel.mainBody).magnitude}., " +
    //        $"Fc{FlightGlobals.getCoriolisAcc(part.vessel.velocityD, part.vessel.mainBody).magnitude}, " +
    //      $"Fcent ={FlightGlobals.getCentrifugalAcc(part.vessel.GetWorldPos3D(), part.vessel.mainBody).magnitude}");
    //      Vector3 grav = FlightGlobals.getGeeForceAtPosition(part.vessel.GetWorldPos3D(), part.vessel.mainBody) + FlightGlobals.getCoriolisAcc(part.vessel.velocityD, part.vessel.mainBody) + FlightGlobals.getCentrifugalAcc(part.vessel.GetWorldPos3D(), part.vessel.mainBody);
    //      //rb.AddForce(-grav, ForceMode.Force);
    //      //Debug.Log($"g={}");

    //      if (frameVelocity.sqrMagnitude > 0f)
    //      {
    //        //Debug.Log($"TICK: delda from change={(snapshotVelocity - frameVelocity).magnitude:F2}, " +
    //        //  $"gvel={frameGravVel.magnitude:F2}, " +
    //        //  $"total ={(snapshotVelocity - frameVelocity + frameGravVel).magnitude:F2})");
    //        // Debug.Log($"TICK: Frame={part.vessel.velocityD}, dOrbit={snapshotVelocity - part.vessel.velocityD } delta={snapshotVelocity - frameVelocity}");
    //        //Debug.Log($"Velocities: Frame={Krakensbane.GetFrameVelocity().magnitude:F1}, VesselRB={part.vessel.rb_velocity.magnitude:F1}, VesselOrbit={part.vessel.obt_velocity.magnitude:F1}");
    //        //deltaPosition =  - FloatingOrigin.OffsetNonKrakensbane;
    //        deltaPosition = (snapshotVelocity - part.vessel.velocityD) * TimeWarp.fixedDeltaTime;
    //      }
    //      else
    //      {
    //        deltaPosition = snapshotVelocity * TimeWarp.fixedDeltaTime;
    //      }
    //      for (int i = 0; i < effects.Length; i++)
    //      {
    //        //Vector3 deltas = effects[i].position + snapshotVelocity *TimeWarp.fixedDeltaTime;
    //        //Debug.Log($"{deltaPosition} position, {FloatingOrigin.OffsetNonKrakensbane}");
    //        //effects[i].position += deltaPosition;
    //      }
    //    }
    //  }
    //}


  }
  public class PulseEngineColorAnimatorEffect : PulseEngineEffect
  {
    public string fxName;

    protected ModuleColorAnimator[] fxAnimators;
    protected string[] fxNames;

    public PulseEngineColorAnimatorEffect() { }
    public PulseEngineColorAnimatorEffect(ConfigNode node, Part hostPart) : base(node, hostPart) { }

    public override void Load(ConfigNode node)
    {
      base.Load(node);
      node.TryGetValue("animatorIDs", ref fxName);
      fxNames = fxName.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
    }
    public override void Update(float time)
    {
      if (fxAnimators == null)
      {
        CollectAnimators();
      }
      for (int i = 0; i < fxAnimators.Length; i++)
      {
        fxAnimators[i].SetScalar(timeIntensityCurve.Evaluate(time));
      }
    }
    protected void CollectAnimators()
    {
      fxAnimators = new ModuleColorAnimator[fxNames.Length];
      for (int i = 0; i < fxNames.Length; i++)
      {
        fxAnimators[i] = part.GetComponents<ModuleColorAnimator>().ToList().Find(x => x.moduleID == fxNames[i]);
      }
    }
  }
}
