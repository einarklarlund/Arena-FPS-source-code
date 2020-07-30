using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameOverMenu : MonoBehaviour
{
    [SerializeField] private Text _gameOverText = null;
    // [SerializeField] private Button _restartButton = null;
    [SerializeField] private Button _mainMenuButton = null;
    [SerializeField] private Text _timeText = null;
    [SerializeField] private Text _enemiesKilledText = null;
    
    public Events.EventFadeComplete OnGameOverMenuFadeComplete;

    [SerializeField] private Animation _gameOverMenuAnimator = null;
    
    private void Start()
    {
        _gameOverText.text = "You died.";
        // _restartButton.onClick.AddListener(HandleRestartClicked);
        _mainMenuButton.onClick.AddListener(HandleMainMenuClicked);
    }

    // public void HandleRestartClicked()
    // {
    //     FadeOut();
    //     GameManager.Instance.RestartGame();
    // }

    public void HandleMainMenuClicked()
    {
        GameManager.Instance.QuitGame();
    }

    public void OnFadeOutComplete()
    {
        OnGameOverMenuFadeComplete.Invoke(true);
    }

    public void OnFadeInComplete()
    {
        UIManager.Instance.setDummyCameraActive(true);
        OnGameOverMenuFadeComplete.Invoke(false);
    }

    public void FadeIn()
    {
        _timeText.text = "" + GameFlowManager.Instance.playTime + " seconds";
        _enemiesKilledText.text = "" + EnemyManager.Instance.enemiesKilled + " enemies";
        _gameOverMenuAnimator.Stop();
        _gameOverMenuAnimator.Play("GameOverFadeIn");
    }

    public void FadeOut()
    {
        UIManager.Instance.setDummyCameraActive(false);
        _gameOverMenuAnimator.Stop();
        _gameOverMenuAnimator.Play("GameOverFadeOut");
    }
}
