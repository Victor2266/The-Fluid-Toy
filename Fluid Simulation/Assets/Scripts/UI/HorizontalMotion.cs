using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

public class HorizontalMotion : MonoBehaviour
{
    public float velocity = 0.1F;

    public float acceleration = 0.0005F;

    public float offset = 0;

    public float maxOffset = 10;

    public float maxRandomVelocityStart = 0;
    // Start is called before the first frame update
    void Start()
    {
        velocity += UnityEngine.Random.Range(-maxRandomVelocityStart, maxRandomVelocityStart);
    }

    // Update is called once per frame
    void Update()
    {

        velocity += -1 * Math.Sign(offset) * acceleration;

        offset += velocity;

        transform.Translate(new Vector3(velocity, 0, 0));
    }
}
