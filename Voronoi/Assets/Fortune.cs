#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

class QueueItem
{
    private FortunePoint item;
    private float weight;
    private int nextIndex = -1;
    private int previousIndex = -1;
    private bool alive = true;

    public QueueItem(FortunePoint item, float weight)
    {
        this.item = item;
        this.weight = weight;
    }

    public FortunePoint getItem()
    {
        return item;
    }
    public float getWeight()
    {
        return weight;
    }
    public int getNextIndex()
    {
        return nextIndex;
    }
    public int getPreviousIndex()
    {
        return previousIndex;
    }
    public bool isAlive()
    {
        return alive;
    }
    public void setNextIndex(int newIndex)
    {
        nextIndex = newIndex;
    }
    public void setPreviousIndex(int newIndex)
    {
        previousIndex = newIndex;
    }
    public void die()
    {
        alive = false;
    }
}

class PriorityQueue
{
    public List<QueueItem> queue = new List<QueueItem>();
    private int front = 0;

    public int findInsertIndex()
    {
        for (int i = 0; i < queue.Count; i++)
        {
            if (!queue[i].isAlive())
            {
                return i;
            }
        }
        return -1;
    }

    public void enqueue(FortunePoint item, float weighting)
    {
        QueueItem newItem = new QueueItem(item, weighting);

        int insertIndex = findInsertIndex();

        if (isEmpty())
        {
            if (insertIndex == -1)
            {
                queue.Add(newItem);
            }
            else
            {
                queue[insertIndex] = newItem;
            }
            return;
        }


        bool found = false;
        int nextIndex = front;
        int itemBelow = -1;
        while (!found && nextIndex != -1)
        {
            if (queue[nextIndex].getWeight() > weighting) //prioritising low weighting
            {
                found = true;
                itemBelow = nextIndex; //the item with a lower weighting (insert before this one)
            }
            else
            {
                if (queue[nextIndex].getNextIndex() == -1)
                {
                    newItem.setPreviousIndex(nextIndex);
                    if (insertIndex != -1)
                    {
                        queue[nextIndex].setNextIndex(insertIndex);
                        queue[insertIndex] = newItem;
                    }
                    else
                    {
                        queue[nextIndex].setNextIndex(queue.Count);
                        queue.Add(newItem);
                    }
                    return;
                }
                nextIndex = queue[nextIndex].getNextIndex();
            }
        }

        int itemAbove = queue[itemBelow].getPreviousIndex();
        newItem.setNextIndex(itemBelow);
        if (insertIndex == -1)
        {
            queue[itemBelow].setPreviousIndex(queue.Count);
        }
        else
        {
            queue[itemBelow].setPreviousIndex(insertIndex);
        }
        if (itemAbove != -1)
        {
            newItem.setPreviousIndex(itemAbove);
            if (insertIndex == -1)
            {
                queue[itemAbove].setNextIndex(queue.Count);
            }
            else
            {
                queue[itemAbove].setNextIndex(insertIndex);
            }
        }
        else
        {
            if (insertIndex != -1)
            {
                front = insertIndex;
            }
            else
            {
                front = queue.Count;
            }
        }
        if (insertIndex != -1)
        {
            queue[insertIndex] = newItem;
        }
        else
        {
            queue.Add(newItem);
        }
        return;
    }
    public FortunePoint dequeue()
    {
        //bounce along past removed items
        int counter = 0;
        while (!queue[front].isAlive() && counter < queue.Count)
        if (!queue[front].isAlive())
        {
            counter++;
            front = queue[front].getNextIndex();
        }

        FortunePoint item = queue[front].getItem();
        int nextItem = queue[front].getNextIndex();

        queue[front].die();
        if (nextItem != -1)
        {
            queue[nextItem].setPreviousIndex(-1);
            front = nextItem;
        }
        else
        {
            front = 0;
        }
        return item;
    }

    public bool isEmpty()
    {
        foreach (QueueItem item in queue)
        {
            if (item.isAlive())
            {
                return false;
            }
        }
        return true;
    }
}



public class FortunePoint
{
    public float x;
    public float y;
    public FortuneArc[]? arcs;
    public FortunePoint site;
    public FortunePoint? circumcentre;
    public string? side;

    public FortunePoint(float x, float y)
    {
        this.x = x;
        this.y = y;
    }
}

public class FortuneArc
{
    public FortunePoint point;
    public float minX;
    public float maxX;
    public float a;
    public float b;
    public float c;
    public FortuneEdge? leftEdge;
    public FortuneEdge? rightEdge;

