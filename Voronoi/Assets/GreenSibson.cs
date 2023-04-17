using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GSPoint
{
    public float x;
    public float y;
    public float[] polarCoord;
    public GSLine[] lines;
    public GSPoint(float x, float y)
    {
        this.x = x;
        this.y = y;
        this.polarCoord = new float[2];
    }
}

public class GSSeed : GSPoint
{
    public List<GSSeed> connectedSeeds;
    public List<GSLine> vLines;
    public GSSeed(float x, float y) : base(x, y)
    {
        this.connectedSeeds = new List<GSSeed>();
        this.vLines = new List<GSLine>();
    }
}

public class Intersection : GSPoint
{
    public Intersection(float x, float y, GSLine[] lines) : base(x, y)
    {
        this.lines = lines;
    }
}

public class GSLine
{
    public GSPoint[] points;
    public GSSeed[] seeds;
    public List<Intersection> intersections;
    public string label;
    public bool alive = true;
    public float? m;
    public float? c;
    public bool? vertical;
    public float? xVal;
    public GSLine(GSPoint[] points, bool? vertical, float? c, float? xVal, float? m = int.MaxValue)
    {
        this.points = new GSPoint[2];
        Array.Copy(points, this.points, 2);
        this.intersections = new List<Intersection>();
        this.m = m;
        this.c = c;
        this.vertical = vertical;
        this.xVal = xVal;
        if (points[0] != null && points[1] != null)
        {
            genEq();
        }
    }
    private void genEq()
    {
        if (points[0].x == points[1].x)
        {
            vertical = true;
            xVal = points[0].x;
        }
        else
        {
            m = (points[0].y - points[1].y) / (points[0].x - points[1].x);
            c = points[0].y - (m * points[0].x);
        }
    }
}

public class GSvLine : GSLine
{
    public GSvLine(GSPoint[] points, GSSeed[] seeds, bool? vertical = null, float? c = null, float? xVal = null, float? m = int.MaxValue) : base(points, vertical, c, xVal, m)
    {
        this.seeds = new GSSeed[2];
        Array.Copy(seeds, this.seeds, 2);
    }
}

public class Boundary : GSLine
{
    public Boundary(string label, GSPoint[] points, bool? vertical = null, float? c = null, float? xVal = null, float? m = int.MaxValue) : base(points, vertical, c, xVal, m)
    {
        this.label = label;
    }

