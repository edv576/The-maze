using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BlinkingText : MonoBehaviour
{
    //public Text text;
    TextMeshProUGUI textContinue;



    private void Start()

    {

        //Get text component from the game object
        textContinue = GetComponent<TextMeshProUGUI>();

        //Call function for blinking text
        StartBlinking();

    }


    //Corroutine in charge of the text blinking
    IEnumerator Blink()

    {

        while (true)

        {
            //Pick what to do case the alpha of the color of the text is certain number
            switch (textContinue.color.a.ToString())

            {

                case "0":

                    textContinue.color = new Color(textContinue.color.r, textContinue.color.g, textContinue.color.b, 1);

                    yield return new WaitForSeconds(0.5f);

                    break;

                case "1":

                    textContinue.color = new Color(textContinue.color.r, textContinue.color.g, textContinue.color.b, 0);

                    yield return new WaitForSeconds(0.5f);

                    break;

            }

        }

    }


    //Call the blinking coroutine. First stops it then starts it again
    void StartBlinking()

    {

        StopCoroutine("Blink");

        StartCoroutine("Blink");

    }


    //Stops the blinking coroutine
    void StopBlinking()

    {

        StopCoroutine("Blink");

    }

}

