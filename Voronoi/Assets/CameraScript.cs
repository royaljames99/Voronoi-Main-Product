using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

//SOURCE https://www.youtube.com/watch?v=0G4vcH9N0gc

public class CameraScript : MonoBehaviour
{
    private Camera cam;
    public VoronoiMaster voronoiMaster;
    private Vector3 startPos;
    private bool panning = false;
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
        int cursorType = voronoiMaster.GetComponent<VoronoiMaster>().cursorType;
        //Panning
        if (cursorType == 2)
        {
            //mouse clicked down and not over the side panel
            if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
            {
                panning = true;
                startPos = cam.ScreenToWorldPoint(Input.mousePosition);
            }
            //stop panning
            if (panning && Input.GetMouseButtonUp(0))
            {
                panning = false;
            }
            //adjust camera position
            if (panning)
            {
                if (Input.GetMouseButton(0))
                {
                    Vector3 direction = startPos - cam.ScreenToWorldPoint(Input.mousePosition);
                    cam.transform.position += direction;
                }
            }
        }

        //Zoom
        cam.orthographicSize = Mathf.Clamp(cam.orthographicSize - (Input.GetAxis("Mouse ScrollWheel") * (float)(zoomRatio * cam.orthographicSize)), minZoom, maxZoom);
    }
}
