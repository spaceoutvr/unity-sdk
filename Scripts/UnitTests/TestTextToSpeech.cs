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
using IBM.Watson.DeveloperCloud.Services.TextToSpeech.v1;
using IBM.Watson.DeveloperCloud.Logging;
using IBM.Watson.DeveloperCloud.Utilities;

#pragma warning disable 0414
namespace IBM.Watson.DeveloperCloud.UnitTests
{
  public class TestTextToSpeech : UnitTest
  {
    private TextToSpeech textToSpeech = new TextToSpeech();
    private bool getTested = false;
    private bool postTested = false;
    private bool getVoicesTested = false;
    private bool getVoiceTested = false;
    private bool getPronunciationTested = false;
    private bool getCustomizationsTested = false;
    private bool createCustomizationTested = false;
    private bool deleteCustomizationTested = false;
    private bool getCustomizationTested = false;
    private bool updateCustomizationTested = false;
    private bool getCustomizationWordsTested = false;
    private bool addCustomizationWordsTested = false;
    private bool getCustomizationWordTested = false;
    private bool deleteCustomizationWordTested = false;

    private string customizationIDToTest;
    private string customizationToCreateName = "unity-integration-test-created-customization";
    private string customizationToCreateLanguage = "en-US";
    private string customizationToCreateDescription = "A text to speech voice customization created within Unity.";
    private string updateWord0 = "hello";
    private string updateWord1 = "goodbye";
    private string updateTranslation0 = "hullo";
    private string updateTranslation1 = "gbye";
    private Word updateWordObject0 = new Word();
    private Word updateWordObject1 = new Word();
    private string customizationIdCreated;

    public override IEnumerator RunTest()
    {
      customizationIDToTest = Config.Instance.GetVariableValue("TextToSpeech_IntegrationTestCustomVoiceModel");
      updateWordObject0.word = updateWord0;
      updateWordObject0.translation = updateTranslation0;
      updateWordObject1.word = updateWord1;
      updateWordObject1.translation = updateTranslation1;

      if (Config.Instance.FindCredentials(textToSpeech.GetServiceID()) == null)
        yield break;

      // Test GET
      textToSpeech.ToSpeech("Hello World using GET", OnSpeechGET);
      while (!getTested)
        yield return null;

      // Test POST
      textToSpeech.ToSpeech("Hello World using POST", OnSpeechPOST, true);
      while (!postTested)
        yield return null;

      //	Get Pronunciation
      string testWord = "Watson";
      Log.Debug("ExampleTextToSpeech", "Attempting to get pronunciation of {0}", testWord);
      textToSpeech.GetPronunciation(OnGetPronunciation, testWord, VoiceType.en_US_Allison);
      while (!getPronunciationTested)
        yield return null;

      //  Get Customizations
      Log.Debug("ExampleTextToSpeech", "Attempting to get a list of customizations");
      textToSpeech.GetCustomizations(OnGetCustomizations);
      while (!getCustomizationsTested)
        yield return null;

      //  Create Customization
      //Log.Debug("ExampleTextToSpeech", "Attempting to create a customization");
      //textToSpeech.CreateCustomization(OnCreateCustomization, customizationToCreateName, customizationToCreateLanguage, customizationToCreateDescription);
      //while (!createCustomizationTested)
      //    yield return null;

      //  Get Customization
      Log.Debug("ExampleTextToSpeech", "Attempting to get a customization");
      if (!textToSpeech.GetCustomization(OnGetCustomization, customizationIDToTest))
        Log.Debug("ExampleTextToSpeech", "Failed to get custom voice model!");
      while (!getCustomizationTested)
        yield return null;

      //  Update Customization
      Log.Debug("ExampleTextToSpeech", "Attempting to update a customization");
      Word[] words = { updateWordObject0 };
      CustomVoiceUpdate customVoiceUpdate = new CustomVoiceUpdate();
      customVoiceUpdate.words = words;
      if (!textToSpeech.UpdateCustomization(OnUpdateCustomization, customizationIDToTest, customVoiceUpdate))
        Log.Debug("ExampleTextToSpeech", "Failed to update customization!");
      while (!updateCustomizationTested)
        yield return null;

      //  Get Customization Words
      Log.Debug("ExampleTextToSpeech", "Attempting to get a customization's words");
      if (!textToSpeech.GetCustomizationWords(OnGetCustomizationWords, customizationIDToTest))
        Log.Debug("ExampleTextToSpeech", "Failed to get {0} words!", customizationIDToTest);
      while (!getCustomizationWordsTested)
        yield return null;

      //  Add Customization Words
      Log.Debug("ExampleTextToSpeech", "Attempting to add words to a customization");
      Word[] wordArray = { updateWordObject1 };
      Words wordsObject = new Words();
      wordsObject.words = wordArray;
      if (!textToSpeech.AddCustomizationWords(OnAddCustomizationWords, customizationIDToTest, wordsObject))
        Log.Debug("ExampleTextToSpeech", "Failed to add words to {0}!", wordsObject);
      while (!addCustomizationWordsTested)
        yield return null;

      ////  Get Customization Word
      Log.Debug("ExampleTextToSpeech", "Attempting to get the translation of a custom voice model's word.");
      if (!textToSpeech.GetCustomizationWord(OnGetCustomizationWord, customizationIDToTest, updateWord1))
        Log.Debug("ExampleTextToSpeech", "Failed to get the translation of {0} from {1}!", updateWord0, customizationIDToTest);
      while (!getCustomizationWordTested)
        yield return null;

      //Delete Customization Word
      //Log.Debug("ExampleTextToSpeech", "Attempting to delete customization word from custom voice model.");
      //if (!textToSpeech.DeleteCustomizationWord(OnDeleteCustomizationWord, customizationIdCreated, updateWord1))
      //    Log.Debug("ExampleTextToSpeech", "Failed to delete {0} from {1}!", updateWord1, customizationIdCreated);
      //while (!deleteCustomizationWordTested)
      //    yield return null;

      //  Delete Customization
      //Log.Debug("ExampleTextToSpeech", "Attempting to delete a customization");
      //if (!textToSpeech.DeleteCustomization(OnDeleteCustomization, customizationIdCreated))
      //    Log.Debug("ExampleTextToSpeech", "Failed to delete custom voice model!");
      //while (!deleteCustomizationTested)
      //    yield return null;

      yield break;
    }

