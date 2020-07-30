using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Selectable : MonoBehaviour
{
    public bool selectable { get; protected set; }
    public bool selected { get; protected set; }
    public abstract void Select(PlayerUpgradeManager playerUpgradeManager);
    public abstract void Unselect();
    public bool playerInSelectionRadius = false;
}
