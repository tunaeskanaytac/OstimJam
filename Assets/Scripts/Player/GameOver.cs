using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.UIElements;
using Image = UnityEngine.UI.Image;

namespace Player
{
    public class GameOver : MonoBehaviour
    {
        [SerializeField] private GameObject darkPanel;
        [SerializeField] private TMPro.TMP_Text gameOverText;
        [SerializeField] private float fadeDuration = 1f; // Duration of fade animation in seconds

        private void Start()
        {
            // Set everything invisible at start
            SetInitialState();
        }

        private void SetInitialState()
        {
            // Get the Image component and set it to fully transparent
            Image panelImage = darkPanel.GetComponent<Image>();
            panelImage.color = new Color(0, 0, 0, 0);
            darkPanel.gameObject.SetActive(true);

            // Set text to transparent
            gameOverText.color = new Color(gameOverText.color.r, gameOverText.color.g, gameOverText.color.b, 0);
            gameOverText.gameObject.SetActive(true);
        }

        public void ShowGameOver()
        {
            StartCoroutine(FadeInGameOver());
        }

        private IEnumerator FadeInGameOver()
        {
            float elapsedTime = 0f;
            Image panelImage = darkPanel.GetComponent<Image>();

            // Make sure everything is visible but transparent
            darkPanel.gameObject.SetActive(true);
            gameOverText.gameObject.SetActive(true);

            // Get the initial colors
            Color panelColor = new Color(0, 0, 0, 0);
            Color textColor = new Color(1, 0, 0, 0); // Red color with 0 alpha
            gameOverText.text = "GAME OVER";

            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                float alpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeDuration);

                // Update panel color
                panelImage.color = new Color(panelColor.r, panelColor.g, panelColor.b, alpha);

                // Update text color
                gameOverText.color = new Color(textColor.r, textColor.g, textColor.b, alpha);

                yield return null;
            }

            // Ensure we end up with the exact final values
            panelImage.color = new Color(0, 0, 0, 1f);
            gameOverText.color = new Color(1, 0, 0, 1);
            
        }

        // Optional: Method to reset/hide the game over screen
        public void HideGameOver()
        {
            SetInitialState();
        }
    }
}