using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;

public class Level5Manager : LevelManager
{
    [Header("Level References")]
    private GameObject simulationGameobject;
    private IFluidSimulation sim;
    public FluidDetector fluidDetector;
    public ThermalSensor thermalSensor;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI planetStatusReportText;
    public TextMeshProUGUI SelectedWeaponText;

    public Button TwinIonCannonButton;
    public Image TwinIonCannonImage;
    public float TwinIonCannonMaxVelocity = 54f;
    public Button DeathRayButton;
    public Image DeathRayImage;
    public float DeathRayMaxVelocity = 64f;
    public Button TractorBeamButton;
    public Image TractorBeamImage;
    public Button NeutronBombButton;
    public Image NeutronBombImage;
    public float twinIonCannonAimSpeed = 10f; // Adjust this value as needed

    public float deathRayAimSpeed = 1f; // Adjust this value as needed

    [Header("Sound effects")]
    public AudioSource soundEffectPlayer;
    public AudioClip[] soundEffects;

    // Private Variables:
    private float[] weaponMaxVelocities;
    [SerializeField] private int selectedWeaponIndex = 0;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.visible = false;
        weaponMaxVelocities = new float[] { TwinIonCannonMaxVelocity, TwinIonCannonMaxVelocity, DeathRayMaxVelocity, 0f };

        if (fluidDetector == null) // Auto-find references if not assigned in inspector on start
        {
            fluidDetector = FindFirstObjectByType<FluidDetector>();
            if (fluidDetector == null)
            {
                Debug.LogError("No FluidDetector found in the scene!");
                enabled = false;
                return;
            }
        }

        if (thermalSensor == null) // Auto-find references if not assigned in inspector on start
        {
            thermalSensor = FindFirstObjectByType<ThermalSensor>();
            if (thermalSensor == null)
            {
                Debug.LogError("No ThermalSensor found in the scene!");
                enabled = false;
                return;
            }
        }
        // Reference the simulation script
        simulationGameobject = GameObject.FindGameObjectWithTag("Simulation");
        sim = simulationGameobject.GetComponent<IFluidSimulation>();

