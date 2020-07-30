using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeCanvas : MonoBehaviour
{
    [SerializeField] private Animation _fadeCanvasAnimator = null;

    public void FadeIn()
    {
        _fadeCanvasAnimator.Stop();
        _fadeCanvasAnimator.Play("GameOverFadeIn");
    }

    public void FadeOut()
    {
        _fadeCanvasAnimator.Stop();
        _fadeCanvasAnimator.Play("GameOverFadeOut");
    }
}
