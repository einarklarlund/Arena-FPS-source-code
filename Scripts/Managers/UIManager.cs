using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : Singleton<UIManager>
{
    public Events.EventFadeComplete OnMainMenuFadeComplete;
    public Events.EventFadeComplete OnGameOverMenuFadeComplete;

    [SerializeField] private MainMenu mainMenu = null;
    [SerializeField] private GameOverMenu gameOverMenu = null;
    [SerializeField] private Camera dummyCamera = null;
    [SerializeField] private PauseMenu pauseMenu = null;
    public void Start()
    {
        GameManager.Instance.OnGameStateChanged.AddListener(HandleGameStateChanged);
        mainMenu.OnMainMenuFadeComplete.AddListener(HandleMainMenuFadeComplete);
        gameOverMenu.OnGameOverMenuFadeComplete.AddListener(HandleGameOverMenuFadeComplete);
    }

    private void Update()
    {
        //start game and fade out menu if currently in MAINMENU state and space is pressed
        if(GameManager.Instance.CurrentGameState == GameManager.GameState.MAINMENU 
            && Input.GetKeyDown(KeyCode.Space))
        {
            GameManager.Instance.StartGame();
            mainMenu.FadeOut(); 
        }
    }

    public void HandleGameStateChanged(GameManager.GameState currentState, GameManager.GameState previousState)
    {
        //turn on pause menu if in paused state
        pauseMenu.gameObject.SetActive(currentState == GameManager.GameState.PAUSED);
        //turn on game over menu if in game over state
        gameOverMenu.gameObject.SetActive(currentState == GameManager.GameState.GAMEOVER);

        //fade the main menu in if game is entering mainmenu
        if(currentState == GameManager.GameState.MAINMENU)
        {
            dummyCamera.gameObject.SetActive(true);  
            mainMenu.FadeIn();
        }
    }

    public void FadeInGameOverMenu()
    {
        gameOverMenu.gameObject.SetActive(true);
        gameOverMenu.FadeIn();
    }

    public void HandleMainMenuFadeComplete(bool isFadeOut)
    {
        OnMainMenuFadeComplete.Invoke(isFadeOut);
    }

    public void HandleGameOverMenuFadeComplete(bool isFadeOut)
    {
        OnGameOverMenuFadeComplete.Invoke(isFadeOut);
    }

    public void setDummyCameraActive(bool active)
    {
        dummyCamera.gameObject.SetActive(active);  
    }
}
