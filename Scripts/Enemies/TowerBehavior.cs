using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerBehavior : EnemyBehavior
{
    [Header("Components")]
    [SerializeField] private Rigidbody _rigidbody = null;
    [SerializeField] private GameObject _hitbox = null;
    [SerializeField] private GameObject _hurtbox = null;
    [SerializeField] private Transform _eyeballTransform = null;
    [SerializeField] private Transform _bodyTransform = null;
    [SerializeField] private Health _health = null;
    [SerializeField] private EyeballColorController _eyeballColorController = null;
    private Transform _centerStageTransform = null;
    private Transform _playerTransform = null;
    
    [Header("Values")]
    [SerializeField] private float _maxVelocity = 3;
    [SerializeField] private float _distanceFromCenterStage = 15;
    [Tooltip("ratio of diplacement from centerstage on spawn to radius of the stage")]
    [SerializeField] private float _displacementToRadiusRatio = 0.95f;
    [Tooltip("rotational speed (rad/frame) at which the body rotates")]
    [SerializeField] private float _deltaThetaBody = 0.1f;
    [Tooltip("rotational speed (deg/frame) at which the eyeball rotates")]
    private float _deltaThetaLook = 0.03f;

    [Header("Prefabs")]
    [SerializeField] private GameObject _pickup = null;
    [SerializeField] private EnemyCard _swarmerCard = null;

    
    //swarmer spawn variables
    private int _numSwarmerSpawns = 10;
    private float _spawnInterval;
    private float _durationOfSwarmerSpawns;
    private float _lastSpawnTime = 0f;

    void Start()
    {
        //spawn enemies every 5 frames 
        _spawnInterval = 5f * Time.fixedDeltaTime;

        _durationOfSwarmerSpawns = (float) _spawnInterval * _numSwarmerSpawns;

        _centerStageTransform = Stage.Instance.transform.Find("CenterStage");
        _playerTransform = GameObject.FindWithTag("Player").transform;

        //set initial position
        float displacementFromCenterStage = _displacementToRadiusRatio * Stage.Instance.radius;
        Vector2 randomPoint = GameFlowManager.Instance.currentRound <= 1 ? new Vector2(0,1) : UnityEngine.Random.insideUnitCircle;
        Vector3 spawnPosition = new Vector3(randomPoint.normalized.x * displacementFromCenterStage, 0, randomPoint.normalized.y * displacementFromCenterStage);
        gameObject.transform.Translate(spawnPosition);
        
        //set initial eyeball rotation
        Vector3 awayFromPlayer = _eyeballTransform.position - _playerTransform.position;
        _eyeballTransform.rotation = Quaternion.LookRotation(awayFromPlayer);

        _health.onDamaged += HandleOnDamaged;

        _enemyAnimator.Stop();
        _enemyAnimator.Play("TowerSpawn");
        
        if(_spawnClip)
            AudioUtility.CreateSFX(_spawnClip, transform.position, AudioUtility.AudioGroups.EnemySpawn, 0.6f, 5f, 500f, 0.75f);
    }

    public override void SpawnBehavior(float timeSpentInCurrentState)
    {
        //activate the hitbox after 1 second has passed so that the player wont get killed immediately if a tower spawns beneath 
        if(!_hitbox.activeSelf && timeSpentInCurrentState >= 1)
        {        
            _hitbox.SetActive(true);
        }

        if(timeSpentInCurrentState >= _spawnDuration)
        {
            OnSpawnComplete.Invoke();
        }

        LookTowardsPlayer();
        MoveTower();
    }
    
    public override void AttackBehavior(float timeSpentInCurrentState)
    {
        int framesSpentInState = (int) (timeSpentInCurrentState / Time.fixedDeltaTime);
        int attackIntervalInFrames = (int) (_attackInterval / Time.fixedDeltaTime);
        if(!_hurtbox.activeSelf && timeSpentInCurrentState <= Time.fixedDeltaTime)
        {
            _hurtbox.SetActive(true);
        }

        if(framesSpentInState % attackIntervalInFrames <= _durationOfSwarmerSpawns / Time.fixedDeltaTime
            && timeSpentInCurrentState - _lastSpawnTime >= _spawnInterval)
        {     
            _lastSpawnTime = timeSpentInCurrentState;

            //calculate inital direction of the new Swarmer
            Vector3 initialDirection = new Vector3(0, 1, 0);
            Vector3 towardsPlayer = _playerTransform.position - _rigidbody.transform.position;
            Vector3 perpendicularToPlayer = Vector3.Cross(towardsPlayer, initialDirection * (float) Math.Pow(-1, framesSpentInState));
            Vector3 randomDeviation = new Vector3(UnityEngine.Random.value * 0.3f, UnityEngine.Random.value * 0.3f, UnityEngine.Random.value * 0.3f);
            initialDirection = Vector3.RotateTowards(initialDirection, towardsPlayer, 0.03f, 0f);
            initialDirection = Vector3.RotateTowards(initialDirection, perpendicularToPlayer, 0.3f, 0f);
            initialDirection += randomDeviation;

            //initial x of the swarmer will either be 0.6 or -0.6 away from center of tower
            float initialX = (float) Math.Round(UnityEngine.Random.value * 2f - 1f) * -0.6f;
            Vector3 initialPosition = _rigidbody.position + new Vector3(initialX, 1f, 0f);

            EnemyManager.Instance.SpawnEnemy(_swarmerCard, initialPosition, initialDirection);
        }

        _bodyTransform.Rotate(0f, _deltaThetaBody, 0f);

        LookTowardsPlayer();
        MoveTower();
    }

    public override void DeathBehavior(float timeSpentInCurrentState)
    {
        if(timeSpentInCurrentState < Time.fixedDeltaTime)
        {
            _enemyAnimator.Stop();
            _enemyAnimator.Play("TowerDeath");
            
            //ignore all collision
            _rigidbody.detectCollisions = false;

            //move to DontHit layer (so that dying enemy doesn't get hit by projectiles)
            gameObject.layer = LayerMask.NameToLayer("DontHit");
            _hitbox.layer = LayerMask.NameToLayer("DontHit");
            _hurtbox.layer = LayerMask.NameToLayer("DontHit");

            //spawn pickup
            PickupManager.Instance.ChooseDeathPickup(_eyeballTransform.position);

            if(_deathClip)
                AudioUtility.CreateSFX(_deathClip, transform.position, AudioUtility.AudioGroups.EnemySpawn, 0.6f, 5f, 500f, 1f);
        }

        if(timeSpentInCurrentState >= _deathDuration)
        {
            OnDeathComplete.Invoke();
        }

        //fade out idle audio during deathDuration (idle audio starts at volume 0.3)
        _mainAudioSource.volume = Mathf.Lerp(0.3f, 0f, (Time.time - timeSpentInCurrentState) / _deathDuration );

        MoveTower();
    }

    void HandleOnDamaged(float trueDamage, GameObject damageSource)
    {
        if(_health.currentHealth != 0)
        {
            // _enemyAnimator.Stop();
            // _enemyAnimator.Play("TowerDamaged");
            _eyeballColorController.TakeDamage();
        }
    }

    private void MoveTower()
    {
        Vector3 currentVelocity = _rigidbody.velocity.normalized;
        //find the tangent around the center stage by crossing towardsCenterStage and upwards direction
        Vector3 towardsCenterStage = _centerStageTransform.position - _rigidbody.position;
        towardsCenterStage.y = 0;
        Vector3 targetVelocity = new Vector3(0, 0, 0);

        //set target velocity to be perpendicular to towardsCenterStage or towardsCenterStage
        if(towardsCenterStage.magnitude <= _distanceFromCenterStage)
        {
            Vector3 perpendicularToCenterStage = Vector3.Cross(towardsCenterStage, new Vector3(0, 1, 0));
            targetVelocity.y = 0;
            targetVelocity = perpendicularToCenterStage.normalized;
        }
        else
        {
            targetVelocity.y = 0;
            targetVelocity = towardsCenterStage.normalized;
        }

        //scale unit vector of perpendicularToCenterStage by max velocity to get target velocity
        targetVelocity *= _maxVelocity;

        ChangeVelocity(targetVelocity);
    }

    private void LookTowardsPlayer()
    {
        //calculate the new direcion for the eyeball
        Vector3 towardsPlayer = _playerTransform.position - _eyeballTransform.position;
        Vector3 newLookDirection =  Vector3.RotateTowards(_eyeballTransform.forward, towardsPlayer, _deltaThetaLook, 1f);

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

        _rigidbody.AddForce(force);
    }
}
