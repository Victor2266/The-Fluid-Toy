using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections;

public class DBZLevelManager : LevelManager
{
    [Header("DBZ Level References")]
    private GameObject simulationGameobject;
    private IFluidSimulation sim;
    
    [Header("Characters")]
    public Transform playerCharacter;
    public Transform antagonistCharacter;
    public Image playerPowerAura;
    public Image antagonistPowerAura;
    
    [Header("Beam Sources")]
    public int playerBeamSourceIndex = 0;
    public int antagonistBeamSourceIndex = 1;
    public float playerBeamMaxVelocity = 54f;
    public float antagonistBeamMaxVelocity = 54f;
    public Color playerBeamColor = Color.blue;
    public Color antagonistBeamColor = Color.red;
    
    [Header("UI Elements")]
    public TextMeshProUGUI syllableText;
    public Slider beamPowerSlider;
    public Image beamPowerFill;
    public TextMeshProUGUI powerLevelText;
    public TextMeshProUGUI instructionText;
    
    [Header("Sound Effects")]
    public AudioClip[] kamehamehaClips; // 5 clips for ka-me-ha-me-ha
    public AudioClip beamStartSound;
    public AudioClip beamOngoingSound;
    public AudioClip beamClashSound;
    public AudioClip playerWinSound;
    public AudioClip playerLoseSound;
    public AudioClip[] powerUpSounds;
    
    [Header("Camera Effects")]
    public Transform mainCameraTransform;
    public Vector3 playerCameraPosition;
    public Vector3 antagonistCameraPosition;
    public Vector3 clashCameraPosition;
    public float cameraZoomSpeed = 2f;
    public float cameraPanSpeed = 3f;
    
    [Header("Battle Parameters")]
    public float maxBeamPower = 100f;
    public float powerDecayRate = 5f; // How fast power decreases when not pressing space
    public float powerIncreasePerPress = 2f; // How much power increases per space press
    public float winThreshold = 90f; // When beam power reaches this %, player wins
    public float loseThreshold = 10f; // When beam power drops below this %, player loses
    public float powerBarSpeed = 2f; // How fast the power bar moves
    public float spacePressResetTime = 0.3f; // How long before space press count starts dropping
    
    // State variables
    private enum BattleState { Intro, Charging, BeamClash, Victory, Defeat }
    private BattleState currentState = BattleState.Intro;
    private int syllableCount = 0;
    private float currentBeamPower = 30f; // Starting power (30%)
    private float targetBeamPower = 30f;
    private int spaceBarPressCount = 0;
    private float lastSpacePressTime;
    private float spaceBarPressCooldown = 0.05f; // Minimum time between space presses
    private float lastPowerDecayTime;
    private bool beamSoundPlaying = false;
    
    new void Start()
    {
        base.Start();
        
        // Find simulation object
        simulationGameobject = GameObject.FindGameObjectWithTag("Simulation");
        sim = simulationGameobject.GetComponent<IFluidSimulation>();
        
        // Setup initial UI state
        if (syllableText != null)
            syllableText.alpha = 0f;
            
        if (beamPowerSlider != null)
        {
            beamPowerSlider.minValue = 0f;
            beamPowerSlider.maxValue = 100f;
            beamPowerSlider.value = currentBeamPower;
        }
        
        // Hide beam power UI until clash begins
        if (beamPowerSlider != null)
            beamPowerSlider.gameObject.SetActive(false);
            
        // Display initial instruction
        if (instructionText != null)
            instructionText.text = "Press SPACE to begin Kamehameha";
            
        // Set up camera
        if (mainCameraTransform == null)
            mainCameraTransform = Camera.main.transform;
            
        // Initialize sources but turn them off
        InitializeBeamSources(false);
        
        // Set initial player auras
        if (playerPowerAura != null)
            playerPowerAura.color = new Color(playerBeamColor.r, playerBeamColor.g, playerBeamColor.b, 0);
            
        if (antagonistPowerAura != null)
            antagonistPowerAura.color = new Color(antagonistBeamColor.r, antagonistBeamColor.g, antagonistBeamColor.b, 0.5f);
    }
    
