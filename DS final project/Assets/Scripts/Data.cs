using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Data : MonoBehaviour
{
    private int[] angels = { 10, 27, 43, 96, 108, 16, 59, 103, 122, 150 };
    private string[] direction = { "right", "right", "right", "right", "right", "left", "left", "left", "left", "left" };
    [SerializeField] private List<TableRow> officeTable = new List<TableRow>();
    [SerializeField] private List<TableRow> roomTable = new List<TableRow>();
    [SerializeField] private List<TableRow> emptyTable = new List<TableRow>();

    private int[] FirstAngles;
    private int[] SecondAngles;
    private int[] ThirdAngles;
    private int[] ScenesOreder;
    private int[][] AllTheAngles;
    private int[] NowAngles;
    [SerializeField] private int[] current = { 0, 0 };
    private string[] ScenesNames = { "Office", "Room", "Empty" };
    private DateTime startTime;
    private string outputFileName;

    [SerializeField] private GameObject numberRandomizer;
    [SerializeField] private GameObject taskPanel;
    [SerializeField] private GameObject headLocation;
    [SerializeField] private GameObject thanksPanel;
    [SerializeField] private TextMeshProUGUI taskText;
    [SerializeField] private Button taskButton;
    [SerializeField] private GameObject stopPanel;
    [SerializeField] private Button stopButton;
    [SerializeField] private Button thanksButton;

    [SerializeField] private int currentAngleIndex = 0;
    [SerializeField] private int currentSceneIndex = 0;
    private bool waitingForStartPosition = true;
    private bool waitingForEndPosition = true;
    private bool SLConfirmation = false;
    private bool ELConfirmation = false;

    private Vector3 tempStartPosition;
    private Vector3 tempEndPosition;
    private float tempElapsedTime;
    private float tempAngleError;
    private float tempResult;
    private float tempStartTime;
    private TableRow lastAddedRow; 


    public event EventHandler <CurrentEventArgs> OnCurentScenes;
    public class CurrentEventArgs : EventArgs
    {
        public int CurrentSceneNum { get; }


        public CurrentEventArgs(int SceneNum)
        {
            CurrentSceneNum = SceneNum;
        }
    }

    public event EventHandler<AngleEventArgs> OnCurentAngle;
    public class AngleEventArgs : EventArgs
    {
        public int Angle { get; }


        public AngleEventArgs(int angleIn360)
        {
            Angle = angleIn360;
        }
    }

    public event EventHandler OnTaskButtonClicked;
    public event EventHandler OnStopButtonClicked;

    [System.Serializable] 
    public class TableRow
    {
        public string screneNum;
        public int screneOrder;
        public int Taskorder;
        public int instruction;
        public string direction;
        public int angle360;
        public Vector3 startPosition;
        public Vector3 endPosition;
        public float result;
        public float error;
        public float TotTime;
        public float Time;


        public TableRow(string screne, int sceneOrder, int order, int instruction, string direction, int angle360, Vector3 startPosition, Vector3 endPosition,
                         float Result, float error, float Time, float startTime)
        {
            this.screneNum = screne;
            this.screneOrder = sceneOrder;
            this.Taskorder = order;
            this.instruction = instruction;
            this.direction = direction;
            this.angle360 = angle360;
            this.startPosition = startPosition;
            this.endPosition = endPosition;
            this.result = Result;
            this.error = error;
            this.TotTime = Time;
            this.Time = startTime;
        }
    }

    void Start()
    {
        startTime = DateTime.Now;
        outputFileName = "Data_" + startTime.ToString("yyyy MM dd _ HH mm ss") + ".txt";

        taskButton.onClick.AddListener(TaskButtonClicked);
        stopButton.onClick.AddListener(StopButtonClicked);
        thanksButton.onClick.AddListener(ThanksButtonClicked);
        stopPanel.SetActive(false);
        taskPanel.SetActive(false);
        thanksPanel.SetActive(false);

        RandomNubers randomNubers = numberRandomizer.GetComponent<RandomNubers>();
        randomNubers.OnGenerateAngles += ComponentAnglesOrder;
        randomNubers.OnGenerateScenes += ComponentAScenesOrder;
        
        HeadLocation location = headLocation.GetComponent<HeadLocation>();
        location.OnSaveStartLoction += StartLocationConfirmation;
        location.OnSaveEndLoction += EndLocationConfirmation;
        location.OnPoseCaptured += HandlePoseCaptured;

        waitingForStartPosition = true;
    }

    private void ComponentAnglesOrder(object sender, RandomNubers.AngelsEventArgs e)
    {
        Debug.Log("ComponentAnglesOrder called");
        FirstAngles = e.FirstAngles;
        SecondAngles = e.SecondAngles;
        ThirdAngles = e.ThirdAngles;
        AllTheAngles = new int[][] { FirstAngles, SecondAngles, ThirdAngles };
    }


    private void ComponentAScenesOrder(object sender, RandomNubers.ScenesEventArgs s)
    {
        ScenesOreder = s.Order;
        CreateTables();
    }

    private void StartLocationConfirmation(object sender, EventArgs SLC)
    {
        SLConfirmation = true;
    }

    private void EndLocationConfirmation(object sender, EventArgs SLC)
    {
        ELConfirmation = true;
    }

    public void CreateTables()
    {
        NowAngles = AllTheAngles[currentSceneIndex];
        current[0] = ScenesOreder[currentSceneIndex];
        OnCurentScenes?.Invoke(this, new CurrentEventArgs(ScenesOreder[currentSceneIndex]));

        if (ScenesOreder[currentSceneIndex] == 1)
        {
            PopulateTable(officeTable, "Office", NowAngles);
        }
        if (ScenesOreder[currentSceneIndex] == 2)
        {
            PopulateTable(roomTable, "Room", NowAngles);
        }
        if (ScenesOreder[currentSceneIndex] == 3)
        {
            PopulateTable(emptyTable, "Empty", NowAngles) ;
        }
    }

    void BeginNextProcess()
    {
        if (currentAngleIndex >= angels.Length)
        {
            if (currentSceneIndex >= ScenesOreder.Length-1)
            {
                thanksPanel.SetActive(true);
                return;
            }

            else
            {
               Debug.Log("Lets Start The Next Level");
               currentAngleIndex = 0;
               currentSceneIndex++;
               waitingForStartPosition = true;
               CreateTables();
               return;
            }
        }

        waitingForStartPosition = true;
        CreateTables();
    } 

    private void UpdateCurrentAndShowMessage(int angelValue, string directionValue)
    {
        taskPanel.SetActive(true);
        taskText.text = "Please turn " + angelValue.ToString() + " degrees to the " + directionValue.ToString();
    }

    private void PopulateTable(List<TableRow> table, string sceneName, int[] anglesOrder)
    {
        current[1] = anglesOrder[currentAngleIndex];

        int CurrentAngel = angels[current[1]-1];
        string Currentdirection = direction[current[1]-1];
        int angle360 = CurrentAngel;
        string currentTableName = sceneName;

        if ( direction[current[1]-1] == "left")
        {
            angle360 = 360 - angle360;
        }

        OnCurentAngle?.Invoke(this, new AngleEventArgs(angle360));

        lastAddedRow = new TableRow(
               sceneName,
               currentSceneIndex+1,
               current[1],
               CurrentAngel,
               Currentdirection,
               angle360,
               Vector3.zero,  
               Vector3.zero, 
               0,
               0,               
               0,
               0);        
        
        table.Add(lastAddedRow);
        UpdateCurrentAndShowMessage(CurrentAngel, Currentdirection);
    }


    private void TaskButtonClicked()
    {
        if (waitingForStartPosition)
        {
            OnTaskButtonClicked?.Invoke(this, EventArgs.Empty);
            taskPanel.SetActive(false);
            if (SLConfirmation) {
                waitingForStartPosition = false;
                stopPanel.SetActive(true);
                waitingForEndPosition = true;
                SLConfirmation = false;
            }
        }
    }

    private void StopButtonClicked()
    {
        if (waitingForEndPosition)
        {
            OnStopButtonClicked?.Invoke(this, EventArgs.Empty);
            stopPanel.SetActive(false);
            if (ELConfirmation)
            {
                ApplyPoseDataToCurrentRow();
                waitingForEndPosition = false;
                ELConfirmation = false;
                currentAngleIndex = currentAngleIndex+1;
                BeginNextProcess();
            }
       
        }
    }
    private void HandlePoseCaptured(object sender, HeadLocation.PoseEventArgs e)
    {
        tempStartPosition = e.StartPosition;
        tempEndPosition = e.EndPosition;
        tempElapsedTime = e.ElapsedTime;
        tempAngleError = e.ErrorTime;
        tempResult = e.Result;
        tempStartTime = e.Time;
    }

    private void ApplyPoseDataToCurrentRow()
    {
        if (lastAddedRow != null)
        {
            lastAddedRow.startPosition = tempStartPosition;
            lastAddedRow.endPosition = tempEndPosition;
            lastAddedRow.error = tempAngleError; 
            lastAddedRow.TotTime = tempElapsedTime;
            lastAddedRow.result = tempResult;
            lastAddedRow.Time = tempStartTime;
        }
    }

    private void ThanksButtonClicked()
    {
        SaveTablesToFile();
        EndGame();
    }

    private void SaveTablesToFile()
    {
        List<TableRow> allTables = new List<TableRow>();
        allTables.AddRange(officeTable);
        allTables.AddRange(roomTable);
        allTables.AddRange(emptyTable);

        string filePath = Path.Combine(Application.persistentDataPath, outputFileName);
        using (StreamWriter writer = new StreamWriter(filePath))
        {
            foreach (TableRow row in allTables)
            {
                writer.WriteLine($"Scene: {row.screneNum}, SenceOrder: {row.screneOrder}, Taskorder: {row.Taskorder}, Instruction: {row.instruction}, Direction: {row.direction}, Angle: {row.angle360}, StartPosition: {row.startPosition}, EndPosition: {row.endPosition}, Result: {row.result} , Error: {row.error}, TotalTime: {row.TotTime}, StartTime: {row.Time}");
            }
        }

        Debug.Log("Tables saved to file: " + filePath);
    }

    private void EndGame()
    {
        Debug.Log("Game ended.");
        Application.Quit();
    }

}