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
using IBM.Watson.DeveloperCloud.Utilities;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

/// <summary>
/// Main UI menu item.
/// </summary>
[System.Serializable]
public class MenuScene
{
  /// <summary>
  /// The name of the scene.
  /// </summary>
  public string sceneName;
  /// <summary>
  /// the description of the scene.
  /// </summary>
  public string sceneDesc;
  /// <summary>
  /// Back button position for this scene.
  /// </summary>
  public Vector3 customBackButtonPosition = Vector3.zero;
  /// <summary>
  /// Back button scale for this scene.
  /// </summary>
  public Vector2 customBackButtonScale = Vector2.zero;
  /// <summary>
  /// Is the back button visible.
  /// </summary>
  public bool isVisibleBackButton = true;
}

/// <summary>
/// Script for the main UI.
/// </summary>
public class MainUI : MonoBehaviour
{
  [SerializeField]
  private LayoutGroup buttonLayout = null;
  [SerializeField]
  private Button buttonPrefab = null;
  [SerializeField]
  private GameObject backgroundUI = null;
  [SerializeField]
  private RectTransform buttonBack = null;
  private Vector3 initialBackButtonPosition;
  private Vector3 initialBackButtonScale;
  private Color initialBackButtonColor;

  [SerializeField]
  private MenuScene[] scenes = null;

  private const string MAIN_SCENE = "Main";

#if UNITY_EDITOR
  [UnityEditor.MenuItem("CONTEXT/MainUI/Update Scene Names")]
  private static void UpdateNames(UnityEditor.MenuCommand command)
  {
    MainUI context = (MainUI)command.context;
    List<string> scenes = new List<string>();
    foreach (UnityEditor.EditorBuildSettingsScene scene in UnityEditor.EditorBuildSettings.scenes)
    {
      if (scene == null || !scene.enabled)
        continue;

      string name = Path.GetFileNameWithoutExtension(scene.path);
      if (name == MAIN_SCENE)
        continue;

      scenes.Add(name);
    }
    scenes.Sort();
    context.scenes = new MenuScene[scenes.Count];
    for (int i = 0; i < scenes.Count; i++)
    {
      context.scenes[i] = new MenuScene();
      context.scenes[i].sceneName = scenes[i];
      context.scenes[i].sceneDesc = scenes[i];
    }
  }
#endif

  private IEnumerator Start()
  {
    if (backgroundUI == null)
      throw new WatsonException("backgroundUI is null.");
    if (buttonLayout == null)
      throw new WatsonException("buttonLayout is null.");
    if (buttonPrefab == null)
      throw new WatsonException("buttonPrefab is null.");
    if (buttonBack == null)
      throw new WatsonException("buttonBack is null.");
    else
    {
      if (buttonBack.GetComponent<RectTransform>() != null)
      {
        initialBackButtonPosition = buttonBack.GetComponent<RectTransform>().anchoredPosition3D;
        initialBackButtonScale = buttonBack.GetComponent<RectTransform>().sizeDelta;
      }
      else
      {
        throw new WatsonException("buttonBack doesn't have RectTransform");
      }

      if (buttonBack.GetComponent<Image>() != null)
      {
        initialBackButtonColor = buttonBack.GetComponentInChildren<Image>().color;
      }
      else
      {
        throw new WatsonException("buttonBack doesn't have Image");
      }

    }
    // wait for the configuration to be loaded first..
    while (!Config.Instance.ConfigLoaded)
      yield return null;

    // create the buttons..
    UpdateButtons();
  }

  private void OnLevelWasLoaded(int level)
  {
    UpdateButtons();
  }