        // Set button click listeners
        TwinIonCannonButton.onClick.AddListener(TwinIonCannonButtonClick);
        DeathRayButton.onClick.AddListener(DeathRayButtonClick);
        TractorBeamButton.onClick.AddListener(TractorBeamButtonClick);
        NeutronBombButton.onClick.AddListener(NeutronBombButtonClick);
    }

    // Update is called once per frame
    // This script will check for the win conditions
    // this can be customized for each level
    void Update()
    {
        if (hasWon) return;
        timer += Time.deltaTime;

        // Update timer text
        timerText.text = $"TIME WASTED ON TASK: <size=16>{timer:F4}s</size>";

        // Update Planet Status Report
        planetStatusReportText.text = $"REMOVAL STATUS: <color=red>{Mathf.FloorToInt((1 - fluidDetector.currentDensity / 3000f) * 100f)}%</color>\n" +
                                      $"CLIMATE STATUS: <color=red>{(thermalSensor.currentTemperature > 550 ? "HOSTILE" : "HOSPITABLE")}</color>\n" +
                                      $"PLANET DENSITY: {fluidDetector.currentDensity:F0}g/cm³\n" +
                                      $"PLANET TEMPERATURE: {thermalSensor.currentTemperature:F0}C";

        // Aim the selected weapon towards to mouse position
        if (selectedWeaponIndex != -1){
            AimWeapons();
        }


        // Check for any mouse input
        if (Input.GetMouseButton(0) || Input.GetMouseButton(1))
        {
            lastMouseInputTime = Time.time;
            ResetHoldTimer();
            return;
        }

        // Only start counting after 0.5 seconds have past since last mouse input
        if (Time.time - lastMouseInputTime < 0.5f)
        {
            ResetHoldTimer();
            return;
        }

        // Check if fluid detector is above threshold (WIN CONDITION)
        if (!fluidDetector.isFluidPresent)
        {
            if (!isHolding) // This is used to show the holding timer at the top once fluid is detected
            {
                isHolding = true;
                holdTimer = 0f;
            }

            holdTimer += Time.deltaTime;

            // Update background music volume
            if (backgroundMusic != null)
            {
                float fadeStartThreshold = requiredHoldTime * fadeOutStartTime;
                if (holdTimer >= fadeStartThreshold)
                {
                    float fadeProgress = (holdTimer - fadeStartThreshold) / (requiredHoldTime - fadeStartThreshold);
                    fadeProgress = Mathf.Clamp01(fadeProgress);
                    backgroundMusic.volume = Mathf.Lerp(initialMusicVolume, 0f, fadeProgress);
                }
            }

            // Check if we've held for long enough
            if (holdTimer >= requiredHoldTime)
            {
                TriggerWin();
            }
        }
        else
        {
            ResetHoldTimer();
        }
    }

    public void TwinIonCannonButtonClick()
    {
        SelectedWeaponText.text = "SELECTED WEAPON: <color=red>TWIN ION-CANNONS";
        selectedWeaponIndex = 0;
        FlashImage(TwinIonCannonImage);

        //Disable all weapons
        DisableAllWeapons();

        // Activate selected weapon
        SourceObjectInitializer LeftIonCannon = sim.GetSourceObject(selectedWeaponIndex);
        LeftIonCannon.spawnRate = 1f;

        sim.SetSourceObject(LeftIonCannon, selectedWeaponIndex);

        SourceObjectInitializer RightIonCannon = sim.GetSourceObject(selectedWeaponIndex + 1);
        RightIonCannon.spawnRate = 1f;

        sim.SetSourceObject(RightIonCannon, selectedWeaponIndex + 1);
    }

    public void DeathRayButtonClick()
    {
        SelectedWeaponText.text = "SELECTED WEAPON: <color=red>DEATH RAY";
        selectedWeaponIndex = 2;
        FlashImage(DeathRayImage);

        //Disable all weapons
        DisableAllWeapons();

        // Activate selected weapon
        SourceObjectInitializer weapon = sim.GetSourceObject(selectedWeaponIndex);
        weapon.spawnRate = 1f;

        sim.SetSourceObject(weapon, selectedWeaponIndex);
    }

    public void TractorBeamButtonClick()
    {
        SelectedWeaponText.text = "SELECTED WEAPON: <color=red>TRACTOR BEAM";
        selectedWeaponIndex = -1;
        FlashImage(TractorBeamImage);

        //Disable all weapons
        DisableAllWeapons();

        sim.SetBrushType(1);
    }

    public void NeutronBombButtonClick()
    {
        SelectedWeaponText.text = "SELECTED WEAPON: <color=red>NEUTRON BOMB";
        selectedWeaponIndex = 3;
        FlashImage(NeutronBombImage);

        //Disable all weapons
        DisableAllWeapons();

        // Activate selected weapon
        SourceObjectInitializer weapon = sim.GetSourceObject(selectedWeaponIndex);
        weapon.spawnRate = 1f;

        sim.SetSourceObject(weapon, selectedWeaponIndex);
    }

    void DisableAllWeapons()
    {
        sim.SetBrushType(2);

        SourceObjectInitializer[] sourceObjects = new SourceObjectInitializer[4];
        for (int i = 0; i < 4; i++)
        {
            sourceObjects[i] = sim.GetSourceObject(i);
            sourceObjects[i].spawnRate = 0f;
            sim.SetSourceObject(sourceObjects[i], i);
        }
    }

    void AimWeapons(){
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = (mousePosition - (Vector2)sim.GetSourceObject(selectedWeaponIndex).transform.position).normalized;
        Vector2 sourceVelocity = direction * weaponMaxVelocities[selectedWeaponIndex];
        if (selectedWeaponIndex == 0) // Twin Ion-Cannons
        {
            SourceObjectInitializer LeftIonCannon = sim.GetSourceObject(selectedWeaponIndex);
            SourceObjectInitializer RightIonCannon = sim.GetSourceObject(selectedWeaponIndex + 1);

            LeftIonCannon.velo = Vector2.Lerp(LeftIonCannon.velo, sourceVelocity, twinIonCannonAimSpeed * Time.deltaTime);
            RightIonCannon.velo = Vector2.Lerp(RightIonCannon.velo, new Vector2(-sourceVelocity.x, sourceVelocity.y), twinIonCannonAimSpeed * Time.deltaTime);

            sim.SetSourceObject(LeftIonCannon, selectedWeaponIndex);
            sim.SetSourceObject(RightIonCannon, selectedWeaponIndex + 1);
        }
        else if (selectedWeaponIndex == 2) // Death Ray
        {
            SourceObjectInitializer source = sim.GetSourceObject(selectedWeaponIndex);
            source.velo = Vector2.Lerp(source.velo, sourceVelocity, deathRayAimSpeed * Time.deltaTime);
            sim.SetSourceObject(source, selectedWeaponIndex);
        }
    }

    void FlashImage(Image image)
    {
        // Change image color to white instantly
        image.color = Color.white;

        // Flash image white briefly and fade back to green
        image.DOColor(Color.green, 0.5f).SetEase(Ease.InOutQuad);
    }

    void OnDestroy()
    {
        Cursor.visible = true;
        // Kill all the animations on each image
        foreach (var image in new[] { TwinIonCannonImage, DeathRayImage, TractorBeamImage, NeutronBombImage })
        {
            DOTween.Kill(image);
        }
    }
}
