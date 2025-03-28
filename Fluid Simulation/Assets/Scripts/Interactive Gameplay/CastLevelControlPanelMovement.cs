using System.Runtime.CompilerServices;
using UnityEngine;
using DG.Tweening;
public class CastLevelControlPanelMovement : MonoBehaviour
{
    public ThermalSensor sensor;

    public float moveDistance;
    public float movementTime;

    public AudioSource sound;
    private Tweener currentTween;
    private bool moved = false;
    void FixedUpdate()
    {
        if(sensor != null)
        {
            if(sensor.metThreshold && !moved)
            {
                moved = true;
                currentTween?.Kill();
                Vector3 openPosition = transform.position;
                openPosition.x += moveDistance;

                // Move to open position smoothly
                currentTween = transform.DOMove(openPosition, movementTime)
                    .SetEase(Ease.OutQuad) // Optional: choose an easing function
                    .OnComplete(() => {
                        // Optional: You can add any completion logic here
                        
                    });
                if(sound != null){
                    sound.Play();
                }
            }
        }
    }
}