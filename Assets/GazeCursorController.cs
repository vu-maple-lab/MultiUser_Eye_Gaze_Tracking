using Microsoft.MixedReality.Toolkit.UI;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GazeCursorController : MonoBehaviour
{
    [SerializeField] public float cursorScaleMax = 2.5f;
    [SerializeField] public float cursorScaleMin = 0.5f;
    [SerializeField] public GameObject slider;
    [SerializeField] public Material myMaterial;
    private float cursorScaleGradient;
    private bool isOtherCursorVisible = true;
    private int otherCursorStyle = 0;
    private GameObject otherPhotoViewObj = null;

    private bool isMyCursorVisible = false;
    private int myCursorStyle = 0;
    private GameObject myPhotoViewObj = null;
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
    }

    // Update is called once per frame
    void Update()
    {

        // Retrieve (both) photonViews in the scene
        // Assumptions: 1. Only 2 photonViews are present. 2. One is my gaze, the other is the other's gaze.
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
}
