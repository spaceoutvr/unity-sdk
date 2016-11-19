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

//#define TEST_DELETE

using System.Collections;
using IBM.Watson.DeveloperCloud.Services.NaturalLanguageClassifier.v1;
using IBM.Watson.DeveloperCloud.Logging;
using IBM.Watson.DeveloperCloud.Utilities;
using System.IO;
using UnityEngine;
using System;

namespace IBM.Watson.DeveloperCloud.UnitTests
{
  public class TestNaturalLanguageClassifier : UnitTest
  {
    NaturalLanguageClassifier naturalLanguageClassifier = new NaturalLanguageClassifier();
    bool findClassifierTested = false;
    bool trainClasifierTested = false;
    bool trainClassifier = false;
#if TEST_DELETE
        bool deleteTested = false;
#endif
    string classifierId = null;
    bool classifyTested = false;

    public override IEnumerator RunTest()
    {
      if (Config.Instance.FindCredentials(naturalLanguageClassifier.GetServiceID()) == null)
        yield break;

      naturalLanguageClassifier.FindClassifier("TestNaturalLanguageClassifier/", OnFindClassifier);
      while (!findClassifierTested)
        yield return null;

      if (trainClassifier)
      {
        string trainingData = File.ReadAllText(Application.dataPath + "/Watson/Scripts/Editor/TestData/weather_data_train.csv");

        Test(naturalLanguageClassifier.TrainClassifier("TestNaturalLanguageClassifier/" + DateTime.Now.ToString(), "en", trainingData, OnTrainClassifier));
        while (!trainClasifierTested)
          yield return null;
      }
      else if (!string.IsNullOrEmpty(classifierId))
      {
        Test(naturalLanguageClassifier.Classify(classifierId, "Is it hot outside", OnClassify));
        while (!classifyTested)
          yield return null;
      }

#if TEST_DELETE
            if ( !string.IsNullOrEmpty( classifierId ) )
            {
                Test( naturalLanguageClassifier.DeleteClassifer( classifierId, OnDeleteClassifier ) );
                while(! deleteTested ) 
                    yield return null;
            }
#endif

      yield break;
    }

#if TEST_DELETE
        private void OnDeleteClassifier( bool success )
        {
            Test( success );
            deleteTested = true;
        }
#endif

    private void OnFindClassifier(Classifier find)
    {
      if (find != null)
      {
        Log.Status("TestNaturalLanguageClassifier", "Find Result, Classifier ID: {0}, Status: {1}", find.classifier_id, find.status);

        trainClassifier = false;
        if (find.status == "Available")
          classifierId = find.classifier_id;
      }
      else
      {
        trainClassifier = true;
      }
      findClassifierTested = true;
    }

    private void OnClassify(ClassifyResult result)
    {
      Test(result != null);
      if (result != null)
      {
        Log.Status("TestNaturalLanguageClassifier", "Classify Result: {0}", result.top_class);
        Test(result.top_class == "temperature");
      }
      classifyTested = true;
    }

    private void OnTrainClassifier(Classifier classifier)
    {
      Test(classifier != null);
      if (classifier != null)
        Log.Status("TestNaturalLanguageClassifier", "Classifier ID: {0}, Status: {1}", classifier.classifier_id, classifier.status);

      trainClasifierTested = true;
    }
  }
}

