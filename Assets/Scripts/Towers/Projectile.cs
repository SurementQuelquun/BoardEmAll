using UnityEngine;

public class Projectile : MonoBehaviour
{
    private GameObject target;
    private float damage;
    public float speed = 10f;

    public void SetTarget(GameObject t, float d)
    {
        target = t;
        damage = d;
    }

    private void Update()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        Vector3 dir = (target.transform.position - transform.position).normalized;
        transform.position += dir * speed * Time.deltaTime;

        if (Vector3.Distance(transform.position, target.transform.position) < 0.2f)
        {
            HitTarget();
        }
    }

    void HitTarget()
    {
        // Inflige les dégâts
        Health h = target.GetComponent<Health>();
        if (h != null)
        {
            h.TakeDamage(damage);
        }
        Destroy(gameObject);
    }
}

