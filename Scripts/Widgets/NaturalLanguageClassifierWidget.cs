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
using IBM.Watson.DeveloperCloud.Services.NaturalLanguageClassifier.v1;
using IBM.Watson.DeveloperCloud.Logging;
using IBM.Watson.DeveloperCloud.Utilities;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

#pragma warning disable 414

namespace IBM.Watson.DeveloperCloud.Widgets
{
  /// <summary>
  /// Natural Language Classifier Widget.
  /// </summary>
  public class NaturalLanguageClassifierWidget : Widget
  {
    #region Inputs
    [SerializeField]
    private Input recognizeInput = new Input("Recognize", typeof(SpeechToTextData), "OnRecognize");
    #endregion

    #region Outputs
    [SerializeField]
    private Output classifyOutput = new Output(typeof(ClassifyResultData), true);
    #endregion

    #region Private Data
    private NaturalLanguageClassifier naturalLanguageClassifier = new NaturalLanguageClassifier();
    private Classifier selected = null;

    [SerializeField]
    private string classifierName = string.Empty;
    [SerializeField]
    private string classifierId = string.Empty;
    [SerializeField, Tooltip("What is the minimum word confidence needed to send onto the Natural Language Classifier?")]
    private float minWordConfidence = 0f;
    private float minWordConfidenceDelta = 0.0f;
    [SerializeField, Tooltip("Recognized speech below this confidence is just ignored.")]
    private float ignoreWordConfidence = 0f;
    private float ignoreWordConfidenceDelta = 0.0f;
    [SerializeField, Tooltip("What is the minimum confidence for a classification event to be fired.")]
    private float minClassEventConfidence = 0f;
    private float minClassEventConfidenceDelta = 0.0f;
    [SerializeField]
    private string language = "en";

    [Serializable]
    private class ClassEventMapping
    {
      public string _class = null;
      public string _event = "";
    };
    [SerializeField]
    private List<ClassEventMapping> classEventList = new List<ClassEventMapping>();
    private Dictionary<string, string> classEventMap = new Dictionary<string, string>();
    //		private Dictionary<string, Constants.Event> classEventMap = new Dictionary<string, Constants.Event>();

    [SerializeField]
    private Text topClassText = null;
    #endregion

    #region Public Properties
    /// <summary>
    /// Returns the Natural Language Classifier service object.
    /// </summary>
    public NaturalLanguageClassifier NaturalLanguageClassifier { get { return naturalLanguageClassifier; } }

    /// <summary>
    /// Gets or sets the value of ignore word confidence.
    /// </summary>
    /// <value>The ignore word confidence.</value>
    public float IgnoreWordConfidence
    {
      get
      {
        return Mathf.Clamp01(ignoreWordConfidence + ignoreWordConfidenceDelta);
      }
      set
      {
        ignoreWordConfidenceDelta = value + ignoreWordConfidence;
        if (IgnoreWordConfidence > MinWordConfidence)
          MinWordConfidence = IgnoreWordConfidence;
        PlayerPrefs.SetFloat("ignoreWordConfidenceDelta", ignoreWordConfidenceDelta);
        PlayerPrefs.Save();
      }
    }
    /// <summary>
    /// Gets or sets the value of ignore word confidence delta.
    /// </summary>
    /// <value>The ignore word confidence delta.</value>
    public float IgnoreWordConfidenceDelta
    {
      get { return ignoreWordConfidenceDelta; }
      set
      {
        ignoreWordConfidenceDelta = value;
        PlayerPrefs.SetFloat("ignoreWordConfidenceDelta", ignoreWordConfidenceDelta);
        PlayerPrefs.Save();
      }
    }

