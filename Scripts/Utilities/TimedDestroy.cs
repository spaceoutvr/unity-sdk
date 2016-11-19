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

namespace IBM.Watson.DeveloperCloud.Utilities
{
  /// <summary>
  /// Helper class for automatically destroying objects after a static amount of time has elapsed.
  /// </summary>
  public class TimedDestroy : MonoBehaviour
  {
    [SerializeField, Tooltip("How many seconds until this component destroy's it's parent object.")]
    private float destroyTime = 5.0f;
    private float elapsedTime = 0.0f;
    private bool timeReachedToDestroy = false;
    [SerializeField]
    private bool alphaFade = true;
    [SerializeField]
    private bool alphaFadeOnAwake = false;
    [SerializeField]
    private float fadeTime = 1.0f;
    [SerializeField]
    private float fadeTimeOnAwake = 1.0f;
    [SerializeField]
    private Graphic alphaTarget = null;
    private bool fading = false;
    private float fadeStart = 0.0f;
    private Color initialColor = Color.white;
    private float fadeAwakeRatio = 0.0f;

    private void Start()
    {
      elapsedTime = 0.0f;

      if (alphaFade && alphaTarget != null)
      {
        initialColor = alphaTarget.color;

        if (alphaFadeOnAwake)
        {
          alphaTarget.color = new Color(initialColor.r, initialColor.g, initialColor.b, 0.0f);
        }
      }
    }

    private void Update()
    {

      if (alphaFadeOnAwake)
      {
        fadeAwakeRatio += (Time.deltaTime / fadeTimeOnAwake);
        alphaTarget.color = new Color(initialColor.r, initialColor.g, initialColor.b, Mathf.Clamp01(fadeAwakeRatio));
        if (fadeAwakeRatio > 1.0f)
          alphaFadeOnAwake = false;
      }

      if (!timeReachedToDestroy)
      {
        elapsedTime += Time.deltaTime;
        if (elapsedTime > destroyTime)
        {
          timeReachedToDestroy = true;
          OnTimeExpired();
        }
      }

      if (fading)
      {
        float fElapsed = Time.time - fadeStart;
        if (fElapsed < fadeTime && alphaTarget != null)
        {
          Color c = alphaTarget.color;
          c.a = 1.0f - fElapsed / fadeTime;
          alphaTarget.color = c;
        }
        else
          Destroy(gameObject);
      }
    }

    /// <summary>
    /// Resets the timer.
    /// </summary>
    public void ResetTimer()
    {
      elapsedTime = 0.0f;
      fading = false;
      timeReachedToDestroy = false;

      if (alphaFade && alphaTarget != null)
      {
        alphaTarget.color = initialColor;

      }
    }

    private void OnTimeExpired()
    {
      if (alphaFade && alphaTarget != null)
      {
        fading = true;
        fadeStart = Time.time;
      }
      else
        Destroy(gameObject);
    }
  }
}
