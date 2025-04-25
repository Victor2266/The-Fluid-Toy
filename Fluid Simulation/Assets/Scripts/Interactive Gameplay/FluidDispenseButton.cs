using UnityEngine;

public class FluidDispenseButton : MonoBehaviour
{
    [Header("Dispensing Settings")]
    public int sourceIndex;
    public float fixedDispenseQuantity = 0.5f;
    public float dispenseDuration = 0.5f;
    public Vector2 fixedVelocity = new Vector2(0, 1f);
    public bool keepDispensingWhilePressed = false;

    [Header("Button Visuals")]
    public SpriteRenderer buttonRenderer;
    public Sprite unpressedSprite;
    public Sprite pressedSprite;
    [Tooltip("Should button stay visually pressed while dispensing?")]
    public bool stayPressedWhileDispensing = true;

    [Header("Sound Effects")]
    public AudioSource audioSource;
    public AudioClip pressSound;
    public AudioClip releaseSound;

    private SourceObjectInitializer source;
    private float dispenseEndTime;
    private bool isDispensing;
    private bool isPressed = false;
    private IFluidSimulation sim;

    void Start()
    {
        if (sim == null)
        {
            GameObject simObject = GameObject.FindGameObjectWithTag("Simulation");
            sim = simObject.GetComponent<IFluidSimulation>();
        }

        // Set initial sprite
        if (buttonRenderer != null && unpressedSprite != null)
        {
            buttonRenderer.sprite = unpressedSprite;
        }
    }

    void Update()
    {
        // Stop dispensing after duration elapses
        if (keepDispensingWhilePressed)
        {
            if (isPressed == false)
            {
                StopDispensing();
            }
        }
        else if (isDispensing && Time.time > dispenseEndTime)
        {
            StopDispensing();
        }
    }

    void OnMouseDown()
    {
        if (!isPressed)
        {
            PressButton();
        }
    }

    void OnMouseUp()
    {
        if (!stayPressedWhileDispensing && isPressed)
        {
            ReleaseButton();
        }
    }

    void PressButton()
    {
        isPressed = true;

        // Change to pressed sprite
        if (buttonRenderer != null && pressedSprite != null)
        {
            buttonRenderer.sprite = pressedSprite;
        }

        // Play press sound
        if (audioSource != null && pressSound != null)
        {
            audioSource.PlayOneShot(pressSound);
        }

        StartDispensing();
    }

    void ReleaseButton()
    {
        isPressed = false;

        // Change back to unpressed sprite
        if (buttonRenderer != null && unpressedSprite != null)
        {
            buttonRenderer.sprite = unpressedSprite;
        }

        // Play release sound
        if (audioSource != null && releaseSound != null)
        {
            audioSource.PlayOneShot(releaseSound);
        }
    }

    void StartDispensing()
    {
        // Get the fluid source
        source = sim.GetSourceObject(sourceIndex);

        // Configure for fixed quantity dispensing
        source.velo = fixedVelocity;
        source.spawnRate = fixedDispenseQuantity / dispenseDuration;

        // Apply settings to simulation
        sim.SetSourceObject(source, sourceIndex);

        // Set dispensing state
        isDispensing = true;
        dispenseEndTime = Time.time + dispenseDuration;
    }

    void StopDispensing()
    {
        if (!isDispensing) return;

        // Get the fluid source
        source = sim.GetSourceObject(sourceIndex);

        // Stop the flow
        source.spawnRate = 0f;
        sim.SetSourceObject(source, sourceIndex);

        // Reset state
        isDispensing = false;

        // Release button if configured to stay pressed
        if (stayPressedWhileDispensing)
        {
            ReleaseButton();
        }
    }

    // FIXME Visual feedback when hovering
    void OnMouseEnter()
    {
        // Optional: Add hover effect like sprite tint change
    }

    void OnMouseExit()
    {
        // Optional: Remove hover effect
    }
}