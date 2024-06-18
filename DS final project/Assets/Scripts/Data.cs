using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;

public class Data : MonoBehaviour
{
    private int[] angels = { 280, 290, 300, 310, 320, 330 };
    private string[] direction = { "right", "right", "right", "left", "left", "left" };
    [SerializeField] private List<TableRow> DataTable = new List<TableRow>();


    private int[] FirstAngles;
    private int[] SecondAngles;
    private int[] ThirdAngles;
    private int[] FourthAngles;
    private int[] ScenesOreder;
    private int[][] AllTheAngles;
    private int[] NowAngles;
    [SerializeField] private int[] current = { 0, 0 };
    private string[] ScenesNames = { "R0", "R1", "S0", "S1" };
    private string currentScreneName;
    private DateTime startTime;
    private string outputFileName;
    private string filePath;

    [SerializeField] private GameObject numberRandomizer;
    [SerializeField] private GameObject headLocation;
    [SerializeField] private GameObject thanksPanel;
    [SerializeField] private GameObject newScrenePanel;
    [SerializeField] private TextMeshProUGUI ScreneText;
    [SerializeField] private Button ScreneButton;
    [SerializeField] private GameObject stopPanel;
    [SerializeField] private Button stopButton;
    [SerializeField] private Button thanksButton;
    [SerializeField] private GameObject butterfly;


    [SerializeField] private int currentAngleIndex = 0;
    [SerializeField] private int currentSceneIndex = 0;
    public static float startAngle = 0;
    float tempButterAngle = 0;
    float tempButterAngleRefSP = 0;
    public static bool SaveData = false;
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

    public event EventHandler StartToSample;
    public event EventHandler AskForStartAngle;
    public event EventHandler OnTaskButtonClicked;
    public event EventHandler OnStopButtonClicked;

    [System.Serializable] 
    public class TableRow
    {
        public string screneName;
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
        filePath = Path.Combine(Application.persistentDataPath, outputFileName);

        //taskButton.onClick.AddListener(TaskButtonClicked);
        stopButton.onClick.AddListener(StopButtonClicked);
        thanksButton.onClick.AddListener(ThanksButtonClicked);
        // stopPanel.SetActive(false);
        newScrenePanel.SetActive(false);
        thanksPanel.SetActive(false);

        RandomNubers randomNubers = numberRandomizer.GetComponent<RandomNubers>();
        randomNubers.OnGenerateAngles += ComponentAnglesOrder;
        randomNubers.OnGenerateScenes += ComponentAScenesOrder;
        
        HeadLocation location = headLocation.GetComponent<HeadLocation>();
       //location.OnSaveEndLoction += 
       //location.OnPoseCaptured += HandlePoseCaptured;

        //waitingForStartPosition = true;
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

    private void TopManager()
    {
        NowAngles = AllTheAngles[currentSceneIndex];
        current[0] = ScenesOreder[currentSceneIndex];
        ChangeSceneName();
        OnCurentScenes?.Invoke(this, new CurrentEventArgs(ScenesOreder[currentSceneIndex]));
        newScrenePanel.SetActive(true);
        ScreneText.text = "You are now in the " + currentScreneName + "let's start catching butterflies";
        ScreneButton.onClick.AddListener(OnScreneButtonClicked);
    }

    private void OnScreneButtonClicked()
    {
        newScrenePanel.SetActive(false);
        SaveData = true;
        SceneManeger();
    }
    private void SceneManeger()
    {
        AskForStartAngle?.Invoke(this, EventArgs.Empty);
        ButterflyLocation(NowAngles, startAngle); // change butterfly location
        // איוונט על מיקום פרפר
        // הופעת חץ וכיוון פניה
        while (SaveData)
        {
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
            0); 
            
            DataTable.Add(lastAddedRow);
            // temp table row = event data
            // addin the row to he main table
        }
        // העלמת חץ ופרפר
        currentAngleIndex++;
        BeginNextProcess();
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
        StartToSample?.Invoke(this, EventArgs.Empty);
        TopManager();
    }


    public void ChangeSceneName()
    {
      
        if (current[0] == 1)
        {
            currentScreneName = "empty square room";
        }
        if (current[0] == 2)
        {
            currentScreneName = "furnished square room";
        }
        if (current[0] == 3)
        {
            currentScreneName = "empty round room";
        }
        if (current[0] == 4)
        {
            currentScreneName = "furnished round room";
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
               SaveTablesToFile();
               TopManager();
               return;
            }
        }
    } 

    //private void UpdateCurrentAndShowMessage(int angelValue, string directionValue) // לשנות לחץ בלבד
    //{
    //    taskPanel.SetActive(true);
    //    taskText.text = "Please turn " + angelValue.ToString() + " degrees to the " + directionValue.ToString();
    //}

    private void PopulateTable(List<TableRow> table, string sceneName, int[] anglesOrder) // שיבוא יחד עם דגימת מקום 
    {
        string currentTableName = sceneName;


        OnCurentAngle?.Invoke(this, new AngleEventArgs(ButterAngleSP));// איפנ כדאי לשים?

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
               0); table.Add(lastAddedRow); // לשמור

        UpdateCurrentAndShowMessage(ButterAngleSP, Currentdirection);
    }

    /// /////////////////////////////////////////////////////////////////////
    private void TaskButtonClicked()
    {
        if (waitingForStartPosition)
        {
            OnTaskButtonClicked?.Invoke(this, EventArgs.Empty);
            taskPanel.SetActive(false);
            if (SLConfirmation)
            {
                waitingForStartPosition = false;
                stopPanel.SetActive(true);
                waitingForEndPosition = true;
                SLConfirmation = false;
            }
        }
    }

    private void TaskButtonClicked()
    {
        if (waitingForStartPosition)
        {
            OnTaskButtonClicked?.Invoke(this, EventArgs.Empty);
            taskPanel.SetActive(false);
            if (SLConfirmation)
            {
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
                currentAngleIndex = currentAngleIndex + 1;
                BeginNextProcess();
            }

        }
    }
    /// /////////////////////////////////////////////////
  
    
    private void HandlePoseCaptured(object sender, HeadLocation.PoseEventArgs e) // קרה אוטמטית
    {
        tempStartPosition = e.StartPosition;
        tempElapsedTime = e.ElapsedTime;
        tempAngleError = e.ErrorTime;
        tempResult = e.Result;
        tempStartTime = e.Time;
    }

    private void ApplyPoseDataToCurrentRow() // קרה בהזמנה אחרי לחיצה על סטופ
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
        using (StreamWriter writer = new StreamWriter(filePath))
        {
            foreach (TableRow row in DataTable)
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