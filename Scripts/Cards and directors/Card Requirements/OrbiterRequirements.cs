using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbiterRequirements : CardRequirements
{
    public int maxOrbiters = 20;

    public override bool CheckRequirements()
    {
        int enemyIndex = EnemyManager.Instance.enemyIndexDictionary["Orbiter"];
    
        return EnemyManager.Instance.enemyCounts[enemyIndex] < maxOrbiters;
    }
}
