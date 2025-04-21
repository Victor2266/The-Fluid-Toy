using System.Runtime.CompilerServices;
using UnityEngine;
using DG.Tweening;
public class CastLevelBasinLidMovement : MonoBehaviour
{
    public CastingLevelManager manager;
    public float delay = 2F;
    public float moveAmount = 25F;
    private float moveTime;
    private bool falling = false;

    void FixedUpdate()
    {
        if(manager.fall && !falling){
            falling = true;
            moveTime = Time.time + delay;
        }

        if(falling && moveTime <= Time.time)
        {
            transform.position = new Vector3(transform.position.x + moveAmount, 0, 0);
        }
    }
}