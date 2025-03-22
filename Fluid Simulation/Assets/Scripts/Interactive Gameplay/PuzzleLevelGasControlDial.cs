using System.Runtime.CompilerServices;
using UnityEngine;

public class PuzzleLevelGasControlDial : MonoBehaviour
{  
    [Header("Level references")]
    public IFluidSimulation sim;

    [Header("Burner Sound settings")]
    public AudioSource burnerSound;
    public float minVolume = 0;
    public float maxVolume = 1;

    [Header("Fluid source control settings")]
    public int sourceIndex;
    public float minVelo = 0;
    public float maxVelo = 1F;
    public float minAngle = 0F;
    public float maxAngle = 180F;
    public float totalVelo = 1.5F;
    public float minSpawn = 0;
    public float maxSpawn = 1F;

    [Header("Dial references")]
    public PuzzleLevelGasControlDial[] dials;


    private GameObject simObject;
    private float currVelo = 0F;
    private float currSpawn = 0F;
    private SourceObjectInitializer source;
    private bool pressed = false;


    void Start()
    {
        //burner sound source retrieved from components if not specified
        if(burnerSound == null){
            burnerSound = GetComponent<AudioSource>();
        }
        if (sim == null){
            simObject = GameObject.FindGameObjectWithTag("Simulation");
            sim = simObject.GetComponent<IFluidSimulation>();
        }
    }

    /// <summary>
    /// Updates sprite/object rotation if object is currently being held by player.
    /// Direction of sprite rotation is determined by mouse position, and limited to minAngle and maxAngle.
    /// Spawnrate and velocity of source object specified by sourceIndex are varied between their minimum and maximum values by the angleToMouse variable.
    /// Dial position and sound is updated every fixed update since each dial in dials array affect eachother.
    /// </summary>
    void FixedUpdate()
    {
        if(pressed){
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3 toMouse = mousePos - transform.position;
            float angleToMouse = -1 * Mathf.Atan2(toMouse.x, toMouse.y) * Mathf.Rad2Deg;
            if(angleToMouse < minAngle){
                if(Mathf.Abs(angleToMouse) > 90){
                    angleToMouse = maxAngle;
                }else{
                    angleToMouse = minAngle;
                }
            }
            angleToMouse = Mathf.Clamp(angleToMouse, minAngle, maxAngle);
            transform.eulerAngles = new Vector3(0, 0, angleToMouse);
            currVelo = Remap(angleToMouse, minAngle, maxAngle, minVelo, maxVelo);
            currSpawn = Remap(angleToMouse, minAngle, maxAngle, minSpawn, maxSpawn);
            updateSource();
        }else{
            updateDial();
        }
        updateSound();
    }

    /// <summary>
    /// Updates the velocity and spawnrate of each dial in Dials based on the maximum allowed velocity total.
    /// If player increases velocity of selected dial past allowed maximum of all dials, the other dials will both decrease evenly to ensure the maximum value is not violated.
    /// The source indexed by sourceIndex is updated with the velocity and spawn rate values after each dial velocity has been updated.
    /// </summary>
    void updateSource(){
        float sumVelo = 0;
        float reduction = 0;
        foreach(PuzzleLevelGasControlDial dial in dials){
            sumVelo += dial.getVelo();
        }
        if (sumVelo + currVelo > totalVelo){
            reduction = (sumVelo + currVelo -totalVelo)/dials.Length;
        }
        foreach(PuzzleLevelGasControlDial dial in dials){
            dial.setVelo(Mathf.Max(dial.getVelo() - reduction, minVelo));
        }
        source = sim.GetSourceObject(sourceIndex);
        source.velo.y = currVelo;
        source.spawnRate = currSpawn;
        sim.SetSourceObject(source, sourceIndex);
    }

    /// <summary>
    /// check for player click on dial
    /// </summary>
    void OnMouseDown()
    {
        pressed = true;
        
    }

    /// <summary>
    /// checks if player release dial
    /// </summary>
	void OnMouseUp()
	{
		pressed = false;
	}

    /// <summary>
    /// public getter function for velocity.
    /// </summary>
    /// <returns>currVelo</returns>
	public float getVelo(){
        return currVelo;
    }
    /// <summary>
    /// public setter function for velocity
    /// </summary>
    /// <param name="velo"></param>
    public void setVelo(float velo){
        currVelo = velo;
    }
    
    /// <summary>
    /// Updates dial position and source spawnrate and velocity based on currVelo value.
    /// Called when dial is not being controlled by player.
    /// </summary>
    void updateDial(){
        float angle = Remap(currVelo, minVelo, maxVelo, minAngle, maxAngle);
        currSpawn = Remap(currVelo, minVelo, maxVelo, minSpawn, maxSpawn);
        transform.eulerAngles = new Vector3(0, 0, angle);
        source = sim.GetSourceObject(sourceIndex);
        source.velo.y = currVelo;
        source.spawnRate = currSpawn;
        sim.SetSourceObject(source, sourceIndex);
    }

    /// <summary>
    /// Updates burner sound effect volume based on current velocity.
    /// Volume is constrained to minVolume and maxVolume.
    /// </summary>
    void updateSound(){
        float vol = Remap(currVelo, minVelo, maxVelo, minVolume, maxVolume);
        if(burnerSound.volume == 0 && vol != 0){
            burnerSound.volume = vol;
            burnerSound.Play();

        }else if(vol == 0 && burnerSound.volume != 0){
            burnerSound.volume = vol;
            burnerSound.Stop();
        }else{
            burnerSound.volume = vol;
        }
        
        
    }

    /// <summary>
    /// Remaps input value with range sourceFrom to SourceTo, to a value in range targetFrom to targetTo.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="sourceFrom"></param>
    /// <param name="sourceTo"></param>
    /// <param name="targetFrom"></param>
    /// <param name="targetTo"></param>
    /// <returns>source value remapped to new limits.</returns>
    float Remap(float source, float sourceFrom, float sourceTo, float targetFrom, float targetTo)
    {
	    return targetFrom + (source-sourceFrom)*(targetTo-targetFrom)/(sourceTo-sourceFrom);
    }

    
}
