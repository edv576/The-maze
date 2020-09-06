using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShaonController : MonoBehaviour
{
    //Script that controls the actions of Shaon, the playable character.
    Animator anim;

    //Rotation speed for the player
    public float rotationSpeed;

    //Camera that follows the player
    public Camera playerCamera;

    //Camera that is located in the winning location.
    public Camera victoryCamera;

    //Clip of the song played after the player arrives to the final floor tile.
    public AudioClip winClip;

    //Audisource with background music
    public AudioSource checkAudioSource;

    //Floating text with victory line
    public GameObject victoryText;

    bool victory = false;
    // Start is called before the first frame update
    void Start()
    {
        //Gets the animator component.
        anim = GetComponent<Animator>();

        //Gets the player to the initial position of the maze.
        Vector3 firstPos = GameObject.Find("Floor_0_0").transform.position;
        transform.position = new Vector3(firstPos.x, transform.position.y, firstPos.z);
        
        //Get the player camera from the children's components.
        playerCamera = GetComponentInChildren<Camera>();

        //Get the victory camera and disable it at the beginning.
        victoryCamera = GameObject.Find("Victory camera").GetComponent<Camera>();
        victoryCamera.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        //Changes the animations depending of user input. "W" key to go forward and "S" to go backwards.
        anim.SetFloat("vertical", Input.GetAxis("Vertical"));
        anim.SetFloat("horizontal", Input.GetAxis("Horizontal"));

        //If the player didn't get to the victory floor tile yet, its possible to move around.
        if (!victory)
        {
            //Activates the jump animation
            if (Input.GetKeyDown(KeyCode.Space))
            {
                anim.SetBool("jump", true);
            }
            if (Input.GetKeyUp(KeyCode.Space))
            {
                anim.SetBool("jump", false);
            }

            //Rotates to the right.
            if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
            {
                anim.SetFloat("turn", 1.0f);
                transform.eulerAngles += new Vector3(0, rotationSpeed * Time.deltaTime, 0);
            }

            if (Input.GetKeyUp(KeyCode.RightArrow) || Input.GetKeyUp(KeyCode.D))
            {
                anim.SetFloat("turn", 0);
            }

            //Rotates to the left
            if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
            {
                anim.SetFloat("turn", -1.0f);
                transform.eulerAngles -= new Vector3(0, rotationSpeed * Time.deltaTime, 0);
            }

            if (Input.GetKeyUp(KeyCode.LeftArrow) || Input.GetKeyUp(KeyCode.A))
            {
                anim.SetFloat("turn", 0);
            }
        }


    }


    //In case the player arrives to the final floor tile
    private void OnTriggerEnter(Collider other)
    {
        //If the trigger activated corresponds to the final floor tile
        if(other.gameObject.name == "Victory button" && !victory)
        {
            //Victory flag is set to true.
            victory = true;

            //Activates the victory animation.
            anim.SetBool("win", true);

            //Deactivates the victory button collider.
            other.gameObject.GetComponent<Collider>().enabled = false;

            //Deactivates the player camera and activates the victory camera.
            victoryCamera = other.transform.parent.gameObject.GetComponentInChildren<Camera>();
            playerCamera.enabled = false;
            victoryCamera.enabled = true;

            //Disables the turn and jump animation.
            anim.SetFloat("turn", 0);
            anim.SetBool("jump", false);

            //Changes the background clip to the victory clip.
            checkAudioSource.clip = winClip;
            checkAudioSource.Play();

            //Enables the victory floating text.
            victoryText.SetActive(true);
        }
    }
}
