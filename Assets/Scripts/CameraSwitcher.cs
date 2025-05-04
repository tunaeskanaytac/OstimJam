using Unity.Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CameraSwitcher : MonoBehaviour
{
    [SerializeField] private CinemachineCamera camToEnable;
    [SerializeField] private CinemachineCamera camToDisable;
    [SerializeField] private GameObject door;

    private void Start()
    {
        camToDisable.Priority = 20;
        camToEnable.Priority = 5;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            door.SetActive(true);
            if (camToEnable != null && camToDisable != null)
            {
                camToEnable.Priority = 20;
                camToDisable.Priority = 5;
            }
        }
    }
}
