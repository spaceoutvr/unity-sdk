/**
* Copyright 2015 IBM Corp. All Rights Reserved.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
*      http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*
*/

//#define SPLINE_INTERPOLATOR

using UnityEngine;
using IBM.Watson.DeveloperCloud.Logging;

namespace IBM.Watson.DeveloperCloud.Camera
{
  /// <summary>
  /// Camera target class to identify the camera target to follow the position and rotation
  /// </summary>
  public class CameraTarget : MonoBehaviour
  {

    #region Private Members

    private WatsonCamera watsonCamera = null;
    private UnityEngine.Camera cameraAttached = null;
    [SerializeField]
    private bool useCustomPosition = false;
    [SerializeField]
    private Vector3 customPosition = Vector3.zero;
    [SerializeField]
    private Vector3 offsetPosition = Vector3.zero;
    private Quaternion offsetPositionRotation = Quaternion.identity;
    [SerializeField]
    private bool useCustomRotation = false;
    private Quaternion customRotation = Quaternion.identity;
    private bool useTargetObjectToRotate = false;
    [SerializeField]
    private GameObject customTargetObjectToLookAt = null;
    [SerializeField]
    private GameObject cameraPathRootObject = null;
    [SerializeField]
    private float ratioAtCameraPath = 0.0f;
    [SerializeField]
    private Vector3 distanceFromCamera = Vector3.zero;
#if SPLINE_INTERPOLATOR
        [SerializeField]
        private SplineInterpolator splineInterpolator;
#endif
    private Transform[] pathTransforms;

    [SerializeField]
    private bool textEnableCamera = false;
    [SerializeField]
    private bool testToMakeItCurrent = false;

    #endregion

    #region Public Members

    /// <summary>
    /// Gets or sets a value indicating whether this <see cref="IBM.Watson.DeveloperCloud.Camera.CameraTarget"/> use
    /// custom position.
    /// </summary>
    /// <value><c>true</c> if use custom position; otherwise, <c>false</c>.</value>
    public bool UseCustomPosition
    {
      get
      {
        return useCustomPosition;
      }
      set
      {
        useCustomPosition = value;
      }
    }

    /// <summary>
    /// Gets or sets a value indicating whether this <see cref="IBM.Watson.DeveloperCloud.Camera.CameraTarget"/> use
    /// custom rotation.
    /// </summary>
    /// <value><c>true</c> if use custom rotation; otherwise, <c>false</c>.</value>
    public bool UseCustomRotation
    {
      get
      {
        return useCustomRotation;
      }
      set
      {
        useCustomRotation = value;
      }
    }

    /// <summary>
    /// Gets or sets the ratio at camera path. It is used if there is path root object assigned to the system
    /// </summary>
    /// <value>The ratio at camera path.</value>
    public float RatioAtCameraPath
    {
      get
      {
        return ratioAtCameraPath;
      }
      set
      {
        ratioAtCameraPath = Mathf.Repeat(value, 1.0f);
      }
    }

    /// <summary>
    /// Gets or sets the camera path root object.
    /// </summary>
    /// <value>The camera path root object.</value>
    public GameObject CameraPathRootObject
    {
      get
      {
        return cameraPathRootObject;
      }
      set
      {
        cameraPathRootObject = value;
      }
    }

    public Vector3 OffsetPosition
    {
      get
      {
        return offsetPosition;
      }
      set
      {
        offsetPosition = value;
      }
    }

    public Vector3 DistanceFromCamera
    {
      get
      {
        return distanceFromCamera;
      }
      set
      {
        distanceFromCamera = value;
      }
    }

    public Quaternion OffsetPositionRotation
    {
      get
      {
        return offsetPositionRotation;
      }
      set
      {
        offsetPositionRotation = value;
      }
    }

    /// <summary>
    /// Gets or sets the target position.
    /// </summary>
    /// <value>The target position.</value>
    public Vector3 TargetPosition
    {
      get
      {
#if SPLINE_INTERPOLATOR
                if (cameraPathRootObject != null)
                {
                    if (pathTransforms == null)
                    {
                        List<Transform> childrenTransforms = new List<Transform>(cameraPathRootObject.GetComponentsInChildren<Transform>());

                        childrenTransforms.Remove(cameraPathRootObject.transform);
                        childrenTransforms.Sort(delegate(Transform t1, Transform t2)
                            {
                                return t1.name.CompareTo(t2.name);
                            });

                        pathTransforms = childrenTransforms.ToArray();

                        if (splineInterpolator == null)
                        {
                            splineInterpolator = this.gameObject.GetComponent<SplineInterpolator>();
                            if (splineInterpolator == null)
                                splineInterpolator = this.gameObject.AddComponent<SplineInterpolator>();
                        }

                        splineInterpolator.SetupSplineInterpolator(pathTransforms);
                    }

                    if (offsetPosition != Vector3.zero)
                    {
                        return splineInterpolator.GetHermiteAtTime(ratioAtCameraPath) + (TargetRotation * offsetPosition) + DistanceFromCamera;
                    }
                    else
                    {
                        return splineInterpolator.GetHermiteAtTime(ratioAtCameraPath) + DistanceFromCamera;
                    }

                }
                else 
#endif
        if (useCustomPosition)
        {
          return customPosition;
        }
        else if (offsetPosition != Vector3.zero)
        {
          return transform.position + (Quaternion.Euler(transform.rotation.eulerAngles - offsetPositionRotation.eulerAngles) * offsetPosition);
        }
        else
        {
          return transform.position;
        }
      }
      set
      {
        useCustomPosition = true;
        customPosition = value;
      }
    }

