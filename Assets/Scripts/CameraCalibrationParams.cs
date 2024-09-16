using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraCalibrationParams : MonoBehaviour
{
    // Calibration parameters from opencv, compute once for each hololens 2 device
    //{"camera_matrix": [[677.8968352717175, 0.0, 439.2388714449508], [0.0, 677.1775976226464, 231.50848952714483], [0.0, 0.0, 1.0]], 
    //"dist_coeff": [[-0.002602963842533594, -0.008751170499511022, -0.0022398259556777236, -5.941804169976817e-05, 0.0]], 
    //"height": 504, "width": 896}
    //677.8968352717175f, 677.1775976226464f, // focal length (0,0) & (1,1)
    //439.2388714449508f, 231.50848952714483f, // principal point (0,2) & (2,2)
    //-0.002602963842533594f, -0.008751170499511022f, 0.0f, // radial distortion (0,0) & (0,1) & (0,4)
    //-0.0022398259556777236f, -5.941804169976817e-05f, // tangential distortion (0,2) & (0,3)
    //504, 896); // image width and height

    // {"camera_matrix":
    // [[693.4305426218743, 0.0, 437.03727907144844],
    // [0.0, 695.5037202296445, 262.13567816922205],
    // [0.0, 0.0, 1.0]],
    // "dist_coeff": [[-0.012403659422355866, 0.13632714725420572, 0.0020656498447123007, -0.0012036440149849534, 0.0]],
    // "height": 504, "width": 896}

    // {"camera_matrix":
    // [[700.5947678028714, 0.0, 444.23325758595183],
    // [0.0, 699.1184687182421, 280.825939756619],
    // [0.0, 0.0, 1.0]],
    // "dist_coeff": [[0.02331962350456162, 0.013914169740196464, 0.007996713426053344, 0.0021629066035315264, 0.0]],
    // "height": 504, "width": 896}

    public Vector2 focalLength;
    public Vector2 principalPoint;
    public Vector3 radialDistortion;
    public Vector2 tangentialDistortion;
    public int imageWidth;
    public int imageHeight;
}
