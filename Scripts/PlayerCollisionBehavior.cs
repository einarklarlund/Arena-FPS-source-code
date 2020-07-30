using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCollisionBehavior : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        Debug.Log("trigger enter " + other.tag + " " + other.transform.position);
    }

    //enemies will only collide w enemies of the same type
    void OnCollisionEnter(Collision collision)
    {
        Debug.Log("player collision enter");
    }
}
