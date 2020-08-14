using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ProjectileBase))]
public class ProjectileStandard : MonoBehaviour
{
    [Header("General")]
    [Tooltip("Radius of this projectile's collision detection")]
    public float radius = 0.01f;
    [Tooltip("Transform representing the root of the projectile (used for accurate collision detection)")]
    public Transform root;
    [Tooltip("Transform representing the tip of the projectile (used for accurate collision detection)")]
    public Transform tip;
    [Tooltip("LifeTime of the projectile")]
    public float maxLifeTime = 5f;
    [Tooltip("VFX prefab to spawn upon impact")]
    public GameObject impactVFX;
    [Tooltip("LifeTime of the VFX before being destroyed")]
    public float impactVFXLifetime = 5f;
    [Tooltip("Offset along the hit normal where the VFX will be spawned")]
    public float impactVFXSpawnOffset = 0.1f;
    [Tooltip("Clip to play on impact")]
    public AudioClip impactSFXClip;
    [Tooltip("Audio to play on damage")]
    public AudioClip damageSFXClip;
    [Tooltip("Layers this projectile can collide with")]
    public LayerMask hittableLayers = -1;

    [Header("Movement")]
    [Tooltip("Speed of the projectile")]
    public float speed = 20f;
    [Tooltip("Whether or not the projectile is homing")]
    public bool homing = false;
    [Tooltip("Max radians per frame that the projectile can turn if it is homing")]
    public float deltaTheta = 0.03f;
    [Tooltip("Downward acceleration from gravity")]
    public float gravityDownAcceleration = 0f;
    [Tooltip("Distance over which the projectile will correct its course to fit the intended trajectory (used to drift projectiles towards center of screen in First Person view). At values under 0, there is no correction")]
    public float trajectoryCorrectionDistance = -1;
    [Tooltip("Determines if the projectile inherits the velocity that the weapon's muzzle had when firing")]
    public bool inheritWeaponVelocity = false;

    [Header("Damage")]
    [Tooltip("Damage of the projectile")]
    public float damage = 40f;
    [Tooltip("Area of damage. Keep empty if you don<t want area damage")]
    public DamageArea areaOfDamage;

    [Header("Debug")]
    [Tooltip("Color of the projectile radius debug view")]
    public Color radiusColor = Color.cyan * 0.2f;

    ProjectileBase m_ProjectileBase;
    Vector3 m_LastRootPosition;
    Vector3 m_Velocity;
    bool m_HasTrajectoryOverride;
    float m_ShootTime;
    Vector3 m_TrajectoryCorrectionVector;
    Vector3 m_ConsumedTrajectoryCorrectionVector;
    List<Collider> m_IgnoredColliders;
    Transform m_TargetTransform;
    float spawnTime;

    const QueryTriggerInteraction k_TriggerInteraction = QueryTriggerInteraction.Collide;

    private void OnEnable()
    {
        m_ProjectileBase = GetComponent<ProjectileBase>();
        DebugUtility.HandleErrorIfNullGetComponent<ProjectileBase, ProjectileStandard>(m_ProjectileBase, this, gameObject);

        m_ProjectileBase.onShoot += OnShoot;

        Destroy(gameObject, maxLifeTime);

        int stageLayer = 1 << LayerMask.NameToLayer("Stage");
        int enemiesLayer = 1 << LayerMask.NameToLayer("Enemies");
        hittableLayers = stageLayer | enemiesLayer;

        if(homing)
        {
            FindHomingTarget();
        }

        spawnTime = Time.time;
    }

