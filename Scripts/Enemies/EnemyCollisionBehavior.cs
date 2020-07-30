using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyCollisionBehavior : MonoBehaviour
{
    //hitboxes are set as triggers, hurtboxes are set as triggers too (except for swarmer enemy). this means that hurtboxes can kill player too.
    void OnTriggerEnter(Collider other)
    {
        if(other.GetComponent<PlayerCharacterController>() != null)
        {
            Damageable damageable = other.GetComponent<Damageable>();
            
            if(damageable != null)
            {
                damageable.InflictDamage(1, false, gameObject);
            }
            else
            {
                Debug.LogWarning("[EnemyBehavior] tried to damage player on collision enter but could not find Damageable component");
            }
        }
    }

    //sometimes, hurtboxes are not set as triggers. in this case, enemies of same type can collide w one another.
    void OnCollisionEnter(Collision collision)
    {
        if(collision.collider.gameObject.GetComponent<PlayerCharacterController>() != null)
        {
            Damageable damageable = collision.collider.gameObject.GetComponent<Damageable>();
            
            if(damageable != null)
            {
                damageable.InflictDamage(1, false, gameObject);
            }
            else
            {
                Debug.LogWarning("[EnemyBehavior] tried to damage player on collision enter but could not find Damageable component");
            }
        }
        
        // if(collision.collider.gameObject.tag != gameObject.tag && collision.collider.gameObject.tag != "Stage")
        // {
        //     foreach(Collider collider in GetComponentsInChildren<Collider>())
        //         Physics.IgnoreCollision(collider, collision.collider);
        // }        
    }
}
