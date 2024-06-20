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
    [SerializeField] private GameObject ScreneButton;
    [SerializeField] private GameObject thanksButton;
    [SerializeField] private GameObject butterfly;


    [SerializeField] private int currentAngleIndex = 0;
    [SerializeField] private int currentSceneIndex = 0;
    public static float startAngle = 0;
    float tempButterAngle = 0;
    float tempButterAngleRefSP = 0;
    public static bool SaveData = false;
    private bool waitingForEndPosition = true;


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

    public event EventHandler<ButterEventArgs> ButterflyAngleUpdated;
    public class ButterEventArgs : EventArgs
    {
        public float ButterAngle { get; }


        public ButterEventArgs(float butterAngle)
        {
            ButterAngle = butterAngle;
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
        public int butterAngleRefStartPosition;
        public int butterAngle;
        public string direction;
        public Vector3 startPosition;
        public float startPositionAngle;
        public Vector3 currentPosition;
        public float currentPositionAngle;
        public float currentPositionAngleRefButter;
        public float TotTime;
        public float TimeRefTask;

        public TableRow(string screneName, int sceneOrder, int taskOrder, int butterAngleRefSP, int butterAngle, string direction,
            Vector3 startPosition, float startPositionAngle, Vector3 currentPosition, float currentPositionAngle, float currentPositionAngleRefButter, float totTime, float TimeRefTask)
        {
            this.screneName = screneName;
            this.screneOrder = sceneOrder;
            this.Taskorder = taskOrder; 
            this.butterAngleRefStartPosition = butterAngleRefSP; // butterfly angle
            this.butterAngle = butterAngle; // butterfly angle
            this.direction = direction; //right or left
            this.startPosition = startPosition;
            this.startPositionAngle = startPositionAngle;
            this.currentPosition = currentPosition;
            this.currentPositionAngle = currentPositionAngle;
            this.currentPositionAngleRefButter = currentPositionAngleRefButter;
            this.TotTime = totTime; // from the beginning
            this.TimeRefTask = TimeRefTask;
        }
    }

    void Start()
    {
        startTime = DateTime.Now;
        outputFileName = "Data_" + startTime.ToString("yyyy MM dd _ HH mm ss") + ".txt";
        filePath = Path.Combine(Application.persistentDataPath, outputFileName);

        //thanksButton.onClick.AddListener(ThanksButtonClicked);
        newScrenePanel.SetActive(false);
        thanksPanel.SetActive(false);

        RandomNubers randomNubers = numberRandomizer.GetComponent<RandomNubers>();
        randomNubers.OnGenerateAngles += ComponentAnglesOrder;
        randomNubers.OnGenerateScenes += ComponentAScenesOrder;
        
        HeadLocation location = headLocation.GetComponent<HeadLocation>();
        location.OnPositionSampled += SaveDataRow;
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
        SceneManager();
    }

    private void SceneManager()
    {
        AskForStartAngle?.Invoke(this, EventArgs.Empty);
        waitingForEndPosition = false;
        if (!waitingForEndPosition)
        {
            ButterflyLocation(NowAngles, startAngle);
        }
    }

    //private void HandlePositionSampled(object sender, HeadLocation.PositionSampledEventArgs e)
    //{
    //    SaveDataRow(e);
    //}

    private void ButterflyLocation(int[] anglesList, float startAngle)
    {
        float tempButterAngle = anglesList[currentAngleIndex-1] ; // Assuming this is correct, please verify
        string currentDirection = "right"; // Replace with actual direction logic

        float tempButterAngleRefSP;
        if (currentDirection == "right")
        {
            tempButterAngleRefSP = startAngle + tempButterAngle;
        }
        else
        {
            tempButterAngleRefSP = startAngle - tempButterAngle;
        }

        if (tempButterAngleRefSP < 0)
        {
            tempButterAngleRefSP += 360;
        }

        float tempButterAngleRefSPRad = tempButterAngleRefSP * Mathf.Deg2Rad;
        float radius = 1.5f;
        float x = radius * Mathf.Cos(tempButterAngleRefSPRad);
        float y = radius * Mathf.Sin(tempButterAngleRefSPRad);
        float z = transform.position.z; // Ensure this is the correct position reference

        // Set the new position
        Vector3 newPosition = new Vector3(x, y, z);
        transform.position = newPosition;

        // Trigger the event
        ButterflyAngleUpdated?.Invoke(this, new ButterEventArgs(tempButterAngleRefSP));

        SaveData = true;
    }


    private void SaveDataRow(object sender, HeadLocation.PositionSampledEventArgs e)
    {
        Vector3 startPosition = e.StartPosition;
        float startPositionAngle = e.StartAngle;
        Vector3 currentPosition = e.CurrentPosition;
        float currentPositionAngle = e.CurrentAngle;
        float currentTime = e.CurrentTime;
        float timeDifference = e.TimeDifference;
        float currentPositionAngleRefButter = tempButterAngle - currentPositionAngle;
        float totalTime = (float)(DateTime.Now - startTime).TotalSeconds;

        lastAddedRow = new TableRow(
            currentScreneName,
            currentSceneIndex + 1,
            currentAngleIndex,
            NowAngles[currentAngleIndex],
            (int)tempButterAngleRefSP,
            direction[currentAngleIndex],
            startPosition,
            startPositionAngle,
            currentPosition,
            currentPositionAngle,
            currentPositionAngleRefButter,
            totalTime,
            timeDifference
        );

        DataTable.Add(lastAddedRow);

        if (!SaveData)
        {
            currentAngleIndex++;
            BeginNextProcess();
        }
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
        switch (current[0])
        {
            case 1:
                currentScreneName = "empty square room";
                break;
            case 2:
                currentScreneName = "furnished square room";
                break;
            case 3:
                currentScreneName = "empty round room";
                break;
            case 4:
                currentScreneName = "furnished round room";
                break;
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
        else
        {
            waitingForEndPosition = true;
            SceneManager();
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
                writer.WriteLine($"Scene: {row.screneName}, Sence Order(1-4): {row.screneOrder}, Task Order(1-6): {row.Taskorder}, Relative butterfly angle: {row.butterAngle}, Direction: {row.direction},Butterfly angle in space: {row.butterAngleRefStartPosition}, Start Position Vector: {row.startPosition}, Start Position Angle Of The Task: {row.startPositionAngle}, Cuurent Position Vector: {row.currentPosition},  Cuurent Position Angle: {row.startPositionAngle}, Angle Relative To Butterfly Position: {row.currentPositionAngleRefButter}, Time: {row.TotTime}, Time From The Start Of The Task: {row.TimeRefTask}");
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