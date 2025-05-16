using System;

public sealed class DataFloat
{
    private readonly string _key;

    private float _value;
    private float _defaultValue;

    public event Action<float> OnValueChanged;

    public float Value
    {
        get => _value;
        set
        {
            _value = value;
            Prefs.SaveVariable(_value, _key);
            OnValueChanged?.Invoke(value);
        }
    }

    public DataFloat(string key, float defaultValue)
    {
        _key = key;
        _defaultValue = defaultValue;
        LoadValue(defaultValue);
    }

    public void ResetValue()
    {
        Value = _defaultValue;
    }

    private void LoadValue(float defaultValue)
    {
        _value = Prefs.LoadVariable(_key, defaultValue);
    }
}