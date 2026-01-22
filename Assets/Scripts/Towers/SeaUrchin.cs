using UnityEngine;

using UnityEngine;

public class SeaUrchin : Tower
{
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

