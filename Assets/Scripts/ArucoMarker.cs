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
    public bool trackStarted=false;

    // Start is called before the first frame update
    void Start()
    {
        trackStarted = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