    public FortuneArc(FortunePoint Point, float? minX = null, float? maxX = null)
    {
        Debug.Log("asdf");
        this.point = Point;
        if(minX is not null)
        {
            this.minX = (float)minX;
        }
        if(maxX is not null)
        {
            this.maxX = (float)maxX;
        }
    }

    public void calcVals(float sweeplineY)
    {
        float k = (this.point.y + sweeplineY) / 2;
        float p = (this.point.y - sweeplineY) / 2;

        this.a = 1 / (4 * p);
        this.b = (-1 * this.point.x) / (2 * p);
        this.c = ((float)(Math.Pow(this.point.x, 2)) / (4 * p)) + k;
    }

    public float calcY(float x)
    {
        float y = ((this.a * ((float)Math.Pow(x, 2))) + (this.b * x) + this.c);
        return y;
    }
}

public class FortuneEdge
{
    public FortunePoint start;
    public FortunePoint? end;
    public (float, float) directionVector;
    public string? propDirection;
    public float? m;
    public float? c;
    public bool vertical;
    public float? xVal;

    public FortuneEdge(FortunePoint start, (float, float) directionVector, string? propDirection = null, float? m = null, bool vertical = false)
    {
        this.start = start;
        this.directionVector = directionVector;
        this.propDirection = propDirection;

        if (m == null && !vertical)
        {
            this.m = (directionVector.Item2 / directionVector.Item1);
        }
        else
        {
            this.m = m;
        }

        this.vertical = vertical;
        if (!vertical)
        {
            this.c = start.y - (this.m * start.x);
            this.xVal = null;
        }
        else
        {
            this.xVal = start.x;
        }
        this.end = null;
    }

    public float calcX(float y)
    {
        float dy = y - this.start.y;
        float dx = dy * this.directionVector.Item1;
        float x = this.start.x + dx;
        return x;
    }

    public float? calcY(float x)
    {
        if (this.vertical)
        {
            return null;
        }
        float dx = x - this.start.x;
        float dy = dx * this.directionVector.Item2;
        float y = this.start.y + dy;
        return y;
    }
}

public class SiteEvent : FortunePoint
{
    public string eType = "site";

    public SiteEvent(float x, float y, FortunePoint site) : base(x, y)
    {
        this.site = site;
    }
}

public class CircleEvent : FortunePoint
{
    public string side;
    public string eType = "circle";

    public CircleEvent(float x, float y, FortuneArc[] arcs, string side) : base(x, y)
    {
        this.arcs = arcs;
        this.side = side;
        this.circumcentre = calcCircumcentre(arcs[0], arcs[1], arcs[2]);
    }

    public static FortunePoint calcCircumcentre(FortuneArc arc1, FortuneArc arc2, FortuneArc arc3)
    {
        FortunePoint p1 = arc1.point;
        FortunePoint p2 = arc2.point;
        FortunePoint p3 = arc3.point;

        float x1 = p1.x;
        float y1 = p1.y;
        float x2 = p2.x;
        float y2 = p2.y;
        float x3 = p3.x;
        float y3 = p3.y;
        float xab = (x1 + x2) / 2;
        float yab = (y1 + y2) / 2;
        float xac = (x1 + x3) / 2;
        float yac = (y1 + y3) / 2;

        //if 2 points have same y then x is directly in centre
        float x;
        float y;
        if (y1 == y2)
        {
            x = (x1 + x2) / 2;
            y = ((-((x1 - x3) / (y1 - y3))) * (x - xac)) + yac;
           
        }
        else if (y1 == y3)
        {
            x = (x1 + x3) / 2;
            y = ((-((x1 - x2) / (y1 - y2))) * (x - xab)) + yab;
        }
        else if (y2 == y3)
        {
            x = (x2 + x3) / 2;
            y = ((-((x1 - x2) / (y1 - y2))) * (x - xab)) + yab;
        }
        else
        {
            x = ((((-x1 * xab) + (xab * x2)) / (y1 - y2)) + (((x1 * xac) - (xac * x3)) / (y1 - y3)) + yac - yab) / (((-x1 + x2) / (y1 - y2)) + ((x1 - x3) / (y1 - y3)));
            y = ((-((x1 - x3) / (y1 - y3))) * (x - xac)) + yac;
        }
        FortunePoint circumcentre = new FortunePoint(x, y);
        return circumcentre;
    }

    public FortuneArc getArcAtIdx(int index)
    {
        return arcs[index];
    }
}

public class Sweepline
{
    public float y;
    public List<FortuneArc> arcs;
    
    public Sweepline(float startingY)
    {
        this.y = startingY;
        this.arcs = new List<FortuneArc>();
    }

