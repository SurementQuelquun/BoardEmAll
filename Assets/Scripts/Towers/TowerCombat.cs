using UnityEngine;

public class TowerCombat : MonoBehaviour
{
    [Header("Tower Stats")]
    public float damage = 10f;
    public float range = 5f;
    public float fireRate = 1f;
    public GameObject projectilePrefab;

    public bool isPlaced = false;

    private float fireCountdown = 0f;

    private void Update()
    {
        if (!isPlaced) return;
        
        UpdateShooting();
    }

    void UpdateShooting()
    {
        if (fireCountdown > 0f)
        {
            fireCountdown -= Time.deltaTime;
        }

        // Find closest monster
        GameObject[] monsters = GameObject.FindGameObjectsWithTag("Monster");
        GameObject target = null;
        float shortestDistance = Mathf.Infinity;

        foreach (GameObject monster in monsters)
        {
            float distance = Vector3.Distance(transform.position, monster.transform.position);
            if (distance < shortestDistance && distance <= range)
            {
                shortestDistance = distance;
                target = monster;
            }
        }

        if (target != null && fireCountdown <= 0f)
        {
            Shoot(target);
            fireCountdown = 1f / fireRate;
        }
    }

    protected virtual void Shoot(GameObject target)
    {
        if (projectilePrefab == null || target == null) return;

        // Instantiate projectile
        GameObject projectileGO = Instantiate(projectilePrefab, transform.position + Vector3.up * 0.5f, Quaternion.identity);
        
        TowerSFX sfx = GetComponent<TowerSFX>();
        if (sfx != null)
        {
            sfx.PlayShoot();
        }
        // Configure projectile
        Projectile proj = projectileGO.GetComponent<Projectile>();
        if (proj != null)
        {
            proj.SetTarget(target, damage);
        }
    }

    // Optional: Draw range in editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}