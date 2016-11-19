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


using IBM.Watson.DeveloperCloud.Logging;
using IBM.Watson.DeveloperCloud.Services.SpeechToText.v1;
using IBM.Watson.DeveloperCloud.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IBM.Watson.DeveloperCloud.UnitTests
{
  public class TestSpeechToText : UnitTest
  {
    private SpeechToText speechToText = new SpeechToText();

    private bool getModelsTested = false;
    private bool getModelTested = false;

    private bool getCustomizationsTested = false;
    private bool createCustomizationTested = false;
    private bool getCustomizationTested = false;
    private bool getCustomCorporaTested = false;
    private bool addCustomCorpusTested = false;
    private bool getCustomWordsTested = false;
    private bool addCustomWordsUsingFileTested = false;
    private bool addCustomWordsUsingObjectTested = false;
    private bool getCustomWordTested = false;
    private bool trainCustomizationTested = false;
    private bool deleteCustomCorpusTested = false;
    private bool deleteCustomWordTested = false;
    private bool resetCustomizationTested = false;
    private bool deleteCustomizationTested = false;

    private string createdCustomizationID;
    private string createdCustomizationName = "unity-integration-test-customization";
    private string createdCorpusName = "unity-integration-test-corpus";
    private string customCorpusFilePath;
    private string speechToTextModelEnglish = "en-US_BroadbandModel";
    private string customWordsFilePath;
    private bool allowOverwrite = true;
    private string wordToGet = "watson";

    private bool isCustomizationBusy = false;

    public override IEnumerator RunTest()
    {
      if (Config.Instance.FindCredentials(speechToText.GetServiceID()) == null)
        yield break;

      customCorpusFilePath = Application.dataPath + "/Watson/Examples/ServiceExamples/TestData/test-stt-corpus.txt";
      customWordsFilePath = Application.dataPath + "/Watson/Examples/ServiceExamples/TestData/test-stt-words.json";

      //	GetModels
      Log.Debug("TestSpeechToText", "**********  Attempting to to GetModels");
      speechToText.GetModels(HandleGetModels);
      while (!getModelsTested)
        yield return null;

      //	GetModel
      Log.Debug("TestSpeechToText", "**********  Attempting to to GetModel {0}", speechToTextModelEnglish);
      speechToText.GetModel(HandleGetModel, speechToTextModelEnglish);
      while (!getModelTested)
        yield return null;

      //	GetCustomizations
      Log.Debug("TestSpeechToText", "**********  Attempting to to get customizations");
      speechToText.GetCustomizations(HandleGetCustomizations);
      while (!getCustomizationsTested)
        yield return null;

      //	CreateCustomization
      Log.Debug("TestSpeechToText", "**********  Attempting to to create customization {0}", createdCustomizationName);
      speechToText.CreateCustomization(HandleCreateCustomization, createdCustomizationName);
      while (!createCustomizationTested)
        yield return null;

      //	GetCustomization
      Log.Debug("TestSpeechToText", "**********  Attempting to to get customization {0}", createdCustomizationID);
      speechToText.GetCustomization(HandleGetCustomization, createdCustomizationID);
      while (!getCustomizationTested)
        yield return null;

      //	GetCustomCorpora
      Log.Debug("TestSpeechToText", "**********  Attempting to to get custom corpora");
      speechToText.GetCustomCorpora(HandleGetCustomCorpora, createdCustomizationID);
      while (!getCustomCorporaTested)
        yield return null;

      //	AddCustomCorpus
      Log.Debug("TestSpeechToText", "**********  Attempting to to add custom corpus {0}", createdCorpusName);
      speechToText.AddCustomCorpus(HandleAddCustomCorpus, createdCustomizationID, createdCorpusName, allowOverwrite, customCorpusFilePath);
      while (!addCustomCorpusTested)
        yield return null;

      Runnable.Run(CheckCustomizationStatus(createdCustomizationID));
      while (isCustomizationBusy)
        yield return null;

      //	GetCustomWords
      Log.Debug("TestSpeechToText", "**********  Attempting to to get custom words");
      speechToText.GetCustomWords(HandleGetCustomWords, createdCustomizationID);
      while (!getCustomWordsTested)
        yield return null;

      //	AddCustomWordsUsingFile
      Log.Debug("TestSpeechToText", "**********  Attempting to to add custom words using file {0}", customWordsFilePath);
      speechToText.AddCustomWords(HandleAddCustomWordsUsingFile, createdCustomizationID, true, customWordsFilePath);
      while (!addCustomWordsUsingFileTested)
        yield return null;

      Runnable.Run(CheckCustomizationStatus(createdCustomizationID));
      while (isCustomizationBusy)
        yield return null;

      //	AddCustomWordsUsingObject
      Words words = new Words();
      Word w0 = new Word();
      List<Word> wordList = new List<Word>();
      w0.word = "mikey";
      w0.sounds_like = new string[1];
      w0.sounds_like[0] = "my key";
      w0.display_as = "Mikey";
      wordList.Add(w0);
      Word w1 = new Word();
      w1.word = "charlie";
      w1.sounds_like = new string[1];
      w1.sounds_like[0] = "char lee";
      w1.display_as = "Charlie";
      wordList.Add(w1);
      Word w2 = new Word();
      w2.word = "bijou";
      w2.sounds_like = new string[1];
      w2.sounds_like[0] = "be joo";
      w2.display_as = "Bijou";
      wordList.Add(w2);
      words.words = wordList.ToArray();

      Log.Debug("TestSpeechToText", "**********  Attempting to to add custom words using object");
      speechToText.AddCustomWords(HandleAddCustomWordsUsingObject, createdCustomizationID, words);
      while (!addCustomWordsUsingObjectTested)
        yield return null;

      Runnable.Run(CheckCustomizationStatus(createdCustomizationID));
      while (isCustomizationBusy)
        yield return null;

      //	GetCustomWord
      Log.Debug("TestSpeechToText", "**********  Attempting to to get custom word {0}", wordToGet);
      speechToText.GetCustomWord(HandleGetCustomWord, createdCustomizationID, wordToGet);
      while (!getCustomWordTested)
        yield return null;

      //	TrainCustomization
      Log.Debug("TestSpeechToText", "**********  Attempting to to train customization {0}", createdCustomizationID);
      speechToText.TrainCustomization(HandleTrainCustomization, createdCustomizationID);
      while (!trainCustomizationTested)
        yield return null;

      Runnable.Run(CheckCustomizationStatus(createdCustomizationID));
      while (isCustomizationBusy)
        yield return null;

      //	DeleteCustomCorpus
      Log.Debug("TestSpeechToText", "**********  Attempting to to delete custom corpus {0}", createdCorpusName);
      speechToText.DeleteCustomCorpus(HandleDeleteCustomCorpus, createdCustomizationID, createdCorpusName);
      while (!deleteCustomCorpusTested)
        yield return null;

      Runnable.Run(CheckCustomizationStatus(createdCustomizationID));
      while (isCustomizationBusy)
        yield return null;

      //	DeleteCustomWord
      Log.Debug("TestSpeechToText", "**********  Attempting to to delete custom word {0}", wordToGet);
      speechToText.DeleteCustomWord(HandleDeleteCustomWord, createdCustomizationID, wordToGet);
      while (!deleteCustomWordTested)
        yield return null;

      Runnable.Run(CheckCustomizationStatus(createdCustomizationID));
      while (isCustomizationBusy)
        yield return null;

      //	ResetCustomization
      Log.Debug("TestSpeechToText", "**********  Attempting to to reset customization {0}", createdCustomizationID);
      speechToText.ResetCustomization(HandleResetCustomization, createdCustomizationID);
      while (!resetCustomizationTested)
        yield return null;

      //	The customization is always pending after reset for some reason!
      //Runnable.Run(CheckCustomizationStatus(createdCustomizationID));
      //while (isCustomizationBusy)
      //	yield return null;

      //	DeleteCustomization
      //Log.Debug("TestSpeechToText", "**********  Attempting to to delete customization {0}", createdCustomizationID);
      //speechToText.DeleteCustomization(HandleDeleteCustomization, createdCustomizationID);
      //while (!deleteCustomizationTested)
      //  yield return null;

      yield break;
    }

    private void HandleGetModels(Model[] models)
    {
      if (models != null)
        Log.Status("TestSpeechToText", "GetModels() returned {0} models.", models.Length);

      Test(models != null);
      getModelsTested = true;
    }

    private void HandleGetModel(Model model)
    {
      if (model != null)
      {
        Log.Debug("TestSpeechToText", "Model - name: {0} | description: {1} | language:{2} | rate: {3} | sessions: {4} | url: {5} | customLanguageModel: {6}",
          model.name, model.description, model.language, model.rate, model.sessions, model.url, model.supported_features.custom_language_model);
      }
      else
      {
        Log.Warning("TestSpeechToText", "Failed to get model!");
      }

      Test(model != null);
      getModelTested = true;
    }

    private void HandleGetCustomizations(Customizations customizations, string customData)
    {
      if (!string.IsNullOrEmpty(customData))
        Log.Debug("TestSpeechToText", "custom data: {0}", customData);

      if (customizations != null)
      {
        if (customizations.customizations.Length > 0)
        {
          foreach (Customization customization in customizations.customizations)
            Log.Debug("TestSpeechToText", "Customization - name: {0} | description: {1} | status: {2}", customization.name, customization.description, customization.status);

          Log.Debug("TestSpeechToText", "GetCustomizations() succeeded!");
        }
        else
        {
          Log.Debug("TestSpeechToText", "There are no customizations!");
        }
      }
      else
      {
        Log.Debug("TestSpeechToText", "Failed to get customizations!");
      }

      Test(customizations != null);
      getCustomizationsTested = true;
    }

    private void HandleCreateCustomization(CustomizationID customizationID, string customData)
    {
      if (customizationID != null)
      {
        Log.Debug("TestSpeechToText", "Customization created: {0}", customizationID.customization_id);
        Log.Debug("TestSpeechToText", "CreateCustomization() succeeded!");

        createdCustomizationID = customizationID.customization_id;
      }
      else
      {
        Log.Debug("TestSpeechToText", "Failed to create customization!");
      }

      Test(customizationID != null);
      createCustomizationTested = true;
    }

    private void HandleDeleteCustomization(bool success, string customData)
    {
      if (success)
      {
        Log.Debug("TestSpeechToText", "Deleted customization {0}!", createdCustomizationID);
        Log.Debug("TestSpeechToText", "DeletedCustomization() succeeded!");
        createdCustomizationID = default(string);
      }
      else
      {
        Log.Debug("TestSpeechToText", "Failed to delete customization!");
      }

      Test(success);
      deleteCustomizationTested = true;
    }

    private void HandleGetCustomization(Customization customization, string customData)
    {
      if (!string.IsNullOrEmpty(customData))
        Log.Debug("TestSpeechToText", "custom data: {0}", customData);

      if (customization != null)
      {
        Log.Debug("TestSpeechToText", "Customization - name: {0} | description: {1} | status: {2}", customization.name, customization.description, customization.status);
        Log.Debug("TestSpeechToText", "GetCustomization() succeeded!");
      }
      else
      {
        Log.Debug("TestSpeechToText", "Failed to get customization {0}!", createdCustomizationID);
      }

      Test(customization != null);
      getCustomizationTested = true;
    }

    private void HandleTrainCustomization(bool success, string customData)
    {
      if (!string.IsNullOrEmpty(customData))
        Log.Debug("TestSpeechToText", "custom data: {0}", customData);

      if (success)
      {
        Log.Debug("TestSpeechToText", "Train customization {0}!", createdCustomizationID);
        Log.Debug("TestSpeechToText", "TrainCustomization() succeeded!");
        isCustomizationBusy = true;
      }
      else
      {
        Log.Debug("TestSpeechToText", "Failed to train customization!");
      }

      Test(success);
      trainCustomizationTested = true;
    }

    private void HandleUpgradeCustomization(bool success, string customData)
    {
      if (!string.IsNullOrEmpty(customData))
        Log.Debug("TestSpeechToText", "custom data: {0}", customData);

      if (success)
      {
        Log.Debug("TestSpeechToText", "Upgrade customization {0}!", createdCustomizationID);
        Log.Debug("TestSpeechToText", "UpgradeCustomization() succeeded!");

      }
      else
      {
        Log.Debug("TestSpeechToText", "Failed to upgrade customization!");
      }

      Test(success);
      //upgradeCustomizationTested = true;
      //	Note: This method is not yet implemented!
    }

    private void HandleResetCustomization(bool success, string customData)
    {
      if (!string.IsNullOrEmpty(customData))
        Log.Debug("TestSpeechToText", "custom data: {0}", customData);

      if (success)
      {
        Log.Debug("TestSpeechToText", "Reset customization {0}!", createdCustomizationID);
        Log.Debug("TestSpeechToText", "ResetCustomization() succeeded!");
        //isCustomizationBusy = true;
      }
      else
      {
        Log.Debug("TestSpeechToText", "Failed to reset customization!");
      }

      Test(success);
      resetCustomizationTested = true;
    }

    private void HandleGetCustomCorpora(Corpora corpora, string customData)
    {
      if (!string.IsNullOrEmpty(customData))
        Log.Debug("TestSpeechToText", "CustomData: {0}", customData);

      if (corpora != null)
      {
        if (corpora.corpora.Length > 0)
        {
          foreach (Corpus corpus in corpora.corpora)
            Log.Debug("TestSpeechToText", "Corpus - name: {0} | total_words: {1} | out_of_vocabulary_words: {2} | staus: {3}",
              corpus.name, corpus.total_words, corpus.out_of_vocabulary_words, corpus.status);
        }
        else
        {
          Log.Debug("TestSpeechToText", "There are no custom corpora!");
        }

        Log.Debug("TestSpeechToText", "GetCustomCorpora() succeeded!");
      }
      else
      {
        Log.Debug("TestSpeechToText", "Failed to get custom corpora!");
      }

      Test(corpora != null);
      getCustomCorporaTested = true;
    }

    private void HandleDeleteCustomCorpus(bool success, string customData)
    {
      if (!string.IsNullOrEmpty(customData))
        Log.Debug("TestSpeechToText", "custom data: {0}", customData);

      if (success)
      {
        Log.Debug("TestSpeechToText", "DeleteCustomCorpus() succeeded!");
        isCustomizationBusy = true;
      }
      else
      {
        Log.Debug("TestSpeechToText", "Failed to delete custom corpus!");
      }

      Test(success);
      deleteCustomCorpusTested = true;
    }

    private void HandleAddCustomCorpus(bool success, string customData)
    {
      if (!string.IsNullOrEmpty(customData))
        Log.Debug("TestSpeechToText", "custom data: {0}", customData);

      if (success)
      {
        Log.Debug("TestSpeechToText", "AddCustomCorpus() succeeded!");
        isCustomizationBusy = true;
      }
      else
      {
        Log.Debug("TestSpeechToText", "Failed to delete custom corpus!");
      }

      Test(success);
      addCustomCorpusTested = true;
    }

    private void HandleGetCustomWords(WordsList wordList, string customData)
    {
      if (!string.IsNullOrEmpty(customData))
        Log.Debug("TestSpeechToText", "custom data: {0}", customData);

      if (wordList != null)
      {
        if (wordList.words != null && wordList.words.Length > 0)
        {
          foreach (WordData word in wordList.words)
            Log.Debug("TestSpeechToText", "WordData - word: {0} | sounds like[0]: {1} | display as: {2} | source[0]: {3}", word.word, word.sounds_like[0], word.display_as, word.source[0]);
        }
        else
        {
          Log.Debug("TestSpeechToText", "No custom words found!");
        }
      }
      else
      {
        Log.Debug("TestSpeechToText", "Failed to get custom words!");
      }

      Test(wordList != null);
      getCustomWordsTested = true;
    }

    private void HandleAddCustomWordsUsingFile(bool success, string customData)
    {
      if (!string.IsNullOrEmpty(customData))
        Log.Debug("TestSpeechToText", "custom data: {0}", customData);

      if (success)
      {
        Log.Debug("TestSpeechToText", "AddCustomWords() using file succeeded!");
        isCustomizationBusy = true;
      }
      else
      {
        Log.Debug("TestSpeechToText", "Failed to add custom words using file!");
      }

      Test(success);
      addCustomWordsUsingFileTested = true;
    }

    private void HandleAddCustomWordsUsingObject(bool success, string customData)
    {
      if (!string.IsNullOrEmpty(customData))
        Log.Debug("TestSpeechToText", "custom data: {0}", customData);

      if (success)
      {
        Log.Debug("TestSpeechToText", "AddCustomWords() using object succeeded!");
        isCustomizationBusy = true;
      }
      else
      {
        Log.Debug("TestSpeechToText", "Failed to add custom words using object!");
      }

      Test(success);
      addCustomWordsUsingObjectTested = true;
    }

    private void HandleDeleteCustomWord(bool success, string customData)
    {
      if (!string.IsNullOrEmpty(customData))
        Log.Debug("TestSpeechToText", "custom data: {0}", customData);

      if (success)
      {
        Log.Debug("TestSpeechToText", "DeleteCustomWord() succeeded!");
        isCustomizationBusy = true;
      }
      else
      {
        Log.Debug("TestSpeechToText", "Failed to delete custom word!");
      }

      Test(success);
      deleteCustomWordTested = true;
    }

    private void HandleGetCustomWord(WordData word, string customData)
    {
      if (!string.IsNullOrEmpty(customData))
        Log.Debug("TestSpeechToText", "custom data: {0}", customData);

      if (word != null)
        Log.Debug("TestSpeechToText", "WordData - word: {0} | sounds like[0]: {1} | display as: {2} | source[0]: {3}", word.word, word.sounds_like[0], word.display_as, word.source[0]);

      Test(word != null);
      getCustomWordTested = true;
    }

    private void OnCheckCustomizationStatus(Customization customization, string customData)
    {
      if (customization != null)
      {
        Log.Debug("TestSpeechToText", "Customization status: {0}", customization.status);
        if (customization.status != "ready" && customization.status != "available")
          Runnable.Run(CheckCustomizationStatus(customData, 5f));
        else
          isCustomizationBusy = false;
      }
      else
      {
        Log.Debug("TestSpeechToText", "Check customization status failed!");
      }
    }

    private IEnumerator CheckCustomizationStatus(string customizationID, float delay = 0.1f)
    {
      Log.Debug("TestSpeechToText", "Checking customization status in {0} seconds...", delay.ToString());
      yield return new WaitForSeconds(delay);

      //	passing customizationID in custom data
      speechToText.GetCustomization(OnCheckCustomizationStatus, customizationID, customizationID);
    }
  }
}
