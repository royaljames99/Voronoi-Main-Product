using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;

public class Seed
{
    public int x;
    public int y;
    public Color colour;
    public GameObject unityObject;
    public Seed(int x, int y, Color colour, GameObject seedObject)
    {
        this.x = x;
        this.y = y;
        this.colour = colour;
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
    //seed input boxes, transparency slider & delete button
    public GameObject xInputBox;
    public GameObject yInputBox;
    public GameObject hexInputBox;
    public GameObject transparencySlider;
    public GameObject deleteButton;
    //boundary input boxes
    public GameObject minXInputBox;
    public GameObject maxXInputBox;
    public GameObject minYInputBox;
    public GameObject maxYInputBox;
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

    //selectedSeed
    private Seed selectedSeed;
    public float pointerDistanceThreashold;

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
                //point cursor handling (selecting seeds)
                if (cursorType == 0)
                {
                    if (seeds.Count != 0)
                    {
                        Vector3 mousePos = cam.ScreenToWorldPoint(Input.mousePosition, cam.stereoActiveEye);
                        int x = Convert.ToInt32(mousePos.x);
                        int y = Convert.ToInt32(mousePos.y);
                        //loop through points and find closest point within threashold
                        Seed closestSeed = seeds[0];
                        float closestDistance = 1000000;
                        foreach (Seed seed in seeds)
                        {
                            if (seed.x == x && seed.y == y)
                            {
                                closestSeed = seed;
                                break;
                            }
                            else
                            {
                                float dist = (float)Math.Sqrt(Math.Pow((seed.x - mousePos.x), 2) + Math.Pow((seed.y - mousePos.y), 2));
                                if (dist <= pointerDistanceThreashold && dist < closestDistance)
                                {
                                    closestDistance = dist;
                                    closestSeed = seed;
                                }
                            }
                        }
                        if (closestDistance != 1000000)
                        {
                            selectedSeed = closestSeed;
                            selectedSeedChanged();
                        }
                    }
                }
                //adding seeds
                else if (cursorType == 1)
                {
                    Debug.Log("adding seed");
                    addSeed();
                    selectedSeed = seeds[seeds.Count - 1];
                    selectedSeedChanged();
                }                 
            }
        }
    }

    private void selectedSeedChanged()
    {
        xInputBox.GetComponent<txtBox>().setValue(selectedSeed.x);
        yInputBox.GetComponent<txtBox>().setValue(selectedSeed.y);
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
        System.Random rand = new System.Random();
        Color colour = new Color(rand.Next(255), rand.Next(255), rand.Next(255), 1);
        Seed newSeed = new Seed(x, y, colour, seedObject);
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
    
    //delete seed
    private void deleteSeed()
    {
        Destroy(selectedSeed.unityObject);
        seeds.Remove(selectedSeed);
        xInputBox.GetComponent<TMP_InputField>().interactable = false;
        xInputBox.GetComponent<TMP_InputField>().text = "";
        yInputBox.GetComponent<TMP_InputField>().interactable = false;
        yInputBox.GetComponent<TMP_InputField>().text = "";
        hexInputBox.GetComponent<TMP_InputField>().interactable = false;
        hexInputBox.GetComponent<TMP_InputField>().text = "";
        transparencySlider.GetComponent<Slider>().interactable = false;
        transparencySlider.GetComponent<Slider>().value = 0;
        deleteButton.GetComponent<NonToggleableButton>().disableButton();
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


    //boundary changes
    public void minXChanged()
    {

    }
    public void maxXChanged()
    {

    }
    public void minYChanged()
    {

    }
    public void maxYChanged()
    {

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
            deleteSeed();
        }
        else if (buttonID < 7) //cursor tool buttons
        {
            cursorType = buttonID - 4;
            if(buttonID != 4 && PointerCursorButton.GetComponent<ToggleableButton>().isPressed)
            {
                PointerCursorButton.GetComponent<ToggleableButton>().toggle();
            }
            if(buttonID != 5 && SeedCursorButton.GetComponent<ToggleableButton>().isPressed)
            {
                SeedCursorButton.GetComponent<ToggleableButton>().toggle();
            }
            if(buttonID != 6 && PanCursorButton.GetComponent<ToggleableButton>().isPressed)
            {
                PanCursorButton.GetComponent<ToggleableButton>().toggle();
            }
        }
        else if (buttonID < 10) //Generation setting buttons
        {
            genType = buttonID - 7;
            if (buttonID != 7 && SpeedyButton.GetComponent<ToggleableButton>().isPressed)
            {
                SpeedyButton.GetComponent<ToggleableButton>().toggle();
            }
            if (buttonID != 8 && AnimationButton.GetComponent<ToggleableButton>().isPressed)
            {
                AnimationButton.GetComponent<ToggleableButton>().toggle();
            }
            if (buttonID != 9 && LiveUpdatesButton.GetComponent<ToggleableButton>().isPressed)
            {
                LiveUpdatesButton.GetComponent<ToggleableButton>().toggle();
                GenerateButton.GetComponent<NonToggleableButton>().enableButton();
            }
            else if (buttonID == 9)
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
