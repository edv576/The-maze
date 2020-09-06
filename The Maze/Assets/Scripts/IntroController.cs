using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class IntroController : MonoBehaviour
{
    //Only class on the intro scene. Just lets the player press a key to go to the maze generator


    public UISoundsController uiSoundsController;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //Go to the maze generator scenes if the player presses the "E" key
        if (Input.GetKeyDown(KeyCode.E))
        {
            uiSoundsController.PlayGoNextSceneSound();
            SceneManager.LoadScene(1);
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
        
    }
}
