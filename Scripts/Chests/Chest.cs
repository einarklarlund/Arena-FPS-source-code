using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Chest : Selectable
{
    [SerializeField] private List<GameObject> _pickups = null;
    [SerializeField] protected Animation _chestAnimator = null;
    [SerializeField] float _initialDistanceFromPlayer = 12f;
    [SerializeField] float lifetime = 25f; 
    public Vector3 initialPosition = new Vector3(0, 0, 0);
    public bool open { get; private set; }
    public int costToOpen = 6;

    private float startTime;

    protected PlayerUpgradeManager _playerUpgradeManager = null;

    protected abstract void OnSelect();
    protected abstract void OnUnselect();
    protected abstract void OnOpen();
    protected abstract void OnStart();

    // Start is called before the first frame update
    private void Start()
    {
        playerInSelectionRadius = false;
        open = false;

        _playerUpgradeManager = GameObject.Find("Player").GetComponent<PlayerUpgradeManager>();

        //set initial position
        Transform playerTransform = UnityEngine.Object.FindObjectOfType<PlayerCharacterController>().transform;
        Vector2 randomPoint = UnityEngine.Random.insideUnitCircle;
        Vector3 spawnPosition = new Vector3(randomPoint.normalized.x * _initialDistanceFromPlayer, 0, randomPoint.normalized.y * _initialDistanceFromPlayer);
        spawnPosition += new Vector3(playerTransform.position.x, 0f, playerTransform.position.z);

        transform.Translate(spawnPosition);
        transform.LookAt(playerTransform);

        selectable = true;

        startTime = Time.time;

        OnStart();
    }

    private void FixedUpdate()
    {        
        if(!selectable && transform.position.y < 0.35f)
        {
            transform.Translate(0f, 0.1f, 0f);
            if(transform.position.y >= 0.35)
            {
                selectable = true;
            }
        }

        if(Time.time - startTime >= lifetime)
        {
            OnOpen();
            Destroy(gameObject, 0.6f);
        }
    }

    private void Update()
    {
        if(selected && !playerInSelectionRadius)
        {
            Unselect();
        }

        if(selected && PlayerInputHandler.Instance.GetUseInputDown() && !open)
        {
            OpenChest();
        }
    }

    public void OpenChest()
    {
        if(!_playerUpgradeManager)
        {
            Debug.LogError("[Chest] playerupgrademanager was null when openchest was called");
            return;
        }

        if(!open && _playerUpgradeManager.numBasicPickups >= costToOpen)
        {
            //find random integer between 0 and _pickups.Count
            double rand = Math.Truncate(UnityEngine.Random.value * _pickups.Count);

            //instantiate random pickup from the list
            GameObject pickupGameObject = Instantiate(_pickups[(int) rand]) as GameObject;
            Pickup pickup = pickupGameObject.GetComponent<Pickup>();
            pickup.initialPosition = transform.position;
            open = true;
            _playerUpgradeManager.numBasicPickups -= costToOpen;
            
            Destroy(gameObject, 0.6f);

            OnOpen();
        }

    }

    public override void Select(PlayerUpgradeManager playerUpgradeManager)
    {
        if(playerInSelectionRadius && !selected && !open)
        {
            selected = true;
            _playerUpgradeManager = playerUpgradeManager;
            OnSelect();
        }
    }

    public override void Unselect()
    {
        if(selected && !open)
        {
            selected = false;
            _playerUpgradeManager = null;
            OnUnselect();
        }
    }

}
