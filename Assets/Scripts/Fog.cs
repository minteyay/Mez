using UnityEngine;

public class Fog : MonoBehaviour
{
    private const float _epsilon = 0.00001f;

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

    [SerializeField] private uint _steps = 0;
    public uint steps
    {
        get { return _steps; }
        set { _steps = value; StepsChanged(); }
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
            _maxDistance = _minDistance + _epsilon;
        // Max distance must be positive.
        if (_maxDistance < 0.0f)
            _maxDistance = 0.0f;
        Shader.SetGlobalFloat("_FogMaxDistance", _maxDistance);
    }

    private void ColorChanged()
    {
        Shader.SetGlobalColor("_FogColor", _color);
    }

    private void StepsChanged()
    {
        Shader.SetGlobalFloat("_FogSteps", (float)_steps);
    }

    private void OnValidate()
    {
        MinDistanceChanged();
        MaxDistanceChanged();
        ColorChanged();
        StepsChanged();
    }
}
