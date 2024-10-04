using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace FarFutureTechnologies
{
  public class ModuleEngineExhaustDamage : PartModule
  {
    [KSPField(isPersistant = false)]
    public string engineID;

    /// <summary>
    /// Damage per second to apply at 1m range from source
    /// </summary>
    [KSPField(isPersistant = false)]
    public float damagePerSecond = 0f;

    /// <summary>
    /// Maximum distance to apply damange at
    /// </summary>
    [KSPField(isPersistant = false)]
    public float damageDistance = 10f;

    /// <summary>
    /// How fast the damage falls off, 2 is square
    /// </summary>
    [KSPField(isPersistant = false)]
    public float damageFalloffExponent = 2f;

    /// <summary>
    /// Damage to apply at 1m range from source from a pulse
    /// </summary>
    [KSPField(isPersistant = false)]
    public float damagePulse = 0f;

    /// <summary>
    /// Damage to apply at 1m range from source from a pulse
    /// </summary>
    [KSPField(isPersistant = false)]
    public float heatPulse = 0f;

    /// <summary>
    /// Heat to apply at 1m range from source from a pulse
    /// </summary>
    [KSPField(isPersistant = false)]
    public float forcePulse = 0f;

    /// <summary>
    /// Heat falloff. If negative will be populated from damage falloff
    /// </summary>
    [KSPField(isPersistant = false)]
    public float heatFalloffExponent = -1f;

    /// <summary>
    /// Force falloff. If negative will be populated from damage falloff
    /// </summary>
    [KSPField(isPersistant = false)]
    public float forceFalloffExponent = -1f;

    /// <summary>
    /// Maximum distance to apply heat at. If negative will be populated from damage distance
    /// </summary>
    [KSPField(isPersistant = false)]
    public float heatDistance = -1f;

    /// <summary>
    /// Maximum distance to apply force at. If negative will be populated from damage distance
    /// </summary>
    [KSPField(isPersistant = false)]
    public float forceDistance = -1f;

    /// <summary>
    /// Whether atmosphere affects force quantity
    /// </summary>
    [KSPField(isPersistant = false)]
    public bool atmosphereAffectsForce = true;

    /// <summary>
    /// Whether atmosphere affects heat applied
    /// </summary>
    [KSPField(isPersistant = false)]
    public bool atmosphereAffectsHeat = true;

    /// <summary>
    /// Whether atmosphere affects building damage
    /// </summary>
    [KSPField(isPersistant = false)]
    public bool atmosphereAffectsDamage = true;

    /// <summary>
    /// Scales damage by atmosphere density
    /// </summary>
    [KSPField(isPersistant = false)]
    public FloatCurve damageAtmosphereCurve;

    /// <summary>
    /// Scales force by atmosphere density
    /// </summary>
    [KSPField(isPersistant = false)]
    public FloatCurve forceAtmosphereCurve;

    /// <summary>
    /// Scales heat by atmosphere density
    /// </summary>
    [KSPField(isPersistant = false)]
    public FloatCurve heatAtmosphereCurve;

    /// <summary>
    /// Application pattern of pulse damage over time
    /// </summary>
    [KSPField(isPersistant = false)]
    public FloatCurve damageApplicationCurve;

    /// <summary>
    /// Application pattern of pulse force over time
    /// </summary>
    [KSPField(isPersistant = false)]
    public FloatCurve forceApplicationCurve;

    /// <summary>
    /// Application pattern of pulse heat over time
    /// </summary>
    [KSPField(isPersistant = false)]
    public FloatCurve heatApplicationCurve;

    /// <summary>
    /// Test visibility to parts affected by a pulse before applying force and heat. Expensive. 
    /// </summary>
    [KSPField(isPersistant = false)]
    public bool testPulseVisibility = false;

    private float forceTick = 0.02f;
    private float heatTick = 0.02f;
    private float damageTick = 0.02f;
    private float forceDuration = 0f;
    private float heatDuration = 0f;
    private float damageDuration = 0f;

    private float perTransformDamage = 0f;

    private LayerMask buildingsMask;
    private LayerMask partsMask;
    private LayerMask everythingMask;
    private ModuleEnginesFX engine;



    public void Awake()
    {
      base.Awake();
      if (HighLogic.LoadedSceneIsFlight)
      {
        /// Set up curve defaults 
        if (heatApplicationCurve.Curve.keys.Length == 0)
        {
          heatApplicationCurve = new FloatCurve();
          heatApplicationCurve.Add(0f, 0f);
          heatApplicationCurve.Add(0.02f, 1f);
          heatApplicationCurve.Add(.2f, 0f);
        }
        if (forceApplicationCurve.Curve.keys.Length == 0)
        {
          forceApplicationCurve = new FloatCurve();
          forceApplicationCurve.Add(0f, 0f);
          forceApplicationCurve.Add(0.02f, 1f);
          forceApplicationCurve.Add(.2f, 0f);
        }
        if (damageApplicationCurve.Curve.keys.Length == 0)
        {
          damageApplicationCurve = new FloatCurve();
          damageApplicationCurve.Add(0f, 0f);
          damageApplicationCurve.Add(0.02f, 1f);
          damageApplicationCurve.Add(.1f, 0f);
        }
        if (damageAtmosphereCurve.Curve.keys.Length == 0)
        {
          damageAtmosphereCurve = new FloatCurve();
          damageAtmosphereCurve.Add(0f, 0.5f);
          damageAtmosphereCurve.Add(1f, 1f);
        }
        if (heatAtmosphereCurve.Curve.keys.Length == 0)
        {
          heatAtmosphereCurve = new FloatCurve();
          heatAtmosphereCurve.Add(0f, 1f);
          heatAtmosphereCurve.Add(1f, 1f);
          heatAtmosphereCurve.Add(10f, 10f);
        }
        if (forceAtmosphereCurve.Curve.keys.Length == 0)
        {
          forceAtmosphereCurve = new FloatCurve();
          forceAtmosphereCurve.Add(0f, 0.1f);
          forceAtmosphereCurve.Add(1f, 1f);
          forceAtmosphereCurve.Add(10f, 5f);
        }
        /// If the data for the heat/force distances wasn't specified, use the stuff for damage
        heatDistance = heatDistance <= 0f ? damageDistance : heatDistance;
        forceDistance = forceDistance <= 0f ? damageDistance : forceDistance;
        heatFalloffExponent = heatFalloffExponent <= 0f ? damageFalloffExponent : heatFalloffExponent;
        forceFalloffExponent = forceFalloffExponent <= 0f ? damageFalloffExponent : forceFalloffExponent;

        heatDuration = heatApplicationCurve.maxTime;
        forceDuration = forceApplicationCurve.maxTime;
        damageDuration = damageApplicationCurve.maxTime;
      }
    }

    public void Start()
    {
      if (HighLogic.LoadedSceneIsFlight)
      {
        // Set up the layers we need
        LayerMask defaultsMask = 1 << LayerMask.NameToLayer("Default");
        LayerMask EVAMask = 1 << LayerMask.NameToLayer("EVA");
        LayerMask objMask = 1 << LayerMask.NameToLayer("physicalObjects");
        LayerMask terrainMask = 1 << LayerMask.NameToLayer("TerrainColliders");

        buildingsMask = LayerMask.GetMask(new string[] { "Local Scenery" });

        partsMask = defaultsMask | EVAMask | objMask;
        everythingMask = defaultsMask | EVAMask | buildingsMask | terrainMask;

        if (engineID != "")
        {
          engine = this.GetComponents<ModuleEnginesFX>().ToList().Find(x => x.engineID == engineID);
        }
        else
        {
          engine = this.GetComponents<ModuleEnginesFX>().ToList().First();
        }
        if (engine != null)
        {
          perTransformDamage = damagePerSecond / (float)engine.thrustTransforms.Count;
        }
      }
    }

    void FixedUpdate()
    {
      if (HighLogic.LoadedSceneIsFlight && engine != null && damagePerSecond > 0f)
      {
        if (engine.EngineIgnited)
        {
          if (engine.requestedThrottle > 0f && !engine.flameout)
          {
            for (int i = 0; i < engine.thrustTransforms.Count; i++)
            {
              DoContinuousBuildingDamage(engine.thrustTransforms[i], engine.finalThrust / engine.maxThrust);
            }
          }
        }
      }
    }

    void DoContinuousBuildingDamage(Transform thrustTransform, float thrustScale)
    {
      RaycastHit hit = new RaycastHit();
      if (Physics.Raycast(thrustTransform.position, thrustTransform.forward, out hit, damageDistance, buildingsMask))
      {
        DestructibleBuilding hitBuilding = hit.collider.GetComponentInParent<DestructibleBuilding>();
        if (hitBuilding != null)
        {
          float distanceScale = 1f / (Mathf.Pow(hit.distance, damageFalloffExponent) / 1f);
          ApplyDamageNoThreshold(hitBuilding, TimeWarp.deltaTime * perTransformDamage * distanceScale * thrustScale);
        }
      }
    }

    /// <summary>
    /// Damage a building. This is a reimplementation of DestructibleBuilding.ApplyDamage with the damage thresholds removed
    /// </summary>
    /// <param name="building"></param>
    /// <param name="damage"></param>
    void ApplyDamageNoThreshold(DestructibleBuilding building, float damage)
    {

      if (HighLogic.CurrentGame.Parameters.Difficulty.IndestructibleFacilities || !FarFutureTechnologiesSettings_EngineDamage.DamageFacilities || damage < FarFutureTechnologySettings.MinimumExhaustBuildingDamagePerTick)
      {
        return;
      }
      /// TODO: Optimize me
      FieldInfo field = building.GetType().GetField("damage", BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.Instance);
      field.SetValue(building, building.Damage + FarFutureTechnologiesSettings_EngineDamage.FacilityDamageScale * damage);

      // These numbers belong to stock
      if (!(building.Damage > 500f * DestructibleBuilding.BuildingToughnessFactor))
      {
        return;
      }
      if (building.IsIntact)
      {
        building.Demolish();
      }
    }

    /// <summary>
    /// Do damage to builds in a sphere around a point. This applies building damage, which is special
    /// </summary>
    /// <param name="position"></param>
    public void DoPulseBuildingDamageSpherical(Vector3 position)
    {
      Collider[] hitColliders = Physics.OverlapSphere(position, damageDistance, buildingsMask);

      /// Collect the hit buildings
      List<DestructibleBuilding> hitBuildings = new List<DestructibleBuilding>();
      for (int i = 0; i < hitColliders.Length; i++)
      {
        DestructibleBuilding hitBuilding = hitColliders[i].GetComponentInParent<DestructibleBuilding>();
        if (hitBuilding != null && !hitBuildings.Contains(hitBuilding))
        {
          hitBuildings.Add(hitBuilding);
        }
      }
      for (int i = 0; i < hitBuildings.Count; i++)
      {
        float scale = 1f / (Mathf.Pow(Vector3.Distance(position, hitBuildings[i].transform.position), damageFalloffExponent) / 1f);
        if (scale * damagePulse > FarFutureTechnologySettings.MinimumPulseBuildingDamage)
        {
          StartCoroutine(ApplyDamageCoroutine(hitBuildings[i], scale * damagePulse));
        }
      }
    }

    /// <summary>
    /// Do damage to parts in a sphere around a point. This applies force and heat
    /// </summary>
    /// <param name="position"></param>
    public void DoPulsePartsDamageSpherical(Vector3 position)
    {
      if (heatPulse > 0f || forcePulse > 0f)
      {
        Collider[] hitColliders = Physics.OverlapSphere(position, Mathf.Max(forceDistance, heatDistance), partsMask);
        float atmosphereHeatScale = 1f;
        if (atmosphereAffectsHeat)
        {
          atmosphereHeatScale = heatAtmosphereCurve.Evaluate((float)this.part.staticPressureAtm);
        }
        float atmosphereForceScale = 1f;
        if (atmosphereAffectsForce)
        {
          atmosphereForceScale = forceAtmosphereCurve.Evaluate((float)this.part.staticPressureAtm);
        }


        List<Part> hitParts = new List<Part>();
        for (int i = 0; i < hitColliders.Length; i++)
        {
          Part hitPart = hitColliders[i].GetComponentInParent<Part>();
          // we don't affect parts on our own vessel
          if (hitPart != null && hitPart.vessel != this.part.vessel && !hitParts.Contains(hitPart))
          {
            hitParts.Add(hitPart);
          }
        }
        for (int i = 0; i < hitParts.Count; i++)
        {
          float heatToApply = 0f;
          float forceToApply = 0f;
          float distance = Vector3.Distance(position, hitParts[i].transform.position);
          if (distance <= heatDistance)
          {
            heatToApply = atmosphereHeatScale * heatPulse * 1f / (Mathf.Pow(distance, heatFalloffExponent) / 1f);
          }
          if (distance <= forceDistance)
          {
            forceToApply = atmosphereForceScale * forcePulse * 1f / (Mathf.Pow(distance, forceFalloffExponent) / 1f);
          }
          /// if allowed, test if this part can see the point
          if (testPulseVisibility)
          {
            if (Physics.Raycast(position, hitParts[i].transform.position - position, out RaycastHit hit, distance, everythingMask))
            {
              heatToApply = 0f;
              forceToApply = 0f;
            }
          }

          if (heatToApply > FarFutureTechnologySettings.MinimumPulsePartsHeat)
          {
            StartCoroutine(ApplyHeatCoroutine(hitParts[i], heatToApply));
          }
          if (forceToApply > FarFutureTechnologySettings.MinimumPulsePartsForce)
          {
            StartCoroutine(ApplyForceCoroutine(hitParts[i], (hitParts[i].transform.position - position).normalized * forceToApply));
          }

        }
      }
    }


    IEnumerator ApplyDamageCoroutine(DestructibleBuilding targetBuilding, float damage)
    {
      float timer = 0f;
      while (timer <= damageDuration)
      {
        ApplyDamageNoThreshold(targetBuilding, damage * damageApplicationCurve.Evaluate(timer));
        timer += damageTick;
        yield return new WaitForSeconds(damageTick);
      }
    }
    IEnumerator ApplyForceCoroutine(Part targetPart, Vector3d force)
    {
      float timer = 0f;
      while (timer <= forceDuration)
      {
        targetPart.AddForce(force * forceApplicationCurve.Evaluate(timer));
        timer += forceTick;
        yield return new WaitForSeconds(forceTick);
      }
    }
    IEnumerator ApplyHeatCoroutine(Part targetPart, float heat)
    {
      float timer = 0f;
      while (timer <= heatDuration)
      {
        targetPart.AddExposedThermalFlux(heat * heatApplicationCurve.Evaluate(timer));
        timer += heatTick;
        yield return new WaitForSeconds(heatTick);
      }
    }
  }
}

