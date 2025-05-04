using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Timer : MonoBehaviour
{
    public static Timer instance;

    [SerializeField] public TextMeshProUGUI timerText;
    [SerializeField] public GameObject timerAddition;
    public float elapsedTime;

    private void Start()
    {
        elapsedTime = 15f;
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    void Update()
    {
        elapsedTime -= Time.deltaTime;
        if (elapsedTime <= 0)
        {
            elapsedTime = 0;
            timerText.color = Color.red;
        }
        else if (elapsedTime > 5)
        {
            timerText.color = Color.green;
        }
        else if (elapsedTime < 5)
        {
            timerText.color = Color.red;
        }
        int minutes = Mathf.FloorToInt(elapsedTime / 60);
        int seconds = Mathf.FloorToInt(elapsedTime % 60);
        timerText.text = string.Format("{0:00} : {1:00}", minutes, seconds);
    }

    public void AddTime(float timeToAdd)
    {
        elapsedTime += timeToAdd;
        timerAddition.GetComponent<Animator>().SetTrigger("TimeAdded");
        
    }
}
