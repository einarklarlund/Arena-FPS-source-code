using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GolemRequirements : CardRequirements
{
    public int maxGolems = 4;

    public override bool CheckRequirements()
    {
        int enemyIndex = EnemyManager.Instance.enemyIndexDictionary["Golem"];
    
        return EnemyManager.Instance.enemyCounts[enemyIndex] < maxGolems && GameFlowManager.Instance.currentRound > 3;
    }
}
