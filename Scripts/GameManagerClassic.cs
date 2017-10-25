using UnityEngine;
using UnityEngine.Advertisements;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class GameManagerClassic : MonoBehaviour {

    private static GameManagerClassic that;
    public GameObject MenuCamera;
    public GUISkin MainSkin;
    private Vector3 CameraPos = new Vector3(0, 0, 0);
    //высота камеры
    public float camh;
    private Camera cam;
    public int xleng, zleng;
    //время анимации поворта клеток
    public float TurnTm;
    public static FieldScript gamefield;

    //TODO Resoursec.Load / unload
    private int customPatCount = 0;
    const int MAX_CUSTOMS = 100;
    private bool[] customPatsNums;
    string customPatPfxCount = "_count";
    string customPatsBaseKey = "custom_pat";
    string customPatsLanePfx = "_lane_";
    public TextAsset movPatterns;
    public TextAsset periPatterns;
    public TextAsset genPatterns;
    public TextAsset statPatterns;
    public TextAsset customPatterns;

    class Pattern
    {
        public string name;
        public List<List<bool>> def;
        public int rows {get { return def.Count; } }
        public int columns { get { return def[0].Count; } }
        public Pattern()
        {
            name = "";
            def = new List<List<bool>>();
        }
    }

    enum PatternNums : int
    {
        mov = 0,
        peri,
        gen,
        stat
    }

    public GameObject rotate, tick, cross;
    public GameObject emptyGO;
    public GameObject patBlack, patWhite;
    public GameObject areaOutline;
    public float outlineScale;
    public float outlineHeight;
    //Мама, смотри, я сделал синглтон!
    class InstPatt
    {
        private static InstPatt instance;
        Pattern pattern;
        public GameObject pattImg;
        int rotation;
        float widthPatImg, heightPatImg;
        private InstPatt()
        {
        }
        public void Place(Pattern pat, Vector3 position, int rot)
        {
            pattImg = new GameObject();
            Quaternion sprRot = new Quaternion();
            sprRot.eulerAngles = new Vector3(90, 0, 0);
            Quaternion AliveRot = new Quaternion();
            AliveRot.eulerAngles = new Vector3(180, 0, 0);
            widthPatImg = pat.columns;
            heightPatImg = pat.rows;
            float offsetX = 0.5f, offsetZ = 0.5f;
            if (pat.columns % 2 != 0) offsetX = 1;
            if (pat.rows % 2 != 0) offsetZ = 1;
            pattImg.transform.position = new Vector3(pat.columns / 2, 0, pat.rows / 2);
            GameObject patt = Instantiate(that.emptyGO, new Vector3(pat.columns / 2 + offsetX, 0, pat.rows / 2 + offsetZ), Quaternion.identity) as GameObject;
            patt.transform.parent = pattImg.transform;
            Vector3 pos = new Vector3(1, 0, 1);
            for (int i = 0; i < pat.def.Count; i++)
            {
                for (int j = 0; j < pat.def[i].Count; j++)
                {
                    GameObject temp = Instantiate(!pat.def[i][j] ? that.patWhite : that.patBlack, pos, !pat.def[i][j] ? Quaternion.identity : AliveRot) as GameObject;
                    temp.transform.parent = patt.transform;
                    pos.x++;
                }
                pos.z++;
                pos.x = 1;
            }
            pos.x = 1;
            if (pat.columns > 3)
                pos.x += pat.columns % 3;
            pos.z = 0;
            GameObject UI = Instantiate(that.emptyGO, pos, sprRot) as GameObject;
            UI.transform.parent = pattImg.transform;
            GameObject tmp = Instantiate(that.rotate, pos, sprRot) as GameObject;
            tmp.transform.parent = UI.transform;
            pos.x++;
            tmp = Instantiate(that.tick, pos, sprRot) as GameObject;
            tmp.transform.parent = UI.transform;
            pos.x++;
            tmp = Instantiate(that.cross, pos, sprRot) as GameObject;
            tmp.transform.parent = UI.transform;
            pattImg.transform.position = new Vector3(Mathf.RoundToInt(position.x), 0.5f, Mathf.RoundToInt(position.z));
            stdPos = patt.transform.localPosition;
            if ((pat.columns % 2 == 0 && pat.rows % 2 != 0) || (pat.columns % 2 != 0 && pat.rows % 2 == 0))
                biasPos = new Vector3(patt.transform.localPosition.x + 0.5f, patt.transform.localPosition.y, patt.transform.localPosition.z + 0.5f);
            else
                biasPos = stdPos;

            lastCh2 = pattImg.transform.GetChild(0).childCount - 1;
            pattern = pat;
            rotation = rot;
        }

        public void SetPattern()
        {
            int childs = pattImg.transform.GetChild(0).childCount;
            Transform holder = pattImg.transform.GetChild(0);
            int rows = pattern.def.Count;
            int columns = pattern.def[0].Count;
            Debug.Log("cells: " + childs);
            for (int i = 0; i < childs; i++)
            {
                Transform go = holder.GetChild(i);
                int x = Methods.Round(go.position.x);
                int z = Methods.Round(go.position.z);

                int patX = Methods.Round(go.localPosition.x + columns/(float)2)-1;
                int patZ = Methods.Round(go.localPosition.z + rows / (float)2)-1;
                Debug.Log("coor: " + patX + " " + patZ);
                //TODO FIX
                //КОСТЫЛЬ! [patZ][patX] ДОЛЖНО БЫТЬ [patX][patZ], УЗНАТЬ В ЧЕМ ДЕЛО, ИСПРАВИТЬ!
                if (gamefield.CellStatsR[x, z] != pattern.def[patZ][patX])
                {
                    gamefield.FlipCell(x, z);
                    Debug.Log("Fliped at: " + x + " " + z);
                }
            }
            Reset();
        }

        public void Reset()
        {
            Destroy(pattImg);
            pattern = null;
        }
        Vector3 biasPos;
        Vector3 stdPos;
        Vector3 gPosB;
        Vector3 gPosStd;
        int lastCh2;
        public void Rotate()
        {
            //ОГРОМНЫЙ КОСТЫЛЬ
            //TODO
            pattImg.transform.GetChild(0).localPosition = biasPos;
            Methods.Swap(ref biasPos, ref stdPos);
            pattImg.transform.GetChild(0).Rotate(0,90,0);
            Vector3 gPos = new Vector3();
            if (pattImg.transform.GetChild(0).rotation.eulerAngles.y == 90 || pattImg.transform.GetChild(0).rotation.eulerAngles.y == 180)
                gPos = new Vector3(pattImg.transform.GetChild(1).position.x, pattImg.transform.GetChild(1).position.y, pattImg.transform.GetChild(0).GetChild(lastCh2).position.z - 1);
            if (pattImg.transform.GetChild(0).rotation.eulerAngles.y == 0 || pattImg.transform.GetChild(0).rotation.eulerAngles.y == 270)
                gPos = new Vector3(pattImg.transform.GetChild(1).position.x, pattImg.transform.GetChild(1).position.y, pattImg.transform.GetChild(0).GetChild(0).position.z - 1);
            pattImg.transform.GetChild(1).position = gPos;
            Debug.Log("rotated");
        }

        public static InstPatt GetInstance()
        {
            if (instance == null)
                instance = new InstPatt();
            return instance;
        }
    }

    class AreaSelecter
    {
        private static AreaSelecter instance;
        private AreaSelecter() { }
        public Vector3 screenCorner1, screenCorner2;
        private Vector3 corn1, corn2;
        private GameObject bordL, bordR, bordU, bordD;
        private bool done = false;
        public bool isDone() { return done; }

        public static AreaSelecter GetInstance()
        {
            if (instance == null)
                instance = new AreaSelecter();
            return instance;
        }

        //TODO: смещение камеры дальше при приближении к краю экрана, красные рамки вокруг выделенной области клеток (не квадрат гуи)
        public void SetArea()
        {
            if (Input.mousePosition.y < (Screen.height - rGpanH))
            {
                if (Input.GetMouseButtonDown(0))
                {
                    screenCorner1 = Input.mousePosition;
                    if (gamefield.GetGOFromScreen(screenCorner1) != null)
                        corn1 = gamefield.GetGOFromScreen(screenCorner1).transform.position;
                    else
                        corn1 = Vector3.zero;
                    done = false;
                }
                if (!done)
                {
                    screenCorner2 = Input.mousePosition;
                    //Костыль для работы DrawLine
                    if (screenCorner2.x == screenCorner1.x)
                        screenCorner2.x++;
                    if (screenCorner2.y == screenCorner1.y)
                        screenCorner2.y++;
                    if (gamefield.GetGOFromScreen(screenCorner2) != null)
                        corn2 = gamefield.GetGOFromScreen(screenCorner2).transform.position;
                    else
                        corn2 = Vector3.zero;
                }
                if (Input.GetMouseButtonUp(0))
                    done = true;
            }
        }

        public List<Structers.Pair<int, int>> GetAreaGO()
        {
            Debug.Log("scr1: " + screenCorner1);
            Debug.Log("scr2: " + screenCorner2);
            Debug.Log("corn1:" + corn1);
            Debug.Log("corn2:" + corn2);
            if (corn1 == Vector3.zero || corn2 == Vector3.zero)
                return new List<Structers.Pair<int, int>>();
            List<Structers.Pair<int, int>> area = new List<Structers.Pair<int, int>>();
            if (corn1.x > corn2.x) Methods.Swap(ref corn1.x, ref corn2.x);
            if (corn1.z > corn2.z) Methods.Swap(ref corn1.z, ref corn2.z);
            for (int i = (int)corn1.x; i <= (int)corn2.x; i++)
                for (int j = (int)corn1.z; j <= (int)corn2.z; j++)
                {
                    area.Add(new Structers.Pair<int, int>(i, j));
                }
            return area;
        }

        //FIXME
        //баг если вызывать постоянно в OnGUI()
        public Structers.Pair<Structers.Pair<int, int>, Structers.Pair<int, int>> GetAreaCorners()
        {
            var temp1 = corn1;
            var temp2 = corn2;
            if (temp1.x > temp2.x) Methods.Swap(ref temp1.x, ref temp2.x);
            if (temp1.z > temp2.z) Methods.Swap(ref temp1.z, ref temp2.z);
            Debug.Log("corn1: " + (int)temp1.x + "," + (int)temp1.z);
            Debug.Log("corn2: " + (int)temp2.x + "," + (int)temp2.z);
            return new Structers.Pair<Structers.Pair<int, int>, Structers.Pair<int, int>>(new Structers.Pair<int, int>((int)temp1.x,(int)temp1.z), new Structers.Pair<int, int>((int)temp2.x, (int)temp2.z));
        }
        public void Reset()
        {
           screenCorner1 = screenCorner2 = corn1 = corn2 = Vector3.zero;
           bordL = bordR = bordU = bordD = null;
           done = false;
        }
    }

    public void Woke (string state)
    {
        Debug.Log("GAME AWAKE");
        that = GetComponent<GameManagerClassic>();
        gamefield = this.GetComponent<FieldScript>();
        gamefield.Initialize(xleng, zleng);
        cam = gamefield.cam;
        switch (state)
        {
            case "false":
                gamefield.CreateEmptyField(false);
                break;
            case "true":
                gamefield.CreateEmptyField(true);
                break;
            case "random":
                gamefield.CreateRandomField();
                break;
        }
        gamefield.SetTurnSpeed(TurnTm);
        ResetCamera();
    }

    public void ResetCamera()
    {
        camh = 15;
        CameraPos.y = camh;
        CameraPos.x = gamefield.GetSize().first / 2 - 0.5f;
        CameraPos.z = gamefield.GetSize().second / 2 - 0.5f;
        this.transform.position = CameraPos;
    }

    List<Pattern> custPat = new List<Pattern>();
    void LoadCustoms()
    {
        if (PlayerPrefs.HasKey(customPatsBaseKey + customPatPfxCount))
        {
            customPatsNums = PlayerPrefsX.GetBoolArray(customPatsBaseKey + "_nums");
            customPatCount = PlayerPrefs.GetInt(customPatsBaseKey + customPatPfxCount);
            for(int i = 0; i<MAX_CUSTOMS; i++)
                if (customPatsNums[i])
                {
                    Pattern custom = new Pattern();
                    string nameGet = "custom " + i;
                    string name = PlayerPrefs.GetString(customPatsBaseKey + "_name_" + i);
                    custom.name = name;
                    int size = PlayerPrefs.GetInt(customPatsBaseKey + "_" + nameGet);
                    List<List<bool>> def = new List<List<bool>>();
                    for (int j = 0; j < size; j++)
                    {
                        //ОЧЕНЬ КРИВО
                        //хотя не так уж и криво, если подумать. Наглядно зато, top kek
                        //FIXME
                        bool[] tmp = PlayerPrefsX.GetBoolArray(customPatsBaseKey + "_" + nameGet + customPatsLanePfx + j);
                        List<bool> tmpL = new List<bool>();
                        tmpL.AddRange(tmp);
                        def.Add(tmpL);
                    }
                    custom.def = def;
                    custPat.Add(custom);
                }
        }
        else
        {
            customPatsNums = new bool[MAX_CUSTOMS];
            for (int i = 0; i < MAX_CUSTOMS; i++)
                customPatsNums[i] = false;
        }
    }

    List<List<Pattern>> Patterns = new List<List<Pattern>>();
    void Start()
    {
        LoadCustoms();
        List<string> patConts = new List<string>();
        patConts.Add(movPatterns.text);
        patConts.Add(periPatterns.text);
        patConts.Add(genPatterns.text);
        patConts.Add(statPatterns.text);

        PatternNums patternNum = PatternNums.mov;
        for (; (int)patternNum <= (int)PatternNums.stat; patternNum++)
        {
            Patterns.Add(new List<Pattern>());
            for (int i = 0; i < patConts[(int)patternNum].Length; i++)
            {
                if (patConts[(int)patternNum][i] == '.' && i < patConts[(int)patternNum].Length - 1)
                {
                    i++;
                    Pattern pat = new Pattern();
                    string name = "";
                    while (patConts[(int)patternNum][i] != ':' && i < patConts[(int)patternNum].Length)
                    {
                        name += patConts[(int)patternNum][i];
                        i++;
                    }
                    pat.name = name;
                    List<bool> row = new List<bool>();
                    while (patConts[(int)patternNum][i] != ';' && i < patConts[(int)patternNum].Length)
                    {
                        if (patConts[(int)patternNum][i] == '0')
                        {
                            row.Add(false);
                        }
                        if (patConts[(int)patternNum][i] == '1')
                        {
                            row.Add(true);
                        }
                        if (patConts[(int)patternNum][i] == '_')
                        {
                            pat.def.Add(row);
                            row = new List<bool>();
                        }
                        i++;
                    }
                    Patterns[(int)patternNum].Add(pat);
                }
            }
            //DEBUG
            /*for (int i = 0; i < Patterns[(int)patternNum].Count; i++)
            {
                Debug.Log(Patterns[(int)patternNum][i].name);
                string row = "";
                for (int a = 0; a < Patterns[(int)patternNum][i].def.Count; a++)
                {
                    for (int b = 0; b < Patterns[(int)patternNum][i].def[a].Count; b++)
                        row += Patterns[(int)patternNum][i].def[a][b];
                    Debug.Log(row);
                    row = "";
                }
            }*/
        }
        //LoadCustoms();
        ResetCamera();
        CalculateGUI();
        readFromTime = new IntInputString(0, 2, int.MaxValue - 1, timerR);
    }

    //Rectы для всего GUI
    private Rect pauseR, placeR, customR, toolsR, timerR, autoturnR, drawR;
    private Rect pauseMenuR, continueR, exitR, saveR, loadR, recreateFieldR;
    private Rect movPatR, periPatR, genPatR, statPatR;
    private Rect fillWhiteR, fillBlackR, invertR, savePattR;
    private Rect patSaveNameR, patSaveNameBoxR;
    private Rect deletePatR, deletePatConfR, deletePatConfBoxR, deletePatTickR, deletePatTickBoxR, deletePatCrossBoxR, deletePatCrossR;
    private Rect patSaveNameConfR, patSaveNameConfBoxR;
    private Rect drawWhiteR, drawBlackR, drawInvertR;
    private Rect tickR, crossR;
    private Rect endDrawR, endDrawBoxR;
    private Rect SLMenuRect, SLSlotRect, SLBackRect, SLBackButtRect, SLAsureQBox, SLAsureQ, SLConfBox, SLConf;
    

    public GUISkin GameGUI;
    //изображения стрелок "вверх" и "вниз"
    public Texture2D arrup, arrdwn;
    //длина "черного" отступа
    public int blackind;
    //контент для GUI
    public GUIContent pauseC, placeC, customC, toolsC, autoturnC, drawC;
    public GUIContent continueC, exitC, saveC, loadC, recreateFieldC;
    public GUIContent deletePatConfC;
    public GUIContent movPatC, periPatC, genPatC, statPatC;
    public GUIContent fillWhiteC, fillBlackC, invertC, savePattC;
    public GUIContent drawWhiteC, drawBlackC, drawInvertC;
    public GUIContent tickC, crossC;
    public GUIContent SLConfC;
    //какую часть ширинvы экрана занимает каждый элемент GUI
    public float pauseW, placeW, customW, toolsW, timerW, autoturnW;
    private float drawW;
    public float gpanH, placeBoxHW;
    public float pauseMenuW, pauseMenuH;
    public float SaveSlotsSpaceH;
    public float TickCrossAreaSelect;
    const int patMenuNum = 4;
    const int pauseRowsNum = 4;

    //реальная ширина элементов GUI в пикселях
    private static int rGpanH;
    private int rPauseW, rPlaceW, rCustomW, rToolsW, rTimerW, rAutoturnW;
    private int rDrawW;
    private int rPlaceBoxHW;
    private int rPauseMenuW, rPauseMenuH, rPauseMenuElemH;
    private int rSaveLoadW;
    private int rCustomPlaceBoxH, rCustomPlaceBoxW;
    //field restarts
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
    //
    private int rSlotH;
    Dictionary<string, Rect> restraints = new Dictionary<string, Rect>();
    void CalculateGUI()
    {
        drawW = 1 - pauseW - placeW - customW - toolsW - timerW - autoturnW;
        rGpanH = Mathf.RoundToInt(Screen.height * gpanH) - blackind;
        gamefield.rGpanH = rGpanH;
        rPauseW = Mathf.RoundToInt(Screen.width * pauseW) - blackind * 2;
        rPlaceW = Mathf.RoundToInt(Screen.width * placeW) - blackind;
        rCustomW = Mathf.RoundToInt(Screen.width * customW) - blackind;
        rToolsW = Mathf.RoundToInt(Screen.width * toolsW) - blackind;
        rDrawW = Mathf.RoundToInt(Screen.width * drawW) - blackind;
        rPlaceBoxHW = Mathf.RoundToInt(Screen.width * placeBoxHW);

        rPauseMenuW = Mathf.RoundToInt(Screen.width * pauseMenuW) - blackind * 2;
        rPauseMenuH = Mathf.RoundToInt(Screen.height * pauseMenuH) - blackind*(pauseRowsNum + 1);
        rPauseMenuElemH = Mathf.RoundToInt(rPauseMenuH / pauseRowsNum);
        rSaveLoadW = Mathf.RoundToInt(rPauseMenuW / 2 - blackind/2);

        pauseR = new Rect(blackind, 0, rPauseW, rGpanH);
        placeR = new Rect(rPauseW + blackind * 2, 0, rPlaceW, rGpanH);
        customR = new Rect(rPauseW + rPlaceW + blackind * 3, 0, rCustomW, rGpanH);
        toolsR = new Rect(rPauseW + rPlaceW + rCustomW + blackind * 4, 0, rToolsW, rGpanH);
        drawR = new Rect(rPauseW + rPlaceW + rCustomW + rToolsW + blackind * 5, 0, rDrawW, rGpanH);

        movPatR = new Rect(rPauseW + blackind * 2, rGpanH + blackind, rPlaceW, rGpanH);
        periPatR = new Rect(rPauseW + blackind * 2, (rGpanH + blackind)*2, rPlaceW, rGpanH);
        genPatR = new Rect(rPauseW + blackind * 2, (rGpanH + blackind) * 3, rPlaceW, rGpanH);
        statPatR = new Rect(rPauseW + blackind * 2, (rGpanH + blackind) * 4, rPlaceW, rGpanH);

        fillWhiteR = new Rect(rPauseW + rPlaceW + rCustomW + blackind * 4, rGpanH + blackind, rToolsW, rGpanH);
        fillBlackR = new Rect(rPauseW + rPlaceW + rCustomW + blackind * 4, (rGpanH + blackind)*2, rToolsW, rGpanH);
        invertR = new Rect(rPauseW + rPlaceW + rCustomW + blackind * 4, (rGpanH + blackind)*3, rToolsW, rGpanH);
        savePattR = new Rect(rPauseW + rPlaceW + rCustomW + blackind * 4, (rGpanH + blackind)*4, rToolsW, rGpanH);

        drawWhiteR = new Rect(drawR.x, fillWhiteR.y, drawR.width, rGpanH);
        drawBlackR = new Rect(drawR.x, fillBlackR.y, drawR.width, rGpanH);
        drawInvertR = new Rect(drawR.x, invertR.y, drawR.width, rGpanH);

        tickR = new Rect(Screen.width / 2 - Screen.width * TickCrossAreaSelect, 0, Screen.width * TickCrossAreaSelect, rGpanH);
        crossR = new Rect(Screen.width / 2 +blackind, 0, Screen.width * TickCrossAreaSelect, rGpanH);
        endDrawR = tickR;
        endDrawR.height = endDrawR.width;
        endDrawR.x = Screen.width - endDrawR.width-blackind;
        endDrawR.y = Screen.height - endDrawR.height-blackind;
        endDrawBoxR = new Rect(endDrawR.x - blackind, endDrawR.y - blackind, endDrawR.width + blackind * 2, endDrawR.height + blackind * 2);

        int pauseMenuLeft, pauseMenuTop;
        pauseMenuTop = Mathf.RoundToInt((Screen.height - rPauseMenuH) / 2);
        pauseMenuLeft = Mathf.RoundToInt((Screen.width - rPauseMenuW) / 2);
        pauseMenuR = new Rect(pauseMenuLeft - blackind, pauseMenuTop - blackind, rPauseMenuW + blackind * 2, rPauseMenuH + blackind * (pauseRowsNum+1));
        restraints.Add("pauseMenu", pauseMenuR);
        int heightSt = pauseMenuTop;
        continueR = new Rect(pauseMenuLeft, heightSt, rPauseMenuW, rPauseMenuElemH);
        heightSt += rPauseMenuElemH + blackind;
        recreateFieldR = new Rect(pauseMenuLeft, heightSt, rPauseMenuW, rPauseMenuElemH);
        heightSt += rPauseMenuElemH + blackind;
        //костыль с черными отступами в save load
        saveR = new Rect(pauseMenuLeft, heightSt, rPauseMenuW / 2-1, rPauseMenuElemH);
        loadR = new Rect(pauseMenuLeft+ rPauseMenuW / 2+1, heightSt, rPauseMenuW / 2-1, rPauseMenuElemH);
        heightSt += rPauseMenuElemH + blackind;
        exitR = new Rect(pauseMenuLeft, heightSt, rPauseMenuW, rPauseMenuElemH);

        int SLSlotsHeight = (int) (Screen.height*SaveSlotsSpaceH);
        rSlotH = (int)(SLSlotsHeight / saveSlotsNum);

        SLMenuRect = new Rect(Screen.width / 4, (Screen.height - SLSlotsHeight - rSlotH - continueR.height - blackind * (saveSlotsNum + 1)) / 2, pauseMenuR.width, SLSlotsHeight + blackind * (saveSlotsNum-3));
        SLSlotRect = new Rect(SLMenuRect.x + blackind, SLMenuRect.y + blackind, continueR.width, rSlotH);
        restraints.Add("slMenu", SLMenuRect);

        SLBackRect = new Rect(Screen.width / 4, (Screen.height - SLSlotsHeight - rSlotH - continueR.height - blackind * (saveSlotsNum + 3)) / 2 + SLSlotsHeight + rSlotH + blackind * (saveSlotsNum + 1), pauseMenuR.width, continueR.height + blackind);
        SLBackButtRect = new Rect(Screen.width / 4 + blackind, (Screen.height - rSlotH * (saveSlotsNum + 1) - continueR.height - blackind * (saveSlotsNum + 1)) / 2 + rSlotH * (saveSlotsNum + 1) + blackind * (saveSlotsNum + 2), continueR.width, continueR.height);
        restraints.Add("slBack", SLBackRect);

        SLAsureQBox = new Rect(Screen.width / 2, SLMenuRect.y + (SLMenuRect.height - continueR.height - continueR.width / 2 - blackind * 4 - rSlotH), continueR.width + blackind * 2, continueR.height + blackind * 2);
        SLAsureQ = new Rect(SLAsureQBox.x + blackind, SLAsureQBox.y + blackind, SLAsureQBox.width - blackind * 2, SLAsureQBox.height - blackind * 2);
        restraints.Add("slAsure", SLAsureQBox);

        SLConfBox = new Rect(SLAsureQBox.x + continueR.width / 4f - blackind, SLAsureQBox.y + SLAsureQBox.height + rSlotH, continueR.width / 2 + blackind * 2, continueR.width / 2 + blackind * 2);
        SLConf = new Rect(SLConfBox.x + blackind, SLConfBox.y + blackind, SLConfBox.width - blackind * 2, SLConfBox.height - blackind * 2);
        restraints.Add("slConf", SLConfBox);

        rCustomPlaceBoxH = rPlaceBoxHW;
        rCustomPlaceBoxW = rCustomW;

        deletePatR = new Rect(0, 0, rCustomPlaceBoxW/4, rCustomPlaceBoxH-2);
        int widPatname = rPauseMenuW * 2;
        deletePatConfR = new Rect((Screen.width - widPatname) / 2, (Screen.height - rPauseMenuElemH) / 2, widPatname, rPauseMenuElemH);
        deletePatConfBoxR = new Rect((Screen.width - widPatname) / 2 - blackind, (Screen.height - rPauseMenuElemH) / 2 - blackind, widPatname + blackind * 2, rPauseMenuElemH + blackind * 2);
        deletePatTickR = new Rect (deletePatConfR.x + deletePatConfR.width + tickR.width, deletePatConfR.y, tickR.width, rPauseMenuElemH);
        deletePatTickBoxR = new Rect(deletePatConfR.x + deletePatConfR.width + tickR.width - blackind, deletePatConfR.y - blackind, tickR.width + blackind * 2, rPauseMenuElemH + blackind * 2);
        deletePatCrossR = new Rect(deletePatConfR.x - crossR.width*2, deletePatConfR.y, crossR.width, rPauseMenuElemH);
        deletePatCrossBoxR = new Rect(deletePatConfR.x - crossR.width*2-blackind, deletePatConfR.y-blackind, crossR.width+blackind*2, rPauseMenuElemH + blackind * 2);

        rAutoturnW = Mathf.RoundToInt(Screen.width * autoturnW) - blackind;
        autoturnR = new Rect(Screen.width - rAutoturnW - blackind, 0, rAutoturnW, rGpanH);
        rTimerW = Mathf.RoundToInt(Screen.width * timerW) - blackind;
        timerR = new Rect(autoturnR.x - blackind - rTimerW, 0, rTimerW, rGpanH);

        
        patSaveNameR = new Rect((Screen.width - widPatname) / 2, (Screen.height - rPauseMenuElemH) / 2, widPatname, rPauseMenuElemH);
        patSaveNameBoxR = new Rect((Screen.width - widPatname) / 2 - blackind, (Screen.height - rPauseMenuElemH) / 2 - blackind, widPatname + blackind*2, rPauseMenuElemH + blackind * 2);
        patSaveNameConfR = new Rect(patSaveNameR.x + patSaveNameR.width + tickR.width, patSaveNameR.y, tickR.width, rPauseMenuElemH);
        patSaveNameConfBoxR = new Rect(patSaveNameR.x + patSaveNameR.width + tickR.width - blackind, patSaveNameR.y - blackind, tickR.width + blackind*2, rPauseMenuElemH + blackind * 2);

        //ДА, Я ЗНАЮ ЧТО Я ПРОСТО СКОПИРОВАЛ КОД ИЗ MenuScript!
        //это последняя фича во всем проекте, мне лень парить себе мозги
        //отстань от меня
        //зато работает
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
        typeSelectRandR = new Rect(typeSelectEmptR.x, typeSelectBlackR.y + blackind + typeSelectBlackR.height, typeSelectEmptR.width, typeSelectEmptR.height);
        typeSelecterBoxR = new Rect(typeSelectEmptR.x - blackind, fieldTypeSelectR.y - blackind, typeSelectEmptR.width + 2 * blackind, typeSelectEmptR.height * 3 + blackind * 4);
        xInput = new IntInputString(20, 60, 300, fieldXInR);
        zInput = new IntInputString(20, 60, 300, fieldZInR);

        MainSkin.customStyles[1].fontSize = Mathf.RoundToInt(rGpanH / 2.4f);
    }


    GameObject pressedGo;
    bool getoffset = false;
    bool dragpatt = false;
    bool drawGoes = false;
    Rect area = new Rect(0, 0, 0, 0);
    int autoturnTime = 2;
    void Update()
    {
        gamefield.paused = paused;
        gamefield.mouseRestr = false;
        gamefield.autoturn = autoturn;
        gamefield.autoturnTime = autoturnTime;
        gamefield.drawing = drawing;
        if (placing)
        {
            if (Input.GetMouseButtonDown(0))
            {
                pressedGo = gamefield.GetPressedGO();
                if (pressedGo != null)
                {
                    if (pressedGo.name == "Cross(Clone)")
                    {
                        Debug.Log("clicked cross");
                        placing = false;
                        gpan = true;
                        paused = false;
                        InstPatt.GetInstance().Reset();
                    }
                    else
                    {
                        if (pressedGo.name == "Tick(Clone)")
                        {
                            Debug.Log("clicked tick");
                            placing = false;
                            gpan = true;
                            paused = false;
                            InstPatt.GetInstance().SetPattern();
                        }
                        else
                        {
                            if (pressedGo.name == "Rotate(Clone)")
                            {
                                Debug.Log("clicked rotate");
                                InstPatt.GetInstance().Rotate();
                            }
                            else
                            {
                                if (pressedGo.tag == "Pattern")
                                {
                                    gamefield.mouseposcl = Input.mousePosition;
                                    getoffset = true;
                                }
                            }
                        }
                    }
                }
            }
            if (Input.GetMouseButtonUp(0))
            {
                getoffset = false;
                dragpatt = false;
            }
            if (getoffset)
            {
                if (!dragpatt && (gamefield.GetDragOffset().x != 0 || gamefield.GetDragOffset().y != 0))
                    dragpatt = true;
                if (dragpatt)
                {
                    Ray ray = cam.ScreenPointToRay(Input.mousePosition);
                    RaycastHit hit;
                    if (Physics.Raycast(ray, out hit, 1000f))
                        InstPatt.GetInstance().pattImg.transform.Translate(hit.transform.position.x - InstPatt.GetInstance().pattImg.transform.position.x, 0, hit.transform.position.z - InstPatt.GetInstance().pattImg.transform.position.z);
                }
            }
        }

        if (selectArea)
        {
            gamefield.mouseRestr = true;
            AreaSelecter.GetInstance().SetArea();
        }

        if(drawing)
        {
            int pgoX = 0;
            int pgoZ = 0;
            if (Input.GetMouseButtonDown(0))
            {
                drawGoes = true;
                pressedGo = gamefield.GetPressedGO();
                pgoX = Mathf.RoundToInt(pressedGo.transform.position.x);
                pgoZ = Mathf.RoundToInt(pressedGo.transform.position.z);
            }
            if (Input.GetMouseButtonUp(0))
                drawGoes = false;
            if (drawGoes)
            {
                if (pressedGo != null)
                {
                    
                    switch (drawType)
                    {
                        case DrawType.black:
                            pressedGo = gamefield.GetPressedGO();
                            pgoX = Mathf.RoundToInt(pressedGo.transform.position.x);
                            pgoZ = Mathf.RoundToInt(pressedGo.transform.position.z);
                            if (gamefield.CellStatsR[pgoX,pgoZ] == false)
                                gamefield.FlipCell(pgoX, pgoZ);
                            break;
                        case DrawType.white:
                            pressedGo = gamefield.GetPressedGO();
                            pgoX = Mathf.RoundToInt(pressedGo.transform.position.x);
                            pgoZ = Mathf.RoundToInt(pressedGo.transform.position.z);
                            if (gamefield.CellStatsR[pgoX, pgoZ] == true)
                                gamefield.FlipCell(pgoX, pgoZ);
                            break;
                        case DrawType.invert:
                            GameObject probeGO = gamefield.GetPressedGO();
                            if (probeGO != null)
                                if (probeGO != pressedGo)
                                {
                                    pressedGo = probeGO;
                                    pgoX = Mathf.RoundToInt(pressedGo.transform.position.x);
                                    pgoZ = Mathf.RoundToInt(pressedGo.transform.position.z);
                                    gamefield.FlipCell(pgoX, pgoZ);
                                }
                            break;
                    }
                    
                }
            }
        }
        gamefield.PCalculations();
    }

    enum DrawType
    {
        white = 0,
        black,
        invert
    }

    //а ну ка, опробуем делегаты
    delegate void WorkWithArea(List<Structers.Pair<int, int>> area);
    WorkWithArea ChangeArea;
    bool selectArea = false;
    bool gpan = true;
    bool paused = false;
    bool pauseMenu = false, placeMenu = false, placePatTMenu = false;
    bool toolsMenu = false;
    bool drawMenu = false;
    bool placing = false;
    bool slmenu = false, asureSL = false;

    bool recreateMenu = false, typeSelector = false;
    string typeSelectT = "Select type";
    IntInputString xInput;
    IntInputString zInput;
    int xNewLeng, zNewLeng;

    bool savePattern = false;
    bool customPlaceMenu= false;
    bool autoturn = false;
    bool drawing = false;
    bool savingCustomName = false;
    bool deletingCustomPat = false;
    string custName = "Enter name";
    DrawType drawType = DrawType.black;
    IntInputString readFromTime;
    PatternNums placePatTNum;
    Vector2 scrollPosition = Vector2.zero;
    Rect scrollViewPos;
    public float scrollerWidth;
    public float saveSlotsNum = 1;
    enum SaveLoad
    {
        Save = 0,
        Load = 1
    }
    SaveLoad checkSL;
    int chosedSaveSlot = 1;

    void UnpauseFuckedWay()
    {
        //FIXME
        //УЖАСНЫЙ КОСТЫЛЬ!
        pressedGo = gamefield.GetPressedGO();
        if (pressedGo != null)
        {
            int pgoX, pgoZ;
            pgoX = Mathf.RoundToInt(pressedGo.transform.position.x);
            pgoZ = Mathf.RoundToInt(pressedGo.transform.position.z);
            gamefield.FlipCell(pgoX, pgoZ);
        }
        //конец костыля
    }

    //vars for pat deletion
    int numDeleteArr = 0;
    int numDeletePat = 0;
    //TODO: автоматизировать убирание/появление gpan, паузу и пр.
    void OnGUI()
    {
        if (selectArea)
        {
            if (!savingCustomName)
            {
                if (AreaSelecter.GetInstance().screenCorner1 != Vector3.zero)
                {
                    Vector2 corn1 = AreaSelecter.GetInstance().screenCorner1;
                    Vector2 corn2 = AreaSelecter.GetInstance().screenCorner2 - AreaSelecter.GetInstance().screenCorner1;
                    Drawing.DrawRect(new Rect(corn1, corn2), Color.blue);
                }
                GUI.Box(new Rect(tickR.x - blackind, 0, tickR.width * 2 + blackind * 3, rGpanH + blackind), "", MainSkin.customStyles[0]);
                if (GUI.Button(tickR, tickC, MainSkin.customStyles[1]))
                {
                    if (!savePattern)
                    {
                        ChangeArea(AreaSelecter.GetInstance().GetAreaGO());
                        paused = selectArea = false;
                        gpan = true;
                        gamefield.mouseRestr = false;
                    }
                    else
                    {
                        SavePattern(AreaSelecter.GetInstance().GetAreaCorners());
                        savingCustomName = true;
                    }
                    AreaSelecter.GetInstance().Reset();
                }
                if (GUI.Button(crossR, crossC, MainSkin.customStyles[1]))
                {
                    AreaSelecter.GetInstance().Reset();
                    gamefield.mouseRestr = false;
                    paused = selectArea = false;
                    savePattern = false;
                    gpan = true;
                }
            }
            if (savingCustomName)
            {
                GUI.Box(patSaveNameBoxR, "", MainSkin.customStyles[0]);
                GUI.Box(patSaveNameConfBoxR, "", MainSkin.customStyles[0]);

                custName = GUI.TextField(patSaveNameR, custName, MainSkin.textField);
                if (GUI.Button(patSaveNameConfR, tickC, MainSkin.customStyles[1]))
                {
                    //FIXME
                    UnpauseFuckedWay();
                    PlayerPrefs.SetString(customPatsBaseKey + "_name_" + (customPatCount - 1),custName);
                    custPat[custPat.Count - 1].name = custName;
                    savePattern = paused = selectArea = savingCustomName = false;
                    gpan = true;
                    gamefield.mouseRestr = false;
                }
            }
        }

        if (!pauseMenu && gpan)
        {
            GUI.Box(new Rect(0, 0, Screen.width, rGpanH + blackind), "", MainSkin.customStyles[0]);
            if (GUI.Button(pauseR, pauseC, MainSkin.customStyles[1]))
            {
                gamefield.scrRestraints.Add(restraints["pauseMenu"]);
                paused = pauseMenu = true;
                customPlaceMenu = placeMenu = placePatTMenu = toolsMenu = drawMenu = false;
            }
            if (GUI.Button(placeR, placeC, MainSkin.customStyles[1]))
            {
                paused = !placeMenu;
                placeMenu = !placeMenu;
                customPlaceMenu = placePatTMenu = toolsMenu = drawMenu = false;
            }
            if (GUI.Button(customR, customC, MainSkin.customStyles[1]))
            {
                paused = !customPlaceMenu;
                customPlaceMenu = !customPlaceMenu;
                placeMenu = placePatTMenu = toolsMenu = drawMenu = false;
                scrollViewPos = new Rect(customR.x - blackind, rGpanH + blackind, rCustomPlaceBoxW + scrollerWidth + deletePatR.width + 1, Screen.height - rCustomPlaceBoxH - blackind);
            }
            if (GUI.Button(toolsR,toolsC, MainSkin.customStyles[1]))
            {
                paused = !toolsMenu;
                toolsMenu = !toolsMenu;
                customPlaceMenu = placePatTMenu = placeMenu = drawMenu = false;
            }
            if (GUI.Button(drawR,drawC, MainSkin.customStyles[1]))
            {
                paused = !drawMenu;
                drawMenu = !drawMenu;
                customPlaceMenu = placePatTMenu = placeMenu = toolsMenu = false;
            }
            readFromTime.TextField(ref autoturnTime, MainSkin.textField);
            autoturn = GUI.Toggle(autoturnR, autoturn, " ", MainSkin.customStyles[3]);
        }
        if (pauseMenu)
        {
            if (!slmenu && !recreateMenu)
            {
                GUI.Box(pauseMenuR, "", MainSkin.customStyles[0]);
                if (GUI.Button(continueR, continueC, MainSkin.customStyles[1]))
                {
                    //FIXME
                    UnpauseFuckedWay();
                    gamefield.scrRestraints.Clear();
                    paused = pauseMenu = false;
                }
                if (GUI.Button(recreateFieldR, recreateFieldC, MainSkin.customStyles[1]))
                {
                    recreateMenu = true;
                }

                if (GUI.Button(saveR, saveC, MainSkin.customStyles[1]))
                {
                    gamefield.scrRestraints.Add(restraints["slMenu"]);
                    gamefield.scrRestraints.Add(restraints["slBack"]);
                    gamefield.scrRestraints.Add(restraints["slAsure"]);
                    gamefield.scrRestraints.Add(restraints["slConf"]);
                    slmenu = true;
                    checkSL = SaveLoad.Save;
                }
                if (GUI.Button(loadR, loadC, MainSkin.customStyles[1]))
                {
                    slmenu = true;
                    checkSL = SaveLoad.Load;
                }
                if (GUI.Button(exitR, exitC, MainSkin.customStyles[1]))
                {
                    Loader.Save(0, ref gamefield);
                    gamefield.DestroyField();
                    Quaternion menuRot = Quaternion.identity;
                    menuRot.eulerAngles = new Vector3(60, 0, 0);
                    GameObject tmp = Instantiate(MenuCamera, new Vector3(0, 0, 0), menuRot) as GameObject;
                    Destroy(this.gameObject);
                }
            }
            if (slmenu)
            {
                GUI.Box(SLMenuRect, "", MainSkin.customStyles[0]);

                GUI.Box(SLBackRect, "", MainSkin.customStyles[0]);
                if (GUI.Button(SLBackButtRect, "Back", MainSkin.customStyles[1]))
                {
                    gamefield.scrRestraints.Clear();
                    slmenu = false;
                }
                for (int i = 1; i<=saveSlotsNum; i++)
                {
                    string cont = " Empty";
                    if (Loader.GetSizeAt(i).first != 0)
                        cont = "Save " + i + ": " + Loader.GetSizeAt(i).first + "x" + Loader.GetSizeAt(i).second;
                    if (GUI.Button(new Rect(SLSlotRect.x,SLSlotRect.y + (rSlotH + blackind)*(i-1),SLSlotRect.width,SLSlotRect.height),cont, MainSkin.customStyles[1]))
                    {
                        asureSL = true;
                        chosedSaveSlot = i;
                    }
                }
                if (asureSL)
                {
                    GUI.Box(SLAsureQBox, "", MainSkin.customStyles[0]);
                    GUI.Box(SLAsureQ, ((checkSL == SaveLoad.Load) ? "Load from slot " : (Loader.GetSizeAt(chosedSaveSlot).first!=0 ? "Rewrite " : "Save ") + "in slot ") + chosedSaveSlot + "?", MainSkin.customStyles[1]);
                    GUI.Box(SLConfBox, "", MainSkin.customStyles[0]);
                    if (GUI.Button(SLConf, SLConfC, MainSkin.customStyles[1]))
                    {
                        gamefield.scrRestraints.Clear();
                        asureSL = false;
                        slmenu = false;
                        if (checkSL == SaveLoad.Load)
                            Loader.Load(chosedSaveSlot, ref gamefield);
                        else
                            Loader.Save(chosedSaveSlot, ref gamefield);
                    }
                }

            }
            if (recreateMenu)
            {
                GUI.Box(fieldMenuBoxR, " ", MainSkin.customStyles[0]);
                if (!typeSelector)
                {
                    xInput.TextField(ref xNewLeng, MainSkin.textField);
                    zInput.TextField(ref zNewLeng, MainSkin.textField);
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
                    Restart(typeSelecterC.text);
                    recreateMenu = false;
                    ShowAd();
                }
                if (GUI.Button(fieldBackR, backC, MainSkin.customStyles[1]))
                {
                    typeSelector = false;
                    typeSelecterC.text = typeSelectT;
                    recreateMenu = false;
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
        if (placeMenu)
        {
            GUI.Box(new Rect(rPauseW + blackind, rGpanH + blackind, rPlaceW + blackind * 2, (rGpanH + blackind) * 4), "", MainSkin.customStyles[0]);
            if (GUI.Button(movPatR, movPatC, MainSkin.customStyles[1]))
            {
                placePatTNum = PatternNums.mov;
                placePatTMenu = true;
                scrollViewPos = new Rect(rPauseW + rPlaceW + blackind * 3, rGpanH + blackind + rGpanH * (int)placePatTNum, rPlaceBoxHW + scrollerWidth, Screen.height - rGpanH - blackind - rGpanH * (int)placePatTNum);
            }
            if (GUI.Button(periPatR, periPatC, MainSkin.customStyles[1]))
            {
                placePatTNum = PatternNums.peri;
                placePatTMenu = true;
                scrollViewPos = new Rect(rPauseW + rPlaceW + blackind * 3, rGpanH + blackind + rGpanH * (int)placePatTNum, rPlaceBoxHW + scrollerWidth, Screen.height - rGpanH - blackind - rGpanH * (int)placePatTNum);
            }
            if (GUI.Button(genPatR, genPatC, MainSkin.customStyles[1]))
            {
                placePatTNum = PatternNums.gen;
                placePatTMenu = true;
                scrollViewPos = new Rect(rPauseW + rPlaceW + blackind * 3, rGpanH + blackind + rGpanH * (int)placePatTNum, rPlaceBoxHW + scrollerWidth, Screen.height - rGpanH - blackind - rGpanH * (int)placePatTNum);
            }
            if (GUI.Button(statPatR, statPatC, MainSkin.customStyles[1]))
            {
                placePatTNum = PatternNums.stat;
                placePatTMenu = true;
                scrollViewPos = new Rect(rPauseW + rPlaceW + blackind * 3, rGpanH + blackind + rGpanH * (int)placePatTNum, rPlaceBoxHW + scrollerWidth, Screen.height- rGpanH - blackind - rGpanH * (int)placePatTNum);
            }
            if(placePatTMenu)
            {
                scrollPosition = GUI.BeginScrollView(scrollViewPos, scrollPosition, new Rect(0, 0, rPlaceBoxHW, Patterns[(int)placePatTNum].Count * rPlaceBoxHW));
                for (int i = 0; i < Patterns[(int)placePatTNum].Count; i++)
                    if (GUI.Button(new Rect(0, rPlaceBoxHW * i, rPlaceBoxHW, rPlaceBoxHW), Patterns[(int)placePatTNum][i].name))
                    {
                        placePatTMenu = placeMenu = false;
                        paused = true;
                        placing = true;
                        gpan = false;
                        InstPatt.GetInstance().Place(Patterns[(int)placePatTNum][i], this.transform.position, 0);
                        break;
                    }
                GUI.EndScrollView();
            }
        }
        if (customPlaceMenu)
        {
            if (!deletingCustomPat)
            {
                scrollPosition = GUI.BeginScrollView(scrollViewPos, scrollPosition, new Rect(0, 0, rCustomPlaceBoxW + deletePatR.width, custPat.Count * rCustomPlaceBoxH));
                int i = 0;
                for (int j = 0; j < MAX_CUSTOMS; j++)
                    if (customPatsNums[j])
                    {
                        if (GUI.Button(new Rect(0, rCustomPlaceBoxH * i, rCustomPlaceBoxW, rCustomPlaceBoxH), custPat[i].name))
                        {
                            customPlaceMenu = false;
                            paused = true;
                            placing = true;
                            gpan = false;
                            InstPatt.GetInstance().Place(custPat[i], this.transform.position, 0);
                            break;
                        }
                        if (GUI.Button(new Rect(rCustomPlaceBoxW + 1, rCustomPlaceBoxH * i + 1, deletePatR.width, deletePatR.height), crossC, MainSkin.customStyles[4]))
                        {
                            numDeleteArr = j;
                            numDeletePat = i;
                            deletingCustomPat = true;
                            break;
                        }
                        i++;
                    }
                GUI.EndScrollView();
            }
            else
            {
                GUI.Box(deletePatConfBoxR, "", MainSkin.customStyles[0]);
                GUI.Box(deletePatConfR, deletePatConfC.text + "'" + /*possible bug creator*/custPat[numDeletePat].name + "'" + "?" , MainSkin.customStyles[1]);
                GUI.Box(deletePatTickBoxR, "", MainSkin.customStyles[0]);
                GUI.Box(deletePatCrossBoxR, "", MainSkin.customStyles[0]);
                if (GUI.Button(deletePatTickR, tickC, MainSkin.customStyles[1]))
                {
                    //FIXME
                    UnpauseFuckedWay();
                    DeleteCustomPattern(numDeleteArr, numDeletePat);
                    customPlaceMenu = false;
                    paused = false;
                    deletingCustomPat = false;
                }
                if (GUI.Button(deletePatCrossR, crossC, MainSkin.customStyles[1]))
                {
                    //FIXME
                    UnpauseFuckedWay();
                    customPlaceMenu = false;
                    paused = false;
                    deletingCustomPat = false;
                }
            }
        }
        if (toolsMenu)
        {
            GUI.Box(new Rect(rPauseW + rPlaceW + rCustomW + blackind * 3, rGpanH + blackind, rPlaceW + blackind * 2, (rGpanH + blackind) * 4), "", MainSkin.customStyles[0]);
            if (GUI.Button(fillWhiteR,fillWhiteC, MainSkin.customStyles[1]))
            {
                gamefield.mouseRestr = true;
                gpan = false;
                paused = true;
                toolsMenu = false;
                selectArea = true;
                ChangeArea = FillWhite;
            }
            if (GUI.Button(fillBlackR,fillBlackC, MainSkin.customStyles[1]))
            {
                gamefield.mouseRestr = true;
                gpan = false;
                paused = true;
                toolsMenu = false;
                selectArea = true;
                ChangeArea = FillBlack;
            }
            if (GUI.Button(invertR,invertC, MainSkin.customStyles[1]))
            {
                gamefield.mouseRestr = true;
                gpan = false;
                paused = true;
                toolsMenu = false;
                selectArea = true;
                ChangeArea = Invert;
            }
            if (GUI.Button(savePattR,savePattC, MainSkin.customStyles[1]))
            {
                gamefield.mouseRestr = true;
                gpan = false;
                paused = true;
                toolsMenu = false;
                selectArea = true;
                savePattern = true;
            }
        }
        if (drawMenu)
        {
            GUI.Box(new Rect(drawR.x - blackind, rGpanH + blackind, drawR.width + blackind * 2, drawWhiteR.height * 3 + blackind * 3), " ", MainSkin.customStyles[0]);
            if (GUI.Button(drawWhiteR, drawWhiteC, MainSkin.customStyles[1]))
            {
                drawType = DrawType.white;
                drawing = true;
                drawMenu = false;
                gpan = false;
            }
            if (GUI.Button(drawBlackR, drawBlackC, MainSkin.customStyles[1]))
            {
                drawType = DrawType.black;
                drawing = true;
                drawMenu = false;
                gpan = false;
            }
            if (GUI.Button(drawInvertR, drawInvertC, MainSkin.customStyles[1]))
            {
                drawType = DrawType.invert;
                drawing = true;
                drawMenu = false;
                gpan = false;
            }
        }
        if (drawing)
        {
            GUI.Box(endDrawBoxR, "", MainSkin.customStyles[0]);
            if (GUI.Button(endDrawR,crossC, MainSkin.customStyles[1]))
            {
                gpan = true;
                drawing = false;
                paused = false;
                if (drawType == DrawType.black)
                {
                    //FIXME
                    UnpauseFuckedWay();
                }
            }
        }
    }

    void Invert (List<Structers.Pair<int, int>> area)
    {
        foreach (var item in area)
            gamefield.FlipCell(item.first, item.second);
        Debug.Log("Inverted");
    }

    void FillBlack (List<Structers.Pair<int, int>> area)
    {
        foreach (var item in area)
            if (gamefield.CellStatsR[item.first, item.second] == false)
                gamefield.FlipCell(item.first, item.second);
        Debug.Log("Filled black");
    }

    void FillWhite(List<Structers.Pair<int, int>> area)
    {
        foreach (var item in area)
            if (gamefield.CellStatsR[item.first, item.second] == true)
                gamefield.FlipCell(item.first, item.second);
        Debug.Log("Filled white");
    }

    void SaveCustomPatternNums () { PlayerPrefsX.SetBoolArray(customPatsBaseKey + "_nums", customPatsNums); }
    void SavePattern(Structers.Pair<Structers.Pair<int,int>, Structers.Pair<int, int>> coord)
    {
        //string path = "Assets/Texts/customPatterns.txt";
        //StreamWriter writer = new StreamWriter(path, true);
        Structers.Pair<int, int> corn1 = coord.first;
        Structers.Pair<int, int> corn2 = coord.second;
        //writer.Write("." + name + ":" + writer.NewLine);
        List<List<bool>> pat = new List<List<bool>>();
        List<bool> row = new List<bool>();
        for (int i = corn1.first-1; i <= corn2.first; i++)
        {
            for (int j = corn1.second; j <= corn2.second; j++)
            {
                row.Add(gamefield.Cells[i, j].transform.rotation == gamefield.AliveR ? true : false);
                //writer.Write(gamefield.Cells[i, j].transform.rotation == gamefield.AliveR ? '1' : '0');
            }
            //writer.Write('_' + writer.NewLine);
            row = new List<bool>();
            pat.Add(row);
        }
        //writer.Write(';' + writer.NewLine);
        //writer.Close(); 
        Pattern custom = new Pattern();
        string name = "custom " + customPatCount;
        customPatsNums[customPatCount] = true;
        SaveCustomPatternNums();
        custom.name = name;
        custom.def = pat;
        custPat.Add(custom);
        customPatCount++;
        PlayerPrefs.SetInt(customPatsBaseKey + customPatPfxCount, customPatCount);
        int size = pat.Count;
        List<bool>[] arr = pat.ToArray();
        PlayerPrefs.SetInt(customPatsBaseKey + "_" + name, size);
        for (int i = 0; i < arr.Length; i++)
            PlayerPrefsX.SetBoolArray(customPatsBaseKey + "_" + name + customPatsLanePfx + i, arr[i].ToArray());
        Debug.Log("saved");
        savePattern = false;
    }

    void DeleteCustomPattern(int numArr, int numPat)
    {
        Debug.Log("Deleting at " + numPat);
        custPat.RemoveAt(numPat);
        string name = "custom " + numArr;
        PlayerPrefs.DeleteKey(customPatsBaseKey + "_" + name);
        //TODO
        //possible memory leak
        /*for (int i = 0; i < custPat[numPat].def.Count; i++)
            PlayerPrefs.DeleteKey(customPatsBaseKey + "_" + name + customPatsLanePfx + i);*/
        customPatsNums[numArr] = false;
        SaveCustomPatternNums();
    }

    void Restart(string fType)
    {
        gamefield.DestroyField();
        gamefield.Initialize(xNewLeng, zNewLeng);
        switch (fType)
        {
            case typeWhite:
                gamefield.CreateEmptyField(false);
                break;
            case typeBlack:
                gamefield.CreateEmptyField(true);
                break;
            case typeRandom:
                gamefield.CreateRandomField();
                break;
            default:
                gamefield.CreateEmptyField(false);
                break;
        }
    }

    public void ShowAd()
    {
        if (Advertisement.IsReady())
        {
            Debug.Log("ad");
            Advertisement.Show();
        }
    }
}
