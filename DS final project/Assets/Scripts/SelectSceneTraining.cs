using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SelectSceneTraining : MonoBehaviour
{
    [SerializeField] private GameObject data;
    [SerializeField] private GameObject plane;
    [SerializeField] private GameObject props;

    private int scence;

    void Start()
    {
        props.SetActive(false);
        plane.SetActive(true);
      
        DataTraining currentScene = data.GetComponent<DataTraining>();
        currentScene.OnCurentScenes += ComponentCurentScenes;
    }

    private void ComponentCurentScenes(object sender, DataTraining.CurrentEventArgs e)
    {
        scence = e.CurrentSceneNum;
       
        if (scence == 1) {
            Debug.Log("T1");
            props.SetActive(false);
        }

        if (scence == 2)
        {
            Debug.Log("T2");
            props.SetActive(true);
        }

    }
}
