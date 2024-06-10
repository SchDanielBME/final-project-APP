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
    [SerializeField] private GameObject props;
    private int scence;

    void Start()
    {
        room.SetActive(false);
        props.SetActive(false);
        Data currentScene = data.GetComponent<Data>();
        currentScene.OnCurentScenes += ComponentCurentScenes;
    }

    private void ComponentCurentScenes(object sender, Data.CurrentEventArgs e)
    {
        scence = e.CurrentSceneNum;
       
        if (scence == 1) {
            Debug.Log("OFFICE");
            room.SetActive(true);
            props.SetActive(true);
            plane.SetActive(false);
        }

        if (scence == 2)
        {
            Debug.Log("room");
            room.SetActive(true);
            props.SetActive(false);
            plane.SetActive(false);
        }

        if (scence == 3)
        {
            Debug.Log("empty");
            room.SetActive(false);
            props.SetActive(false);
            plane.SetActive(true);
        }

    }
}
