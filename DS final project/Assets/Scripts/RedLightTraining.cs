
using UnityEngine;
using UnityEngine.UI;
using System.Collections;



public class RedLightTraining : MonoBehaviour
{
    [SerializeField] private GameObject yellowLight;
    [SerializeField] private GameObject redLight;
    [SerializeField] private GameObject SpinFinePanel;
    [SerializeField] private Button SpinFineButton;
    [SerializeField] private GameObject NextTextPanel;
    [SerializeField] private Button NextTextButton;
    [SerializeField] private GameObject BackroundPanel;
    [SerializeField] private GameObject leftController;
    [SerializeField] private GameObject rightController;


    private CustomLSLMarkerStream markerStream;

    private float updateInterval = 1f / 50f;
    private float nextUpdateTime = 0f;
    private Vector3 currentPosition;
    private float currentTime;
    private float currentAngle;
    private float lastAngle = 0;
    private float lastTime = 0;
    [SerializeField] private int speedExceedCounter = 0;
    [SerializeField] private int speedExceedCounter2 = 0;

    private const int speedThresholdSamples = 19;
    [SerializeField] private int redFlag = 0;
    [SerializeField] private int redFlag2 = 0;
    [SerializeField] private float angularVelocity = 0;


    void Start()
    {
        leftController.SetActive(false);
        rightController.SetActive(false);
        if (SpinFineButton != null)
        {
            SpinFineButton.onClick.AddListener(OnSpinFineButtonClick); // Corrected spelling
        }
        markerStream = FindObjectOfType<CustomLSLMarkerStream>();
        if (markerStream == null) Debug.LogError("CustomLSLMarkerStream component is not found in the scene.");
    }

    public void Update()
    {
        if (Time.time >= nextUpdateTime)
        {
            currentPosition = Camera.main.transform.forward;
            Vector3 D1 = new Vector3(0, 0, 1);
            currentAngle = AngleFromVector(D1, currentPosition);
            currentTime = Time.time;
     
         
            float angleDifference = Mathf.Abs(currentAngle - lastAngle);
            float timeDiff = currentTime - lastTime;
            angularVelocity = angleDifference / timeDiff; 

            
            if (angularVelocity > 17f)
            {
                speedExceedCounter++;
                if (speedExceedCounter > speedThresholdSamples)
                {
                    redLight.SetActive(true);
                    yellowLight.SetActive(false);
                    if (speedExceedCounter == speedThresholdSamples + 1)
                    {
                        markerStream.Write(55555);
                        redFlag = 1;
                        redFlag2 = 1;
                        Debug.Log("redFlag = 1");
                    }
                }
            }
            else
            {
                speedExceedCounter = 0;
                redLight.SetActive(false);
                yellowLight.SetActive(true);
                if (redFlag2 == 1)
                {
                    if (angularVelocity > 0.2f)
                    {
                        speedExceedCounter2++;
                    }
                    Debug.Log("In2");
                    if (redFlag == 1)
                    {
                        markerStream.Write(66666);
                        redFlag = 0;
                    }
                    if (speedExceedCounter2 > 2.5 * speedThresholdSamples)
                    {
                        redFlag2 = 0;
                        StartCoroutine(ShowSpinFinePanelAfterDelay(1));
                    }
                }
            }

            lastAngle = currentAngle;
            lastTime = currentTime;

            nextUpdateTime = Time.time + updateInterval;
        }
    }
    private float AngleFromVector(Vector3 direction1, Vector3 direction2)
    {
        direction2.y = 0;

        float angleInXZPlane = Vector3.SignedAngle(direction1, direction2, Vector3.up);

        if (angleInXZPlane < 0)
        {
            angleInXZPlane += 360;
        }
        return angleInXZPlane;
    }
    private void OnSpinFineButtonClick()
    {
        SpinFinePanel.SetActive(false);
        SpinFineButton.gameObject.SetActive(false);
        NextTextPanel.SetActive(true);
        NextTextButton.gameObject.SetActive(true);
        this.enabled = false; 
    }
    private IEnumerator ShowSpinFinePanelAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        BackroundPanel.SetActive(true);
        SpinFinePanel.SetActive(true);
        SpinFineButton.gameObject.SetActive(true);
        leftController.SetActive(true);
        rightController.SetActive(true);
    }

}
