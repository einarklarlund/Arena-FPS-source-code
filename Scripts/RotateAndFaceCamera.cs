using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaceCamera : MonoBehaviour
{
    [Tooltip("Rotation angle per second")]
    public float rotatingSpeed = 90f;
    public Transform cameraTransform;

    private void OnEnable()
    {
        transform.LookAt(cameraTransform);
    }

    private void Update()
    {
        // Handle rotating
        transform.Rotate(transform.up, rotatingSpeed * Time.deltaTime, Space.Self);
    }
}
