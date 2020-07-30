using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackPickup : Pickup
{
    protected override void HandleOnPick(PlayerUpgradeManager upgradeManager)
    {
        upgradeManager.AddAttackPickup();
    }

    protected override void OnStart()
    {
        Vector3 initialVelocity = _playerTransform.position - transform.position;
        initialVelocity = initialVelocity.normalized * _maxVelocity;
        _pickupRigidbody.AddForce(initialVelocity * _pickupRigidbody.mass / Time.fixedDeltaTime);
    }
}
