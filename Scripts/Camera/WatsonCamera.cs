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

using UnityEngine;
using System.Collections.Generic;
using IBM.Watson.DeveloperCloud.Logging;
using IBM.Watson.DeveloperCloud.Utilities;

namespace IBM.Watson.DeveloperCloud.Camera
{

  /// <summary>
  /// Watson camera. The main responsible camera on the scene of the Watson applications which handles the camera movements via touch / keyboard inputs / voice commands.
  /// </summary>
  public class WatsonCamera : MonoBehaviour
  {

    #region Private Variables
    private static WatsonCamera mp_Instance;
    private List<CameraTarget> listCameraTarget = new List<CameraTarget>();
    private CameraTarget targetCamera = null;

    protected Vector3 cameraInitialLocation;
    protected Quaternion cameraInitialRotation;
    [SerializeField]
    protected float panSpeed = 0.07f;
    [SerializeField]
    protected float zoomSpeed = 20.0f;
    [SerializeField]
    protected float speedForCameraAnimation = 2f;

    private float commandMovementModifier = 10.0f;

    [SerializeField]
    private MonoBehaviour antiAliasing;
    [SerializeField]
    private MonoBehaviour depthOfField;
    protected bool disableInteractivity = false;

    #endregion

    #region Public Variable

    /// <summary>
    /// Static instance of WatsonCamera.
    /// </summary>
    public static WatsonCamera Instance
    {
      get
      {
        return mp_Instance;
      }
    }

    /// <summary>
    /// The camera's current target.
    /// </summary>
    public CameraTarget CurrentCameraTarget
    {
      get
      {
        if (targetCamera == null)
        {
          InitializeCameraTargetList();
        }

        return targetCamera;
      }
      set
      {
        if (value != null)
        {
          targetCamera = value;

          if (!listCameraTarget.Contains(value))
          {
            listCameraTarget.Add(value);
          }
        }
        else
        {   //Delete current camera and clear from the list

          if (listCameraTarget.Contains(targetCamera))
          {
            listCameraTarget.Remove(targetCamera);
          }

          if (listCameraTarget.Count > 0)
          {
            targetCamera = listCameraTarget[listCameraTarget.Count - 1];
          }
          else
          {
            InitializeCameraTargetList();
          }
        }
      }
    }

    /// <summary>
    /// The camera's default target.
    /// </summary>
    public CameraTarget DefaultCameraTarget
    {
      get
      {
        if (listCameraTarget == null || listCameraTarget.Count == 0)
          InitializeCameraTargetList();

        return listCameraTarget[0];
      }
    }

    #endregion

    #region Event Registration

    protected virtual void OnEnable()
    {
      EventManager.Instance.RegisterEventReceiver("OnCameraReset", ResetCameraPosition);
      EventManager.Instance.RegisterEventReceiver("OnCameraSetAntiAliasing", OnCameraSetAntiAliasing);
      EventManager.Instance.RegisterEventReceiver("OnCameraSetDepthOfField", OnCameraSetDepthOfField);
      EventManager.Instance.RegisterEventReceiver("OnCameraSetInteractivity", OnCameraSetTwoFingerDrag);
    }

    protected virtual void OnDisable()
    {
      EventManager.Instance.UnregisterEventReceiver("OnCameraReset", ResetCameraPosition);
      EventManager.Instance.UnregisterEventReceiver("OnCameraSetAntiAliasing", OnCameraSetAntiAliasing);
      EventManager.Instance.UnregisterEventReceiver("OnCameraSetDepthOfField", OnCameraSetDepthOfField);
      EventManager.Instance.UnregisterEventReceiver("OnCameraSetInteractivity", OnCameraSetTwoFingerDrag);
    }

    #endregion

    #region Start / Update

    protected virtual void Awake()
    {
      mp_Instance = this;
    }

    protected virtual void Start()
    {
      cameraInitialLocation = transform.localPosition;
      cameraInitialRotation = transform.rotation;
    }

    protected virtual void Update()
    {
      CameraPositionOnUpdate();
    }

    protected virtual void CameraPositionOnUpdate()
    {
      //For Zooming and Panning
      if (CurrentCameraTarget != null)
      {
        transform.localPosition = Vector3.Lerp(transform.localPosition, CurrentCameraTarget.TargetPosition, Time.deltaTime * speedForCameraAnimation);
        transform.rotation = Quaternion.Slerp(transform.localRotation, CurrentCameraTarget.TargetRotation, Time.deltaTime * speedForCameraAnimation);
      }
    }

    protected virtual void InitializeCameraTargetList()
    {
      if (listCameraTarget == null)
        listCameraTarget = new List<CameraTarget>();

      for (int i = 0; listCameraTarget != null && i < listCameraTarget.Count; i++)
      {
        Destroy(listCameraTarget[i]);
      }

      listCameraTarget.Clear();

      CameraTarget defaultCameraTarget = this.gameObject.GetComponent<CameraTarget>();
      if (defaultCameraTarget == null)
        defaultCameraTarget = this.gameObject.AddComponent<CameraTarget>();

      defaultCameraTarget.TargetPosition = cameraInitialLocation;
      if (defaultCameraTarget.TargetObject == null)
      {
        defaultCameraTarget.TargetRotation = cameraInitialRotation;
      }
      else
      {
        defaultCameraTarget.TargetObject = defaultCameraTarget.TargetObject;
      }

      listCameraTarget.Add(defaultCameraTarget);

      targetCamera = listCameraTarget[0];
    }

