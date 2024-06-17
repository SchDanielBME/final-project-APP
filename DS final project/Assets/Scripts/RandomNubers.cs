using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomNubers : MonoBehaviour
{
    private int[] order = { 1, 2, 3, 4 };
    [SerializeField] private int first = 0;
    [SerializeField] private int second = 0;
    [SerializeField] private int third = 0;
    [SerializeField] private int fourth = 0;


    [SerializeField] private int[] firstAnglesOrder = { 1, 2, 3, 4, 5, 6};
    [SerializeField] private int[] secondAnglesOrder = { 1, 2, 3, 4, 5, 6};
    [SerializeField] private int[] thirdAnglesOrder = {1, 2, 3, 4, 5, 6};
    [SerializeField] private int[] fourthAnglesOrder = { 1, 2, 3, 4, 5, 6 };


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
        ShuffleArry(order);
        ShuffleArry(firstAnglesOrder);
        ShuffleArry(secondAnglesOrder);
        ShuffleArry(thirdAnglesOrder);
        ShuffleArry(fourthAnglesOrder);

        first = order[0];
        second = order[1];
        third = order[2];
        fourth = order[3];

        OnGenerateAngles?.Invoke(this, new AngelsEventArgs(firstAnglesOrder, secondAnglesOrder, thirdAnglesOrder, fourthAnglesOrder));
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
