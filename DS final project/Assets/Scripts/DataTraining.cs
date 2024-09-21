using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class DataTraining : MonoBehaviour
{
    private int[] angels = {250, 275};
    private string[] direction = { "right", "left"};
    [SerializeField] private List<TableRow> DataTable = new List<TableRow>();


    private int[] FirstAngles;
    private int[] SecondAngles;
    private int[] ThirdAngles;
    private int[] FourthAngles;
    private int[] ScenesOreder;
    private int[][] AllTheAngles;
    private int[] NowAngles;
    [SerializeField] private int[] current = { 0, 0 };
    private string[] ScenesNames = { "E", "H"};
    private string currentScreneName;
    private string currentScreneName2Show;
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
    public static float startCenterX = 0;
    public static float startCenterZ = 0;
    public static float startCenterY = 0;
    float tempButterAngle;
    float tempButterAngleRefSP;
    public static bool SaveData = false;
    private bool waitingForEndPosition = true;
    private float butterHigh = 1.2f;
    public long unixStartTime = 0;

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
        public float butterAngleInSpace;
        public string direction;
        public Vector3 startPosition;
        public float startPositionAngle;
        public Vector3 currentPosition;
        public float currentPositionAngle;
        public float currentPositionAngleRefButter;
        public char InFOV;
        public char IsMarker;
        public string SampTime;
        public float TotTime;
        public float TimeRefTask;

        public TableRow(string screneName, int screneIndex, int taskIndex, float butterAngleRefSP, float butterAngleInSpace, string direction,
           Vector3 startPosition, float startPositionAngle, Vector3 currentPosition, float currentPositionAngle, float currentPositionAngleRefButter,
           char inFOV, char isMarker, string sampleTime, float totTime, float TimeRefTask)
        {
            this.screneName = screneName;
            this.screneIndex = screneIndex;
            this.TaskIndex = taskIndex;
            this.butterAngleRefStartPosition = butterAngleRefSP; // butterfly angle 
            this.butterAngleInSpace = butterAngleInSpace; // butterfly angle
            this.direction = direction; //right or left
            this.startPosition = startPosition;
            this.startPositionAngle = startPositionAngle;
            this.currentPosition = currentPosition;
            this.currentPositionAngle = currentPositionAngle;
            this.currentPositionAngleRefButter = currentPositionAngleRefButter;
            this.InFOV = inFOV; this.IsMarker = isMarker;
            this.TotTime = totTime; // from the beginning
            this.SampTime = sampleTime;
            this.TimeRefTask = TimeRefTask;
        }
    }
    void Start()
    {
        startTime = DateTime.Now;
        int currentMinutes = startTime.Minute;
        int currentSeconds = startTime.Second;
        int timeMarker = currentMinutes * 100 + currentSeconds;
        markerStream.Write(timeMarker);

        outputFileName = "Data_Training" + startTime.ToString("yyyy MM dd _ HH mm ss fff") + ".txt";
        filePath = Path.Combine(Application.persistentDataPath, outputFileName);

        unixStartTime = new DateTimeOffset(startTime).ToUnixTimeSeconds();
        using (StreamWriter writer = new StreamWriter(filePath))
        {
            writer.WriteLine($"unix time at the beginning of the experiment is: {unixStartTime}");
        }

        thanksButton.onClick.AddListener(ThanksButtonClicked);
        newScrenePanel.SetActive(false);
        thanksPanel.SetActive(false);
        diractionPanel.SetActive(false);
        rightText.SetActive(false);
        leftText.SetActive(false);

        RandomNubersTraining randomNubers = numberRandomizer.GetComponent<RandomNubersTraining>();
        randomNubers.OnGenerateAngles += ComponentAnglesOrder;
        randomNubers.OnGenerateScenes += ComponentAScenesOrder;

        HeadLocationTraining location = headLocation.GetComponent<HeadLocationTraining>();
        location.OnPositionSampled += SaveDataRow;
        location.OnSameLocation += SceneManager2;
        location.ButterHighCalculated += OnButterHighCalculated;

        markerStream = FindObjectOfType<CustomLSLMarkerStream>();
        if (markerStream == null)
        {
            Debug.LogError("CustomLSLMarkerStream component not found in the scene. Please add it to a GameObject.");
        }
    }
    private void ComponentAnglesOrder(object sender, RandomNubersTraining.AngelsEventArgs e)
    {
        FirstAngles = e.FirstAngles;
        SecondAngles = e.SecondAngles;

        AllTheAngles = new int[][] { FirstAngles, SecondAngles};
    }

    private void OnButterHighCalculated(object sender, HeadLocationTraining.ButterHighEventArgs e)
    {
        butterHigh = e.ButterHigh;
    }

    private void ComponentAScenesOrder(object sender, RandomNubersTraining.ScenesEventArgs s)
    {
        ScenesOreder = s.Order;
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
        ScreneText.text = "You are now " + currentScreneName + " let's start catching butterflies";
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
        if (!waitingForEndPosition)
        {
            ButterflyLocation(NowAngles, direction, startAngle);
            butterfly.SetActive(true);

            int sceneIndex = ScenesOreder[currentSceneIndex];
            int angleIndex = NowAngles[currentAngleIndex];
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
            int endMarker = sceneIndex * 100000 + angleIndex * 10000 + 1;
            markerStream.Write(endMarker);

            butterfly.SetActive(false);
            currentAngleIndex++;
            BeginNextProcess();
        }
    }


    private void ButterflyLocation(int[] anglesList, string[] direction, float startAngle)
    {

        float tempButterAngle = angels[anglesList[currentAngleIndex] - 1]; // Assuming this is correct, please verify
        string currentDirection = direction[anglesList[currentAngleIndex] - 1];
        float tempButterAngleRefSP;

        float tempPanelAngle = startAngle * Mathf.Deg2Rad;
        float radius = 3f;
        float xP = radius * Mathf.Sin(tempPanelAngle) + startCenterX;
        float yP = startCenterY + 0.3f;
        float zP = radius * Mathf.Cos(tempPanelAngle) + startCenterZ;
        Vector3 PanelPosition = new Vector3(xP, yP, zP);
        diractionPanel.transform.position = PanelPosition;

        Vector3 directionToCenter = new Vector3(startCenterX, startCenterY, startCenterZ) - PanelPosition;
        Quaternion panelRotation = Quaternion.LookRotation(directionToCenter);
        diractionPanel.transform.rotation = panelRotation;

        if (currentDirection == "right")
        {
            tempButterAngleRefSP = startAngle + tempButterAngle;
            diractionPanel.SetActive(true);
            rightText.SetActive(true);
            leftText.SetActive(false);
            markerStream.Write(22222); // panel right on

        }
        else
        {
            tempButterAngleRefSP = startAngle - tempButterAngle;
            diractionPanel.SetActive(true);
            leftText.SetActive(true);
            rightText.SetActive(false);
            markerStream.Write(33333); // panel left on
        }

        Debug.Log("direction: " + currentDirection);

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
        float radius2 = 3f;
        float x = radius2 * Mathf.Sin(tempButterAngleRefSPRad) + startCenterX;
        float y = butterHigh - 0.1f;
        float z = radius * Mathf.Cos(tempButterAngleRefSPRad) + startCenterZ;  // Ensure this is the correct position reference

        Vector3 newPosition = new Vector3(x, y, z);
        butterfly.transform.position = newPosition;

        ButterflyAngleUpdated?.Invoke(this, new ButterEventArgs(tempButterAngleRefSP));
        SaveData = true;
    }

    private IEnumerator TurnOffDirectionPanel()
    {
        yield return new WaitForSeconds(3);
        diractionPanel.SetActive(false);
        rightText.SetActive(false);
        leftText.SetActive(false);
        markerStream.Write(44444); //panel off
    }

    private void SaveDataRow(object sender, HeadLocationTraining.PositionSampledEventArgs e)
    {
        Vector3 startPosition = e.StartPosition;
        float startPositionAngle = e.StartAngle;
        Vector3 currentPosition = e.CurrentPosition;
        float currentPositionAngle = e.CurrentAngle;
        float currentButterflyAngle = e.CurrentButterflyAngle;
        char inFOV = e.InFOV;
        char isMarker = e.IsMarker;
        DateTime currentTimeAndDate = e.CurrentTime;
        string currentTime = currentTimeAndDate.ToString("HH:mm:ss");
        float timeDifference = e.TimeDifference;
        float currentPositionAngleRefButter = currentButterflyAngle - currentPositionAngle;
        float totalTime = (float)(DateTime.Now - startTime).TotalSeconds;

        lastAddedRow = new TableRow(
            currentScreneName,
            currentSceneIndex + 1,
            currentAngleIndex + 1,
            angels[NowAngles[currentAngleIndex] - 1],
            currentButterflyAngle,
            direction[NowAngles[currentAngleIndex] - 1],
            startPosition,
            startPositionAngle,
            currentPosition,
            currentPositionAngle,
            currentPositionAngleRefButter,
            inFOV,
            isMarker,
            currentTime,
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
                currentScreneName2Show = "erea 1";
                currentScreneName = "between the ancient city walls"; //
                break;
            case 2:
                currentScreneName2Show = "erea 2";
                currentScreneName = "in the ancient city"; //
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
        using (StreamWriter writer = new StreamWriter(filePath, true))
        {
            foreach (TableRow row in DataTable)
            {
                writer.WriteLine($"Scene: {row.screneName}, Scene Index: {row.screneIndex}, Task Index: {row.TaskIndex}, Relative butterfly angle: {row.butterAngleRefStartPosition}, Direction: {row.direction},Butterfly angle in space: {row.butterAngleInSpace}, Start Position Vector: {row.startPosition}, Start Position Angle Of The Task: {row.startPositionAngle}, Current Position Vector: {row.currentPosition},  Current Position Angle: {row.currentPositionAngle}, Angle Relative To Butterfly Position: {row.currentPositionAngleRefButter}, Is butterfly in FOV? {row.InFOV}, Is there a marker? {row.IsMarker}, Time: {row.TotTime}, Current Time: {row.SampTime}, Time From The Start Of The Task: {row.TimeRefTask}");
            }
        }

        DataTable.Clear();

        Debug.Log("Tables saved to file: " + filePath);
    }


    private void EndGame()
    {
        markerStream.Write(99999);
        UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("Main Scene");
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