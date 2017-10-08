//using UnityEngine;
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
    public class NumberInputString
    {
        private string _field;
        private int _value;
        private int _defaultValue;
        private int _minValue;
        private int _maxValue;

        public NumberInputString(int minV_, int defaultV_, int maxV_)
        {
            _defaultValue = defaultV_;
            _minValue = minV_;
            _maxValue = maxV_;
            _value = _defaultValue;
            _field = _value.ToString();
        }
        public int value
        {
            get { return _value; }
        }
        public string field
        {
            get { return _field; }
            //TODO
            set
            {
                if (value.Length == 0)
                {
                    _field = _minValue.ToString();
                    _value = _minValue;
                }
                else
                {
                    _field = value;
                    for (int i = 0; i < _field.Length; i++)
                        if (!char.IsNumber(_field[i]))
                            _field = _field.Remove(i, 1);
                    if (_field.Length == 0)
                    {
                        _field = _minValue.ToString();
                        _value = _minValue;
                    }
                    else
                    {
                        _value = int.Parse(_field);
                        if (_value < _minValue)
                        {
                            _value = _minValue;
                            _field = _minValue.ToString();
                        }
                        else if (_value > _maxValue)
                        {
                            _value = _maxValue;
                            _field = _maxValue.ToString();
                        }
                    }
                }
            }
        }
        public void Reset()
        {
            _value = _defaultValue;
            _field = _value.ToString();
        }
    }
}
