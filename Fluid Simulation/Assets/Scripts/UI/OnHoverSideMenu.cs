using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class OnHoverSideMenu : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    // Reference to your button (optional)
    public int menuIndex;
    public SideBarWrapper sideBarWrapper;
    
    // Called when the pointer enters the button area
    public void OnPointerEnter(PointerEventData eventData)
    {
        // Debug.Log("Mouse hover entered");
        if (sideBarWrapper.IsBottomMenuVisible()) 
        {   // Only play the sound effect and activate the menu on hover if it is not being hidden
            sideBarWrapper.SelectBottomMenu(menuIndex);
            sideBarWrapper.playHoverSound();
        }
    }
    
    // Called when the pointer exits the button area
    public void OnPointerExit(PointerEventData eventData)
    {
        //Debug.Log("Mouse hover exited");
        // Optional: Call another function when hover ends
    }
}