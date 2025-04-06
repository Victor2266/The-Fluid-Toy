using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;


public class DrinkMixLevelManager : LevelManager
{
    [Header("Level References")]
    public CupFactory largeCupFactory;
    public CupFactory mediumCupFactory;
    public CupFactory smallCupFactory;
    public GameObject orderZoneObj;

    [Header("Cup Processing")]
    public float checkInterval = 0.2f; // Optimize performance by not checking every frame
    private float _lastCheckTime;

    // Private references
    private Transform orderZone;
    private Bounds _zoneBounds;
    private List<CupFactory.CupInstance> _allCups;

    // Start is called before the first frame update
    void Start()
    {
        if (!orderZoneObj || !largeCupFactory || !mediumCupFactory || !smallCupFactory)
        {
            Debug.LogError("Error! One or more null level references!");
            return;
        }
        _lastCheckTime = Time.time;

        // Get bounds of order zone transform
        orderZone = orderZoneObj.transform;
        _zoneBounds = new Bounds(orderZone.position, orderZone.localScale);
    }

    // Update is called once per frame
    // This script will check for the win conditions
    // this can be customized for each level
    void Update()
    {
        if (hasWon) TriggerWin();

        if (Time.time - _lastCheckTime < checkInterval) return;
        _lastCheckTime = Time.time;

        CheckOrderZone();
    }

    void CheckOrderZone()
    {
        List<CupFactory.CupInstance> cupsToRemove = new List<CupFactory.CupInstance>();

        // Get cups from factories
        _allCups = smallCupFactory.GetCups();
        _allCups.AddRange(mediumCupFactory.GetCups());
        _allCups.AddRange(largeCupFactory.GetCups());

        foreach (CupFactory.CupInstance cup in _allCups)
        {
            if (IsCupCompletelyContained(cup))
            {
                ProcessCupContents(cup, out FluidType majorityType, out float fluidPercent, out bool isFluidPresent);
                Debug.Log($"Order complete - Cup {cup.uniqueID} | FluidPercent: {fluidPercent}% | Present: {isFluidPresent} | MajorityFluid: {Enum.GetName(typeof(FluidType), majorityType)}");
                cupsToRemove.Add(cup);
            }
        }

        foreach (CupFactory.CupInstance cup in cupsToRemove)
        {
            if (smallCupFactory.GetCloneByID(cup.uniqueID) != null) smallCupFactory.DeleteCloneByID(cup.uniqueID);
            if (mediumCupFactory.GetCloneByID(cup.uniqueID) != null) mediumCupFactory.DeleteCloneByID(cup.uniqueID);
            if (largeCupFactory.GetCloneByID(cup.uniqueID) != null) largeCupFactory.DeleteCloneByID(cup.uniqueID);
        }
        TriggerDrain(0.05f);
    }

    bool IsCupCompletelyContained(CupFactory.CupInstance cup)
    {
        if (cup.cupObject == null) return false;

        // Get all renderers for precise bounds checking
        Renderer[] renderers = cup.cupObject.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) return false;

        foreach (Renderer renderer in renderers)
        {
            if (!_zoneBounds.Contains(renderer.bounds.min) ||
                !_zoneBounds.Contains(renderer.bounds.max))
            {
                return false;
            }
        }
        return true;
    }

    void ProcessCupContents(CupFactory.CupInstance cup, out FluidType majorityType, out float fluidPercent, out bool isFluidPresent)
    {
        FluidDetector[] detectors = cup.cupObject.GetComponentsInChildren<FluidDetector>();
        majorityType = FluidType.Disabled;
        fluidPercent = 0f;
        isFluidPresent = false;

        if (detectors.Length >= 2) // Large/medium
        {
            fluidPercent = (detectors[0].particlePercentage + detectors[1].particlePercentage) / 2f;
            isFluidPresent = detectors[0].isFluidPresent && detectors[1].isFluidPresent;
            if (detectors[0].majorityType == detectors[1].majorityType)
            {
                majorityType = detectors[0].majorityType; // Just take [0]
            }
            else // If they mixed 50/50 / only half full, we fail them
            {
                majorityType = FluidType.Disabled;
            }
        }
        else // Small
        {
            fluidPercent = detectors[0].particlePercentage;
            isFluidPresent = detectors[0].isFluidPresent;
            majorityType = detectors[0].majorityType;
        }
    }

    public void TriggerDrain(float duration)
    {
        GameObject drainObj = orderZone.transform.Find("Drain").gameObject;
        // OR use tag as shown above

        if (drainObj != null)
        {
            StartCoroutine(ActivateDrain(drainObj, duration));
        }
    }

    private IEnumerator ActivateDrain(GameObject drain, float duration)
    {
        drain.SetActive(true);
        yield return new WaitForSeconds(duration);
        drain.SetActive(false);
    }
}