using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MazeController : MonoBehaviour
{
    //The variables rows and columns contain the maximum number of rows and columns the grid that will become the maze can contain.
    //These values can change depending of user input.
    public int rows = 2;
    public int columns = 2;

    //Prefab object corresponding to the floor of a maze cell
    public GameObject wall;

    //Prefab object corresponding to the wall of a maze cell
    public GameObject floor;

    //Prefab object corresponding to the invisible barrier that prevents the player from leaving the maze using the entrance door.
    public GameObject clearDoor;

    //Prefab object corresponding to the final tile of floor the user needs to step on to complete the game. It has a special victory button
    //at the center that activates the victory animation once it gets stepped on.
    public GameObject victoryFloor;

    //Prefab object corresponding to a corner of the maze. It needs to be spawned several times to cover the spaces left empty by the
    //maze creation algorithm.
    public GameObject corner;

    //Prefab object corresponding to the playable character.
    public GameObject playerCharacter;

    //Input field that lets the player change the number of columns the maze will have. The minimum input can be of 2.
    public InputField widthInput;

    //Input field that lets the player change the number of rows the maze will have. The minimu input can be of 2.
    public InputField heightInput;

    //Canvas containing all the input option for the maze generation.
    public GameObject inputCanvas;

    //Button in charge of having the user advance to the exploration scene. It can only be pressed if the player has generated at least
    //one maze. Otherwise it stays invisible. Also, it will only be visible if the maze created has both rows and columns be less than 21.
    //Bigger mazes would be tedious to explore.
    public GameObject exploreButton;

    //Ortographic camera that lets the maze be seen from above in a 2D plane. In the exploration scene, it will serve as the companion map
    //that will help the player traverse the maze.
    public Camera ortoCamera;

    //Render texture that displays what the ortographic camera is seeing. Used for the companion map in the exploration scene.
    public RenderTexture cameraTexture;

    //Size of the floor tile of single maze cell. For this example is of 5 units in the X and Z axis as seen in the floor prefab.
    float size;

    //Height of the wall of a single maze cell. For this example is of 3 units in the Y axis as seen in the wall an corner prefabs.
    float heightWall;

    //Thickness of the wall and floor of the maze. As seen in the prefab in 0.5 units for boths cases.
    float thickness;

    //Full grid containing all the cells of the maze. Initially would have all the cells with all its walls intact making it a full grid.
    //As the algorithm is run some of the walls will be destroyed to create the actual maze.
    private MazeCell[,] grid;

    //Current row of the grid the algorithm is visiting
    private int currentRow = 0;

    //Current column of the grid the algorithm is visiting
    private int currentColumn = 0;

    //Flag that tells if there aren't unvisited maze cells that are adyacent to visited neighbor cells. Will be explained with more detail
    //when the algorithm functions are explained. The algorithm used for the maze creation is called Hunt and Kill. 
    private bool scanComplete = false;

    public UISoundsController uiSoundsController;

    //This used to prevent the created maze from being destroyed when going to the exploration scene.
    private void Awake()
    {
        DontDestroyOnLoad(this);
        
    }
    // Start is called before the first frame update
    void Start()
    {
        //Get the maximum number of rows from the height input
        if (heightInput)
        {
            heightInput.text = rows.ToString();
        }

        //Get the maximum number of columns from the width input
        if (widthInput)
        {
            widthInput.text = columns.ToString();
        }

        //The ortographic camera is found here. Its done this way since in the exploration scene the camera would not be present since
        //the beginning. It will be inherited from the maze generation scene instead.
        ortoCamera = GameObject.Find("Orto camera").GetComponent<Camera>();

        //If the ortographic camera is found, the camera will only be able to see the UI elements (aka the input canvas) at the beginning
        //of the maze generation scene.
        if (ortoCamera)
        {
            ortoCamera.cullingMask = 1 << LayerMask.NameToLayer("UI");
        }

    }

    void CreateGrid()
    {
        //Get the size, thickness and height of the wall from the prefabs
        size = floor.transform.localScale.x;
        thickness = floor.transform.localScale.y;
        heightWall = wall.transform.localScale.y;

        //Initialize the grid for the maze with the maximum number of columns and rows indicated by the player
        grid = new MazeCell[rows, columns];

        //Traverse through the grid initializing all the maze cells.
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                //Initialize of the objects of a maze cell (upper wall (upWall), bottom wall (downWall), the left wall (leftWall) and 
                //the right wall (rightWall)).
                GameObject floorTile = Instantiate(floor, new Vector3(j * size, 0, -i * size), Quaternion.identity);
                floorTile.name = "Floor_" + i + "_" + j;

                GameObject upWall = Instantiate(wall, new Vector3(j * size, (heightWall + thickness) / 2, -i * size + (size - thickness) / 2), Quaternion.identity);
                upWall.name = "UpWall_" + i + "_" + j;

                GameObject downWall = Instantiate(wall, new Vector3(j * size, (heightWall + thickness) / 2, -i * size - (size - thickness) / 2), Quaternion.identity);
                downWall.name = "DownWall_" + i + "_" + j;

                GameObject leftWall = Instantiate(wall, new Vector3(j * size - (size - thickness) / 2, (heightWall + thickness) / 2, -i * size), Quaternion.Euler(0, 90, 0));
                leftWall.name = "LeftWall_" + i + "_" + j;

                GameObject rightWall = Instantiate(wall, new Vector3(j * size + (size - thickness) / 2, (heightWall + thickness) / 2, -i * size), Quaternion.Euler(0, 90, 0));
                rightWall.name = "RightWall_" + i + "_" + j;


                //Assign all the maze cell objects to the corresponding maze cell
                grid[i, j] = new MazeCell();
                grid[i, j].upWall = upWall;
                grid[i, j].downWall = downWall;
                grid[i, j].leftWall = leftWall;
                grid[i, j].rightWall = rightWall;
                grid[i, j].floorTile = floorTile;


                //Make the parent of all the maze objects to be the maze object. This maze object is the one that has the current script attached to it.
                floorTile.transform.parent = transform;
                upWall.transform.parent = transform;
                downWall.transform.parent = transform;
                leftWall.transform.parent = transform;
                rightWall.transform.parent = transform;
                floorTile.transform.parent = transform;

                //In case its the first maze cell of the grid, this destroys the left left wall to create the entrance door.
                if(i == 0 && j == 0)
                {
                    Destroy(grid[i, j].leftWall);
                    grid[i, j].leftWall = null;
                    GameObject g = new GameObject();
                    Destroy(g);
                    //g = null;
                    if(!grid[i, j].leftWall)
                    {
                        print("Destroyed");
                    }
                }

                //In case its the final maze cell of the grid, this destroys the right wall to create the exit door.
                if (i == rows - 1 && j == columns - 1)
                {
                    Destroy(grid[i, j].rightWall);
                }
            }
        }

        //Initializes the player position in the first floor of the maze
        playerCharacter.transform.position = new Vector3(GameObject.Find("Floor_0_0").transform.position.x,
            playerCharacter.transform.position.y, GameObject.Find("Floor_0_0").transform.position.z);
    }

    //Tells if any of the neighbors of a maze cell have been visited. If one of them is detected unvisited, the function return "true".
    //If none of them are visited return "false".
    bool AreThereUnvisitedNeighbors()
    {
        //Look the neighbor up
        if (IsCellUnvisitedAndBetweenBoundaries(currentRow - 1, currentColumn))
        {
            return true;
        }

        //Look the neighbor down
        if (IsCellUnvisitedAndBetweenBoundaries(currentRow + 1, currentColumn))
        {
            return true;
        }

        //Look neighbor left
        if (IsCellUnvisitedAndBetweenBoundaries(currentRow, currentColumn - 1))
        {
            return true;
        }

        //Look neighbor right
        if (IsCellUnvisitedAndBetweenBoundaries(currentRow, currentColumn + 1))
        {
            return true;
        }

        //There aren't unvisited neighbors
        return false;
    }

    //Tells if there are visited neighbors given the row and column of a maze cell. Can be combined with the
    //AreThereUnvisitedNeighbors() function in a next iteration.
    bool AreThereVisitedNeighbors(int row, int col)
    {
        //Look up for visited neighbor.
        if(row>0 && grid[row - 1, col].visited)
        {
            return true;
        }

        //Look down for visited neighbor.
        if (row < rows-1 && grid[row + 1, col].visited)
        {
            return true;
        }

        //Look left for visited neighbor.
        if (col > 0 && grid[row, col - 1].visited)
        {
            return true;
        }

        //Look right for visited neighbor.
        if (col < columns - 1 && grid[row, col + 1].visited)
        {
            return true;
        }

        //Return false if all the neighbors are unvisited. 
        return false;
    }

    //Verifies if the maze cell is unvisited between the boundaries of the maze given a row and column.
    bool IsCellUnvisitedAndBetweenBoundaries(int row, int col)
    {
        if(row>=0 && row<rows && col>=0 && col<columns &&  !grid[row, col].visited)
        {
            return true;
        }

        return false;
    }

    //This verifies if the cell is between the boundaries of maze
    bool IsCellBetweenBoundaries(int row, int col)
    {
        if (row >= 0 && row < rows && col >= 0 && col < columns)
        {
            return true;
        }

        return false;
    }


    //Calls the algorithm functions
    void HuntAndKill()
    {
        //Mark the first cell of the random walk as visited. This is the starting point of the algorithm.
        grid[currentRow, currentColumn].visited = true;

        //Executes the loop as long as there aren't unvisited maze cells that are adyacent to visited neighbor cells.
        while (!scanComplete)
        {
            //Starts step 1 of the algorithm
            Walk();
            //Starts step 2 of the algorithm
            Hunt();
        }

       
        
    }

    //First phase of the algorithm. Performs a random walk from the starting point until there aren't any more possiblr 
    //unvisited neighbors.
    void Walk()
    {
        //Checks if there aren't unvisited neighbors.
        while (AreThereUnvisitedNeighbors())
        {
            //Gets a random direction. 0 represents the up direction, 1 represents the down direction, 2 represents the left direction
            //and 3 represents the right direction.
            int direction = Random.Range(0, 4);

            //Switch actions to take depending in the direction taken.
            switch (direction)
            {
                //Check up
                case 0:
                    {
                        //If the maze cell looked up is unvisited and between the maze boundaries, this destroys that cell's down wall
                        //and the current's up wall. It then moves the current cell to the up neighbor and marks it as visited.
                        if (IsCellUnvisitedAndBetweenBoundaries(currentRow - 1, currentColumn))
                        {
                            if (grid[currentRow, currentColumn].upWall)
                            {
                                Destroy(grid[currentRow, currentColumn].upWall);
                                grid[currentRow, currentColumn].upWall = null;
                            }
                            else
                            {
                                print("this");
                            }

                            currentRow--;
                            grid[currentRow, currentColumn].visited = true;

                            if (grid[currentRow, currentColumn].downWall)
                            {
                                Destroy(grid[currentRow, currentColumn].downWall);
                                grid[currentRow, currentColumn].downWall = null;
                            }

                        }
                        break;
                    }

                //Check down
                case 1:
                    {
                        //If the maze cell looked down is unvisited and between the maze boundaries, this destroys that cell's up wall
                        //and the current's down wall. It then moves the current cell to the down neighbor and marks it as visited.
                        if (IsCellUnvisitedAndBetweenBoundaries(currentRow + 1, currentColumn))
                        {
                            if (grid[currentRow, currentColumn].downWall)
                            {
                                Destroy(grid[currentRow, currentColumn].downWall);
                                grid[currentRow, currentColumn].downWall = null;
                            }

                            currentRow++;
                            grid[currentRow, currentColumn].visited = true;

                            if (grid[currentRow, currentColumn].upWall)
                            {
                                Destroy(grid[currentRow, currentColumn].upWall);
                                grid[currentRow, currentColumn].upWall = null;
                            }
                        }
                        break;
                    }

                //Check left
                case 2:
                    {
                        //If the maze cell looked left is unvisited and between the maze boundaries, this destroys that cell's righ wall
                        //and the current's left wall. It then moves the current cell to the left neighbor and marks it as visited.
                        if (IsCellUnvisitedAndBetweenBoundaries(currentRow, currentColumn - 1))
                        {
                            if (grid[currentRow, currentColumn].leftWall)
                            {
                                Destroy(grid[currentRow, currentColumn].leftWall);
                                grid[currentRow, currentColumn].leftWall = null;
                            }

                            currentColumn--;
                            grid[currentRow, currentColumn].visited = true;

                            if (grid[currentRow, currentColumn].rightWall)
                            {
                                Destroy(grid[currentRow, currentColumn].rightWall);
                                grid[currentRow, currentColumn].rightWall = null;
                            }
                        }
                        break;
                    }

                //Check right
                case 3:
                    {
                        //If the maze cell looked right is unvisited and between the maze boundaries, this destroys that cell's up left
                        //and the current's right wall. It then moves the current cell to the right neighbor and marks it as visited.
                        if (IsCellUnvisitedAndBetweenBoundaries(currentRow, currentColumn + 1))
                        {
                            if (grid[currentRow, currentColumn].rightWall)
                            {
                                Destroy(grid[currentRow, currentColumn].rightWall);
                                grid[currentRow, currentColumn].rightWall = null;
                            }

                            currentColumn++;
                            grid[currentRow, currentColumn].visited = true;

                            if (grid[currentRow, currentColumn].leftWall)
                            {
                                Destroy(grid[currentRow, currentColumn].leftWall);
                                grid[currentRow, currentColumn].leftWall = null;
                            }
                        }
                        break;
                    }
                default:
                    break;
            }
        }
    }

    //Destroys an adjacent wall of a maze cell
    void DestroyAdjacentWall()
    {
        //Create a flag indicating if a wall has been destroyed.
        bool destroyed = false;

        //Run as long no walls were destroyed.
        while (!destroyed)
        {
            //Get a random look direction. 0 represents the up direction, 1 represents the down direction, 2 represents the left direction
            //and 3 represents the right direction 
            int direction = Random.Range(0, 4);

            switch (direction)
            {
                //If the looked up cell is visited, this destroys the walls between them and mark the destroyed flag as "true".
                case 0:
                    {
                        if(currentRow>0 && grid[currentRow - 1, currentColumn].visited)
                        {
                            if(grid[currentRow - 1, currentColumn].downWall)
                            {
                                Destroy(grid[currentRow - 1, currentColumn].downWall);
                                grid[currentRow - 1, currentColumn].downWall = null;
                            }

                            if (grid[currentRow, currentColumn].upWall)
                            {
                                Destroy(grid[currentRow, currentColumn].upWall);
                                grid[currentRow, currentColumn].upWall = null;
                            }

                            destroyed = true;
                        }
                        break;
                    }

                //If the looked down cell is visited, this destroys the walls between them and mark the destroyed flag as "true".
                case 1:
                    {
                        if (currentRow < rows - 1 && grid[currentRow + 1, currentColumn].visited)
                        {
                            if(grid[currentRow + 1, currentColumn].upWall)
                            {
                                Destroy(grid[currentRow + 1, currentColumn].upWall);
                                grid[currentRow + 1, currentColumn].upWall = null;
                            }

                            if (grid[currentRow, currentColumn].downWall)
                            {
                                Destroy(grid[currentRow, currentColumn].downWall);
                                grid[currentRow, currentColumn].downWall = null;
                            }

                            destroyed = true;
                        }
                        break;
                    }

                //If the looked left cell is visited, this destroys the walls between them and mark the destroyed flag as "true".
                case 2:
                    {
                        if (currentColumn > 0 && grid[currentRow, currentColumn - 1].visited)
                        {
                            if(grid[currentRow, currentColumn - 1].rightWall)
                            {
                                Destroy(grid[currentRow, currentColumn - 1].rightWall);
                                grid[currentRow, currentColumn - 1].rightWall = null;
                            }

                            if (grid[currentRow, currentColumn].leftWall)
                            {
                                Destroy(grid[currentRow, currentColumn].leftWall);
                                grid[currentRow, currentColumn].leftWall = null;
                            }

                            destroyed = true;
                        }
                        break;
                    }

                //If the looked right cell is visited, this destroys the walls between them and mark the destroyed flag as "true".
                case 3:
                    {
                        if (currentColumn < columns - 1 && grid[currentRow, currentColumn + 1].visited)
                        {
                            if(grid[currentRow, currentColumn + 1].leftWall)
                            {
                                Destroy(grid[currentRow, currentColumn + 1].leftWall);
                                grid[currentRow, currentColumn + 1].leftWall = null;
                            }

                            if (grid[currentRow, currentColumn].rightWall)
                            {
                                Destroy(grid[currentRow, currentColumn].rightWall);
                                grid[currentRow, currentColumn].rightWall = null;
                            }

                            destroyed = true;
                        }
                        break;
                    }
                    
                default:
                    {
                        break;
                    }
                    
            }

        }
    }

    //Second phase of the algorithm. This scans the whole grid for unvisited maze cells that are adyacent to visited neighbor cells and then
    //then destroys the adjacent walls between the cells carving a passage between them.
    void Hunt()
    {

        //Mark the scan completed flag as true.
        scanComplete = true;

        //Traverse the grid
        for(int i = 0; i < rows; i++)
        {
            for(int j = 0; j < columns; j++)
            {
                //if the visited cell is unvisited and there are visited neighbors, this checks the scan complete flag as false,
                //destroys the adjacent walls between the cells, updates the current row and current cell and finally ends the function.
                if(!grid[i,j].visited && AreThereVisitedNeighbors(i, j))
                {
                    scanComplete = false;
                    currentRow = i;
                    currentColumn = j;
                    grid[currentRow, currentColumn].visited = true;
                    DestroyAdjacentWall();
                    return;

                }
            }
        }
    }


    //This function repositiond the ortographic camera to fit the complete maze no matter its number of columns and rows.
    void RepositionOrtoCamera()
    {
        //Sets the size of the ortographic camera
        float sizeOrtoCamera = (Mathf.Max(rows, columns)*size)/2;
        ortoCamera.orthographicSize = sizeOrtoCamera;

        //Gets the initial position of the ortographic camera
        Vector3 ortoCameraPosition = ortoCamera.transform.position;

        //Repositions the camera so it moves to the center of the maze

        float posX = size*columns/2 - size/2;
        float posZ = -size*rows/2 + size/2;

        ortoCameraPosition.x = posX;
        ortoCameraPosition.z = posZ;

        //Assign the new position to the camera
        ortoCamera.transform.position = ortoCameraPosition;
        

    }

    //This function corrects the missing corners not created by the hunt and kill algorithm
    void FixCorners()
    {
        //Traverses the grid cell by cell
        for(int i = 0; i < rows; i++)
        {
            for(int j = 0; j < columns; j++)
            {
                //Check upper left corner. If a missing corner is found, create a new corner and reposition it in the correct spot. Then
                //the corner's parent is set to be the maze object.
                if (!grid[i, j].leftWall && !grid[i, j].upWall && IsCellBetweenBoundaries(i - 1, j) && IsCellBetweenBoundaries(i, j - 1) &&
                    grid[i - 1, j].leftWall && grid[i, j - 1].upWall)
                {
                    Vector3 cornerPosition = grid[i, j].floorTile.transform.position;
                    cornerPosition.x -= size / 2 - thickness/2;
                    cornerPosition.y = (heightWall + thickness) / 2;
                    cornerPosition.z += size / 2 - thickness/2;
                    GameObject cornerFix = Instantiate(corner, cornerPosition, Quaternion.identity);
                    cornerFix.transform.parent = transform;
                }

                //Check upper right corner. If a missing corner is found, create a new corner and reposition it in the correct spot. Then
                //the corner's parent is set to be the maze object.
                if (!grid[i, j].rightWall && !grid[i, j].upWall && IsCellBetweenBoundaries(i - 1, j) && IsCellBetweenBoundaries(i, j + 1) &&
                    grid[i - 1, j].rightWall && grid[i, j + 1].upWall)
                {
                    Vector3 cornerPosition = grid[i, j].floorTile.transform.position;
                    cornerPosition.x += size / 2 - thickness / 2;
                    cornerPosition.y = (heightWall + thickness) / 2;
                    cornerPosition.z += size / 2 - thickness / 2;
                    GameObject cornerFix = Instantiate(corner, cornerPosition, Quaternion.identity);
                    cornerFix.transform.parent = transform;
                }

                //Check bottom left corner. If a missing corner is found, create a new corner and reposition it in the correct spot. Then
                //the corner's parent is set to be the maze object.
                if (!grid[i, j].leftWall && !grid[i, j].downWall && IsCellBetweenBoundaries(i + 1, j) && IsCellBetweenBoundaries(i, j - 1) &&
                    grid[i + 1, j].leftWall && grid[i, j - 1].downWall)
                {
                    Vector3 cornerPosition = grid[i, j].floorTile.transform.position;
                    cornerPosition.x -= size / 2 - thickness / 2;
                    cornerPosition.y = (heightWall + thickness) / 2;
                    cornerPosition.z -= size / 2 - thickness / 2;
                    GameObject cornerFix = Instantiate(corner, cornerPosition, Quaternion.identity);
                    cornerFix.transform.parent = transform;
                }

                //Check bottom right corner. If a missing corner is found, create a new corner and reposition it in the correct spot. Then
                //the corner's parent is set to be the maze object.
                if (!grid[i, j].rightWall && !grid[i, j].downWall && IsCellBetweenBoundaries(i + 1, j) && IsCellBetweenBoundaries(i, j + 1) &&
                    grid[i + 1, j].rightWall && grid[i, j + 1].downWall)
                {
                    Vector3 cornerPosition = grid[i, j].floorTile.transform.position;
                    cornerPosition.x += size / 2 - thickness / 2;
                    cornerPosition.y = (heightWall + thickness) / 2;
                    cornerPosition.z -= size / 2 - thickness / 2;
                    GameObject cornerFix = Instantiate(corner, cornerPosition, Quaternion.identity);
                    cornerFix.transform.parent = transform;
                }



            }
        }


    }

    //This function generates the maze  according to the user input
    void GenerateGrid()
    {
        //First destroys all the child objects of the maze except the ortographic camera.
        foreach(Transform t in transform)
        {
            if(t.gameObject.name != "Orto camera")
            {
                Destroy(t.gameObject);
            }
            
        }

        //This created the full grid.
        CreateGrid();

        //The current row and column are set to 0.
        currentRow = 0;
        currentColumn = 0;

        //The scan complete flag is set to false.
        scanComplete = false;

        //Executes the maze creation algorithm.
        HuntAndKill();

        //Fixes the maze's corners.
        FixCorners();

        //Repositions the camera to see the whole maze.
        RepositionOrtoCamera();
    }



    //Executed to generate a new maze according to user input. Mazes with the same number of columns and rows are likely to be different
    //each time.
    public void Regenerate()
    {
        //Define the lower limit of rows and columns to 2.
        int rowsI = 2;
        int columnsI = 2;

        //Parse the height and width inputs so they only accept integers and they are always equal or more than 2.
        if (int.TryParse(heightInput.text, out rowsI))
        {
            rows = Mathf.Max(rowsI, 2);
        }

        if(int.TryParse(widthInput.text, out columnsI))
        {
            columns = Mathf.Max(columnsI, 2);
        }

        //Generates the new maze
        GenerateGrid();

        //Shows the input canvas in case its disabled and viceversa.
        ShowUI(!inputCanvas);

        //Put the current number of rows and columns in the input fields. This works for the initialization to show them to be 2 each at
        //the beginning. 
        heightInput.text = rows.ToString();
        widthInput.text = columns.ToString();

        //Only enable the explore button if both rows and columns are less than 21.
        if(rows < 21 && columns < 21)
        {
            exploreButton.SetActive(true);
        }
        else
        {
            exploreButton.SetActive(false);
        }

        
    }
    
    //Instantiates the entrance door and the final floor the user can walk over. 
    void SummonStartAndFinish()
    {
        GameObject cf = Instantiate(clearDoor, new Vector3(0.0f * size - (size - thickness) / 2, (heightWall + thickness) / 2, -0.0f * size), Quaternion.Euler(0, 90, 0));
        cf.transform.parent = transform;
        GameObject vf = Instantiate(victoryFloor, new Vector3(columns * size, 0, -(rows - 1) * size), Quaternion.identity);
        vf.transform.parent = transform;

    }

    //Prepares the maze for the exploration scene
    public void StartExploration()
    {
        //Ortographic can now only see the maze and a reference object. This reference object is not the playable character but a red
        //luminiscent ball hovering over her
        ortoCamera.cullingMask = 1 << LayerMask.NameToLayer("Maze") | 1 << LayerMask.NameToLayer("Reference");
        
        //Gives the ortographic camera its corresponding render texture so it shows what its seeing to the player.
        ortoCamera.targetTexture = cameraTexture;

        //Initializes the entrance door and the final floor.
        SummonStartAndFinish();

        uiSoundsController.PlayGoNextSceneSound();

        //Loads the exploration scene.
        SceneManager.LoadScene(2);
    }

    //Shows or hides the UI
    void ShowUI(bool show)
    {
        if (show)
        {
            //The camera can only see the "UI"layer
            ortoCamera.cullingMask = 1 << LayerMask.NameToLayer("UI");
            inputCanvas.SetActive(true);
        }
        else
        {
            //The camera can only see the "Maze"layer
            ortoCamera.cullingMask = 1 << LayerMask.NameToLayer("Maze");
            inputCanvas.SetActive(false);
        }

    }

    //Goes back to the maze generation scene
    public void BackToGeneration()
    {
        //Sets the maze paren to the checkpoint object in the exploration scene so it can be destroyed when loading the new scene.
        if (GameObject.Find("Checkpoint"))
        {
            transform.parent = GameObject.Find("Checkpoint").transform;
        }
        
    }

    //Destroys every child object of the maze
    public void DestroyMaze()
    {
        foreach (Transform t in transform)
        {
            Destroy(t.gameObject);
        }
    }

    // Update is called once per frame
    void Update()
    {
        //Shows or hides the input canvas if the user presses the "E" key.
        if (Input.GetKeyDown(KeyCode.E))
        {
            uiSoundsController.PlayHideUISound();
            if (inputCanvas)
            {
                ShowUI(!inputCanvas.activeSelf);

            }
            

        }
    }
}
