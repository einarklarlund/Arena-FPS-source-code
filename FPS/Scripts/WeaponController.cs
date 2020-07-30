using UnityEngine;
using UnityEngine.Events;

public enum WeaponShootType
{
    Manual,
    Automatic,
    Charge,
}

[System.Serializable]
public struct CrosshairData
{
    [Tooltip("The image that will be used for this weapon's crosshair")]
    public Sprite crosshairSprite;
    [Tooltip("The size of the crosshair image")]
    public int crosshairSize;
    [Tooltip("The color of the crosshair image")]
    public Color crosshairColor;
}

[RequireComponent(typeof(AudioSource))]
public class WeaponController : MonoBehaviour
{
    [Header("Information")]
    [Tooltip("The name that will be displayed in the UI for this weapon")]
    public string weaponName;
    [Tooltip("The image that will be displayed in the UI for this weapon")]
    public Sprite weaponIcon;

    [Tooltip("Default data for the crosshair")]
    public CrosshairData crosshairDataDefault;
    [Tooltip("Data for the crosshair when targeting an enemy")]
    public CrosshairData crosshairDataTargetInSight;

    [Header("Internal References")]
    [Tooltip("The root object for the weapon, this is what will be deactivated when the weapon isn't active")]
    public GameObject weaponRoot;
    [Tooltip("Tip of the weapon, where the projectiles are shot")]
    public Transform weaponMuzzle;

    [Header("Shoot Parameters")]
    [Tooltip("The type of weapon wil affect how it shoots")]
    public WeaponShootType shootType;
    [Tooltip("Shoot type of alt fire")]
    public WeaponShootType altShootType;
    [Tooltip("The projectile prefab")]
    public ProjectileBase projectilePrefab;
    [Tooltip("Alt fire projectile prefab")]
    public ProjectileBase altProjectilePrefab;
    [Tooltip("Minimum duration between two shots")]
    public float delayBetweenShots = 0.5f;
    [Tooltip("Minimum duration between two alt fire shots")]
    public float delayBetweenAltShots = 7f;
    [Tooltip("Minimum duration between launching alt fire bullets")]
    public float delayBetweenAltLaunching = 0.005f;
    [Tooltip("Angle for the cone in which the bullets will be shot randomly (0 means no spread at all)")]
    public float bulletSpreadAngle = 0f;
    [Tooltip("Angle for the cone in which alt fire bullets will be shot randomly (0 means no spread at all)")]
    public float altBulletSpreadAngle = 100f;
    [Tooltip("Amount of bullets per shot")]
    public int bulletsPerShot = 1;
    [Tooltip("Amount of bullets per alt fire shot")]
    public int bulletsPerAltShot = 10;
    [Tooltip("Force that will push back the weapon after each shot")]
    [Range(0f, 2f)]
    public float recoilForce = 1;
    [Tooltip("Ratio of the default FOV that this weapon applies while aiming")]
    [Range(0f, 1f)]
    public float aimZoomRatio = 1f;
    [Tooltip("Translation to apply to weapon arm when aiming with this weapon")]
    public Vector3 aimOffset;
    [Tooltip("Chance to spawn a missile on hit")]
    public float chanceToSpawnMissile = 0f;

    [Header("Ammo Parameters")]
    [Tooltip("Amount of ammo reloaded per second")]
    public float ammoReloadRate = 1f;
    [Tooltip("Delay after the last shot before starting to reload")]
    public float ammoReloadDelay = 2f;
    [Tooltip("Maximum amount of ammo in the gun")]
    public float maxAmmo = 8;

    [Header("Charging parameters (charging weapons only)")]
    [Tooltip("Trigger a shot when maximum charge is reached")]
    public bool automaticReleaseOnCharged;
    [Tooltip("Duration to reach maximum charge")]
    public float maxChargeDuration = 2f;
    [Tooltip("Initial ammo used when starting to charge")]
    public float ammoUsedOnStartCharge = 1f;
    [Tooltip("Additional ammo used when charge reaches its maximum")]
    public float ammoUsageRateWhileCharging = 1f;

    [Header("Audio & Visual")]
    [Tooltip("Optional weapon animator for OnShoot animations")]
    public Animator weaponAnimator;
    [Tooltip("Prefab of the muzzle flash")]
    public GameObject muzzleFlashPrefab;
    [Tooltip("Unparent the muzzle flash instance on spawn")]
    public bool unparentMuzzleFlash;
    [Tooltip("sound played when shooting")]
    public AudioClip shootSFX;
    [Tooltip("sound played when shooting missile")]
    public AudioClip missileSFX;
    [Tooltip("Sound played when changing to this weapon")]
    public AudioClip changeWeaponSFX;
    [Tooltip("Lightning particle system that appears under hand weapon")]
    [SerializeField] private ParticleSystem m_LightningVFX = null;

    public UnityAction onShoot;