    #endregion

    #region Touch Drag Actions

    /// <summary>
    /// Event handler to pan and zoom with two-finger dragging
    /// </summary>
    /// <param name="args">Arguments.</param>
    public virtual void DragTwoFinger(System.Object[] args)
    {
      if (disableInteractivity)
        return;

      if (args != null && args.Length > 0 && args[0] is TouchScript.Gestures.ScreenTransformGesture)
      {
        TouchScript.Gestures.ScreenTransformGesture transformGesture = args[0] as TouchScript.Gestures.ScreenTransformGesture;

        //Pannning with 2-finger
        DefaultCameraTarget.TargetPosition += (transformGesture.DeltaPosition * panSpeed * -1.0f);
        //Zooming with 2-finger
        DefaultCameraTarget.TargetPosition += transform.forward * (transformGesture.DeltaScale - 1.0f) * zoomSpeed;
      }
      else
      {
        Log.Warning("WatsonCamera", "TwoFinger drag has invalid argument");
      }
    }

    #endregion

    #region Camera Events Received from Outside - Set default position / Move Left - Right - Up - Down / Zoom-in-out
    /// <summary>
    /// Event Handler for setting Antialiasing event
    /// </summary>
    /// <param name="args">Arguments.</param>
    protected virtual void OnCameraSetAntiAliasing(System.Object[] args)
    {
      if (args != null && args.Length > 0 && args[0] is bool)
      {
        bool valueSet = (bool)args[0];

        if (antiAliasing != null)
        {
          antiAliasing.enabled = valueSet;
        }
      }
    }

    /// <summary>
    /// Event Handler for setting Depth of Field event
    /// </summary>
    /// <param name="args">Arguments.</param>
    protected virtual void OnCameraSetDepthOfField(System.Object[] args)
    {
      if (args != null && args.Length > 0 && args[0] is bool)
      {
        bool valueSet = (bool)args[0];

        if (depthOfField != null)
        {
          depthOfField.enabled = valueSet;
        }
      }
    }

    /// <summary>
    /// Event Handler for Two Finger Drag
    /// </summary>
    /// <param name="args">Arguments.</param>
    protected virtual void OnCameraSetTwoFingerDrag(System.Object[] args)
    {
      if (args != null && args.Length > 0 && args[0] is bool)
      {
        disableInteractivity = !(bool)args[0];
      }
    }


    /// <summary>
    /// Event handler reseting the camera. Deleting all camera target and set the initial as default. 
    /// </summary>
    /// <param name="args">Arguments.</param>
    protected virtual void ResetCamera(System.Object[] args)
    {
      if (disableInteractivity)
        return;

      InitializeCameraTargetList();
    }

    /// <summary>
    /// Event handler reseting the camera position.
    /// </summary>
    /// <param name="args">Arguments.</param>
    protected virtual void ResetCameraPosition(System.Object[] args)
    {
      if (disableInteractivity)
        return;
      //Log.Status("WatsonCamera", "Reset Camera Position");
      CurrentCameraTarget.TargetPosition = cameraInitialLocation;
      CurrentCameraTarget.TargetRotation = cameraInitialRotation;
    }


    /// <summary>
    /// Event handler moving the camera up.
    /// </summary>
    /// <param name="args">Arguments.</param>
    protected virtual void MoveUp(System.Object[] args)
    {
      if (disableInteractivity)
        return;

      DefaultCameraTarget.TargetPosition += this.transform.up * commandMovementModifier;
    }

    /// <summary>
    /// Event handler moving the camera down.
    /// </summary>
    /// <param name="args">Arguments.</param>
    protected virtual void MoveDown(System.Object[] args)
    {
      if (disableInteractivity)
        return;

      DefaultCameraTarget.TargetPosition += this.transform.up * -commandMovementModifier;
    }

    /// <summary>
    /// Event handler moving the camera left.
    /// </summary>
    /// <param name="args">Arguments.</param>
    protected virtual void MoveLeft(System.Object[] args)
    {
      if (disableInteractivity)
        return;

      DefaultCameraTarget.TargetPosition += this.transform.right * -commandMovementModifier;
    }

    /// <summary>
    /// Event handler moving the camera right.
    /// </summary>
    /// <param name="args">Arguments.</param>
    protected virtual void MoveRight(System.Object[] args)
    {
      if (disableInteractivity)
        return;

      DefaultCameraTarget.TargetPosition += this.transform.right * commandMovementModifier;
    }

    /// <summary>
    /// Event handler zooming-in the camera.
    /// </summary>
    /// <param name="args">Arguments.</param>
    protected virtual void ZoomIn(System.Object[] args)
    {
      if (disableInteractivity)
        return;

      DefaultCameraTarget.TargetPosition += transform.forward * zoomSpeed;
    }

    /// <summary>
    /// Event handler zooming-out the camera.
    /// </summary>
    /// <param name="args">Arguments.</param>
    protected virtual void ZoomOut(System.Object[] args)
    {
      if (disableInteractivity)
        return;

      DefaultCameraTarget.TargetPosition += transform.forward * zoomSpeed * -1.0f;
    }

    #endregion
  }
}
