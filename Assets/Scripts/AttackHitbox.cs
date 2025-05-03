using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class AttackHitbox : MonoBehaviour
{
    [SerializeField] private GameObject sliceEffect;
    public bool isPlayerHitbox = false;
    
    public UnityEvent onParried;
    private bool isParried = false;

    private void Start()
    {
        if (onParried == null)
            onParried = new UnityEvent();
    }

    private void OnEnable()
    {
        StartCoroutine(SliceMeUpBaby());
    }

    private IEnumerator SliceMeUpBaby()
    {
        sliceEffect.SetActive(false);
        yield return new WaitForSeconds(0.12f);
        sliceEffect.SetActive(true);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (isParried) return;

        if (other.CompareTag("Enemy"))
        {
            EnemyController enemy = other.GetComponent<EnemyController>();
            if (enemy != null)
            {
                enemy.Die();
            }
        }
        // Normal attack logic
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null && player._canGetHurt && !player.isParrying)
            {
                player.Die();
            }
            else if (player != null && player._canGetHurt && player.isParrying)
            {
                onParried?.Invoke();
            }
        }
    }
}