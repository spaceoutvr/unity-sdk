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
using IBM.Watson.DeveloperCloud.Services.SpeechToText.v1;
using UnityEngine;
using UnityEngine.UI;

#pragma warning disable 414

namespace IBM.Watson.DeveloperCloud.Widgets
{
  /// <summary>
  /// Simple class for displaying the SpeechToText result data in the UI.
  /// </summary>
  public class SpeechDisplayWidget : Widget
  {
    #region Inputs
    [SerializeField]
    private Input speechInput = new Input("SpeechInput", typeof(SpeechToTextData), "OnSpeechInput");
    #endregion

    #region Widget interface
    /// <exclude />
    protected override string GetName()
    {
      return "SpeechDisplay";
    }
    #endregion

    #region Private Data

    [SerializeField]
    private bool continuousText = false;
    [SerializeField]
    private Text output = null;
    [SerializeField]
    private InputField outputAsInputField = null;
    [SerializeField]
    private Text outputStatus = null;
    [SerializeField]
    private float minConfidenceToShow = 0.5f;

    private string previousOutputTextWithStatus = "";
    private string previousOutputText = "";
    private float thresholdTimeFromLastInput = 3.0f; //3 secs as threshold time. After 3 secs from last OnSpeechInput, we are considering input as new input
    private float timeAtLastInterim = 0.0f;
    #endregion

    #region Event Handlers
    private void OnSpeechInput(Data data)
    {
      if (output != null || outputAsInputField != null)
      {
        SpeechRecognitionEvent result = ((SpeechToTextData)data).Results;
        if (result != null && result.results.Length > 0)
        {
          string outputTextWithStatus = "";
          string outputText = "";

          if (Time.time - timeAtLastInterim > thresholdTimeFromLastInput)
          {
            if (output != null)
              previousOutputTextWithStatus = output.text;
            if (outputAsInputField != null)
              previousOutputText = outputAsInputField.text;
          }

          if (output != null && continuousText)
            outputTextWithStatus = previousOutputTextWithStatus;

          if (outputAsInputField != null && continuousText)
            outputText = previousOutputText;

          foreach (var res in result.results)
          {
            foreach (var alt in res.alternatives)
            {
              string text = alt.transcript;
              if (output != null)
              {
                output.text = string.Concat(outputTextWithStatus, string.Format("{0} ({1}, {2:0.00})\n", text, res.final ? "Final" : "Interim", alt.confidence));
              }

              if (outputAsInputField != null)
              {
                if (!res.final || alt.confidence > minConfidenceToShow)
                {
                  outputAsInputField.text = string.Concat(outputText, " ", text);

                  if (outputStatus != null)
                  {
                    outputStatus.text = string.Format("{0}, {1:0.00}", res.final ? "Final" : "Interim", alt.confidence);
                  }
                }
              }

              if (!res.final)
                timeAtLastInterim = Time.time;

            }
          }
        }
      }
    }
    #endregion
  }
}
