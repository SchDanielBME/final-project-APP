using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
public class HeadLocation : MonoBehaviour
{
    [SerializeField] private GameObject data;
    public event EventHandler<PoseEventArgs> OnPoseCaptured;
    public event EventHandler OnSaveEndLoction;
    private Vector3 startPosition;
    private Vector3 endPosition;
    private float StartAngle; 
    private float startTime;
    private float endTime;
    private float realAngleInXZPlane;
    private float angleError;
    [SerializeField] private int angle;

    public class PoseEventArgs : EventArgs
    {
        public Vector3 StartPosition { get; private set; }
        public Vector3 EndPosition { get; private set; }
        public float Result { get; private set; }
        public float ElapsedTime { get; private set; }
        public float Time { get; private set; }
        public float ErrorTime { get; private set; }


        public PoseEventArgs(Vector3 startPosition, float angleResult, float elapsedTime,float time, float angleError)
        {
            StartPosition = startPosition;
            Result = angleResult;
            ElapsedTime = elapsedTime;
            Time = time;
            ErrorTime = angleError;
        }
    }
    

    void Start()
    {
        Data dataInfo = data.GetComponent<Data>();
        if (dataInfo != null)
        {
            dataInfo.AskForStartAngle += TakeStartPose;
            dataInfo.OnTaskButtonClicked += TakeStartPose;
            dataInfo.OnStopButtonClicked += TakeEndPose;
            dataInfo.OnCurentAngle += GetCurentAngle; ;
        }
    }

    private void TakeStartPose(object sender, EventArgs e)
    {
        startPosition = Camera.main.transform.forward;
        startTime = Time.time;
        StartAngle = AngleFromVector(Vector3.zero, startPosition);
        Data.startAngle = StartAngle;
    }

    private float AngleFromVector(Vector3 direction1, Vector3 direction2)
    {
        direction1.y = 0;
        direction2.y = 0;
        float AngleInXZPlane = Vector3.SignedAngle(direction1, direction2, Vector3.up);

        if (AngleInXZPlane < 0)
        {
            AngleInXZPlane += 360;
        }
        return AngleInXZPlane;
    }
    private void TakeEndPose(object sender, EventArgs e)
    {
        endPosition = Camera.main.transform.forward;
        endTime = Time.time;
        float elapsedTime = endTime - startTime;
        OnSaveEndLoction?.Invoke(this, EventArgs.Empty);

        Vector3 startDirection = new Vector3(startPosition.x, 0, startPosition.z);
        Vector3 endDirection = new Vector3(endPosition.x, 0, endPosition.z);
        realAngleInXZPlane = Vector3.SignedAngle(startDirection, endDirection, Vector3.up);

        if (realAngleInXZPlane < 0)
        {
            realAngleInXZPlane += 360;
        }
        angleError = Mathf.Abs(angle - realAngleInXZPlane);

        OnPoseCaptured?.Invoke(this, new PoseEventArgs(startPosition, realAngleInXZPlane, elapsedTime, startTime, angleError));
        startTime = 0;
        endTime = 0;
    }

    private void GetCurentAngle(object sender, Data.AngleEventArgs en)
    {
        angle = en.Angle;

    }
}
