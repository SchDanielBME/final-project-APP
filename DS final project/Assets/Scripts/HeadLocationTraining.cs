using System;
using UnityEngine;


public class HeadLocationTraining : MonoBehaviour
{
    [SerializeField] private GameObject data;
    [SerializeField] private GameObject yellowLight;
    [SerializeField] private GameObject redLight;

    public event EventHandler<PositionSampledEventArgs> OnPositionSampled;
    public event EventHandler<ButterHighEventArgs> ButterHighCalculated;

    public event EventHandler OnSameLocation;


    private Vector3 startPosition;
    private Vector3 currentPosition;
    private Vector3 currentPosition2;
    private float butterHigh;
    private float currentTime;
    private float currentAngle;
    private char inFOV;
    private char isMarker;
    private bool shouldUpdatePosition = false;
    private float updateInterval = 1f / 50f; 
    private float nextUpdateTime = 0f;
    private float startTime;
    private float currentButterflyAngle;
    private int matchingSamples = 0;
    private int flagCenter = 0;
    private float lastAngle = 0;
    private float lastTime = 0;
    private int speedExceedCounter = 0;
    private const int speedThresholdSamples = 50;

    private CustomLSLMarkerStream markerStream;
    private float lastSentAngle = 0;
    public class ButterHighEventArgs : EventArgs
    {
        public float ButterHigh { get; }

        public ButterHighEventArgs(float butterHigh)
        {
            ButterHigh = butterHigh;
        }
    }
    public class PositionSampledEventArgs : EventArgs
    {
        public Vector3 StartPosition { get; }
        public float StartAngle { get; }
        public Vector3 CurrentPosition { get; }
        public float CurrentAngle { get; }
        public float CurrentTime { get; }
        public float TimeDifference { get; }
        public float CurrentButterflyAngle { get; }
        public char InFOV { get; }
        public char IsMarker { get; }


        public PositionSampledEventArgs(Vector3 startPosition, float startAngle, Vector3 currentPosition, float currentAngle, float currentButterflyAngle, char inFOV, char isMarker, float currentTime, float timeDifference)
        {
            StartPosition = startPosition;
            StartAngle = startAngle;
            CurrentPosition = currentPosition;
            CurrentAngle = currentAngle;
            CurrentButterflyAngle = currentButterflyAngle;
            InFOV = inFOV;
            IsMarker = isMarker;
            CurrentTime = currentTime;
            TimeDifference = timeDifference;
        }
    }
    private void Start()
    {
        DataTraining dataInfo = data.GetComponent<DataTraining>();
        if (dataInfo != null)
        {
            dataInfo.AskForStartAngle += TakeStartPose;
            dataInfo.StartToSample += StartSampling;
            dataInfo.ButterflyAngleUpdated += UpdateButterflyAngle;
        }

        markerStream = FindObjectOfType<CustomLSLMarkerStream>();
        if (markerStream == null)
        {
            Debug.LogError("CustomLSLMarkerStream component not found in the scene. Please add it to a GameObject.");
        }
    }

    private void OnEnable()
    {
        DataTraining dataInfo = data.GetComponent<DataTraining>();
        if (dataInfo != null)
        {
            dataInfo.StartToSample += StartSampling;
            dataInfo.ButterflyAngleUpdated += UpdateButterflyAngle;
        }
    }

    private void OnDisable()
    {
        if (data != null)
        {
            DataTraining dataInfo = data.GetComponent<DataTraining>();
            if (dataInfo != null)
            {
                dataInfo.StartToSample -= StartSampling;
                dataInfo.ButterflyAngleUpdated -= UpdateButterflyAngle;
            }
        }
    }

    private void StartSampling(object sender, EventArgs e)
    {
        shouldUpdatePosition = true;
        nextUpdateTime = Time.time;
        currentPosition2 = Camera.main.transform.position;
        butterHigh = currentPosition2.y;
        ButterHighCalculated?.Invoke(this, new ButterHighEventArgs(butterHigh));
    }

    private void TakeStartPose(object sender, EventArgs e)
    {
        if (!DataTraining.SaveData)
        {
            startPosition = currentPosition;
            startTime = currentTime;
            DataTraining.startAngle = currentAngle;
            if (flagCenter < 1)
            {
                Vector3 currentCenter = Camera.main.transform.position;
                Vector3 currentRotation = Camera.main.transform.rotation.eulerAngles;

                DataTraining.startCenterX = currentCenter.x;
                DataTraining.startCenterZ = currentCenter.z;
                DataTraining.startCenterY = currentCenter.y; ;
                flagCenter = 1;
            }
        }
        DataTraining.SaveData = true;
        flagCenter = 0;
    }

    private void Update()
    {
        if (shouldUpdatePosition && Time.time >= nextUpdateTime)
        {
            currentPosition = Camera.main.transform.forward;
            Vector3 D1 = new Vector3(0,0,1);
            currentAngle = AngleFromVector(D1, currentPosition);
            currentTime = Time.time;
            float timeDifference = currentTime - startTime;
            isMarker = 'X';

            if (Math.Abs(currentButterflyAngle - currentAngle) <= 60)
            {
                inFOV = 'V';
            }
            else { inFOV = 'X'; }

            if (Mathf.Abs(currentAngle - lastSentAngle) >= 3f)
            {
                double roundedAngle = Math.Round((double)currentAngle);
                SendAngleMarker((int)roundedAngle);
                lastSentAngle = currentAngle;
            }

            if (DataTraining.SaveData)
            {
                OnPositionSampled?.Invoke(this, new PositionSampledEventArgs(
                     startPosition,
                     DataTraining.startAngle,
                     currentPosition,
                     currentAngle,
                     currentButterflyAngle,
                     inFOV,
                     isMarker,
                     currentTime,
                     timeDifference
                 ));

                if (Mathf.Abs(currentAngle - currentButterflyAngle) <= 5f)
                {
                    matchingSamples++;
                    if (matchingSamples >= 50)
                    {
                        matchingSamples = 0;
                        DataTraining.SaveData = false;
                        OnSameLocation?.Invoke(this, EventArgs.Empty);
                    }
                }
                else
                {
                    matchingSamples = 0;
                }
            }


            // Calculate angular velocity
            float angleDifference = Mathf.Abs(currentAngle - lastAngle);
            float timeDiff = currentTime - lastTime;
            float angularVelocity = angleDifference / timeDiff; // Degrees per second

            // Convert angular velocity to km/h
            //float angularVelocityKmH = angularVelocity * 3600 / 1000;
            int redFlag = 0;
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
                    }
                }
            }
            else
            {
                speedExceedCounter = 0;
                redLight.SetActive(false);
                yellowLight.SetActive(true);
                if (redFlag == 1)
                {
                    markerStream.Write(66666);
                    redFlag = 0;
                }
            }

            lastAngle = currentAngle;
            lastTime = currentTime;

            nextUpdateTime = Time.time + updateInterval;
        }
    }

    private void UpdateButterflyAngle(object sender, DataTraining.ButterEventArgs e)
    {
        currentButterflyAngle = e.ButterAngle;
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

    private void SendAngleMarker(int angle)
    {
        if (markerStream != null)
        {
            markerStream.Write(angle);
        }
    }
}