    /// <summary>
    /// Gets or sets the target rotation.
    /// </summary>
    /// <value>The target rotation.</value>
    public Quaternion TargetRotation
    {
      get
      {
        if (useTargetObjectToRotate)
        {
          if (TargetObject != null)
          {
            if (CameraAttached != null)
            {
              Vector3 relativePos = TargetObject.transform.position - CameraAttached.transform.position;
              if (relativePos != Vector3.zero)
                return Quaternion.LookRotation(relativePos);
              else
                return Quaternion.identity;
            }
            else
            {
              Log.Warning("CameraTarget", "WatsonCamera couldn't find");
              return Quaternion.identity;
            }
          }
          else
          {
            Log.Warning("CameraTarget", "TargetObject couldn't find");
            return Quaternion.identity;
          }
        }
        else if (useCustomRotation)
        {
          return customRotation;
        }
        else
        {
          return transform.rotation;
        }
      }
      set
      {
        useCustomRotation = true;
        customRotation = value;
      }
    }

    /// <summary>
    /// Gets or sets the target object.
    /// </summary>
    /// <value>The target object.</value>
    public GameObject TargetObject
    {
      get
      {
        return customTargetObjectToLookAt;
      }
      set
      {
        if (value != null)
        {
          useTargetObjectToRotate = true;
          customTargetObjectToLookAt = value;
        }
        else
        {
          useTargetObjectToRotate = false;
          customTargetObjectToLookAt = null;
        }
      }
    }

    /// <summary>
    /// Gets the camera attached.
    /// </summary>
    /// <value>The camera attached.</value>
    public UnityEngine.Camera CameraAttached
    {
      get
      {
        if (cameraAttached == null)
        {
          if (WatsonCameraAttached != null)
            cameraAttached = WatsonCameraAttached.GetComponent<UnityEngine.Camera>();
        }
        return cameraAttached;
      }
    }

    /// <summary>
    /// Gets the watson camera attached.
    /// </summary>
    /// <value>The watson camera attached.</value>
    public WatsonCamera WatsonCameraAttached
    {
      get
      {
        if (watsonCamera == null)
        {
          //Check if is there any local camera attached
          watsonCamera = this.gameObject.GetComponent<WatsonCamera>();

          if (watsonCamera == null)
          {
            watsonCamera = GameObject.FindObjectOfType<WatsonCamera>();
          }
        }
        return watsonCamera;
      }
    }

    #endregion

    #region Set Target on Camera

    /// <summary>
    /// Sets the current target on camera.
    /// </summary>
    /// <param name="enable">If set to <c>true</c> enable.</param>
    public void SetCurrentTargetOnCamera(bool enable)
    {
      if (WatsonCamera.Instance != null)
      {
        if (enable)
          WatsonCamera.Instance.CurrentCameraTarget = this;
        else
          WatsonCamera.Instance.CurrentCameraTarget = null;
      }
    }

    /// <summary>
    /// Sets the target position default.
    /// </summary>
    public void SetTargetPositionDefault()
    {
      if (WatsonCamera.Instance != null && WatsonCamera.Instance.DefaultCameraTarget != null)
      {
        TargetPosition = WatsonCamera.Instance.DefaultCameraTarget.TargetPosition;
      }
    }

    /// <summary>
    /// Sets the target rotation default.
    /// </summary>
    public void SetTargetRotationDefault()
    {
      if (WatsonCamera.Instance != null && WatsonCamera.Instance.DefaultCameraTarget != null)
      {
        TargetRotation = WatsonCamera.Instance.DefaultCameraTarget.TargetRotation;
      }
    }

    #endregion

    #region Update

    void Update()
    {
      if (testToMakeItCurrent)
      {
        testToMakeItCurrent = false;
        SetCurrentTargetOnCamera(textEnableCamera);
      }
    }

    #endregion

    #region public Functions

    /// <summary>
    /// Sets the target position with offset.
    /// </summary>
    /// <param name="offsetPosition"></param>
    public void SetTargetPositionWithOffset(Vector3 offset)
    {
      offsetPosition = offset;
      offsetPositionRotation = this.transform.rotation;
    }

    #endregion

#if SPLINE_INTERPOLATOR

        void OnDrawGizmos()
        {
            if (cameraPathRootObject != null)
            {
                List<Transform> childrenTransforms = new List<Transform>(cameraPathRootObject.GetComponentsInChildren<Transform>());

                childrenTransforms.Remove(cameraPathRootObject.transform);
                childrenTransforms.Sort(delegate(Transform t1, Transform t2)
                    {
                        return t1.name.CompareTo(t2.name);
                    });

                pathTransforms = childrenTransforms.ToArray();

                if (splineInterpolator == null)
                {
                    splineInterpolator = this.gameObject.GetComponent<SplineInterpolator>();
                    if (splineInterpolator == null)
                        splineInterpolator = this.gameObject.AddComponent<SplineInterpolator>();
                }

                splineInterpolator.SetupSplineInterpolator(pathTransforms);

                Vector3 prevPos = pathTransforms[0].position;
                for (int c = 1; c <= 100; c++)
                {
                    float currTime = c * 1.0f / 100;
                    Vector3 currPos = splineInterpolator.GetHermiteAtTime(currTime);
                    float mag = (currPos - prevPos).magnitude * 2;
                    Gizmos.color = new Color(mag, 0, 0, 1);
                    Gizmos.DrawLine(prevPos, currPos);
                    prevPos = currPos;
                }
            }
        }

#endif
  }

}
