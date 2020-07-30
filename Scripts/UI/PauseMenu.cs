using UnityEngine;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private Button _resumeButton = null;
    [SerializeField] private Button _restartButton = null;
    [SerializeField] private Button _quitButton = null;

    private void Start()
    {
        _resumeButton.onClick.AddListener(HandleResumeClicked);
        _restartButton.onClick.AddListener(HandleRestartClicked);
        _quitButton.onClick.AddListener(HandleQuitClicked);
    }
    
    //this listener is selected for the resume button from within the inspector
    public void HandleResumeClicked()
    {
        GameManager.Instance.TogglePause();
    }
    
    //this listener is selected for the restart button from within the inspector
    public void HandleRestartClicked()
    {
        GameManager.Instance.RestartGame();
    }
    
    //this listener is selected for the quit button from within the inspector
    public void HandleQuitClicked()
    {
        GameManager.Instance.QuitGame();
    }
}
