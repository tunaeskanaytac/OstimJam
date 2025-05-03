using System;
using System.Collections;
using Enemy;
using UnityEngine;
using UnityEngine.Events;

public class AttackHitbox : MonoBehaviour
{
    [SerializeField] private GameObject sliceEffect;
    public bool isPlayerHitbox = false;

    private FinalEnemyBehaviour lastEnemy;

    public UnityEvent onParried;
    private bool isParried = false;
    public Timer timer;

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
            lastEnemy = other.GetComponent<FinalEnemyBehaviour>();
            if (lastEnemy != null)
            {
                if (!isPlayerHitbox)
                {
                    lastEnemy.Death();
                    timer.elapsedTime += 3;
                }
            }
        }
        /*
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

                if (lastEnemy != null)
                {
                    lastEnemy.OnParried(player.transform.position, 10f); // Knockback force = 10 (tweakable)
                    isParried = true;
                }
            }
        }
        */
    }
}