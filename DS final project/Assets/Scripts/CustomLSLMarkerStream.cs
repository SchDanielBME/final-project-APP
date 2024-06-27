using UnityEngine;
using LSL;
using System;

public class CustomLSLMarkerStream : MonoBehaviour
{
    private StreamOutlet outlet;
    private StreamInfo streamInfo;

    void Start()
    {
        // Define the stream info
        streamInfo = new StreamInfo("UnityMarkerStream", "Markers", 1, LSL.LSL.IRREGULAR_RATE, channel_format_t.cf_float32, "unique12345");

        // Create a new outlet
        outlet = new StreamOutlet(streamInfo);
    }

    public void Write(float marker)
    {
        float[] sample = new float[1] { marker };
        outlet.push_sample(sample);
    }

    public void Write(int marker)
    {
        float[] sample = new float[1] { marker };
        outlet.push_sample(sample);
    }
    public void PullData(IntPtr inlet, float[,] data_buffer, double[] timestamp_buffer, double timeout)
    {
        int numPulled = LSLHelper.PullChunk(inlet, data_buffer, timestamp_buffer, timeout);
        Debug.Log("Number of samples pulled: " + numPulled);
    }
}
