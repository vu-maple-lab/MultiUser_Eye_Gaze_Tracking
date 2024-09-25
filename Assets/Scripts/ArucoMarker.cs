using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ArucoMarker : MonoBehaviour
{
    public int ID;
    public bool newCoordTracked;
    public GameObject transformToScreenObj;
    public GameObject markerObj;
    [HideInInspector] public int initTrackCount;
    public bool trackStarted=false;

    // Start is called before the first frame update
    void Start()
    {
        trackStarted = false;
        initTrackCount = 0;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
