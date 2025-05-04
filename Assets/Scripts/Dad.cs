using UnityEngine;
using UnityEngine.SceneManagement;

public class Dad : MonoBehaviour
{
    private Animator anim;
    private bool isDying = false;

    private void Start()
    {
        anim = GetComponent<Animator>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isDying)
        {
            isDying = true;
            anim.SetTrigger("KillDad");
            StartCoroutine(DieAfterDelay(3f));
        }
    }

    private System.Collections.IEnumerator DieAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
        
    }
}
