using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OscillateLeftRight : MonoBehaviour
{
    public Vector3 startPos;
    public float hoverSpeed;
    public float hoverWidthAmp;
    public bool isChild;

    // Update is called once per frame
    void Update()
    {
        if(!isChild)
            this.transform.position = new Vector3(startPos.x + Mathf.Sin(Time.time * hoverSpeed) * hoverWidthAmp, transform.position.y, transform.position.z);
        else
            this.transform.localPosition = new Vector3(startPos.x + Mathf.Sin(Time.time * hoverSpeed) * hoverWidthAmp, transform.localPosition.y, transform.localPosition.z);
    }
}

