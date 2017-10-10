using UnityEngine;
//using System.Collections;

//Класс с различными удобными типами данных
public class Structers
{
    //Пара из двух типов данных
    public struct Pair<T1, T2>
    {
        public T1 first;
        public T2 second;
        public Pair(T1 x, T2 y)
        {
            first = x;
            second = y;
        }
    }
}

public class IntInputString : MonoBehaviour 
{
    private string _field;
    private int _value;
    private int _defaultValue;
    private int _minValue;
    private int _maxValue;
    private Rect _rect;
    private bool active;
    //Minimal, default, maximum, position
    public IntInputString(int minV_, int defaultV_, int maxV_, Rect rect_)
    {
        _defaultValue = defaultV_;
        _minValue = minV_;
        _maxValue = maxV_;
        _value = _defaultValue;
        _field = _value.ToString();
        _rect = rect_;
        active = false;
    }
    //Minimal, maximum, position
    public IntInputString(int minV_, int maxV_, Rect rect_)
    {
        _defaultValue = minV_;
        _minValue = minV_;
        _maxValue = maxV_;
        _value = _defaultValue;
        _field = _value.ToString();
        _rect = rect_;
        active = false;
    }
    //Default, position
    public IntInputString(int defaultV_, Rect rect_)
    {
        _defaultValue = defaultV_;
        _minValue = int.MinValue;
        _maxValue = int.MaxValue;
        _value = _defaultValue;
        _field = _value.ToString();
        _rect = rect_;
        active = false;
    }
    private string field
    {
        get { return _field;}
        set
        {
            active = true;
            _field = value;
            if (field.Length != 0)
                if (!(_field[0] == '-' || char.IsNumber(_field[0])))
                    _field = field.Substring(1);
            for (int i = 1; i < _field.Length; i++)
                if (!char.IsNumber(_field[i]))
                    _field = _field.Remove(i, 1);
            if (_field.Length > 1 && _field[0] == '0')
                _field = field.Substring(1);
            else if (_field.Length == 0)
                _field = "0";
        }
    }
    private void CheckVal()
    {
        if (!(_field.Length == 1 && field[0] == '-'))
        {
            if (_field.Length == 1 && field[0] == '0')
            {
                _value = _minValue;
            }
            else
            {
                _value = int.Parse(_field);
                if (_value < _minValue)
                {
                    _value = _minValue;
                }
                else if (_value > _maxValue)
                {
                    _value = _maxValue;
                }
            }
        }
    }
    public void TextField(ref int setVar, GUIStyle style)
    {
        field = GUI.TextField(_rect, field, style);
        CheckVal();
        if (active && Input.GetMouseButtonUp(0) && !_rect.Contains(Input.mousePosition))
        {
            active = false;
            _field = _value.ToString();
        }
        setVar = _value;
    }
    public string GetCont()
    {
        return field;
    }
    public void Reset()
    {
        active = false;
        _value = _defaultValue;
        _field = _value.ToString();
    }
}