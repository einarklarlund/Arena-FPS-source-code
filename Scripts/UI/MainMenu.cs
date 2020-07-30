using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public Text welcomeText;
    public Events.EventFadeComplete OnMainMenuFadeComplete;

    [SerializeField] private Animation _mainMenuAnimator = null;
    
    private void Start()
    {
        welcomeText.text = "Press Space to start.";
    }

    public void OnFadeOutComplete()
    {
        OnMainMenuFadeComplete.Invoke(true);
    }

    public void OnFadeInComplete()
    {
        UIManager.Instance.setDummyCameraActive(true);
        OnMainMenuFadeComplete.Invoke(false);
    }

    public void FadeIn()
    {
        _mainMenuAnimator.Stop();
        _mainMenuAnimator.Play("FadeIn");
    }

    public void FadeOut()
    {
        UIManager.Instance.setDummyCameraActive(false);
        _mainMenuAnimator.Stop();
        _mainMenuAnimator.Play("FadeOut");
    }
}
