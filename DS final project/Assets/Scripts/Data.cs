using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Data : MonoBehaviour
{
    private int[] angels = { 280, 290, 300, 310, 320, 330 };
    private string[] direction = { "right", "right", "right", "left", "left", "left" };
    [SerializeField] private List<TableRow> R0_Data = new List<TableRow>();
    [SerializeField] private List<TableRow> R1_Data = new List<TableRow>();
    [SerializeField] private List<TableRow> S0_Data = new List<TableRow>();
    [SerializeField] private List<TableRow> S1_Data = new List<TableRow>();


    private int[] FirstAngles;
    private int[] SecondAngles;
    private int[] ThirdAngles;
    private int[] FourthAngles;
    private int[] ScenesOreder;
    private int[][] AllTheAngles;
    private int[] NowAngles;
    [SerializeField] private int[] current = { 0, 0 };
    private string[] ScenesNames = { "R0", "R1", "S0", "S1" };
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
    [SerializeField] private GameObject butterfly;


    [SerializeField] private int currentAngleIndex = 0;
    [SerializeField] private int currentSceneIndex = 0;
    float startAngle = 0;
    float tempButterAngle = 0;
    float tempButterAngleRefSP = 0;
    private bool waitingForStartPosition = true;
    private bool waitingForEndPosition = true;
    private bool SLConfirmation = false;
    private bool ELConfirmation = false;

    private Vector3 tempStartPosition;
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
        public string screneName;

        public TableRow(string screneName)
        {
            this.screneName = screneName;
        }

        public int screneOrder;
        public int Taskorder;
        public int butterAngle;
        public int butterAngleRefStartPosition;
        public string direction;
        public Vector3 startPosition;
        public Vector3 currentPosition;
        public float angleIn360;
        public float angleRefStartPosition;
        public float TotTime;
        public float TimeRefTask;


        public TableRow(string screne, int sceneOrder, int order, int butterAngleRefSP, int butterAngle, string direction, Vector3 startPosition, Vector3 currentPosition,
                         float angleIn360, float angleRefStartPosition, float Time, float TimeRefTask)
        {
            this.screneName = screne;
            this.screneOrder = sceneOrder;
            this.Taskorder = order; // ??
            this.butterAngleRefStartPosition = butterAngleRefSP; // butterfly angle
            this.butterAngle = butterAngle; // butterfly angle
            this.direction = direction; //right or left
            this.startPosition = startPosition;
            this.currentPosition = currentPosition;
            this.angleIn360 = angleIn360;
            this.angleRefStartPosition = angleRefStartPosition;
            this.TotTime = Time; // from the beginning
            this.TimeRefTask = TimeRefTask;
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

    private void ButterflyLocation(int[] anglesList, float startAngle)
    {
        current[1] = anglesList[currentAngleIndex];
        tempButterAngle = angels[current[1] - 1];
        string Currentdirection = direction[current[1] - 1];

        if (Currentdirection == "right")
        {
            tempButterAngleRefSP = startAngle + tempButterAngle;
        }
        else{
            tempButterAngleRefSP = startAngle - tempButterAngle;
        }

        if (tempButterAngleRefSP < 0)
        {
            tempButterAngleRefSP = tempButterAngleRefSP + 360;
        }

        float tempButterAngleRefSPRad = tempButterAngleRefSP * Mathf.Deg2Rad;
        float radius = 1.5f; 
        float x = radius * Mathf.Cos(tempButterAngleRefSPRad);
        float y = radius * Mathf.Sin(tempButterAngleRefSPRad);
        float z = butterfly.transform.position.z; 

        // Set the new position
        Vector3 newPosition = new Vector3(x, y, z);
        butterfly.transform.position = newPosition;
    }

    private void ComponentAnglesOrder(object sender, RandomNubers.AngelsEventArgs e)
    {
        Debug.Log("ComponentAnglesOrder called");
        FirstAngles = e.FirstAngles;
        SecondAngles = e.SecondAngles;
        ThirdAngles = e.ThirdAngles;
        FourthAngles = e.FourthAngles;

        AllTheAngles = new int[][] { FirstAngles, SecondAngles, ThirdAngles, FourthAngles }; // ?  
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
        ButterflyLocation (NowAngles, startAngle); // change butterfly location

        if (ScenesOreder[currentSceneIndex] == 1)
        {
            PopulateTable(R0_Data, "empty square room", NowAngles);
        }
        if (ScenesOreder[currentSceneIndex] == 2)
        {
            PopulateTable(R1_Data, "furnished square room", NowAngles);
        }
        if (ScenesOreder[currentSceneIndex] == 3)
        {
            PopulateTable(S0_Data, "empty round room", NowAngles) ;
        }
        if (ScenesOreder[currentSceneIndex] == 4)
        {
            PopulateTable(S1_Data, "furnished round room", NowAngles);
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

    private void UpdateCurrentAndShowMessage(int angelValue, string directionValue) // לשנות לחץ בלבד
    {
        taskPanel.SetActive(true);
        taskText.text = "Please turn " + angelValue.ToString() + " degrees to the " + directionValue.ToString();
    }

    private void PopulateTable(List<TableRow> table, string sceneName, int[] anglesOrder) // שיבוא יחד עם דגימת מקום 
    {
        string currentTableName = sceneName;


        OnCurentAngle?.Invoke(this, new AngleEventArgs(ButterAngleSP));

        lastAddedRow = new TableRow(
               sceneName,
               currentSceneIndex + 1,
               current[1],
               ButterAngleSP,
               0,
               Currentdirection,
               Vector3.zero, 
               Vector3.zero,
               0,
               0,
               0,
               0); table.Add(lastAddedRow); // שמירה

        UpdateCurrentAndShowMessage(ButterAngleSP, Currentdirection);
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
        tempElapsedTime = e.ElapsedTime;
        tempAngleError = e.ErrorTime;
        tempResult = e.Result;
        tempStartTime = e.Time;
    }

    private void ApplyPoseDataToCurrentRow() // ?
    {
        if (lastAddedRow != null)
        {
            lastAddedRow.startPosition = tempStartPosition;
            lastAddedRow.angleIn360 = tempAngleError; // ?
            lastAddedRow.TotTime = tempElapsedTime;
            lastAddedRow.angleRefStartPosition = tempResult; //?
            lastAddedRow.TimeRefTask = tempStartTime; //? 
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
        allTables.AddRange(R0_Data);
        allTables.AddRange(R1_Data);
        allTables.AddRange(S0_Data);
        allTables.AddRange(S1_Data);


        string filePath = Path.Combine(Application.persistentDataPath, outputFileName);
        using (StreamWriter writer = new StreamWriter(filePath))
        {
            foreach (TableRow row in allTables)
            {
                writer.WriteLine($"Scene: {row.screneName}, SenceOrder: {row.screneOrder}, Taskorder: {row.Taskorder}, Instruction: {row.butterAngle}, Direction: {row.direction}, StartPosition: {row.startPosition}, Butterfly angle in 360: {row.angleIn360} , Butterfly angle relative to starting angle: {row.angleRefStartPosition}, Time from the beginning : {row.TotTime}, time relative to the task: {row.TimeRefTask}");
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