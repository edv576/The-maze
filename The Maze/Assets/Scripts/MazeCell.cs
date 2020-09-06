using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeCell
{
    //This class contains all the information related to a cell of the maze. It includes the objects corresponding to the
    //floor, the upper wall (upWall), bottom wall (downWall), the left wall (leftWall), the right wall (rightWall) and a boolean
    //flag that indicates if the maze cell has been visited before or not.

    public bool visited = false;
    public GameObject upWall;
    public GameObject downWall;
    public GameObject leftWall;
    public GameObject rightWall;
    public GameObject floorTile;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
