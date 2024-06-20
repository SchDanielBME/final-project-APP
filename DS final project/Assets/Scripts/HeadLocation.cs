using System;
using UnityEngine;

public class HeadLocation : MonoBehaviour
{
    [SerializeField] private GameObject data;

    public event EventHandler OnSaveEndLocation;
    public event EventHandler<PositionSampledEventArgs> OnPositionSampled;

    private Vector3 startPosition;
    private bool shouldUpdatePosition = false;
    private float updateInterval = 1f / 8f; // 8Hz
    private float nextUpdateTime = 0f;
    private float startAngle;
    private float startTime;
    private float currentButterflyAngle;
    private float angleMatchStartTime = 0f;
    private bool isAngleMatching = false;
    private int matchingSamples = 0;

    public class PositionSampledEventArgs : EventArgs
    {
        public Vector3 StartPosition { get; }
        public float StartAngle { get; }
        public Vector3 CurrentPosition { get; }
        public float CurrentAngle { get; }
        public float CurrentTime { get; }
        public float TimeDifference { get; }

        public PositionSampledEventArgs(Vector3 startPosition, float startAngle, Vector3 currentPosition, float currentAngle, float currentTime, float timeDifference)
        {
            StartPosition = startPosition;
            StartAngle = startAngle;
            CurrentPosition = currentPosition;
            CurrentAngle = currentAngle;
            CurrentTime = currentTime;
            TimeDifference = timeDifference;
        }
    }

    private void Start()
    {
        Data dataInfo = data.GetComponent<Data>();
        if (dataInfo != null)
        {
            dataInfo.AskForStartAngle += TakeStartPose;
            dataInfo.StartToSample += StartSampling;
            dataInfo.ButterflyAngleUpdated += UpdateButterflyAngle;
        }
    }

    private void OnEnable()
    {
        Data dataInfo = data.GetComponent<Data>();
        if (dataInfo != null)
        {
            dataInfo.StartToSample += StartSampling;
            dataInfo.ButterflyAngleUpdated += UpdateButterflyAngle;
        }
    }

    private void OnDisable()
    {
        Data dataInfo = data.GetComponent<Data>();
        if (dataInfo != null)
        {
            dataInfo.StartToSample -= StartSampling;
            dataInfo.ButterflyAngleUpdated -= UpdateButterflyAngle;
        }
    }

    private void StartSampling(object sender, EventArgs e)
    {
        shouldUpdatePosition = true;
        nextUpdateTime = Time.time;
    }

    private void TakeStartPose(object sender, EventArgs e)
    {
        startPosition = Camera.main.transform.forward;
        startTime = Time.time;
        startAngle = AngleFromVector(Vector3.zero, startPosition);
        Data.startAngle = startAngle;
        Data.SaveData = true;
        shouldUpdatePosition = true;
        nextUpdateTime = Time.time;
    }

    private void Update()
    {
        if (shouldUpdatePosition && Time.time >= nextUpdateTime)
        {
            Vector3 currentPosition = Camera.main.transform.forward;
            float currentAngle = AngleFromVector(Vector3.zero, currentPosition);
            float currentTime = Time.time;
            float timeDifference = currentTime - startTime;

            if (Data.SaveData)
            {
                OnPositionSampled?.Invoke(this, new PositionSampledEventArgs(
                     startPosition,
                     startAngle,
                     currentPosition,
                     currentAngle,
                     currentTime,
                     timeDifference
                 ));
            }

            if (Mathf.Abs(currentAngle - currentButterflyAngle) <= 3f)
            {
                matchingSamples++;
                if (matchingSamples >= 16)
                {
                    Data.SaveData = false;
                }
            }
            else
            {
                matchingSamples = 0;
            }

            nextUpdateTime = Time.time + updateInterval;
        }
    }

    private void UpdateButterflyAngle(object sender, Data.ButterEventArgs e)
    {
        currentButterflyAngle = e.ButterAngle;
    }

    private float AngleFromVector(Vector3 direction1, Vector3 direction2)
    {
        direction1.y = 0;
        direction2.y = 0;
        float angleInXZPlane = Vector3.SignedAngle(direction1, direction2, Vector3.up);

        if (angleInXZPlane < 0)
        {
            angleInXZPlane += 360;
        }
        return angleInXZPlane;
    }
}
