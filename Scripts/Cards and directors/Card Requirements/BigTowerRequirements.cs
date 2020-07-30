using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BigTowerRequirements : CardRequirements
{
    public int maxBigTowers = 4;

    public override bool CheckRequirements()
    {
        int enemyIndex = EnemyManager.Instance.enemyIndexDictionary["Big Tower"];
    
        return EnemyManager.Instance.enemyCounts[enemyIndex] < maxBigTowers && GameFlowManager.Instance.currentRound > 1;
    }
}
