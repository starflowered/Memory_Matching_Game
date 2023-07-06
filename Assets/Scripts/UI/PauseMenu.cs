using UnityEngine;
using UnityEngine.SceneManagement;

namespace UI
{
    public class PauseMenu : MonoBehaviour
    {
        [SerializeField] private GameObject menu;
        
        public void PauseGame()
        {
            menu.SetActive(true);
        }
        
        public void UnpauseGame()
        {
            menu.SetActive(false);
        }

        public void StartGame()
        {
            SceneManager.LoadScene(1);
        }
        
        public void Restart()
        {
            SceneManager.LoadScene(1);
        }

        public void ExitToMainMenu()
        {
            SceneManager.LoadScene(0);
        }

        public void Exit()
        {
            Application.Quit();
        }
    }
}