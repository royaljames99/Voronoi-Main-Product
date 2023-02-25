using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.EventSystems;

public class Seed
{
    public int x;
    public int y;
    public GameObject unityObject;
    public Seed(int x, int y, GameObject seedObject)
    {
        this.x = x;
        this.y = y;
        unityObject = UnityEngine.Object.Instantiate(seedObject);
        unityObject.transform.position = new Vector3(x, y, 0);
        unityObject.SetActive(true);
    }
}

public class VoronoiMaster : MonoBehaviour
{
    public int algorithm = 0; //0:delaunay, 1:GS, 2:fortune
    public int cursorType = 0; //0:pointer, 1:seed, 2:pan
    public int genType = 0; //0:speedy, 1:animation, 2:live updates

    //the camera
    public Camera cam;

    //algorithm buttons
    public GameObject DelaunayButton;
    public GameObject GSButton;
    public GameObject FortuneButton;
    //seed input buttons
    public GameObject txtBox;
    //cursor tool buttons
    public GameObject PointerCursorButton;
    public GameObject SeedCursorButton;
    public GameObject PanCursorButton;
    //generation type buttons
    public GameObject SpeedyButton;
    public GameObject AnimationButton;
    public GameObject LiveUpdatesButton;
    //other buttons
    public GameObject LoadBackgroundButton;
    public GameObject SaveButton;
    public GameObject TemplateButton;
    public GameObject LoadButton;
    public GameObject GenerateButton;
    public GameObject EdBallsButton;

    //seed object used as model
    public GameObject seedObject;

    public List<Seed> seeds = new List<Seed>();

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (!EventSystem.current.IsPointerOverGameObject())
            {
                Debug.Log("asdf");
                if (cursorType == 1)
                {
                    Debug.Log("adding seed");
                    addSeed();
                }
            }
        }
    }

    // Add new seed
    private void addSeed()
    {
        Vector3 mousePos = cam.ScreenToWorldPoint(Input.mousePosition, cam.stereoActiveEye);
        int x = Convert.ToInt32(mousePos.x);
        int y = Convert.ToInt32(mousePos.y);
        if(checkForDuplicateSeeds(x, y))
        {
            Debug.Log("duplicate");
            return;
        }
        Seed newSeed = new Seed(x, y, seedObject);
        seeds.Add(newSeed);
    }

    private bool checkForDuplicateSeeds(int x, int y)
    {
        foreach(Seed seed in seeds)
        {
            if(seed.x == x && seed.y == y)
            {
                return true;
            }
        }
        return false;
    }
    
    
    //input box changes
    public void xValChanged()
    {
        return;
    }
    public void yValChanged()
    {
        return;
    }
    public void hexValChanged()
    {
        return;
    }

    
    //group and handle button inputs
    public void ButtonInputs(int buttonID)
    {
        if (buttonID < 3) //the algorithm buttons
        {
            algorithm = buttonID;
            if (buttonID != 0 && DelaunayButton.GetComponent<ToggleableButton>().isPressed)
            {
                DelaunayButton.GetComponent<ToggleableButton>().toggle();
            }
            if (buttonID != 1 && GSButton.GetComponent<ToggleableButton>().isPressed)
            {
                GSButton.GetComponent<ToggleableButton>().toggle();
            }
            if (buttonID != 2 && FortuneButton.GetComponent<ToggleableButton>().isPressed)
            {
                FortuneButton.GetComponent<ToggleableButton>().toggle();
            }
        }
        else if (buttonID == 3) //delete seed
        {

        }
        else if (buttonID < 7) //cursor tool buttons
        {
            cursorType = buttonID - 4;
            if(buttonID != 3 && PointerCursorButton.GetComponent<ToggleableButton>().isPressed)
            {
                PointerCursorButton.GetComponent<ToggleableButton>().toggle();
            }
            if(buttonID != 4 && SeedCursorButton.GetComponent<ToggleableButton>().isPressed)
            {
                SeedCursorButton.GetComponent<ToggleableButton>().toggle();
            }
            if(buttonID != 5 && PanCursorButton.GetComponent<ToggleableButton>().isPressed)
            {
                PanCursorButton.GetComponent<ToggleableButton>().toggle();
            }
        }
        else if (buttonID < 10) //Generation setting buttons
        {
            genType = buttonID - 7;
            if (buttonID != 6 && SpeedyButton.GetComponent<ToggleableButton>().isPressed)
            {
                SpeedyButton.GetComponent<ToggleableButton>().toggle();
            }
            if (buttonID != 7 && AnimationButton.GetComponent<ToggleableButton>().isPressed)
            {
                AnimationButton.GetComponent<ToggleableButton>().toggle();
            }
            if (buttonID != 8 && LiveUpdatesButton.GetComponent<ToggleableButton>().isPressed)
            {
                LiveUpdatesButton.GetComponent<ToggleableButton>().toggle();
                GenerateButton.GetComponent<NonToggleableButton>().enableButton();
            }
            else if (buttonID == 8)
            {
                GenerateButton.GetComponent<NonToggleableButton>().disableButton(); //can't hit generate when doing live updates
            }
        }
        else if (buttonID == 10) //load background image button
        {
            selectBackgroundImage();
        }
        else if (buttonID == 11)
        {
            save();
        }
        else if (buttonID == 12)
        {
            template();
        }
        else if (buttonID == 13)
        {
            load();
        }
        else if (buttonID == 14)
        {
            generate();
        }
        else if (buttonID == 15)
        {
            edBalls();
        }
    }

    private void selectBackgroundImage()
    {
        string imagePath = EditorUtility.OpenFilePanel("Select background image", "", "png");
        Debug.Log(imagePath);
    }

    private void save()
    {

    }

    private void template()
    {

    }

    private void load()
    {

    }

    private void generate()
    {

    }

    private void edBalls()
    {

    }

}
