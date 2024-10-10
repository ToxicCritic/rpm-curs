using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Scripts.StartMenu
{
    public class ControlsScene : MonoBehaviour
    {
        public void BackToMenu() {
            SceneManager.LoadScene("MainMenu");
        }
    }
}
