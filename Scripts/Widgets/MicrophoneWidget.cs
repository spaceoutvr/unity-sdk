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

using IBM.Watson.DeveloperCloud.DataTypes;
using IBM.Watson.DeveloperCloud.Logging;
using IBM.Watson.DeveloperCloud.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#pragma warning disable 414

namespace IBM.Watson.DeveloperCloud.Widgets
{
  /// <summary>
  /// This widget records audio from the microphone device.
  /// </summary>
  public class MicrophoneWidget : Widget
  {
    #region Inputs
    [SerializeField]
    private Input disableInput = new Input("Disable", typeof(DisableMicData), "OnDisableInput");
    #endregion

    #region Outputs
    [SerializeField]
    private Output audioOutput = new Output(typeof(AudioData));
    [SerializeField]
    private Output levelOutput = new Output(typeof(LevelData));
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
    [SerializeField, Tooltip("Size of recording buffer in seconds.")]
    private int recordingBufferSize = 2;
    [SerializeField]
    private int recordingHZ = 22050;                  // default recording HZ
    [SerializeField, Tooltip("ID of the microphone to use.")]
    private string microphoneID = null;               // what microphone to use for recording.
    [SerializeField, Tooltip("How often to sample for level output.")]
    private float levelOutputInterval = 0.05f;
    [SerializeField]
    private float levelOutputModifier = 1.0f;
    [SerializeField, Tooltip("If true, microphone will playback recorded audio on stop.")]
    private bool playbackRecording = false;
    [SerializeField]
    private Text statusText = null;

    private int recordingRoutine = 0;                      // ID of our co-routine when recording, 0 if not recording currently.
    private AudioClip recording = null;
    private List<AudioClip> playback = new List<AudioClip>();
    #endregion

    #region Constants
    // how often to retry the open the microphone on failure
    private const int RETRY_INTERVAL = 10000;
    #endregion

    #region Public Properties
    /// <summary>
    /// Returns a list of available microphone devices.
    /// </summary>
    public string[] Microphones { get { return Microphone.devices; } }
    /// <summary>
    /// True if microphone is active, false if inactive.
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
    /// True if microphone is disabled, false if enabled.
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
    /// This is set to true when the microhphone fails, the update will continue to try to start
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
    /// Button handler for toggling the active state.
    /// </summary>
    public void OnToggleActive()
    {
      Active = !Active;
    }
    #endregion

    #region Public Functions
    /// <summary>
    /// Activates the microphone.
    /// </summary>
    public void ActivateMicrophone()
    {
      Active = true;
    }

    /// <summary>
    /// Deactivates the microphone.
    /// </summary>
    public void DeactivateMicrophone()
    {
      Active = false;
    }

    #endregion

    #region Widget interface
    /// <exclude />
    protected override string GetName()
    {
      return "Microphone";
    }
    #endregion

    #region Event Handlers
    /// <exclude />
    protected override void Start()
    {
      base.Start();
      if (activateOnStart)
        Active = true;
    }
    private void Update()
    {
      if (Failure && Active && !Disable
          && (DateTime.Now - lastFailure).TotalMilliseconds > RETRY_INTERVAL)
      {
        // try to restart the recording..
        StartRecording();
      }
    }
    private void OnDisableInput(Data data)
    {
      Disable = ((DisableMicData)data).Boolean;
    }

    void OnDestroy()
    {
      Active = false;
      Disable = true;
    }

    #endregion

    #region Recording Functions
    private void StartRecording()
    {
      if (recordingRoutine == 0)
      {
        UnityObjectUtil.StartDestroyQueue();

        recordingRoutine = Runnable.Run(RecordingHandler());
        activateOutput.SendData(new BooleanData(true));

        if (statusText != null)
          statusText.text = "RECORDING";
      }
    }