  private void UpdateButtons()
  {
    while (buttonLayout.transform.childCount > 0)
      DestroyImmediate(buttonLayout.transform.GetChild(0).gameObject);

    //Log.Debug( "MainUI", "UpdateBottons, level = {0}", Application.loadedLevelName );
    if (SceneManager.GetActiveScene().name == MAIN_SCENE)
    {
      backgroundUI.SetActive(true);

      foreach (var scene in scenes)
      {
        if (string.IsNullOrEmpty(scene.sceneName))
          continue;

        GameObject buttonObj = GameObject.Instantiate(buttonPrefab.gameObject);
        buttonObj.transform.SetParent(buttonLayout.transform, false);

        Text buttonText = buttonObj.GetComponentInChildren<Text>();
        if (buttonText != null)
          buttonText.text = scene.sceneDesc;
        Button button = buttonObj.GetComponentInChildren<Button>();

        string captured = scene.sceneName;
        button.onClick.AddListener(() => OnLoadLevel(captured));
      }
    }
    else
    {
      backgroundUI.SetActive(false);
    }
  }

  private void OnLoadLevel(string name)
  {
    GameObject touchScript = GameObject.Find("TouchScript");
    if (touchScript != null)
    {
      DestroyImmediate(touchScript);
    }

    Log.Debug("MainUI", "OnLoadLevel, name = {0}", name);
    StartCoroutine(LoadLevelAsync(name));
  }

  private IEnumerator LoadLevelAsync(string name)
  {

    AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(name);
    if (asyncOperation == null)
      yield break;

    while (!asyncOperation.isDone)
      yield return new WaitForSeconds(0.1f);

    for (int i = 0; scenes != null && i < scenes.Length; i++)
    {
      if (scenes[i].sceneName == name)
      {
        if (scenes[i].customBackButtonPosition != Vector3.zero)
        {
          buttonBack.anchoredPosition3D = scenes[i].customBackButtonPosition;
          ChangeVisibilityOfButton(buttonBack, scenes[i].isVisibleBackButton);
        }
        else
        {
          buttonBack.anchoredPosition3D = initialBackButtonPosition;
          ChangeVisibilityOfButton(buttonBack, true);
        }

        if (scenes[i].customBackButtonScale != Vector2.zero)
        {
          buttonBack.sizeDelta = scenes[i].customBackButtonScale;
        }
        else
        {
          buttonBack.sizeDelta = initialBackButtonScale;
        }

        break;
      }
    }
  }

  private void ChangeVisibilityOfButton(RectTransform buttonBack, bool isVisible)
  {
    if (buttonBack.GetComponentInChildren<Text>() != null)
      buttonBack.GetComponentInChildren<Text>().enabled = isVisible;
    if (buttonBack.GetComponentInChildren<Image>() != null)
      buttonBack.GetComponentInChildren<Image>().color = isVisible ? initialBackButtonColor :
          new Color(initialBackButtonColor.r, initialBackButtonColor.g, initialBackButtonColor.b, 0.0f);
  }

  /// <summary>
  /// Back button handler for the MainUI.
  /// </summary>
  public void OnBack()
  {
    Log.Debug("MainUI", "OnBack invoked");
    if (SceneManager.GetActiveScene().name != MAIN_SCENE)
    {
      OnLoadLevel(MAIN_SCENE);
    }
    else
      Application.Quit();
  }

  private static MainUI _instance = null;
  private void Awake()
  {
    if (!_instance)
    {
      //first-time opening
      _instance = this;
    }
    else if (_instance != this)
    {
      Destroy(_instance.gameObject);
      _instance = this;

      MakeActiveEventSystem(false);
      StartCoroutine(MakeActiveEventSystemWithDelay(true));
    }
    else
    {
      //do nothing - the other instance will destroy the current instance.
    }

    DontDestroyOnLoad(transform.gameObject);
  }

  private IEnumerator MakeActiveEventSystemWithDelay(bool active)
  {
    yield return new WaitForEndOfFrame();
    MakeActiveEventSystem(active);
  }

  private void MakeActiveEventSystem(bool active)
  {
    Log.Debug("MainUI", "MakeActiveEventSystem, active = {0}", active);
    object[] systems = Resources.FindObjectsOfTypeAll(typeof(EventSystem));
    foreach (var system in systems)
      ((EventSystem)system).gameObject.SetActive(active);
  }

  private void Update()
  {
    if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.Escape))
    {
      OnBack();
    }
  }
}
