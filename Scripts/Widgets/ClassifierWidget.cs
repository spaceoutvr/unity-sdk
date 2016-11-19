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

using IBM.Watson.DeveloperCloud.Services.NaturalLanguageClassifier.v1;
using IBM.Watson.DeveloperCloud.DataTypes;
using IBM.Watson.DeveloperCloud.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine;

#pragma warning disable 414

namespace IBM.Watson.DeveloperCloud.Widgets
{
  /// <summary>
  /// This widget class maps Natural Language Classifier results to a SerializedDelegate.
  /// </summary>
  public class ClassifierWidget : Widget
  {
    #region Inputs
    [SerializeField]
    private Input classifyInput = new Input("Classified", typeof(ClassifyResultData), "OnClassifyInput");
    #endregion

    #region Outputs
    [SerializeField]
    private Output classifyOutput = new Output(typeof(ClassifyResultData));
    #endregion

    #region Widget interface
    /// <exclude />
    protected override string GetName()
    {
      return "Classifier";
    }
    #endregion

    #region Private Data
    private delegate void OnClassifierResult(ClassifyResult result);

    [Serializable]
    private class Mapping
    {
      public string _class = string.Empty;
      public SerializedDelegate callback = new SerializedDelegate(typeof(OnClassifierResult));
      public bool exclusive = true;
    };

    [SerializeField]
    private List<Mapping> mappings = new List<Mapping>();
    #endregion

    #region Event Handlers
    private void OnClassifyInput(Data data)
    {
      ClassifyResultData input = (ClassifyResultData)data;

      bool bPassthrough = true;
      foreach (var mapping in mappings)
      {
        if (mapping._class == input.Result.top_class)
        {
          OnClassifierResult callback = mapping.callback.ResolveDelegate() as OnClassifierResult;
          if (callback != null)
          {
            callback(input.Result);
            if (mapping.exclusive)
            {
              bPassthrough = false;
              break;
            }
          }
        }
      }

      if (bPassthrough)
        classifyOutput.SendData(data);
    }
    #endregion
  }

}