using System.Runtime.CompilerServices;
using UnityEngine;
using DG.Tweening;
public class CastLevelBasinMovement : MonoBehaviour
{
    [Header("Level reference")]

    public CastingLevelManager manager;
    [Header("Settings")]
    public float distanceToMove = -10F;
    public float movementTime = 3F;

    private Tweener currentTween;

    private bool winSet = false;
	void FixedUpdate()
	{
		if (!winSet && manager.finishedCooling)
        {
            winSet = true;
            moveToLastPos();
        }
	}


    void moveToLastPos()
    {
        currentTween?.Kill();
        Vector3 destPos = transform.position + new Vector3(0, distanceToMove, 0);

        currentTween = transform.DOMove(destPos, movementTime)
            .SetEase(Ease.OutQuad) // Optional: choose an easing function
            .OnComplete(() => {
                // Optional: You can add any completion logic here
                currentTween = null;
                manager.setWin();
            });
    }
}