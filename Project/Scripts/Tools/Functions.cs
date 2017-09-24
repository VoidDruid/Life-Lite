using UnityEngine;
//using System.Collections;

//Клсс с различными полезными "подручными" методами 
public class Methods
{
    //Меняет местами значения двух переменных, переданных по ссылке
    public static void Swap<T>(ref T x, ref T y)
    {
        T tmp = x;
        x = y;
        y = tmp;
    }

    //Математически правильное округление float до int
    public static int Round(float x)
    {
        if (Mathf.Abs(x % 1) == 0.5)
            x += 0.01f * ((x % 1) / 0.5f);
        return Mathf.RoundToInt(x);
    }
}
