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
        Debug.Log("Mouse hover entered");
        sideBarWrapper.SelectBottomMenu(menuIndex);
    }
    
    // Called when the pointer exits the button area
    public void OnPointerExit(PointerEventData eventData)
    {
        //Debug.Log("Mouse hover exited");
        // Optional: Call another function when hover ends
    }
}