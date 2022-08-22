using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class GridMapCreator : EditorWindow
{
    Vector2 offset;
    Vector2 drag;
    List<List<Node>> nodes;//Nodes on the grid. On each node can be placed one object.
    List<List<PartScripts>> parts;
    GUIStyle empty;
    Vector2 nodePos;

    StyleManager styleManager;//Reference to all of the buttons we will have in the window.

    bool isErasing;//For our erasing feature;

    Rect menuBarRect;

    GUIStyle currentStyle;//This represents the texture of the object we want to place at a particular moment.

    public GameObject theMap;

    [MenuItem("Window/Grid Map Creator")]
    private static void OpenWindow()
    {
        GridMapCreator window = GetWindow<GridMapCreator>();
        window.titleContent = new GUIContent("Grid Map Creator");//Creating the window for our tool and giving it a name.
    }

    private void OnEnable()
    {
        SetUpStyles();
        SetUpNodesAndParts();
        SetUpMap(GetTheMap());
    }

    private GameObject GetTheMap()
    {
        return theMap;
    }

    private void SetUpMap(GameObject _theMap)
    {
        Debug.Log("1");
        Debug.Log("this is beng calklked");
        try { _theMap = GameObject.FindGameObjectWithTag("Map"); } catch (Exception e) { }
        if(_theMap == null)
        {
            Debug.Log("null map!");
            _theMap = new GameObject("Map");

            _theMap.tag = "Map";
        }
    }

    private void RestoreTheMap(GameObject theMap)//This function saves the work that you did on the map so you can close and open it and previous work will still be there.
    {
        if(theMap.transform.childCount > 0)
        {
            for (int i = 0; i < theMap.transform.childCount; i++)
            {
                int ii = theMap.transform.GetChild(i).GetComponent<PartScripts>().row;
                int jj = theMap.transform.GetChild(i).GetComponent<PartScripts>().column;
                GUIStyle theStyle = theMap.transform.GetChild(i).GetComponent<PartScripts>().style;
                nodes[ii][jj].SetStyle(theStyle);
                parts[ii][jj] = theMap.transform.GetChild(i).GetComponent<PartScripts>();
                parts[ii][jj].part = theMap.transform.GetChild(i).gameObject;
                parts[ii][jj].name = theMap.transform.GetChild(i).name;
                parts[ii][jj].row = ii;
                parts[ii][jj].column = jj;
            }
        }
    }

    private void SetUpStyles()
    {
        try
        {
            styleManager = GameObject.FindGameObjectWithTag("StyleManager").GetComponent<StyleManager>();
            for (int i = 0; i < styleManager.buttonStyles.Length; i++)
            {
                styleManager.buttonStyles[i].nodeStyle = new GUIStyle();//Allocating space in the memory for each button's nodestyle.
                styleManager.buttonStyles[i].nodeStyle.normal.background = styleManager.buttonStyles[i].icon;//Each button in the stylemanager gets its own respective icon for its background.
            }
        }
        catch (Exception e) { }
        empty = styleManager.buttonStyles[0].nodeStyle;
        currentStyle = styleManager.buttonStyles[1].nodeStyle;//Whatever is in index[1] of the buttonStyles array will be the thing the user draws if they try to draw without pressing a button.
    }

    private void SetUpNodesAndParts()
    {
        nodes = new List<List<Node>>();
        parts = new List<List<PartScripts>>();
        for (int i = 0; i < 20; i++)
        {
            nodes.Add(new List<Node>());
            parts.Add(new List<PartScripts>());
            for (int j = 0; j < 10; j++)
            {
                nodePos.Set(i * 30, j * 30);
                nodes[i].Add(new Node(nodePos, 30, 30, empty));
                parts[i].Add(null);
            }
        }
    }

    private void OnGUI()//All of the functions in here will be called whenever the player does some interation (input) with our UI window, so potentially multiple times per frame...
    {
        DrawGrid();
        DrawNodes();
        DrawMenuBar();
        ProcessNodes(Event.current);
        ProcessGrid(Event.current);//The events class dectects user inputs such as key presses and mouse clicks
        if (GUI.changed)
        {
            Repaint();
        }
    }

    private void DrawMenuBar()
    {
        menuBarRect = new Rect(0, 0, position.width, 20);
        GUILayout.BeginArea(menuBarRect, EditorStyles.toolbar);
        GUILayout.BeginHorizontal();

        for (int i = 0; i < styleManager.buttonStyles.Length; i++)
        {
            if (GUILayout.Toggle((currentStyle == styleManager.buttonStyles[i].nodeStyle), new GUIContent(styleManager.buttonStyles[i].ButtonTex), EditorStyles.toolbarButton, GUILayout.Width(80)))
            {
                currentStyle = styleManager.buttonStyles[i].nodeStyle;//Changing the current style to be that of whichever button the user presses.
            }
        }

        /*if(GUILayout.Toggle((currentStyle == styleManager.buttonStyles[1].nodeStyle), new GUIContent("Chicken"), EditorStyles.toolbarButton, GUILayout.Width(80)))
        {
            currentStyle = styleManager.buttonStyles[1].nodeStyle;
        }
        if (GUILayout.Toggle((currentStyle == styleManager.buttonStyles[2].nodeStyle), new GUIContent("Cow"), EditorStyles.toolbarButton, GUILayout.Width(80)))
        {
            currentStyle = styleManager.buttonStyles[2].nodeStyle;
        }
        if (GUILayout.Toggle((currentStyle == styleManager.buttonStyles[3].nodeStyle), new GUIContent("Lamb"), EditorStyles.toolbarButton, GUILayout.Width(80)))
        {
            currentStyle = styleManager.buttonStyles[3].nodeStyle;
        }
        if (GUILayout.Toggle((currentStyle == styleManager.buttonStyles[4].nodeStyle), new GUIContent("Tree"), EditorStyles.toolbarButton, GUILayout.Width(80)))
        {
            currentStyle = styleManager.buttonStyles[4].nodeStyle;
        }*/
        GUILayout.EndHorizontal();//Begin and EndHorizontal so the buttons are not drawn on top of each other and actually one by one horizontally.
        GUILayout.EndArea();
    }

    private void ProcessNodes(Event e)
    {
        int row = (int)(e.mousePosition.x - offset.x) / 30;
        int column = (int)(e.mousePosition.y - offset.y) / 30;
        if ((e.mousePosition.x - offset.x) < 0 || (e.mousePosition.x - offset.x > 600) || (e.mousePosition.y - offset.y < 0) || (e.mousePosition.y - offset.y > 300))
        { }//The else statement below is what we actually want to happen. Having the 'if statement' directly above means that the grid will only move around if the mouse is being dragged NOT on the nodes area.
        else
        {
            if (e.type == EventType.MouseDown)//If user left-clicks...
            {
                if (nodes[row][column].style.normal.background.name == "Empty")
                {
                    isErasing = false;
                }
                else
                {
                    isErasing = true;
                }

                if (isErasing)
                {
                    nodes[row][column].SetStyle(empty);
                    GUI.changed = true;
                }
                else
                {
                    nodes[row][column].SetStyle(styleManager.buttonStyles[1].nodeStyle);
                    GUI.changed = true;
                }
            }
            if (e.type == EventType.MouseDrag)//To draw stuff on the grid if the user is dragging...
            {
                PaintNodes(row, column);
                e.Use();//Item only drawn when the event actually happens, i.e the mouse is dragged onto a new node that is empty.
            }
        }
    }

    private void PaintNodes(int row, int column)//This is the function that actually draws the images onto the green nodes.
    {
        Debug.Log("2");
        if (isErasing)
        {
            if (parts[row][column] != null)
            {
                nodes[row][column].SetStyle(empty);
                DestroyImmediate(parts[row][column].gameObject);
                GUI.changed = true;
            }
            parts[row][column] = null;
        }
        else
        {
            if (parts[row][column] == null)//If there's nothing on that node the user clicked on, then do something.
            {
                //nodes[row][column].SetStyle(styleManager.buttonStyles[1].nodeStyle);
                nodes[row][column].SetStyle(currentStyle);//Now when the user presses, the image that is drawn is based on the 'currentStyle' which changes based on the buttons we press on the window.
                GameObject g = Instantiate(Resources.Load("MapParts/" + currentStyle.normal.background.name)) as GameObject;//Spawns the currentStyle object at the node user clicked on.
                g.name = currentStyle.normal.background.name;
                g.transform.position = new Vector3(column * 10, 0, row * 10) + Vector3.forward * 5 + Vector3.right * 5;
                g.transform.parent = theMap.transform;
                parts[row][column] = g.GetComponent<PartScripts>();
                parts[row][column].part = g;
                parts[row][column].name = g.name;
                parts[row][column].row = row;
                parts[row][column].column = column;
                parts[row][column].style = currentStyle;
                GUI.changed = true;

            }
        }
    }

    private void DrawNodes()
    {
        for (int i = 0; i < 20; i++)
        {
            for (int j = 0; j < 10; j++)
            {
                nodes[i][j].Draw();//Makes the nodes actually visible in the window.
            }
        }
    }

    private void ProcessGrid(Event e)//Checks what the current user input is then does something accordingly.
    {
        drag = Vector2.zero;
        switch (e.type)
        {
            case EventType.MouseDrag://Checking whenever the user left-clicks on the window grid to drag around.
                if (e.button == 0)
                {
                    OnMouseDrag(e.delta);
                }
                break;
        }
    }

    private void OnMouseDrag(Vector2 delta)//Sets the mouse postion (that was set by the user when dragging around) to the 'drag' variable which will be used to change the positioning of the grid.
    {
        drag = delta;

        for (int i = 0; i < 20; i++)
        {
            for (int j = 0; j < 10; j++)
            {
                nodes[i][j].Drag(delta);//Move each node around based on the dragging of the mouse.
            }
        }

        GUI.changed = true;
    }

    private void DrawGrid()
    {
        int widthDivider = Mathf.CeilToInt(position.width / 20);
        int heightDivider = Mathf.CeilToInt(position.height / 20);
        Handles.BeginGUI();
        Handles.color = new Color(0.5f, 0.5f, 0.5f, 0.2f);
        offset += drag; //Everytime the user drags on the window, offset will change.
        Vector3 newOffset = new Vector3(offset.x % 20, offset.y % 20, 0);
        for (int i = 0; i < widthDivider; i++)//These two loops just draw some lines on the window for the tool to represent the grid. The postions of the lines are based off the offset value, which is changed above 
        {                                                                                                                                                                                       //in the ProcessGrid();
            Handles.DrawLine(new Vector3(20 * i, -20, 0) + newOffset, new Vector3(20 * i, position.height, 0) + newOffset);
        }
        for (int i = 0; i < heightDivider; i++)
        {
            Handles.DrawLine(new Vector3(-20, 20 * i, 0) + newOffset, new Vector3(position.width, 20 * i, 0) + newOffset);
        }
        Handles.color = Color.white;
        Handles.EndGUI();
    }
}
