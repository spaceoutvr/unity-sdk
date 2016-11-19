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
using IBM.Watson.DeveloperCloud.DataTypes;
using System;
using UnityEngine.UI;
using IBM.Watson.DeveloperCloud.Logging;

#pragma warning disable 0414

namespace IBM.Watson.DeveloperCloud.Widgets
{
  /// <summary>
  /// This widget displays WebCam video.
  /// </summary>
  public class WebCamDisplayWidget : Widget
  {
    #region Inputs
    [SerializeField]
    private Input webCamTextureInput = new Input("WebCamTexture", typeof(WebCamTextureData), "OnWebCamTexture");
    #endregion

    #region Outputs
    #endregion

    #region Private Data
    [SerializeField]
    private RawImage rawImage;
    [SerializeField]
    private Material material;

    private WebCamTexture webCamTexture;
    private int requestedWidth;
    private int requestedHeight;
    private int requestedFPS;
    #endregion

    #region Constants
    #endregion

    #region Public Properties
    /// <summary>
    /// The Raw Image displaying the WebCam stream in UI.
    /// </summary>
    public RawImage RawImage
    {
      get { return rawImage; }
      set { rawImage = value; }
    }
    /// <summary>
    /// The Material displaying the WebCam stream on Geometry.
    /// </summary>
    public Material Material
    {
      get { return material; }
    }
    #endregion

    #region Public Functions
    #endregion

    #region Widget Interface
    protected override string GetName()
    {
      return "WebCamDisplay";
    }
    #endregion

    #region Private Functions
    #endregion

    #region Event Handlers
    private void OnWebCamTexture(Data data)
    {
      Log.Debug("WebCamDisplayWidget", "OnWebCamTexture()");
      if (Material == null && RawImage == null)
        throw new ArgumentNullException("A Material or RawImage is required to display WebCamTexture");

      WebCamTextureData webCamTextureData = (WebCamTextureData)data;
      webCamTexture = webCamTextureData.CamTexture;
      requestedWidth = webCamTextureData.RequestedWidth;
      requestedHeight = webCamTextureData.RequestedHeight;
      requestedFPS = webCamTextureData.RequestedFPS;

      if (Material != null)
        Material.mainTexture = webCamTexture;

      if (RawImage != null)
      {
        RawImage.texture = webCamTexture;
        RawImage.material.mainTexture = webCamTexture;
      }
      if (!webCamTexture.isPlaying)
        webCamTexture.Play();
    }
    #endregion
  }
}
