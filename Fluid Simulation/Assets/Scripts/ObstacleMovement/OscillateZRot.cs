using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OscillateZRot : MonoBehaviour
{
    public float startRot;
    public float Speed;
    public float HeightAmp =1;
    public bool isChild;

    // Update is called once per frame
    void Update()
    {
        if (!isChild)
            this.transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, startRot + Mathf.Cos(Time.time * Speed) * HeightAmp);
        else
            this.transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, transform.localEulerAngles.y, startRot + Mathf.Cos(Time.time * Speed) * HeightAmp);
    }
}
