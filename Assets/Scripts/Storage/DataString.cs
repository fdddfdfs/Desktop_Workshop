using System;

public sealed class DataString
{
    private readonly string _key;

    private string _value;
    private string _defaultValue;

    public event Action<string> OnValueChanged;

    public string Value
    {
        get => _value;
        set
        {
            _value = value;
            Prefs.SaveVariable(_value, _key);
            OnValueChanged?.Invoke(value);
        }
    }

    public DataString(string key, string defaultValue)
    {
        _key = key;
        _defaultValue = defaultValue;
        LoadValue(defaultValue);
    }

    public void ResetValue()
    {
        Value = _defaultValue;
    }

    private void LoadValue(string defaultValue)
    {
        _value = Prefs.LoadVariable(_key, defaultValue);
    }
}