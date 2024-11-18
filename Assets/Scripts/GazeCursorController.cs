using Microsoft.MixedReality.Toolkit.UI;
using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;

/* 
 * Also In Charge of General Frame Rate settings.
 */

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
    private List<GameObject> recordingGameObjs = null;

    private DateTime curRecordStartTime;

    private StreamWriter myGazeWriter = null;
    private StreamWriter otherGazeWriter = null;
    private StreamWriter screenPosWriter = null;

    // Start is called before the first frame update
    void Start()
    {
        Time.fixedDeltaTime = 0.016666667f;
        // Application.targetFrameRate = -1;
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
        recordingGameObjs = new List<GameObject> { myPhotoViewObj, otherPhotoViewObj, screenObj };
    }

    // Update is called once per frame
    void FixedUpdate()
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

            DateTime curTime = DateTime.Now;
            saveTransformData(
                myPhotoViewObj.transform.localPosition.x,
                myPhotoViewObj.transform.localPosition.y,
                myPhotoViewObj.transform.localPosition.z,
                myPhotoViewObj.transform.localRotation.w,
                myPhotoViewObj.transform.localRotation.x,
                myPhotoViewObj.transform.localRotation.y,
                myPhotoViewObj.transform.localRotation.z,
                curRecordStartTime,
                curTime,
                ref myGazeWriter,
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
                    curRecordStartTime,
                    curTime,
                    ref otherGazeWriter,
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
                    curRecordStartTime,
                    curTime,
                    ref screenPosWriter,
                    "screen_Track_Transforms"
                );
            }
        }
    }

    // Not used; Don't Use; This function will create a file descriptor each time it is called, which is hyper-inefficient
    public void saveAllTransformData(List<GameObject> gameObjs, DateTime recordStartTime, string fileNamePrefix = "combined_track_coordinate")
    {
        if (!isRecording)
        {
            return;
        }
        string timeStamp = recordStartTime.ToLocalTime().ToString("yyyyMMdd_HHmmss");

        DateTime curTime = DateTime.Now;
        long curUnixTime = ((DateTimeOffset)curTime).ToUnixTimeMilliseconds();
        string curUnixTimeString = curUnixTime.ToString();

        string filepath_in_function2 = Application.persistentDataPath + "/" + fileNamePrefix + "_" + timeStamp + "_" + recordingTrialCount + ".csv";
        string curLine = "";
        foreach (GameObject obj in gameObjs)
        {
            if (obj != null)
            {
                Transform T = obj.transform;
                curLine += T.localPosition.x + "," + T.localPosition.y + "," + T.localPosition.z + "," + T.localRotation.w + "," + T.localRotation.x + "," + T.localRotation.y + "," + T.localRotation.z + ",";
            }
            else
            {
                curLine += -1 + "," + -1 + "," + -1 + "," + -1 + "," + -1 + "," + -1 + "," + -1 + ",";
            }
        }
        curLine += timeStamp + "," + curUnixTimeString;
        try
        {
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(@filepath_in_function2, true))
            {
                file.WriteLine(curLine);
            }
        }
        catch (Exception ex)
        {
            throw new ApplicationException("Failed to save:  ", ex);
        }
    }

    public void saveTransformData(float px, float py, float pz, float rw, float rx, float ry, float rz, DateTime recordStartTime, DateTime curTime, ref StreamWriter writer, string fileNamePrefix = "my_Eye_Gaze_Coordinate")
    {
        // long startUnixTime = ((DateTimeOffset)recordStartTime).ToUnixTimeMilliseconds();
        string timeStamp = recordStartTime.ToLocalTime().ToString("yyyyMMdd_HHmmss");

        long curUnixTime = ((DateTimeOffset)curTime).ToUnixTimeMilliseconds();
        string curUnixTimeString = curUnixTime.ToString();
        

        if (isRecording)
        {
            //Debug.Log("filepath" + filepath_in_function2);
            //string test_filePath = "U:/Users/yizhou.li@vanderbilt.edu/AppData/Local/Packages/Eyerecorder_pzq3xp76mxafg/LocalState/position_Cursor_IO.csv"; 
            try
            {
                string curFilePath = Application.persistentDataPath + "/" + fileNamePrefix + "_" + timeStamp + "_" + recordingTrialCount + ".csv";
                writer ??= new System.IO.StreamWriter(@curFilePath, true);
                writer.WriteLine(px + "," + py + "," + pz + "," + rw + "," + rx + "," + ry + "," + rz + "," + timeStamp + "," + curUnixTimeString);
            }

            catch (Exception ex)
            {
                writer?.Dispose();
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


    private void updateMyCursorStyle(int newIdx)
    {
        if (myPhotoViewObj == null)
        {
            return;
        }

        myPhotoViewObj.transform.GetChild(myCursorStyle).gameObject.GetComponent<Renderer>().enabled = false;
        myCursorStyle = newIdx % myPhotoViewObj.transform.childCount;
        myPhotoViewObj.transform.GetChild(myCursorStyle).gameObject.GetComponent<Renderer>().enabled = true;

        Debug.Log("Updated My Cursor's Visibility to " + myCursorStyle.ToString());
    }

    public void onCycleMyCursorStyle()
    {
        updateMyCursorStyle((myCursorStyle + 1) % myPhotoViewObj.transform.childCount);
    }

    private void updateOtherCursorStyle(int newIdx)
    {
        if (otherPhotoViewObj == null)
        {
            return;
        }

        otherPhotoViewObj.transform.GetChild(otherCursorStyle).gameObject.GetComponent<Renderer>().enabled = false;
        otherCursorStyle = newIdx % otherPhotoViewObj.transform.childCount;
        otherPhotoViewObj.transform.GetChild(otherCursorStyle).gameObject.GetComponent<Renderer>().enabled = true;

        Debug.Log("Updated Other Cursor's Visibility to " + myCursorStyle.ToString());
    }

    public void onCycleOtherCursorStyle()
    {
        updateOtherCursorStyle((otherCursorStyle + 1) % otherPhotoViewObj.transform.childCount);
    }

    public void onUpdateCursorScale()
    {
        float scale = slider.GetComponent<PinchSlider>().SliderValue;
        setCursorScale(scale);
    }

    private void setCursorScale(float scale)
    {
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
            hideCursor(cursorObj);
        }
        else
        {
            showCursor(cursorObj, visibleChildIdx);
        }

    }

    private void hideCursor(GameObject cursorObj)
    {
        for (int childIdx = 0; childIdx < cursorObj.gameObject.transform.childCount; childIdx++)
        {
            cursorObj.transform.GetChild(childIdx).gameObject.GetComponent<Renderer>().enabled = false;
        }
    }

    private void showCursor(GameObject cursorObj, int visibleChildIdx)
    {
        cursorObj.transform.GetChild(visibleChildIdx).gameObject.GetComponent<Renderer>().enabled = true;
    }

    private void startRecording()
    {
        if (isRecording)
        {
            return;
        }
        isRecording = true;
        if (buttonTMP != null)
        {
            buttonTMP.GetComponent<TextMeshPro>().text = "Stop Recording Data";
        }
        myGazeWriter?.Dispose();
        otherGazeWriter?.Dispose();
        screenPosWriter?.Dispose();

        curRecordStartTime = DateTime.Now;
        string timeStamp = curRecordStartTime.ToLocalTime().ToString("yyyyMMdd_HHmmss");
        string myGazeFilePath = Application.persistentDataPath + "/" + "my_Eye_Gaze_Transforms" + "_" + timeStamp + "_" + recordingTrialCount + ".csv";
        string otherGazeFilePath = Application.persistentDataPath + "/" + "other_Eye_Gaze_Transforms" + "_" + timeStamp + "_" + recordingTrialCount + ".csv";
        string screenPoseFilePath = Application.persistentDataPath + "/" + "screen_Track_Transforms" + "_" + timeStamp + "_" + recordingTrialCount + ".csv";
        myGazeWriter = new System.IO.StreamWriter(myGazeFilePath, true);
        otherGazeWriter = new System.IO.StreamWriter(otherGazeFilePath, true);
        screenPosWriter = new System.IO.StreamWriter(screenPoseFilePath, true);
    }

    private void stopRecording()
    {
        if (!isRecording)
        {
            return;
        }
        recordingTrialCount++;
        isRecording = false;
        if (buttonTMP != null)
        {
            buttonTMP.GetComponent<TextMeshPro>().text = "Record Gaze Data";
        }
        myGazeWriter?.Dispose();
        otherGazeWriter?.Dispose();
        screenPosWriter?.Dispose();
    }

    public void onToggleGazeRecording()
    {
        if (isRecording)
        {
            stopRecording();
        }
        else
        {
            startRecording();
        }
    }
}
