using UnityEngine;

public class SeaUrchin : TowerCombat
{
    void Start()
    {
        damage = 10f;
        range = 5f;
        fireRate = 1f;
        cost = 10;
    }

    [System.Obsolete]
    protected override void Shoot(GameObject target)
    {
        Monster[] monsters = FindObjectsOfType<Monster>();

        foreach (Monster m in monsters)
        {
            float distance = Mathf.Abs(m.transform.position.x - transform.position.x)
                           + Mathf.Abs(m.transform.position.z - transform.position.z);

            if (distance <= range)
            {
                m.TakeDamage(damage);
            }
        }
    }
}

