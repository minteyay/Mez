using UnityEngine;

public class Fog : MonoBehaviour
{
    [SerializeField] private float _minDistance = 1.0f;
    public float minDistance
    {
        get { return _minDistance; }
        set { _minDistance = value; MinDistanceChanged(); MaxDistanceChanged(); }
    }

    [SerializeField] private float _maxDistance = 1.0f;
    public float maxDistance
    {
        get { return _maxDistance; }
        set { _maxDistance = value; MaxDistanceChanged(); }
    }

    [SerializeField] private Color _color = new Color();
    public Color color
    {
        get { return _color; }
        set { _color = value; ColorChanged(); }
    }

    private void MinDistanceChanged()
    {
        // Min distance must be positive.
        if (_minDistance < 0.0f)
            _minDistance = 0.0f;
        Shader.SetGlobalFloat("_FogMinDistance", _minDistance);
    }

    private void MaxDistanceChanged()
    {
        // Max distance can't be less than min distance.
        if (_maxDistance < _minDistance)
            _maxDistance = _minDistance;
        // Max distance must be positive.
        if (_maxDistance < 0.0f)
            _maxDistance = 0.0f;
        Shader.SetGlobalFloat("_FogMaxDistance", _maxDistance);
    }

    private void ColorChanged()
    {
        Shader.SetGlobalColor("_FogColor", _color);
    }

    private void OnValidate()
    {
        MinDistanceChanged();
        MaxDistanceChanged();
        ColorChanged();
    }
}
