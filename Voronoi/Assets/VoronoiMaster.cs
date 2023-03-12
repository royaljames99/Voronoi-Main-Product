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
        xInputBox.GetComponent<TMP_InputField>().interactable = true;
        yInputBox.GetComponent<TMP_InputField>().interactable = true;
        hexInputBox.GetComponent<TMP_InputField>().interactable = true;
        transparencySlider.GetComponent<Slider>().interactable = true;
        deleteButton.GetComponent<NonToggleableButton>().enableButton();
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
        if (!xInputBox.GetComponent<TMP_InputField>().interactable)
        {
            return;
        }
        int newX = Convert.ToInt16(xInputBox.GetComponent<TMP_InputField>().text);
        selectedSeed.x = newX;
        selectedSeed.unityObject.transform.position = new Vector3(newX, selectedSeed.y);
    }
    public void yValChanged()
    {
        if (!yInputBox.GetComponent<TMP_InputField>().interactable)
        {
            return;
        }
        int newY = Convert.ToInt16(yInputBox.GetComponent<TMP_InputField>().text);
        selectedSeed.y = newY;
        selectedSeed.unityObject.transform.position = new Vector3(selectedSeed.x, newY);
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
        List<DelaunayTriangle> triangulation = Delaunay.GetComponent<Delaunay>().getWholeTriangulation(delaunaySeeds, supertriangle);
        Delaunay.GetComponent<Delaunay>().convertWholeToVoronoi(triangulation);
        foreach (DelaunayVoronoiLine l in Delaunay.GetComponent<Delaunay>().delVLines)
        {
            Debug.Log("Segment((" + Convert.ToString(l.a.x) + "," + Convert.ToString(l.a.y) + "),(" + Convert.ToString(l.b.x) + "," + Convert.ToString(l.b.y) + "))");
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
    private void renderDelaunay()
    {
        List<DelaunayVoronoiLine> delVLines = Delaunay.GetComponent<Delaunay>().delVLines;

        //each seed has an accompanying list of lines for drawing triangles
        List<List<DelaunayVoronoiLine>> lines = new List<List<DelaunayVoronoiLine>>();
        List<DelaunayPoint> seeds = new List<DelaunayPoint>();


        foreach(DelaunayVoronoiLine line in delVLines)
        {
            foreach (DelaunayPoint seed in line.seeds)
            {
                if (seeds.Contains(seed))
                {
                    lines[seeds.IndexOf(seed)].Add(line);
                }
                else
                {
                    seeds.Add(seed);
                    lines.Add(new List<DelaunayVoronoiLine> { line });
                }
            }
        }

        //make mesh for each voronoi region
        Debug.Log(lines.Count);
        for(int i = 0; i < lines.Count; i++)
        {
            //collate list of points and reference triangles
            List<DelaunayPoint> points = new List<DelaunayPoint>(); //list of vertices
            points.Add(seeds[i]);
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
            /*
            foreach(int integer in triangles)
            {
                Debug.Log(integer);
            }*/


            //convert point list into vectors
            Vector3[] vectors = new Vector3[points.Count];
            arrayIndex = 0;
            foreach(DelaunayPoint point in points)
            {
                vectors[arrayIndex] = new Vector3(point.x, point.y, 0);
                arrayIndex++;
            }


            //whack the coloUrs on there
            Color[] colors = new Color[vectors.Length];
            for (int a = 0; a < vectors.Length; a++)
            {
                colors[a] = new Color(255, 255, 255);
            }

            Mesh mesh = new Mesh();
            mesh.Clear();
            GameObject meshObject = Instantiate(voroRegion);
            meshObject.active = true;
            System.Random random = new System.Random();
            meshObject.GetComponent<MeshRenderer>().material.color = new Color(random.Next(0, 255), random.Next(0, 255), random.Next(0, 255), 1);
            Debug.Log(meshObject.GetComponent<MeshRenderer>().material.color.r);
            //meshObject.GetComponent<MeshRenderer>().material.color = Color.red;
            //GameObject meshObject = new GameObject("VoroRegion", typeof(MeshFilter), typeof(MeshRenderer));
            meshObject.GetComponent<MeshFilter>().mesh = mesh;
            mesh.vertices = vectors;
            mesh.triangles = triangles;
            //mesh.colors = colors;
        }

        
    }
}
