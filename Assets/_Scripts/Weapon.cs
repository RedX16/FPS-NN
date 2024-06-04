using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Weapon : NetworkBehaviour
{
    [SerializeField] CinemachineVirtualCamera virtualCamera;
    //shooting
    [SerializeField] bool isShooting, readyToShoot;
    bool allowReset = true;
    [SerializeField] float shootingDelay = 2f;

    //Burst
    [SerializeField] int bulletsPerBurst = 3;
    [SerializeField] int currentBurst;

    //Spread
    [SerializeField] int spreadIntensity;
    //Bullet
    [SerializeField] GameObject bulletPrefab;
    [SerializeField] Transform bulletSpawnPoint;
    [SerializeField] int bulletSpeed = 30;
    [SerializeField] int bulletPrefabLifeTime = 3;

    PlayerInput input;

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
        readyToShoot = true;
        currentBurst = bulletsPerBurst;
    }

    // Update is called once per frame
    void Update()
    {
        if(!IsOwner) { return; }
        if(input.Player.PrimaryFire.ReadValue<float>() > 0)
        {
            Shoot();
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
}
