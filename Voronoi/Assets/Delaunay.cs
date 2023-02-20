using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DelaunayPoint{

    public float x;
    public float y;

    public DelaunayPoint(float x, float y)
    {
        this.x = x;
        this.y = y;
    }
}

public class DelaunayEdge
{
    public DelaunayPoint a;
    public DelaunayPoint b;
    public DelaunayVoronoiLine dualLine;

    public DelaunayEdge(DelaunayPoint a, DelaunayPoint b)
    {
        this.a = a;
        this.b = b;
    }
}

public class DelaunayVoronoiLine : DelaunayEdge //inherit delaunay edge to act as an edge while keeping track of seeds
{
    DelaunayPoint[] seeds = new DelaunayPoint[2];
    
    public DelaunayVoronoiLine(DelaunayPoint a, DelaunayPoint b, DelaunayPoint seed1, DelaunayPoint seed2) : base(a, b)
    {
        seeds[0] = seed1;
        seeds[1] = seed2;
    }
}

public class DelaunayCircle
{
    public DelaunayPoint centre;
    public float radius;

    public DelaunayCircle(DelaunayPoint centre, float radius)
    {
        this.centre = centre;
        this.radius = radius;
    }
}

public class DelaunayTriangle
{
    public DelaunayPoint a;
    public DelaunayPoint b;
    public DelaunayPoint c;
    public DelaunayEdge[] edges;
    public DelaunayCircle circumcircle;

    public DelaunayTriangle(DelaunayPoint a, DelaunayPoint b, DelaunayPoint c)
    {
        this.a = a;
        this.b = b;
        this.c = c;
        edges = new DelaunayEdge[3];
        edges[0] = new DelaunayEdge(a, b);
        edges[1] = new DelaunayEdge(a, c);
        edges[2] = new DelaunayEdge(b, c);
        circumcircle = genCircumCircle();
    }

    private DelaunayCircle genCircumCircle()
    {
        float xab = (a.x + b.x) / 2;
        float xac = (a.x + c.x) / 2;
        float yab = (a.y + b.y) / 2;
        float yac = (a.y + c.y) / 2;

        //if two points have same y then x is directly in the middle
        float x;
        float y;
        if (a.y == b.y)
        {
            x = (a.x + b.x) / 2;
            y = ((-((a.x - c.x) / (a.y - c.y))) * (x - xac) + yac);
        }
        else if(a.y == c.y)
        {
            x = (a.x + c.x) / 2;
            y = ((-((a.x - b.x) / (a.y - b.y))) * (x - xab) + yab);
        }
        else if(b.y == c.y)
        {
            x = (b.x + c.x) / 2;
            y = ((-((a.x - b.x) / (a.y - b.y))) * (x - xab) + yab);
        }
        else
        {
            x = ((((-a.x * xab) + (xab * b.x)) / (a.y - b.y)) + (((a.x * xac) - (xac * c.x)) / (a.y - c.y)) + yac - yab) / (((-a.x + b.x) / (a.y - b.y)) + ((a.x - c.x) / (a.y - c.y)));
            y = ((-((a.x - b.x) / (a.y - b.y))) * (x - xab) + yab);
        }

        DelaunayPoint centre = new DelaunayPoint(x, y);
        float radius = (float) Math.Sqrt((Math.Pow((x - b.x), 2)) + (Math.Pow((y - b.y), 2)));
        DelaunayCircle circle = new DelaunayCircle(centre, radius);
        return circle;
    }
}

public class Delaunay : MonoBehaviour
{
    //internal variables
    List<DelaunayVoronoiLine> delVLines = new List<DelaunayVoronoiLine>(); //finished Voronoi Lines

