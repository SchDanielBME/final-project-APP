using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomNubers : MonoBehaviour
{
    private int[] order = { 1, 2, 3, 4 };
    private int[] firstAnglesOrder = { 1, 2, 3, 4, 5, 6};
    private int[] secondAnglesOrder = { 1, 2, 3, 4, 5, 6};
    private int[] thirdAnglesOrder = {1, 2, 3, 4, 5, 6};
    private int[] fourthAnglesOrder = { 1, 2, 3, 4, 5, 6 };


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
        public int[] ThirdAngles { get; }
        public int[] FourthAngles { get; }

        public AngelsEventArgs(int[] firstAnglesOrder, int[] secondAnglesOrder, int[] thirdAnglesOrder, int[] fourthAngles)
        {
            FirstAngles = firstAnglesOrder;
            SecondAngles = secondAnglesOrder;
            ThirdAngles = thirdAnglesOrder;
            FourthAngles = fourthAngles;
        }
    }

    public void GenerrateOrder()
    {
        bool firstValid, secondValid, thirdValid, fourthValid;
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

        do
        {
            ShuffleArray(thirdAnglesOrder);
            thirdValid = CheckAnglesCoverage(ConvertToDegrees(thirdAnglesOrder));
        } while (!thirdValid);

        do
        {
            ShuffleArray(fourthAnglesOrder);
            fourthValid = CheckAnglesCoverage(ConvertToDegrees(fourthAnglesOrder));
        } while (!fourthValid);


        OnGenerateAngles?.Invoke(this, new AngelsEventArgs(firstAnglesOrder, secondAnglesOrder, thirdAnglesOrder, fourthAnglesOrder));
        ShuffleArray(order);
        OnGenerateScenes?.Invoke(this, new ScenesEventArgs(order));
    }


    private int[] ConvertToDegrees(int[] anglesOrder)
    {
        return anglesOrder.Select(a => a switch
        {
            1 => 288,
            2 => 294,
            3 => 300,
            4 => -285,
            5 => -291,
            6 => -297,
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