    private void StopRecording()
    {
      if (recordingRoutine != 0)
      {
        Microphone.End(microphoneID);
        Runnable.Stop(recordingRoutine);
        recordingRoutine = 0;

        activateOutput.SendData(new BooleanData(false));
        if (statusText != null)
          statusText.text = "STOPPED";

        if (playbackRecording && playback.Count > 0)
        {
          AudioClip combined = AudioClipUtil.Combine(playback.ToArray());
          if (combined != null)
          {
            AudioSource source = GetComponentInChildren<AudioSource>();
            if (source != null)
            {
              // destroy any previous audio clip..
              if (source.clip != null)
                UnityObjectUtil.DestroyUnityObject(source.clip);

              source.spatialBlend = 0.0f;     // 2D sound
              source.loop = false;            // do not loop
              source.clip = combined;         // clip
              source.Play();
            }
            else
              Log.Warning("MicrophoneWidget", "Failed to find AudioSource.");
          }

          foreach (var clip in playback)
            UnityObjectUtil.DestroyUnityObject(clip);
          playback.Clear();
        }
      }
    }

    private IEnumerator RecordingHandler()
    {
      Failure = false;
#if UNITY_WEBPLAYER
            yield return Application.RequestUserAuthorization( UserAuthorization.Microphone );
#endif
      recording = Microphone.Start(microphoneID, true, recordingBufferSize, recordingHZ);
      yield return null;      // let recordingRoutine get set..

      if (recording == null)
      {
        Failure = true;
        StopRecording();
        yield break;
      }

      bool bFirstBlock = true;
      int midPoint = recording.samples / 2;

      bool bOutputLevelData = levelOutput.IsConnected;
      bool bOutputAudio = audioOutput.IsConnected || playbackRecording;

      int lastReadPos = 0;
      float[] samples = null;

      while (recordingRoutine != 0 && recording != null)
      {
        int writePos = Microphone.GetPosition(microphoneID);
        if (writePos > recording.samples || !Microphone.IsRecording(microphoneID))
        {
          Log.Error("MicrophoneWidget", "Microphone disconnected.");

          Failure = true;
          StopRecording();
          yield break;
        }

        if (bOutputAudio)
        {
          if ((bFirstBlock && writePos >= midPoint)
              || (!bFirstBlock && writePos < midPoint))
          {
            // front block is recorded, make a RecordClip and pass it onto our callback.
            samples = new float[midPoint];
            recording.GetData(samples, bFirstBlock ? 0 : midPoint);

            AudioData record = new AudioData();
            record.MaxLevel = Mathf.Max(samples);
            record.Clip = AudioClip.Create("Recording", midPoint, recording.channels, recordingHZ, false);
            record.Clip.SetData(samples, 0);

            if (playbackRecording)
              playback.Add(record.Clip);
            if (audioOutput.IsConnected && !audioOutput.SendData(record))
              StopRecording();        // automatically stop recording if the callback goes away.

            bFirstBlock = !bFirstBlock;
          }
          else
          {
            // calculate the number of samples remaining until we ready for a block of audio, 
            // and wait that amount of time it will take to record.
            int remaining = bFirstBlock ? (midPoint - writePos) : (recording.samples - writePos);
            float timeRemaining = (float)remaining / (float)recordingHZ;
            if (bOutputLevelData && timeRemaining > levelOutputInterval)
              timeRemaining = levelOutputInterval;
            yield return new WaitForSeconds(timeRemaining);
          }
        }
        else
        {
          yield return new WaitForSeconds(levelOutputInterval);
        }

        if (recording != null && bOutputLevelData)
        {
          float fLevel = 0.0f;
          if (writePos < lastReadPos)
          {
            // write has wrapped, grab the last bit from the buffer..
            samples = new float[recording.samples - lastReadPos];
            recording.GetData(samples, lastReadPos);
            fLevel = Mathf.Max(fLevel, Mathf.Max(samples));

            lastReadPos = 0;
          }

          if (lastReadPos < writePos)
          {
            samples = new float[writePos - lastReadPos];
            recording.GetData(samples, lastReadPos);
            fLevel = Mathf.Max(fLevel, Mathf.Max(samples));

            lastReadPos = writePos;
          }

          levelOutput.SendData(new LevelData(fLevel * levelOutputModifier, levelOutputModifier));
        }
      }

      yield break;
    }
    #endregion
  }
}
