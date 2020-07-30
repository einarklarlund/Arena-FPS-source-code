using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupManager : Singleton<PickupManager>
{
    public List<Pickup> _pickupPrefabs;
    public Dictionary<string, Pickup> _pickupPrefabDictionary;

    // Start is called before the first frame update
    void Start()
    {
        _pickupPrefabDictionary = new Dictionary<string, Pickup>(); 

        _pickupPrefabDictionary.Add("Basic", _pickupPrefabs[0]);
        _pickupPrefabDictionary.Add("Attack", _pickupPrefabs[1]);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SpawnBasicPickup(Vector3 initialPosition)
    {
        // Pickup newPickup = Instantiate(_basicPickupPrefab) as Pickup;
        Pickup newPickup = Instantiate(_pickupPrefabDictionary["Basic"]) as Pickup;
        newPickup.initialPosition = initialPosition;
    }

    public void SpawnAttackPickup(Vector3 initialPosition)
    {
        // Pickup newPickup = Instantiate(_basicPickupPrefab) as Pickup;
        Pickup newPickup = Instantiate(_pickupPrefabDictionary["Attack"]) as Pickup;
        newPickup.initialPosition = initialPosition;
    }
}