    public Intersection? hugWall(Intersection intersect, string startingWall, GSDiagram diagram, GSSeed seed, bool first = true)
    {
        if(startingWall == label && !first)
        {
            throw new ArgumentException("Wallhugging completed full loop");
        }

        Intersection? nextIntersect = null;
        if (intersections.Count > 0)
        {
            //for starting walls look along current wall
            if (label == "top" && label == startingWall)
            {
                foreach (Intersection i in intersections)
                {
                    if (i.x > intersect.x && i != intersect && !(seed.connectedSeeds.Contains(i.lines[1].seeds[0]) && seed.connectedSeeds.Contains(i.lines[1].seeds[1])) && intersect.lines[1].alive)
                    {
                        if (nextIntersect == null || nextIntersect.x > i.x)
                        {
                            nextIntersect = i;
                        }
                    }
                }
                if (nextIntersect == null)
                {
                    nextIntersect = diagram.boundaries[1].hugWall(intersect, startingWall, diagram, seed, false);
                }
                return nextIntersect;
            }
            else if (label == "right" && label == startingWall)
            {
                foreach (Intersection i in intersections)
                {
                    if (i.y < intersect.y && i != intersect && !(seed.connectedSeeds.Contains(i.lines[1].seeds[0]) && seed.connectedSeeds.Contains(i.lines[1].seeds[1])) && intersect.lines[1].alive)
                    {
                        if (nextIntersect == null || nextIntersect.y < i.y)
                        {
                            nextIntersect = i;
                        }
                    }
                }
                if (nextIntersect == null)
                {
                    nextIntersect = diagram.boundaries[2].hugWall(intersect, startingWall, diagram, seed, false);
                }
                return nextIntersect;
            }
            else if (label == "bottom" && label == startingWall)
            {
                foreach (Intersection i in intersections)
                {
                    if (i.x < intersect.x && i != intersect && !(seed.connectedSeeds.Contains(i.lines[1].seeds[0]) && seed.connectedSeeds.Contains(i.lines[1].seeds[1])) && intersect.lines[1].alive)
                    {
                        if (nextIntersect == null || nextIntersect.x < i.x)
                        {
                            nextIntersect = i;
                        }
                    }
                }
                if (nextIntersect == null)
                {
                    nextIntersect = diagram.boundaries[3].hugWall(intersect, startingWall, diagram, seed, false);
                }
                return nextIntersect;
            }
            else if (label == "left" && label == startingWall)
            {
                foreach (Intersection i in intersections)
                {
                    if (i.y > intersect.y && i != intersect && !(seed.connectedSeeds.Contains(i.lines[1].seeds[0]) && seed.connectedSeeds.Contains(i.lines[1].seeds[1])) && intersect.lines[1].alive)
                    {
                        if (nextIntersect == null || nextIntersect.y > i.y)
                        {
                            nextIntersect = i;
                        }
                    }
                }
                if (nextIntersect == null)
                {
                    nextIntersect = diagram.boundaries[0].hugWall(intersect, startingWall, diagram, seed, false);
                }
                return nextIntersect;
            }
            else //for all subsequent ones just look for first one
            {
                Intersection firstInt = intersections[0];
                for(int i = 1; i < intersections.Count; i++)
                {
                    Intersection inter = intersections[i];
                    if (label == "top")
                    {
                        if(inter.x < firstInt.x && !(seed.connectedSeeds.Contains(inter.lines[1].seeds[0]) && seed.connectedSeeds.Contains(inter.lines[1].seeds[1])))
                        {
                            firstInt = inter;
                        }
                    }
                    else if (label == "right")
                    {
                        if (inter.y > firstInt.y && !(seed.connectedSeeds.Contains(inter.lines[1].seeds[0]) && seed.connectedSeeds.Contains(inter.lines[1].seeds[1])))
                        {
                            firstInt = inter;
                        }
                    }
                    else if (label == "bottom")
                    {
                        if (inter.x > firstInt.x && !(seed.connectedSeeds.Contains(inter.lines[1].seeds[0]) && seed.connectedSeeds.Contains(inter.lines[1].seeds[1])))
                        {
                            firstInt = inter;
                        }
                    }
                    else
                    {
                        if (inter.y < firstInt.y && !(seed.connectedSeeds.Contains(inter.lines[1].seeds[0]) && seed.connectedSeeds.Contains(inter.lines[1].seeds[1])))
                        {
                            firstInt = inter;
                        }
                    }
                }
                return firstInt;
            }
        }
        //no other intersections on this line
        if (label == "top")
        {
            nextIntersect = diagram.boundaries[1].hugWall(intersect, startingWall, diagram, seed, false);
        }
        else if (label == "right")
        {
            nextIntersect = diagram.boundaries[2].hugWall(intersect, startingWall, diagram, seed, false);
        }
        else if (label == "bottom")
        {
            nextIntersect = diagram.boundaries[3].hugWall(intersect, startingWall, diagram, seed, false);
        }
        else
        {
            nextIntersect = diagram.boundaries[0].hugWall(intersect, startingWall, diagram, seed, false);
        }

        return nextIntersect;
    }
}

public class GSDiagram
{
    float minX;
    float minY;
    float maxX;
    float maxY;
    public Boundary[] boundaries;
    public List<GSSeed> addedSeeds;
    public GSDiagram(float minX, float minY, float maxX, float maxY)
    {
        this.minX = minX;
        this.minY = minY;
        this.maxX = maxX;
        this.maxY = maxY;
        this.boundaries = new Boundary[4];
        genBoundaries(minX, maxX, minY, maxY);
        this.addedSeeds = new List<GSSeed>();
    }
    private void genBoundaries(float minX, float maxX, float minY, float maxY)
    {
        boundaries[0] = new Boundary("top", new GSPoint[] { new GSPoint(minX, maxY), new GSPoint(maxX, maxY) });
        boundaries[1] = new Boundary("right", new GSPoint[] { new GSPoint(maxX, minY), new GSPoint(maxX, maxY) });
        boundaries[2] = new Boundary("bottom", new GSPoint[] { new GSPoint(minX, minY), new GSPoint(maxX, minY) });
        boundaries[3] = new Boundary("left", new GSPoint[] { new GSPoint(minX, minY), new GSPoint(minX, maxY) });
    }

