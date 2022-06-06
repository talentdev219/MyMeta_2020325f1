using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class CapitalizeLetter : MonoBehaviour
{
    InputField inputField;

    private void Start()
    {
        inputField = GetComponent<InputField>();
    }
    public void Capitalize()
    {
        if (inputField.text != "")
        {
            string text = inputField.text;
            //string first_letter = text.Substring(0, 1).ToUpper() + text.Substring(1);
            string capitals = string.Join(" ", text.Split(' ').ToList().ConvertAll(word => word.Substring(0, 1).ToUpper() + word.Substring(1)));
            inputField.text = capitals;
        }
    }

    public void EmptyTextField()
    {
        inputField.text = "";
    }
}
