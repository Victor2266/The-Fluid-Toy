using UnityEngine;
using DG.Tweening;
using System.Data;

public class CastLevelLock : MonoBehaviour
{
    [Header("Level References")]
    public ThermalSensor sensor;
    public BoundaryRestriction res;
    public CastingLevelManager manager;
    public float thresholdToLock = 1000F;
    private bool isLocked = false;
    private bool isFall = false;

    private BoundaryRestriction savedRes;

    void FixedUpdate()
    {
        if(sensor.currentTemperature > thresholdToLock && !isLocked){
            savedRes = res;
            res.setStartingPosition(transform.position);
            res.minX = 0;
            res.maxX = 0;
            res.minY = 0;
            res.maxY = 0;
            isLocked = true;
        }else if(!isFall && manager.fall){
            res.maxY = savedRes.maxY;
            isFall = true;
        }
    }
}