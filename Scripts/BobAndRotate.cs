using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BobAndRotate : MonoBehaviour
{
    [Tooltip("Frequency at which the item will move up and down")]
    public float verticalBobFrequency = 1f;
    [Tooltip("Distance the item will move up and down")]
    public float bobbingAmount = 1f;
    [Tooltip("Rotation angle per second")]
    public float rotatingSpeed = 360f;

    [SerializeField] private Transform _parentTransform = null;
    
    private void Update()
    {
        // Handle bobbing
        float bobbingAnimationPhase = ((Mathf.Sin(Time.time * verticalBobFrequency) * 0.5f) + 0.5f) * bobbingAmount;
        transform.position = _parentTransform.position + Vector3.up * bobbingAnimationPhase;

        // Handle rotating
        transform.Rotate(Vector3.up, rotatingSpeed * Time.deltaTime, Space.Self);
    }
}
