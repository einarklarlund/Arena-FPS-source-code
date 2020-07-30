using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwarmerBehavior : EnemyBehavior
{
    [Header("Enemy properties")]
    [Tooltip("Change in angle (radians) of thrust vector between FixedUpdate calls")]
    [SerializeField] private float _deltaTheta = 0.0225f;
    [SerializeField] private float _maxVelocity = 10; 
    [SerializeField] private GameObject _hitbox = null;
    [SerializeField] private GameObject _hurtbox = null;
    [SerializeField] private Transform _eyeballTransform = null;
    [SerializeField] private Rigidbody _rigidbody = null;
    
    private float _deltaThetaLook;
    private Transform _playerTransform = null;
    private float _maxForce; 

    void Start()
    {
        //spawn duration will last 30 frames
        _spawnDuration = 30f * Time.fixedDeltaTime;
        //death duration will last 10 frames
        _deathDuration = 10f * Time.fixedDeltaTime;

        _maxForce = _maxVelocity / 3 * _rigidbody.mass / Time.fixedDeltaTime;

        _deltaThetaLook = _deltaTheta * 2f;

        gameObject.transform.Translate(initialPosition);
        
        _playerTransform = GameObject.FindWithTag("Player").transform.Find("Main Camera");

        _enemyAnimator.Stop();
        _enemyAnimator.Play("SwarmerSpawn");
        
        if(_spawnClip)
            AudioUtility.CreateSFX(_spawnClip, transform.position, AudioUtility.AudioGroups.EnemySpawn, 0.6f, 30f, 500f, 0.4f);
    }

    public override void SpawnBehavior(float timeSpentInCurrentState)
    {
        if(!_hitbox.activeSelf && timeSpentInCurrentState <= Time.fixedDeltaTime)
        {
            _hitbox.SetActive(true);
            _hurtbox.SetActive(true);
        }

        if(timeSpentInCurrentState >= _spawnDuration)
        {
            OnSpawnComplete.Invoke();
        }

        if(timeSpentInCurrentState < Time.fixedDeltaTime && !initialPosition.Equals(default(Vector3)))
        {
            //spawnforce is calculated by the tower that instantiates the Swarmer
            float multiplier = _maxVelocity * _rigidbody.mass / Time.fixedDeltaTime;
            Vector3 spawnForce = initialDirection;
            spawnForce.Scale(new Vector3(multiplier, multiplier, multiplier));
            _rigidbody.AddForce(spawnForce);
        }
        else
        {
            MoveTowardsPlayer();
            LookTowardsPlayer();
        }
    }

    public override void AttackBehavior(float timeSpentInCurrentState)
    {
        MoveTowardsPlayer();
        LookTowardsPlayer();
    }

    public override void DeathBehavior(float timeSpentInCurrentState)
    {
        if(timeSpentInCurrentState < Time.fixedDeltaTime)
        {
            _enemyAnimator.Stop();
            _enemyAnimator.Play("SwarmerDeath");
            
            //ignore all collision
            _rigidbody.detectCollisions = false;

            //move to DontHit layer (so that dying enemy doesn't get hit by projectiles)
            gameObject.layer = LayerMask.NameToLayer("DontHit");
            _hitbox.layer = LayerMask.NameToLayer("DontHit");

            if(_deathClip)
                AudioUtility.CreateSFX(_deathClip, transform.position, AudioUtility.AudioGroups.EnemySpawn, 0.6f, 5f, 500f, 1f);
        }

        //fade out idle audio during deathDuration (idle audio starts at volume 0.1)
        _mainAudioSource.volume = Mathf.Lerp(0.1f, 0f, (Time.time - timeSpentInCurrentState) / _deathDuration );
        
        if(timeSpentInCurrentState >= _deathDuration)
        {
            OnDeathComplete.Invoke();
        }
    }

    private void MoveTowardsPlayer()
    {
        Vector3 towardsPlayer = _playerTransform.position - _rigidbody.position;

        // newDirection will be the direction of the velocity that we want to add to the Swarmer
        Vector3 newDirection = Vector3.RotateTowards(_rigidbody.velocity.normalized, towardsPlayer, _deltaTheta, _maxVelocity);

        //scale unit vector of newDirection by max velocity to get target velocity
        Vector3 targetVelocity = newDirection.normalized;
        targetVelocity *= _maxVelocity;

        ChangeVelocity(targetVelocity);
    }

    private void LookTowardsPlayer()
    {
        //calculate the new direcion for the eyeball
        Vector3 towardsPlayer = _playerTransform.position - _rigidbody.position;
        Vector3 newLookDirection =  Vector3.RotateTowards(_rigidbody.velocity.normalized, towardsPlayer, _deltaThetaLook, 1f);

        //set the eyeball to look towards the player
        _eyeballTransform.eulerAngles = new Vector3(_eyeballTransform.eulerAngles.x, _eyeballTransform.eulerAngles.y, 0f); //reset z rotation to 0 degrees in case it has changed
        Quaternion angularDisplacement = Quaternion.FromToRotation(_eyeballTransform.forward, newLookDirection);
        _eyeballTransform.Rotate(angularDisplacement.eulerAngles.x, angularDisplacement.eulerAngles.y, angularDisplacement.eulerAngles.z, Space.World);
    }

    private void ChangeVelocity(Vector3 targetVelocity)
    {
        //we need to calculate a velocity component such that when it is added to _rigidbody.velocity, we get targetVelocity (current velocity - targetvelocity)
        Vector3 newVelocityComponent = targetVelocity - _rigidbody.velocity;
        //scale newVelocityComponent by mass/deltaTime to find the force necessary to acheive targetVelocity in the next FixedUpdate
        Vector3 force = (newVelocityComponent) * _rigidbody.mass / Time.fixedDeltaTime;

        _rigidbody.AddForce(Vector3.ClampMagnitude(force, _maxForce));
    }
}
