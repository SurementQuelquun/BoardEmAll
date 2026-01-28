using UnityEngine;

public class SeaUrchin : TowerCombat
{
    //[Header("Stats")]
    //[SerializeField] private float defaultDamage = 10f;
    //[SerializeField] private float defaultRange = 5f;
    //[SerializeField] private float defaultFireRate = 1f;
    //[SerializeField] private int defaultCost = 10;
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