    private void OnSpeechGET(AudioClip clip)
    {
      Log.Debug("TestTestToSpeech", "OnSpeechGET invoked.");

      Test(clip != null);
      getTested = true;

      PlayClip(clip);
    }

    private void OnSpeechPOST(AudioClip clip)
    {
      Log.Debug("TestTestToSpeech", "OnSpechPOST invoked.");

      Test(clip != null);
      postTested = true;

      PlayClip(clip);
    }

    private void PlayClip(AudioClip clip)
    {
      if (Application.isPlaying && clip != null)
      {
        GameObject audioObject = new GameObject("AudioObject");
        AudioSource source = audioObject.AddComponent<AudioSource>();
        source.spatialBlend = 0.0f;     // 2D sound
        source.loop = false;            // do not loop
        source.clip = clip;             // clip
        source.Play();

        // automatically destroy the object after the sound has played..
        GameObject.Destroy(audioObject, clip.length);
      }
    }

    private void OnGetVoices(Voices voices)
    {
      Log.Debug("ExampleTextToSpeech", "-----OnGetVoices-----");
      foreach (Voice voice in voices.voices)
        Log.Debug("ExampleTextToSpeech", "Voice | name: {0} | gender: {1} | language: {2} | customizable: {3} | description: {4}.", voice.name, voice.gender, voice.language, voice.customizable, voice.description);
      Log.Debug("ExampleTextToSpeech", "-----OnGetVoices-----");

      Test(voices.HasData());
      getVoicesTested = true;
    }

    private void OnGetVoice(Voice voice)
    {
      Log.Debug("ExampleTextToSpeech", "-----OnGetVoice-----");
      Log.Debug("ExampleTextToSpeech", "Voice | name: {0} | gender: {1} | language: {2} | customizable: {3} | description: {4}", voice.name, voice.gender, voice.language, voice.customizable, voice.description);
      Log.Debug("ExampleTextToSpeech", "-----OnGetVoice-----");

      Test(!string.IsNullOrEmpty(voice.name));
      getVoiceTested = true;
    }

    private void OnGetPronunciation(Pronunciation pronunciation)
    {
      Log.Debug("ExampleTextToSpeech", "-----OnGetPronunciation-----");
      Log.Debug("ExampleTextToSpeech", "Pronunciation: {0}.", pronunciation.pronunciation);
      Log.Debug("ExampleTextToSpeech", "-----OnGetPronunciation-----");

      Test(!string.IsNullOrEmpty(pronunciation.pronunciation));
      getPronunciationTested = true;
    }

    private void OnGetCustomizations(Customizations customizations, string data)
    {
      Log.Debug("ExampleTextToSpeech", "-----OnGetCustomizations-----");
      if (data != default(string))
        Log.Debug("ExampleTextToSpeech", "data: {0}", data);
      foreach (Customization customization in customizations.customizations)
        Log.Debug("ExampleTextToSpeech", "Customization: name: {0} | customization_id: {1} | language: {2} | description: {3} | owner: {4} | created: {5} | last modified: {6}", customization.name, customization.customization_id, customization.language, customization.description, customization.owner, customization.created, customization.last_modified);
      Log.Debug("ExampleTextToSpeech", "-----OnGetCustomizations-----");

      Test(customizations.HasData());
      getCustomizationsTested = true;
    }

