using System;
using System.Runtime.InteropServices;

public class LSLHelper
{
#if UNITY_ANDROID && !UNITY_EDITOR
        [DllImport("lsl")]
        private static extern uint lsl_pull_chunk_f(IntPtr obj, [In, Out] float[] data_buffer, [In, Out] double[] timestamp_buffer, uint data_buffer_elements, uint timestamp_buffer_elements, double timeout, ref int ec);
#else
    [DllImport("liblsl")]
    private static extern uint lsl_pull_chunk_f(IntPtr obj, [In, Out] float[] data_buffer, [In, Out] double[] timestamp_buffer, uint data_buffer_elements, uint timestamp_buffer_elements, double timeout, ref int ec);
#endif

    public static int PullChunk(IntPtr obj, float[,] data_buffer, double[] timestamp_buffer, double timeout = 0.0)
    {
        int ec = 0;
        int numRows = data_buffer.GetLength(0);
        int numCols = data_buffer.GetLength(1);
        float[] data_buffer_flat = new float[numRows * numCols];

        for (int i = 0; i < numRows; i++)
        {
            for (int j = 0; j < numCols; j++)
            {
                data_buffer_flat[i * numCols + j] = data_buffer[i, j];
            }
        }

        uint res = lsl_pull_chunk_f(obj, data_buffer_flat, timestamp_buffer, (uint)data_buffer_flat.Length, (uint)timestamp_buffer.Length, timeout, ref ec);
        LSL.LSL.check_error(ec);

        for (int i = 0; i < numRows; i++)
        {
            for (int j = 0; j < numCols; j++)
            {
                data_buffer[i, j] = data_buffer_flat[i * numCols + j];
            }
        }

        return (int)res / numCols;
    }
}
