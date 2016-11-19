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

using IBM.Watson.DeveloperCloud.Services.ConversationExperimental.v1;

#pragma warning disable 0169
#pragma warning disable 0414
public class TestConversationExperimental// : UnitTest  //  Commented out integration test
{
  private ConversationExperimental conversation = new ConversationExperimental();
  private string workspaceID;
  private string input = "Can you unlock the door?";
  private bool messageTested = false;

  //public override IEnumerator RunTest()
  //{
  //       workspaceID = Config.Instance.GetVariableValue("ConversationExperimentalV1_WorkspaceID");

  //	if (Config.Instance.FindCredentials(conversation.GetServiceID()) == null)
  //		yield break;

  //	if(!messageTested)
  //	{
  //		conversation.Message(workspaceID, input, OnMessage);
  //		while(!messageTested)
  //			yield return null;
  //	}
  //}

  //private void OnMessage(MessageResponse resp)
  //{
  //	Test(resp != null);
  //	if(resp != null)
  //	{
  //		foreach(MessageIntent mi in resp.intents)
  //			Log.Debug("TestConversation", "intent: " + mi.intent + ", confidence: " + mi.confidence);
  //		Log.Debug("TestConversation", "response: " + resp.output.text);
  //	}

  //	messageTested = true;
  //}
}
