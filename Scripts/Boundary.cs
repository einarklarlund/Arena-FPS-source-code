using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boundary : MonoBehaviour
{
    void OnCollisionEnter(Collision collision)
    {
        Debug.Log("boundary collision!");
    }
}
