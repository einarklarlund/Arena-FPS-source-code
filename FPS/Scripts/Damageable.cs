using UnityEngine;

public class Damageable : MonoBehaviour
{
    [Tooltip("Multiplier to apply to the received damage")]
    public float damageMultiplier = 1f;
    [Range(0, 1)]
    [Tooltip("Multiplier to apply to self damage")]
    public float sensibilityToSelfdamage = 0.5f;

    public Health health { get; private set; }

    void Awake()
    {
        // find the health component either at the same level, or higher in the hierarchy
        Transform currTransform = transform;
        for(int i = 0 ; i < 5; ++i)
        {
            if(!health)
                health = currTransform.GetComponent<Health>();

            if(currTransform)
                currTransform = currTransform.parent;
        }

        if(!health)
        {
            Debug.LogWarning("[Damageable] could not find health object for gameobject with tag " + gameObject.tag + " and name " + gameObject.name);
        }
    }

    public void InflictDamage(float damage, bool isExplosionDamage, GameObject damageSource)
    {

        if(health && gameObject.layer != LayerMask.NameToLayer("DontHit"))
        {
            var totalDamage = damage;

            // skip the crit multiplier if it's from an explosion
            if (!isExplosionDamage)
            {
                totalDamage *= damageMultiplier;
            }

            // potentially reduce damages if inflicted by self
            if (health.gameObject == damageSource)
            {
                totalDamage *= sensibilityToSelfdamage;
            }

            // apply the damages
            health.TakeDamage(totalDamage, damageSource);
        }
    }
}
