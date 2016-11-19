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
using UnityEngine.UI;
using IBM.Watson.DeveloperCloud.DataTypes;
using IBM.Watson.DeveloperCloud.Logging;
using IBM.Watson.DeveloperCloud.Services.LanguageTranslation.v1;
using IBM.Watson.DeveloperCloud.Services.TextToSpeech.v1;
using System;
using System.Collections.Generic;
using IBM.Watson.DeveloperCloud.Utilities;

#pragma warning disable 414

namespace IBM.Watson.DeveloperCloud.Widgets
{

  /// <summary>
  /// Translation widget to handle translation service calls
  /// </summary>
  public class LanguageTranslatorWidget : Widget
  {
    #region Inputs
    [SerializeField]
    private Input speechInput = new Input("SpeechInput", typeof(SpeechToTextData), "OnSpeechInput");
    #endregion

    #region Outputs
    [SerializeField]
    private Output recognizeLanguageOutput = new Output(typeof(LanguageData));
    [SerializeField]
    private Output speechOutput = new Output(typeof(TextToSpeechData));
    [SerializeField]
    private Output voiceOutput = new Output(typeof(VoiceData));
    #endregion

    #region Private Data
    private LanguageTranslation translate = new LanguageTranslation();

    [SerializeField, Tooltip("Source language, if empty language will be auto-detected.")]
    private string sourceLanguage = string.Empty;
    [SerializeField, Tooltip("Target language to translate into.")]
    private string targetLanguage = "es";
    [SerializeField, Tooltip("Input field for inputting speech")]
    private InputField input = null;
    [SerializeField, Tooltip("Output text for showing translated text")]
    private Text output = null;
    [SerializeField]
    private Dropdown dropDownSourceLanguage = null;
    [SerializeField]
    private Dropdown dropDownTargetLanguage = null;
    [SerializeField]
    private string defaultDomainToUse = "conversational";
    [SerializeField]
    private string detectLanguageName = "Detect Language";
    private string detectLanguageID = "";

    // Mapping from language ID to it's Name
    private Dictionary<string, string> languageIDToName = new Dictionary<string, string>();
    // Mapping from language name to ID
    private Dictionary<string, string> languageNameToID = new Dictionary<string, string>();
    // Mapping from language to a list of languages that can be translated into..
    private Dictionary<string, List<string>> languageToTranslate = new Dictionary<string, List<string>>();
    // array of availablel languages;
    private string[] languages = null;
    // Last string of input text    
    private string translateText = string.Empty;
    #endregion

    #region Widget interface
    /// <exclude />
    protected override string GetName()
    {
      return "Translate";
    }
    #endregion

    #region Public Members
    /// <summary>
    /// Set or get the source language ID. If set to null or empty, then the language will be auto-detected.
    /// </summary>
    public string SourceLanguage
    {
      set
      {
        if (sourceLanguage != value)
        {
          sourceLanguage = value;

          if (recognizeLanguageOutput.IsConnected && !string.IsNullOrEmpty(sourceLanguage))
            recognizeLanguageOutput.SendData(new LanguageData(sourceLanguage));
          ResetSourceLanguageDropDown();
          ResetTargetLanguageDropDown();
        }
      }
      get { return sourceLanguage; }
    }

    /// <summary>
    /// Set or get the target language ID.
    /// </summary>
    public string TargetLanguage
    {
      set
      {
        if (TargetLanguage != value)
        {
          targetLanguage = value;
          ResetVoiceForTargetLanguage();
        }
      }
      get { return targetLanguage; }
    }
    #endregion

    #region Event Handlers
    private void OnEnable()
    {
      Log.Status("TranslatorWidget", "OnEnable");
      //UnityEngine.Debug.LogWarning("TranslatorWidget - OnEnable");
      translate.GetLanguages(OnGetLanguages);
    }

    /// <exclude />
    protected override void Awake()
    {
      base.Awake();

      if (input != null)
        input.onEndEdit.AddListener(delegate { OnInputEnd(); });
      if (dropDownSourceLanguage != null)
        dropDownSourceLanguage.onValueChanged.AddListener(delegate { DropDownSourceValueChanged(); });
      if (dropDownTargetLanguage != null)
        dropDownTargetLanguage.onValueChanged.AddListener(delegate { DropDownTargetValueChanged(); });
    }

