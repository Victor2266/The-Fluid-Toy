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
    public TextMeshProUGUI WeaponChargeText;
    public TextMeshProUGUI planetDetectedText;
    public Image planetDetectedOutline;
    public Image planetDetectedBG;

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
    public float twinIonCannonAimSpeed = 10f; // Adjust as needed
    public float deathRayAimSpeed = 1f;       // Adjust as needed

    [Header("Sound effects")]
    public AudioSource soundEffectPlayer;
    public AudioClip[] soundEffects;

    // These control how long a weapon stays active and how long its cooldown lasts.
    public float[] weaponActiveDuration = new float[] { 5f, 5f, 7.5f, 2f };
    public float[] weaponCooldownDuration = new float[] { 3f, 3f, 3f, 12f };

    // The weapon “state” (for our UI and simulation) is one of:
    private enum WeaponState { Ready, Active, Cooldown }

    // We “pack” each weapon’s state-and-timer info into this class.
    private class WeaponCooldown
    {
        public WeaponState state;
        public float timeRemaining;
    }

    // Order here is your UI order:
    // [0] Twin Ion-Cannons, [1] Death Ray, [2] Tractor Beam, [3] Neutron Bomb
    private WeaponCooldown[] weaponCooldowns = new WeaponCooldown[4];

    // This variable tracks which weapon (by our UI order) is currently “active” in the simulation.
    // (For Tractor Beam, the simulation uses brush types, so our code later distinguishes it.)
    [SerializeField] private int UIActiveWeapon = -1;

    void Start()
    {
        Cursor.visible = false;

        if (fluidDetector == null)
        {
            fluidDetector = FindFirstObjectByType<FluidDetector>();
            if (fluidDetector == null)
            {
                Debug.LogError("No FluidDetector found in the scene!");
                enabled = false;
                return;
            }
        }

        if (thermalSensor == null)
        {
            thermalSensor = FindFirstObjectByType<ThermalSensor>();
            if (thermalSensor == null)
            {
                Debug.LogError("No ThermalSensor found in the scene!");
                enabled = false;
                return;
            }
        }

        simulationGameobject = GameObject.FindGameObjectWithTag("Simulation");
        sim = simulationGameobject.GetComponent<IFluidSimulation>();

        // Setup button click listeners
        TwinIonCannonButton.onClick.AddListener(TwinIonCannonButtonClick);
        DeathRayButton.onClick.AddListener(DeathRayButtonClick);
        TractorBeamButton.onClick.AddListener(TractorBeamButtonClick);
        NeutronBombButton.onClick.AddListener(NeutronBombButtonClick);

        // **** Initialize each weapon’s state as Ready.
        for (int i = 0; i < weaponCooldowns.Length - 1; i++)
        {
            weaponCooldowns[i] = new WeaponCooldown();
            weaponCooldowns[i].state = WeaponState.Ready;
            weaponCooldowns[i].timeRemaining = 0f;
        }

        weaponCooldowns[3] = new WeaponCooldown();
        weaponCooldowns[3].state = WeaponState.Cooldown;
        weaponCooldowns[3].timeRemaining = weaponCooldownDuration[3];
    }

    void Update()
    {
        if (hasWon) return;
        timer += Time.deltaTime;

        // Update timer and status texts (existing code)
        timerText.text = $"TIME WASTED ON TASK: <size=16>{timer:F4}s</size>";
        planetStatusReportText.text = $"REMOVAL STATUS: <color=red>{Mathf.FloorToInt((1 - fluidDetector.currentDensity / 3000f) * 100f)}%</color>\n" +
                                      $"CLIMATE STATUS: <color=red>{(thermalSensor.currentTemperature > 550 ? "HOSTILE" : "HOSPITABLE")}</color>\n" +
                                      $"PLANET DENSITY: {fluidDetector.currentDensity:F0}g/cm³\n" +
                                      $"PLANET TEMPERATURE: {thermalSensor.currentTemperature:F0}C";

        // Aim the selected weapon (if applicable)
        if (UIActiveWeapon != 2)
        {
            AimWeapons();
        }

        // (Existing mouse input / hold timer code …)
        if (Input.GetMouseButton(0) || Input.GetMouseButton(1))
        {
            lastMouseInputTime = Time.time;
            ResetHoldTimer();
        }
        if (Time.time - lastMouseInputTime < 0.5f)
        {
            ResetHoldTimer();
        }
        if (!fluidDetector.isFluidPresent)
        {
            planetDetectedText.text = "<COLOR=GREEN>PLANET REMOVED\n<SIZE=16>ALL CLEAR!</COLOR>";
            planetDetectedBG.color = new Color32(0x00, 0x40, 0x00, 0x80);
            planetDetectedOutline.color = Color.green;
            if (!isHolding)
            {
                isHolding = true;
                holdTimer = 0f;
            }
            holdTimer += Time.deltaTime;
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
            if (holdTimer >= requiredHoldTime)
            {
                TriggerWin();
            }
        }
        else
        {
            planetDetectedText.text = "<COLOR=RED>PLANET DETECTED\n<SIZE=16>REMOVE IMMEDIATELY</COLOR>";
            planetDetectedBG.color = new Color32(0x65, 0x00, 0x00, 0x80);
            planetDetectedOutline.color = Color.red;
            ResetHoldTimer();
        }

        // === NEW: Update each weapon’s active/cooldown timer ===
        for (int i = 0; i < weaponCooldowns.Length; i++)
        {

            switch (weaponCooldowns[i].state)
            {
                case WeaponState.Active:
                    weaponCooldowns[i].timeRemaining -= Time.deltaTime;
                    if (weaponCooldowns[i].timeRemaining <= 0f)
                    {
                        // Time’s up for the active period.
                        weaponCooldowns[i].state = WeaponState.Cooldown;
                        weaponCooldowns[i].timeRemaining = weaponCooldownDuration[i];
                        // disable it.
                        DisableWeaponSimulation(i);

                    }
                    break;
                case WeaponState.Cooldown:
                    weaponCooldowns[i].timeRemaining -= Time.deltaTime;
                    if (weaponCooldowns[i].timeRemaining <= 0f)
                    {
                        weaponCooldowns[i].state = WeaponState.Ready;
                        weaponCooldowns[i].timeRemaining = 0f;
                    }
                    break;
                default:
                    break;

            }
        }

        // === NEW: Update the UI text for weapon charge ===
        WeaponChargeText.text =
            $"TWIN ION-CANNONS: {GetWeaponStatusText(0)}\n" +
            $"DEATH RAY: {GetWeaponStatusText(1)}\n" +
            $"TRACTOR BEAM: {GetWeaponStatusText(2)}\n" +
            $"NEUTRON BOMB: {GetWeaponStatusText(3)}";
    }

    // ===== Helper: returns a formatted status string for each weapon =====
    string GetWeaponStatusText(int index)
    {
        if (weaponCooldowns[index].state == WeaponState.Ready)
        {
            return "<color=green>READY</color>";
        }
        else if (weaponCooldowns[index].state == WeaponState.Active)
        {
            int percentage = Mathf.FloorToInt((weaponCooldowns[index].timeRemaining / weaponActiveDuration[index]) * 100f);
            return $"<color=yellow>{percentage}%</color>";
        }
        else if (weaponCooldowns[index].state == WeaponState.Cooldown)
        {
            // We show the percentage of the cooldown that has been completed.
            int percentage = Mathf.FloorToInt(((weaponCooldownDuration[index] - weaponCooldowns[index].timeRemaining) / weaponCooldownDuration[index]) * 100f);
            return $"<color=red>{percentage}%</color>";
        }
        return "";
    }

    // ===== Helper: disables a weapon in the simulation, based on our UI index =====
    void DisableWeaponSimulation(int uiWeaponIndex)
    {
        if (uiWeaponIndex == 0) // Twin Ion-Cannons (both sources 0 and 1)
        {
            SourceObjectInitializer LeftIonCannon = sim.GetSourceObject(0);
            LeftIonCannon.spawnRate = 0f;
            sim.SetSourceObject(LeftIonCannon, 0);
            SourceObjectInitializer RightIonCannon = sim.GetSourceObject(1);
            RightIonCannon.spawnRate = 0f;
            sim.SetSourceObject(RightIonCannon, 1);
        }
        else if (uiWeaponIndex == 1) // Death Ray (simulation source index 2)
        {
            SourceObjectInitializer weapon = sim.GetSourceObject(2);
            weapon.spawnRate = 0f;
            sim.SetSourceObject(weapon, 2);
        }
        else if (uiWeaponIndex == 2) // Tractor Beam (using brush types)
        {
            sim.SetBrushType(2);
        }
        else if (uiWeaponIndex == 3) // Neutron Bomb (simulation source index 3)
        {
            SourceObjectInitializer weapon = sim.GetSourceObject(3);
            weapon.spawnRate = 0f;
            sim.SetSourceObject(weapon, 3);
        }
        // Reset our active weapon tracking if needed.
        if (UIActiveWeapon == uiWeaponIndex)
        {
            UIActiveWeapon = -1;
        }
    }

    public void TwinIonCannonButtonClick()
    {
        // Only allow activation if ready.
        if (weaponCooldowns[0].state != WeaponState.Ready)
        {
            FlashImage(TwinIonCannonImage, Color.red);
            return;
        }
        FlashImage(TwinIonCannonImage, Color.white);

        UIActiveWeapon = 0;
        weaponCooldowns[0].state = WeaponState.Active;
        weaponCooldowns[0].timeRemaining = weaponActiveDuration[0];

        SelectedWeaponText.text = "SELECTED WEAPON: <color=red>TWIN ION-CANNONS";


        // Disable any previously active weapons.
        //DisableAllWeapons();

        // Activate twin ion cannons (simulation indices 0 and 1)
        SourceObjectInitializer LeftIonCannon = sim.GetSourceObject(0);
        LeftIonCannon.spawnRate = 1f;
        sim.SetSourceObject(LeftIonCannon, 0);

        SourceObjectInitializer RightIonCannon = sim.GetSourceObject(1);
        RightIonCannon.spawnRate = 1f;
        sim.SetSourceObject(RightIonCannon, 1);
    }

    public void DeathRayButtonClick()
    {
        if (weaponCooldowns[1].state != WeaponState.Ready)
        {
            FlashImage(DeathRayImage, Color.red);
            return;
        }

        FlashImage(DeathRayImage, Color.white);
        UIActiveWeapon = 1;
        weaponCooldowns[1].state = WeaponState.Active;
        weaponCooldowns[1].timeRemaining = weaponActiveDuration[1];

        SelectedWeaponText.text = "SELECTED WEAPON: <color=red>DEATH RAY";


        //DisableAllWeapons();

        // Activate death ray (simulation index 2)
        SourceObjectInitializer weapon = sim.GetSourceObject(2);
        weapon.spawnRate = 1f;
        sim.SetSourceObject(weapon, 2);
    }

    public void TractorBeamButtonClick()
    {
        if (weaponCooldowns[2].state != WeaponState.Ready)
        {
            FlashImage(TractorBeamImage, Color.red);
            return;
        }

        FlashImage(TractorBeamImage, Color.white);
        UIActiveWeapon = 2;
        weaponCooldowns[2].state = WeaponState.Active;
        weaponCooldowns[2].timeRemaining = weaponActiveDuration[2];

        SelectedWeaponText.text = "SELECTED WEAPON: <color=red>TRACTOR BEAM";


        //DisableAllWeapons();
        sim.SetBrushType(1);
    }

    public void NeutronBombButtonClick()
    {

        if (weaponCooldowns[3].state != WeaponState.Ready)
        {
            FlashImage(NeutronBombImage, Color.red);
            return;
        }

        FlashImage(NeutronBombImage, Color.white);
        UIActiveWeapon = 3;
        weaponCooldowns[3].state = WeaponState.Active;
        weaponCooldowns[3].timeRemaining = weaponActiveDuration[3];

        SelectedWeaponText.text = "SELECTED WEAPON: <color=red>NEUTRON BOMB";


        //DisableAllWeapons();

        SourceObjectInitializer weapon = sim.GetSourceObject(3);
        weapon.spawnRate = 1f;
        sim.SetSourceObject(weapon, 3);
    }

    void DisableAllWeapons()
    {
        sim.SetBrushType(2);

        SourceObjectInitializer[] sourceObjects = new SourceObjectInitializer[4];
        for (int i = 0; i < 4; i++)
        {
            sourceObjects[i] = sim.GetSourceObject(i); sourceObjects[i].spawnRate = 0f;
            sim.SetSourceObject(sourceObjects[i], i);
        }
    }

    void AimWeapons()
    {
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        // Aim twin ion cannons
        Vector2 direction = (mousePosition - (Vector2)sim.GetSourceObject(0).transform.position).normalized;
        Vector2 sourceVelocity = direction * TwinIonCannonMaxVelocity;

        SourceObjectInitializer LeftIonCannon = sim.GetSourceObject(0);
        SourceObjectInitializer RightIonCannon = sim.GetSourceObject(1);

        LeftIonCannon.velo = Vector2.Lerp(LeftIonCannon.velo, sourceVelocity, twinIonCannonAimSpeed * Time.deltaTime);
        RightIonCannon.velo = Vector2.Lerp(RightIonCannon.velo, new Vector2(-sourceVelocity.x, sourceVelocity.y), twinIonCannonAimSpeed * Time.deltaTime);

        sim.SetSourceObject(LeftIonCannon, 0);
        sim.SetSourceObject(RightIonCannon, 1);

        // Aim death ray
        direction = (mousePosition - (Vector2)sim.GetSourceObject(2).transform.position).normalized;
        sourceVelocity = direction * DeathRayMaxVelocity;

        SourceObjectInitializer source = sim.GetSourceObject(2);
        source.velo = Vector2.Lerp(source.velo, sourceVelocity, deathRayAimSpeed * Time.deltaTime);
        sim.SetSourceObject(source, 2);

    }

    void FlashImage(Image image, Color color)
    {
        // Flash the image: change to white then tween back to green.
        image.color = color;
        DOTween.Kill(image);
        image.DOColor(Color.green, 0.5f).SetEase(Ease.InOutQuad);
    }

    void OnDestroy()
    {
        Cursor.visible = true;
        // Kill DOTween animations on all weapon images.
        foreach (var image in new[] { TwinIonCannonImage, DeathRayImage, TractorBeamImage, NeutronBombImage })
        {
            DOTween.Kill(image);
        }
    }
}
