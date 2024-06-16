using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SelectScene : MonoBehaviour
{
    [SerializeField] private GameObject data;
    [SerializeField] private GameObject plane;
    [SerializeField] private GameObject room;
    [SerializeField] private GameObject parquet;
    [SerializeField] private GameObject celling;
    [SerializeField] private GameObject props;
    [SerializeField] private GameObject pipe;

    private int scence;

    void Start()
    {
        room.SetActive(false);
        props.SetActive(false);
        parquet.SetActive(false);
        celling.SetActive(false);
        pipe.SetActive(false);
        Data currentScene = data.GetComponent<Data>();
        currentScene.OnCurentScenes += ComponentCurentScenes;
    }

    private void ComponentCurentScenes(object sender, Data.CurrentEventArgs e)
    {
        scence = e.CurrentSceneNum;
       
        if (scence == 1) {
            Debug.Log("S0");
            room.SetActive(true);
            props.SetActive(false);
            parquet.SetActive(true);
            celling.SetActive(true);
            pipe.SetActive(false);
            plane.SetActive(false);
        }

        if (scence == 2)
        {
            Debug.Log("S1");
            room.SetActive(true);
            props.SetActive(true);
            parquet.SetActive(true);
            celling.SetActive(true);
            pipe.SetActive(false);
            plane.SetActive(false);
        }

        if (scence == 3)
        {
            Debug.Log("R0");
            room.SetActive(false);
            props.SetActive(false);
            parquet.SetActive(true);
            celling.SetActive(true);
            pipe.SetActive(true);
            plane.SetActive(false);
        }

        if (scence == 4)
        {
            Debug.Log("R1");
            room.SetActive(false);
            props.SetActive(true);
            parquet.SetActive(true);
            celling.SetActive(true);
            pipe.SetActive(true);
            plane.SetActive(false);
        }

    }
}