    int m_AltShotsToFire;
    float m_CurrentAmmo;
    float m_LastTimeShot = Mathf.NegativeInfinity;
    float m_LastTimeAltShot = Mathf.NegativeInfinity;
    float m_LastTimeAltLaunched = Mathf.NegativeInfinity;
    float m_TimeBeginCharge;
    Vector3 m_LastMuzzlePosition;
    

    public GameObject owner { get; set; }
    public GameObject sourcePrefab { get; set; }
    public bool isCharging { get; private set; }
    public float currentAmmoRatio { get; private set; }
    public bool isWeaponActive { get; private set; }
    public bool isCooling { get; private set; }
    public float currentCharge { get; private set; }
    public Vector3 muzzleWorldVelocity { get; private set; }
    public float GetAmmoNeededToShoot() => (shootType != WeaponShootType.Charge ? 1 : ammoUsedOnStartCharge) / maxAmmo;

    AudioSource m_ShootAudioSource;

    const string k_AnimAttackParameter = "Attack";

    void Awake()
    {
        m_CurrentAmmo = maxAmmo;
        m_LastMuzzlePosition = weaponMuzzle.position;

        m_ShootAudioSource = GetComponent<AudioSource>();
        DebugUtility.HandleErrorIfNullGetComponent<AudioSource, WeaponController>(m_ShootAudioSource, this, gameObject);

        if(m_LightningVFX)
        {
            ParticleSystem.EmissionModule emission = m_LightningVFX.emission;
            emission.enabled = false;
        }
    }

    void Update()
    {
        UpdateAmmo();

        UpdateCharge();

        if (Time.deltaTime > 0)
        {
            muzzleWorldVelocity = (weaponMuzzle.position - m_LastMuzzlePosition) / Time.deltaTime;
            m_LastMuzzlePosition = weaponMuzzle.position;
        }

        if(m_AltShotsToFire > 0 && Time.time - m_LastTimeAltLaunched >= delayBetweenAltLaunching)
        {
            Vector3 shotDirection = GetShotDirectionWithinSpread(weaponMuzzle, false);
            ProjectileBase newProjectile = Instantiate(altProjectilePrefab, m_LastMuzzlePosition , Quaternion.LookRotation(shotDirection));
            newProjectile.Shoot(gameObject);
            m_AltShotsToFire--;
            m_LastTimeAltLaunched = Time.time;
        }
    }

    void UpdateAmmo()
    {
        if (m_LastTimeShot + ammoReloadDelay < Time.time && m_CurrentAmmo < maxAmmo && !isCharging)
        {
            // reloads weapon over time
            m_CurrentAmmo += ammoReloadRate * Time.deltaTime;

            // limits ammo to max value
            m_CurrentAmmo = Mathf.Clamp(m_CurrentAmmo, 0, maxAmmo);

            isCooling = true;
        }
        else
        {
            isCooling = false;
        }

        if (maxAmmo == Mathf.Infinity)
        {
            currentAmmoRatio = 1f;
        }
        else
        {
            currentAmmoRatio = m_CurrentAmmo / maxAmmo;
        }
    }

    void UpdateCharge()
    {
        if (isCharging)
        {
            if (currentCharge < 1f)
            {
                float chargeLeft = 1f - currentCharge;

                // Calculate how much charge ratio to add this frame
                float chargeAdded = 0f;
                if (maxChargeDuration <= 0f)
                {
                    chargeAdded = chargeLeft;
                }
                else
                {
                    chargeAdded = (1f / maxChargeDuration) * Time.deltaTime;
                }

                chargeAdded = Mathf.Clamp(chargeAdded, 0f, chargeLeft);

                // See if we can actually add this charge
                float ammoThisChargeWouldRequire = chargeAdded * ammoUsageRateWhileCharging;
                if (ammoThisChargeWouldRequire <= m_CurrentAmmo)
                {
                    // Use ammo based on charge added
                    UseAmmo(ammoThisChargeWouldRequire);

                    // set current charge ratio
                    currentCharge = Mathf.Clamp01(currentCharge + chargeAdded);
                }
            }
        }
    }

    public void ShowWeapon(bool show)
    {
        weaponRoot.SetActive(show);

        if (show && changeWeaponSFX)
        {
            m_ShootAudioSource.PlayOneShot(changeWeaponSFX);
        }

        isWeaponActive = show;
    }

    public void UseAmmo(float amount)
    {
        m_CurrentAmmo = Mathf.Clamp(m_CurrentAmmo - amount, 0f, maxAmmo);
        m_LastTimeShot = Time.time;
    }

    public bool HandleShootInputs(bool inputDown, bool inputHeld, bool inputUp)
    {
        switch (shootType)
        {
            case WeaponShootType.Manual:
                if (inputDown)
                {
                    return TryShoot();
                }
                return false;

            case WeaponShootType.Automatic:
                if (inputHeld)
                {
                    return TryShoot();
                }
                return false;

            case WeaponShootType.Charge:
                if (inputHeld)
                {
                    TryBeginCharge();
                }
                // Check if we released charge or if the weapon shoot autmatically when it's fully charged
                if (inputUp || (automaticReleaseOnCharged && currentCharge >= 1f))
                {
                    return TryReleaseCharge();
                }
                return false;

            default:
                return false;
        }
    }

