using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZRotation : MonoBehaviour
{

    //Vector3 pos = Vector3.zero;
    public float speed;
    // Use this for initialization
    void Start()
    {
        //transform.eulerAngles = pos;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, transform.eulerAngles.z + speed);
        //pos.z += speed;
        //transform.eulerAngles = pos;
    }
}

