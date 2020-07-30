using System;
using System.Collections;
using System.Collections.Specialized;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EnemyManager : Singleton<EnemyManager>
{
    public List<EnemyCard> enemyCards;
    public Dictionary<string, int> enemyIndexDictionary;
    public List<int> enemyCounts { get; private set; }

    public int numberOfEnemiesRemaining { get; private set; }
    public int numberOfEnemiesTotal { get; private set; }
    public int enemiesKilled => numberOfEnemiesTotal - numberOfEnemiesRemaining;
    
    private List<int> enemySpawnCounts = null;
    private List<float> spawnTimes;
    
    private void Start()
    {
        GameManager.Instance.OnGameStart.AddListener(HandleGameStart);
    }

    private void FixedUpdate()
    {
        if(GameManager.Instance.CurrentGameState != GameManager.GameState.RUNNING)
            return;

        for(int index = 0; index < enemySpawnCounts.Count; ++index)
        {
            EnemyCard card = enemyCards[index];

            //spawn new enemy if spawncount > 0 and spawn interval has passed
            if(enemySpawnCounts[index] > 0 && Time.time - spawnTimes[index] >= card.spawnInterval)
            {
                SpawnEnemy(card);
                enemySpawnCounts[index]--;
                spawnTimes[index] = Time.time;
            }
        }
    } 

    public void HandleGameStart(float time)
    {   
        numberOfEnemiesTotal = 0;
        numberOfEnemiesRemaining = 0;
        enemySpawnCounts = new List<int>();
        enemyCounts = new List<int>();
        spawnTimes = new List<float>();
        enemyIndexDictionary = new Dictionary<string, int>(); 

        for(int i = 0; i < enemyCards.Count; i++)
        {
            enemyIndexDictionary.Add(enemyCards[i].entity.name, i);
            enemySpawnCounts.Add(0);
            enemyCounts.Add(0);
            spawnTimes.Add(0f);
        }
    }

    public void SpawnEnemy(EnemyCard card, Vector3 position = default(Vector3), Vector3 direction = default(Vector3))
    {
        GameObject newEnemy = Instantiate(card.entity);
        EnemyBehavior behavior = newEnemy.GetComponent<EnemyBehavior>();
        
        if(position != default(Vector3))
        {
            behavior.initialPosition = position;
        }

        if(direction != default(Vector3))
        {
            behavior.initialDirection = direction;
        }

        numberOfEnemiesTotal++;
        numberOfEnemiesRemaining++;
        enemyCounts[enemyIndexDictionary[card.entity.name]]++;
    }

    public void EnqueueEnemy(EnemyCard card)
    {
        //add to the number of spawns for this enemy type
        enemySpawnCounts[enemyIndexDictionary[card.entity.name]]++;
    }

    public void UnregisterEnemy(string enemyName)
    {
        enemyCounts[enemyIndexDictionary[enemyName]]--;
        numberOfEnemiesRemaining--;
    }
}