    public bool HandleAltShootInputs(bool inputDown, bool inputHeld, bool inputUp)
    {
        switch(altShootType)
        {
            case WeaponShootType.Automatic:
                if(inputHeld)
                {
                    return TryAltShoot();
                }
                return false;

            default:
                Debug.LogError("[WeaponController] alt fire behavior for shoot type " + altShootType + " is not defined");
                return false;
        }
    }

    bool TryShoot()
    {
        // if (m_CurrentAmmo >= 1f 
        //     && m_LastTimeShot + delayBetweenShots < Time.time)
        if (m_LastTimeShot + delayBetweenShots < Time.time)
        {
            HandleShoot(true);
            // m_CurrentAmmo -= 1;

            return true;
        }

        return false;
    }
    
    bool TryAltShoot()
    {
        // if (m_CurrentAmmo >= 1f 
        //     && m_LastTimeShot + delayBetweenShots < Time.time)
        if (m_LastTimeAltShot + delayBetweenAltShots < Time.time && bulletsPerAltShot > 0)
        {
            HandleShoot(false);
            // m_CurrentAmmo -= 1;
            return true;
        }

        return false;
    }

    bool TryBeginCharge()
    {
        if (!isCharging 
            && m_CurrentAmmo >= ammoUsedOnStartCharge 
            && m_LastTimeShot + delayBetweenShots < Time.time)
        {
            UseAmmo(ammoUsedOnStartCharge); 
            isCharging = true;

            return true;
        }

        return false;
    }

    bool TryReleaseCharge()
    {
        if (isCharging)
        {
            HandleShoot(true);

            currentCharge = 0f;
            isCharging = false;

            return true;
        }
        return false;
    }

    //public method to enable sfx and vfx that playerweaponsmanager will use
    public void SetFX(bool active)
    {
        if(m_LightningVFX)
        {                        
            ParticleSystem.EmissionModule emission = m_LightningVFX.emission;
            emission.enabled = active;
        }
        if(m_ShootAudioSource)
        {
            if(active)
            {
                m_ShootAudioSource.Play();
            }
            else
            {
                m_ShootAudioSource.Stop();
            }
        }
    }

    void HandleShoot(bool primaryFire)
    {
        ProjectileBase projectile;
        int numBullets;

        if(primaryFire)
        {
            projectile = projectilePrefab;
            m_LastTimeShot = Time.time;   
            numBullets = bulletsPerShot; 
            // spawn all bullets with random direction
            for (int i = 0; i < numBullets; i++)
            {
                Vector3 shotDirection = GetShotDirectionWithinSpread(weaponMuzzle, primaryFire);
                ProjectileBase newProjectile = Instantiate(projectile, m_LastMuzzlePosition , Quaternion.LookRotation(shotDirection));
                newProjectile.Shoot(gameObject);
            }
        }
        else
        {
            if(bulletsPerAltShot > 0 && missileSFX)
                m_ShootAudioSource.PlayOneShot(missileSFX);
                
            m_LastTimeAltShot = Time.time;  
            m_AltShotsToFire = bulletsPerAltShot;
        }


        // muzzle flash
        // if (muzzleFlashPrefab != null)
        // {
        //     GameObject muzzleFlashInstance = Instantiate(muzzleFlashPrefab, weaponMuzzle.position, weaponMuzzle.rotation, weaponMuzzle.transform);
        //     // Unparent the muzzleFlashInstance
        //     if (unparentMuzzleFlash)
        //     {
        //         muzzleFlashInstance.transform.SetParent(null);
        //     }

        //     Destroy(muzzleFlashInstance, 2f);
        // }
        // play shoot SFX
        // if (shootSFX)
        // {
        //     m_ShootAudioSource.PlayOneShot(shootSFX);
        // }

        // Trigger attack animation if there is any
        if (weaponAnimator)
        {
            weaponAnimator.SetTrigger(k_AnimAttackParameter);
        }

        // Callback on shoot
        if (onShoot != null)
        {
            onShoot();
        }
    }

    public Vector3 GetShotDirectionWithinSpread(Transform shootTransform, bool primaryFire)
    {
        float spreadAngleRatio;
        if(primaryFire)
        {
            spreadAngleRatio = bulletSpreadAngle / 180f;
            float rand = UnityEngine.Random.value;
            if(rand < 0.3f)
            {
                spreadAngleRatio /= 4f;
            }
            else if(rand < 0.7f)
            {
                spreadAngleRatio /= 2f;
            }
        }
        else
        {
            spreadAngleRatio = altBulletSpreadAngle / 180f;
        }
        
        Vector3 spreadWorldDirection = Vector3.Slerp(shootTransform.forward, UnityEngine.Random.insideUnitSphere, spreadAngleRatio);

        return spreadWorldDirection;
    }
}
