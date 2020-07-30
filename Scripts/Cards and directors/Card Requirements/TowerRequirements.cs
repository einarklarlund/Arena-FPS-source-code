using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerRequirements : CardRequirements
{
    public int maxTowers = 8;

    public override bool CheckRequirements()
    {
        int numTowers = EnemyManager.Instance.enemyCounts[EnemyManager.Instance.enemyIndexDictionary["Tower"]];
    
        return numTowers < GameFlowManager.Instance.difficulty && numTowers < maxTowers;
    }
}
