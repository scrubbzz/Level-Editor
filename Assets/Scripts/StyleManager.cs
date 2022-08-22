using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StyleManager : MonoBehaviour
{/// <summary>
/// This class holds an array of the different buttons we can use while using the grid window...
/// </summary>
    public ButtonStyle[] buttonStyles;//We set the variables for each button in the inspector.
}
[System.Serializable]
public struct ButtonStyle
{
    public Texture2D icon;
    public string ButtonTex;
    [HideInInspector]
    public GUIStyle nodeStyle;
    public GameObject prefabObject;
}

