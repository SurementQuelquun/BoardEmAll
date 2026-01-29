using UnityEngine;

public class SeaUrchin : TowerCombat
{
    public GameObject shockwavePrefab;
    

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

        if (shockwavePrefab != null)
        {
            GameObject wave = Instantiate(
                shockwavePrefab,
                transform.position + Vector3.up * 0.1f,
                Quaternion.identity
            );

            Shockwave sw = wave.GetComponent<Shockwave>();
            if (sw != null)
                sw.Init(range);
            
        }
    }
}