    /// <summary>
    /// Gets or sets the minimum value of word confidence.
    /// </summary>
    /// <value>The minimum word confidence.</value>
    public float MinWordConfidence
    {
      get
      {
        return Mathf.Clamp01(minWordConfidence + minWordConfidenceDelta);
        //                return Mathf.Clamp01(minWordConfidenceDelta);
      }
      set
      {
        minWordConfidenceDelta = value + minWordConfidence;
        if (MinWordConfidence < IgnoreWordConfidence)
          IgnoreWordConfidence = MinWordConfidence;
        PlayerPrefs.SetFloat("minWordConfidenceDelta", minWordConfidenceDelta);
        PlayerPrefs.Save();
      }
    }

    /// <summary>
    /// Gets or sets the minimum value of word confidence delta.
    /// </summary>
    /// <value>The minimum word confidence delta.</value>
    public float MinWordConfidenceDelta
    {
      get { return minWordConfidenceDelta; }
      set
      {
        minWordConfidenceDelta = value;
        PlayerPrefs.SetFloat("minWordConfidenceDelta", minWordConfidenceDelta);
        PlayerPrefs.Save();
      }
    }

    /// <summary>
    /// Gets or sets the minimum value of class event confidence.
    /// </summary>
    /// <value>The minimum class event confidence.</value>
    public float MinClassEventConfidence
    {
      get
      {
        return Mathf.Clamp01(minClassEventConfidence + minClassEventConfidenceDelta);
      }
      set
      {
        minClassEventConfidenceDelta = value + minClassEventConfidence;
        PlayerPrefs.SetFloat("minClassEventConfidenceDelta", minClassEventConfidenceDelta);
        PlayerPrefs.Save();
      }
    }

    /// <summary>
    /// Gets or sets the minimum value of class event confidence delta.
    /// </summary>
    /// <value>The minimum class event confidence delta.</value>
    public float MinClassEventConfidenceDelta
    {
      get { return minClassEventConfidenceDelta; }
      set
      {
        minClassEventConfidenceDelta = value;
        PlayerPrefs.SetFloat("minClassEventConfidenceDelta", minClassEventConfidenceDelta);
        PlayerPrefs.Save();
      }
    }
    #endregion

    #region Widget interface
    /// <exclude />
    protected override string GetName()
    {
      return "Natural Language Classifier";
    }
    #endregion

    #region Event Handlers
    /// <exclude />
    protected override void Start()
    {
      base.Start();

      ignoreWordConfidenceDelta = PlayerPrefs.GetFloat("ignoreWordConfidenceDelta", 0.0f);
      minWordConfidenceDelta = PlayerPrefs.GetFloat("minWordConfidenceDelta", 0.0f);
      minClassEventConfidenceDelta = PlayerPrefs.GetFloat("minClassEventConfidenceDelta", 0.0f);

      // resolve configuration variables
      classifierName = Config.Instance.ResolveVariables(classifierName);
      classifierId = Config.Instance.ResolveVariables(classifierId);

      if (string.IsNullOrEmpty(classifierId))
      {
        Log.Status("NaturalLanguageClassifierWidget", "Auto selecting a classifier.");
        if (!naturalLanguageClassifier.GetClassifiers(OnGetClassifiers))
          Log.Error("NaturalLanguageClassifierWidget", "Failed to request all classifiers.");
      }
      else
      {
        if (!naturalLanguageClassifier.GetClassifier(classifierId, OnGetClassifier))
          Log.Equals("NaturalLanguageClassifierWidget", "Failed to request classifier.");
      }
    }

    private void OnEnable()
    {
      EventManager.Instance.RegisterEventReceiver("OnDebugCommand", OnDebugCommand);
    }
    private void OnDisable()
    {
      EventManager.Instance.UnregisterEventReceiver("OnDebugCommand", OnDebugCommand);
    }

    private void OnGetClassifiers(Classifiers classifiers)
    {
      if (classifiers != null)
      {
        bool bFound = false;
        foreach (var classifier in classifiers.classifiers)
        {
          if (!string.IsNullOrEmpty(classifierName) && !classifier.name.ToLower().StartsWith(classifierName.ToLower()))
            continue;
          if (classifier.language != language)
            continue;

          naturalLanguageClassifier.GetClassifier(classifier.classifier_id, OnGetClassifier);
          bFound = true;
        }

        if (!bFound)
          Log.Error("NaturalLanguageClassifierWidget", "No classifiers found that match {0}", classifierName);
      }
    }

