using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

public class PipesLevelManager : LevelManager
{
    [Header("Level References")]
    public FluidDetector tankFluidDetector1;
    public FluidDetector tankFluidDetector2;
    void FixedUpdate()
    {
        if(hasWon) return;
        timer += Time.deltaTime;

        if(tankFluidDetector1 != null && tankFluidDetector2 != null){
            if(tankFluidDetector1.isFluidPresent && tankFluidDetector2.isFluidPresent){
                TriggerWin();
                hasWon = true;
            }
        }
    }
}