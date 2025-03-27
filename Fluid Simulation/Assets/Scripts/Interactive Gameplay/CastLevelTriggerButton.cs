using System.Runtime.CompilerServices;
using UnityEngine;
using DG.Tweening;

public class CastLevelTriggerButton : MonoBehaviour
{
    [Header("Level Reference")]
    public BoundaryRestriction release;
    public CastLevelCastButton castButton;

    [Header("Button status flags and settings")]
    // public float hoverScale = 0.8F;
    public float clickedScale = 0.6F;
    public bool fall = false;
    private Vector3 startingScale;

    void Start()
    {
        startingScale = transform.localScale;
    }
    
    void OnMouseDown()
    {
        transform.localScale = new Vector3(startingScale.x * clickedScale, startingScale.y * clickedScale, 0);
        if(!fall && castButton.isOpened){
            fall = true;
            release.maxY += 10F;
        }
        
    }

    void OnMouseUp()
    {
        transform.localScale = startingScale;
    }
	// void OnMouseExit()
	// {
	// 	transform.localScale = startingScale;
	// }

    // void OnMouseEnter()
	// {
	// 	transform.localScale = new Vector3(startingScale.x * hoverScale, startingScale.y * hoverScale, 0);
	// }
}