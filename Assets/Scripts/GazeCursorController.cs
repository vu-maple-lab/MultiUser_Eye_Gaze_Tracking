using Microsoft.MixedReality.Toolkit.UI;
using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GazeCursorController : MonoBehaviour
{
    [SerializeField] public float cursorScaleMax = 2.5f;
    [SerializeField] public float cursorScaleMin = 0.5f;
    [SerializeField] public GameObject slider;
    [SerializeField] public Material myMaterial;
    [SerializeField] public GameObject recordButton = null;
    [SerializeField] public GameObject screenObj = null;
    private float cursorScaleGradient;
    private bool isOtherCursorVisible = true;
    private int otherCursorStyle = 0;
    private GameObject otherPhotoViewObj = null;
    private GameObject buttonTMP = null;

    private bool isMyCursorVisible = false;
    private int myCursorStyle = 0;
    private GameObject myPhotoViewObj = null;
    [HideInInspector] public bool isRecording;
    [HideInInspector] public int recordingTrialCount;

    private DateTime curStartTime;

    // Start is called before the first frame update
    void Start()
    {
        cursorScaleGradient = cursorScaleMax - cursorScaleMin;
        float curScaleValue = (1 - cursorScaleMin) / cursorScaleGradient;
        slider.GetComponent<PinchSlider>().SliderValue = curScaleValue;
        myPhotoViewObj = null;
        otherPhotoViewObj = null;

        isOtherCursorVisible = true;
        isMyCursorVisible = true;
        onToggleMyCursorVisibility();

        if (recordButton != null )
        {
            buttonTMP = recordButton.transform.Find("IconAndText").Find("TextMeshPro").gameObject;
        }
    }

    // Update is called once per frame
    void Update()
    {

        // Retrieve (both) photonViews in the scene
        // Assumptions: 1. Only 2 photonViews are present. 2. One is my gaze, the other is the other's gaze.
        // TODO: very inefficient code, try improve it in the future
        var photonViews = FindObjectsOfType<PhotonView>();
        foreach (PhotonView view in photonViews)
        {
            if (view.IsMine)
            { 
                if (myPhotoViewObj == null)
                {
                    myPhotoViewObj = view.gameObject;
                    for (int i = 0; i < myPhotoViewObj.transform.childCount; i++)
                    {
                        myPhotoViewObj.transform.GetChild(i).gameObject.GetComponent<Renderer>().material = myMaterial;
                    }
                }
            } else 
            {
                otherPhotoViewObj = view.gameObject;
            }
        }

        if (isRecording)
        {
            // Data to Save:
            // My Gaze, local to screen
            // Other's Gaze, local to screen
            // My Screen Position
            saveTransformData(
                myPhotoViewObj.transform.localPosition.x, 
                myPhotoViewObj.transform.localPosition.y, 
                myPhotoViewObj.transform.localPosition.z,
                myPhotoViewObj.transform.localRotation.w,
                myPhotoViewObj.transform.localRotation.x,
                myPhotoViewObj.transform.localRotation.y,
                myPhotoViewObj.transform.localRotation.z,
                curStartTime,
                "my_Eye_Gaze_Transforms"
                );
            if (otherPhotoViewObj != null)
            {
                saveTransformData(
                    otherPhotoViewObj.transform.localPosition.x,
                    otherPhotoViewObj.transform.localPosition.y,
                    otherPhotoViewObj.transform.localPosition.z,
                    otherPhotoViewObj.transform.localRotation.w,
                    otherPhotoViewObj.transform.localRotation.x,
                    otherPhotoViewObj.transform.localRotation.y,
                    otherPhotoViewObj.transform.localRotation.z,
                    curStartTime,
                    "other_Eye_Gaze_Transforms"
                );
            }

            if (screenObj != null)
            {
                saveTransformData(
                    screenObj.transform.localPosition.x,
                    screenObj.transform.localPosition.y,
                    screenObj.transform.localPosition.z,
                    screenObj.transform.localRotation.w,
                    screenObj.transform.localRotation.x,
                    screenObj.transform.localRotation.y,
                    screenObj.transform.localRotation.z,
                    curStartTime,
                    "screen_Track_Transforms"
                );
            }
        }
    }

    public void saveTransformData(float px, float py, float pz, float rw, float rx, float ry, float rz, DateTime recordStartTime, string fileNamePrefix = "my_Eye_Gaze_Coordinate")
    {
        long unixTime = ((DateTimeOffset)recordStartTime).ToUnixTimeMilliseconds();
        string unixTime_String = unixTime.ToString();
        string timeStamp = recordStartTime.ToLocalTime().ToString("yyyyMMdd_HHmmss");
        
        if (isRecording)
        {
            string filepath_in_function2 = Application.persistentDataPath + "/" + fileNamePrefix + "_" + timeStamp + "_" + recordingTrialCount + ".csv";
            //Debug.Log("filepath" + filepath_in_function2);
            //string test_filePath = "U:/Users/yizhou.li@vanderbilt.edu/AppData/Local/Packages/Eyerecorder_pzq3xp76mxafg/LocalState/position_Cursor_IO.csv";
            try
            {
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(@filepath_in_function2, true))
                {
                    file.WriteLine(px + "," + py + "," + pz + "," + rw + "," + rx + "," + ry + "," + rz + "," + timeStamp + "," + unixTime_String);
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Failed to save:  ", ex);
            }
        }
    }

    public void onToggleMyCursorVisibility()
    {
        if (myPhotoViewObj == null)
        {
            return;
        }

        onToggleVisibility(myPhotoViewObj, isMyCursorVisible, myCursorStyle);
        isMyCursorVisible = !isMyCursorVisible;

        Debug.Log("Toggled My Cursor's Visibility to " + isMyCursorVisible.ToString());
    }

    public void onToggleOtherCursorVisibility()
    {
        if (otherPhotoViewObj == null)
        {
            return;
        }

        onToggleVisibility(otherPhotoViewObj, isOtherCursorVisible, otherCursorStyle);
        isOtherCursorVisible = !isOtherCursorVisible;

        Debug.Log("Toggled Other Cursor's Visibility to " + isOtherCursorVisible.ToString());
    }

    public void onCycleMyCursorStyle()
    {
        if (myPhotoViewObj == null)
        {
            return;
        }

        myPhotoViewObj.transform.GetChild(myCursorStyle).gameObject.GetComponent<Renderer>().enabled = false;
        myCursorStyle = (myCursorStyle + 1) % myPhotoViewObj.transform.childCount;
        myPhotoViewObj.transform.GetChild(myCursorStyle).gameObject.GetComponent<Renderer>().enabled = true;

        Debug.Log("Cycled My Cursor's Visibility to " + myCursorStyle.ToString());
    }

    public void onCycleOtherCursorStyle()
    {
        if (otherPhotoViewObj == null)
        {
            return;
        }

        otherPhotoViewObj.transform.GetChild(otherCursorStyle).gameObject.GetComponent<Renderer>().enabled = false;
        otherCursorStyle = (otherCursorStyle + 1) % otherPhotoViewObj.transform.childCount;
        otherPhotoViewObj.transform.GetChild(otherCursorStyle).gameObject.GetComponent<Renderer>().enabled = true;

        Debug.Log("Cycled Other Cursor's Visibility to " + otherCursorStyle.ToString());

    }

    public void onUpdateCursorScale()
    {
        float scale = slider.GetComponent<PinchSlider>().SliderValue;
        float convertedScale = scale * cursorScaleGradient + cursorScaleMin;
        if (myPhotoViewObj != null)
        {
            myPhotoViewObj.transform.localScale = new Vector3(convertedScale, convertedScale, convertedScale);
        }
        if (otherPhotoViewObj != null)
        {
            otherPhotoViewObj.transform.localScale = new Vector3(convertedScale, convertedScale, convertedScale);
        }
        Debug.Log("Updated Cursors' Scale to" + convertedScale.ToString());
    }

    private void onToggleVisibility(GameObject cursorObj, bool curVisibility, int visibleChildIdx=0)
    {
        if (curVisibility == true)
        {
            for (int childIdx = 0; childIdx < cursorObj.gameObject.transform.childCount; childIdx++)
            {
                cursorObj.transform.GetChild(childIdx).gameObject.GetComponent<Renderer>().enabled = false;
            }
        }
        else
        {
            cursorObj.transform.GetChild(visibleChildIdx).gameObject.GetComponent<Renderer>().enabled = true;
        }

    }

    public void onToggleGazeRecording()
    {
        if (isRecording)
        {
            recordingTrialCount++;
            isRecording = false;
            if (buttonTMP != null)
            {
                buttonTMP.GetComponent<TextMeshPro>().text = "Record Gaze Data";
            }
        } else
        {
            isRecording = true;
            if (buttonTMP != null)
            {
                buttonTMP.GetComponent<TextMeshPro>().text = "Stop Recording Data";
            }
            curStartTime = DateTime.Now;
        }
    }
}
