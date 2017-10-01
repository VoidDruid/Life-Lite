using UnityEngine;
using System.Collections;

public class Loader {
    private static int saves = 1;
    private static int lengthx, lengthz;
    private Loader() { }

    public static Structers.Pair<int,int> GetSizeAt(int savenum)
    {
        if (!PlayerPrefs.HasKey(savenum + ".cstatsr"))
            return new Structers.Pair<int, int>(0, 0);
        else
        {
            return new Structers.Pair<int, int>(PlayerPrefs.GetInt(savenum + ".x"), PlayerPrefs.GetInt(savenum + ".z"));
        }
    }

    public static bool Save(int savenum, ref FieldScript field)
    {
        lengthx = field.GetSizeForSave().first;
        lengthz = field.GetSizeForSave().second;
        if (field == null)
        {
            Debug.Log("Save fail");
            return false;
        }
        else
        {
            bool[] CellStatsRS;
            CellStatsRS = new bool[lengthx * lengthz];

            for (int i = 0; i < lengthx; i++)
                for (int z = 0; z < lengthz; z++)
                {
                    CellStatsRS[i * lengthz + z] = field.CellStatsR[i, z];
                }
            PlayerPrefsX.SetBoolArray(savenum + ".cstatsr", CellStatsRS);
            PlayerPrefs.Save();
            PlayerPrefs.SetInt(savenum + ".x", lengthx);
            PlayerPrefs.SetInt(savenum + ".z", lengthz);
            Debug.Log("saved on " + savenum + " with: " + lengthx + ", " + lengthz);
            return true;
        }
    }

    public static bool Load(int savenum, ref FieldScript field)
    {
        if (PlayerPrefs.HasKey(savenum + ".cstatsr") && field != false)
        {
            lengthx = PlayerPrefs.GetInt(savenum + ".x");
            lengthz = PlayerPrefs.GetInt(savenum + ".z");
            field.SetSize(lengthx-2, lengthz-2);
            bool[] CellStatsRS;
            CellStatsRS = new bool[lengthx * lengthz];

            CellStatsRS = PlayerPrefsX.GetBoolArray(savenum + ".cstatsr");
            for (int i = 0; i < lengthx; i++)
                for (int z = 0; z < lengthz; z++)
                {
                    field.CellStatsR[i, z] = CellStatsRS[i * lengthz + z];
                }

            for (int i = 1; i < lengthx - 1; i++)
                for (int z = 1; z < lengthz - 1; z++)
                {
                    if (field.CellStatsR[i, z])
                        field.Cells[i, z].transform.rotation = field.AliveR;
                    if (!field.CellStatsR[i, z])
                        field.Cells[i, z].transform.rotation = field.DeadR;
                }
            field.SetAllModefied();
            Debug.Log("loaded on " + savenum + " with: " + lengthx + ", " + lengthz);
            return true;
        }
        else
        {
            Debug.Log("Load fail");
            return false;
        }
    }

}
