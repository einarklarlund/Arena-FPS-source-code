using UnityEngine;
using UnityEngine.Events;

public class ProjectileBase : MonoBehaviour
{
    public GameObject owner { get; private set; }
    public Vector3 initialPosition { get; private set; }
    public Vector3 initialDirection { get; private set; }
    public Vector3 inheritedMuzzleVelocity { get; private set; }
    public float initialCharge { get; private set; }
    [Tooltip("the chance of spawning a missile on damage")]
    public float chanceToSpawnMissile = 0f;
    [Tooltip("the missile prefab to spawn")]
    public ProjectileBase missile;

    public UnityAction onShoot;

    public void Shoot(GameObject controller)
    {
        initialPosition = transform.position;
        initialDirection = transform.forward;
        WeaponController weaponController = controller.GetComponent<WeaponController>();

        if(weaponController)
        {
            owner = weaponController.owner;
            inheritedMuzzleVelocity = weaponController.muzzleWorldVelocity;
            initialCharge = weaponController.currentCharge;
            chanceToSpawnMissile = weaponController.chanceToSpawnMissile;
        }
        else
        {
            ProjectileBase projectileBase = controller.GetComponent<ProjectileBase>();
            if(projectileBase)
            {
                owner = controller;
                inheritedMuzzleVelocity = new Vector3(0,0,0);
                initialCharge = 0;
                chanceToSpawnMissile = projectileBase.chanceToSpawnMissile;
            }
            else
            {
                Debug.LogError("[ProjectileBase] project spawned without a projectilebase or weaponcontroller owner");
            }
        }

        if (onShoot != null)
        {
            onShoot.Invoke();
        }
    }

    public void OnHit(Vector3 currVelocity)
    {
        if(UnityEngine.Random.value < chanceToSpawnMissile)
        {
            Vector3 newDirection = Vector3.RotateTowards(Vector3.up, -1 * currVelocity, 0.08f, 0.0f);
            ProjectileBase newProjectile = Instantiate(missile, transform.position, Quaternion.LookRotation(newDirection));
            newProjectile.Shoot(gameObject);
        }
    }
}
