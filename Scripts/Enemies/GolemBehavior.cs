using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GolemBehavior : EnemyBehavior
{
    [Header("Enemy properties")]
    [SerializeField] private GameObject _hitbox = null;
    [SerializeField] private GameObject _headHitbox = null;
    [SerializeField] private GameObject _hurtbox = null;
    [SerializeField] private Health _health = null;
    [SerializeField] private GameObject _pickup = null;
    [Tooltip("ratio of diplacement from centerstage on spawn to radius of the stage")]
    [SerializeField] private float _displacementToRadiusRatio = 28f/50f;
    [SerializeField] private EyeballColorController _eyeballColorController = null;

    [Header("Rotation/aiming variables")]
    [SerializeField] private Transform _bodyTransform = null;
    [SerializeField] private Rigidbody _bodyRigidbody = null;
    [SerializeField] private Transform _headTransform = null;
    [SerializeField] private Transform _eyeTransform = null;
    [Tooltip("the minimum distance between player and golem at which the golem's aim can keep up with player's max ground speed")]
    [SerializeField] private float _aimFollowsPlayerDistance = 20f;
    private Transform _centerStageTransform = null;
    private float _deltaThetaHead;
    private float _deltaThetaBody;

    [Header("Laser variables")]
    [SerializeField] private AudioClip _attackClip = null;
    [SerializeField] private LineRenderer _aimLaser = null;
    [SerializeField] private LineRenderer _attackLaser = null;
    [SerializeField] AudioSource _laserHum = null;
    private Renderer _aimLaserRenderer = null;
    private float _lastShotTime;
    private float _laserLength = 100f;
    private float _attackLaserLifetime = 0.15f;

    [Header("Movement variables")]
    [Tooltip("initial velocity of each step towards the player that the golem takes")]
    [SerializeField] private float _stepVelocity = 1f;
    [Tooltip("time spent waiting while velocity = 0 after taking a step towards the player")]
    [SerializeField] private float _timeBetweenSteps = 0.5f; 
    private float _mininumVelocity = 0.1f;
    private float _stepEndTime;

    private Transform _playerTransform = null;

    // Start is called before the first frame update
    void Start()
    {   
        GameObject playerGameObject = GameObject.FindWithTag("Player");
        _playerTransform = playerGameObject.transform;

        _aimLaserRenderer = _aimLaser.gameObject.GetComponent<Renderer>(); 

        _centerStageTransform = Stage.Instance.transform.Find("CenterStage");

        //set deltaThetaHead such that the rotation of the golem's head is enough to follow player.MaxSpeedOnGround when player is _aimFollowsPlayerDistance units away from golem
        float playerMaxSpeedOnGround = playerGameObject.GetComponent<PlayerCharacterController>().maxSpeedOnGround; 
        _deltaThetaHead = (float) Mathf.Atan((playerMaxSpeedOnGround * Time.fixedDeltaTime) / _aimFollowsPlayerDistance);
        
        //set initial position
        float displacementFromCenterStage = _displacementToRadiusRatio * Stage.Instance.radius;
        Vector2 randomPoint = GameFlowManager.Instance.currentRound <= 1 ? new Vector2(0,1) : UnityEngine.Random.insideUnitCircle;
        Vector3 spawnPosition = new Vector3(randomPoint.normalized.x * displacementFromCenterStage, 0, randomPoint.normalized.y * displacementFromCenterStage);
        gameObject.transform.Translate(spawnPosition);

        //set initial rotation
        Vector3 towardsCenterStage = _centerStageTransform.position - _headTransform.position;
        Quaternion initialRotation = Quaternion.FromToRotation(_headTransform.forward, towardsCenterStage); 
        initialRotation.eulerAngles = new Vector3(initialRotation.eulerAngles.x, initialRotation.eulerAngles.y, 0f);
        _headTransform.rotation = initialRotation;
        _bodyTransform.rotation = initialRotation;
        // _headTransform.Rotate(initialRotation.eulerAngles.x, initialRotation.eulerAngles.y, initialRotation.eulerAngles.z);
        // _headTransform.rotation = initialRotation;

        _health.onDamaged += HandleOnDamaged;

        _enemyAnimator.Stop();
        _enemyAnimator.Play("GolemSpawn");
        
        if(_spawnClip)
            AudioUtility.CreateSFX(_spawnClip, transform.position, AudioUtility.AudioGroups.EnemySpawn, 0.6f, 5f, 500f, 1f);
    }

    public override void SpawnBehavior(float timeSpentInCurrentState)
    {
        //activate the hitbox after 2 seconds has passed so that the player wont get killed immediately if a golem spawns beneath them
        if(!_hitbox.activeSelf && timeSpentInCurrentState >= 2)
        {        
            _hitbox.SetActive(true);
            _headHitbox.SetActive(true);
        }

        if(timeSpentInCurrentState >= _spawnDuration)
        {
            _stepEndTime = Time.time;
            _lastShotTime = Time.time;
            _aimLaser.gameObject.SetActive(true);
            OnSpawnComplete.Invoke();
            _hurtbox.SetActive(true);  
        }
    }

    public override void AttackBehavior(float timeSpentInCurrentState)
    {
        //rotate golem's head towards the player
        AimTowardsPlayer();

        //change aim laser color
        Color currColor = _aimLaserRenderer.material.GetColor("_Color2");
        currColor.a = Mathf.Lerp(0, 1, (Time.time - _lastShotTime) / _attackInterval);
        _aimLaserRenderer.material.SetColor("_Color2", currColor);
        
        if(_attackLaser.gameObject.activeSelf && Time.time - _lastShotTime >= _attackLaserLifetime)
        {
            _attackLaser.gameObject.SetActive(false);
        }

        //shoot a laser every _attackInterval seconds
        if(Time.time - _lastShotTime >= _attackInterval)
        {
            ShootLaser();
            _lastShotTime = Time.time;
        }

        //movement logic
        if(_bodyRigidbody.velocity.magnitude == 0)
        {
            if(Time.time - _stepEndTime >= _timeBetweenSteps)
            {
                StepTowardsPlayer();
                _stepEndTime = 0;
            }
        }
        else if(_bodyRigidbody.velocity.magnitude <= _mininumVelocity)
        {
            ChangeVelocity(new Vector3(0, 0, 0));
            _stepEndTime = Time.time;
        }
        else
        {
            Vector3 towardsPlayer = _playerTransform.position - _bodyRigidbody.position;
            towardsPlayer = towardsPlayer.normalized;
            towardsPlayer *= _bodyRigidbody.velocity.magnitude * 2 / 3;
            ChangeVelocity(_bodyRigidbody.velocity * 0.9f);
        }
    }

    public override void DeathBehavior(float timeSpentInCurrentState)
    {
        if(timeSpentInCurrentState < Time.fixedDeltaTime)
        {
            _enemyAnimator.Stop();
            _enemyAnimator.Play("GolemDeath");

            //ignore all collision
            _bodyRigidbody.detectCollisions = false;

            //move to DontHit layer (so that dying enemy doesn't get hit by projectiles)
            gameObject.layer = LayerMask.NameToLayer("DontHit");
            _hitbox.layer = LayerMask.NameToLayer("DontHit");
            _hurtbox.layer = LayerMask.NameToLayer("DontHit");

            //stop moving
            ChangeVelocity(new Vector3(0, 0, 0));

            //turn off lasers
            _aimLaser.gameObject.SetActive(false);
            _attackLaser.gameObject.SetActive(false);
            _laserHum.gameObject.SetActive(false);

            //spawn pickup
            // PickupManager.Instance.SpawnBasicPickup(_eyeTransform.position);
            Instantiate(_pickup, _eyeTransform.position, Quaternion.identity);

            
            if(_deathClip)
                AudioUtility.CreateSFX(_deathClip, transform.position, AudioUtility.AudioGroups.EnemySpawn, 0.6f, 5f, 500f, 1f);
        }

        _mainAudioSource.volume = Mathf.Lerp(0.3f, 0f, (Time.time - timeSpentInCurrentState) / _deathDuration );

        if(timeSpentInCurrentState >= _deathDuration)
        {
            OnDeathComplete.Invoke();
        }
    }

    void HandleOnDamaged(float trueDamage, GameObject damageSource)
    {
        if(_health.currentHealth != 0)
        {
            _enemyAnimator.Stop();
            _enemyAnimator.Play("GolemDamaged");
            _eyeballColorController.TakeDamage();
        }
    }

    private float MomentOfInertiaAlongAxis(Rigidbody rb, Vector3 axis) 
    {
        axis = Quaternion.Inverse(rb.inertiaTensorRotation) * axis.normalized; //rotating the torque because it's equivalent and more efficient
        Vector3 angularAcceleration = new Vector3(Vector3.Dot(Vector3.right, axis) / rb.inertiaTensor.x, Vector3.Dot(Vector3.up, axis) / rb.inertiaTensor.y, Vector3.Dot(Vector3.forward, axis) / rb.inertiaTensor.z); //calculating the angular acceleration that would result from a torque of 1 Nm (the same way that unity does it)
        return 1 / angularAcceleration.magnitude; //moment of inertia = Torque / angular acceleration
    }
    
    private void AimTowardsPlayer()
    {
        // Vector3 towardsPlayer = _playerTransform.position - _headTransform.position + new Vector3(0f, 0.5f, 0f);
        Vector3 towardsPlayer = _playerTransform.position - _headTransform.position + new Vector3(0f, 0.5f, 0f);
        // newAimDirection will be the direction in which the golem's head will point next
        Vector3 newAimDirection = Vector3.RotateTowards(_headTransform.forward, towardsPlayer, _deltaThetaHead, 1f);
        // newBodyDirection will be the direction in which the golem's body will point next
        Vector3 newBodyDirection = Vector3.RotateTowards(_headTransform.forward, towardsPlayer, _deltaThetaHead * 3f / 4f, 2f);
        
        /*
        //project newAimDirection and _headtransform.forward onto y-z plane to find the angular displacement bewteen the two vectors in x axis
        float xDisplacement = Vector3.Angle(Vector3.ProjectOnPlane(newAimDirection, Vector3.right), Vector3.ProjectOnPlane(_headTransform.forward, Vector3.right));
        xDisplacement = 360 - xDisplacement < xDisplacement ? 360 - xDisplacement : xDisplacement; //mirror the displacement if it results in smaller displacement
        //project newAimDirection and _headtransform.forward onto x-z plane to find the angular displacement bewteen the two vectors in y axis
        float yDisplacement = Vector3.Angle(Vector3.ProjectOnPlane(newAimDirection, Vector3.up), Vector3.ProjectOnPlane(_headTransform.forward, Vector3.up));
        yDisplacement = Mathf.Cos(Vector3.Angle(towardsPlayer, -1 * _headTransform.right) * Mathf.PI / 180f) >= 0 ? -1 * yDisplacement : yDisplacement; //multiply ydisplacement by -1 if its on the golem's right side
        */

        //project newAimDirection and _headtransform.forward onto y-z plane to find the angular displacement bewteen the two vectors in x axis
        // float xDisplacement = Vector3.Angle(Vector3.ProjectOnPlane(towardsPlayer, Vector3.right), Vector3.ProjectOnPlane(_headTransform.forward, Vector3.right));
        // xDisplacement = towardsPlayer.normalized.y < _headTransform.forward.y ? Mathf.Clamp(-1f * xDisplacement, -_deltaThetaHead, 0) : Mathf.Clamp(xDisplacement, 0, _deltaThetaHead); //multiply ydisplacement by -1 if its on the golem's right side

        //project newAimDirection and _headtransform.forward onto x-z plane to find the angular displacement bewteen the two vectors in y axis
        // float yDisplacement = Vector3.Angle(Vector3.ProjectOnPlane(towardsPlayer, Vector3.up), Vector3.ProjectOnPlane(_headTransform.forward, Vector3.up));
        // yDisplacement = Mathf.Cos(Vector3.Angle(towardsPlayer, -1 * _headTransform.right) * Mathf.PI / 180f) >= 0 ? Mathf.Clamp(-1f * yDisplacement, -_deltaThetaHead, 0) : Mathf.Clamp(yDisplacement, 0, _deltaThetaHead); //multiply ydisplacement by -1 if its on the golem's right side
        
        // Vector3 omega = new Vector3(xDisplacement, yDisplacement, 0f) / Time.fixedDeltaTime;
        // _headRigidbody.angularVelocity = _headTransform.InverseTransformVector(omega);
        // float momentOfInertia = MomentOfInertiaAlongAxis(_headRigidbody, Vector3.Cross(_headTransform.forward, _headTransform.forward + omega));
        // _headRigidbody.AddTorque(omega * momentOfInertia);

        
        _headTransform.eulerAngles = new Vector3(_headTransform.eulerAngles.x, _headTransform.eulerAngles.y, 0f); //reset z rotation to 0 degrees in case it has changed
        _bodyTransform.eulerAngles = new Vector3(0f, _bodyTransform.eulerAngles.y, 0f); //reset z and x rotation to 0 degrees in case it has changed

        Quaternion angularDisplacementAim = Quaternion.FromToRotation(_headTransform.forward, newAimDirection);
        Quaternion angularDisplacementBody = Quaternion.FromToRotation(_bodyTransform.forward, newBodyDirection);
        
        _headTransform.Rotate(angularDisplacementAim.eulerAngles.x, angularDisplacementAim.eulerAngles.y, angularDisplacementAim.eulerAngles.z, Space.World);
        _bodyTransform.Rotate(angularDisplacementAim.eulerAngles.x, angularDisplacementAim.eulerAngles.y, angularDisplacementAim.eulerAngles.z, Space.World);

        RaycastHit hit;
        Vector3 laserVector = -1 * _eyeTransform.position;
        if (Physics.Raycast(transform.TransformVector(_eyeTransform.position), transform.TransformVector(_headTransform.forward), out hit, Mathf.Infinity, 1 << LayerMask.NameToLayer("Stage")))
        {
            _aimLaser.SetPosition(1, _eyeTransform.InverseTransformPoint(hit.point));
            // Debug.DrawLine(transform.TransformVector(_eyeTransform.position), hit.point, Color.red, 0.2f);
        }
        else
        {
            _aimLaser.SetPosition(1, _eyeTransform.InverseTransformPoint(transform.TransformVector(_eyeTransform.position) + _headTransform.forward * _laserLength));
        }

        //set the hum audio thingy always be as close to player as possible but on the aim line, so its as if the player is hearing a hum coming from the laser
        Transform humTransform = _laserHum.gameObject.transform;
        Vector3 aimAsVector = _eyeTransform.TransformVector(_aimLaser.GetPosition(1)) - _eyeTransform.TransformVector(_aimLaser.GetPosition(0));
        Vector3 newHumPosition = Vector3.Project(towardsPlayer, aimAsVector);
        //set the hum's position to newHumPosition only if the cos of the angle between the aim and the new position is greater than 0 (the angle must be between -90 and 90 degrees, otherwise newhumposition is behind the golem's head)
        humTransform.position = Mathf.Cos(Vector3.Angle(newHumPosition, _eyeTransform.forward) * Mathf.PI / 180f) > 0 ? newHumPosition + _eyeTransform.position : _eyeTransform.position;
    }

    private void ShootLaser()
    {
        // enable collision w stage and player (in default layer)
        int playerMask = 1 << LayerMask.NameToLayer("Default");
        int stageMask = 1 << LayerMask.NameToLayer("Stage");
        int layerMask = playerMask | stageMask;

        Vector3 eyeToPlayer = _playerTransform.position - _eyeTransform.position + new Vector3(0f, 0.5f, 0f);
        RaycastHit hit;
        // if (Physics.SphereCast(transform.TransformVector(_eyeTransform.position), 0.2f, transform.TransformVector(_headTransform.forward), out hit, Mathf.Infinity, layerMask))
        if (Physics.Raycast(transform.TransformVector(_eyeTransform.position), transform.TransformVector(_headTransform.forward), out hit, Mathf.Infinity, layerMask))
        {
            _attackLaser.SetPosition(1, _eyeTransform.InverseTransformPoint(hit.point));
            _attackLaser.gameObject.SetActive(true);

            if(hit.transform.gameObject.name == "Player")
            {
                hit.transform.gameObject.GetComponent<Health>().TakeDamage(1f, gameObject);
            }
        }
        else
        {    
            _attackLaser.SetPosition(1, _eyeTransform.position + _headTransform.forward * _laserLength);
            _attackLaser.gameObject.SetActive(true);
        }
        
        AudioUtility.CreateSFX(_attackClip, transform.position, AudioUtility.AudioGroups.EnemyAttack, 0.8f, 10f, 100f, 1f);
    }

    private void StepTowardsPlayer()
    {
        Vector3 currentVelocity = _bodyRigidbody.velocity.normalized;
        Vector3 towardsPlayer = _playerTransform.position - _bodyRigidbody.position;
        towardsPlayer.y = 0;
        // newDirection will be the direction of the velocity that we want to add to the Golem
        Vector3 newDirection = Vector3.RotateTowards(currentVelocity, towardsPlayer, (float) Math.PI * 2, _stepVelocity);

        //scale unit vector of newDirection by step velocity to get target velocity
        Vector3 targetVelocity = newDirection.normalized;
        targetVelocity *= _stepVelocity;

        ChangeVelocity(targetVelocity);
    }

    
    private void ChangeVelocity(Vector3 targetVelocity)
    {
        //we need to calculate a velocity component such that when it is added to _bodyRigidbody.velocity, we get targetVelocity (current velocity - targetvelocity)
        Vector3 newVelocityComponent = targetVelocity - _bodyRigidbody.velocity;
        //scale newVelocityComponent by mass/deltaTime to find the force necessary to acheive targetVelocity in the next FixedUpdate
        Vector3 force = (newVelocityComponent) * _bodyRigidbody.mass / Time.fixedDeltaTime;

        _bodyRigidbody.AddForce(force);
    }
}
