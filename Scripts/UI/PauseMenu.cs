using UnityEngine;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private Button _resumeButton = null;
    [SerializeField] private Button _restartButton = null;
    [SerializeField] private Button _settingsButton = null;
    [SerializeField] private Button _quitButton = null;

    private void Start()
    {
        _resumeButton.onClick.AddListener(HandleResumeClicked);
        _restartButton.onClick.AddListener(HandleRestartClicked);
        _settingsButton.onClick.AddListener(HandleSettingsClicked);
        _quitButton.onClick.AddListener(HandleQuitClicked);
    }
    
    public void HandleResumeClicked()
    {
        GameManager.Instance.TogglePause();
    }
    
    public void HandleRestartClicked()
    {
        GameManager.Instance.RestartGame();
    }

    public void HandleSettingsClicked()
    {
        UIManager.Instance.ToggleSettingsMenu(true);
    }
    
    public void HandleQuitClicked()
    {
        GameManager.Instance.QuitGame();
    }
}
