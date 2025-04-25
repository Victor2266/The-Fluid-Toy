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
    public GameObject playerCharacter1;
    public GameObject playerCharacter2;
    public GameObject antagonistCharacter;
    public GameObject antagonistCharacter2;
    public ParticleSystem playerPowerAura;
    public ParticleSystem antagonistPowerAura;
    
    [Header("Beam Sources")]
    public int playerBeamSourceIndex = 0;
    public int antagonistBeamSourceIndex = 1;
    public GameObject[] PlayerBeamEffects;
    public GameObject[] AntagonistBeamEffects;
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
    public AudioClip KA_Sound;
    public AudioClip ME_Sound;
    public AudioClip HA_Sound;
    public AudioClip ME_Sound2;
    public AudioClip GokuScreamSound;
    public AudioClip beamStartSound;
    public GameObject BeamClashSound;
    public AudioSource playerWinSound;
    public AudioSource playerLoseSound;
    public AudioClip[] powerUpSounds;
    
    [Header("Camera Effects")]
    public Transform mainCameraTransform;
    public OrthographicCameraAdjuster cameraAdjuster;
    public Vector3 playerCameraPosition;
    public Vector3 antagonistCameraPosition;
    public Vector3 clashCameraPosition;
    private float cameraZoomLevel = 9.87f;
    private float zoomLevelTarget = 9.87f;
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
    Color orange = new Color(1f, 0.5f, 0f);
    
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
            instructionText.text = "Press SPACE/LMB to begin Kamehameha";
            
        // Set up camera
        if (mainCameraTransform == null)
            mainCameraTransform = Camera.main.transform;
            
        // Initialize sources but turn them off
        InitializeBeamSources(false);
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
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
        {
            if (syllableCount < 5)
            {
                syllableCount++;
                PlayNextSyllable();
                if (syllableCount == 1){
                    instructionText.DOFade(0f, 0.25f);
                    for (int i = 0; i < 6; i++){
                        PlayerBeamEffects[i].SetActive(true);
                        AntagonistBeamEffects[i].SetActive(true);
                    }
                    audioSource.PlayOneShot(KA_Sound);
                }
                else if (syllableCount == 2){
                    audioSource.PlayOneShot(ME_Sound);
                }
                else if (syllableCount == 3){
                    PlayerBeamEffects[6].SetActive(true);
                    AntagonistBeamEffects[6].SetActive(true);
                    audioSource.PlayOneShot(HA_Sound);
                }
                else if (syllableCount == 4){
                    audioSource.PlayOneShot(ME_Sound2);
                }
                else if (syllableCount >= 5)
                {
                    StartCoroutine(StartBeamClash());
                }
            }
        }
        // Zoom camera
        if (syllableCount == 1 || syllableCount == 2){
            zoomLevelTarget = 3.81f;
        } else if (syllableCount == 3 || syllableCount == 4){
            zoomLevelTarget = 4.5f; 
        }
        // Gradually zoom out
        cameraZoomLevel = Mathf.Lerp(cameraZoomLevel, zoomLevelTarget, Time.deltaTime * 1.5f);
        cameraAdjuster.SetReferenceSizes(cameraZoomLevel);
        
    }
    
    private void HandleChargingState()
    {
        // Gradually zoom out
        zoomLevelTarget = 9.87f; 
        cameraZoomLevel = Mathf.Lerp(cameraZoomLevel, zoomLevelTarget, Time.deltaTime * 5f);
        cameraAdjuster.SetReferenceSizes(cameraZoomLevel);

        // Show charging animation
        // Automatically transitions to BeamClash after a short delay via coroutine
    }
    
    private void HandleBeamClashState()
    {
        // Process space bar spam
        if ((Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0)) && Time.time - lastSpacePressTime > spaceBarPressCooldown)
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
            int displayPower = Mathf.RoundToInt(Mathf.Lerp(0, 9999, currentBeamPower / maxBeamPower));

            powerLevelText.text = $"POWER LEVEL: {displayPower}";
            
            // Change color based on power level
            powerLevelText.color = Color.Lerp(Color.red, Color.blue, Mathf.InverseLerp(40f, 75f, currentBeamPower));
            powerLevelText.color = Color.Lerp(powerLevelText.color, Color.white, Mathf.InverseLerp(75f, 100f, currentBeamPower));
        }
        
        // Check for win/lose conditions
        if (currentBeamPower >= winThreshold)
        {
            // Player wins!
            powerLevelText.text = $"OVER 9000!";
            if (currentState != BattleState.Victory){
                currentState = BattleState.Victory;
                StartCoroutine(HandleVictory());
            }
        }
        else if (currentBeamPower <= loseThreshold)
        {
            // Player loses!
            powerLevelText.text = $"0";
            if (currentState != BattleState.Defeat){
                currentState = BattleState.Defeat;
                StartCoroutine(HandleDefeat());
            }
        }

        // Adjust screen shake based on beam power
        float shakeStrength = Mathf.Lerp(0.2f, 1.0f, currentBeamPower / maxBeamPower);
        if (Random.value < 0.05f) // Occasional random shake
        {
            ShakeCamera(0.2f, shakeStrength);
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

            if (syllableCount % 2 == 1)
            {
                syllableText.color = playerBeamColor;
            } else
            {
                syllableText.color = antagonistBeamColor;
            }
            if (syllableCount == 5){
                syllableText.color = Color.white;
                syllableText.colorGradient = new VertexGradient(playerBeamColor, playerBeamColor, antagonistBeamColor, antagonistBeamColor);
            }
            
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
            mainCameraTransform.DOMove(targetPos, 0.5f).SetEase(Ease.InOutQuad);
            
            //Camera.main.DOOrthoSize(zoomLevel, 0.5f);
            //Camera.main.DOFieldOfView(zoomLevel, 0.5f);
        }
    }
    
    private IEnumerator StartBeamClash()
    {
        currentState = BattleState.Charging;
        
        // Update instruction text
        if (instructionText != null)
        {
            instructionText.alpha = 1f;
            instructionText.text = "HAAAAAAAAAAAAAAAAAAAA...";
        }
        
        // Move camera to clash position
        if (mainCameraTransform != null)
        {
            mainCameraTransform.DOMove(clashCameraPosition, 1f).SetEase(Ease.InOutQuad);
            Camera.main.DOFieldOfView(65f, 1f); // Wider field of view to see both beams
        }
        
        // Charging animation/effects
        playerCharacter1.SetActive(false);
        playerCharacter2.SetActive(true);
        antagonistCharacter.SetActive(false);
        antagonistCharacter2.SetActive(true);

        backgroundMusic.Play();

        // Play beam start sound
        if (GokuScreamSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(GokuScreamSound, 0.6f);
        }

        // Wait for charging
        yield return new WaitForSeconds(2f);
        
        // Show beam power UI
        if (beamPowerSlider != null)
            beamPowerSlider.gameObject.SetActive(true);
            
        // Update instruction text
        if (instructionText != null)
        {
            instructionText.text = "MASH SPACE/LMB TO INCREASE BEAM POWER!";
            instructionText.transform.DOScale(Vector3.one * 1.2f, 0.25f).SetLoops(-1, LoopType.Yoyo);
        }
        
        // Initialize beams
        InitializeBeamSources(true);
        
        // Play beam start sound
        if (beamStartSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(beamStartSound, 0.25f);
        }
        
        // Start ongoing beam sound
        BeamClashSound.SetActive(true);
        
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
        
        // Play win sound
        if (playerWinSound)
        {
            playerWinSound.Play();
        }

        // Hide beams
        for(int i = 0; i < AntagonistBeamEffects.Length; i++)
        {
            AntagonistBeamEffects[i].transform.DOScale(Vector3.zero, 0.5f).OnComplete(() => 
            {
                for (int i = 0; i < AntagonistBeamEffects.Length; i++)
                {
                    Destroy(AntagonistBeamEffects[i]);
                }
            });
        }
        antagonistCharacter2.GetComponent<SpriteRenderer>().DOFade(0f, 0.5f);
        
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
            instructionText.text = "<color=red>DEFEAT!";
            instructionText.DOKill();
            instructionText.transform.DOScale(Vector3.one * 2f, 0.5f).SetEase(Ease.OutBack);
        }

        powerLevelText.DOFade(0f, 0.5f);
        
        // Play lose sound
        if (playerLoseSound != null)
        {
            playerLoseSound.Play();
        }

        // Hide beams
        for (int i = 0; i < PlayerBeamEffects.Length; i++)
        {
            PlayerBeamEffects[i].transform.DOScale(Vector3.zero, 0.5f).OnComplete(() =>
            {
                for (int i = 0; i < PlayerBeamEffects.Length; i++)
                {
                    Destroy(PlayerBeamEffects[i]);
                }
            });
        }
        playerCharacter2.GetComponent<SpriteRenderer>().DOFade(0f, 0.5f);

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
        playerBeam.spawnRate = Mathf.Lerp(0f, 1.8f, playerPowerRatio);
        //playerBeam.radius = Mathf.Lerp(1f, 3f, playerPowerRatio);
        //playerBeam.lifetime = Mathf.Lerp(2f, 5f, playerPowerRatio);
        sim.SetSourceObject(playerBeam, playerBeamSourceIndex);
        
        // Update antagonist beam (inverse of player power)
        SourceObjectInitializer antagonistBeam = sim.GetSourceObject(antagonistBeamSourceIndex);
        float antagonistPowerRatio = 1f - playerPowerRatio;
        antagonistBeam.spawnRate = Mathf.Lerp(0.1f, 1.8f, antagonistPowerRatio);
        //antagonistBeam.radius = Mathf.Lerp(1f, 3f, antagonistPowerRatio);
        //antagonistBeam.lifetime = Mathf.Lerp(2f, 5f, antagonistPowerRatio);
        sim.SetSourceObject(antagonistBeam, antagonistBeamSourceIndex);
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
        if (syllableText != null) DOTween.Kill(syllableText);
       
        if (instructionText != null) {
            DOTween.Kill(instructionText);
        }
        DOTween.Kill(powerLevelText);
        DOTween.Kill(mainCameraTransform);
        DOTween.Kill(playerPowerAura);
        DOTween.Kill(antagonistPowerAura);

        for(int i = 0; i < PlayerBeamEffects.Length; i++)
        {
            if (PlayerBeamEffects[i] != null) DOTween.Kill(PlayerBeamEffects[i]);
            if (AntagonistBeamEffects[i] != null) DOTween.Kill(AntagonistBeamEffects[i]);
        }
        if (playerCharacter2 != null) DOTween.Kill(playerCharacter2.GetComponent<SpriteRenderer>());
        if (antagonistCharacter2 != null) DOTween.Kill(antagonistCharacter2.GetComponent<SpriteRenderer>());

        // Clean up camera shake
        if (shakeSequence != null && shakeSequence.IsActive())
        {
            shakeSequence.Kill();
        }
    }
}