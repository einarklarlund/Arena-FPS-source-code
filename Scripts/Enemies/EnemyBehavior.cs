using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class EnemyBehavior : MonoBehaviour
{
    //events
    public UnityEvent OnSpawnComplete;
    public UnityEvent OnDeathComplete;

    [Header("Generic time variables")]
    [SerializeField] protected float _spawnDuration;
    [SerializeField] protected float _deathDuration;
    [SerializeField] protected float _attackInterval;

    [Header("Audio resources")]
    [SerializeField] protected AudioSource _mainAudioSource = null;
    [SerializeField] protected AudioClip _spawnClip = null;
    [SerializeField] protected AudioClip _deathClip = null;
    [SerializeField] protected Animation _enemyAnimator = null;

    [Header("Enemy variables")]
    //variables that are set by the initializer of the enemy
    public Vector3 initialPosition;
    public Vector3 initialDirection;
    public string enemyName = "";

    //behavior methods to be called by MyEnemyAI script
    public abstract void SpawnBehavior(float timeSpentInCurrentState);
    public abstract void AttackBehavior(float timeSpentInCurrentState);
    public abstract void DeathBehavior(float timeSpentInCurrentState);    
}
