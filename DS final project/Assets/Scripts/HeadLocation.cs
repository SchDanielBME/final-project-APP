using System;
using UnityEngine;

public class HeadLocation : MonoBehaviour
{
    [SerializeField] private GameObject data;

    public event EventHandler<PositionSampledEventArgs> OnPositionSampled;
    public event EventHandler OnSameLocation;


    private Vector3 startPosition;
    private Vector3 currentPosition;
    private float currentTime;
    private float currentAngle;
    private bool shouldUpdatePosition = false;
    private float updateInterval = 1f / 8f; // 8Hz
    private float nextUpdateTime = 0f;
    private float startTime;
    private float currentButterflyAngle;
    private int matchingSamples = 0;

    public class PositionSampledEventArgs : EventArgs
    {
        public Vector3 StartPosition { get; }
        public float StartAngle { get; }
        public Vector3 CurrentPosition { get; }
        public float CurrentAngle { get; }
        public float CurrentTime { get; }
        public float TimeDifference { get; }
        public float CurrentButterflyAngle { get; }


        public PositionSampledEventArgs(Vector3 startPosition, float startAngle, Vector3 currentPosition, float currentAngle, float currentButterflyAngle, float currentTime, float timeDifference)
        {
            StartPosition = startPosition;
            StartAngle = startAngle;
            CurrentPosition = currentPosition;
            CurrentAngle = currentAngle;
            CurrentButterflyAngle = currentButterflyAngle;
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
        if (!Data.SaveData)
        {
            startPosition = currentPosition;
            //Debug.Log($"start position: {startPosition}");
            startTime = currentTime;
            Data.startAngle = currentAngle;
            //Debug.Log($"start angle is: {Data.startAngle}");
        }
        Data.SaveData = true;
    }

    private void Update()
    {
        if (shouldUpdatePosition && Time.time >= nextUpdateTime)
        {
            currentPosition = Camera.main.transform.forward;
            //Debug.Log($"current position: {currentPosition}");
            Vector3 D1 = new Vector3(0,0,1);
            currentAngle = AngleFromVector(D1, currentPosition);
            //Debug.Log($"current angle: {currentAngle}");
            currentTime = Time.time;
            float timeDifference = currentTime - startTime;

            if (Data.SaveData)
            {
                OnPositionSampled?.Invoke(this, new PositionSampledEventArgs(
                     startPosition,
                     Data.startAngle,
                     currentPosition,
                     currentAngle,
                     currentButterflyAngle,
                     currentTime,
                     timeDifference
                 ));

                if (Mathf.Abs(currentAngle - currentButterflyAngle) <= 4f)
                {
                    matchingSamples++;
                    //Debug.Log($"matching Samples: {matchingSamples}");
                    if (matchingSamples >= 16)
                    {
                        matchingSamples = 0;
                        //Debug.Log($"matching Samples: {matchingSamples}");
                        Data.SaveData = false;
                        OnSameLocation?.Invoke(this, EventArgs.Empty);
                    }
                }
                else
                {
                    matchingSamples = 0;
                }
            }

            nextUpdateTime = Time.time + updateInterval;
        }
    }

    private void UpdateButterflyAngle(object sender, Data.ButterEventArgs e)
    {
        currentButterflyAngle = e.ButterAngle;
        //Debug.Log($"current BUTTER angle: {currentButterflyAngle}");
    }

    private float AngleFromVector(Vector3 direction1, Vector3 direction2)
    {
        direction2.y = 0;

        float angleInXZPlane = Vector3.SignedAngle(direction1, direction2, Vector3.up);

        if (angleInXZPlane < 0)
        {
            angleInXZPlane += 360;
        }
        //Debug.Log($"angle In XZ: {angleInXZPlane}");
        return angleInXZPlane;
    }
}
