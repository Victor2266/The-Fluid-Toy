using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;


public class DrinkLevelManager : LevelManager
{
    public enum CupSize
    {
        Small,
        Medium,
        Large
    }
    public class DrinkOrder
    {
        public string uniqueID;
        public FluidType drinkType;
        public CupSize size;
        public System.DateTime orderTime;
        public float timeLimitSeconds;
        public bool isExpired = false;
        public bool fulfilled = false;

        public DrinkOrder(FluidType drinkType, CupSize size)
        {
            uniqueID = System.Guid.NewGuid().ToString();
            this.drinkType = drinkType;
            this.size = size;
            orderTime = System.DateTime.Now;
            timeLimitSeconds = float.PositiveInfinity; // No limit
        }

        public DrinkOrder(FluidType drinkType, CupSize size, float timeLimitSeconds = 20f)
        {
            uniqueID = System.Guid.NewGuid().ToString();
            this.drinkType = drinkType;
            this.size = size;
            orderTime = System.DateTime.Now;
            this.timeLimitSeconds = timeLimitSeconds;
        }
 
        public float GetRemainingTime()
        {
            return timeLimitSeconds - (float)(System.DateTime.Now - orderTime).TotalSeconds;
        }


        public bool CheckExpired()
        {
            if (isExpired) return true;

            isExpired = GetRemainingTime() <= 0f;
            return isExpired;
        }

        public string GetFormattedTime()
        {
            // Get formatted time string (MM:SS)
            float remaining = GetRemainingTime();
            if (remaining <= 0) return "00:00";

            int minutes = Mathf.FloorToInt(remaining / 60f);
            int seconds = Mathf.FloorToInt(remaining % 60f);
            return $"{minutes:00}:{seconds:00}";
        }
    }

    [Header("Level References")]
    public CupFactory largeCupFactory;
    public CupFactory mediumCupFactory;
    public CupFactory smallCupFactory;
    public GameObject orderZoneObj;
    public DrinkLevelUIManager orderUIManager;

    [Header("Order Timing")]
    public float defaultTimeLimit = 20f; // Default time limit in seconds
    public Color normalOrderColor = Color.white;
    public Color warningOrderColor = Color.yellow;
    public Color criticalOrderColor = Color.red;
    public float warningThreshold = 0.3f; // 30% of time remaining
    public float criticalThreshold = 0.1f; // 10% of time remaining

    [Header("Drink Orders")]
    public List<DrinkOrder> orders = new List<DrinkOrder>();

    [Header("Cup Processing")]
    public float checkInterval = 0.2f; // Optimize performance by not checking every frame
    private float _lastCheckTime;

    // Private references
    private Transform orderZone;
    private Bounds _zoneBounds;
    private List<CupFactory.CupInstance> _allCups;
    private IFluidSimulation sim;

    // Start is called before the first frame update
    void Start()
    {
        GameObject simObject = GameObject.FindGameObjectWithTag("Simulation");
        sim = simObject.GetComponent<IFluidSimulation>();

        if (!orderZoneObj || !largeCupFactory || !mediumCupFactory || !smallCupFactory)
        {
            Debug.LogError("Error! One or more null level references!");
            return;
        }
        _lastCheckTime = Time.time;

        // Get bounds of order zone transform
        orderZone = orderZoneObj.transform;
        _zoneBounds = new Bounds(orderZone.position, orderZone.localScale);

        // Generate order list (no timers)
        AddOrder(FluidType.Beer, CupSize.Medium);
        AddOrder(FluidType.Alcohol, CupSize.Small);
        AddOrder(FluidType.Beer, CupSize.Large);
        AddOrder(FluidType.Cola, CupSize.Large);
    }

    // Update is called once per frame
    // This script will check for the win conditions
    // this can be customized for each level
    void Update()
    {
        if (hasWon) TriggerWin();

        if (Time.time - _lastCheckTime < checkInterval) return;
        _lastCheckTime = Time.time;

        CheckOrderTimeouts();
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
            TriggerDrain(0.001f);
        }
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

        if (isFluidPresent && majorityType != FluidType.Disabled)
        {
            CheckOrderFulfillment(cup, cup.size, majorityType);
        }
    }

    public void TriggerDrain(float duration)
    {
        if (orderZone != null)
        {
            StartCoroutine(ActivateDrain(duration));
        }
    }

    private IEnumerator ActivateDrain( float duration)
    {
        // Function is kinda costly because of UpdateDrainObjects's global scan, but it's not too bad because we only check after dragging in cups
        orderZone.tag = "DrainObject";
        sim.UpdateDrainObjects();
        yield return new WaitForSeconds(duration);
        orderZone.tag = "Untagged";
        sim.UpdateDrainObjects();
    }

    private void AddOrder(FluidType drink, CupSize size, float timeLimit = -1f)
    {
        if (timeLimit > 0)
        {
            orders.Add(new DrinkOrder(drink, size, timeLimit));
            orderUIManager.CreateOrderUI(drink, size, timeLimit);
        }
        else
        {
            orders.Add(new DrinkOrder(drink, size)); // No timer
            orderUIManager.CreateOrderUI(drink, size, float.PositiveInfinity);
        }
    }

    void CheckOrderTimeouts()
    {
        for (int i = orders.Count - 1; i >= 0; i--)
        {
            if (orders[i].CheckExpired())
            {
                Debug.Log($"Order expired! ID: {orders[i].uniqueID}");
                orders.RemoveAt(i);
            }
        }
    }

    public Color GetOrderTimeColor(DrinkOrder order)
    {
        float remainingRatio = order.GetRemainingTime() / order.timeLimitSeconds;

        if (remainingRatio <= criticalThreshold)
            return criticalOrderColor;
        if (remainingRatio <= warningThreshold)
            return warningOrderColor;
        return normalOrderColor;
    }

    void CompleteOrder(DrinkOrder order)
    {
        orders.Remove(order);
        if (orderUIManager != null)
        {
            orderUIManager.RemoveOrderUI($"Order_{order.drinkType}_{order.size}");
        }
    }

    private void CheckOrderFulfillment(CupFactory.CupInstance cup, CupSize cupSize, FluidType fluidType)
    {
        // Find all matching orders
        List<DrinkOrder> matchingOrders = orders.FindAll(order =>
            order.drinkType == fluidType &&
            order.size == cupSize &&
            !order.isExpired);

        if (matchingOrders.Count > 0)
        {
            // Fulfill the first matching order
            DrinkOrder fulfilledOrder = matchingOrders[0];
            CompleteOrder(fulfilledOrder);

            Debug.Log($"Order fulfilled! {cupSize} {fluidType}");
        }
    }

}