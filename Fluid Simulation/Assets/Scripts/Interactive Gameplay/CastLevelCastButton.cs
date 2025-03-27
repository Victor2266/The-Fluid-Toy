using System.Runtime.CompilerServices;
using UnityEngine;

public class CastLevelCastButton : MonoBehaviour
{

    [Header("Button status flags and settings")]
    // public float hoverScale = 0.8F;
    public float clickedScale = 0.6F;
    public bool isOpening = false;
    public bool isOpened = false;
    public bool isClosing = false;

    private Vector3 startingScale;

    void Start()
    {
        startingScale = transform.localScale;
    }

    void OnMouseDown()
    {
        transform.localScale = new Vector3(startingScale.x * clickedScale, startingScale.y * clickedScale, 0);
        if(!isOpened){
            if(!isOpening && !isClosing){
                isOpening = true;
            }
        }else{
            if(!isOpening && !isClosing){
                isClosing = true;
            }
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