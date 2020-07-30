using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class Events 
{
    [System.Serializable] public class EventGameState : UnityEvent<GameManager.GameState, GameManager.GameState> { }
    [System.Serializable] public class EventGameStart : UnityEvent<float> { };
    [System.Serializable] public class EventEnemyState : UnityEvent<MyEnemyAI.EnemyState, MyEnemyAI.EnemyState> { }
    [System.Serializable] public class EventFadeComplete : UnityEvent<bool> { }
    [System.Serializable] public class EventPickup : UnityEvent<PlayerUpgradeManager> { };
}
