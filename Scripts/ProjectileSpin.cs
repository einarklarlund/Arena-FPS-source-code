using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileSpin : MonoBehaviour
{
    [Tooltip("number of degrees per fixedupdate that the projectile rotates")]
    [SerializeField] float _degreesPerFrame = 5f;

    void Start()
    {
        transform.Rotate(0f, 0f, UnityEngine.Random.value * 360f);
    }

    void FixedUpdate()
    {
        transform.Rotate(0f, 0f, _degreesPerFrame);
    }
}
