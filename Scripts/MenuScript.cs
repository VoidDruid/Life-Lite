using UnityEngine;
using UnityEngine.Advertisements;
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

    void Awake()
    {
        Advertisement.Initialize("1578434", true);
    }

	// Use this for initialization
	void Start () {
        
        Screen.orientation = ScreenOrientation.LandscapeLeft;
        Debug.Log("MENU START");
        menufield = this.GetComponent<FieldScript>();
        menufield.Initialize(xleng, zleng);
        menufield.SetTurnSpeed(TurnTm);
        menufield.CreateRandomField();
        menufield.mainMenu = true;
		InGame = false;
		rBoxS = Screen.width * BoxS;
		rIndent = Screen.width * Indent;
		rExitS = Screen.width * ExitS;
        this.transform.position = new Vector3(xleng / 2, 7, 3);
        GUICalc();

        ShowAd();
	}

    public void ShowAd()
    {
        if (Advertisement.IsReady())
        {
            Debug.Log("ad");
            Advertisement.Show();
        }
    }

    public int blackind = 1;
    public float fieldSettingsW;
    public float fieldSettigsH;
    public GUIContent xInputC, yInputC, typeSelecterC, confirmC, backC;
    private float fieldSettingsRealW, fieldSettigsRealH;
    private float fieldSettingsElemRealW, fieldSettingsElemRealH;
    float halfElemRealWL, halfElemRealWR;
    float halfElemRightPos;
    private Rect fieldMenuBoxR, fieldXInR, fieldZInR, fieldTypeSelectR, fieldConfirmR, fieldBackR;
    private Rect typeSelectEmptR, typeSelectBlackR, typeSelectRandR, typeSelecterBoxR;
    const string typeWhite = "Type: white", typeBlack = "Type: black", typeRandom = "Type: random";
    void GUICalc()
    {
        fieldSettingsRealW = Screen.width * fieldSettingsW + blackind * 2;
        fieldSettigsRealH = Screen.height * fieldSettigsH + blackind * 4;
        fieldMenuBoxR = new Rect((Screen.width - fieldSettingsRealW) / 2, (Screen.height - fieldSettigsRealH) / 2, fieldSettingsRealW, fieldSettigsRealH);
        fieldSettingsElemRealH = (fieldSettigsRealH - blackind * 4) / 3;
        fieldSettingsElemRealW = (fieldSettingsRealW - blackind * 2);
        //fieldMenuBoxR, fieldXInR, fieldZInR, fieldTypeSelectR, fieldConfirmR
        halfElemRealWL = fieldSettingsElemRealW / 2 - blackind;
        halfElemRealWR = fieldSettingsElemRealW / 2;
        fieldXInR = new Rect(fieldMenuBoxR.x + blackind, fieldMenuBoxR.y + blackind, halfElemRealWL, fieldSettingsElemRealH);
        halfElemRightPos = fieldXInR.x + fieldXInR.width + blackind;
        fieldZInR = new Rect(halfElemRightPos, fieldMenuBoxR.y + blackind, halfElemRealWR, fieldSettingsElemRealH);
        fieldTypeSelectR = new Rect(fieldMenuBoxR.x + blackind, fieldXInR.y + fieldXInR.height + blackind, fieldSettingsElemRealW, fieldSettingsElemRealH);
        fieldConfirmR = new Rect(fieldMenuBoxR.x + blackind, fieldTypeSelectR.y + fieldTypeSelectR.height + blackind, halfElemRealWL, fieldSettingsElemRealH);
        fieldBackR = new Rect(halfElemRightPos, fieldTypeSelectR.y + fieldTypeSelectR.height + blackind, halfElemRealWR, fieldSettingsElemRealH);
        typeSelectEmptR = new Rect(fieldMenuBoxR.x + fieldMenuBoxR.width, fieldTypeSelectR.y, halfElemRealWR, fieldSettingsElemRealH);
        typeSelectBlackR = new Rect(typeSelectEmptR.x, fieldConfirmR.y, typeSelectEmptR.width, fieldSettingsElemRealH);
        typeSelectRandR = new Rect(typeSelectEmptR.x, typeSelectBlackR.y+blackind+typeSelectBlackR.height, typeSelectEmptR.width, typeSelectEmptR.height);
        typeSelecterBoxR = new Rect(typeSelectEmptR.x - blackind, fieldTypeSelectR.y - blackind, typeSelectEmptR.width + 2 * blackind, typeSelectEmptR.height * 3 + blackind * 4);
        xInput = new IntInputString(20, 60, 300, fieldXInR);
        zInput = new IntInputString(20, 60, 300, fieldZInR);
    }
    
    void Continue()
    {
        Debug.Log("Continue");
        menufield.DestroyField();
        Quaternion camrot = Quaternion.identity;
        camrot.eulerAngles = StartCameraRot;
        GameCamera = Instantiate(GameCameraPref, new Vector3(0, 0, 0), camrot) as GameObject;
        GameCamera.GetComponent<GameManagerClassic>().Woke("false");
        InGame = true;
        Loader.Load(0, ref GameManagerClassic.gamefield);
        Destroy(this.gameObject);
    }

	void FRestart(string fType) {
        Debug.Log("FRestart");
        menufield.DestroyField();
        Quaternion camrot = Quaternion.identity;
		camrot.eulerAngles = StartCameraRot;
		GameCamera = Instantiate (GameCameraPref, new Vector3 (0,0,0), camrot) as GameObject;
        GameCamera.GetComponent<GameManagerClassic>().xleng =xleng;
        GameCamera.GetComponent<GameManagerClassic>().zleng = zleng;
        switch (fType)
        {
            case typeWhite:
                GameCamera.GetComponent<GameManagerClassic>().Woke("false");
                break;
            case typeBlack:
                GameCamera.GetComponent<GameManagerClassic>().Woke("true");
                break;
            case typeRandom:
                GameCamera.GetComponent<GameManagerClassic>().Woke("random");
                break;
            default:
                GameCamera.GetComponent<GameManagerClassic>().Woke("false");
                break;
        }
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

    IntInputString xInput;
    IntInputString zInput;
    bool fieldMenu = false;
    bool generalMenu = true;
    bool typeSelector = false;
    bool InGame, Exiting;
    string typeSelectT = "Select type";
    void OnGUI() {
        if (!InGame)
        {
            GUI.skin.settings.cursorColor = MainSkin.settings.cursorColor;
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
                if (!typeSelector)
                {
                    xInput.TextField(ref xleng,MainSkin.textField);
                    zInput.TextField(ref zleng, MainSkin.textField);
                }
                else
                {
                    GUI.Box(fieldXInR, xInput.GetCont(), MainSkin.textField);
                    GUI.Box(fieldZInR, zInput.GetCont(), MainSkin.textField);
                }
                if (GUI.Button(fieldTypeSelectR, typeSelecterC, MainSkin.customStyles[1]))
                {
                    typeSelector = true;
                }
                if (GUI.Button(fieldConfirmR, confirmC, MainSkin.customStyles[1]))
                {
                    FRestart(typeSelecterC.text);
                }
                if (GUI.Button(fieldBackR, backC, MainSkin.customStyles[1]))
                {
                    typeSelector = false;
                    typeSelecterC.text = typeSelectT;
                    generalMenu = true;
                    fieldMenu = false;
                    xInput.Reset();
                    zInput.Reset();
                }
                if (typeSelector)
                {
                    GUI.Box(typeSelecterBoxR, " ", MainSkin.customStyles[0]);
                    if (GUI.Button(typeSelectEmptR, "Empty", MainSkin.customStyles[1]))
                    {
                        typeSelecterC.text = "Type: empty";
                        typeSelector = false;
                    }
                    if (GUI.Button(typeSelectBlackR, "Black", MainSkin.customStyles[1]))
                    {
                        typeSelecterC.text = "Type: black";
                        typeSelector = false;
                    }
                    if (GUI.Button(typeSelectRandR, "Random", MainSkin.customStyles[1]))
                    {
                        typeSelecterC.text = "Type: random";
                        typeSelector = false;
                    }
                }
            }
        }
        if (Exiting)
        {
            Application.Quit();
        }
	}
}
