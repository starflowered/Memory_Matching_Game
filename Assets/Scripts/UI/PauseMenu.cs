using UnityEngine;
using UnityEngine.SceneManagement;

namespace UI
{
    /// <summary>
    /// This class is mainly used for button interactions.
    /// </summary>
    public class PauseMenu : MonoBehaviour
    {
        #region serialized fields

        [SerializeField] [Tooltip("The pause menu that should be shown if the game is paused.")]
        private GameObject menu;

        #endregion
        
        /// <summary>
        /// Pause the game.
        /// </summary>
        public void PauseGame()
        {
            menu.SetActive(true);
        }
        
        /// <summary>
        /// Unpause the game.
        /// </summary>
        public void UnpauseGame()
        {
            menu.SetActive(false);
        }

        /// <summary>
        /// Start the game from the main menu.
        /// </summary>
        public void StartGame()
        {
            SceneManager.LoadScene(1);
        }
        
        /// <summary>
        /// Restart the current game.
        /// </summary>
        public void Restart()
        {
            SceneManager.LoadScene(1);
        }

        /// <summary>
        /// Exit the scene to the main menu.
        /// </summary>
        public void ExitToMainMenu()
        {
            SceneManager.LoadScene(0);
        }

        /// <summary>
        /// Closes the application.
        /// </summary>
        public void Exit()
        {
            Application.Quit();
        }
    }
}