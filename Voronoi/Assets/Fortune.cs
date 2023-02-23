#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class QueueItem{
    private FortunePoint item;
    private float weighting;
    public int nextIndex;
    public bool alive;

    public int getNextIndex()
    {
        return nextIndex;
    }
    public void setNextIndex(int newNextIndex)
    {
        nextIndex = newNextIndex;
        Debug.Log("woo " + Convert.ToString(nextIndex) + " " + Convert.ToString(newNextIndex));
    }

    public float getWeighting()
    {
        return weighting;
    }
    public FortunePoint getItem()
    {
        return item;
    }
    public bool getAlive()
    {
        return alive;
    }
    public void die()
    {
        alive = false;
    }

    public QueueItem(FortunePoint item, float weighting)
    {
        this.item = item;
        this.weighting = weighting;
        nextIndex = -1;
        alive = true;
    }
}

class PriorityQueue
{
    int front = 0;
    public List<QueueItem> queue = new List<QueueItem>();
    public PriorityQueue()
    {
        
    }

    public void enqueue(FortunePoint item, float weighting) //dynamic so queue is ambiguously typed until use
    {
        QueueItem newItem = new QueueItem(item, weighting);

        if(queue.Count == 0)
        {
            queue.Add(newItem);
            return;
        }

        bool found = false;
        int nextItemIdx = front;
        int previousItemIdx = -1;

        while (!found && nextItemIdx != -1)
        {
            if (queue[nextItemIdx].getWeighting() < weighting)
            {
                found = true;
            }
            else
            {
                previousItemIdx = nextItemIdx;
                nextItemIdx = queue[nextItemIdx].getNextIndex();
            }
        }

        bool addingToFront = false;
        if (nextItemIdx != -1 && previousItemIdx != -1)
        {
            newItem.setNextIndex(queue[previousItemIdx].getNextIndex());
        }
        else if (previousItemIdx == -1)
        {
            newItem.setNextIndex(front);
            previousItemIdx = front;
            addingToFront = true;
        }

        for (int i = 0; !found && i < queue.Count; i++)
        {
            if (queue[i].getAlive() == false)
            {
                queue[i] = newItem;
                queue[previousItemIdx].setNextIndex(i);
                return;
            }
        }
        Debug.Log(previousItemIdx);
        queue[previousItemIdx].setNextIndex(queue.Count);
        Debug.Log(queue[previousItemIdx].nextIndex);
        queue.Add(newItem);
    }

    public FortunePoint dequeue()
    {
        if (isEmpty())
        {
            return null;
        }
        Debug.Log("REEEEE");
        QueueItem item = queue[front];
        queue[front].die();
        front = item.getNextIndex();
        Debug.Log(front);
        return item.getItem();
    }

    public bool isEmpty()
    {
        bool empty = true;
        foreach(QueueItem item in queue)
        {
            if (item.getAlive() == true)
            {
                empty = false;
            }
        }
        return empty;
    }
}


class FortunePoint
{
    public float x;
    public float y;

    public FortunePoint(float x, float y)
    {
        this.x = x;
        this.y = y;
    }
}

class FortuneArc
{
    public FortunePoint point;
    public float minX;
    public float minY;
    public float a;
    public float b;
    public float c;
    public FortuneEdge? leftEdge;
    public FortuneEdge? rightEdge;

    public FortuneArc(FortunePoint point, float minX, float minY)
    {
        this.point = point;
        this.minX = minX;
        this.minY = minY;
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

class FortuneEdge
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

class SiteEvent : FortunePoint
{
    public FortunePoint site;
    public string eType = "site";

    public SiteEvent(float x, float y, FortunePoint site) : base(x, y)
    {
        this.site = site;
    }
}

class CircleEvent : FortunePoint
{
    public FortuneArc[] arcs;
    public string side;
    public string eType = "circle";
    public FortunePoint circumcentre;

    public CircleEvent(float x, float y, FortuneArc[] arcs, string side) : base(x, y)
    {
        this.arcs = arcs;
        this.side = side;
        this.circumcentre = calcCircumcentre();
    }

    private FortunePoint calcCircumcentre()
    {
        FortunePoint p1 = this.arcs[0].point;
        FortunePoint p2 = this.arcs[1].point;
        FortunePoint p3 = this.arcs[2].point;

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
}


public class Fortune : MonoBehaviour
{
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
        Debug.Log("full queue output");
        foreach(QueueItem item in queue.queue)
        {
            Debug.Log(Convert.ToString(item.alive) + " " + Convert.ToString(item.getItem().x) + " " + Convert.ToString(item.getWeighting()));
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