    /// <exclude />
    protected override void Start()
    {
      base.Start();

      // resolve variables
      sourceLanguage = Config.Instance.ResolveVariables(sourceLanguage);
      targetLanguage = Config.Instance.ResolveVariables(targetLanguage);

      if (recognizeLanguageOutput.IsConnected && !string.IsNullOrEmpty(sourceLanguage))
        recognizeLanguageOutput.SendData(new LanguageData(sourceLanguage));
    }

    private void OnInputEnd()
    {
      if (input != null)
      {
        if (!string.IsNullOrEmpty(TargetLanguage))
          Translate(input.text);
        else
          Log.Error("TranslatorWidget", "OnTranslation - Target Language should be set!");
      }
    }

    private void OnSpeechInput(Data data)
    {
      SpeechToTextData speech = data as SpeechToTextData;
      if (speech != null && speech.Results.HasFinalResult())
        Translate(speech.Results.results[0].alternatives[0].transcript);
    }

    private void OnGetLanguages(Languages languages)
    {
      if (languages != null && languages.languages.Length > 0)
      {
        Log.Status("TranslatorWidget", "OnGetLanguagesAndGetModelsAfter as {0}", languages.languages.Length);
        languageIDToName.Clear();

        foreach (var lang in languages.languages)
        {
          languageIDToName[lang.language] = lang.name;
          languageNameToID[lang.name] = lang.language;
        }

        languageIDToName[detectLanguageID] = detectLanguageName;
        languageNameToID[detectLanguageName] = detectLanguageID;
        translate.GetModels(OnGetModels); //To fill dropdown with models to use in Translation
      }
      else
      {
        Log.Error("TranslatorWidget", "OnGetLanguages - There is no language to translate. Check the connections and service of Translation Service.");
      }
    }

    private void OnGetModels(TranslationModels models)
    {
      Log.Status("TranslatorWidget", "OnGetModels, Count: {0}", models.models.Length);
      if (models != null && models.models.Length > 0)
      {
        languageToTranslate.Clear();

        List<string> listLanguages = new List<string>();    //From - To language list to use in translation

        //Adding initial language as detected!
        listLanguages.Add(detectLanguageID);
        languageToTranslate.Add(detectLanguageID, new List<string>());

        foreach (var model in models.models)
        {
          if (string.Equals(model.domain, defaultDomainToUse))
          {
            if (languageToTranslate.ContainsKey(model.source))
            {
              if (!languageToTranslate[model.source].Contains(model.target))
                languageToTranslate[model.source].Add(model.target);
            }
            else
            {
              languageToTranslate.Add(model.source, new List<string>());
              languageToTranslate[model.source].Add(model.target);
            }

            if (!listLanguages.Contains(model.source))
              listLanguages.Add(model.source);
            if (!listLanguages.Contains(model.target))
              listLanguages.Add(model.target);

            if (!languageToTranslate[detectLanguageID].Contains(model.target))
              languageToTranslate[detectLanguageID].Add(model.target);
          }
        }

        languages = listLanguages.ToArray();
        ResetSourceLanguageDropDown();
        ResetVoiceForTargetLanguage();
      }
    }
    #endregion

    #region Private Functions
    private void Translate(string text)
    {
      if (!string.IsNullOrEmpty(text))
      {
        translateText = text;

        if (input != null)
          input.text = text;

        new TranslateRequest(this, text);
      }
    }

    private class TranslateRequest
    {
      private LanguageTranslatorWidget widget;
      private string text;

      public TranslateRequest(LanguageTranslatorWidget languageTranslator, string query)
      {
        widget = languageTranslator;
        text = query;

        if (string.IsNullOrEmpty(widget.SourceLanguage))
          widget.translate.Identify(text, OnIdentified);
        else
          widget.translate.GetTranslation(text, widget.SourceLanguage, widget.TargetLanguage, OnGetTranslation);
      }

