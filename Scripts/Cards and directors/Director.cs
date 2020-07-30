using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Director : MonoBehaviour
{
    [Header("Card and credit variables")]
    public List<Card> cards;
    public float credits;
    public float creditMultiplier = 1f;
    public bool useAllCreditsOnEachSpawn;
    public bool canGenerateCredits = true;

    [Header("Spawning and income variables")]
    public float spawnInterval = 0f;
    public float spawnIntervalRandomDeviation = 0.3f;
    public float incomeInterval = 0f;
    public bool incomeIntervalScales;
    [SerializeField] private float _incomeIntervalScaleModifier = 0.9f;
    private float _incomeIntervalBaseTime;
    private float _spawnIntervalBaseTime;
    private float _lastIncomeGenerationTime;
    private float _lastSpawnTime;
    private float _delta = 0.005f;

    // Start is called before the first frame update
    void Start()
    {
        _lastIncomeGenerationTime = Time.time;
        _incomeIntervalBaseTime = incomeInterval;
        _spawnIntervalBaseTime = spawnInterval;

        credits += 0.05f; // add a little bit to credits to help mitigate floating point precision loss
    }

    // Update is called once per frame
    void Update()
    {
        if(Time.time - _lastIncomeGenerationTime >= incomeInterval && incomeInterval != 0 && canGenerateCredits)
        {
            GenerateIncome();
        }
        
        if(credits > 0 && Time.time - _lastSpawnTime >= spawnInterval && spawnInterval != 0)
        {
            AttemptBuy();
            spawnInterval = _spawnIntervalBaseTime - (spawnIntervalRandomDeviation / 2) + (UnityEngine.Random.value * spawnIntervalRandomDeviation);
        }
    }

    public void GiveCredits(float credits)
    {
        credits += credits;
    }

    public float SumOfCardWeights(List<Card> cardsList)
    {
        float val = 0f;
        foreach(Card card in cardsList)
        {
            val += card.weight;
        }
        return val;
    }

    public void GenerateIncome()
    {
        Debug.Log("Adding " + creditMultiplier * (GameFlowManager.Instance.difficulty) + " credits to " + name);
        credits += creditMultiplier * (GameFlowManager.Instance.difficulty);
        _lastIncomeGenerationTime = Time.time;
        // Debug.Log("GenerateIncome called at time " + Time.time + " final credits is " + credits + " initial income interval is " + incomeInterval);

        if(incomeIntervalScales && GameFlowManager.Instance.difficulty > 0)
        {
            incomeInterval = _incomeIntervalBaseTime * (_incomeIntervalScaleModifier * GameFlowManager.Instance.difficulty);
        }

        // Debug.Log(_incomeIntervalBaseTime + " " + _incomeIntervalScaleModifier +  " " + GameFlowManager.Instance.difficulty);
    }

    public void EmptyCredits()
    {
        credits = 0;
    }
    protected Card PickCard()
    {
        List<Card> validCards = new List<Card>();

        //find the cards that the director has enough credits for
        foreach(Card card in cards)
            if((card.creditCost <= credits + _delta || card.creditCost <= credits - _delta)
                && card.requirements.CheckRequirements())
            {
                validCards.Add(card);
            }
        
        //return null if director doesn't have enough credits to pick any cards
        if(validCards.Count == 0)
            return null;

        float allValidWeights = SumOfCardWeights(validCards);
        float randomValue = UnityEngine.Random.value;
        float cumulativeSum = 0f;
        int index;

        //increment index until the cumulative sum of the cards' relative weight passes the random value
        for(index = 0; cumulativeSum < randomValue && index < validCards.Count; ++index)
        {
            cumulativeSum += validCards[index].weight / allValidWeights;
        }
        //decrement index because we passed the random value in the for loop
        index--;

        if(credits >= validCards[index].creditCost + _delta || credits >= validCards[index].creditCost - _delta)
        {
            //make sure credits doesn't go negative
            float newCredits = credits - validCards[index].creditCost;
            credits = newCredits >= 0f ? newCredits : 0f;
            //return the card at the weighted random index
            return validCards[index];
        }
        else
        {
            Debug.LogError("[Director] Picked a card whose cost was " + validCards[index].creditCost +" but director only has " + credits + " credits, _delta is " + _delta);     
            return null;       
        }
    }

    protected void AttemptBuy()
    {
        if(useAllCreditsOnEachSpawn)
            AttemptBuyCards();
        else
            AttemptBuyCard();
    }

    protected void AttemptBuyCard()
    {
        Card nextCard;
        float initialCredits = credits;

        if((nextCard = PickCard()) != null)
        {
            nextCard.Pick();
            _lastSpawnTime = Time.time;
            // newEntity = Instantiate(nextCard.entity) as GameObject;
            // Debug.Log("Spawned a " + newEntity.tag + " entity at " + Time.time + " for " + nextCard.creditCost + " credits, " + credits + " left");
        }
    }

    protected void AttemptBuyCards()
    {
        Card nextCard;
        int numCards = 0;

        while((nextCard = PickCard()) != null)
        {
            nextCard.Pick();
            numCards++;
        }

        if(numCards > 0)
        {
            _lastSpawnTime = Time.time;
        }
        // Debug.Log("Started w " + initialCredits + " credits, created " + entitiesSpawned.Count + " entities, ended w " + credits + " credits");
    }
}
