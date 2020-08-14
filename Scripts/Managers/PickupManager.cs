using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupManager : Singleton<PickupManager>
{
    public List<GameObject> _pickupPrefabs;
    public Dictionary<string, GameObject> _pickupPrefabDictionary;

    // Start is called before the first frame update
    void Start()
    {
        _pickupPrefabDictionary = new Dictionary<string, GameObject>(); 

        _pickupPrefabDictionary.Add("Basic", _pickupPrefabs[0]);
        _pickupPrefabDictionary.Add("Attack", _pickupPrefabs[1]);
        _pickupPrefabDictionary.Add("Movement", _pickupPrefabs[2]);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ChooseDeathPickup(Vector3 initialPosition)
    {
        float rand = UnityEngine.Random.value;

        if(rand < 0.1f)
        {
            Debug.Log("spawning attack pickup");
            SpawnAttackPickup(initialPosition);
        }
        else if(rand < 0.3f)
        {
            SpawnMovementPickup(initialPosition);
        }
    }

    public void SpawnBasicPickup(Vector3 initialPosition)
    {
        GameObject newPickup = Instantiate(_pickupPrefabDictionary["Basic"]);
        Pickup pickupComponent = newPickup.GetComponent<Pickup>();
        pickupComponent.initialPosition = initialPosition;
    }

    public void SpawnAttackPickup(Vector3 initialPosition)
    {
        GameObject newPickup = Instantiate(_pickupPrefabDictionary["Attack"]);
        Pickup pickupComponent = newPickup.GetComponent<Pickup>();
        pickupComponent.initialPosition = initialPosition;
    }
    
    public void SpawnMovementPickup(Vector3 initialPosition)
    {
        GameObject newPickup = Instantiate(_pickupPrefabDictionary["Movement"]);
        Pickup pickupComponent = newPickup.GetComponent<Pickup>();
        pickupComponent.initialPosition = initialPosition;
    }
}
