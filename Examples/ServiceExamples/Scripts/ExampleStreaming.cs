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
using IBM.Watson.DeveloperCloud.Logging;
using IBM.Watson.DeveloperCloud.Services.SpeechToText.v1;
using IBM.Watson.DeveloperCloud.Utilities;
using IBM.Watson.DeveloperCloud.DataTypes;

public class ExampleStreaming : MonoBehaviour
{
  private int recordingRoutine = 0;
  private string microphoneID = null;
  private AudioClip recording = null;
  private int recordingBufferSize = 2;
  private int recordingHZ = 22050;

  private SpeechToText speechToText = new SpeechToText();

  void Start()
  {
    LogSystem.InstallDefaultReactors();
    Log.Debug("ExampleStreaming", "Start();");

    Active = true;

    StartRecording();
  }

  public bool Active
  {
    get { return speechToText.IsListening; }
    set
    {
      if (value && !speechToText.IsListening)
      {
        speechToText.DetectSilence = true;
        speechToText.EnableWordConfidence = false;
        speechToText.EnableTimestamps = false;
        speechToText.SilenceThreshold = 0.03f;
        speechToText.MaxAlternatives = 1;
        speechToText.EnableContinousRecognition = true;
        speechToText.EnableInterimResults = true;
        speechToText.OnError = OnError;
        speechToText.StartListening(OnRecognize);
      }
      else if (!value && speechToText.IsListening)
      {
        speechToText.StopListening();
      }
    }
  }

  private void StartRecording()
  {
    if (recordingRoutine == 0)
    {
      UnityObjectUtil.StartDestroyQueue();
      recordingRoutine = Runnable.Run(RecordingHandler());
    }
  }

  private void StopRecording()
  {
    if (recordingRoutine != 0)
    {
      Microphone.End(microphoneID);
      Runnable.Stop(recordingRoutine);
      recordingRoutine = 0;
    }
  }

  private void OnError(string error)
  {
    Active = false;

    Log.Debug("ExampleStreaming", "Error! {0}", error);
  }

  private IEnumerator RecordingHandler()
  {
    Log.Debug("ExampleStreaming", "devices: {0}", Microphone.devices);
    recording = Microphone.Start(microphoneID, true, recordingBufferSize, recordingHZ);
    yield return null;      // let recordingRoutine get set..

    if (recording == null)
    {
      StopRecording();
      yield break;
    }

    bool bFirstBlock = true;
    int midPoint = recording.samples / 2;
    float[] samples = null;

    while (recordingRoutine != 0 && recording != null)
    {
      int writePos = Microphone.GetPosition(microphoneID);
      if (writePos > recording.samples || !Microphone.IsRecording(microphoneID))
      {
        Log.Error("MicrophoneWidget", "Microphone disconnected.");

        StopRecording();
        yield break;
      }

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

        speechToText.OnListen(record);

        bFirstBlock = !bFirstBlock;
      }
      else
      {
        // calculate the number of samples remaining until we ready for a block of audio, 
        // and wait that amount of time it will take to record.
        int remaining = bFirstBlock ? (midPoint - writePos) : (recording.samples - writePos);
        float timeRemaining = (float)remaining / (float)recordingHZ;

        yield return new WaitForSeconds(timeRemaining);
      }

    }

    yield break;
  }

  private void OnRecognize(SpeechRecognitionEvent result)
  {
    if (result != null && result.results.Length > 0)
    {
      foreach (var res in result.results)
      {
        foreach (var alt in res.alternatives)
        {
          string text = alt.transcript;
          Log.Debug("ExampleStreaming", string.Format("{0} ({1}, {2:0.00})\n", text, res.final ? "Final" : "Interim", alt.confidence));
        }
      }
    }
  }
}