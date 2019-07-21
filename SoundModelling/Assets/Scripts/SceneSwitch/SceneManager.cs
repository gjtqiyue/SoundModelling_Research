using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SceneControl {
    public class SceneManager : SingletonBase<SceneManager>
    {
        public int sceneSlected;
        private int menuScene = 0;

        protected override void Awake()
        {
            base.Awake();
            DontDestroyOnLoad(this);
        }

        public void SetSelectedScene(int value)
        {
            sceneSlected = value;
        }

        public int MenuScene
        {
            set
            {
                menuScene = value;
            }
        }

        public void SwitchScene()
        {
            if (sceneSlected != menuScene)
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(sceneSlected);
            }
        }

        public void BackToMenu()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(menuScene);
        }

        public void Quit()
        {
            Application.Quit();
        }

        public void OnApplicationQuit()
        {
            
        }
    }
}
