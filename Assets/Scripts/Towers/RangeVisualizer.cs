using UnityEngine;


[RequireComponent(typeof(LineRenderer))]
public class RangeVisualizer : MonoBehaviour
{
    private LineRenderer _lr;
    private int _segments = 64;
    public void Initialize(float radius, Color color, float width = 0.05f)
    {
        if (_lr == null) _lr = GetComponent<LineRenderer>();
        // Configure LineRenderer
        _lr.loop = true;
        _lr.useWorldSpace = false; // keep circle local to the transform (follows the ghost)
        _lr.positionCount = _segments;
        _lr.widthMultiplier = Mathf.Max(0.001f, width);

        // Material: simple unlit color
        if (_lr.material == null)
        {
            var mat = new Material(Shader.Find("Unlit/Color"));
            mat.hideFlags = HideFlags.DontSave;
            _lr.material = mat;
        }
        _lr.material.SetColor("_Color", color);
        _lr.startColor = color;
        _lr.endColor = color;

        // compute points (y = small offset so it renders above ground)
        float y = 0.01f;
        for (int i = 0; i < _segments; i++)
        {
            float angle = (float)i / _segments * Mathf.PI * 2f;
            float x = Mathf.Cos(angle) * radius;
            float z = Mathf.Sin(angle) * radius;
            _lr.SetPosition(i, new Vector3(x, y, z));
        }
    }

    private void OnValidate()
    {
        // If running in editor and component exists on a ghost with a LineRenderer,
        // try to refresh visual loop for quick preview (best-effort).
        if (_lr == null) _lr = GetComponent<LineRenderer>();
        if (_lr != null && _lr.positionCount == _segments) return;
    }
}
