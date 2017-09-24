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
}
