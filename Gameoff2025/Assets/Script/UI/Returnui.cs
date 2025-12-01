using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

public class Returnui : MonoBehaviour
{
    public Button BtnReturn;
    // Start is called before the first frame update
    void Start()
    {
        BtnReturn.onClick.AddListener(()=>
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("Start");
        });
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
