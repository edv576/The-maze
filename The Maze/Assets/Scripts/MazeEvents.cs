using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MazeEvents : MonoBehaviour
{
    //Script given to the checkpoint object. Lets the player go back to the maze generation scene.
    GameObject maze;
    // Start is called before the first frame update
    void Start()
    {
        maze = GameObject.Find("Maze");
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }

    //Destroys the current maze and goes to the maze generation scene.
    public void GoBackToGeneration()
    {
        maze.GetComponent<MazeController>().DestroyMaze();
        Destroy(maze);

        SceneManager.LoadScene(1);



    }


}
