using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Main : MonoBehaviour
{
    public delegate void ActionClick(Vector3 pos);
    public static event ActionClick onSpace;

    public void task()
    {
        Debug.Log("hai");
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown("space"))
        {
            OnSpace();
        }
    }

    public void OnSpace()
    {
        if(onSpace != null)
        {
            onSpace(new Vector3(5, 2, 0));
        }
    }


}