    void OnShoot()
    {
        m_ShootTime = Time.time;
        m_LastRootPosition = root.position;
        m_Velocity = transform.forward * speed;
        m_IgnoredColliders = new List<Collider>();
        transform.position += m_ProjectileBase.inheritedMuzzleVelocity * Time.deltaTime;

        // Ignore colliders of owner
        if(m_ProjectileBase.owner)
        {
            Collider[] ownerColliders = m_ProjectileBase.owner.GetComponentsInChildren<Collider>();
            m_IgnoredColliders.AddRange(ownerColliders);
        }

        // Handle case of player shooting (make projectiles not go through walls, and remember center-of-screen trajectory)
        PlayerWeaponsManager playerWeaponsManager = null;
        if(m_ProjectileBase.owner)
            playerWeaponsManager = m_ProjectileBase.owner.GetComponent<PlayerWeaponsManager>();

        if(playerWeaponsManager)
        {
            m_HasTrajectoryOverride = true;

            Vector3 cameraToMuzzle = (m_ProjectileBase.initialPosition - playerWeaponsManager.weaponCamera.transform.position);

            m_TrajectoryCorrectionVector = Vector3.ProjectOnPlane(-cameraToMuzzle, playerWeaponsManager.weaponCamera.transform.forward);
            if (trajectoryCorrectionDistance == 0)
            {
                transform.position += m_TrajectoryCorrectionVector;
                m_ConsumedTrajectoryCorrectionVector = m_TrajectoryCorrectionVector;
            }
            else if (trajectoryCorrectionDistance < 0)
            {
                m_HasTrajectoryOverride = false;
            }
            
            if (Physics.Raycast(playerWeaponsManager.weaponCamera.transform.position, cameraToMuzzle.normalized, out RaycastHit hit, cameraToMuzzle.magnitude, hittableLayers, k_TriggerInteraction))
            {
                if (IsHitValid(hit))
                {
                    OnHit(hit.point, hit.normal, hit.collider);
                }
            }
        }
    }

    void Update()
    {
        // Move

        transform.position += m_Velocity * Time.deltaTime;

        if (inheritWeaponVelocity)
        {
            transform.position += m_ProjectileBase.inheritedMuzzleVelocity * Time.deltaTime;
        }

        // Drift towards trajectory override (this is so that projectiles can be centered 
        // with the camera center even though the actual weapon is offset)
        if (m_HasTrajectoryOverride && m_ConsumedTrajectoryCorrectionVector.sqrMagnitude < m_TrajectoryCorrectionVector.sqrMagnitude)
        {
            Vector3 correctionLeft = m_TrajectoryCorrectionVector - m_ConsumedTrajectoryCorrectionVector;
            float distanceThisFrame = (root.position - m_LastRootPosition).magnitude;
            Vector3 correctionThisFrame = (distanceThisFrame / trajectoryCorrectionDistance) * m_TrajectoryCorrectionVector;
            correctionThisFrame = Vector3.ClampMagnitude(correctionThisFrame, correctionLeft.magnitude);
            m_ConsumedTrajectoryCorrectionVector += correctionThisFrame;

            // Detect end of correction
            if(m_ConsumedTrajectoryCorrectionVector.sqrMagnitude == m_TrajectoryCorrectionVector.sqrMagnitude)
            {
                m_HasTrajectoryOverride = false;
            }

            transform.position += correctionThisFrame;
        }

        // Orient towards velocity
        transform.forward = m_Velocity.normalized;

        // Gravity
        if (gravityDownAcceleration > 0)
        {
            // add gravity to the projectile velocity for ballistic effect
            m_Velocity += Vector3.down * gravityDownAcceleration * Time.deltaTime;
        }

        if(homing)
        {    
            //skip hit detection if was spawned 0.2 seconds ago    
            if(Time.time - spawnTime < 0.2f)
            {
                m_LastRootPosition = root.position;
                return;
            }
            else if(m_TargetTransform) //set velocity towards homing target if it exists
            {
                Vector3 towardsTarget = m_TargetTransform.position - transform.position;
                m_Velocity = Vector3.RotateTowards(m_Velocity, towardsTarget, deltaTheta, 0.0f);
            }
            else //find homing target
            {
                FindHomingTarget();
            }
        }

        // Hit detection
        {
            RaycastHit closestHit = new RaycastHit();
            closestHit.distance = Mathf.Infinity;
            bool foundHit = false;

            // Sphere cast
            Vector3 displacementSinceLastFrame = tip.position - m_LastRootPosition;
            RaycastHit[] hits = Physics.SphereCastAll(m_LastRootPosition, radius, displacementSinceLastFrame.normalized, displacementSinceLastFrame.magnitude, hittableLayers, k_TriggerInteraction);
            foreach (var hit in hits)
            {
                if (IsHitValid(hit) && hit.distance < closestHit.distance)
                {
                    foundHit = true;
                    closestHit = hit;
                }
            }

            if (foundHit)
            {
                // Handle case of casting while already inside a collider
                if(closestHit.distance <= 0f)
                {
                    closestHit.point = root.position;
                    closestHit.normal = -transform.forward;
                }

                OnHit(closestHit.point, closestHit.normal, closestHit.collider);
            }
        }

        m_LastRootPosition = root.position;
    }

