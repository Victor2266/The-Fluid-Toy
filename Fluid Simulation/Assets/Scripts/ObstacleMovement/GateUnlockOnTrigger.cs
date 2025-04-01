using UnityEngine;

public class GateUnlockOnTrigger : MonoBehaviour
{
    public FluidDetector fluidDetector;
    private bool isOpen;
    public float openAmount = 2.5F;
    public bool openDirection = true;
    public float openingSpeed = 0.05F;
    private Vector3 openedPosition;
    private Vector3 originalPosition;
    void Start()
    {
        if (fluidDetector == null){
            Debug.LogError("Error: No fluid detector connected");
            return;
        }
        originalPosition = transform.position;
        openedPosition = transform.position;

        if(openDirection){
            openedPosition.x += openAmount * Mathf.Cos(Mathf.Deg2Rad * transform.rotation.eulerAngles.z);
            openedPosition.y += openAmount * Mathf.Sin(Mathf.Deg2Rad * transform.rotation.eulerAngles.z);
        }else{
            openedPosition.x -= openAmount * Mathf.Cos(Mathf.Deg2Rad * transform.rotation.eulerAngles.z);
            openedPosition.y -= openAmount * Mathf.Sin(Mathf.Deg2Rad * transform.rotation.eulerAngles.z);
        }

    }

    void FixedUpdate()
    {
        if(fluidDetector.isFluidPresent && transform.position != openedPosition){
            transform.position = Vector3.Lerp(transform.position, openedPosition, openingSpeed);
        }else if(!fluidDetector.isFluidPresent && transform.position != originalPosition){
            transform.position = Vector3.Lerp(transform.position, originalPosition, openingSpeed);
        }
    }
}