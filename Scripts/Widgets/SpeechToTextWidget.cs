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

//#define ENABLE_DEBUGGING

using IBM.Watson.DeveloperCloud.DataTypes;
using IBM.Watson.DeveloperCloud.Services.SpeechToText.v1;
using IBM.Watson.DeveloperCloud.Logging;
using UnityEngine;
using UnityEngine.UI;
using IBM.Watson.DeveloperCloud.Utilities;

#pragma warning disable 414

namespace IBM.Watson.DeveloperCloud.Widgets
{
  /// <summary>
  /// SpeechToText Widget that wraps the SpeechToText service.
  /// </summary>
  public class SpeechToTextWidget : Widget
  {
    #region Inputs
    [SerializeField]
    private Input audioInput = new Input("Audio", typeof(AudioData), "OnAudio");
    [SerializeField]
    private Input languageInput = new Input("Language", typeof(LanguageData), "OnLanguage");
    #endregion

    #region Outputs
    [SerializeField]
    private Output resultOutput = new Output(typeof(SpeechToTextData), true);
    #endregion

    #region Private Data
    private SpeechToText speechToText = new SpeechToText();
    [SerializeField]
    private Text statusText = null;
    [SerializeField]
    private bool detectSilence = true;
    [SerializeField]
    private float silenceThreshold = 0.03f;
    [SerializeField]
    private bool wordConfidence = false;
    [SerializeField]
    private bool timeStamps = false;
    [SerializeField]
    private int maxAlternatives = 1;
    [SerializeField]
    private bool enableContinous = false;
    [SerializeField]
    private bool enableInterimResults = false;
    [SerializeField]
    private Text transcript = null;
    [SerializeField, Tooltip("Language ID to use in the speech recognition model.")]
    private string language = "en-US";
    #endregion

    #region Public Properties
    /// <summary>
    /// This property starts or stop's this widget listening for speech.
    /// </summary>
    public bool Active
    {
      get { return speechToText.IsListening; }
      set
      {
        if (value && !speechToText.IsListening)
        {
          speechToText.DetectSilence = detectSilence;
          speechToText.EnableWordConfidence = wordConfidence;
          speechToText.EnableTimestamps = timeStamps;
          speechToText.SilenceThreshold = silenceThreshold;
          speechToText.MaxAlternatives = maxAlternatives;
          speechToText.EnableContinousRecognition = enableContinous;
          speechToText.EnableInterimResults = enableInterimResults;
          speechToText.OnError = OnError;
          speechToText.StartListening(OnRecognize);
          if (statusText != null)
            statusText.text = "LISTENING";
        }
        else if (!value && speechToText.IsListening)
        {
          speechToText.StopListening();
          if (statusText != null)
            statusText.text = "READY";
        }
      }
    }
    #endregion

    #region Widget Interface
    /// <exclude />
    protected override string GetName()
    {
      return "SpeechToText";
    }
    #endregion

    #region Event handlers
    /// <summary>
    /// Button handler to toggle the active state of this widget.
    /// </summary>
    public void OnListenButton()
    {
      Active = !Active;
    }

    /// <exclude />
    protected override void Start()
    {
      base.Start();

      if (statusText != null)
        statusText.text = "READY";
      if (!speechToText.GetModels(OnGetModels))
        Log.Error("SpeechToTextWidget", "Failed to request models.");
    }

    private void OnDisable()
    {
      if (Active)
        Active = false;
    }

    private void OnError(string error)
    {
      Active = false;
      if (statusText != null)
        statusText.text = "ERROR: " + error;
    }

    private void OnAudio(Data data)
    {
      if (!Active)
        Active = true;

      speechToText.OnListen((AudioData)data);
    }

    private void OnLanguage(Data data)
    {
      LanguageData languageData = data as LanguageData;
      if (languageData == null)
        throw new WatsonException("Unexpected data type");

      if (!string.IsNullOrEmpty(languageData.Language))
      {
        language = languageData.Language;

        if (!speechToText.GetModels(OnGetModels))
          Log.Error("SpeechToTextWidget", "Failed to rquest models.");
      }
    }

    private void OnGetModels(Model[] models)
    {
      if (models != null)
      {
        Model bestModel = null;
        foreach (var model in models)
        {
          if (model.language.StartsWith(language)
              && (bestModel == null || model.rate > bestModel.rate))
          {
            bestModel = model;
          }
        }

        if (bestModel != null)
        {
          Log.Status("SpeechToTextWidget", "Selecting Recognize Model: {0} ", bestModel.name);
          speechToText.RecognizeModel = bestModel.name;
        }
      }
    }

    private void OnRecognize(SpeechRecognitionEvent result)
    {
      resultOutput.SendData(new SpeechToTextData(result));

      if (result != null && result.results.Length > 0)
      {
        if (transcript != null)
          transcript.text = "";

        foreach (var res in result.results)
        {
          foreach (var alt in res.alternatives)
          {
            string text = alt.transcript;

            if (transcript != null)
              transcript.text += string.Format("{0} ({1}, {2:0.00})\n",
                  text, res.final ? "Final" : "Interim", alt.confidence);
          }
        }
      }
    }
    #endregion
  }
}
