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

using IBM.Watson.DeveloperCloud.Utilities;
using IBM.Watson.DeveloperCloud.Logging;
using System;
using System.Collections.Generic;
using UnityEngine;


namespace IBM.Watson.DeveloperCloud.Widgets
{

  /// <summary>
  /// This widget class maps Touch events to a SerializedDelegate.
  /// </summary>
  public class TouchWidget : Widget
  {
    #region Widget interface
    /// <exclude />
    protected override string GetName()
    {
      return "Touch";
    }
    #endregion

    #region Private Data
    [Serializable]
    public class TapEventMapping
    {
      public GameObject tapObject = null;
      public bool tapOnObject = true;
      public int sortingLayer = 0;
      public LayerMask layerMask = default(LayerMask);
      public string callback = "";
    };

    [Serializable]
    public class FullScreenDragEventMapping
    {
      [Tooltip("If there is no drag layer object set, it uses FullScreen")]
      public GameObject dragLayerObject = null;
      public int numberOfFinger = 1;
      public int sortingLayer = 0;
      public bool isDragInside = true;
      public string callback = "";
    };

    [SerializeField]
    private List<TapEventMapping> tapMappings = new List<TapEventMapping>();

    [SerializeField]
    private List<FullScreenDragEventMapping> fullScreenDragMappings = new List<FullScreenDragEventMapping>();
    #endregion

    #region Public Members

    /// <summary>
    /// Gets or sets the tap mappings.
    /// </summary>
    /// <value>The tap mappings.</value>
    public List<TapEventMapping> TapMappings
    {
      get
      {
        return tapMappings;
      }
      set
      {
        tapMappings = value;
      }
    }

    /// <summary>
    /// Gets or sets the full screen drag mappings.
    /// </summary>
    /// <value>The full screen drag mappings.</value>
    public List<FullScreenDragEventMapping> FullScreenDragMappings
    {
      get
      {
        return fullScreenDragMappings;
      }
      set
      {
        fullScreenDragMappings = value;
      }
    }

    #endregion

    #region Event Handlers
    private void OnEnable()
    {
      if (TouchEventManager.Instance == null)
      {
        Log.Error("TouchWidget", "There should be TouchEventManager in the scene! No TouchEventManager found.");
        return;
      }

      foreach (var mapping in tapMappings)
      {
        if (!string.IsNullOrEmpty(mapping.callback))
        {
          TouchEventManager.Instance.RegisterTapEvent(mapping.tapObject, mapping.callback, mapping.sortingLayer, mapping.tapOnObject, mapping.layerMask);
        }
        else
        {
          Log.Warning("TouchWidget", "Callback function needs to be defined to register TouchWidget - Tap");
        }
      }

      foreach (var mapping in fullScreenDragMappings)
      {
        if (!string.IsNullOrEmpty(mapping.callback))
        {
          TouchEventManager.Instance.RegisterDragEvent(mapping.dragLayerObject, mapping.callback, mapping.numberOfFinger, mapping.sortingLayer, isDragInside: mapping.isDragInside);
        }
        else
        {
          Log.Warning("TouchWidget", "Callback function needs to be defined to register TouchWidget - Drag");
        }
      }
    }

    private void OnDisable()
    {
      if (TouchEventManager.Instance == null)
      {
        //Log.Error ("TouchWidget", "There should be TouchEventManager in the scene! No TouchEventManager found.");
        return;
      }

      foreach (var mapping in tapMappings)
      {
        if (!string.IsNullOrEmpty(mapping.callback))
        {
          TouchEventManager.Instance.UnregisterTapEvent(mapping.tapObject, mapping.callback, mapping.sortingLayer, mapping.tapOnObject, mapping.layerMask);
        }
        else
        {
          Log.Warning("TouchWidget", "Callback function needs to be defined to unregister TouchWidget - Tap");
        }
      }

      foreach (var mapping in fullScreenDragMappings)
      {
        if (!string.IsNullOrEmpty(mapping.callback))
        {
          TouchEventManager.Instance.UnregisterDragEvent(mapping.dragLayerObject, mapping.callback, mapping.numberOfFinger, mapping.sortingLayer, isDragInside: mapping.isDragInside);
        }
        else
        {
          Log.Warning("TouchWidget", "Callback function needs to be defined to unregister TouchWidget - Drag");
        }


      }
    }
    #endregion
  }

}