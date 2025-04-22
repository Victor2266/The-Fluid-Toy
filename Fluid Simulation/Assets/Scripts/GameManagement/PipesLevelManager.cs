using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

public class PipesLevelManager : LevelManager
{
    [Header("Level References")]
    public FluidDetector tankFluidDetector;
    void FixedUpdate()
    {
        if(hasWon) return;

        if(tankFluidDetector != null){
            if(tankFluidDetector.isFluidPresent){
                TriggerWin();
                hasWon = true;
            }
        }
    }
}