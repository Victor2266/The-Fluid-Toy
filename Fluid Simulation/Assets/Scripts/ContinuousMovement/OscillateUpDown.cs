using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OscillateUpDown : MonoBehaviour
{
    public Vector3 startPos;
    public float hoverSpeed;
    public float hoverHeightAmp;
    public bool isChild;

    // Update is called once per frame
    void Update()
    {
        if(!isChild)
            this.transform.position = new Vector3(transform.position.x, startPos.y + Mathf.Cos(Time.time * hoverSpeed) * hoverHeightAmp, transform.position.z);
        else
            this.transform.localPosition = new Vector3(transform.localPosition.x, startPos.y + Mathf.Cos(Time.time * hoverSpeed) * hoverHeightAmp, transform.localPosition.z);
    }
}
