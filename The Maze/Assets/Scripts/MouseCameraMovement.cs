using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MouseCameraMovement : MonoBehaviour
{

    //Defines the speed of the camera
    public float speedH = 2.0f;
    public float speedV = 2.0f;

    //Sets the forward vector of the camera
    public Vector3 forward;

    //Initial pitch of the camera
    public float pitch = 0.0f;


    //Initializes the head rotation 
    public bool canRotateHead = false;






    // Use this for initialization
    void Start()
    {

    }

    

    // Update is called once per frame
    void Update()
    {

        //If the player can rotate the head it gets the value from the mouse input and multiplies by the speed.
        //Then gives the new rotation to the camera.
        if (canRotateHead)
        {
            pitch -= speedV * Input.GetAxis("Mouse Y");

            pitch = Mathf.Clamp(pitch,-40.0f, 40.0f);

            transform.eulerAngles = new Vector3(pitch, transform.eulerAngles.y, 0.0f);

            forward = transform.forward;
        }

        

    }
}
