using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUpgradeManager : MonoBehaviour
{
    public int numBasicPickups;
    public float nonShootingSpeedModifier;

    private PlayerCharacterController _playerCharacterController = null;
    private PlayerWeaponsManager _playerWeaponsManager = null;
    private WeaponController _weaponController = null;
    private HUD _hud;
    private System.Random rng;

    // Start is called before the first frame update
    void Start()
    {
        Physics.IgnoreLayerCollision(LayerMask.NameToLayer("Default"), LayerMask.NameToLayer("DontHit"), true);
        
        _playerCharacterController = GetComponent<PlayerCharacterController>();
        DebugUtility.HandleErrorIfNullGetComponent<PlayerCharacterController, PlayerWeaponsManager>(_playerCharacterController, this, gameObject);

        _playerWeaponsManager = GetComponent<PlayerWeaponsManager>();
        DebugUtility.HandleErrorIfNullGetComponent<PlayerWeaponsManager, PlayerWeaponsManager>(_playerWeaponsManager, this, gameObject);

        _hud = GetComponentInChildren<HUD>();
        DebugUtility.HandleErrorIfNullGetComponent<HUD, PlayerWeaponsManager>(_hud, this, gameObject);

        _weaponController = _playerWeaponsManager.GetActiveWeapon();

        rng = new System.Random();

        numBasicPickups = 0;
        nonShootingSpeedModifier = 1f;
    }

    // Update is called once per frame
    void Update()
    {
        if(GameManager.Instance.CurrentGameState != GameManager.GameState.RUNNING)
            return;

        if(!_weaponController)
            _weaponController = _playerWeaponsManager.GetActiveWeapon();        
    }

    public void AddBasicPickup()
    {
        numBasicPickups++;
    }

    public void AddAttackPickup()
    {
        //find random integer between 0 and 2
        int rand = rng.Next(3);
        switch(rand)
        {
            case 0:
                //upgrade attack speed
                _hud.DisplayMessage("Attack speed bonus");
                _weaponController.delayBetweenShots *= 0.4f;
                _weaponController.bulletSpreadAngle *= 1.55f;
                break;
            case 1:
                //upgrade alt fire
                _hud.DisplayMessage("Secondary fire bonus: right click to fire secondary weapon");
                
                if(_weaponController.bulletsPerAltShot == 0)
                {
                    _weaponController.bulletsPerAltShot = 7;
                }
                else
                {
                    _weaponController.bulletsPerAltShot = (int) Math.Round(_weaponController.bulletsPerAltShot * 1.6f);
                }
                break;
            case 2:
                //upgrade chance to spawn missile when after an enemy
                _hud.DisplayMessage("Missile bonus: chance to shoot a missile after hitting an enemy");

                if(_weaponController.chanceToSpawnMissile == 0f)
                {                
                    _weaponController.chanceToSpawnMissile = 0.15f;
                }
                else
                {
                    _weaponController.chanceToSpawnMissile *= 1.15f;
                }
                break;
        }
    }

    public void AddMovementPickup()
    {
        //find random integer between 0 and 3
        int rand = rng.Next(4);
        switch(rand)
        {
            case 0: //upgrade movement speed
                _hud.DisplayMessage("Movement speed bonus");
                _playerCharacterController.maxSpeedOnGround *= 1.1f;
                break;
            case 1: //upgrade bunny hop speed modifier
                _hud.DisplayMessage("Bunny hop bonus: jump repeatedly to gain a speed boost");
                _playerCharacterController.maxJumpSpeedBoost += 0.2f;
                break;
            case 2: //upgrade non shooting speed
                _hud.DisplayMessage("Walking bonus: gain a speed boost when you are not shooting");
                nonShootingSpeedModifier *= 1.15f;
                break;
            case 3: //upgrade air acceleration
                _hud.DisplayMessage("Air acceleration bonus: more control in the air while jumping");
                _playerCharacterController.accelerationSpeedInAir += 20f;
                break;

        }
    }

    public void AddUtilityPickup()
    {
        //find random integer between 0 and 1
        int rand = rng.Next(2);
        switch(rand)
        {
            case 0:
                // UpgradeShield();
                break;
            case 1:
                // UpgradeStageSize();
                break;
        }
    }
}
