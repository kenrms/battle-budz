using UnityEngine;

public class Turret : MonoBehaviour
{
    public GameObject BulletPrefab;
    public Transform BulletSpawnPoint;
    public Transform BulletParent;
    public float BulletSpeed = 2f;
    public float Frequency = 1f;
    public bool IsActive = true;

    private float nextFireTime;

    private void Start()
    {
        nextFireTime = Time.time + Frequency;
    }

    private void Update()
    {
        if (IsActive && Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime += Frequency;
        }
    }

    private void Shoot()
    {
        GameObject bullet = Instantiate(
            original: BulletPrefab,
            position: BulletSpawnPoint.position,
            rotation: BulletSpawnPoint.rotation,
            parent: BulletParent);

        var projectile = bullet.GetComponent<Projectile>();
        projectile.Target = BulletSpawnPoint.position + BulletSpawnPoint.forward * 100f;
        projectile.IsHit = false;
        projectile.Speed = BulletSpeed;
    }
}
