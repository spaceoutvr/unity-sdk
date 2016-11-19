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
using IBM.Watson.DeveloperCloud.Services.TextToSpeech.v1;
using IBM.Watson.DeveloperCloud.Logging;
using IBM.Watson.DeveloperCloud.DataTypes;
using System.Collections.Generic;
using IBM.Watson.DeveloperCloud.Utilities;

#pragma warning disable 414

namespace IBM.Watson.DeveloperCloud.Widgets
{
  /// <summary>
  /// TextToSpeech widget class wraps the TextToSpeech serivce.
  /// </summary>
  [RequireComponent(typeof(AudioSource))]
  public class TextToSpeechWidget : Widget
  {
    #region Inputs
    [SerializeField]
    private Input textInput = new Input("Text", typeof(TextToSpeechData), "OnTextInput");
    [SerializeField]
    private Input voiceInput = new Input("Voice", typeof(VoiceData), "OnVoiceSelect");
    #endregion

    #region Outputs
    [SerializeField]
    private Output speaking = new Output(typeof(SpeakingStateData));
    [SerializeField]
    private Output disableMic = new Output(typeof(DisableMicData));
    [SerializeField]
    private Output levelOut = new Output(typeof(LevelData));
    #endregion

    #region Private Data
    TextToSpeech textToSpeech = new TextToSpeech();

    [SerializeField, Tooltip("How often to send level out data in seconds.")]
    private float levelOutInterval = 0.05f;
    [SerializeField]
    private float levelOutputModifier = 1.0f;
    [SerializeField]
    private Button textToSpeechButton = null;
    [SerializeField]
    private InputField input = null;
    [SerializeField]
    private Text statusText = null;
    [SerializeField]
    private VoiceType voice = VoiceType.en_US_Michael;
    [SerializeField]
    private bool usePost = false;

    private AudioSource source = null;
    private int lastPlayPos = 0;

    private class Speech
    {
      ~Speech()
      {
        if (Clip != null)
          UnityObjectUtil.DestroyUnityObject(Clip);
      }

      public bool Ready { get; set; }
      public AudioClip Clip { get; set; }

      public Speech(TextToSpeech textToSpeech, string text, bool usePost)
      {
        textToSpeech.ToSpeech(text, OnAudioClip, usePost);
      }

      private void OnAudioClip(AudioClip clip)
      {
        Clip = clip;
        Ready = true;
      }

    };

    private Queue<Speech> speechQueue = new Queue<Speech>();
    private Speech activeSpeech = null;
    #endregion

    #region Public Memebers

    /// <summary>
    /// Gets or sets the voice. Default voice is English, US - Michael
    /// </summary>
    /// <value>The voice.</value>
    public VoiceType Voice
    {
      get
      {
        return voice;
      }
      set
      {
        voice = value;
      }
    }

    #endregion

    #region Event Handlers
    /// <summary>
    /// Button event handler.
    /// </summary>
    public void OnTextToSpeech()
    {
      if (textToSpeech.Voice != voice)
        textToSpeech.Voice = voice;
      if (input != null)
        speechQueue.Enqueue(new Speech(textToSpeech, input.text, usePost));
      if (statusText != null)
        statusText.text = "THINKING";
      if (textToSpeechButton != null)
        textToSpeechButton.interactable = false;
    }
    #endregion

    #region Private Functions
    private void OnTextInput(Data data)
    {
      TextToSpeechData text = data as TextToSpeechData;
      if (text == null)
        throw new WatsonException("Wrong data type received.");

      if (!string.IsNullOrEmpty(text.Text))
      {
        if (textToSpeech.Voice != voice)
          textToSpeech.Voice = voice;

        speechQueue.Enqueue(new Speech(textToSpeech, text.Text, usePost));
      }
    }

    private void OnVoiceSelect(Data data)
    {
      VoiceData voiceData = data as VoiceData;
      if (voiceData == null)
        throw new WatsonException("Unexpected data type");

      voice = voiceData.Voice;
    }

    private void OnEnable()
    {
      UnityObjectUtil.StartDestroyQueue();

      if (statusText != null)
        statusText.text = "READY";
    }

    /// <exclude />
    protected override void Start()
    {
      base.Start();
      source = GetComponent<AudioSource>();
    }

    private void Update()
    {
      if (source != null && !source.isPlaying
          && speechQueue.Count > 0
          && speechQueue.Peek().Ready)
      {
        CancelInvoke("OnEndSpeech");

        activeSpeech = speechQueue.Dequeue();
        if (activeSpeech.Clip != null)
        {
          if (speaking.IsConnected)
            speaking.SendData(new SpeakingStateData(true));
          if (disableMic.IsConnected)
            disableMic.SendData(new DisableMicData(true));

          source.spatialBlend = 0.0f;     // 2D sound
          source.loop = false;            // do not loop
          source.clip = activeSpeech.Clip;             // clip
          source.Play();

          Invoke("OnEndSpeech", ((float)activeSpeech.Clip.samples / (float)activeSpeech.Clip.frequency) + 0.1f);
          if (levelOut.IsConnected)
          {
            lastPlayPos = 0;
            InvokeRepeating("OnLevelOut", levelOutInterval, levelOutInterval);
          }
        }
        else
        {
          Log.Warning("TextToSpeechWidget", "Skipping null AudioClip");
        }
      }

      if (textToSpeechButton != null)
        textToSpeechButton.interactable = true;
      if (statusText != null)
        statusText.text = "READY";
    }

    private void OnLevelOut()
    {
      if (source != null && source.isPlaying)
      {
        int currentPos = source.timeSamples;
        if (currentPos > lastPlayPos)
        {
          float[] samples = new float[currentPos - lastPlayPos];
          source.clip.GetData(samples, lastPlayPos);
          levelOut.SendData(new LevelData(Mathf.Max(samples) * levelOutputModifier, levelOutputModifier));
          lastPlayPos = currentPos;
        }
      }
      else
        CancelInvoke("OnLevelOut");
    }
    private void OnEndSpeech()
    {
      if (speaking.IsConnected)
        speaking.SendData(new SpeakingStateData(false));
      if (disableMic.IsConnected)
        disableMic.SendData(new DisableMicData(false));
      if (source.isPlaying)
        source.Stop();

      activeSpeech = null;
    }

    /// <exclude />
    protected override string GetName()
    {
      return "TextToSpeech";
    }
    #endregion
  }

}
