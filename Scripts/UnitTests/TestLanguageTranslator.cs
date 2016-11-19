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

using IBM.Watson.DeveloperCloud.Services.LanguageTranslator.v1;
#pragma warning disable 0414

namespace IBM.Watson.DeveloperCloud.UnitTests
{
  public class TestLanguageTranslator// : UnitTest   //  commented out UnitTest unitl LanguageTranslator goes live
  {
    private LanguageTranslator translate = new LanguageTranslator();
    private bool getModelTested = false;
    private bool getModelsTested = false;
    private bool getLanguagesTested = false;
    private bool identifyTested = false;
    private bool translateTested = false;

    //public override IEnumerator RunTest()
    //{
    //    if (Config.Instance.FindCredentials(translate.GetServiceID()) == null)
    //        yield break;

    //    translate.GetModel("en-es", OnGetModel);
    //    while (!getModelTested)
    //        yield return null;

    //    translate.GetModels(OnGetModels);
    //    while (!getModelsTested)
    //        yield return null;

    //    translate.GetLanguages(OnGetLanguages);
    //    while (!getLanguagesTested)
    //        yield return null;

    //    translate.Identify("What does the fox say?", OnIdentify);
    //    while (!identifyTested)
    //        yield return null;

    //    translate.GetTranslation("What does the fox say?", "en", "es", OnGetTranslation);
    //    while (!translateTested)
    //        yield return null;

    //    yield break;
    //}

    //private void OnGetModel(TranslationModel model)
    //{
    //    Test(model != null);
    //    if (model != null)
    //    {
    //        Log.Status("TestTranslate", "ModelID: {0}, Source: {1}, Target: {2}, Domain: {3}",
    //            model.model_id, model.source, model.target, model.domain);
    //    }
    //    getModelTested = true;
    //}

    //private void OnGetModels(TranslationModels models)
    //{
    //    Test(models != null);
    //    if (models != null)
    //    {
    //        foreach (var model in models.models)
    //        {
    //            Log.Status("TestTranslate", "ModelID: {0}, Source: {1}, Target: {2}, Domain: {3}",
    //                model.model_id, model.source, model.target, model.domain);
    //        }
    //    }
    //    getModelsTested = true;
    //}

    //private void OnGetTranslation(Translations translation)
    //{
    //    Test(translation != null);
    //    if (translation != null && translation.translations.Length > 0)
    //        Log.Status("TestTranslate", "Translation: {0}", translation.translations[0].translation);
    //    translateTested = true;
    //}

    //private void OnIdentify(string lang)
    //{
    //    Test(lang != null);
    //    if (lang != null)
    //        Log.Status("TestTranslate", "Identified Language as {0}", lang);
    //    identifyTested = true;
    //}

    //private void OnGetLanguages(Languages languages)
    //{
    //    Test(languages != null);
    //    if (languages != null)
    //    {
    //        foreach (var lang in languages.languages)
    //            Log.Status("TestTranslate", "Language: {0}, Name: {1}", lang.language, lang.name);
    //    }

    //    getLanguagesTested = true;
    //}
  }
}
