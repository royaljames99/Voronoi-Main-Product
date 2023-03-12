using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;
using System.IO;

public class Seed
{
    public int x;
    public int y;
    public string hexCode;
    public int r;
    public int g;
    public int b;
    public GameObject unityObject;
    public Seed(int x, int y, string hexCode, int r, int g, int b, GameObject seedObject)
    {
        this.x = x;
        this.y = y;
        this.hexCode = hexCode;
        this.r = r;
        this.g = g;
        this.b = b;
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

    //boundary values
    public int minX = 0;
    public int maxX = 1000;
    public int minY = 500;
    public int maxY = 2000;

    //the camera
    public Camera cam;

    //algorithms
    public GameObject Delaunay;
    public GameObject Fortune;
    public GameObject GreenSibson;

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
    //voronoi region object template (mesh renderer)
    public GameObject voroRegion;
    //background image object
    public GameObject backgroundImage;

    //selectedSeed
    private Seed selectedSeed;
    public float pointerDistanceThreashold;

    public List<Seed> seeds = new List<Seed>();

    //misc
    List<DelaunayTriangle> triangulation;

    // Start is called before the first frame update
    void Start()
    {
        minXInputBox.GetComponent<txtBox>().setValue(minX);
        maxXInputBox.GetComponent<txtBox>().setValue(maxX);
        minYInputBox.GetComponent<txtBox>().setValue(minY);
        maxYInputBox.GetComponent<txtBox>().setValue(maxY);
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
        xInputBox.GetComponent<TMP_InputField>().interactable = true;
        yInputBox.GetComponent<TMP_InputField>().interactable = true;
        hexInputBox.GetComponent<TMP_InputField>().interactable = true;
        transparencySlider.GetComponent<Slider>().interactable = true;
        deleteButton.GetComponent<NonToggleableButton>().enableButton();
        xInputBox.GetComponent<txtBox>().setValue(selectedSeed.x);
        yInputBox.GetComponent<txtBox>().setValue(selectedSeed.y);
        hexInputBox.GetComponent<colorInputBox>().setValue(selectedSeed.hexCode);
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
        int randRed = rand.Next(256);
        int randGreen = rand.Next(256);
        int randBlue = rand.Next(256);
        string randHex = Convert.ToString(randRed, 16).PadLeft(2, '0') + Convert.ToString(randGreen, 16).PadLeft(2, '0') + Convert.ToString(randBlue, 16).PadLeft(2, '0');
        Seed newSeed = new Seed(x, y, randHex, randRed, randBlue, randGreen, seedObject);
        seeds.Add(newSeed);

        if(genType == 2)
        {
            if(algorithm == 0)
            {
                liveUpdateDelaunay(newSeed);
            }
        }
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
        if (!xInputBox.GetComponent<TMP_InputField>().interactable)
        {
            return;
        }
        int newX = Convert.ToInt16(xInputBox.GetComponent<TMP_InputField>().text);
        if (checkForDuplicateSeeds(newX, selectedSeed.y))
        {
            xInputBox.GetComponent<txtBox>().setValue(selectedSeed.x);
        }
        else
        {
            selectedSeed.x = newX;
            selectedSeed.unityObject.transform.position = new Vector3(newX, selectedSeed.y);
        }
    }
    public void yValChanged()
    {
        if (!yInputBox.GetComponent<TMP_InputField>().interactable)
        {
            return;
        }
        int newY = Convert.ToInt16(yInputBox.GetComponent<TMP_InputField>().text);
        if (checkForDuplicateSeeds(selectedSeed.x, newY))
        {
            yInputBox.GetComponent<txtBox>().setValue(selectedSeed.y);
        }
        else
        {
            selectedSeed.y = newY;
            selectedSeed.unityObject.transform.position = new Vector3(selectedSeed.x, newY);
        }
    }
    public void hexValChanged()
    {
        char[] validHexChars = new char[16] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F' };

        string newInput = hexInputBox.GetComponent<TMP_InputField>().text.ToUpper();
        newInput = newInput.Replace(" ", "");

        if(newInput.Length > 6)
        {
            hexInputBox.GetComponent<TMP_InputField>().text = selectedSeed.hexCode;
            return;
        }

        (int, int, int) rgb = hexValsFromCode(newInput);
        selectedSeed.hexCode = newInput;
        selectedSeed.r = rgb.Item1;
        selectedSeed.g = rgb.Item2;
        selectedSeed.b = rgb.Item3;
        
        if(genType == 2)
        {
            if(algorithm == 0)
            {
                renderDelaunay();
            }
            //more to come
        }
    }

    private (int, int, int) hexValsFromCode(string hexCode)
    {
        char[] validHexChars = new char[16] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F' };
        string red = "";
        string green = "";
        string blue = "";
        for (int i = 0; i < hexCode.Length; i++)
        {
            char c = hexCode[i];
            if (Array.Exists(validHexChars, element => element == c))
            {
                if (i < 2)
                {
                    red += c;
                }
                else if (i < 4)
                {
                    green += c;
                }
                else
                {
                    blue += c;
                }
            }
        }
        int intRed = 0;
        int intGreen = 0;
        int intBlue = 0;
        if (red != "")
        {
            intRed = Convert.ToInt32(red, 16);
        }
        if (green != "")
        {
            intGreen = Convert.ToInt32(green, 16);
        }
        if (blue != "")
        {
            intBlue = Convert.ToInt32(blue, 16);
        }
        return (intRed, intGreen, intBlue);
    }


    //boundary changes
    public void minXChanged()
    {
        int newMinX = Convert.ToInt16(minXInputBox.GetComponent<TMP_InputField>().text);
        if(newMinX < maxX)
        {
            minX = newMinX;
        }
        else
        {
            minXInputBox.GetComponent<txtBox>().setValue(minX);
        }
    }
    public void maxXChanged()
    {
        int newMaxX = Convert.ToInt16(maxXInputBox.GetComponent<TMP_InputField>().text);
        if (newMaxX > minX)
        {
            maxX = newMaxX;
        }
        else
        {
            maxXInputBox.GetComponent<txtBox>().setValue(maxX);
        }
    }
    public void minYChanged()
    {
        int newMinY = Convert.ToInt16(minYInputBox.GetComponent<TMP_InputField>().text);
        if (newMinY < maxY)
        {
            minY = newMinY;
        }
        else
        {
            minYInputBox.GetComponent<txtBox>().setValue(minY);
        }
    }
    public void maxYChanged()
    {
        int newMaxY = Convert.ToInt16(maxYInputBox.GetComponent<TMP_InputField>().text);
        if (newMaxY > minY)
        {
            maxY = newMaxY;
        }
        else
        {
            maxYInputBox.GetComponent<txtBox>().setValue(maxY);
        }
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
            else
            {
                LiveUpdatesButton.GetComponent<ToggleableButton>().enableButton();
            }
            if (buttonID != 1 && GSButton.GetComponent<ToggleableButton>().isPressed)
            {
                GSButton.GetComponent<ToggleableButton>().toggle();
            }
            else
            {
                LiveUpdatesButton.GetComponent<ToggleableButton>().enableButton();
            }
            if (buttonID != 2 && FortuneButton.GetComponent<ToggleableButton>().isPressed)
            {
                FortuneButton.GetComponent<ToggleableButton>().toggle();
            }
            else
            {
                LiveUpdatesButton.GetComponent<ToggleableButton>().disableButton();
                if(genType == 2)
                {
                    genType = 0;
                    LiveUpdatesButton.GetComponent<ToggleableButton>().toggle();
                    SpeedyButton.GetComponent<ToggleableButton>().toggle();
                }
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
        StreamReader reader = new StreamReader(imagePath);
        Texture2D tex = new Texture2D(2, 2);
        tex.LoadImage(File.ReadAllBytes(imagePath));
        Debug.Log(tex.height);
        Debug.Log(tex.width);
        backgroundImage.transform.localScale = new Vector3(tex.width, tex.height, 0);
        backgroundImage.GetComponent<RawImage>().texture = tex;
        backgroundImage.GetComponent<RawImage>().color = new Color(255, 255, 255, 1);
    }

    private void save()
    {
        int choice = EditorUtility.DisplayDialogComplex("Save", "Do you want to save a screenshot or the points", "Cancel", "Image", "Points");
        if(choice == 0)
        {
            return;
        }
        else if(choice == 1)
        {
            string saveLocation = EditorUtility.SaveFilePanel("Save", "", "screenshot", "png");
            ScreenCapture.CaptureScreenshot(saveLocation, 1);
        }
        else
        {
            string saveText = "x,y,hex,transparency";
            foreach(Seed seed in seeds)
            {
                saveText += "\n" + Convert.ToString(seed.x) + "," + Convert.ToString(seed.y) + "," + seed.hexCode;
            }
            string saveLocation = EditorUtility.SaveFilePanel("Save", "", "points", "csv");
            File.WriteAllText(saveLocation, saveText);
        }
    }

    private void template()
    {

    }

    private void load()
    {
        string saveLocation = EditorUtility.OpenFilePanel("Select file with points data", "", "csv");
        StreamReader reader = new(saveLocation);
        string full = reader.ReadToEnd();
        string[] lines = full.Split("\n");
        lines = lines.Skip(1).ToArray();
        foreach(string line in lines)
        {
            string[] vals = line.Split(",");
            (int, int, int) rgb = hexValsFromCode(vals[2]);
            seeds.Add(new Seed(Convert.ToInt16(vals[0]), Convert.ToInt16(vals[1]), vals[2], rgb.Item1, rgb.Item2, rgb.Item3, seedObject));
        }
    }

    private void generate()
    {
        if(seeds.Count > 1)
        {
            if(algorithm == 0)
            {
                genWholeDelaunay();
            }
            else if(algorithm == 1)
            {

            }
            else //fortune
            {
                genWholeFortune();
            }
        }
    }

    private void edBalls()
    {

    }



    //algorithm interfacing subroutines
    private void genWholeDelaunay()
    {
        //convert seeds to Delaunay format
        List<DelaunayPoint> delaunaySeeds = new List<DelaunayPoint>();
        foreach(Seed seed in seeds)
        {
            delaunaySeeds.Add(new DelaunayPoint(seed.x, seed.y));
        }
        DelaunayTriangle supertriangle = new DelaunayTriangle(new DelaunayPoint(-10000000, -10000000), new DelaunayPoint(0, 10000000), new DelaunayPoint(10000000, -10000000));
        triangulation = Delaunay.GetComponent<Delaunay>().getWholeTriangulation(delaunaySeeds, supertriangle);
        Delaunay.GetComponent<Delaunay>().convertWholeToVoronoi(triangulation);
        renderDelaunay();
    }

    private void liveUpdateDelaunay(Seed newSeed)
    {
        DelaunayPoint delSeed = new(newSeed.x, newSeed.y);
        List<DelaunayPoint> delaunaySeeds = new List<DelaunayPoint> { delSeed };
        if (seeds.Count > 1)
        {
            List<DelaunayTriangle> origTriangulation = new List<DelaunayTriangle>(triangulation);
            List<DelaunayTriangle> newTriangulation = Delaunay.GetComponent<Delaunay>().addPoint(delSeed, triangulation);
            (List<DelaunayTriangle> newTriangles, List<DelaunayTriangle> deletedTriangles) = Delaunay.GetComponent<Delaunay>().findChangedTriangles(origTriangulation, newTriangulation);
            Delaunay.GetComponent<Delaunay>().updateVoronoi(newTriangulation, newTriangles, deletedTriangles);
            triangulation = newTriangulation;
        }
        else
        {
            DelaunayTriangle supertriangle = new DelaunayTriangle(new DelaunayPoint(-10000000, -10000000), new DelaunayPoint(0, 10000000), new DelaunayPoint(10000000, -10000000));
            triangulation = Delaunay.GetComponent<Delaunay>().getWholeTriangulation(delaunaySeeds, supertriangle);
            Delaunay.GetComponent<Delaunay>().convertWholeToVoronoi(triangulation);
        }
        renderDelaunay();
    }

    private void genWholeGS()
    {

    }

    private void genWholeFortune()
    {
        List<FortunePoint> fortuneSeeds = new List<FortunePoint>();
        foreach(Seed seed in seeds)
        {
            fortuneSeeds.Add(new FortunePoint(seed.x, seed.y));
        }
        Fortune.GetComponent<Fortune>().generateWholeFortune(fortuneSeeds, -50, -50, 2000);
    }

    //renderers
    private void clearScreen()
    {
        GameObject[] allObjects = GameObject.FindGameObjectsWithTag("Voro");
        foreach(GameObject obj in allObjects)
        {
            Destroy(obj);
        }
    }

    private void renderDelaunay()
    {
        clearScreen();
        List<DelaunayVoronoiLine> delVLines = Delaunay.GetComponent<Delaunay>().delVLines;

        //each seed has an accompanying list of lines for drawing triangles
        List<List<DelaunayVoronoiLine>> lines = new List<List<DelaunayVoronoiLine>>();
        List<DelaunayPoint> delSeeds = new List<DelaunayPoint>();


        foreach(DelaunayVoronoiLine line in delVLines)
        {
            foreach (DelaunayPoint seed in line.seeds)
            {
                if (delSeeds.Contains(seed))
                {
                    lines[delSeeds.IndexOf(seed)].Add(line);
                }
                else
                {
                    delSeeds.Add(seed);
                    lines.Add(new List<DelaunayVoronoiLine> { line });
                }
            }
        }

        //make mesh for each voronoi region
        for(int i = 0; i < lines.Count; i++)
        {
            //collate list of points and reference triangles
            List<DelaunayPoint> points = new List<DelaunayPoint>(); //list of vertices
            points.Add(delSeeds[i]);
            int[] triangles = new int[(lines[i].Count * 3)];
            int arrayIndex = 0;
            foreach (DelaunayVoronoiLine line in lines[i])
            {
                //extract points
                bool aFound = false;
                int aIndex = -1;
                bool bFound = false;
                int bIndex = -1;
                for (int j = 0; j < points.Count; j++)
                {
                    DelaunayPoint point = points[j];
                    if (point.x == line.a.x && point.y == line.a.y)
                    {
                        aFound = true;
                        aIndex = j;
                    }
                    if (point.x == line.b.x && point.y == line.b.y)
                    {
                        bFound = true;
                        bIndex = j;
                    }
                }
                if (!aFound)
                {
                    aIndex = points.Count;
                    points.Add(line.a);
                }
                if (!bFound)
                {
                    bIndex = points.Count;
                    points.Add(line.b);
                }
                //add triangle
                triangles[arrayIndex] = 0;
                triangles[arrayIndex + 1] = aIndex;
                triangles[arrayIndex + 2] = bIndex;
                arrayIndex += 3;
            }

            //convert point list into vectors
            Vector3[] vectors = new Vector3[points.Count];
            arrayIndex = 0;
            foreach(DelaunayPoint point in points)
            {
                vectors[arrayIndex] = new Vector3(point.x, point.y, 0);
                arrayIndex++;
            }

            //assemble the mesh
            Mesh mesh = new Mesh();
            mesh.Clear();
            GameObject meshObject = Instantiate(voroRegion);
            
            //set the colour
            System.Random random = new System.Random();
            foreach (Seed seed in seeds)
            {
                if(seed.x == vectors[0].x && seed.y == vectors[0].y)
                {
                    Color color = new Color((float)((1/255.0) * seed.r), (float)((1 / 255.0) * seed.g), (float)((1 / 255.0) * seed.b), 1.0f);
                    meshObject.GetComponent<MeshRenderer>().material.color = color;
                    break;
                }
            }

            //set bounds
            Bounds bounds = new(new Vector3((minX + maxX) / 2, (minY + maxY) / 2, 0), new Vector3((minX + maxX) / 2, (minY + maxY) / 2, 0));

            meshObject.GetComponent<MeshFilter>().mesh = mesh;
            mesh.vertices = vectors;
            mesh.triangles = triangles;
            meshObject.GetComponent<MeshRenderer>().bounds = bounds;

            meshObject.SetActive(true);
        }
    }
}