    private float getDist(GSPoint p1, GSPoint p2)
    {
        float distance = (float)Math.Sqrt(Math.Pow(p1.x - p2.x, 2) + Math.Pow(p1.y - p2.y, 2));
        return distance;
    }

    public bool pointsEqual(GSPoint? p1, GSPoint? p2)
    {
        if (p1 == null || p2 == null)
        {
            return false;
        }
        if (Math.Round(p1.x, 5) == Math.Round(p2.x, 5) && Math.Round(p1.y, 5) == Math.Round(p2.y, 5))
        {
            return true;
        }
        return false;
    }

    private GSSeed findNearestNeighbour(GSSeed seed)
    {
        (GSSeed, float) nearest = (null, int.MaxValue);
        foreach (GSSeed s in addedSeeds)
        {
            if (s != seed)
            {
                float dist = getDist(seed, s);
                if (dist < nearest.Item2)
                {
                    nearest.Item2 = dist;
                    nearest.Item1 = s;
                }
            }
        }
        return nearest.Item1;
    }

    private GSvLine getBisector(GSSeed seed, GSSeed refSeed)
    {
        float midX = (seed.x + refSeed.x) / 2;
        float midY = (seed.y + refSeed.y) / 2;
        if (seed.y != refSeed.y)
        {
            float gradient = -(seed.x - refSeed.x) / (seed.y - refSeed.y);
            float c = (-1 * gradient * midX) + midY;
            GSvLine perpBisector = new GSvLine(new GSPoint[2], new GSSeed[] { seed, refSeed }, m: gradient, c: c);
            return perpBisector;
        }
        else
        {
            GSvLine perpBisector = new GSvLine(new GSPoint[2], new GSSeed[] { seed, refSeed }, vertical: true, xVal: midX);
            return perpBisector;
        }
    }

    private Intersection? getIntersection(GSLine bisector, GSLine vLine)
    {
        if (bisector.vertical == true && vLine.vertical == true)
        {
            return null;
        }
        if (bisector.m == vLine.m)
        {
            return null;
        }

        float x;
        float y;
        if (bisector.vertical == true)
        {
            x = (float)bisector.xVal;
            y = (float)((vLine.m * x) + vLine.c);
        }
        else if (vLine.vertical == true)
        {
            x = (float)vLine.xVal;
            y = (float)((bisector.m * x) + bisector.c);
        }
        else
        {
            x = (float)((vLine.c - bisector.c) / (bisector.m - vLine.m));
            y = (float)((bisector.m * x) + bisector.c);
        }

        if ((x > vLine.points[0].x + 0.1 && x > vLine.points[1].x + 0.1) || (x < vLine.points[0].x - 0.1 && x < vLine.points[1].x - 0.1))
        {
            return null;
        }
        if ((y > vLine.points[0].y + 0.1 && y > vLine.points[1].y + 0.1) || (y < vLine.points[0].y - 0.1 && y < vLine.points[1].y - 0.1))
        {
            return null;
        }

        Intersection intersect = new Intersection(x, y, new GSLine[] { vLine, bisector });
        return intersect;
    }

    private (Intersection, Intersection) getNearestIntersects(List<Intersection> intersects, GSPoint centrePoint, Intersection? prevIntersect)
    {
        List<GSPoint> polarCoordIntersects = GreenSibson.getPolarCoords(centrePoint, new List<GSPoint>(intersects));

        List<Intersection> rightPoints = new();
        List<Intersection> leftPoints = new();
        bool valid;
        foreach (Intersection i in polarCoordIntersects)
        {
            valid = true;
            if (prevIntersect != null)
            {
                if (i.lines[0] == prevIntersect.lines[0])
                {
                    valid = false;
                }
            }
            if (valid)
            {
                if (!(i.x > maxX || i.x < minX || i.y > maxY || i.y < minY))
                {
                    if (i.polarCoord[1] < Math.PI)
                    {
                        rightPoints.Add(i);
                    }
                    else
                    {
                        leftPoints.Add(i);
                    }
                }
            }
        }

        Intersection? nearestLeft = null;
        foreach (Intersection i in leftPoints)
        {
            if (nearestLeft == null || nearestLeft.polarCoord[0] > i.polarCoord[0])
            {
                nearestLeft = i;
            }
        }
        Intersection? nearestRight = null;
        foreach(Intersection i in rightPoints)
        {
            if (nearestRight == null || nearestRight.polarCoord[0] > i.polarCoord[0])
            {
                nearestRight = i;
            }
        }

        return (nearestRight, nearestLeft);
    }

