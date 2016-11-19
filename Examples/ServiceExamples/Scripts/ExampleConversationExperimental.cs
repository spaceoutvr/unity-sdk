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
using IBM.Watson.DeveloperCloud.Services.ConversationExperimental.v1;
using IBM.Watson.DeveloperCloud.Logging;
using IBM.Watson.DeveloperCloud.Utilities;

public class ExampleConversationExperimental : MonoBehaviour
{
  private ConversationExperimental conversation = new ConversationExperimental();
  private string workspaceID;
  private string input = "Can you unlock the door?";

  void Start()
  {
    LogSystem.InstallDefaultReactors();
    workspaceID = Config.Instance.GetVariableValue("ConversationExperimentalV1_ID");
    Debug.Log("User: " + input);

    conversation.Message(workspaceID, input, OnMessage);
  }

  void OnMessage(MessageResponse resp)
  {
    if (resp != null)
    {
      foreach (MessageIntent mi in resp.intents)
        Debug.Log("intent: " + mi.intent + ", confidence: " + mi.confidence);

      if (resp.output != null && !string.IsNullOrEmpty(resp.output.text))
        Debug.Log("response: " + resp.output.text);
    }
    else
    {
      Debug.Log("Failed to invoke Message();");
    }
  }
}
