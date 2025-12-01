using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StartUI : MonoBehaviour
{
    public Button btnStart;
    public Button btnExit;
    void Start()
    {
        btnStart.onClick.AddListener(()=>{
            UnityEngine.SceneManagement.SceneManager.LoadScene("Game");
        });
        btnExit.onClick.AddListener(()=>{
            Application.Quit();
        });
    }

   
}
