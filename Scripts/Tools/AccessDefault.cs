using UnityEngine;
using System.Collections;

public class AccessDefault : MonoBehaviour {
    public GUISkin defaultSkin;
    void OnGUI()
    {
        defaultSkin = GUI.skin;
    }
}
