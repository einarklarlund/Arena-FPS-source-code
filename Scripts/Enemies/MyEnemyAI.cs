using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyEnemyAI : MonoBehaviour
{
    public enum EnemyState
    {
        SPAWNING,
        ATTACKING,
        DEAD
    }

    private EnemyState _currentState;
    private float _timeSpentInCurrentState;

    [SerializeField] private Health _health = null;
    [SerializeField] private EnemyBehavior _behavior = null;
    
    // Start is called before the first frame update
    void Start()
    {
        _timeSpentInCurrentState = 0;
        _currentState = EnemyState.SPAWNING;

        _behavior.OnSpawnComplete.AddListener(HandleSpawnComplete);
        _behavior.OnDeathComplete.AddListener(HandleDeathComplete);

        _health.onDie += HandleDeath;

        // EnemyManager.Instance.RegisterEnemy(this, gameObject.name);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if(GameManager.Instance.CurrentGameState == GameManager.GameState.RUNNING)
        {
            switch(_currentState)
            {
                case EnemyState.SPAWNING:
                    _behavior.SpawnBehavior(_timeSpentInCurrentState);
                    break;
                case EnemyState.ATTACKING:
                    _behavior.AttackBehavior(_timeSpentInCurrentState);
                    break;
                case EnemyState.DEAD:
                    _behavior.DeathBehavior(_timeSpentInCurrentState);
                    break;
                default:
                    break;
            }
            _timeSpentInCurrentState += Time.fixedDeltaTime;
        }
    }

    void UpdateState(EnemyState state)
    {
        EnemyState previousState = _currentState;
        _currentState = state;
    }

    void HandleSpawnComplete()
    {   
        _timeSpentInCurrentState = 0;
        UpdateState(EnemyState.ATTACKING);
    }

    void HandleDeath()
    {
        EnemyManager.Instance.UnregisterEnemy(_behavior.enemyName);
        _timeSpentInCurrentState = 0;
        UpdateState(EnemyState.DEAD);
    }

    void HandleDeathComplete()
    {   
        _timeSpentInCurrentState = 0;
        Destroy(gameObject);
    }
}
