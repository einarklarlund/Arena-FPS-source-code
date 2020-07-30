using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyCard : Card
{
    public float spawnInterval = 0;

    public override void Pick()
    {
        EnemyManager.Instance.EnqueueEnemy(this);
    }
}
