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
using System.Collections;
using System;
using IBM.Watson.DeveloperCloud.DataTypes;
using IBM.Watson.DeveloperCloud.Logging;
using IBM.Watson.DeveloperCloud.Utilities;
using UnityEngine.UI;
#pragma warning disable 0414
namespace IBM.Watson.DeveloperCloud.Widgets
{
  /// <summary>
  /// This widget gets video from the WebCam. Note: there are issues with WebCamTexture. Do not
  /// start the WebCamTexture at too low resolution!
  /// </summary>
  public class WebCamWidget : Widget
  {
    #region Inputs
    [SerializeField]
    private Input disableInput = new Input("Disable", typeof(DisableWebCamData), "OnDisableInput");
    #endregion

    #region Outputs
    [SerializeField]
    private Output webCamTextureOutput = new Output(typeof(WebCamTextureData));
    [SerializeField]
    private Output activateOutput = new Output(typeof(BooleanData));
    #endregion

    #region Private Data
    private bool active = false;
    private bool disabled = false;
    private bool failure = false;
    private DateTime lastFailure = DateTime.Now;

    [SerializeField]
    private bool activateOnStart = true;
    [SerializeField]
    private int requestedWidth = 640;
    [SerializeField]
    private int requestedHeight = 480;
    [SerializeField]
    private int requestedFPS = 60;
    [SerializeField]
    private Text statusText = null;

    private int recordingRoutine = 0;                      // ID of our co-routine when recording, 0 if not recording currently.
    private WebCamTexture webCamTexture;
    private int webCamIndex = 0;
    #endregion

    #region Public Properties
    /// <summary>
    /// True if WebCam is active, false if inactive.
    /// </summary>
    public bool Active
    {
      get { return active; }
      set
      {
        if (active != value)
        {
          active = value;
          if (active && !disabled)
            StartRecording();
          else
            StopRecording();
        }
      }
    }
    /// <summary>
    /// True if WebCamera is disabled, false if enabled.
    /// </summary>
    public bool Disable
    {
      get { return disabled; }
      set
      {
        if (disabled != value)
        {
          disabled = value;
          if (active && !disabled)
            StartRecording();
          else
            StopRecording();
        }
      }
    }
    /// <summary>
    /// This is set to true when the WebCam fails, the update will continue to try to start
    /// the microphone so long as it's active.
    /// </summary>
    public bool Failure
    {
      get { return failure; }
      private set
      {
        if (failure != value)
        {
          failure = value;
          if (failure)
            lastFailure = DateTime.Now;
        }
      }
    }
    /// <summary>
    /// Returns all available WebCameras.
    /// </summary>
    public WebCamDevice[] Devices
    {
      get { return WebCamTexture.devices; }
    }
    /// <summary>
    /// The curent WebCamTexture
    /// </summary>
    public WebCamTexture WebCamTexture
    {
      get { return webCamTexture; }
    }
    /// <summary>
    /// The requested width of the WebCamTexture.
    /// </summary>
    public int RequestedWidth
    {
      get { return requestedWidth; }
      set { requestedWidth = value; }
    }
    /// <summary>
    /// The requested height of the WebCamTexture.
    /// </summary>
    public int RequestedHeight
    {
      get { return requestedHeight; }
      set { requestedHeight = value; }
    }

    /// <summary>
    /// The requested frame rate of the WebCamTexture.
    /// </summary>
    public int RequestedFPS
    {
      get { return requestedFPS; }
      set { requestedFPS = value; }
    }
    #endregion

    #region Public Funtions
    /// <summary>
    /// Activates the WebCam.
    /// </summary>
    public void ActivateWebCam()
    {
      Active = true;
    }

    /// <summary>
    /// Deactivates the WebCam.
    /// </summary>
    public void DeactivateWebCam()
    {
      Active = false;
    }

    /// <summary>
    /// Switches WebCamDevice to the next device.
    /// </summary>
    public void SwitchWebCam()
    {
      WebCamDevice[] devices = Devices;

      if (devices.Length == 0)
        throw new WatsonException(string.Format("There are no WebCam devices!"));
      if (devices.Length == 1)
      {
        Log.Warning("WebCamWidget", "There is only one WebCam device!");
        return;
      }

      webCamTexture.Stop();
      int requestedIndex;
      if (webCamIndex == devices.Length - 1)
        requestedIndex = 0;
      else
        requestedIndex = webCamIndex + 1;
      webCamTexture.deviceName = devices[requestedIndex].name;
      webCamIndex = requestedIndex;
      Log.Status("WebCamWidget", "Switched to WebCam {0}, name: {1}, isFontFacing: {2}.", webCamIndex, devices[webCamIndex].name, devices[webCamIndex].isFrontFacing);
      webCamTexture.Play();
    }

    /// <summary>
    /// Switches the WebCam device based on index.
    /// </summary>
    /// <param name="index">The WebCam index.</param>
    public void SwitchWebCam(int index)
    {
      WebCamDevice[] devices = Devices;

      if (index < devices.Length)
      {
        throw new WatsonException(string.Format("Requested WebCam index {0} does not exist! There are {1} available WebCams.", index, devices.Length));
      }

      webCamTexture.Stop();
      webCamTexture.deviceName = devices[index].name;
      webCamIndex = index;
      Log.Status("WebCamWidget", "Switched to WebCam {0}, name: {1}, isFontFacing: {2}.", webCamIndex, devices[webCamIndex].name, devices[webCamIndex].isFrontFacing);
      webCamTexture.Play();
    }

    /// <summary>
    /// Toggles between Active and Inactive states.
    /// </summary>
    public void ToggleActive()
    {
      Active = !Active;
    }
    #endregion

    #region EventHandlers
    protected override void Start()
    {
      base.Start();
      LogSystem.InstallDefaultReactors();
      Log.Debug("WebCamWidget", "WebCamWidget.Start();");

      if (activateOnStart)
        Active = true;
    }

    private void OnDisableInput(Data data)
    {
      Disable = ((DisableWebCamData)data).Boolean;
    }
    #endregion

    #region Widget Interface
    protected override string GetName()
    {
      return "WebCam";
    }
    #endregion

    #region Recording Functions
    private void StartRecording()
    {
      Log.Debug("WebCamWidget", "StartRecording(); recordingRoutine: {0}", recordingRoutine);
      if (recordingRoutine == 0)
      {
        //UnityObjectUtil.StartDestroyQueue();

        recordingRoutine = Runnable.Run(RecordingHandler());
        activateOutput.SendData(new BooleanData(true));

        if (statusText != null)
          statusText.text = "WEB CAMERA ON";
      }
    }

    private void StopRecording()
    {
      Log.Debug("WebCamWidget", "StopRecording();");
      if (recordingRoutine != 0)
      {
        Runnable.Stop(recordingRoutine);
        recordingRoutine = 0;

        activateOutput.SendData(new BooleanData(false));

        webCamTexture.Stop();

        if (statusText != null)
          statusText.text = "WEB CAMERA OFF";
      }
    }

    private IEnumerator RecordingHandler()
    {
      Failure = false;

#if !UNITY_EDITOR
			yield return Application.RequestUserAuthorization(UserAuthorization.WebCam);
#endif

      webCamTexture = new WebCamTexture(requestedWidth, requestedHeight, requestedFPS);
      yield return null;

      if (webCamTexture == null)
      {
        Failure = true;
        StopRecording();
        yield break;
      }

      WebCamTextureData camData = new WebCamTextureData(webCamTexture, requestedWidth, requestedHeight, requestedFPS);
      webCamTextureOutput.SendData(camData);
    }
    #endregion
  }
}
