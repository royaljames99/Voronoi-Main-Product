using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    public FortuneLine leftEdge;
    public FortuneLine rightEdge;

    public FortuneArc(FortunePoint point, float minX, float minY)
    {
        this.point = point;
        this.minX = minX;
        this.minY = minY;
    }

    public void calcVals(float sweeplineY)
    {
        float k = (self.point.y + sweeplineY) / 2;
        float p = (self.point.y - sweeplineY) / 2;

        this.a = 1 / (4 * p);
        this.b = (-1 * this.point.x) / (2 * p);
        this.c = ((Math.Pow(this.point.x, 2)) / (4 * p)) + k;
    }

    public float calcY(float x)
    {
        float y = ((this.a * (Math.Pow(x, 2))) + (this.b * x) + this.c);
        return y;
    }
}

class FortuneEdge
{
    public FortunePoint start;
    public FortunePoint end;
    public (float, float) directionVector;
    public string propDirection;
    public float m;
    public float c;
    public bool vertical;
    public float xVal;

    public FortuneEdge(FortunePoint start, (float, float) directionVector, string propDirection = null, float m = null, bool vertical = false)
    {
        this.start = start;
        this.directionVector = directionVector;
        this.propDirection = propDirection;

        if (m == null && !vertical)
        {
            this.m = (directionVector[1] / directionVector[0]);
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
        float dx = dy * this.directionVector[0];
        float x = this.start.x + dx;
        return x;
    }

    public float calcY(float x)
    {
        if (this.vertical)
        {
            return null;
        }
        float dx = x - this.start.x;
        float dy = dx * this.directionVector[1];
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


    }
}


public class Fortune : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
