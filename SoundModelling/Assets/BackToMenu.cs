using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SceneControl;

public class BackToMenu : MonoBehaviour
{
   public void GoBack()
    {
        SceneManager.Instance.BackToMenu();
    }
}
