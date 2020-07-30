using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Card : MonoBehaviour
{
    [Header("Default Card properties")]
    [Tooltip("the entity that this card represents")]
    public GameObject entity;
    [Tooltip("the relative chance of this card being picked")]
    public float weight;
    [Tooltip("how many credits a Director must spend to spawn the entity")]
    public float creditCost;
    [Tooltip("The requirements that must be true in order the director to spawn the entity")]
    public CardRequirements requirements;

    public virtual void Pick()
    {
        Instantiate(entity);
    }
}