      private void OnIdentified(string language)
      {
        if (!string.IsNullOrEmpty(language))
        {
          widget.SourceLanguage = language;
          widget.translate.GetTranslation(text, language, widget.TargetLanguage, OnGetTranslation);
        }
        else
          Log.Error("TranslateWidget", "Failed to identify language: {0}", text);
      }

      private void OnGetTranslation(Translations translations)
      {
        if (translations != null && translations.translations.Length > 0)
          widget.SetOutput(translations.translations[0].translation);
      }
    };

    private void SetOutput(string text)
    {
      Log.Debug("TranslateWidget", "SetOutput(): {0}", text);

      if (output != null)
        output.text = text;

      if (speechOutput.IsConnected)
        speechOutput.SendData(new TextToSpeechData(text));
    }

    private void ResetSourceLanguageDropDown()
    {
      if (dropDownSourceLanguage != null)
      {
        dropDownSourceLanguage.options.Clear();

        int selected = 0;
        foreach (string itemLanguage in languages)
        {
          if (languageIDToName.ContainsKey(itemLanguage))
            dropDownSourceLanguage.options.Add(new Dropdown.OptionData(languageIDToName[itemLanguage]));

          if (String.Equals(SourceLanguage, itemLanguage))
            selected = dropDownSourceLanguage.options.Count - 1;
        }

        dropDownSourceLanguage.value = selected;
      }
    }

    private void DropDownSourceValueChanged()
    {
      if (dropDownSourceLanguage != null && dropDownSourceLanguage.options.Count > 0)
      {
        string selected = dropDownSourceLanguage.options[dropDownSourceLanguage.value].text;
        if (languageNameToID.ContainsKey(selected))
        {
          selected = languageNameToID[selected];
          if (selected != SourceLanguage)
          {
            SourceLanguage = selected;
            Translate(translateText);
          }
        }
      }
    }

    private void DropDownTargetValueChanged()
    {
      if (dropDownTargetLanguage != null && dropDownTargetLanguage.options.Count > 0)
      {
        string selected = dropDownTargetLanguage.options[dropDownTargetLanguage.value].text;
        if (languageNameToID.ContainsKey(selected))
        {
          string target = languageNameToID[selected];
          if (target != TargetLanguage)
          {
            TargetLanguage = target;
            Translate(translateText);
          }
        }
      }
    }

    private void ResetTargetLanguageDropDown()
    {
      if (dropDownTargetLanguage != null)
      {
        if (!string.IsNullOrEmpty(SourceLanguage) && languageToTranslate.ContainsKey(SourceLanguage))
        {
          //Add target language corresponding source language
          dropDownTargetLanguage.options.Clear();
          int selected = 0;

          foreach (string itemLanguage in languageToTranslate[SourceLanguage])
          {
            if (string.Equals(itemLanguage, SourceLanguage))
              continue;

            dropDownTargetLanguage.options.Add(new Dropdown.OptionData(languageIDToName[itemLanguage]));

            if (String.Equals(TargetLanguage, itemLanguage))
              selected = dropDownTargetLanguage.options.Count - 1;
          }

          dropDownTargetLanguage.captionText.text = dropDownTargetLanguage.options[selected].text;
          dropDownTargetLanguage.value = selected;
        }
      }
    }

    private void ResetVoiceForTargetLanguage()
    {
      if (voiceOutput.IsConnected)
      {
        if (TargetLanguage == "en")
          voiceOutput.SendData(new VoiceData(VoiceType.en_US_Michael));
        else if (TargetLanguage == "de")
          voiceOutput.SendData(new VoiceData(VoiceType.de_DE_Dieter));
        else if (TargetLanguage == "es")
          voiceOutput.SendData(new VoiceData(VoiceType.es_ES_Enrique));
        else if (TargetLanguage == "fr")
          voiceOutput.SendData(new VoiceData(VoiceType.fr_FR_Renee));
        else if (TargetLanguage == "it")
          voiceOutput.SendData(new VoiceData(VoiceType.it_IT_Francesca));
        else if (TargetLanguage == "ja")
          voiceOutput.SendData(new VoiceData(VoiceType.ja_JP_Emi));
        else
          Log.Warning("TranslateWidget", "Unsupported voice for language {0}", TargetLanguage);
      }
    }
    #endregion
  }
}