    private void OnCreateCustomization(CustomizationID customizationID, string data)
    {
      Log.Debug("ExampleTextToSpeech", "-----OnCreateCustomization-----");
      if (data != default(string))
        Log.Debug("ExampleTextToSpeech", "data: {0}", data);
      Log.Debug("ExampleTextToSpeech", "CustomizationID: id: {0}.", customizationID.customization_id);
      Log.Debug("ExampleTextToSpeech", "-----OnCreateCustomization-----");

      customizationIdCreated = customizationID.customization_id;

      Test(!string.IsNullOrEmpty(customizationID.customization_id));
      createCustomizationTested = true;
    }

    private void OnDeleteCustomization(bool success, string data)
    {
      Log.Debug("ExampleTextToSpeech", "-----OnDeleteCustomization-----");
      if (data != default(string))
        Log.Debug("ExampleTextToSpeech", "data: {0}", data);
      Log.Debug("ExampleTextToSpeech", "Success: {0}.", success);
      Log.Debug("ExampleTextToSpeech", "-----OnDeleteCustomization-----");

      Test(success);
      deleteCustomizationTested = true;
    }

    private void OnGetCustomization(Customization customization, string data)
    {
      Log.Debug("ExampleTextToSpeech", "-----OnGetCustomization-----");
      if (data != default(string))
        Log.Debug("ExampleTextToSpeech", "data: {0}", data);
      Log.Debug("ExampleTextToSpeech", "Customization: name: {0} | customization_id: {1} | language: {2} | description: {3} | owner: {4} | created: {5} | last modified: {6}", customization.name, customization.customization_id, customization.language, customization.description, customization.owner, customization.created, customization.last_modified);
      Log.Debug("ExampleTextToSpeech", "-----OnGetCustomization-----");

      Test(customization.HasData());
      getCustomizationTested = true;
    }

    private void OnUpdateCustomization(bool success, string data)
    {
      Log.Debug("ExampleTextToSpeech", "-----OnUpdateCustomization-----");
      if (data != default(string))
        Log.Debug("ExampleTextToSpeech", "data: {0}", data);
      Log.Debug("ExampleTextToSpeech", "Success: {0}.", success);
      Log.Debug("ExampleTextToSpeech", "-----OnUpdateCustomization-----");

      Test(success);
      updateCustomizationTested = true;
    }

    private void OnGetCustomizationWords(Words words, string data)
    {
      Log.Debug("ExampleTextToSpeech", "-----OnGetCustomizationWords-----");
      if (data != default(string))
        Log.Debug("ExampleTextToSpeech", "data: {0}", data);
      foreach (Word word in words.words)
        Log.Debug("ExampleTextToSpeech", "Word: {0} | Translation: {1}.", word.word, word.translation);
      Log.Debug("ExampleTextToSpeech", "-----OnGetCustomizationWords-----");

      Test(words.HasData());
      getCustomizationWordsTested = true;
    }

    private void OnAddCustomizationWords(bool success, string data)
    {
      Log.Debug("ExampleTextToSpeech", "-----OnAddCustomizationWords-----");
      if (data != default(string))
        Log.Debug("ExampleTextToSpeech", "data: {0}", data);
      Log.Debug("ExampleTextToSpeech", "Success: {0}.", success);
      Log.Debug("ExampleTextToSpeech", "-----OnAddCustomizationWords-----");

      Test(success);
      addCustomizationWordsTested = true;
    }

    private void OnDeleteCustomizationWord(bool success, string data)
    {
      Log.Debug("ExampleTextToSpeech", "-----OnDeleteCustomizationWord-----");
      if (data != default(string))
        Log.Debug("ExampleTextToSpeech", "data: {0}", data);
      Log.Debug("ExampleTextToSpeech", "Success: {0}.", success);
      Log.Debug("ExampleTextToSpeech", "-----OnDeleteCustomizationWord-----");

      Test(success);
      deleteCustomizationWordTested = true;
    }

    private void OnGetCustomizationWord(Translation translation, string data)
    {
      Log.Debug("ExampleTextToSpeech", "-----OnGetCustomizationWord-----");
      if (data != default(string))
        Log.Debug("ExampleTextToSpeech", "data: {0}", data);
      Log.Debug("ExampleTextToSpeech", "Translation: {0}.", translation.translation);
      Log.Debug("ExampleTextToSpeech", "-----OnGetCustomizationWord-----");

      Test(!string.IsNullOrEmpty(translation.translation));
      getCustomizationWordTested = true;
    }
  }
}
