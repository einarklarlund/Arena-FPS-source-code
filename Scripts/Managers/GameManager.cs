using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager>
{
    //needs to know: current level, how to load/unload level, 
    //keep track of game state, generate other persistent systems

    public enum GameState
    {
        MAINMENU,
        RUNNING,
        PAUSED,
        GAMEOVER
    }

    private List<string> _loadedLevelNames;
    private List<GameObject> _instancedSystemPrefabs;
    private List<AsyncOperation> _loadOperations;
    private List<AsyncOperation> _unloadOperations;
    private GameState _currentGameState;
    private bool _restarting = false;
    private bool _loading = false;

    public GameState CurrentGameState
    {
        get { return _currentGameState; }
        private set { _currentGameState = value; }
    }
    public GameObject[] SystemPrefabs;
    public Events.EventGameState OnGameStateChanged;
    public Events.EventGameStart OnGameStart;

    private void Start()
    {
        DontDestroyOnLoad(gameObject);

        _loadedLevelNames = new List<string>();
        _instancedSystemPrefabs = new List<GameObject>();
        _loadOperations = new List<AsyncOperation>();
        _unloadOperations = new List<AsyncOperation>();
        
        OnGameStateChanged = new Events.EventGameState();
        OnGameStart = new Events.EventGameStart();

        InstantiateSystemPrefabs();

        UIManager.Instance.OnMainMenuFadeComplete.AddListener(HandleMainMenuFadeComplete);
        UIManager.Instance.OnGameOverMenuFadeComplete.AddListener(HandleGameOverMenuFadeComplete);

        Application.targetFrameRate = 60;

        _currentGameState = GameState.MAINMENU;
    }

    private void Update()
    {
        if(GameManager.Instance.CurrentGameState != GameManager.GameState.GAMEOVER
            && GameManager.Instance.CurrentGameState != GameManager.GameState.MAINMENU
            && Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        for(int i = 0; i < _instancedSystemPrefabs.Count; ++i)
        {
            Destroy(_instancedSystemPrefabs[i]);
        }
        _instancedSystemPrefabs.Clear();
    }

    //public method to start the game, for use in UIManager to start game from main menu
    public void StartGame()
    {
        //prevent loading  levels while async load operation is running
        if(_loading)
            return;

        _restarting = false;
        Debug.Log("Starting game");
        LoadLevel("Main");
        //UpdateState is called after main menu fades out completely in HandleMainMenuFadeComplete or in restart
    }

    //public method to restart the game, for use in GameOverMenu to restart game from main menu
    public void RestartGame()
    {
        _restarting = true;
        UnloadAllLevels();
        Debug.Log("Restarting game");
        // LoadLevel("Main");
        //UpdateState is called after game over menu fades out completely in HandleGameOverFadeComplete or in restart
    }

    public void QuitGame()
    {
        UnloadAllLevels();
        //implement features for quitting
        Debug.Log("Quitting to main menu");
        UpdateState(GameState.MAINMENU);
        // Application.Quit();
    }

    public void GameOver()
    {
        Debug.Log("Game over");
        UIManager.Instance.FadeInGameOverMenu();  
    }

    // instantiate the system prefabs and make them accessible to GameManager
    void InstantiateSystemPrefabs()
    {
        GameObject prefabInstance;
        for(int i = 0; i < SystemPrefabs.Length; ++i)
        {
            prefabInstance = Instantiate(SystemPrefabs[i]);
            _instancedSystemPrefabs.Add(prefabInstance);
        }
    }

    //update the game's state
    void UpdateState(GameState state)
    {  
        GameState previousGameState = _currentGameState;
        _currentGameState = state;

        switch (_currentGameState)
        {            
            case GameState.MAINMENU:
                Time.timeScale = 1.0f;
                break;
            case GameState.RUNNING:
                Time.timeScale = 1.0f;
                break;
            case GameState.PAUSED:
                Time.timeScale = 0.0f;
                break;            
            case GameState.GAMEOVER:
                Time.timeScale = 1.0f;
                break;
            default:
                break;
        }

        if(OnGameStateChanged != null)
        {
            OnGameStateChanged.Invoke(_currentGameState, previousGameState);
        }
    }

    // listener function that is called on the event ao.completed (where ao is a load operation)
    void OnLoadOperationComplete(AsyncOperation ao)
    {
        if(_loadOperations.Contains(ao))
        {
            _loadOperations.Remove(ao);

            if(_loadOperations.Count == 0)
            {
                SceneManager.SetActiveScene((SceneManager.GetSceneByName("Main")));
                
                if(OnGameStart != null)
                {
                    // Debug.Log("invoking ongamestart");
                    OnGameStart.Invoke(Time.time);
                }

                UpdateState(GameState.RUNNING);
                EnemyManager.Instance.HandleGameStart(Time.time);
                GameFlowManager.Instance.HandleGameStart(Time.time);
                _loading = false;
            }
        }

        Debug.Log("Load complete.");
    }

    // listener function that is called on the event ao.completed (where ao is an unload operation)
    void OnUnloadOperationComplete(AsyncOperation ao)
    {        
        if(_unloadOperations.Contains(ao))
        {
            _unloadOperations.Remove(ao);
            
            if(_loadOperations.Count == 0 && _restarting)
            {
                StartGame();
            }
        }

        Debug.Log("Unload complete.");
    }

    // listener function that is called after main scene is loaded 
    // void OnMainSceneLoadComplete(AsyncOperation ao)
    // {        
    //     SceneManager.SetActiveScene(SceneManager.GetSceneByName("Main"));
    // }


    // public method to load a level asynchronously using Unity's SceneManager
    public void LoadLevel(string levelName)
    {
        // load the scene asynchronously using ao object
        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(levelName, LoadSceneMode.Additive);
        if(loadOperation == null)
        {
            Debug.LogError("[GameManager] Unable to load level " + levelName);
            return;
        }

        // we must add our listener function (OnLoadOperationComplete) to the ao object so that it is called when the ao completes
        loadOperation.completed += OnLoadOperationComplete;
        //main scene must be set as active scene after it is loaded so that projectiles & effects load in main scene
        // if(levelName == "Main")
        // {
        //     loadOperation.completed += OnMainSceneLoadComplete;
        // }
        
        // change class vars
        _loadOperations.Add(loadOperation);
        _loadedLevelNames.Add(levelName);
        _loading = true;
    }

    // public method to unload a level asynchronously using Unity's SceneManager
    public void UnloadLevel(string levelName)
    {
        // unload the scene asynchronously using ao object
        AsyncOperation unloadOperation = SceneManager.UnloadSceneAsync(levelName);
        if(unloadOperation == null)
        {
            Debug.LogError("[GameManager] Unable to unload level " + levelName);
            return;
        }

        // we must add our listener function (OnLoadOperationComplete) to the ao object so that it is called when the ao completes
        unloadOperation.completed += OnUnloadOperationComplete;
        //change class vars
        _unloadOperations.Add(unloadOperation);
        _loadedLevelNames.Remove(levelName);
    }

    public void UnloadAllLevels()
    {
        for(int i = 0; i < _loadedLevelNames.Count; ++i)
        {
            UnloadLevel(_loadedLevelNames[i]);
        }
    }

    // listens to UIManager.OnMainMenuFadeComplete
    public void HandleMainMenuFadeComplete(bool isFadeOut)
    {
        if(!isFadeOut)
        {
            //after the main menu fades in, make sure that all levels are unloaded            
            UnloadAllLevels();
        }
    }

    // listens to UIManager.OnGameOverMenuFadeComplete
    public void HandleGameOverMenuFadeComplete(bool isFadeOut)
    {
        if(isFadeOut)
        {
            UpdateState(GameState.RUNNING);
        }
        else
        {
            //after the game over screen fades in, make sure that all levels must be unloaded            
            UnloadAllLevels();
            UpdateState(GameState.GAMEOVER);
        }
    }

    //change game state to paused/running
    public void TogglePause()
    {
        UpdateState(_currentGameState == GameState.RUNNING ? GameState.PAUSED : GameState.RUNNING);
    }
}
