using UnityEngine;
using System.Collections;
using System.Linq;

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
        GUICalc();
	}

    public int blackind = 1;
    public float fieldSettingsW;
    public float fieldSettigsH;
    public GUIContent xInputC, yInputC, typeSelecterC, confirmC, backC;
    private float fieldSettingsRealW, fieldSettigsRealH;
    private float fieldSettingsElemRealW, fieldSettingsElemRealH;
    float halfElemRealWL, halfElemRealWR;
    float halfElemRightPos;
    private Rect fieldMenuBoxR, fieldXInR, fieldYInR, fieldTypeSelectR, fieldConfirmR, fieldBackR;
    void GUICalc()
    {
        fieldSettingsRealW = Screen.width * fieldSettingsW + blackind * 2;
        fieldSettigsRealH = Screen.height * fieldSettigsH + blackind * 4;
        fieldMenuBoxR = new Rect((Screen.width - fieldSettingsRealW) / 2, (Screen.height - fieldSettigsRealH) / 2, fieldSettingsRealW, fieldSettigsRealH);
        fieldSettingsElemRealH = (fieldSettigsRealH - blackind * 4) / 3;
        fieldSettingsElemRealW = (fieldSettingsRealW - blackind * 2);
        //fieldMenuBoxR, fieldXInR, fieldYInR, fieldTypeSelectR, fieldConfirmR
        halfElemRealWL = fieldSettingsElemRealW / 2 - blackind;
        halfElemRealWR = fieldSettingsElemRealW / 2;
        fieldXInR = new Rect(fieldMenuBoxR.x + blackind, fieldMenuBoxR.y + blackind, halfElemRealWL, fieldSettingsElemRealH);
        halfElemRightPos = fieldXInR.x + fieldXInR.width + blackind;
        fieldYInR = new Rect(halfElemRightPos, fieldMenuBoxR.y + blackind, halfElemRealWR, fieldSettingsElemRealH);
        fieldTypeSelectR = new Rect(fieldMenuBoxR.x + blackind, fieldXInR.y + fieldXInR.height + blackind, fieldSettingsElemRealW, fieldSettingsElemRealH);
        fieldConfirmR = new Rect(fieldMenuBoxR.x + blackind, fieldTypeSelectR.y + fieldTypeSelectR.height + blackind, halfElemRealWL, fieldSettingsElemRealH);
        fieldBackR = new Rect(halfElemRightPos, fieldTypeSelectR.y + fieldTypeSelectR.height + blackind, halfElemRealWR, fieldSettingsElemRealH);
    }
    
    void Continue()
    {
        Debug.Log("Continue");
        menufield.DestroyField();
        Quaternion camrot = Quaternion.identity;
        camrot.eulerAngles = StartCameraRot;
        GameCamera = Instantiate(GameCameraPref, new Vector3(0, 0, 0), camrot) as GameObject;
        GameCamera.GetComponent<GameManagerClassic>().Woke();
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
        GameCamera.GetComponent<GameManagerClassic>().xleng = xleng;
        GameCamera.GetComponent<GameManagerClassic>().zleng = zleng;
        GameCamera.GetComponent<GameManagerClassic>().Woke();
        InGame = true;
		Destroy(this.gameObject);
	}

    
    // Update is called once per frame
    float timer;
    void Update()
    { 
        menufield.PCalculations();
        timer += Time.deltaTime;
        if (timer > movetime)
        {
            menufield.Turn();
            timer = 0;
        }
    }
    string xInput = "30";
    string yInput = "30";
    bool fieldMenu = false;
    bool generalMenu = true;
    bool InGame, Exiting;
    void OnGUI() {
        if (!InGame)
        {
            
            if (generalMenu)
            {
                //DEBUG START
                if (GUI.Button(new Rect(0, 0, 50, 50), " Clear prefs"))
                {
                    PlayerPrefs.DeleteAll();
                    Debug.Log("cleared player prefs");
                }
                //DEBUG END

                if (GUI.Button(new Rect((Screen.width - rBoxS * 2 - rIndent) / 2, (Screen.height - rBoxS) / 2, rBoxS, rBoxS), Playc, MainSkin.customStyles[2]))
                {
                    Continue();
                    generalMenu = false;
                }
                if (GUI.Button(new Rect((Screen.width - rBoxS * 2 - rIndent) / 2 + rBoxS + rIndent, (Screen.height - rBoxS) / 2, rBoxS, rBoxS), Restartc, MainSkin.customStyles[2]))
                {
                    generalMenu = false;
                    fieldMenu = true;
                }
                if (GUI.Button(new Rect(Screen.width - rExitS, Screen.height - rExitS, rExitS, rExitS), Exitc, MainSkin.customStyles[2]))
                {
                    Exiting = true;
                    generalMenu = false;
                }
            }
            if (fieldMenu)
            {
                GUI.Box(fieldMenuBoxR, " ", MainSkin.customStyles[0]);
                xInput = GUI.TextField(fieldXInR, xInput, MainSkin.textField);
                for (int i = 0; i < xInput.Length; i++)
                    if (!char.IsNumber(xInput[i]))
                        xInput = xInput.Remove(i, 1);
                xleng = int.Parse(xInput);
                yInput = GUI.TextField(fieldYInR, yInput, MainSkin.textField);
                for (int i = 0; i < yInput.Length; i++)
                    if (!char.IsNumber(yInput[i]))
                        yInput = yInput.Remove(i, 1);
                zleng = int.Parse(yInput);
                //Debug.Log("lengs: " + xleng + " " + zleng);
                GUI.Box(fieldTypeSelectR, typeSelecterC, MainSkin.customStyles[1]);
                if (GUI.Button(fieldConfirmR, confirmC, MainSkin.customStyles[1]))
                {
                    FRestart();
                }
                if (GUI.Button(fieldBackR, backC, MainSkin.customStyles[1]))
                {
                    generalMenu = true;
                    fieldMenu = false;
                    xInput = "30";
                    yInput = "30";
                }
            }
        }
        if (Exiting)
        {
            Application.Quit();
        }
	}
}
