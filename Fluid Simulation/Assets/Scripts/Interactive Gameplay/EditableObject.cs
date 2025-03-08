using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class EditableObject : MonoBehaviour
{
    enum ChildObjectType { None, BoxCollider, SolidThermalBox, ThermalBox, CircleCollider, SourceObject, DrainObject };
    // Prefab references
    public GameObject contextMenuPrefab;

    [SerializeField] private string contextMenuHeading = "Edit Menu";
    [SerializeField] private ChildObjectType childObjectType = ChildObjectType.None;

    // Internal references
    protected GameObject activeContextMenu;
    protected GameObject contentParent;
    protected RectTransform menuRectTransform;

    protected Draggable draggableScript;
    protected Canvas canvas;

    protected GameObject simulationGameobject;
    protected IFluidSimulation fluidSimulationScript;

    protected virtual void Start()
    {
        // Find or create canvas for UI elements
        canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObject = new GameObject("UICanvas");
            canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObject.AddComponent<CanvasScaler>();
            canvasObject.AddComponent<GraphicRaycaster>();
        }
        draggableScript = GetComponent<Draggable>();
        simulationGameobject = GameObject.FindGameObjectWithTag("Simulation");
        fluidSimulationScript = simulationGameobject.GetComponent<IFluidSimulation>();
    }

    protected virtual void Update()
    {
        // Close menu if open and player clicks outside
        if (activeContextMenu != null && Input.GetMouseButtonDown(0))
        {
            Vector2 mousePosition = Input.mousePosition;
            if (!RectTransformUtility.RectangleContainsScreenPoint(menuRectTransform, mousePosition))
            {
                CloseContextMenu();
            }
        }
    }

    void OnMouseOver()
    {
        // Check for right click
        if (Input.GetMouseButtonDown(1))
        {
            OpenContextMenu();
        }
    }

    protected virtual void OpenContextMenu()
    {
        // Close any existing menu
        CloseContextMenu();

        // Instantiate the menu prefab
        activeContextMenu = Instantiate(contextMenuPrefab, canvas.transform);
        menuRectTransform = activeContextMenu.GetComponent<RectTransform>();

        // Position the menu at mouse position
        Vector2 mousePosition = Input.mousePosition;
        menuRectTransform.position = mousePosition;

        // Ensure menu stays on screen
        EnsureMenuOnScreen();

        // Setup the menu controls
        SetupMenuControls();
    }

    protected virtual void EnsureMenuOnScreen()
    {
        // Get screen boundaries
        Vector2 screenSize = new Vector2(Screen.width, Screen.height);
        Vector2 menuSize = menuRectTransform.sizeDelta * canvas.scaleFactor;

        // Calculate position
        Vector3 menuPosition = menuRectTransform.position;

        // Check right edge
        if (menuPosition.x + menuSize.x / 2 > screenSize.x)
        {
            menuPosition.x = screenSize.x - menuSize.x / 2;
        }

        // Check left edge
        if (menuPosition.x - menuSize.x / 2 < 0)
        {
            menuPosition.x = menuSize.x / 2;
        }

        // Check top edge
        if (menuPosition.y + menuSize.y / 2 > screenSize.y)
        {
            menuPosition.y = screenSize.y - menuSize.y / 2;
        }

        // Check bottom edge
        if (menuPosition.y - menuSize.y / 2 < 0)
        {
            menuPosition.y = menuSize.y / 2;
        }

        menuRectTransform.position = menuPosition;
    }

    protected virtual void SetupMenuControls()
    {
        // Find controls in the menu
        Transform heading = activeContextMenu.transform.Find("Heading Text");
        Transform closeButton = activeContextMenu.transform.Find("CloseButton");
        Transform scrollView = activeContextMenu.transform.Find("Scroll View");
        Transform content = scrollView.Find("Viewport").Find("Content");
        Transform deleteButton = content.transform.Find("DeleteButton");
        Transform duplicateButton = content.transform.Find("DuplicateButton");

        Transform xScaleInput = content.transform.Find("XScaleInput");
        Transform yScaleInput = content.transform.Find("YScaleInput");
        Transform zRotationInput = content.transform.Find("ZRotationInput");

        // Set Heading Text
        if (heading != null)
        {
            TextMeshProUGUI headingText = heading.GetComponent<TextMeshProUGUI>();
            headingText.text = "    " + contextMenuHeading;
        }

        // Add event listeners
        closeButton.GetComponent<Button>().onClick.AddListener(CloseContextMenu);
        deleteButton.GetComponent<Button>().onClick.AddListener(DeleteObject);
        duplicateButton.GetComponent<Button>().onClick.AddListener(DuplicateObject);

        // Initialize input fields with current values
        if (xScaleInput != null)
        {
            TMP_InputField xScaleField = xScaleInput.GetComponentInChildren<TMP_InputField>();
            xScaleField.text = transform.localScale.x.ToString("F2");

            // Add event listeners for input changes
            xScaleField.onEndEdit.AddListener((value) =>
            {
                if (float.TryParse(value, out float newXScale))
                {
                    Vector3 newScale = transform.localScale;
                    newScale.x = newXScale;
                    draggableScript.setTargetScale(newScale);
                }
            });
        }

        if (yScaleInput != null)
        {
            TMP_InputField yScaleField = yScaleInput.GetComponentInChildren<TMP_InputField>();
            yScaleField.text = transform.localScale.y.ToString("F2");

            // Add event listeners for input changes
            yScaleField.onEndEdit.AddListener((value) =>
            {
                if (float.TryParse(value, out float newYScale))
                {
                    Vector3 newScale = transform.localScale;
                    newScale.y = newYScale;
                    draggableScript.setTargetScale(newScale);
                }
            });
        }

        if (zRotationInput != null)
        {
            TMP_InputField zRotationField = zRotationInput.GetComponentInChildren<TMP_InputField>();
            zRotationField.text = transform.rotation.eulerAngles.z.ToString("F2");

            // Add event listeners for input changes
            zRotationField.onEndEdit.AddListener((value) =>
            {
                if (float.TryParse(value, out float newZRotation))
                {
                    Vector3 rotation = transform.rotation.eulerAngles;
                    rotation.z = newZRotation;
                    transform.rotation = Quaternion.Euler(rotation);
                }
            });
        }
    }

    protected virtual void CloseContextMenu()
    {
        if (activeContextMenu != null)
        {
            Destroy(activeContextMenu);
            activeContextMenu = null;
        }
    }

    protected virtual void DeleteObject()
    {
        CloseContextMenu();
        Destroy(gameObject);
    }

    protected virtual void DuplicateObject()
    {
        GameObject duplicate = Instantiate(gameObject, transform.position + new Vector3(0.5f, 0.5f, 0), transform.rotation);
        CloseContextMenu();
    }
    void OnDestroy()
    {
        if (fluidSimulationScript == null) return;

        if (childObjectType == ChildObjectType.None)
        {
            string tagName = gameObject.tag;

            if (tagName == "BoxCollider")
            {
                fluidSimulationScript.UpdateBoxColliders();
            }
            else if (tagName == "SolidThermalBox")
            {
                fluidSimulationScript.UpdateBoxColliders();
                fluidSimulationScript.UpdateThermalBoxes();
            }
            else if (tagName == "ThermalBox")
            {
                fluidSimulationScript.UpdateThermalBoxes();
            }
            else if (tagName == "CircleCollider")
            {
                fluidSimulationScript.UpdateCircleColliders();
            }
            else if (tagName == "SourceObject")
            {
                fluidSimulationScript.UpdateSourceObjects();
            }
            else if (tagName == "DrainObject")
            {
                fluidSimulationScript.UpdateDrainObjects();
            }
        }
        else
        {
            if (childObjectType == ChildObjectType.BoxCollider)
            {
                fluidSimulationScript.UpdateBoxColliders();
            }
            else if (childObjectType == ChildObjectType.SolidThermalBox)
            {
                fluidSimulationScript.UpdateBoxColliders();
                fluidSimulationScript.UpdateThermalBoxes();
            }
            else if (childObjectType == ChildObjectType.ThermalBox)
            {
                fluidSimulationScript.UpdateThermalBoxes();
            }
            else if (childObjectType == ChildObjectType.CircleCollider)
            {
                fluidSimulationScript.UpdateCircleColliders();
            }
            else if (childObjectType == ChildObjectType.SourceObject)
            {
                fluidSimulationScript.UpdateSourceObjects();
            }
            else if (childObjectType == ChildObjectType.DrainObject)
            {
                fluidSimulationScript.UpdateDrainObjects();
            }
        }

    }

    // Add any additional common functionality here
}