    public void insert(int pos, FortuneArc item)
    {
        arcs.Insert(pos, item);
    }
}


public class Fortune : MonoBehaviour
{
    PriorityQueue queue;
    Sweepline sweep;
    List<FortunePoint> vertices;
    List<FortuneEdge> edges;
    


    // Start is called before the first frame update
    void Start()
    
    {   
        PriorityQueue queue = new PriorityQueue();
        queue.enqueue(new FortunePoint(1, 2), 9);
        queue.enqueue(new FortunePoint(2, 2), 5);
        queue.enqueue(new FortunePoint(3, 2), 22);
        queue.enqueue(new FortunePoint(4, 2), 164);
        queue.enqueue(new FortunePoint(5, 2), 2);
        queue.enqueue(new FortunePoint(6, 2), 3);
        queue.enqueue(new FortunePoint(7, 2), 19);
        queue.enqueue(new FortunePoint(8, 2), 1);

        Debug.Log("queue emptying, expect order 8,5,6,2,1,7,3,4");
        Debug.Log(queue.isEmpty());
        while (!queue.isEmpty())
        {
            Debug.Log(queue.dequeue().x);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /* Not sure this is ever actually used
    private float intersectArcs(FortuneArc arc1, FortuneArc arc2) //returns x coord
    {
        float a = (arc1.a - arc2.a);
        float b = (arc1.b - arc2.b);
        float c = (arc1.c - arc2.c);
        float discriminant = (b * b) - (4 * a * c);
        if (discriminant < 0)
        {
            return
        }
    }
    */

    private float arcEdgeIntersection(FortuneArc arc, FortuneEdge edge)
    {
        if (edge.vertical)
        {
            #pragma warning disable 8629 //stop "may be null" as it is incorrect here
            return (float)edge.xVal;
        }

        float a = arc.a;
        float b = arc.b;
        float c = arc.c;

        float discriminant = (b * b) - (4 * a * c);
        if(discriminant < 0)
        {
            return -1234.56789f; //stupidly specific number as an error marker
        }

        float x1 = (float)(-b + Math.Sqrt(discriminant)) / (2 * a);
        float x2 = (float)(-b - Math.Sqrt(discriminant)) / (2 * a);

        if (edge.directionVector.Item1 < 0)
        {
            if(x1 > x2)
            {
                return x1;
            }
            else
            {
                return x2;
            }
        }
        else
        {
            if(x1 > x2)
            {
                return x2;
            }
            else
            {
                return x1;
            }
        }
    }

    private FortunePoint intersectEdges(FortuneEdge edge1, FortuneEdge edge2)
    {
        float x;
        float y;
        #pragma warning disable 8629 //stop "may be null" as it is incorrect here
        if (edge1.vertical)
        {
            x = (float)edge1.xVal;
            y = (float)edge2.calcY(x);
        }
        else if (edge2.vertical)
        {
            x = (float)edge2.xVal;
            y = (float)edge1.calcY(x);
        }
        else
        {
            x = (float)((edge1.c - edge2.c) / (edge2.m + edge1.m));
            y = (float)edge1.calcY(x);
        }
        FortunePoint point = new FortunePoint(x, y);
        return point;
    }

    private void splitArc(Sweepline sweep, PriorityQueue queue, int origArcIdx, FortuneArc newArc)
    {
        FortuneArc origArc = sweep.arcs[origArcIdx];
        //manually clone arc twice to get new left and right arcs
        FortuneArc leftArc = new FortuneArc(origArc.point, origArc.minX, origArc.maxX);
        leftArc.calcVals(sweep.y);
        leftArc.leftEdge = origArc.leftEdge;
        leftArc.rightEdge = origArc.rightEdge;
        FortuneArc rightArc = new FortuneArc(origArc.point, origArc.minX, origArc.maxX);
        rightArc.calcVals(sweep.y);
        rightArc.leftEdge = origArc.leftEdge;
        rightArc.rightEdge = origArc.rightEdge;
        //update circle events
        FortunePoint item;
        foreach (QueueItem i in queue.queue)
        {
            item = i.getItem();
            if(item is CircleEvent)
            {
                if (item.arcs[0] == origArc)
                {
                    item.arcs[0] = rightArc;
                }
                else if(item.arcs[1] == origArc)
                {
                    i.die();
                }
                else if (item.arcs[2] == origArc)
                {
                    item.arcs[2] = leftArc;
                }
            }
        }
        //remake sweepline (replace original arc with (leftArc, newArc, rightArc)
        sweep.arcs[origArcIdx] = newArc;
        sweep.arcs.Insert(origArcIdx, leftArc);
        sweep.arcs.Insert(origArcIdx + 2, rightArc);
        //add new edges
        float x = newArc.point.x;
        FortunePoint start = new FortunePoint(x, origArc.calcY(x));

        (float, float) direction = ((-1 * (origArc.point.y - newArc.point.y)), (origArc.point.x - newArc.point.x));
        bool isVertical = false;
        if (direction.Item1 == 0)
        {
            isVertical = true;
        }
        FortuneEdge edge1 = new FortuneEdge(start, direction, "left", null, isVertical);
        sweep.arcs[origArcIdx].rightEdge = edge1;
        sweep.arcs[origArcIdx + 1].leftEdge = edge1;

        direction = ((-1 * (newArc.point.y - origArc.point.y), newArc.point.x - origArc.point.x));
        isVertical = false;
        if (direction.Item1 == 0)
        {
            isVertical = true;
        }
        FortuneEdge edge2 = new FortuneEdge(start, direction, "right", null, isVertical);
        sweep.arcs[origArcIdx + 1].rightEdge = edge2;
        sweep.arcs[origArcIdx + 2].leftEdge = edge2;
    }

    private void checkNewCircleEvents(Sweepline sweep, int index, PriorityQueue queue, bool subEvent = false)
    {
        //to the left
        if (index - 1 >= 1)
        {
            FortuneArc[] arcs = sweep.arcs.GetRange(index - 2, 3).ToArray();
            //doesn't share 2 points
            if ((arcs[0].point.x != arcs[1].point.x | arcs[0].point.y != arcs[1].point.y) && (arcs[0].point.x != arcs[2].point.x | arcs[0].point.y != arcs[2].point.y) && (arcs[1].point.x != arcs[2].point.x | arcs[1].point.y != arcs[2].point.y))
            {
                FortunePoint circumcentre = CircleEvent.calcCircumcentre(arcs[0], arcs[1], arcs[2]);
                if(!(circumcentre.x > arcs[2].point.x))
                {
                    float radius = (float)Math.Sqrt(Math.Pow((arcs[0].point.y - circumcentre.y), 2) + Math.Pow((arcs[0].point.x - circumcentre.x), 2));
                    if(!(circumcentre.y + radius <= sweep.y))
                    {
                        CircleEvent newEvent = new CircleEvent(circumcentre.x, circumcentre.y + radius, arcs, "left");
                        queue.enqueue(newEvent, newEvent.y);
                    }
                }
            }
        }
        //to the right
        if (index <= sweep.arcs.Count - 3)
        {
            FortuneArc[] arcs = sweep.arcs.GetRange(index, 3).ToArray();
            if ((arcs[0].point.x != arcs[1].point.x | arcs[0].point.y != arcs[1].point.y) && (arcs[0].point.x != arcs[2].point.x | arcs[0].point.y != arcs[2].point.y) && (arcs[1].point.x != arcs[2].point.x | arcs[1].point.y != arcs[2].point.y))//doesn't share 2 points
            {
                FortunePoint circumcentre = CircleEvent.calcCircumcentre(arcs[0], arcs[1], arcs[2]);
                if(!(circumcentre.x < arcs[0].point.x))
                {
                    float radius = (float)Math.Sqrt(Math.Pow((arcs[0].point.y - circumcentre.y), 2) + Math.Pow((arcs[0].point.x - circumcentre.x), 2));
                    if(!(circumcentre.y + radius <= sweep.y))
                    {
                        CircleEvent newEvent = new CircleEvent(circumcentre.x, circumcentre.y + radius, arcs, "right");
                        queue.enqueue(newEvent, newEvent.y);
                    }
                }
            }
        }
    }

    private void firstPoint(int minX, int minY, int maxX)
    {
        FortuneEdge leftMostEdge = new FortuneEdge(new FortunePoint(minX, minY), (0, 1), null, null, true);
        FortuneEdge rightMostEdge = new FortuneEdge(new FortunePoint(maxX, minY), (0, 1), null, null, true);
        FortunePoint firstPoint = queue.dequeue();
        sweep = new Sweepline(firstPoint.y);
        FortuneArc firstArc = new FortuneArc(firstPoint);
        firstArc.leftEdge = leftMostEdge;
        firstArc.rightEdge = rightMostEdge;
        sweep.arcs.Add(firstArc);
        vertices = new List<FortunePoint>();
        edges = new List<FortuneEdge>();
    }

    private void nextEvent()
    {
        FortunePoint e = queue.dequeue();
        sweep.y = e.y + 0.0000001f;

        if(e is SiteEvent)
        {
            FortunePoint site = e.site;
            FortuneArc arc = new FortuneArc(site);
            arc.calcVals(sweep.y);
            //add arc
            (int, float) highest = (-1, -50000);
            FortuneArc otherArc;
            float yIntersection;
            for (int i = 0; i < sweep.arcs.Count; i++){
                otherArc = sweep.arcs[i];
                otherArc.calcVals(sweep.y);
                yIntersection = otherArc.calcY(e.site.x);
                if(yIntersection > highest.Item2)
                {
                    highest = (i, yIntersection);
                }
            }
            splitArc(sweep, queue, highest.Item1, arc);

            //add circle events
            checkNewCircleEvents(sweep, highest.Item1 + 1, queue);
        }

        if(e is CircleEvent)
        {
            //mark down circumcentre and edge
            vertices.Add(e.circumcentre);
            e.arcs[1].leftEdge.end = e.circumcentre;
            e.arcs[1].rightEdge.end = e.circumcentre;
            edges.Add(e.arcs[1].leftEdge);
            edges.Add(e.arcs[1].rightEdge);
            //remove arc
            FortuneArc arc1 = e.arcs[1];
            int idx = sweep.arcs.IndexOf(arc1);
            sweep.arcs.RemoveAt(idx);
            //remove other circle events using that arc
            FortunePoint ev;
            foreach(QueueItem item in queue.queue)
            {
                ev = item.getItem();
                if(ev is CircleEvent)
                {
                    if(ev.arcs.Contains(arc1)){
                        item.die();
                    }
                }
            }
            //assign new edge to left and right arc
            (float, float) direction = (-1 * (e.arcs[0].point.y - e.arcs[2].point.y), e.arcs[0].point.x - e.arcs[2].point.x);
            FortuneEdge newEdge = new FortuneEdge(e.circumcentre, direction, e.side);
            sweep.arcs[idx - 1].rightEdge = newEdge;
            sweep.arcs[idx].leftEdge = newEdge;

            //if adjacent arcs are same arc
            if(idx != sweep.arcs.Count - 1)
            {
                if (sweep.arcs[idx - 1].point == sweep.arcs[idx + 1].point)
                {
                    sweep.arcs[idx - 1].rightEdge = sweep.arcs[idx + 1].rightEdge;
                    sweep.arcs.RemoveAt(idx + 1);
                }
            }

            if(e.side == "left")
            {
                checkNewCircleEvents(sweep, idx, queue, true);
            }
            else
            {
                checkNewCircleEvents(sweep, idx - 2, queue, true);
            }
        }
    }

    //generating the whole thing at once
    public void generateWholeFortune(List<FortunePoint> points, int minX, int minY, int maxX)
    {
        queue = new PriorityQueue();
        foreach(FortunePoint point in points)
        {
            queue.enqueue(new SiteEvent(point.x, point.y, point), point.y);
        }
        firstPoint(minX, minY, maxX);
        while (!queue.isEmpty())
        {
            nextEvent();
        }

        foreach(FortuneEdge edge in edges)
        {
            Debug.Log("Segment((" + Convert.ToString(edge.start.x) + "," + Convert.ToString(edge.start.y) + "),(" + Convert.ToString(edge.end.x) + "," + Convert.ToString(edge.end.y) + "))");
        }
        foreach (FortuneArc arc in sweep.arcs)
        {
            if (!edges.Contains(arc.leftEdge))
            {
                if (!arc.leftEdge.vertical)
                {
                    if(arc.leftEdge.propDirection == "left")
                    {
                        Debug.Log($"If(x<{arc.leftEdge.start.x},{arc.leftEdge.m}x + {arc.leftEdge.c})");
                    }
                    else
                    {
                        Debug.Log($"If(x>{arc.leftEdge.start.x},{arc.leftEdge.m}x + {arc.leftEdge.c})");
                    }
                }
                else
                {
                    Debug.Log($"x = {arc.leftEdge.xVal}");
                }
            }
            if (!edges.Contains(arc.rightEdge))
            {
                if (!arc.rightEdge.vertical)
                {
                    if (arc.rightEdge.propDirection == "left")
                    {
                        Debug.Log($"If(x<{arc.rightEdge.start.x},{arc.rightEdge.m}x + {arc.rightEdge.c})");
                    }
                    else
                    {
                        Debug.Log($"If(x>{arc.rightEdge.start.x},{arc.rightEdge.m}x + {arc.rightEdge.c})");
                    }
                }
                else
                {
                    Debug.Log($"x = {arc.rightEdge.xVal}");
                }
            }
        }
    }

    //animation generation
}
