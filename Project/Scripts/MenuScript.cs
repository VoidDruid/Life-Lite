using UnityEngine;
using System.Collections;

public class MenuScript : MonoBehaviour {
	public GUIContent Playc, Restartc, Exitc;
	public float BoxS, Indent, ExitS;
	public GUISkin MainSkin;
    public int xleng, zleng;
    //время анимации поворта клеток
    public float TurnTm;
	public Vector3 StartCameraRot;
	public GameObject  GameCameraPref;
    public float movetime;
	private float rBoxS, rIndent, rExitS;
	private bool InGame, Exiting;
	private GameObject GameCamera;
    FieldScript menufield;
	
	// Use this for initialization
	void Start () {
        Screen.orientation = ScreenOrientation.LandscapeLeft;
        Debug.Log("MENU START");
        menufield = this.GetComponent<FieldScript>();
        menufield.Initialize(xleng, zleng);
        menufield.SetTurnSpeed(TurnTm);
        menufield.CreateRandomField();
		InGame = false;
		rBoxS = Screen.width * BoxS;
		rIndent = Screen.width * Indent;
		rExitS = Screen.width * ExitS;
        this.transform.position = new Vector3(xleng / 2, 7, 3);
	}

    // Update is called once per frame
    float timer;
	void Update () {
        menufield.PCalculations();
        timer += Time.deltaTime;
        if (timer>movetime)
        {
            menufield.Turn();
            timer = 0;
        }
	}
    
    void Continue()
    {
        Debug.Log("Continue");
        menufield.DestroyField();
        Quaternion camrot = Quaternion.identity;
        camrot.eulerAngles = StartCameraRot;
        GameCamera = Instantiate(GameCameraPref, new Vector3(0, 0, 0), camrot) as GameObject;
        InGame = true;
        Loader.Load(0, ref GameManagerClassic.gamefield);
        Destroy(this.gameObject);
    }

	void FRestart() {
        Debug.Log("FRestart");
        menufield.DestroyField();
        Quaternion camrot = Quaternion.identity;
		camrot.eulerAngles = StartCameraRot;
		GameCamera = Instantiate (GameCameraPref, new Vector3 (0,0,0), camrot) as GameObject;
		InGame = true;
		Destroy(this.gameObject);
	}

	void OnGUI() {
        if (!InGame && !Exiting)
        {
            //DEBUG START
            if (GUI.Button(new Rect (0,0,50,50)," Clear prefs"))
            {
                
                PlayerPrefs.DeleteAll();
                Debug.Log("cleared player prefs");
                
            }
            //DEBUG END

            if (GUI.Button(new Rect((Screen.width - rBoxS * 2 - rIndent) / 2, (Screen.height - rBoxS) / 2, rBoxS, rBoxS), Playc, MainSkin.customStyles[2]))
            {
                Continue();
            }
            if (GUI.Button(new Rect((Screen.width - rBoxS * 2 - rIndent) / 2 + rBoxS + rIndent, (Screen.height - rBoxS) / 2, rBoxS, rBoxS), Restartc, MainSkin.customStyles[2]))
            {
                FRestart();
            }
            if (GUI.Button(new Rect(Screen.width - rExitS, Screen.height - rExitS, rExitS, rExitS), Exitc, MainSkin.customStyles[2]))
            {
                Exiting = true;
            }
        }
        if (Exiting)
        {
            Application.Quit();
        }
	}
}
