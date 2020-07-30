using UnityEngine;
using UnityEngine.Events;
using System;

public abstract class Pickup : MonoBehaviour
{
    [Header("Pickup properties")]
    [SerializeField] protected Rigidbody _pickupRigidbody = null;
    [SerializeField] protected Animation _pickupAnimator = null;
    [SerializeField] Collider _rigidbodyCollider = null;
    [SerializeField] protected float _maxVelocity = 10f;
    [SerializeField] protected float _minimumVelocity = 0.1f;
    [SerializeField] protected float _lifetime = 12f;
    [Tooltip("Whether enemies can affect the trajectory of the pickup")]
    public bool affectedByEnemies = false;
    public Vector3 initialPosition;
    protected Transform _playerTransform = null;

    [Header("Effects")]
    [Tooltip("Sound played on pickup")]
    public AudioClip pickupSFX;
    [Tooltip("VFX spawned on pickup")]
    public GameObject pickupVFXPrefab;

    public Events.EventPickup OnPick;
    
    protected bool m_HasPlayedFeedback = false;
    protected bool _attractedToEnemy = false;


    protected abstract void HandleOnPick(PlayerUpgradeManager upgradeController);
    protected abstract void OnStart();

    private void Start()
    {
        OnPick.AddListener(HandleOnPick);
        OnPick.AddListener(PlayPickupFeedback);
        
        gameObject.transform.Translate(initialPosition);
        // gameObject.transform.position = initialPosition;

        _playerTransform = GameObject.FindWithTag("Player").transform;

        _pickupAnimator.Play("PickupFlashDelayed");

        //destroy pickup after _lifetime seconds
        Destroy(gameObject, _lifetime);

        //call overrideable onstart function so that start behavior can be modified w/o modifying start()
        OnStart();
    }

    private void FixedUpdate()
    {
        if(affectedByEnemies && _attractedToEnemy)
        {
            MoveTowardsEnemy();
        }
        else if(!PlayerInputHandler.Instance.GetFireInputHeld())
        {
            MoveTowardsPlayer();
        }
        else if(_pickupRigidbody.velocity.magnitude > _minimumVelocity)
        {
            ChangeVelocity(_pickupRigidbody.velocity * 0.995f);
        }
        else if(_pickupRigidbody.velocity.magnitude != 0)
        {
            ChangeVelocity(new Vector3(0, 0, 0));
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.collider.tag != "Stage")
        {
            Physics.IgnoreCollision(_rigidbodyCollider, collision.collider);
        }
    }

    // this method shouldnt really take an upgradeController but it was the best way for me to attach it to the onpick event while keeping handleonpick as an abstract method
    public void PlayPickupFeedback(PlayerUpgradeManager upgradeController)
    {
        if (m_HasPlayedFeedback)
            return;

        if (pickupSFX)
        {
            AudioUtility.CreateSFX(pickupSFX, transform.position, AudioUtility.AudioGroups.Pickup, 0f);
        }

        if (pickupVFXPrefab)
        {
            var pickupVFXInstance = Instantiate(pickupVFXPrefab, transform.position, Quaternion.identity);
        }

        m_HasPlayedFeedback = true;

        Destroy(gameObject);
    }

    private void MoveTowardsEnemy()
    {
        
    }

    private void MoveTowardsPlayer()
    {
        Vector3 towardsPlayer = _playerTransform.position - _pickupRigidbody.position;
        // newDirection will be the direction of the velocity that we want to add to the Swarmer
        Vector3 newDirection = Vector3.RotateTowards(_pickupRigidbody.velocity.normalized, towardsPlayer, (float) Mathf.PI * 2f, _maxVelocity);

        //scale unit vector of newDirection by max velocity to get target velocity
        Vector3 targetVelocity = newDirection.normalized;
        targetVelocity *= _maxVelocity;

        ChangeVelocity(targetVelocity);
    }

    private void ChangeVelocity(Vector3 targetVelocity)
    {
        //we need to calculate a velocity component such that when it is added to _rigidbody.velocity, we get targetVelocity (current velocity - targetvelocity)
        Vector3 newVelocityComponent = targetVelocity - _pickupRigidbody.velocity;
        //scale newVelocityComponent by mass/deltaTime to find the force necessary to acheive targetVelocity in the next FixedUpdate
        Vector3 force = (newVelocityComponent) * _pickupRigidbody.mass / Time.fixedDeltaTime;

        _pickupRigidbody.AddForce(force);
    }
}
