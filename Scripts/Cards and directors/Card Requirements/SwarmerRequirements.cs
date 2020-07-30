using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwarmerRequirements : CardRequirements
{
    public int maxSwarmers = 50;

    public override bool CheckRequirements()
    {
        int enemyIndex = EnemyManager.Instance.enemyIndexDictionary["Swarmer"];
    
        return EnemyManager.Instance.enemyCounts[enemyIndex] < maxSwarmers;
    }
}