    void Update()
    {
        if (hasWon) return;
        timer += Time.deltaTime;
        
        switch (currentState)
        {
            case BattleState.Intro:
                HandleIntroState();
                break;
                
            case BattleState.Charging:
                HandleChargingState();
                break;
                
            case BattleState.BeamClash:
                HandleBeamClashState();
                break;
                
            case BattleState.Victory:
            case BattleState.Defeat:
                // Wait for win sequence to finish
                break;
        }
        
        // Update beam power slider
        if (beamPowerSlider != null && beamPowerSlider.gameObject.activeSelf)
        {
            beamPowerSlider.value = Mathf.Lerp(beamPowerSlider.value, currentBeamPower, Time.deltaTime * powerBarSpeed);
            
            // Update colors based on who's winning
            if (currentBeamPower > 50f)
                beamPowerFill.color = Color.Lerp(Color.white, playerBeamColor, (currentBeamPower - 50f) / 50f);
            else
                beamPowerFill.color = Color.Lerp(antagonistBeamColor, Color.white, currentBeamPower / 50f);
        }
        
        // Update beam sources
        if (currentState == BattleState.BeamClash)
        {
            UpdateBeamSources();
        }
    }
    
    private void HandleIntroState()
    {
        // Process space bar presses for kamehameha syllables
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (syllableCount < 5)
            {
                syllableCount++;
                PlayNextSyllable();
                if (syllableCount >= 5)
                {
                    StartCoroutine(StartBeamClash());
                }
            }
        }
    }
    
    private void HandleChargingState()
    {
        // Show charging animation
        // Automatically transitions to BeamClash after a short delay via coroutine
    }
    
    private void HandleBeamClashState()
    {
        // Process space bar spam
        if (Input.GetKeyDown(KeyCode.Space) && Time.time - lastSpacePressTime > spaceBarPressCooldown)
        {
            lastSpacePressTime = Time.time;
            spaceBarPressCount++;
            
            // Increase beam power
            targetBeamPower = Mathf.Min(targetBeamPower + powerIncreasePerPress, maxBeamPower);
            
            // Play power up sound
            if (powerUpSounds.Length > 0 && audioSource != null)
            {
                int soundIndex = Mathf.Min(spaceBarPressCount / 10, powerUpSounds.Length - 1);
                audioSource.PlayOneShot(powerUpSounds[soundIndex], 0.5f);
            }
        }
        
        // Decay power if no input for a while
        if (Time.time - lastSpacePressTime > spacePressResetTime)
        {
            targetBeamPower = Mathf.Max(targetBeamPower - powerDecayRate * Time.deltaTime, 0f);
            spaceBarPressCount = Mathf.Max(0, spaceBarPressCount - 1);
        }
        
        // Smooth power changes
        currentBeamPower = Mathf.Lerp(currentBeamPower, targetBeamPower, Time.deltaTime * 5f);
        
        // Update power level text
        if (powerLevelText != null)
        {
            int displayPower = Mathf.RoundToInt(currentBeamPower);
            powerLevelText.text = $"POWER: {displayPower}%";
            
            // Change color based on power level
            if (displayPower > 75)
                powerLevelText.color = Color.green;
            else if (displayPower > 40)
                powerLevelText.color = Color.yellow;
            else
                powerLevelText.color = Color.red;
        }
        
        // Check for win/lose conditions
        if (currentBeamPower >= winThreshold)
        {
            // Player wins!
            currentState = BattleState.Victory;
            StartCoroutine(HandleVictory());
        }
        else if (currentBeamPower <= loseThreshold)
        {
            // Player loses!
            currentState = BattleState.Defeat;
            StartCoroutine(HandleDefeat());
        }

        // Adjust screen shake based on beam power
        float shakeStrength = Mathf.Lerp(0.2f, 1.0f, currentBeamPower / maxBeamPower);
        if (Random.value < 0.05f) // Occasional random shake
        {
            ShakeCamera(0.2f, shakeStrength);
        }
        
        // Update character auras
        if (playerPowerAura != null)
        {
            float playerAuraIntensity = Mathf.Lerp(0.2f, 0.8f, currentBeamPower / maxBeamPower);
            playerPowerAura.color = new Color(playerBeamColor.r, playerBeamColor.g, playerBeamColor.b, playerAuraIntensity);
        }
        
        if (antagonistPowerAura != null)
        {
            float antagonistAuraIntensity = Mathf.Lerp(0.8f, 0.2f, currentBeamPower / maxBeamPower);
            antagonistPowerAura.color = new Color(antagonistBeamColor.r, antagonistBeamColor.g, antagonistBeamColor.b, antagonistAuraIntensity);
        }
    }
    
    private void PlayNextSyllable()
    {
        string[] syllables = new string[] { "KA", "ME", "HA", "ME", "HA!!!" };
        
        // Show the current syllable
        if (syllableText != null)
        {
            // Clear Current Animation on previous syllable
            syllableText.transform.DOKill();
            syllableText.DOKill();

            syllableText.text = syllables[syllableCount - 1];
            syllableText.alpha = 1f;
            
            // Fade and scale animation
            syllableText.transform.localScale = Vector3.one * 0.5f;
            syllableText.transform.DOScale(Vector3.one * 1.5f, 0.5f).SetEase(Ease.OutBack);
            syllableText.DOFade(0f, 0.5f).SetDelay(0.5f);
        }
        
        // Play syllable sound
        if (kamehamehaClips.Length >= 5 && audioSource != null)
        {
            audioSource.PlayOneShot(kamehamehaClips[syllableCount - 1]);
        }
        
        // Camera movement
        if (mainCameraTransform != null)
        {
            // Alternate between focusing on player and antagonist
            Vector3 targetPos = (syllableCount % 2 == 1) ? playerCameraPosition : antagonistCameraPosition;
            float zoomLevel = Mathf.Lerp(60f, 30f, syllableCount / 5f); // Gradually zoom in
            
            mainCameraTransform.DOMove(targetPos, 0.5f).SetEase(Ease.InOutQuad);
            Camera.main.DOFieldOfView(zoomLevel, 0.5f);
        }
    }
    
    private IEnumerator StartBeamClash()
    {
        currentState = BattleState.Charging;
        
        // Update instruction text
        if (instructionText != null)
        {
            instructionText.text = "CHARGING...";
        }
        
        // Move camera to clash position
        if (mainCameraTransform != null)
        {
            mainCameraTransform.DOMove(clashCameraPosition, 1f).SetEase(Ease.InOutQuad);
            Camera.main.DOFieldOfView(65f, 1f); // Wider field of view to see both beams
        }
        
        // Charging animation/effects
        if (playerPowerAura != null)
        {
            playerPowerAura.DOFade(0.4f, 1f).SetLoops(2, LoopType.Yoyo);
        }
        
        if (antagonistPowerAura != null)
        {
            antagonistPowerAura.DOFade(0.8f, 1f).SetLoops(2, LoopType.Yoyo);
        }
        
        // Wait for charging
        yield return new WaitForSeconds(2f);
        
        // Show beam power UI
        if (beamPowerSlider != null)
            beamPowerSlider.gameObject.SetActive(true);
            
        // Update instruction text
        if (instructionText != null)
        {
            instructionText.text = "MASH SPACE TO INCREASE BEAM POWER!";
            instructionText.transform.DOScale(Vector3.one * 1.2f, 0.5f).SetLoops(-1, LoopType.Yoyo);
        }
        
        // Initialize beams
        InitializeBeamSources(true);
        
        // Play beam start sound
        if (beamStartSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(beamStartSound);
        }
        
        // Start ongoing beam sound
        StartCoroutine(PlayBeamSounds());
        
        // Set battle state
        currentState = BattleState.BeamClash;
        lastSpacePressTime = Time.time;
        
        // Strong camera shake for beam start
        ShakeCamera(0.5f, 1.0f);
    }
    
    private IEnumerator HandleVictory()
    {
        // Update UI
        if (instructionText != null)
        {
            instructionText.text = "VICTORY!";
            instructionText.DOKill();
            instructionText.transform.DOScale(Vector3.one * 2f, 0.5f).SetEase(Ease.OutBack);
        }
        
        // Stop beam sounds
        StopAllCoroutines();
        
        // Play win sound
        if (playerWinSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(playerWinSound);
        }
        
        // Final camera shake
        ShakeCamera(1.5f, 1.2f);
        
        // Update beam strengths for final victory animation
        SourceObjectInitializer playerBeam = sim.GetSourceObject(playerBeamSourceIndex);
        SourceObjectInitializer antagonistBeam = sim.GetSourceObject(antagonistBeamSourceIndex);
        
        playerBeam.spawnRate = 2f; // Increase player beam
        antagonistBeam.spawnRate = 0f; // Disable antagonist beam
        
        sim.SetSourceObject(playerBeam, playerBeamSourceIndex);
        sim.SetSourceObject(antagonistBeam, antagonistBeamSourceIndex);
        
        // Wait for animation
        yield return new WaitForSeconds(2f);
        
        // Trigger level completion
        TriggerWin();
    }
    
    private IEnumerator HandleDefeat()
    {
        // Update UI
        if (instructionText != null)
        {
            instructionText.text = "DEFEATED!";
            instructionText.DOKill();
            instructionText.transform.DOScale(Vector3.one * 2f, 0.5f).SetEase(Ease.OutBack);
        }
        
        // Stop beam sounds
        StopAllCoroutines();
        
        // Play lose sound
        if (playerLoseSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(playerLoseSound);
        }
        
        // Final camera shake
        ShakeCamera(1.0f, 1.0f);
        
        // Update beam strengths for defeat animation
        SourceObjectInitializer playerBeam = sim.GetSourceObject(playerBeamSourceIndex);
        SourceObjectInitializer antagonistBeam = sim.GetSourceObject(antagonistBeamSourceIndex);
        
        playerBeam.spawnRate = 0f; // Disable player beam
        antagonistBeam.spawnRate = 2f; // Increase antagonist beam
        
        sim.SetSourceObject(playerBeam, playerBeamSourceIndex);
        sim.SetSourceObject(antagonistBeam, antagonistBeamSourceIndex);
        
        // Wait for animation
        yield return new WaitForSeconds(2f);
        
        // Restart level
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }
    
    private void InitializeBeamSources(bool active)
    {
        // Setup player beam
        SourceObjectInitializer playerBeam = sim.GetSourceObject(playerBeamSourceIndex);
        //playerBeam.color = playerBeamColor;
        playerBeam.spawnRate = active ? 1f : 0f;
        sim.SetSourceObject(playerBeam, playerBeamSourceIndex);
        
        // Setup antagonist beam
        SourceObjectInitializer antagonistBeam = sim.GetSourceObject(antagonistBeamSourceIndex);
        //antagonistBeam.color = antagonistBeamColor;
        antagonistBeam.spawnRate = active ? 1f : 0f;
        sim.SetSourceObject(antagonistBeam, antagonistBeamSourceIndex);
    }
    
    private void UpdateBeamSources()
    {
        // Update player beam strength based on power level
        SourceObjectInitializer playerBeam = sim.GetSourceObject(playerBeamSourceIndex);
        float playerPowerRatio = currentBeamPower / maxBeamPower;
        playerBeam.spawnRate = Mathf.Lerp(0.2f, 1.8f, playerPowerRatio);
        //playerBeam.radius = Mathf.Lerp(1f, 3f, playerPowerRatio);
        //playerBeam.lifetime = Mathf.Lerp(2f, 5f, playerPowerRatio);
        sim.SetSourceObject(playerBeam, playerBeamSourceIndex);
        
        // Update antagonist beam (inverse of player power)
        SourceObjectInitializer antagonistBeam = sim.GetSourceObject(antagonistBeamSourceIndex);
        float antagonistPowerRatio = 1f - playerPowerRatio;
        antagonistBeam.spawnRate = Mathf.Lerp(0.2f, 1.8f, antagonistPowerRatio);
        //antagonistBeam.radius = Mathf.Lerp(1f, 3f, antagonistPowerRatio);
        //antagonistBeam.lifetime = Mathf.Lerp(2f, 5f, antagonistPowerRatio);
        sim.SetSourceObject(antagonistBeam, antagonistBeamSourceIndex);
    }
    
    private IEnumerator PlayBeamSounds()
    {
        // Play ongoing beam clash sound in a loop
        while (currentState == BattleState.BeamClash)
        {
            if (beamClashSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(beamClashSound, 0.5f);
            }
            
            yield return new WaitForSeconds(1.0f);
        }
    }
    
    private Vector3 originalPosition;
    private Sequence shakeSequence;
    public void ShakeCamera(float duration, float strength)
    {
        // Kill any ongoing shake
        if (shakeSequence != null && shakeSequence.IsActive())
        {
            shakeSequence.Kill();
            if (originalPosition != Vector3.zero) Camera.main.transform.localPosition = originalPosition;
        }
        
        // Store the current position before shaking
        originalPosition = new Vector3(0, 0, -10);
        
        // Create a new sequence
        shakeSequence = DOTween.Sequence();
        
        // Add the shake with the passed parameters
        shakeSequence.Append(Camera.main.DOShakePosition(duration, strength));
        
        // Add a callback to return to the original position
        shakeSequence.OnComplete(() => Camera.main.transform.localPosition = originalPosition);

        // Play the sequence
        shakeSequence.Play();
    }
    
    void OnDestroy()
    {
        // Clean up DOTween animations
        DOTween.Kill(syllableText);
        DOTween.Kill(instructionText?.transform);
        DOTween.Kill(mainCameraTransform);
        DOTween.Kill(playerPowerAura);
        DOTween.Kill(antagonistPowerAura);
        
        // Clean up camera shake
        if (shakeSequence != null && shakeSequence.IsActive())
        {
            shakeSequence.Kill();
        }
    }
}