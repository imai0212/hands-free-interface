using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using System.IO; 
using UnityEngine.SceneManagement;

public class Result : MonoBehaviour
{
    public Text timeText;
    private float time;
    public AudioClip FinishSound;
    AudioSource audioSource;
    private float start;


    void Start()
    {
        time = FittsLaw.SendExperimentTime();
        timeText.text = $"Time : {time}";
        
        audioSource = GetComponent<AudioSource>();
        audioSource.PlayOneShot(FinishSound);
        start = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.time - start > 10)
        {
            SceneManager.LoadScene("FittsLaw");
        }
    }
}