    void FindHomingTarget()
    {
        //spherecast around the projectile to find a target
        // RaycastHit[] hits = Physics.SphereCastAll(transform.position, 100f, Vector3.zero, 0f, 1 << LayerMask.NameToLayer("Enemies"), k_TriggerInteraction);
        Collider[] hits = Physics.OverlapSphere(transform.position, 100f, 1 << LayerMask.NameToLayer("Enemies"), k_TriggerInteraction);
        System.Random rng = new System.Random();
        m_TargetTransform = null;

        if(hits.Length != 0)
        {
            int rand;
            int tries = 0;
            while (tries++ < 10)
            {
                //choose a random target in hits
                rand = rng.Next(0, hits.Length);
                //set target transform if the target is a hurtbox
                if(hits[rand].tag == "Hurtbox")
                {
                    m_TargetTransform = hits[rand].transform;
                    break;
                }
            }
        }
    }

    bool IsHitValid(RaycastHit hit)
    {
        // ignore hits with an ignore component
        if(hit.collider.GetComponent<IgnoreHitDetection>())
        {
            return false;
        }

        // ignore hits with triggers that don't have a Damageable component
        if(hit.collider.isTrigger && hit.collider.GetComponent<Damageable>() == null)
        {
            return false;
        }

        // ignore hits with specific ignored colliders (self colliders, by default)
        if (m_IgnoredColliders.Contains(hit.collider))
        {
            return false;
        }

        return true;
    }

    void OnHit(Vector3 point, Vector3 normal, Collider collider)
    { 
        Damageable damageable = null;
        // damage
        if (areaOfDamage)
        {
            // area damage
            areaOfDamage.InflictDamageInArea(damage, point, hittableLayers, k_TriggerInteraction, m_ProjectileBase.owner);
        }
        else
        {
            // point damage
            damageable = collider.GetComponent<Damageable>();
            if (damageable)
            {
                damageable.InflictDamage(damage, false, m_ProjectileBase.owner);
                m_ProjectileBase.OnHit(m_Velocity);
            }
        }

        // impact vfx
        if (impactVFX)
        {
            GameObject impactVFXInstance = Instantiate(impactVFX, point + (normal * impactVFXSpawnOffset), Quaternion.LookRotation(normal));
            if (impactVFXLifetime > 0)
            {
                Destroy(impactVFXInstance.gameObject, impactVFXLifetime);
            }
        }

        // impact sfx
        if (impactSFXClip)
        {
            if(damageable && damageable.damageMultiplier > 0f)
            {
                AudioUtility.CreateSFX(damageSFXClip, point, AudioUtility.AudioGroups.Impact, 0.6f, 1f, 100f, 1f);
            }
            else
            {
                AudioUtility.CreateSFX(impactSFXClip, point, AudioUtility.AudioGroups.Impact, 1f, 3f, 5f);
            }
        }

        // Self Destruct
        Destroy(this.gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = radiusColor;
        Gizmos.DrawSphere(transform.position, radius);
    }
}