using UnityEngine;

public class Shockwave : MonoBehaviour
{
    public float duration = 0.4f;

    private float timer = 0f;
    private Vector3 startScale;
    private Vector3 endScale;
    private Material mat;

    public void Init(float range)
    {
        startScale = Vector3.zero;
        endScale = new Vector3(range * 2f, 0.05f, range * 2f);

        transform.localScale = startScale;
        mat = GetComponent<Renderer>().material;
    }

    void Update()
    {
        timer += Time.deltaTime;
        float t = timer / duration;

        transform.localScale = Vector3.Lerp(startScale, endScale, t);

        if (mat != null)
        {
            Color c = mat.color;
            c.a = Mathf.Lerp(0.4f, 0f, t);
            mat.color = c;
        }

        if (t >= 1f)
            Destroy(gameObject);
    }
}

