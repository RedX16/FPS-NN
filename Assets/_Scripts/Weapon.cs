using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class Weapon : NetworkBehaviour
{
    [SerializeField] CinemachineVirtualCamera virtualCamera;
    //shooting
    private bool _isShooting;
    private bool _canShoot;
    private bool _burstFinished = true;
    [SerializeField] float shootCooldown = 0.5f;
    private float _currentCooldown;

    //Burst
    [SerializeField] int bulletsPerBurst = 3;
    int _currentBurst;

    //Spread
    [SerializeField] int spreadIntensity;
    //Bullet
    [SerializeField] GameObject bulletPrefab;
    [SerializeField] Transform bulletSpawnPoint;
    [SerializeField] int bulletSpeed = 30;
    [SerializeField] int bulletPrefabLifeTime = 3;

    PlayerInput input;

    private Coroutine _shootCoroutine;

    public enum ShootingMode
    {
        Single,
        Burst,
        Auto
    }

    public ShootingMode currentShootingMode;

    // Start is called before the first frame update
    void Awake()
    {
        input = new PlayerInput();
        input.Player.Enable();
        _canShoot = true;
        _currentBurst = bulletsPerBurst;
        input.Player.PrimaryFire.started += StartShooting;
        input.Player.PrimaryFire.canceled += StopShooting;
        input.Player.SecondaryFire.started += CycleFireMode;
    }

    private void CycleFireMode(InputAction.CallbackContext obj)
    {
        currentShootingMode = currentShootingMode switch
        {
            ShootingMode.Auto => ShootingMode.Burst,
            ShootingMode.Burst => ShootingMode.Single,
            ShootingMode.Single => ShootingMode.Auto,
            _ => currentShootingMode
        };
    }

    private void StopShooting(InputAction.CallbackContext obj)
    {
        _currentBurst = 0;

        if(_shootCoroutine != null)
            StopCoroutine(_shootCoroutine);
    }

    private void StartShooting(InputAction.CallbackContext obj)
    {
        switch (currentShootingMode)
        {
            case ShootingMode.Single:
                Shoot();
                break;
            case ShootingMode.Burst:
                _shootCoroutine = StartCoroutine(ShootBurst());
                break;
            case ShootingMode.Auto:
                _shootCoroutine = StartCoroutine(ShootAuto());
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private IEnumerator ShootBurst()
    {
        while (true)
        {
            if (_currentBurst >= bulletsPerBurst)
            {
                StopShooting(new InputAction.CallbackContext());
            }
            else if (_canShoot)
            {
                Shoot();
                _currentBurst++;
            }
            yield return null;
        }
        yield return null;
        // ReSharper disable once IteratorNeverReturns
    }

    private IEnumerator ShootAuto()
    {
        while (true)
        {
            if (_canShoot)
            {
                Shoot();
            }
            yield return null;
        }
        yield return null;
        // ReSharper disable once IteratorNeverReturns
    }



    // Update is called once per frame
    void Update()
    {
        if(!IsOwner) { return; }

        if (!_canShoot && _currentCooldown < shootCooldown)
        {
            _currentCooldown += Time.deltaTime;

            if (_currentCooldown >= shootCooldown)
            {
                _canShoot = true;
            }
        }
    }

    void Shoot()
    {
        GameObject bullet = Instantiate(bulletPrefab, bulletSpawnPoint.position, Quaternion.identity);

        bullet.GetComponent<Rigidbody>().AddForce(bulletSpawnPoint.forward.normalized * bulletSpeed);
        StartCoroutine(DestroyBulletAfterTime(bullet, bulletPrefabLifeTime));
    }

    private IEnumerator DestroyBulletAfterTime(GameObject bullet, int delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(bullet);
    }

    private void DetermineReadyToShoot()
    {

    }
}