using UnityEngine;

public class Fog : MonoBehaviour
{
    [SerializeField] private float _distance = 10.0f;
    public float distance
    {
        get { return _distance; }
        set { _distance = value; DistanceChanged(); }
    }

    [SerializeField] private Color _color = new Color();
    public Color color
    {
        get { return _color; }
        set { _color = value; ColorChanged(); }
    }

    private void DistanceChanged()
    {
        if (_distance < 0.0f)
            _distance = 0.0f;
        Shader.SetGlobalFloat("_FogDistance", _distance);
    }

    private void ColorChanged()
    {
        Shader.SetGlobalColor("_FogColor", _color);
    }

    private void OnValidate()
    {
        DistanceChanged();
        ColorChanged();
    }
}
