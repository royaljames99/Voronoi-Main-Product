using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//SOURCE https://www.youtube.com/watch?v=0G4vcH9N0gc

public class CameraScript : MonoBehaviour
{
    private Camera cam;
    private Vector3 startPos;
    public int maxZoom;
    public int minZoom;
    public float zoomRatio;
    // Start is called before the first frame update
    void Start()
    {
        cam = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        //Panning
        if (Input.GetMouseButtonDown(0))
        {
            startPos = cam.ScreenToWorldPoint(Input.mousePosition);
        }
        if (Input.GetMouseButton(0))
        {
            Vector3 direction = startPos - cam.ScreenToWorldPoint(Input.mousePosition);
            cam.transform.position += direction;
        }

        //Zoom
        cam.orthographicSize = Mathf.Clamp(cam.orthographicSize - (Input.GetAxis("Mouse ScrollWheel") * (float)(zoomRatio * cam.orthographicSize)), minZoom, maxZoom);
    }
}
