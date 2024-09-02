using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomNubersTraining : MonoBehaviour
{
    private int[] order = {1, 2};
    private int[] firstAnglesOrder = {1, 2};
    private int[] secondAnglesOrder = {1, 2};

    public event EventHandler<ScenesEventArgs> OnGenerateScenes;
    public class ScenesEventArgs : EventArgs
    {
        public int[] Order { get;}

        public ScenesEventArgs(int[] order)
        {
            Order = order;
        }
    }
    public event EventHandler<AngelsEventArgs> OnGenerateAngles;
    public class AngelsEventArgs : EventArgs
    {
        public int[] FirstAngles { get; }
        public int[] SecondAngles { get; }
       
        public AngelsEventArgs(int[] firstAnglesOrder, int[] secondAnglesOrder)
        {
            FirstAngles = firstAnglesOrder;
            SecondAngles = secondAnglesOrder;
        }
    }

    public void GenerrateOrder()
    {
        bool firstValid, secondValid;
        do
        {
            ShuffleArray(firstAnglesOrder);
            firstValid = CheckAnglesCoverage(ConvertToDegrees(firstAnglesOrder));
        } while (!firstValid);

        do
        {
            ShuffleArray(secondAnglesOrder);
            secondValid = CheckAnglesCoverage(ConvertToDegrees(secondAnglesOrder));
        } while (!secondValid);

        OnGenerateAngles?.Invoke(this, new AngelsEventArgs(firstAnglesOrder, secondAnglesOrder));
        ShuffleArray(order);
        OnGenerateScenes?.Invoke(this, new ScenesEventArgs(order));
    
    }


    private int[] ConvertToDegrees(int[] anglesOrder)
    {
        return anglesOrder.Select(a => a switch
        {
            1 => 250,
            2 => -275,
            _ => 0
        }).ToArray();
    }

    private bool CheckAnglesCoverage(int[] anglesOrder)
    {
        int currentAngle = 0;
        HashSet<int> coveredAngles = new HashSet<int>();

        foreach (int angle in anglesOrder)
        {
            for (int i = 0; i < Math.Abs(angle); i++)
            {
                coveredAngles.Add((currentAngle + (angle > 0 ? i : -i) + 360) % 360);
            }
            currentAngle = (currentAngle + angle + 360) % 360;
        }
        return coveredAngles.Count == 360;
    }

    private void ShuffleArray(int[] arrary)
    {
        System.Random rand = new System.Random();
        for (int i = 0; i < arrary.Length; i++)
        {
            int randIndex = rand.Next(i, arrary.Length);
            int temp = arrary[i];
            arrary[i] = arrary[randIndex];
            arrary[randIndex] = temp;
        }

    }
}
