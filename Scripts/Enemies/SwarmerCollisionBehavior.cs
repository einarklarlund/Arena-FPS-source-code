using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwarmerCollisionBehavior : MonoBehaviour
{
    //enemies will ignore collisions from the stage objects that aren't of same enemy type (tag)
    void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.GetComponent<PlayerCharacterController>() != null)
        {
            Damageable damageable = collision.gameObject.GetComponent<Damageable>();
            
            if(damageable != null)
            {
                damageable.InflictDamage(1, false, gameObject);
            }
            else
            {
                Debug.LogWarning("[EnemyBehavior] tried to damage player on collision enter but could not find Damageable component");
            }
        }
        else if (collision.gameObject.tag != gameObject.tag && collision.gameObject.layer != LayerMask.NameToLayer("Stage"))
        {
            Debug.Log("ignoring collision between " + gameObject.tag + " " + collision.gameObject.tag);
            Physics.IgnoreCollision(collision.collider, transform.Find("Hitbox").GetComponent<Collider>());
            
            Transform hurtboxTransform = transform.Find("Hurtbox");
            Transform hitboxTransform = transform.Find("Hitbox");
            Transform otherHurtboxTransform = collision.gameObject.transform.Find("Hurtbox");
            Transform otherHitboxTransform = collision.gameObject.transform.Find("Hitbox");
            
            if(otherHurtboxTransform != null)
                Physics.IgnoreCollision(collision.collider, otherHurtboxTransform.GetComponent<Collider>());
            if(otherHitboxTransform != null)
                Physics.IgnoreCollision(collision.collider, otherHitboxTransform.GetComponent<Collider>());
        }
    }

    void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.tag != gameObject.tag && collision.gameObject.layer != LayerMask.NameToLayer("Stage"))
        {
            Transform otherHurtboxTransform = collision.gameObject.transform.Find("Hurtbox");
            Transform otherHitboxTransform = collision.gameObject.transform.Find("Hitbox");

            if(otherHurtboxTransform != null)
                Physics.IgnoreCollision(collision.collider, otherHurtboxTransform.GetComponent<Collider>());
            if(otherHitboxTransform != null)
                Physics.IgnoreCollision(collision.collider, otherHitboxTransform.GetComponent<Collider>());
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.tag != gameObject.tag && collision.gameObject.layer != LayerMask.NameToLayer("Stage"))
        {
            Transform otherHurtboxTransform = collision.gameObject.transform.Find("Hurtbox");
            Transform otherHitboxTransform = collision.gameObject.transform.Find("Hitbox");

            if(otherHurtboxTransform != null)
                Physics.IgnoreCollision(collision.collider, otherHurtboxTransform.GetComponent<Collider>());
            if(otherHitboxTransform != null)
              Physics.IgnoreCollision(collision.collider, otherHitboxTransform.GetComponent<Collider>());
        }
    }
}
