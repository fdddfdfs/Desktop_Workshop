using System;

public sealed class DataIntArray
{
    private readonly string _key;

    private int[] _value;

    public event Action<int[]> OnValueChanged;

    public int[] Value
    {
        get => _value;
        set
        {
            _value = value;
            SaveArray();
            OnValueChanged?.Invoke(value);
        }
    }

    public DataIntArray(string key)
    {
        _key = key;
        LoadValue();
    }

    public void IncreaseElement(int index, int delta)
    {
        _value[index] += delta;
        SaveArray();
        OnValueChanged?.Invoke(_value);
    }

    public void ResetValue()
    {
        for (var i = 0; i < _value.Length; i++)
        {
            _value[i] = 0;
        }

        Value = _value;
    }

    private void SaveArray()
    {
        Prefs.SaveArray(_value, _key);
    }

    private void LoadValue()
    {
        _value = Prefs.LoadArray(_key);
    }
}