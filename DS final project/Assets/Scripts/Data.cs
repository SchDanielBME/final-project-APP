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
    [SerializeField] private GameObject diractionPanel;
    [SerializeField] private GameObject rightText;
    [SerializeField] private GameObject leftText;
    [SerializeField] private Button ScreneButton;
    [SerializeField] private Button thanksButton;
    [SerializeField] private GameObject butterfly;
    [SerializeField] private GameObject leftController;
    [SerializeField] private GameObject rightController;

    private LineRenderer leftLaser;
    private LineRenderer rightLaser;

    [SerializeField] private int currentAngleIndex = 0;
    [SerializeField] private int currentSceneIndex = 0;
    public static float startAngle = 0;
    float tempButterAngle;
    float tempButterAngleRefSP;
    public static bool SaveData = false;
    private bool waitingForEndPosition = true;
    private float butterHigh = 1.2f;

    private Vector3 tempStartPosition;
    private float tempElapsedTime;
    private float tempAngleError;
    private float tempResult;
    private float tempStartTime;
    private TableRow lastAddedRow;

    private CustomLSLMarkerStream markerStream;

    public event EventHandler <CurrentEventArgs> OnCurentScenes;
    public class CurrentEventArgs : EventArgs
    {
        public int CurrentSceneNum { get; }
        public CurrentEventArgs(int SceneNum)
        {
            CurrentSceneNum = SceneNum;
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
    

    [System.Serializable] 
    public class TableRow
    {
        public string screneName;
        public int screneIndex;
        public int TaskIndex;
        public float butterAngleRefStartPosition;
        public float butterAngle;
        public string direction;
        public Vector3 startPosition;
        public float startPositionAngle;
        public Vector3 currentPosition;
        public float currentPositionAngle;
        public float currentPositionAngleRefButter;
        public float TotTime;
        public float TimeRefTask;

        public TableRow(string screneName, int screneIndex, int taskIndex, float butterAngleRefSP, float butterAngle, string direction,
            Vector3 startPosition, float startPositionAngle, Vector3 currentPosition, float currentPositionAngle, float currentPositionAngleRefButter, float totTime, float TimeRefTask)
        {
            this.screneName = screneName;
            this.screneIndex = screneIndex;
            this.TaskIndex = taskIndex; 
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
        thanksButton.onClick.AddListener(ThanksButtonClicked);
        newScrenePanel.SetActive(false);
        thanksPanel.SetActive(false);
        diractionPanel.SetActive(false);
        rightText.SetActive(false);
        leftText.SetActive(false);

        RandomNubers randomNubers = numberRandomizer.GetComponent<RandomNubers>();
        randomNubers.OnGenerateAngles += ComponentAnglesOrder;
        randomNubers.OnGenerateScenes += ComponentAScenesOrder;
        
        HeadLocation location = headLocation.GetComponent<HeadLocation>();
        location.OnPositionSampled += SaveDataRow;
        location.OnSameLocation += SceneManager2;
        location.ButterHighCalculated += OnButterHighCalculated;

        markerStream = FindObjectOfType<CustomLSLMarkerStream>();
        if (markerStream == null)
        {
            Debug.LogError("CustomLSLMarkerStream component not found in the scene. Please add it to a GameObject.");
        }
    }
    private void ComponentAnglesOrder(object sender, RandomNubers.AngelsEventArgs e)
    {
        FirstAngles = e.FirstAngles;
        SecondAngles = e.SecondAngles;
        ThirdAngles = e.ThirdAngles;
        FourthAngles = e.FourthAngles;

        AllTheAngles = new int[][] { FirstAngles, SecondAngles, ThirdAngles, FourthAngles };
        Debug.Log("Angles Order called");
    }

    private void OnButterHighCalculated(object sender, HeadLocation.ButterHighEventArgs e)
    {
        butterHigh = e.ButterHigh;
    }

    private void ComponentAScenesOrder(object sender, RandomNubers.ScenesEventArgs s)
    {
        ScenesOreder = s.Order;
        Debug.Log("Scenes Order called");

        StartToSample?.Invoke(this, EventArgs.Empty);
        TopManager();
    }
    private void TopManager()
    {
        NowAngles = AllTheAngles[currentSceneIndex];
        current[0] = ScenesOreder[currentSceneIndex];
        ChangeSceneName();
        OnCurentScenes?.Invoke(this, new CurrentEventArgs(ScenesOreder[currentSceneIndex]));
        newScrenePanel.SetActive(true);
        EnableLasers();
        ScreneText.text = "You are now in the " + currentScreneName + " let's start catching butterflies";
        ScreneButton.onClick.AddListener(OnScreneButtonClicked);
    }

    private void OnScreneButtonClicked()
    {
        newScrenePanel.SetActive(false);
        DisableLasers();
        SceneManager();
    }

    private void SceneManager()
    {
        AskForStartAngle?.Invoke(this, EventArgs.Empty);
        waitingForEndPosition = false;
        Debug.Log($"SceneManager IN");
        if (!waitingForEndPosition)
        {
            ButterflyLocation(NowAngles, direction, startAngle);
            butterfly.SetActive(true);

            int sceneIndex = ScenesOreder[currentSceneIndex];
            int angleIndex = NowAngles [currentAngleIndex];
            int startMarker = sceneIndex * 100000 + angleIndex * 10000;
            markerStream.Write(startMarker);
        }
    }

    private void SceneManager2(object sender, EventArgs e)
    {
        if (!SaveData)
        {
            int sceneIndex = ScenesOreder[currentSceneIndex];
            int angleIndex = NowAngles[currentAngleIndex];
            int endMarker = sceneIndex * 100000 + angleIndex * 10000 + 1000;
            markerStream.Write(endMarker);

            butterfly.SetActive(false);
            currentAngleIndex++;
            BeginNextProcess();
        }
    }

    private void ButterflyLocation(int[] anglesList, string[] direction, float startAngle)
    {
      
        float tempButterAngle = angels[anglesList[currentAngleIndex]-1]; // Assuming this is correct, please verify
        string currentDirection = direction[anglesList[currentAngleIndex] - 1];

        float tempButterAngleRefSP;
        if (currentDirection == "right")
        {
            tempButterAngleRefSP = startAngle + tempButterAngle;
            diractionPanel.SetActive(true);
            rightText.SetActive(true);
            markerStream.Write(22222222); // panel right on

        }
        else
        {
            tempButterAngleRefSP = startAngle - tempButterAngle;
            diractionPanel.SetActive(true);
            leftText.SetActive(true);
            markerStream.Write(33333333); // panel left on
        }

        StartCoroutine(TurnOffDirectionPanel());

        if (tempButterAngleRefSP < 0)
        {
            tempButterAngleRefSP += 360;
        }

        if (tempButterAngleRefSP > 360)
        {
            tempButterAngleRefSP -= 360;
        }

        float tempButterAngleRefSPRad = tempButterAngleRefSP * Mathf.Deg2Rad;
        float radius = 2.5f;
        float x = radius * Mathf.Sin(tempButterAngleRefSPRad);
        float y = butterHigh - 0.1f;
        float z = radius * Mathf.Cos(tempButterAngleRefSPRad) - 10.5f;  // Ensure this is the correct position reference

        Vector3 newPosition = new Vector3(x, y, z);
        butterfly.transform.position = newPosition;

        ButterflyAngleUpdated?.Invoke(this, new ButterEventArgs(tempButterAngleRefSP));
        Debug.Log($"butterfly angle is: {tempButterAngleRefSP}");
        SaveData = true;
    }

    private IEnumerator TurnOffDirectionPanel()
    {
        yield return new WaitForSeconds(3);
        diractionPanel.SetActive(false);
        rightText.SetActive(false);
        leftText.SetActive(false);
        markerStream.Write(44444444); //panel off

    }

    private void SaveDataRow(object sender, HeadLocation.PositionSampledEventArgs e)
    {
        Vector3 startPosition = e.StartPosition;
        float startPositionAngle = e.StartAngle;
        Vector3 currentPosition = e.CurrentPosition;
        float currentPositionAngle = e.CurrentAngle;
        float currentButterflyAngle = e.CurrentButterflyAngle;
        float currentTime = e.CurrentTime;
        float timeDifference = e.TimeDifference;
        float currentPositionAngleRefButter = currentButterflyAngle - currentPositionAngle;
        float totalTime = (float)(DateTime.Now - startTime).TotalSeconds;

        lastAddedRow = new TableRow(
            currentScreneName,
            currentSceneIndex + 1,
            currentAngleIndex +1,
            angels[NowAngles[currentAngleIndex]-1],
            currentButterflyAngle,
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
                EnableLasers();
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
                writer.WriteLine($"Scene: {row.screneName}, Sence Index: {row.screneIndex}, Task Index: {row.TaskIndex}, Relative butterfly angle: {row.butterAngle}, Direction: {row.direction},Butterfly angle in space: {row.butterAngleRefStartPosition}, Start Position Vector: {row.startPosition}, Start Position Angle Of The Task: {row.startPositionAngle}, Cuurent Position Vector: {row.currentPosition},  Cuurent Position Angle: {row.startPositionAngle}, Angle Relative To Butterfly Position: {row.currentPositionAngleRefButter}, Time: {row.TotTime}, Time From The Start Of The Task: {row.TimeRefTask}");
            }
        }

        Debug.Log("Tables saved to file: " + filePath);
    }

    private void EndGame()
    {
        Debug.Log("Game ended.");
        Application.Quit();
    }

    private void EnableLasers()
    {
        rightController.SetActive(true);
        leftController.SetActive(true);
    }

    private void DisableLasers()
    {
        rightController.SetActive(false);
        leftController.SetActive(false);
    }

}