    private Intersection? selectIntersect(Intersection? nearestLeft, Intersection? nearestRight, GSSeed seed, GSSeed refSeed)
    {
        if (refSeed.y == seed.y)
        {
            if (refSeed.x < seed.x)
            {
                return nearestRight;
            }
            else
            {
                return nearestLeft;
            }
        }
        if (refSeed.y < seed.y)
        {
            return nearestLeft;
        }
        else
        {
            return nearestRight;
        }
    }

    public void addSeed(GSSeed seed)
    {
        Debug.Log(seed.x);
        Debug.Log(seed.y);
        addedSeeds.Add(seed);
        foreach (Boundary b in boundaries)
        {
            seed.vLines.Add(b);
        }

        List<Intersection> intersectionList = new();

        GSSeed nearestSeed = findNearestNeighbour(seed);
        if (nearestSeed == null)
        {
            return;
        }

        GSLine bisector = getBisector(seed, nearestSeed);

        List<Intersection> intersects = new();
        foreach (GSLine vLine in nearestSeed.vLines)
        {
            Intersection intersectPoint = getIntersection(bisector, vLine);
            if (intersectPoint != null)
            {
                intersects.Add(intersectPoint);
            }
        }
        GSPoint centrePoint = new((seed.x + nearestSeed.x) / 2, (seed.y + nearestSeed.y) / 2);
        Intersection nearestRight;
        Intersection nearestLeft;
        (nearestRight, nearestLeft) = getNearestIntersects(intersects, centrePoint, null);


        //add first line
        GSvLine line = new(new GSPoint[] { nearestLeft, nearestRight }, new GSSeed[] { seed, nearestSeed });
        nearestLeft = getIntersection(line, nearestLeft.lines[0]);
        nearestRight = getIntersection(line, nearestRight.lines[0]);
        //add to intersections list
        intersectionList.Add(nearestRight);
        intersectionList.Add(nearestLeft);
        //update with new line that has point locations
        nearestLeft.lines[1] = line;
        nearestRight.lines[1] = line;

        seed.vLines.Add(line);
        seed.connectedSeeds.Add(nearestSeed);
        nearestSeed.vLines.Add(line);
        nearestSeed.connectedSeeds.Add(seed);

        //add to boundary intersectoin lists if needed
        if (nearestLeft.lines[0] is Boundary)
        {
            nearestLeft.lines[0].intersections.Add(nearestLeft);
        }
        if (nearestRight.lines[0] is Boundary)
        {
            nearestRight.lines[0].intersections.Add(nearestRight);
        }

        //select target then start the loop
        Intersection prevIntersect;
        Intersection targetIntersect;
        if (selectIntersect(nearestLeft, nearestRight, seed, nearestSeed) == nearestLeft)
        {
            prevIntersect = nearestLeft;
            targetIntersect = nearestRight;
        }
        else
        {
            prevIntersect = nearestRight;
            targetIntersect = nearestLeft;
        }


        //loop time
        bool wallHugging = false;
        GSSeed refSeed = new(0,0);
        while (!pointsEqual(prevIntersect, targetIntersect))
        {
            if (prevIntersect.lines[0] is Boundary)
            {
                wallHugging = true;
                if (!prevIntersect.lines[0].intersections.Contains(prevIntersect))
                {
                    prevIntersect.lines[0].intersections.Add(prevIntersect);
                }
            }
            else
            {
                if (seed.connectedSeeds.Contains(prevIntersect.lines[0].seeds[0]))
                {
                    refSeed = prevIntersect.lines[0].seeds[1];
                }
                else
                {
                    refSeed = prevIntersect.lines[0].seeds[0];
                }
                seed.connectedSeeds.Add(refSeed);
                refSeed.connectedSeeds.Add(seed);
            }

            if (!wallHugging)
            {
                bisector = getBisector(seed, refSeed);
                intersects = new();
                foreach (GSLine vLine in refSeed.vLines)
                {
                    if (vLine is Boundary || vLine.alive)
                    {
                        Intersection intersectPoint = getIntersection(bisector, vLine);
                        if (intersectPoint != null)
                        {
                            intersects.Add(intersectPoint);
                        }
                    }
                }
                (nearestRight, nearestLeft) = getNearestIntersects(intersects, prevIntersect, prevIntersect);
                Intersection intersect = selectIntersect(nearestLeft, nearestRight, seed, refSeed);

                //make new line
                line = new(new GSPoint[] { prevIntersect, intersect}, new GSSeed[] { seed, refSeed });
                Intersection newIntersect = getIntersection(line, intersect.lines[0]);
                intersectionList.Add(newIntersect);
                seed.vLines.Add(line);
                refSeed.vLines.Add(line);

                prevIntersect = newIntersect;
            }
            else //wallhugging
            {
                //boundary wall indexes = [0:top, 1:right, 2:bottom, 3:left]
                Intersection nextIntersect;
                if (prevIntersect.lines[0].label == "top")
                {
                    nextIntersect = boundaries[0].hugWall(prevIntersect, "top", this, seed);
                }
                else if (prevIntersect.lines[0].label == "right")
                {
                    nextIntersect = boundaries[1].hugWall(prevIntersect, "right", this, seed);
                }
                else if (prevIntersect.lines[0].label == "bottom")
                {
                    nextIntersect = boundaries[2].hugWall(prevIntersect, "bottom", this, seed);
                }
                else
                {
                    nextIntersect = boundaries[3].hugWall(prevIntersect, "left", this, seed);
                }
                wallHugging = false;
                if (pointsEqual(nextIntersect, targetIntersect))
                {
                    prevIntersect = targetIntersect;
                }
                else
                {
                    //when hitting a line one of its seeds will already be visited
                    if (seed.connectedSeeds.Contains(nextIntersect.lines[1].seeds[0]))
                    {
                        refSeed = nextIntersect.lines[1].seeds[1];
                    }
                    else
                    {
                        refSeed = nextIntersect.lines[1].seeds[0];
                    }
                    if (seed.connectedSeeds.Contains(refSeed) && targetIntersect.lines[0] is Boundary)
                    {
                        prevIntersect = targetIntersect;
                    }
                    else
                    {
                        bool selectedStartingPos = false;
                        Intersection? boundIntersect = null;
                        while (!selectedStartingPos)
                        {
                            bisector = getBisector(seed, refSeed);
                            intersects = new();
                            foreach(GSLine vLine in refSeed.vLines)
                            {
                                if (vLine is Boundary || vLine.alive)
                                {
                                    Intersection intersectPoint = getIntersection(bisector, vLine);
                                    if (intersectPoint != null)
                                    {
                                        intersects.Add(intersectPoint);
                                    }
                                }
                            }
                            List<Intersection> boundInters = new();
                            foreach (Intersection inter in intersects)
                            {
                                if (inter.lines[0] is Boundary)
                                {
                                    boundInters.Add(inter);
                                }
                            }
                            //select closest
                            float nearestDist = int.MaxValue;
                            float dist;
                            foreach (Intersection inter in boundInters)
                            {
                                dist = getDist(seed, inter);
                                if (dist < nearestDist)
                                {
                                    nearestDist = dist;
                                    boundIntersect = inter;
                                }
                            }

                            //check if moved into new sector
                            bool valid = true;
                            foreach (GSSeed se in refSeed.connectedSeeds)
                            {
                                if (getDist(se, boundIntersect) < getDist(refSeed, boundIntersect))
                                {
                                    refSeed = se;
                                    valid = false;
                                    break;
                                }
                            }
                            if (valid)
                            {
                                selectedStartingPos = true;
                                seed.connectedSeeds.Add(refSeed);
                                refSeed.connectedSeeds.Add(seed);
                            }
                        }

                        //get other point
                        string boundaryName = boundIntersect.lines[0].label;
                        if (boundaryName == "right" || (boundaryName == "top" && (bisector.vertical == true || bisector.m > 0)) || (boundaryName == "bottom" && (bisector.vertical != true && bisector.m < 0)))
                        {
                            nearestRight = boundIntersect;
                            if (intersects[0] != boundIntersect)
                            {
                                nearestLeft = intersects[0];
                            }
                            else
                            {
                                nearestLeft = intersects[1];
                            }
                            foreach (Intersection inter in intersects)
                            {
                                if (inter != nearestRight && ((inter.x > nearestLeft.x && inter.x < nearestRight.x)||(inter.x == nearestRight.x && inter.y > nearestLeft.y)))
                                {
                                    nearestLeft = inter;
                                }
                            }
                        }
                        else if (boundaryName == "left" || (boundaryName == "top" && (bisector.vertical != true || bisector.m < 0)) || (boundaryName == "bottom" && (bisector.vertical == true || bisector.m > 0)))
                        {
                            nearestLeft = boundIntersect;
                            if (intersects[0] != boundIntersect)
                            {
                                nearestRight = intersects[0];
                            }
                            else
                            {
                                nearestRight= intersects[1];
                            }
                            foreach (Intersection inter in intersects)
                            {
                                if (inter != nearestLeft && ((inter.x < nearestRight.x && inter.x > nearestLeft.x)||(inter.x == nearestLeft.x && inter.y < nearestRight.y)))
                                {
                                    nearestRight = inter;
                                }
                            }
                        }

                        //assemble line
                        Intersection intersect = selectIntersect(nearestLeft, nearestRight, seed, refSeed);
                        line = new(new GSPoint[] { nearestRight, nearestLeft}, new GSSeed[] { seed, refSeed });
                        intersect = getIntersection(line, intersect.lines[0]);
                        seed.vLines.Add(line);
                        seed.connectedSeeds.Add(refSeed);
                        refSeed.vLines.Add(line);
                        refSeed.connectedSeeds.Add(seed);
                        if (nearestLeft.lines[0] is Boundary)
                        {
                            nearestLeft.lines[0].intersections.Add(nearestLeft);
                        }
                        if (nearestRight.lines[0] is Boundary)
                        {
                            nearestRight.lines[0].intersections.Add(nearestRight);
                        }

                        intersectionList.Add(intersect);
                        prevIntersect = intersect;
                    }
                }
            }
        }

        //cropping/housekeeping
        if (addedSeeds.Count > 2)
        {
            foreach (GSSeed s in seed.connectedSeeds)
            {
                foreach (GSLine l in s.vLines)
                {
                    if (l.points[0] == null)
                    {
                        l.alive = false;
                        foreach (Boundary b in boundaries)
                        {
                            foreach (Intersection inter in b.intersections)
                            {
                                if (inter.lines.Contains(l))
                                {
                                    b.intersections.Remove(inter);
                                }
                            }
                        }
                    }
                    if (l is not Boundary && !l.seeds.Contains(seed))
                    {
                        if (l.alive)
                        {
                            float point0DistRoot = (float)Math.Round(getDist(l.points[0], s), 5);
                            float point0DistSeed = (float)Math.Round(getDist(l.points[0], seed), 5);
                            float point1DistRoot = (float)Math.Round(getDist(l.points[1], s), 5);
                            float point1DistSeed = (float)Math.Round(getDist(l.points[1], seed), 5);

                            if (point0DistRoot >= point0DistSeed && point1DistRoot >= point1DistSeed)
                            {
                                l.alive = false;
                                if (l.points[0].lines[0] is Boundary)
                                {
                                    List<Intersection> sublist = l.points[0].lines[0].intersections.FindAll(item => pointsEqual(item, l.points[0]));
                                    //if (l.points[0].lines[0].intersections.Contains(l.points[0]))
                                    if (sublist.Count > 0)
                                    {
                                        l.points[0].lines[0].intersections.RemoveAll(item => pointsEqual(l.points[0], item));
                                    }
                                }
                                if (l.points[1].lines[0] is Boundary)
                                {
                                    List<Intersection> sublist = l.points[1].lines[0].intersections.FindAll(item => pointsEqual(item, l.points[1]));
                                    //if (l.points[0].lines[0].intersections.Contains(l.points[1]))
                                    if ( sublist.Count > 0)
                                    {
                                        l.points[1].lines[0].intersections.RemoveAll(item => pointsEqual(l.points[1], item));
                                    }
                                }
                            }
                            else if (point0DistRoot >= point0DistSeed)
                            {
                                foreach (Intersection intersect in intersectionList)
                                {
                                    if (intersect.lines[0].m == l.m && intersect.lines[0].c == l.c && intersect.lines[0].vertical == l.vertical && intersect.lines[0].xVal == l.xVal)
                                    {
                                        if (l.points[0] is Intersection && l.points[0].lines[0] is Boundary)
                                        {
                                            List<Intersection> sublist = l.points[0].lines[0].intersections.FindAll(item => pointsEqual(item, l.points[0]));
                                            //if (l.points[0].lines[0].intersections.Contains(l.points[0]))
                                            if (sublist.Count > 0)
                                            {
                                                l.points[0].lines[0].intersections.RemoveAll(item => pointsEqual(l.points[0], item));
                                            }
                                        }
                                        l.points[0] = intersect;
                                    }
                                }
                            }
                            else if (point1DistRoot >= point1DistSeed)
                            {
                                foreach (Intersection intersect in intersectionList)
                                {
                                    if (intersect.lines[0].m == l.m && intersect.lines[0].c == l.c && intersect.lines[0].vertical == l.vertical && intersect.lines[0].xVal == l.xVal)
                                    {
                                        if (l.points[1] is Intersection && l.points[1].lines[0] is Boundary)
                                        {
                                            List<Intersection> sublist = l.points[1].lines[0].intersections.FindAll(item => pointsEqual(item, l.points[1]));
                                            //if (l.points[1].lines[0].intersections.Contains(l.points[1]))
                                            if (sublist.Count > 0)
                                            {
                                                l.points[1].lines[0].intersections.RemoveAll(item => pointsEqual(l.points[1], item));
                                            }
                                        }
                                        l.points[1] = intersect;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}



public class GreenSibson : MonoBehaviour
{
    public static List<GSPoint> getPolarCoords(GSPoint centre, List<GSPoint> points)
    {
        List<GSPoint> polarPoints = new List<GSPoint>();
        foreach (GSPoint p in points)
        {
            float r = (float)Math.Sqrt(Math.Pow(centre.x - p.x, 2) + Math.Pow(centre.y - p.y, 2));
            float relx = p.x - centre.x;
            float rely = p.y - centre.y;

            float angle = 0;
            if (relx > 0 && rely > 0)
            {
                angle = (float)((Math.PI / 2) - Math.Asin((double)rely / r));
            }
            else if (relx < 0 && rely < 0)
            {
                angle = (float)((Math.PI * 3 / 2) - Math.Asin((double)Math.Abs(rely) / r));
            }
            else if (relx < 0 && rely > 0)
            {
                angle = (float)((Math.PI * 3 / 2) - Math.Asin((double)rely / r));
            }
            else if (relx > 0 && rely < 0)
            {
                angle = (float)((Math.PI / 2) - Math.Asin((double)Math.Abs(rely) / r));
            }
            else if (relx == 0)
            {
                if (rely > 0)
                {
                    angle = 0;
                }
                else
                {
                    angle = (float)Math.PI;
                }
            }
            else if (rely == 0)
            {
                if (relx > 0)
                {
                    angle = (float)Math.PI / 2;
                }
                else
                {
                    angle = (float)Math.PI * 3 / 2;
                }
            }
            p.polarCoord[0] = r;
            p.polarCoord[1] = angle;
        }
        polarPoints = mergeSortPolarCoords(points);
        return polarPoints;
    }

    public static List<GSPoint> mergeSortPolarCoords(List<GSPoint> points)
    {
        if (points.Count <= 1)
        {
            return points;
        }
        List<GSPoint> left = new();
        List<GSPoint> right = new();
        for (int i = 0; i < points.Count; i++)
        {
            if (i < points.Count / 2)
            {
                left.Add(points[i]);
            }
            else
            {
                right.Add(points[i]);
            }
        }
        left = mergeSortPolarCoords(left);
        right = mergeSortPolarCoords(right);
        return merge(left, right);
    }

    public static List<GSPoint> merge(List<GSPoint> left, List<GSPoint> right)
    {
        List<GSPoint> output = new();
        while (left.Count > 0 && right.Count > 0)
        {
            if (left[0].polarCoord[1] <= right[0].polarCoord[1])
            {
                output.Add(left[0]);
                left.RemoveAt(0);
            }
            else
            {
                output.Add(right[0]);
                right.RemoveAt(0);
            }
        }
        output.AddRange(left);
        output.AddRange(right);
        return output;
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