    private void OnGetClassifier(Classifier classifier)
    {
      if (classifier != null && classifier.status == "Available")
      {
        if (selected == null || selected.created.CompareTo(classifier.created) < 0)
        {
          selected = classifier;
          classifierId = selected.classifier_id;

          Log.Status("NaturalLanguageClassifierWidget", "Selected classifier {0}, Created: {1}, Name: {2}",
              selected.classifier_id, selected.created, selected.name);
        }
      }
    }

    private void OnRecognize(Data data)
    {
      SpeechRecognitionEvent result = ((SpeechToTextData)data).Results;
      if (result.HasFinalResult())
      {
        string text = result.results[0].alternatives[0].transcript;
        double textConfidence = result.results[0].alternatives[0].confidence;

        Log.Debug("NaturalLanguageClassifierWidget", "OnRecognize: {0} ({1:0.00})", text, textConfidence);
        EventManager.Instance.SendEvent("OnDebugMessage", string.Format("{0} ({1:0.00})", text, textConfidence));

        if (textConfidence > MinWordConfidence)
        {
          if (!string.IsNullOrEmpty(classifierId))
          {
            if (!naturalLanguageClassifier.Classify(classifierId, text, OnClassified))
              Log.Error("NaturalLanguageClassifierWidget", "Failed to send {0} to Natural Language Classifier.", text);
          }
          else
            Log.Equals("NaturalLanguageClassifierWidget", "No valid classifier set.");
        }
        else
        {
          Log.Debug("NaturalLanguagClassifierWidget", "Text confidence {0} < {1} (Min word confidence)", textConfidence, MinWordConfidence);
          if (textConfidence > IgnoreWordConfidence)
          {
            Log.Debug("NaturalLanguageClassifierWidget", "Text confidence {0} > {1} (Ignore word confidence)", textConfidence, IgnoreWordConfidence);
            EventManager.Instance.SendEvent("OnClassifyFailure", result);
          }
        }
      }
    }

    private void OnClassified(ClassifyResult result)
    {
      EventManager.Instance.SendEvent("OnClassifyResult", result);

      if (classifyOutput.IsConnected)
        classifyOutput.SendData(new ClassifyResultData(result));

      if (result != null)
      {
        Log.Debug("NaturalLanguageClassifierWidget", "OnClassified: {0} ({1:0.00})", result.top_class, result.topConfidence);

        if (topClassText != null)
          topClassText.text = result.top_class;

        if (!string.IsNullOrEmpty(result.top_class))
        {
          if (result.topConfidence >= MinClassEventConfidence)
          {
            if (classEventList.Count > 0 && classEventMap.Count == 0)
            {
              // initialize the map
              foreach (var ev in classEventList)
                classEventMap[ev._class] = ev._event;
            }

            string sendEvent;
            //						Constants.Event sendEvent;
            if (!classEventMap.TryGetValue(result.top_class, out sendEvent))
            {
              Log.Warning("NaturalLanguageClassifierWidget", "No class mapping found for {0}", result.top_class);
              EventManager.Instance.SendEvent(result.top_class, result);
            }
            else
              EventManager.Instance.SendEvent(sendEvent, result);
          }
          else
          {
            if (result.topConfidence > IgnoreWordConfidence)
              EventManager.Instance.SendEvent("OnClassifyFailure", result);
          }
        }
      }
    }

    private void OnDebugCommand(object[] args)
    {
      string text = args != null && args.Length > 0 ? args[0] as string : string.Empty;
      if (!string.IsNullOrEmpty(text) && !string.IsNullOrEmpty(classifierId))
      {
        if (!naturalLanguageClassifier.Classify(classifierId, text, OnClassified))
          Log.Error("NaturalLanguageClassifierWidget", "Failed to send {0} to Natural Language Classifier.", (string)args[0]);
      }
    }
    #endregion
  }
}