    // Start is called before the first frame update
    void Start()
    {
        VoronoiMaster VM = GameObject.FindGameObjectWithTag("GameController").GetComponent<VoronoiMaster>();
        Debug.Log(VM.interesting);

        Debug.Log("Adding Points");
        List<DelaunayPoint> points = new List<DelaunayPoint>();
        points.Add(new DelaunayPoint(0, 0));
        points.Add(new DelaunayPoint(4, 2));
        points.Add(new DelaunayPoint(4, -2));
        points.Add(new DelaunayPoint(8, 0));

        Debug.Log("Making triangulation");
        List<DelaunayPoint> supertrianglePoints = new List<DelaunayPoint>();
        supertrianglePoints.Add(new DelaunayPoint(-100, -100));
        supertrianglePoints.Add(new DelaunayPoint(0, 100));
        supertrianglePoints.Add(new DelaunayPoint(100, -100));
        DelaunayTriangle supertriangle = new DelaunayTriangle(supertrianglePoints[0], supertrianglePoints[1], supertrianglePoints[2]);
        List<DelaunayTriangle> triangulation = getWholeTriangulation(points, supertriangle);
        Debug.Log("OUTPUTTING TRIANGULATION");
        for (int i = 0; i < triangulation.Count; i++)
        {
            Debug.Log(Convert.ToString(triangulation[i].a.x) + "," + Convert.ToString(triangulation[i].a.y) + "    " + Convert.ToString(triangulation[i].b.x) + "," + Convert.ToString(triangulation[i].b.y) + "    " + Convert.ToString(triangulation[i].c.x) + "," + Convert.ToString(triangulation[i].c.y));
        }
        Debug.Log("Converting");
        convertWholeToVoronoi(triangulation);
        Debug.Log(delVLines.Count);

        foreach (DelaunayVoronoiLine l in delVLines)
        {
            Debug.Log("Segment((" + Convert.ToString(l.a.x) + "," + Convert.ToString(l.a.y) + "),(" + Convert.ToString(l.b.x) + "," + Convert.ToString(l.b.y) + "))"); 
        }
        Debug.Log("Done");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private bool checkEdgeEquality(DelaunayEdge e1, DelaunayEdge e2)
    {
        if (((e1.a == e2.a) && (e1.b == e2.b)) | ((e1.a == e2.b) && (e1.b == e2.a)))
            {
            return true;
        }
        return false;
    }

    private bool checkIfSharesEdges(DelaunayEdge edge, DelaunayTriangle currentTri, List<DelaunayTriangle> triangleSet)
    {
        foreach(DelaunayTriangle tri in triangleSet)
        {
            if (tri != currentTri)
            {
                foreach (DelaunayEdge e in tri.edges)
                {
                    if (checkEdgeEquality(e, edge))
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    public List<DelaunayTriangle> addPoint(DelaunayPoint point, List<DelaunayTriangle> triangulation)
    {
        List<DelaunayTriangle> btriangles = new List<DelaunayTriangle>();
        //find no longer valid triangles
        foreach (DelaunayTriangle tri in triangulation)
        {
            float distToCircumcentre = (float)Math.Sqrt((Math.Pow((tri.circumcircle.centre.x - point.x), 2) + (Math.Pow((tri.circumcircle.centre.y - point.y), 2))));
            if (tri.circumcircle.radius > distToCircumcentre)
            {
                btriangles.Add(tri);
            }
        }
        //find boundary of polygonal hole
        List<DelaunayEdge> polygon = new List<DelaunayEdge>();
        foreach (DelaunayTriangle tri in btriangles)
        {
            foreach (DelaunayEdge edge in tri.edges)
            {
                if (!checkIfSharesEdges(edge, tri, btriangles))
                {
                    polygon.Add(edge);
                }
            }
        }
        //remove bad triangles from triangulation
        foreach (DelaunayTriangle tri in btriangles)
        {
            triangulation.Remove(tri);
        }
        //create new triangles
        foreach (DelaunayEdge edge in polygon)
        {
            triangulation.Add(new DelaunayTriangle(edge.a, edge.b, point));
        }
        return triangulation;

    }

    private void addToVoronoi(DelaunayTriangle triangle, List<DelaunayTriangle> triangles)
    {
        foreach(DelaunayTriangle tri in triangles)
        {
            if (tri != triangle)
            {
                for (int i = 0; i < 3; i++)
                {
                    for(int j = 0; j < 3; j++)
                    {
                        if (checkEdgeEquality(triangle.edges[i], tri.edges[j])){
                            DelaunayVoronoiLine newVEdge = new DelaunayVoronoiLine(triangle.circumcircle.centre, tri.circumcircle.centre, triangle.edges[i].a, triangle.edges[i].b);
                            delVLines.Add(newVEdge);
                            triangle.edges[i].dualLine = newVEdge;
                            tri.edges[j].dualLine = newVEdge;
                        }
                    }
                }
            }
        }
    }

    private void removeDuplicateLines()
    {
        List<DelaunayVoronoiLine> newVLines = new List<DelaunayVoronoiLine>(delVLines); //make copy of the original
        /*
        foreach (DelaunayVoronoiLine line in delVLines)
        {
            foreach (DelaunayVoronoiLine l in delVLines)
            {
                if (line != l)
                {
                    if (checkEdgeEquality(l, line))
                    {
                        newVLines.Remove(line); //remove those that occur twice
                    }
                }
            }
        }
        */
        for(int i = 0; i < newVLines.Count; i++)
        {
            for (int j = i; j < newVLines.Count; j++)
            {
                if (newVLines[i] != newVLines[j])
                {
                    if (checkEdgeEquality(newVLines[i], newVLines[j]))
                    {
                        newVLines.Remove(newVLines[i]);
                    }
                }
            }
        }
        delVLines = new List<DelaunayVoronoiLine>(newVLines); //replace with new list
    }


    //functions for generation of entire diagram at once

    public List<DelaunayTriangle> getWholeTriangulation(List<DelaunayPoint> points, DelaunayTriangle supertriangle) //NEEDS WRITING
    {
        List<DelaunayTriangle> triangulation = new List<DelaunayTriangle>();
        triangulation.Add(supertriangle);
        foreach (DelaunayPoint point in points)
        {
            triangulation = addPoint(point, triangulation);
        }
        return triangulation;
    }

    private void convertWholeToVoronoi(List<DelaunayTriangle> delaunay)
    {
        delVLines.Clear();
        foreach (DelaunayTriangle triangle in delaunay)
        {
            addToVoronoi(triangle, delaunay);
        }
        Debug.Log(delVLines.Count);

        // remove duplicates
        removeDuplicateLines();
        Debug.Log(delVLines.Count);
    }


    //functions for procedural generation

    private (List<DelaunayTriangle>, List<DelaunayTriangle>) findChangedTriangles(List<DelaunayTriangle> delaunay1, List<DelaunayTriangle> delaunay2)
    {
        //Add deleted ones to the deleted list
        //remove existing ones from the newtriangles list (leaving behind only new ones)
        List<DelaunayTriangle> deletedTriangles = new List<DelaunayTriangle>();
        List<DelaunayTriangle> newTriangles= new List<DelaunayTriangle>(delaunay2);

        bool removed;
        foreach(DelaunayTriangle triangle in delaunay1)
        {
            removed = newTriangles.Remove(triangle);
            if (!removed) //if its not in the new list
            {
                deletedTriangles.Add(triangle);
            }
        }
        return (newTriangles, deletedTriangles);
    }

    private void updateVoronoi(List<DelaunayTriangle> triangulation, List<DelaunayTriangle> newTriangles, List<DelaunayTriangle> deletedTriangles)
    {
        //add the new stuff
        foreach(DelaunayTriangle tri in newTriangles)
        {
            addToVoronoi(tri, triangulation);
        }
        //remove the old stuff
        foreach(DelaunayTriangle tri in deletedTriangles)
        {
            if (delVLines.Contains(tri.edges[0].dualLine)){
                delVLines.Remove(tri.edges[0].dualLine);
            }
            else if (delVLines.Contains(tri.edges[1].dualLine))
            {
                delVLines.Remove(tri.edges[1].dualLine);
            }
            else if (delVLines.Contains(tri.edges[2].dualLine))
            {
                delVLines.Remove(tri.edges[2].dualLine);
            }
        }
        //remove duplicate lines
        removeDuplicateLines();
    }
}
