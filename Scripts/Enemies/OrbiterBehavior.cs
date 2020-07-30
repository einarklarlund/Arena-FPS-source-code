using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbiterBehavior : EnemyBehavior
{
    [Header("Components")]
    [SerializeField] private GameObject _hitbox = null;
    [SerializeField] private GameObject _hurtbox = null;
    [SerializeField] private Transform _eyeballTransform = null;
    [SerializeField] private Rigidbody _rigidbody = null;
    [SerializeField] private Health _health = null;
    [SerializeField] private EyeballColorController _eyeballColorController = null;

    [Header("Values")]
    [Tooltip("Change in angle (radians) of thrust vector between FixedUpdate calls")]
    [SerializeField] private float _deltaTheta = 0.0225f;
    [SerializeField] private float _maxVelocity = 10f; 
    [SerializeField] private float _chaseRadius = 10f; 
    [SerializeField] private float _idleRadius = 25f; 
    
    private bool movesClockwise;
    private float _deltaThetaLook;
    private Transform _playerTransform = null;
    private float _maxForce; 

    void Start()
    {
        //spawn duration will last 2 secondss
        _spawnDuration = 2 * Time.fixedDeltaTime;
        //death duration will last 10 frames
        _deathDuration = 10f * Time.fixedDeltaTime;

        _maxForce = _maxVelocity / 3 * _rigidbody.mass / Time.fixedDeltaTime;

        _deltaThetaLook = _deltaTheta * 2f;

        _playerTransform = GameObject.FindWithTag("Player").transform.Find("Main Camera");

        movesClockwise = UnityEngine.Random.value < 0.5 ? true : false;

        _enemyAnimator.Stop();
        _enemyAnimator.Play("OrbiterSpawn");
        
        gameObject.transform.Translate(initialPosition);
        
        _health.onDamaged += HandleOnDamaged;

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
            //spawnforce is calculated by the tower that instantiates the Orbiter
            float multiplier = _maxVelocity * _rigidbody.mass / Time.fixedDeltaTime;
            Vector3 spawnForce = initialDirection;
            spawnForce.Scale(new Vector3(multiplier, multiplier, multiplier));
            _rigidbody.AddForce(spawnForce);
        }
        else
        {
            CircleAroundStage();
        }
    }

    public override void AttackBehavior(float timeSpentInCurrentState)
    {
        MoveOrbiter();
    }

    public override void DeathBehavior(float timeSpentInCurrentState)
    {
        if(timeSpentInCurrentState < Time.fixedDeltaTime)
        {
            _enemyAnimator.Stop();
            _enemyAnimator.Play("OrbiterDeath");

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

    void HandleOnDamaged(float trueDamage, GameObject damageSource)
    {
        if(_health.currentHealth != 0)
        {
            // _enemyAnimator.Stop();
            // _enemyAnimator.Play("OrbiterDamaged");
            _eyeballColorController.TakeDamage();
        }
    }

    private void MoveOrbiter()
    {
        RaycastHit[] hits = Physics.SphereCastAll(transform.position, _chaseRadius, Vector3.up, 0f, 1 << LayerMask.NameToLayer("Default"), QueryTriggerInteraction.Collide);
        if(hits.Length > 0)
        {
            LookTowardsPlayer();
            MoveTowardsPlayer();
        }
        else
        {
            CircleAroundStage();
        }
    }

    private void CircleAroundStage()
    {
        //find the vector pointing from (0, 0, 0) to a point on the circle, while intersecting the position of the rigidbody 
        Vector3 towardsCircle = Vector3.RotateTowards(new Vector3(0f, _idleRadius, 0f), _rigidbody.position, (float) 2 * Mathf.PI, 0f);
        towardsCircle = Vector3.ProjectOnPlane(towardsCircle, Vector3.up).normalized * _idleRadius + new Vector3(0f, 2f, 0f);

        //find vector from rigidbody to the point on the circle
        towardsCircle -= _rigidbody.position;
        
        //find tangent to circle and then rotate it towards  the circle
        Vector3 tangentialToCircle = movesClockwise ? Vector3.Cross(towardsCircle, Vector3.up) : Vector3.Cross(towardsCircle, Vector3.down);
        towardsCircle = Vector3.RotateTowards(tangentialToCircle, towardsCircle, (float) Mathf.PI / 6, 0f);
        // Debug.DrawLine(_rigidbody.position, _rigidbody.position + towardsCircle, Color.red, 1f);

        //newDirection will be the direction of the velocity that we want to add to the Orbiter
        Vector3 newDirection = Vector3.RotateTowards(_rigidbody.velocity.normalized, towardsCircle, _deltaTheta, 0f);
        // Debug.DrawLine(_rigidbody.position, _rigidbody.position + newDirection, Color.yellow, 1f);

        //scale unit vector of newDirection by max velocity to get target velocity
        Vector3 targetVelocity = newDirection.normalized * _maxVelocity;
        
        //look forwards
        _eyeballTransform.rotation = Quaternion.LookRotation(targetVelocity);

        ChangeVelocity(targetVelocity);
    }

    private void MoveTowardsPlayer()
    {
        Vector3 towardsPlayer = _playerTransform.position - _rigidbody.position;

        // newDirection will be the direction of the velocity that we want to add to the Orbiter
        Vector3 newDirection = Vector3.RotateTowards(_rigidbody.velocity.normalized, towardsPlayer, _deltaTheta, _maxVelocity);

        //scale unit vector of newDirection by max velocity to get target velocity
        Vector3 targetVelocity = newDirection.normalized * _maxVelocity;

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
