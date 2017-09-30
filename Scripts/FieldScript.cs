﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FieldScript : MonoBehaviour
{
    GameManagerClassic manager;
    public float rGpanH;
    //собственно префаб клетки
    public GameObject Cell, RBord, RCorner;
    //ширина границы
    public float brdwid;
    public Material whitem, blackm;
    //длина стороны поля
    int lengthx, lengthz;
    //массив всех клеток на поле
    public GameObject[,] Cells;
    //массив границ
    GameObject[] borders;
    //массив реальных состояний клеток и массив состояний клеток для рассчетов
    public bool[,] CellStatsR;
    bool[,] CellStatsPrep;
    List<Structers.Pair<int, int>> modifiedCells = new List<Structers.Pair<int, int>>();
    //поворот "живой" и "мертвой" клетки в кватернионах. так надо
    Quaternion AliveRot, DeadRot;
    public Quaternion AliveR
    {
        get { return AliveRot; }
    }
    public Quaternion DeadR
    {
        get { return DeadRot; }
    }
    //массив клеток, которые надо будет повернуть при ходе
    Dictionary<Vector2, GameObject> ToTurn;
    //сколкьо клеток надо повернуть
    int needturn;
    // происходит ли поворот клеток
    bool turning;
    //состояние паузы
    public bool paused;
    //скорость поворота
    float TurnSpeed;
    public List<Rect> scrRestraints = new List<Rect>();

    void Awake()
    {
        //задаем углы поворота для состояний клеток
        DeadRot.eulerAngles = new Vector3(0, 0, 0);
        AliveRot.eulerAngles = new Vector3(180, 0, 0);
    }

    //USE WITH CAUTION, might break active/deactivated logic (optimization)
    void SetFullyActive(int i, int j)
    {
        Cells[i, j].transform.GetChild(0).gameObject.SetActive(true);
        Cells[i, j].transform.GetChild(1).gameObject.SetActive(true);
    }

    void ReActivate(int i, int j)
    {
        if (Cells[i, j].transform.GetChild(0).gameObject.activeSelf)
        {
            Cells[i, j].transform.GetChild(1).gameObject.SetActive(true);
            Cells[i, j].transform.GetChild(0).gameObject.SetActive(false);
        }
        else
        {
            Cells[i, j].transform.GetChild(1).gameObject.SetActive(false);
            Cells[i, j].transform.GetChild(0).gameObject.SetActive(true);
        }
    }

    public Structers.Pair<int,int> GetSizeForSave ()
    {
        return new Structers.Pair<int, int>(lengthx, lengthz);
    }

    public Structers.Pair<int, int> GetSize()
    {
        return new Structers.Pair<int, int>(lengthx-2, lengthz-2);
    }

    public void SetAllModefied()
    {
        for (int i = 1; i < lengthx - 1; i++)
            for (int j = 1; j < lengthz - 1; j++)
                modifiedCells.Add(new Structers.Pair<int, int>(i, j));
    }
    
    public void SetSize(int x, int z)
    {
        DestroyField();
        PrepareArrays();
        Initialize(x, z);
        CreateEmptyField();
    }

    public void SetTurnSpeed(float TurnTm)
    {
        TurnSpeed = 180 / TurnTm;
    }
    void PrepareArrays()
    {
        //инициализируем массивы
        Cells = new GameObject[lengthx, lengthz];
        CellStatsR = new bool[lengthx, lengthz];
        CellStatsPrep = new bool[lengthx, lengthz];
    }

    public void DestroyField ()
    {
        for (int i = 0; i < lengthx; i++)
            for (int j = 0; j<lengthz; j++)
                Destroy(Cells[i,j]);
        Destroy(bord1);
        Destroy(bord2);
        Destroy(bord3);
        Destroy(bord4);
        Destroy(corn1);
        Destroy(corn2);
        Destroy(corn3);
        Destroy(corn4);
    }

    //initialization
    public void Initialize(int xleng,int zleng)
    {
        Debug.Log("field initialized with: " + xleng + ", " + zleng);
        lengthx = xleng + 2;
        lengthz = zleng + 2;
        PrepareArrays();
        ToTurn = new Dictionary<Vector2, GameObject>();
        cam = this.GetComponent<Camera>();
    }

    public void Turn()
    {
        Debug.Log("moidfied at start of turn calcs: " + modifiedCells.Count);
        ToTurn = new Dictionary<Vector2, GameObject>();
        foreach (var coor in modifiedCells)
        {
            int i = coor.first;
            int j = coor.second;
            for (int x = i - 1; x <= i + 1; x++)
            {
                //МАЛЕНЬКИЙ КОСТЫЛЬ
                if (x < 1)
                    x = 1;
                if (x > lengthx - 2)
                    x = lengthx - 2;
                for (int z = j - 1; z <= j + 1; z++)
                {
                    //МАЛЕНЬКИЙ КОСТЫЛЬ
                    if (z < 1)
                        z = 1;
                    if (z > lengthz - 2)
                        z = lengthz - 2;

                    int calives = 0;
                    if (CellStatsR[x + 1, z] == true) calives++;
                    if (CellStatsR[x + 1, z + 1] == true) calives++;
                    if (CellStatsR[x, z + 1] == true) calives++;
                    if (CellStatsR[x - 1, z] == true) calives++;
                    if (CellStatsR[x, z - 1] == true) calives++;
                    if (CellStatsR[x - 1, z - 1] == true) calives++;
                    if (CellStatsR[x - 1, z + 1] == true) calives++;
                    if (CellStatsR[x + 1, z - 1] == true) calives++;


                    if (CellStatsR[x, z] == false && calives == 3)
                    {
                        CellStatsPrep[x, z] = true;
                        ToTurn[new Vector2(x, z)] = Cells[x, z];
                    }

                    if (CellStatsR[x, z] == true && (calives == 2 || calives == 3))
                        CellStatsPrep[x, z] = true;

                    if (CellStatsR[x, z] == true && !(calives == 2 || calives == 3))
                    {
                        CellStatsPrep[x, z] = false;
                        ToTurn[new Vector2(x, z)] = Cells[x, z];

                    }
                    if (z == lengthz - 2)
                        break;
                    calives = 0;
                }
                if (x == lengthx - 2)
                    break;
            }
        }

        foreach (var go in ToTurn)
            SetFullyActive((int)go.Key.x, (int)go.Key.y);
        modifiedCells.Clear();
        turning = true;
        CellStatsR = CellStatsPrep;
        CellStatsPrep = new bool[lengthx, lengthz];
        Debug.Log("Turn prepared");
    }

    //создаем пустое поле
    GameObject bord1, bord2, bord3, bord4;
    GameObject corn1, corn2, corn3, corn4;
    public void CreateEmptyField ()
    {
        Debug.Log("size on empty creation: " + lengthx + ", " + lengthz);
        //создаем поле
        for (int i = 1; i < lengthx - 1; i++)
            for (int j = 1; j < lengthz - 1; j++)
            {
                //инстансим клетки
                Cells[i, j] = Instantiate(Cell, new Vector3(i, 0, j), Quaternion.identity) as GameObject;
                CellStatsR[i, j] = false;
                CellStatsPrep[i, j] = false;
            }
        PlaceBorders();
    }

    public void CreateRandomField()
    {
        Debug.Log("size on random creation: " + lengthx + ", " + lengthz);
        //создаем поле
        bool ran;
        for (int i = 1; i < lengthx - 1; i++)
            for (int j = 1; j < lengthz - 1; j++)
            {
                modifiedCells.Add(new Structers.Pair<int, int>(i, j));//WORKS?
                if (Random.Range(0, 2) == 1) ran = true;
                else ran = false;
                //инстансим клетки
                if (ran) Cells[i, j] = Instantiate(Cell, new Vector3(i, 0, j), AliveRot) as GameObject;
                else Cells[i, j] = Instantiate(Cell, new Vector3(i, 0, j), DeadRot) as GameObject;

                CellStatsR[i, j] = ran;     
                CellStatsPrep[i, j] = ran;
            }
        PlaceBorders();
    }

    void PlaceBorders()
    {
        Vector3 scl;
        Quaternion rot = Quaternion.identity;
        rot.eulerAngles = new Vector3(0, 90, 0);
        scl = new Vector3(1, 1, lengthz - 2);
        bord1 = Instantiate(RBord, new Vector3(-brdwid / 2 + 0.5f, 0, lengthz / 2 - 0.5f), Quaternion.identity) as GameObject;
        rot.eulerAngles = new Vector3(0, 180, 0);
        bord2 = Instantiate(RBord, new Vector3(lengthx - 1 - 0.5f + brdwid / 2, 0, lengthz / 2 - 0.5f), rot) as GameObject;
        bord1.transform.localScale = bord2.transform.localScale = scl;
        rot.eulerAngles = new Vector3(0, 270, 0);
        scl = new Vector3(1, 1, lengthx - 2);
        bord3 = Instantiate(RBord, new Vector3(lengthx / 2 - 0.5f, 0, -brdwid / 2 + 0.5f), rot) as GameObject;
        rot.eulerAngles = new Vector3(0, 90, 0);
        bord4 = Instantiate(RBord, new Vector3(lengthx / 2 - 0.5f, 0, lengthz - 1 - 0.5f + brdwid / 2), rot) as GameObject;
        bord3.transform.localScale = bord4.transform.localScale = scl;

        corn1 = Instantiate(RCorner, new Vector3(0.5f - brdwid / 2, 0, lengthz - 2 + 0.5f + brdwid / 2), Quaternion.identity) as GameObject;
        rot.eulerAngles = new Vector3(0, -90, 0);
        corn2 = Instantiate(RCorner, new Vector3(0.5f - brdwid / 2, 0, 0.5f - brdwid / 2), rot) as GameObject;
        rot.eulerAngles = new Vector3(0, 180, 0);
        corn3 = Instantiate(RCorner, new Vector3(lengthx - 2 + 0.5f + brdwid / 2, 0, 0.5f - brdwid / 2), rot) as GameObject;
        rot.eulerAngles = new Vector3(0, 90, 0);
        corn4 = Instantiate(RCorner, new Vector3(lengthx - 2 + 0.5f + brdwid / 2, 0, lengthz - 2 + 0.5f + brdwid / 2), rot) as GameObject;
    }

    RaycastHit HitClick;
    Ray RayForClick;
    public GameObject GetPressedGO()
    {
        return GetGOFromScreen(Input.mousePosition);
    }

    public GameObject GetGOFromScreen(Vector3 pos)
    {
        RayForClick = cam.ScreenPointToRay(pos);//создание луча
        if (Physics.Raycast(RayForClick, out HitClick, 1000f))
            return HitClick.collider.gameObject;//выясняем, на что нажали
        else
            return null;
    }

    public Vector3 mouseposcl = new Vector3(0, 0, 0);
    public int mssensitivity = 10;
    public float camerasesitivity = 5;
    public Vector2 GetDragOffset ()
    {
        Vector2 offset = new Vector2(0, 0);
        float dst = Mathf.Sqrt((mouseposcl.x - Input.mousePosition.x) * (mouseposcl.x - Input.mousePosition.x) + (mouseposcl.y - Input.mousePosition.y) * (mouseposcl.y - Input.mousePosition.y));
        if (dst > mssensitivity)
        {
            float movex, movez;
            movex = mouseposcl.x - Input.mousePosition.x;
            movez = mouseposcl.y - Input.mousePosition.y;
            offset.x = movex * camerasesitivity;
            offset.y = movez * camerasesitivity;
            mouseposcl = Input.mousePosition;
        }
        return offset;
    }

    public void FlipCell(int i, int j)
    {
        modifiedCells.Add(new Structers.Pair<int, int>(i, j));
        ReActivate(i, j);
        //Смена состояния и поворота клетки
        if (CellStatsR[i, j] == true)
        {
            
            CellStatsR[i, j] = false;
            Cells[i,j].transform.rotation = DeadRot;
        }
        else
        {
            CellStatsR[i, j] = true;
            Cells[i, j].transform.rotation = AliveRot;
        }
    }

    float deltaAng = 0;
    GameObject pressedGo;
    bool wassaving, wasloading;
    bool mouse0pushed = false;
    public Camera cam;
    bool getoffset = false, mousemoved = false;
    public bool mouseRestr = false;
    public void PCalculations()
    {
        
        //TODO
        if (!turning && !wassaving && !wasloading)
        {
            if (Input.GetMouseButtonDown(0))
            {
                mouse0pushed = true;
                pressedGo = GetPressedGO();
                if (pressedGo != null)
                {
                    //если это клетка
                    if (pressedGo.tag == "Pattern")
                        mouseRestr = true;
                }
                if (Input.mousePosition.y < (Screen.height - rGpanH) && !mouseRestr)
                {
                    mouseposcl = Input.mousePosition;
                    getoffset = true;
                }
            }
            if (getoffset)
            {
                Vector2 offset = GetDragOffset();
                if (offset.x != 0 || offset.y != 0)
                    mousemoved = true;
                this.transform.Translate(offset.x, offset.y,0);
                if (this.transform.position.z <= 0.5 + 5)
                    this.transform.position = new Vector3(this.transform.position.x, this.transform.position.y, 0.5f + 5);
                if (this.transform.position.z >= lengthz + 0.5 - 5)
                    this.transform.position = new Vector3(this.transform.position.x, this.transform.position.y, lengthz + 0.5f - 5);
                if (this.transform.position.x <= 0.5 + 5)
                    this.transform.position = new Vector3(0.5f + 5, this.transform.position.y, this.transform.position.z);
                if (this.transform.position.x >= lengthx + 0.5 - 5)
                    this.transform.position = new Vector3(lengthx + 0.5f - 5, this.transform.position.y, this.transform.position.z);
            }

            //обработка нажатия                                                 /*проверка, не нажали ли на GUI*/
            if (Input.GetMouseButtonUp(0))
            {
                
                if (!mousemoved && Input.mousePosition.y < (Screen.height - rGpanH) && !mouseRestr && mouse0pushed)
                {
                    mouse0pushed = false;
                    if (pressedGo != null)
                    {
                        //если это клетка
                        if (pressedGo.tag == "Cell" && !paused)
                        {
                            int pgoX, pgoZ;
                            pgoX = Mathf.RoundToInt(pressedGo.transform.position.x);
                            pgoZ = Mathf.RoundToInt(pressedGo.transform.position.z);
                            FlipCell(pgoX, pgoZ);
                        }
                    }
                }
                mouseposcl = new Vector3(0, 0, 0);
                getoffset = false;
                mousemoved = false;
            }
            if (Input.GetMouseButtonDown(2) && !paused)
                Turn();
        }
        if (turning)
        {
            Quaternion turnrot = new Quaternion(0, 0, 0, 0);
            foreach (var data in ToTurn)
            {
                turnrot = data.Value.transform.rotation;
                turnrot.eulerAngles = new Vector3(turnrot.eulerAngles.x, turnrot.eulerAngles.y, turnrot.eulerAngles.z + TurnSpeed * Time.deltaTime);
                data.Value.transform.rotation = turnrot;
            }
            deltaAng += TurnSpeed * Time.deltaTime;
            if (deltaAng >= 180)
            {
                turning = false;
                deltaAng = 0;
                foreach (var data in ToTurn)
                {
                    modifiedCells.Add(new Structers.Pair<int, int>((int)data.Key.x,(int)data.Key.y));//сложна
                    int crx, crz;
                    crx = Mathf.RoundToInt(data.Value.transform.position.x);
                    crz = Mathf.RoundToInt(data.Value.transform.position.z);
                    if (CellStatsR[crx, crz])
                    {
                        data.Value.transform.rotation = AliveRot;
                        Cells[crx, crz].transform.GetChild(0).gameObject.SetActive(false);
                    }
                    else
                    {
                        data.Value.transform.rotation = DeadRot;
                        Cells[crx, crz].transform.GetChild(1).gameObject.SetActive(false);
                    }
                }
            }
        }
    }
}