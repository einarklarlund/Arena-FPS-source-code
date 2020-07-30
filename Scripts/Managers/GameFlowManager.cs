using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameFlowManager : Singleton<GameFlowManager>
{
    public bool gameIsEnding { get; private set; }
    public double playTime => _gameEndTime != 0 ? Math.Truncate(100 * (_gameEndTime - _gameStartTime)) / 100 : 0;
    public float timeSpentInRound => Time.time - _roundStartTime;
    public int stage => 1 + (currentRound - 1) / _roundsPerStage;
    public int difficultyMode = 1;
    public float difficultyCoefficient => (1f + 0.15f * Time.time * difficultyMode);
    public float difficulty;
    public int currentRound;

    private PlayerCharacterController m_Player = null;
    private bool _isPlaying = false;
    private float _gameEndTime;
    private float _gameStartTime;

    //directors
    // private Director _towerDirector = null;
    // private Director _golemDirector = null;
    private Director _smallEnemyDirector = null;
    private Director _bigEnemyDirector = null;
    private Director _chestDirector = null;
    private float _creditMultiplier;
    
    //round duration variables (in seconds)
    [SerializeField] private float _timeBetweenRounds = 4f;
    [SerializeField] private float _timeBetweenStages = 12f;
    private float _roundDuration;
    private float _waitDuration;

    //round variables
    [SerializeField] private int _roundsPerStage = 3;
    private float _roundStartTime;
    private float _roundEndTime;
    private bool _roundOngoing;

    void Start()
    {
        AudioUtility.SetMasterVolume(1);
        _isPlaying = false;

        // GameManager.Instance.OnGameStart.AddListener(HandleGameStart);
        // UnityEventTools.AddPersistentListener(GameManager.Instance.OnGameStart, HandleGameStart);
    }

    void FixedUpdate()
    {
        if(!_isPlaying || !_smallEnemyDirector || !_bigEnemyDirector)
            return;

        //end the round if it is ongoing and the round duration has been passed and no enemies are left
        if(_roundOngoing && timeSpentInRound >= _roundDuration
            && EnemyManager.Instance.numberOfEnemiesRemaining == 0)
        {
            EndRound();
        }



        //halfway through the round, disable credit generation for the enemy directors
        _smallEnemyDirector.canGenerateCredits = (_roundOngoing && timeSpentInRound <= _roundDuration / 2f);
        _bigEnemyDirector.canGenerateCredits = (_roundOngoing && timeSpentInRound <= _roundDuration / 2f);

        //start a new round if round isn't ongoing and timebetweenrounds has been passed 
        if(!_roundOngoing && Time.time - _roundEndTime >= _timeBetweenRounds )
        {
            StartRound();
        }
        else if(!_roundOngoing && Time.time - _roundEndTime >= _timeBetweenRounds && EnemyManager.Instance.numberOfEnemiesRemaining != 0)
        {
            Debug.Log("waiting for numberOfEnemiesRemaining to reach 0. current value is " + EnemyManager.Instance.numberOfEnemiesRemaining);
        }
        
    }

    void StartRound()
    {
        _roundStartTime = Time.time;
        _roundOngoing = true;
        currentRound++;

        //calculate difficulty
        difficulty += currentRound % 2 == 1 ? 1 : 0;

        Debug.Log("New Round " + currentRound + " difficulty " + difficulty);

        //generate income and set round duration
        _roundDuration = 3f + 8f * (0.5f * difficulty);
        // _roundDuration *= 1.15f;

        _smallEnemyDirector.canGenerateCredits = true;
        _bigEnemyDirector.canGenerateCredits = true;
        
        switch(difficulty)
        {
            case 1: //4 credits
                _smallEnemyDirector.credits += 4;
                break;
            case 2: //7 credits
                _smallEnemyDirector.credits += 2;
                _bigEnemyDirector.credits += 5;
                break;
            case 3: //10 credits
                _smallEnemyDirector.credits += 5;
                _bigEnemyDirector.credits += 5;
                break;
            case 4: //8 credits
                _smallEnemyDirector.credits += 2;
                _bigEnemyDirector.credits += 6;
                break;
            case 5: //10 credits
                _smallEnemyDirector.credits += 4;
                _bigEnemyDirector.credits += 6;
                break;
            case 6: //10 credits
                _smallEnemyDirector.credits += 2;
                _bigEnemyDirector.credits += 8;
                break;
            case 7: //14 credits
                _smallEnemyDirector.credits += 2;
                _bigEnemyDirector.credits += 12;
                break;
            case 8: //14 credits
                _smallEnemyDirector.credits += 6;
                _bigEnemyDirector.credits += 8;
                break;
            case 9: //18 credits
                _smallEnemyDirector.credits += 6;
                _bigEnemyDirector.credits += 12;
                break;
            default:
                float credits = difficulty * 4f;
                float ratio = UnityEngine.Random.value;
                _smallEnemyDirector.credits += ratio * credits;
                _bigEnemyDirector.credits += (1 - ratio) * credits;
                break;
        }
    }

    void EndRound()
    {
        Debug.Log("round end after " + timeSpentInRound  + " seconds");
        _roundEndTime = Time.time;
        _roundOngoing = false;

        if(_smallEnemyDirector)
        {
            _smallEnemyDirector.EmptyCredits();
            _smallEnemyDirector.canGenerateCredits = false;
        }

        if(_bigEnemyDirector)
        {
            _bigEnemyDirector.EmptyCredits();
            _bigEnemyDirector.canGenerateCredits = false;
        }

        if(currentRound % _roundsPerStage == 0)
        {
            _waitDuration = _timeBetweenStages;
        }
        else 
        {
            _waitDuration = _timeBetweenRounds;
        }
    }

    public void HandleGameStart(float time)
    {
        _isPlaying = true; 
        gameIsEnding = false;
        currentRound = 0;
        _gameStartTime = Time.time;
        _gameEndTime = 0;
        _roundOngoing = true;
        //set roundendtime to curr time so that first round is started after timebetweenrounds seconds
        _roundEndTime = Time.time;
        difficulty = 0;

        if(!m_Player)
        {
            m_Player = FindObjectOfType<PlayerCharacterController>();
            DebugUtility.HandleErrorIfNullFindObject<PlayerCharacterController, GameFlowManager>(m_Player, this);
        }
        // if(!_towerDirector)
        // {
        //     _towerDirector = GameObject.Find("Tower Director").GetComponent<Director>();
        //     if(!_towerDirector)
        //         Debug.LogWarning("[GameFlowManager] Could not find tower director");
        // }
        // if(!_golemDirector)
        // {
        //     _golemDirector = GameObject.Find("Golem Director").GetComponent<Director>();
        //     if(!_golemDirector)
        //         Debug.LogWarning("[GameFlowManager] Could not find golem director");
        // }
        if(!_smallEnemyDirector)
        {
            _smallEnemyDirector = GameObject.Find("Small Enemy Director").GetComponent<Director>();
            if(!_smallEnemyDirector)
                Debug.LogWarning("[GameFlowManager] Could not find Small Enemy director");     
            _smallEnemyDirector.canGenerateCredits = true;
        }
        if(!_bigEnemyDirector)
        {
            _bigEnemyDirector = GameObject.Find("Big Enemy Director").GetComponent<Director>();
            if(!_bigEnemyDirector)
                Debug.LogWarning("[GameFlowManager] Could not find Big Enemy director");          
            _bigEnemyDirector.canGenerateCredits = true;
        }
        if(!_chestDirector)
        {
            _chestDirector = GameObject.Find("Chest Director").GetComponent<Director>();
            if(!_chestDirector)
                Debug.LogWarning("[GameFlowManager] Could not find chest director");
        }
    }

    public void GameOver()
    {
        gameIsEnding = true;
        _roundEndTime = Time.time;
        _gameEndTime = Time.time;
        _isPlaying = false;
        _roundOngoing = false;

        GameManager.Instance.GameOver();
        // play a sound on game over
        // var audioSource = gameObject.AddComponent<AudioSource>();
        // audioSource.clip = victorySound;
        // audioSource.playOnAwake = false;
        // audioSource.outputAudioMixerGroup = AudioUtility.GetAudioGroup(AudioUtility.AudioGroups.HUDVictory);
        // audioSource.PlayScheduled(AudioSettings.dspTime + delayBeforeWinMessage);
    }
}
