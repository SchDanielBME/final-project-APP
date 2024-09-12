using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomNubersTraining : MonoBehaviour
{
    private int[] order = { 1, 2 };
    private int first = 0;
    private int second = 0;

    private int[] firstAnglesOrder = { 1, 2 };
    private int[] secondAnglesOrder = { 1, 2 };

    public event EventHandler<ScenesEventArgs> OnGenerateScenes;
    public class ScenesEventArgs : EventArgs
    {
        public int[] Order { get; }

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
        ShuffleArry(order);
        ShuffleArry(firstAnglesOrder);
        ShuffleArry(secondAnglesOrder);

        first = order[0];
        second = order[1];

        OnGenerateAngles?.Invoke(this, new AngelsEventArgs(firstAnglesOrder, secondAnglesOrder));

        OnGenerateScenes?.Invoke(this, new ScenesEventArgs(order));

    }

    private void ShuffleArry(int[] arrary)
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