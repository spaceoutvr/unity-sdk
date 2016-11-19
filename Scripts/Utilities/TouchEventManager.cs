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

// uncomment to enable debugging
//#define ENABLE_DEBUGGING

using UnityEngine;
using UnityEngine.EventSystems;
using System;
using System.Collections.Generic;
using TouchScript.Gestures;
using IBM.Watson.DeveloperCloud.Logging;

namespace IBM.Watson.DeveloperCloud.Utilities
{
  /// <summary>
  /// Touch Event Manager for all touch events. 
  /// Each element can register their touch related functions using this manager. 
  /// </summary>
  [RequireComponent(typeof(TapGesture))]
  public class TouchEventManager : MonoBehaviour
  {

    /// <summary>
    /// Touch Event Data holds all touch related event data for registering and unregistering events via Touch Event Manager.
    /// </summary>
    public class TouchEventData
    {
      private Collider collider;
      private Collider2D collider2D;
      private RectTransform rectTransform;
      private Collider[] colliderList;
      private Collider2D[] collider2DList;
      private RectTransform[] rectTransformList;
      private GameObject gameObject;
      private string m_tapEventCallback;
      private string m_dragEventCallback;
      private bool m_isInside;
      private int sortingLayer;

      /// <summary> 
      /// Game Object related with touch event
      /// </summary>
      public GameObject GameObjectAttached { get { return gameObject; } }
      /// <summary>
      /// If it is tap event (or one time action event) we are returning the collider of the event.
      /// </summary>
      public Collider Collider { get { return collider; } }
      /// <summary>
      /// Gets the collider2 d.
      /// </summary>
      /// <value>The collider2 d.</value>
      public Collider2D Collider2D { get { return collider2D; } }
      /// <summary>
      /// Gets the rect transform.
      /// </summary>
      /// <value>The rect transform.</value>
      public RectTransform RectTransform { get { return rectTransform; } }
      /// <summary>
      /// If there is a drag event (or continues action) we are holding game object and all colliders inside that object
      /// </summary>
      public Collider[] ColliderList { get { if (colliderList == null && collider != null) colliderList = new Collider[] { collider }; return colliderList; } }
      /// <summary>
      /// If there is a drag event (or continues action) we are holding game object and all colliders inside that object
      /// </summary>
      public Collider2D[] ColliderList2D { get { if (collider2DList == null && collider2D != null) collider2DList = new Collider2D[] { collider2D }; return collider2DList; } }
      /// <summary>
      /// Gets the rect transform list.
      /// </summary>
      /// <value>The rect transform list.</value>
      public RectTransform[] RectTransformList { get { if (rectTransformList == null && rectTransform != null) rectTransformList = new RectTransform[] { rectTransform }; return rectTransformList; } }

      /// <summary>
      /// If the touch event has happened inside of that object (collider) we will fire that event. Otherwise, it is considered as outside
      /// </summary>
      public bool IsInside { get { return m_isInside; } }
      /// <summary>
      /// Tap Delegate to call
      /// </summary>
      public string TapCallback { get { return m_tapEventCallback; } }
      /// <summary>
      /// Drag Delegate to call
      /// </summary>
      public string DragCallback { get { return m_dragEventCallback; } }
      /// <summary>
      /// Greater sorting layer is higher importance level. 
      /// </summary>
      public int SortingLayer { get { return sortingLayer; } }
      /// <summary>
      /// Gets a value indicating whether this instance can drag object.
      /// </summary>
      /// <value><c>true</c> if this instance can drag object; otherwise, <c>false</c>.</value>
      public bool CanDragObject { get { return GameObjectAttached != null && ((ColliderList != null && ColliderList.Length > 0) || (ColliderList2D != null && ColliderList2D.Length > 0) || (RectTransformList != null && RectTransformList.Length > 0)); } }

      /// <summary>
      /// Touch event constructor for Tap Event registration. 
      /// </summary>
      /// <param name="objectCollider">Collider of the object to tap</param>
      /// <param name="callback">Callback for Tap Event. After tapped, callback will be invoked</param>
      /// <param name="sortingLayer">Sorting level in order to sort the event listeners</param>
      /// <param name="isInside">Whether the tap is inside the object or not</param>
      public TouchEventData(Collider objectCollider, string callback, int objectSortingLayer, bool isInside)
      {
        collider = objectCollider;
        collider2D = null;
        rectTransform = null;
        colliderList = null;
        collider2DList = null;
        rectTransformList = null;
        m_tapEventCallback = callback;
        sortingLayer = objectSortingLayer;
        m_isInside = isInside;
      }

      /// <summary>
      /// Touch event constructor for 2D Tap Event registration. 
      /// </summary>
      /// <param name="collider">Collider of the object to tap</param>
      /// <param name="callback">Callback for Tap Event. After tapped, callback will be invoked</param>
      /// <param name="objectSortingLayer">Sorting level in order to sort the event listeners</param>
      /// <param name="isInside">Whether the tap is inside the object or not</param>
      public TouchEventData(Collider2D collider, string callback, int objectSortingLayer, bool isInside)
      {
        collider = null;
        collider2D = collider;
        rectTransform = null;
        colliderList = null;
        collider2DList = null;
        rectTransformList = null;
        m_tapEventCallback = callback;
        sortingLayer = objectSortingLayer;
        m_isInside = isInside;
      }

      /// <summary>
      /// Initializes a new instance of the
      /// <see cref="IBM.Watson.DeveloperCloud.Utilities.TouchEventManager+TouchEventData"/> class.
      /// </summary>
      /// <param name="rectTransform">Rect transform.</param>
      /// <param name="callback">Callback.</param>
      /// <param name="sortingLayer">Sorting layer.</param>
      /// <param name="isInside">If set to <c>true</c> is inside.</param>
      public TouchEventData(RectTransform targetRectTransform, string callback, int targetSortingLayer, bool isInside)
      {
        collider = null;
        collider2D = null;
        rectTransform = targetRectTransform;
        colliderList = null;
        collider2DList = null;
        rectTransformList = null;
        m_tapEventCallback = callback;
        sortingLayer = targetSortingLayer;
        m_isInside = isInside;
      }

      /// <summary>
      /// Touch event constructor for Drag Event registration. 
      /// </summary>
      /// <param name="go">Gameobject to drag</param>
      /// <param name="callback">Callback for Drag event. After dragging started, callback will be invoked until drag will be finished</param>
      /// <param name="targetSortingLayer">Sorting level in order to sort the event listeners</param>
      /// <param name="isInside"></param>
      public TouchEventData(GameObject go, string callback, int targetSortingLayer, bool isInside)
      {
        gameObject = go;
        colliderList = null;
        if (gameObject != null)
        {
          colliderList = gameObject.GetComponentsInChildren<Collider>(includeInactive: true);
          collider2DList = gameObject.GetComponentsInChildren<Collider2D>(includeInactive: true);
          rectTransformList = gameObject.GetComponentsInChildren<RectTransform>(includeInactive: true);
        }
        m_dragEventCallback = callback;
        sortingLayer = targetSortingLayer;
        m_isInside = isInside;
      }

      /// <summary>
      /// Determines whether this instance has touched on the specified hitTransform.
      /// </summary>
      /// <returns><c>true</c> if this instance has touched on the specified hitTransform; otherwise, <c>false</c>.</returns>
      /// <param name="hitTransform">Hit transform.</param>
      public bool HasTouchedOn(Transform hitTransform)
      {
        bool hasTouchedOn = false;
        if (ColliderList != null)
        {
          foreach (Collider itemCollider in ColliderList)
          {
            if (itemCollider.transform == hitTransform && itemCollider.gameObject.activeSelf)
            {
              hasTouchedOn = true;
              break;
            }
          }
        }

        if (!hasTouchedOn && ColliderList2D != null)
        {
          foreach (Collider2D itemCollider in ColliderList2D)
          {
            if (itemCollider.transform == hitTransform && itemCollider.gameObject.activeSelf)
            {
              hasTouchedOn = true;
              break;
            }
          }
        }

        if (!hasTouchedOn && RectTransformList != null)
        {
          foreach (RectTransform itemRectTransform in RectTransformList)
          {
            if (itemRectTransform.transform == hitTransform && itemRectTransform.gameObject.activeSelf)
            {
              hasTouchedOn = true;
              break;
            }
          }
        }

        return hasTouchedOn;
      }

      /// <summary>
      /// To check equality of the same Touch Event Data
      /// </summary>
      /// <param name="obj">Object to check equality</param>
      /// <returns>True if objects are equal</returns>
      public override bool Equals(object obj)
      {
        bool isEqual = false;
        TouchEventData touchEventData = obj as TouchEventData;
        if (touchEventData != null)
        {
          isEqual =
              (touchEventData.Collider == this.Collider &&
                  touchEventData.Collider2D == this.Collider2D &&
                  touchEventData.RectTransform == this.RectTransform &&
                  touchEventData.GameObjectAttached == this.GameObjectAttached &&
                  touchEventData.IsInside == this.IsInside &&
                  touchEventData.SortingLayer == this.SortingLayer &&
                  touchEventData.DragCallback == this.DragCallback &&
                  touchEventData.TapCallback == this.TapCallback);
        }
        else
        {
          isEqual = base.Equals(obj);
        }

        return isEqual;
      }

      /// <summary>
      /// Returns the hash code
      /// </summary>
      /// <returns>Default hash code coming from base class</returns>
      public override int GetHashCode()
      {
        return base.GetHashCode();
      }

      /// <summary>
      /// Returns a <see cref="System.String"/> that represents the current <see cref="IBM.Watson.DeveloperCloud.Utilities.TouchEventManager+TouchEventData"/>.
      /// </summary>
      /// <returns>A <see cref="System.String"/> that represents the current <see cref="IBM.Watson.DeveloperCloud.Utilities.TouchEventManager+TouchEventData"/>.</returns>
      public override string ToString()
      {
        return string.Format("[TouchEventData: GameObjectAttached={0}, Collider={1}, Collider2D={2}, RectTransform={3}, ColliderList={4}, ColliderList2D={5}, RectTransformList={6}, IsInside={7}, TapCallback={8}, DragCallback={9}, SortingLayer={10}, CanDragObject={11}]", GameObjectAttached, Collider, Collider2D, RectTransform, ColliderList, ColliderList2D, RectTransformList, IsInside, TapCallback, DragCallback, SortingLayer, CanDragObject);
      }

    }

    #region Private Data
    private UnityEngine.Camera m_mainCamera;

    private bool active = true;
    private Dictionary<int, List<TouchEventData>> tapEvents = new Dictionary<int, List<TouchEventData>>();
    private Dictionary<int, List<TouchEventData>> doubleTapEvents = new Dictionary<int, List<TouchEventData>>();
    private Dictionary<int, List<TouchEventData>> dragEvents = new Dictionary<int, List<TouchEventData>>();
    #endregion

    #region Serialized Private 
    [SerializeField]
    private TapGesture tapGesture;
    [SerializeField]
    private TapGesture doubleTapGesture;
    [SerializeField]
    private TapGesture threeTapGesture;
    [SerializeField]
    private ScreenTransformGesture oneFingerMoveGesture;
    [SerializeField]
    private ScreenTransformGesture twoFingerMoveGesture;
    [SerializeField]
    private PressGesture pressGesture;
    [SerializeField]
    private ReleaseGesture releaseGesture;
    [SerializeField]
    private LongPressGesture longPressGesture;

    #endregion

    #region Public Properties
    /// <summary>
    /// Set/Get the active state of this manager.
    /// </summary>
    public bool Active { get { return active; } set { active = value; } }

    private static TouchEventManager sinstance = null;
    /// <summary>
    /// The current instance of the TouchEventManager.
    /// </summary>
    public static TouchEventManager Instance { get { return sinstance; } }
    #endregion

    #region Awake / OnEnable / OnDisable

    void Awake()
    {
      sinstance = this;
    }

    private void OnEnable()
    {
      m_mainCamera = UnityEngine.Camera.main;
      if (tapGesture != null)
        tapGesture.Tapped += TapGesture_Tapped;
      if (doubleTapGesture != null)
        doubleTapGesture.Tapped += DoubleTapGesture_Tapped;
      if (threeTapGesture != null)
        threeTapGesture.Tapped += ThreeTapGesture_Tapped;
      if (oneFingerMoveGesture != null)
        oneFingerMoveGesture.Transformed += OneFingerTransformedHandler;
      if (twoFingerMoveGesture != null)
        twoFingerMoveGesture.Transformed += TwoFingerTransformedHandler;
      if (pressGesture != null)
        pressGesture.Pressed += PressGesturePressed;
      if (releaseGesture != null)
        releaseGesture.Released += ReleaseGestureReleased;
      if (longPressGesture != null)
        longPressGesture.LongPressed += LongPressGesturePressed;

    }

    private void OnDisable()
    {
      if (tapGesture != null)
        tapGesture.Tapped -= TapGesture_Tapped;
      if (doubleTapGesture != null)
        doubleTapGesture.Tapped -= DoubleTapGesture_Tapped;
      if (threeTapGesture != null)
        threeTapGesture.Tapped -= ThreeTapGesture_Tapped;
      if (oneFingerMoveGesture != null)
        oneFingerMoveGesture.Transformed -= OneFingerTransformedHandler;
      if (twoFingerMoveGesture != null)
        twoFingerMoveGesture.Transformed -= TwoFingerTransformedHandler;
      if (pressGesture != null)
        pressGesture.Pressed -= PressGesturePressed;
      if (releaseGesture != null)
        releaseGesture.Released -= ReleaseGestureReleased;
      if (longPressGesture != null)
        longPressGesture.LongPressed -= LongPressGesturePressed;

      if (dragEvents != null)
        dragEvents.Clear();
      if (tapEvents != null)
        tapEvents.Clear();
      if (doubleTapEvents != null)
        doubleTapEvents.Clear();
    }

    /// <summary>
    /// Gets the main camera.
    /// </summary>
    /// <value>The main camera.</value>
    public UnityEngine.Camera MainCamera
    {
      get
      {
        if (m_mainCamera == null || !m_mainCamera.transform.CompareTag("MainCamera"))
          m_mainCamera = UnityEngine.Camera.main;

        return m_mainCamera;
      }
    }


    #endregion

    #region OneFinger Events - Register / UnRegister / Call
    /// <summary>
    /// Registers the drag event.
    /// </summary>
    /// <returns><c>true</c>, if drag event was registered, <c>false</c> otherwise.</returns>
    /// <param name="gameObjectToDrag">Game object to drag. If it is null then fullscreen drag is registered. </param>
    /// <param name="callback">Callback.</param>
    /// <param name="numberOfFinger">Number of finger.</param>
    /// <param name="SortingLayer">Sorting layer.</param>
    /// <param name="isDragInside">If set to <c>true</c> is drag inside.</param>
    public bool RegisterDragEvent(GameObject gameObjectToDrag, string callback, int numberOfFinger = 1, int SortingLayer = 0, bool isDragInside = true)
    {
      bool success = false;

      if (!string.IsNullOrEmpty(callback))
      {
        if (dragEvents.ContainsKey(numberOfFinger))
        {
          dragEvents[numberOfFinger].Add(new TouchEventData(gameObjectToDrag, callback, SortingLayer, isDragInside));
        }
        else
        {
          dragEvents[numberOfFinger] = new List<TouchEventData>() { new TouchEventData(gameObjectToDrag, callback, SortingLayer, isDragInside) };
        }

        success = true;
      }
      else
      {
        Log.Warning("TouchEventManager", "There is no callback for drag event registration");
      }

      return success;
    }

    /// <summary>
    /// Unregisters the drag event.
    /// </summary>
    /// <returns><c>true</c>, if drag event was unregistered, <c>false</c> otherwise.</returns>
    /// <param name="gameObjectToDrag">Game object to drag.</param>
    /// <param name="callback">Callback.</param>
    /// <param name="numberOfFinger">Number of finger.</param>
    /// <param name="SortingLayer">Sorting layer.</param>
    /// <param name="isDragInside">If set to <c>true</c> is drag inside.</param>
    public bool UnregisterDragEvent(GameObject gameObjectToDrag, string callback, int numberOfFinger = 1, int SortingLayer = 0, bool isDragInside = true)
    {
      bool success = false;

      if (!string.IsNullOrEmpty(callback))
      {
        if (dragEvents.ContainsKey(numberOfFinger))
        {
          int numberOfRemovedCallbacks = dragEvents[numberOfFinger].RemoveAll(
              e =>
              e.GameObjectAttached == gameObjectToDrag &&
              e.DragCallback == callback &&
              e.SortingLayer == SortingLayer &&
              e.IsInside == isDragInside);

          success &= (numberOfRemovedCallbacks > 0);
        }
      }
      else
      {
        Log.Warning("TouchEventManager", "There is no callback for drag event unregistration");
      }
      return success;
    }


    private void OneFingerTransformedHandler(object sender, System.EventArgs e)
    {
      RaycastHit hitToFire3D = default(RaycastHit);
      RaycastHit2D hitToFire2D = default(RaycastHit2D);
#if UNITY_4_6 || UNITY_5 || UNITY_5_3_OR_NEWER
      RaycastResult hitToFire2DEventSystem = default(RaycastResult);
#endif

      //Log.Status ("TouchEventManager", "oneFingerManipulationTransformedHandler: {0}", oneFingerMoveGesture.DeltaPosition);
      if (active)
      {
        TouchEventData dragEventToFire = null;
        Vector3 oneFingerScreenPosition = oneFingerMoveGesture.ScreenPosition;
        Ray rayForDrag = MainCamera.ScreenPointToRay(oneFingerScreenPosition);

        foreach (var kp in dragEvents)
        {
          if (kp.Key == 1)
          {
            //Adding Variables for 3D Touch Check
            Transform hitTransform3D = null;
            RaycastHit hit3D = default(RaycastHit);
            bool isHitOnLayer3D = Physics.Raycast(rayForDrag, out hit3D, Mathf.Infinity, kp.Key);
            if (isHitOnLayer3D)
            {
              hitTransform3D = hit3D.collider.transform;
            }

            //Adding Variables for 2D Touch Check for 2d Colliders
            Transform hitTransform2D = null;
            RaycastHit2D hit2D = Physics2D.Raycast(rayForDrag.origin, rayForDrag.direction, Mathf.Infinity, kp.Key);
            bool isHitOnLayer2D = false;
            if (hit2D.collider != null)
            {
              isHitOnLayer2D = true;
              hitTransform2D = hit2D.collider.transform;
            }

#if UNITY_4_6 || UNITY_5 || UNITY_5_3_OR_NEWER
            //Adding Variables for Event.System Touch for UI Elements
            Transform hitTransform2DEventSystem = null;
            bool isHitOnLayer2DEventSystem = false;
            RaycastResult hit2DEventSystem = default(RaycastResult);
            if (EventSystem.current != null)
            {
              PointerEventData pointerEventForTap = new PointerEventData(EventSystem.current);
              pointerEventForTap.position = oneFingerScreenPosition;
              List<RaycastResult> raycastResultListFor2DEventSystem = new List<RaycastResult>();
              EventSystem.current.RaycastAll(pointerEventForTap, raycastResultListFor2DEventSystem);
              foreach (RaycastResult itemRaycastResult in raycastResultListFor2DEventSystem)
              {

                isHitOnLayer2DEventSystem = kp.Value.Exists(element => (element.GameObjectAttached != null && element.GameObjectAttached.activeSelf && element.GameObjectAttached.layer == itemRaycastResult.gameObject.layer));
                if (isHitOnLayer2DEventSystem)
                {
                  hit2DEventSystem = itemRaycastResult;
                  hitTransform2DEventSystem = itemRaycastResult.gameObject.transform;
                  break;
                }
              }
            }
#endif

            for (int i = 0; i < kp.Value.Count; ++i)
            {
              TouchEventData dragEventData = kp.Value[i];

              if (kp.Value[i].ColliderList == null && kp.Value[i].ColliderList2D == null && kp.Value[i].RectTransformList == null)
              {
                Log.Warning("TouchEventManager", "Removing invalid collider event receiver from OneFingerDrag");
                kp.Value.RemoveAt(i--);
                continue;
              }

              if (string.IsNullOrEmpty(dragEventData.DragCallback))
              {
                Log.Warning("TouchEventManager", "Removing invalid event receiver from OneFingerDrag");
                kp.Value.RemoveAt(i--);
                continue;
              }

              //If we can drag the object, we should check that whether there is a raycast or not!

              //3d Hit Check
              if (dragEventData.ColliderList != null && dragEventData.ColliderList.Length > 0)
              {
                if (dragEventData.IsInside && isHitOnLayer3D && Array.Exists(dragEventData.ColliderList, element => element.transform == hitTransform3D && element.gameObject.activeSelf))
                {
                  //Tapped inside the object
                  if (dragEventToFire == null)
                  {
                    dragEventToFire = dragEventData;
                    hitToFire3D = hit3D;
                    hitToFire2D = default(RaycastHit2D);
                    hitToFire2DEventSystem = default(RaycastResult);
#if ENABLE_DEBUGGING
                                        Log.Debug("TouchEventManager", "Inside - OneFingerDrag Event Found 3D. itemCollider: {0}, callback: {1}, SortingLayer: {2}, isTapInside: {3} ",hitTransform3D, dragEventData.DragCallback, dragEventData.SortingLayer, dragEventData.IsInside);
#endif
                  }
                  else
                  {
                    if (dragEventData.SortingLayer > dragEventToFire.SortingLayer ||
                        (dragEventToFire.SortingLayer == dragEventData.SortingLayer && !dragEventToFire.IsInside))
                    {
                      dragEventToFire = dragEventData;
                      hitToFire3D = hit3D;
                      hitToFire2D = default(RaycastHit2D);
                      hitToFire2DEventSystem = default(RaycastResult);

#if ENABLE_DEBUGGING
                                            Log.Debug("TouchEventManager", "Inside - OneFingerDrag Found 3D. itemCollider: {0}, callback: {1}, SortingLayer: {2}, isTapInside: {3} ",hitTransform3D, dragEventData.DragCallback, dragEventData.SortingLayer, dragEventData.IsInside);
#endif
                    }
                    else
                    {
                      //do nothing
                    }
                  }

                }
                else if (!dragEventData.IsInside && (!isHitOnLayer3D || !Array.Exists(dragEventData.ColliderList, element => element.transform == hitTransform3D && element.gameObject.activeSelf)))
                {
                  //Tapped outside the object
                  if (dragEventToFire == null)
                  {
                    dragEventToFire = dragEventData;
                    hitToFire3D = hit3D;
                    hitToFire2D = default(RaycastHit2D);
                    hitToFire2DEventSystem = default(RaycastResult);

#if ENABLE_DEBUGGING
                                        Log.Debug("TouchEventManager", "Outside - OneFingerDrag Event Found 3D - outside. itemCollider: {0}, callback: {1}, SortingLayer: {2}, isTapInside: {3} ",hitTransform3D, dragEventData.DragCallback, dragEventData.SortingLayer, dragEventData.IsInside);
#endif
                  }
                  else
                  {
                    if (dragEventData.SortingLayer > dragEventToFire.SortingLayer ||
                        (dragEventToFire.SortingLayer == dragEventData.SortingLayer && !dragEventToFire.IsInside))
                    {
                      dragEventToFire = dragEventData;
                      hitToFire3D = hit3D;
                      hitToFire2D = default(RaycastHit2D);
                      hitToFire2DEventSystem = default(RaycastResult);

#if ENABLE_DEBUGGING
                                            Log.Debug("TouchEventManager", "Outside - OneFingerDrag Event Found 3D - outside. itemCollider: {0}, callback: {1}, SortingLayer: {2}, isTapInside: {3} ",hitTransform3D, dragEventData.DragCallback, dragEventData.SortingLayer, dragEventData.IsInside);
#endif
                    }
                    else
                    {
                      //do nothing
                    }
                  }
                }
                else
                {
                  //do nothing
                }
              }

              //2d Hit Check
              if (dragEventData.ColliderList2D != null && dragEventData.ColliderList2D.Length > 0)
              {
                if (dragEventData.IsInside && isHitOnLayer2D && Array.Exists(dragEventData.ColliderList2D, element => element.transform == hitTransform2D && element.gameObject.activeSelf))
                {
                  //Tapped inside the object
                  if (dragEventToFire == null)
                  {
                    dragEventToFire = dragEventData;
                    hitToFire3D = default(RaycastHit);
                    hitToFire2D = hit2D;
                    hitToFire2DEventSystem = default(RaycastResult);

#if ENABLE_DEBUGGING
                                        Log.Debug("TouchEventManager", "Inside - OneFingerDrag Event Found 2D. itemCollider: {0}, callback: {1}, SortingLayer: {2}, isTapInside: {3} ",hitTransform2D, dragEventData.DragCallback, dragEventData.SortingLayer, dragEventData.IsInside);
#endif
                  }
                  else
                  {
                    if (dragEventData.SortingLayer > dragEventToFire.SortingLayer ||
                        (dragEventToFire.SortingLayer == dragEventData.SortingLayer && !dragEventToFire.IsInside))
                    {
                      dragEventToFire = dragEventData;
                      hitToFire3D = default(RaycastHit);
                      hitToFire2D = hit2D;
                      hitToFire2DEventSystem = default(RaycastResult);

#if ENABLE_DEBUGGING
                                            Log.Debug("TouchEventManager", "Inside - OneFingerDrag Event Found 2D. itemCollider: {0}, callback: {1}, SortingLayer: {2}, isTapInside: {3} ",hitTransform2D, dragEventData.DragCallback, dragEventData.SortingLayer, dragEventData.IsInside);
#endif
                    }
                    else
                    {
                      //do nothing
                    }
                  }

                }
                else if (!dragEventData.IsInside && (!isHitOnLayer2D || !Array.Exists(dragEventData.ColliderList2D, element => element.transform == hitTransform2D && element.gameObject.activeSelf)))
                {
                  //Tapped outside the object
                  if (dragEventToFire == null)
                  {
                    dragEventToFire = dragEventData;
                    hitToFire3D = default(RaycastHit);
                    hitToFire2D = hit2D;
                    hitToFire2DEventSystem = default(RaycastResult);

#if ENABLE_DEBUGGING
                                        Log.Debug("TouchEventManager", "Outside - OneFingerDrag Event Found 2D. itemCollider: {0}, callback: {1}, SortingLayer: {2}, isTapInside: {3} ",hitTransform2D, dragEventData.DragCallback, dragEventData.SortingLayer, dragEventData.IsInside);
#endif
                  }
                  else
                  {
                    if (dragEventData.SortingLayer > dragEventToFire.SortingLayer)
                    {
                      dragEventToFire = dragEventData;
                      hitToFire3D = default(RaycastHit);
                      hitToFire2D = hit2D;
                      hitToFire2DEventSystem = default(RaycastResult);

#if ENABLE_DEBUGGING
                                            Log.Debug("TouchEventManager", "Outside - OneFingerDrag Event Found 2D. itemCollider: {0}, callback: {1}, SortingLayer: {2}, isTapInside: {3} ",hitTransform2D, dragEventData.DragCallback, dragEventData.SortingLayer, dragEventData.IsInside);
#endif
                    }
                    else
                    {
                      //do nothing
                    }
                  }
                }
                else
                {
                  //do nothing
                }
              }

#if UNITY_4_6 || UNITY_5 || UNITY_5_3_OR_NEWER
              //2D UI Hit Check using EventSystem
              if (dragEventData.RectTransformList != null && dragEventData.RectTransformList.Length > 0)
              {
                if (dragEventData.IsInside && isHitOnLayer2DEventSystem && Array.Exists(dragEventData.RectTransformList, element => element.transform == hitTransform2DEventSystem && element.gameObject.activeSelf))
                {
                  //Tapped inside the object
                  if (dragEventToFire == null)
                  {
                    dragEventToFire = dragEventData;
                    hitToFire3D = default(RaycastHit);
                    hitToFire2D = default(RaycastHit2D);
                    hitToFire2DEventSystem = hit2DEventSystem;

#if ENABLE_DEBUGGING
                                        Log.Debug("TouchEventManager", "Inside - OneFingerDrag Event Found 2D Event System. itemCollider: {0}, callback: {1}, SortingLayer: {2}, isTapInside: {3} ",hitTransform2DEventSystem, dragEventData.DragCallback, dragEventData.SortingLayer, dragEventData.IsInside);
#endif
                  }
                  else
                  {
                    if (dragEventData.SortingLayer > dragEventToFire.SortingLayer ||
                        (dragEventToFire.SortingLayer == dragEventData.SortingLayer && !dragEventToFire.IsInside))
                    {
                      dragEventToFire = dragEventData;
                      hitToFire3D = default(RaycastHit);
                      hitToFire2D = default(RaycastHit2D);
                      hitToFire2DEventSystem = hit2DEventSystem;

#if ENABLE_DEBUGGING
                                            Log.Debug("TouchEventManager", "Inside - OneFingerDrag Event Found 2D Event System. itemCollider: {0}, callback: {1}, SortingLayer: {2}, isTapInside: {3} ",hitTransform2DEventSystem, dragEventData.DragCallback, dragEventData.SortingLayer, dragEventData.IsInside);
#endif
                    }
                    else
                    {
                      //do nothing
                    }
                  }

                }
                else if (!dragEventData.IsInside && (!isHitOnLayer2DEventSystem || !Array.Exists(dragEventData.RectTransformList, element => element.transform == hitTransform2DEventSystem && element.gameObject.activeSelf)))
                {
                  //Tapped outside the object
                  if (dragEventToFire == null)
                  {
                    dragEventToFire = dragEventData;
                    hitToFire3D = default(RaycastHit);
                    hitToFire2D = default(RaycastHit2D);
                    hitToFire2DEventSystem = hit2DEventSystem;

#if ENABLE_DEBUGGING
                                        Log.Debug("TouchEventManager", "Outside - OneFingerDrag Event Found 2D Event System. itemCollider: {0}, callback: {1}, SortingLayer: {2}, isTapInside: {3} ",hitTransform2DEventSystem, dragEventData.DragCallback, dragEventData.SortingLayer, dragEventData.IsInside);
#endif
                  }
                  else
                  {
                    if (dragEventData.SortingLayer > dragEventToFire.SortingLayer)
                    {
                      dragEventToFire = dragEventData;
                      hitToFire3D = default(RaycastHit);
                      hitToFire2D = default(RaycastHit2D);
                      hitToFire2DEventSystem = hit2DEventSystem;

#if ENABLE_DEBUGGING
                                            Log.Debug("TouchEventManager", "Outside - OneFingerDrag Event Found 2D Event System. itemCollider: {0}, callback: {1}, SortingLayer: {2}, isTapInside: {3} ",hitTransform2DEventSystem, dragEventData.DragCallback, dragEventData.SortingLayer, dragEventData.IsInside);
#endif
                    }
                    else
                    {
                      //do nothing
                    }
                  }
                }
                else
                {
                  //do nothing
                }
              }
#endif
            }
          }
        }

        if (dragEventToFire != null)
          EventManager.Instance.SendEvent(dragEventToFire.DragCallback, oneFingerMoveGesture, hitToFire3D, hitToFire2D, hitToFire2DEventSystem);

        EventManager.Instance.SendEvent("OnDragOneFingerFullscreen", oneFingerMoveGesture);
      }
    }

    private void TwoFingerTransformedHandler(object sender, System.EventArgs e)
    {
      RaycastHit hitToFire3D = default(RaycastHit);
      RaycastHit2D hitToFire2D = default(RaycastHit2D);
#if UNITY_4_6 || UNITY_5 || UNITY_5_3_OR_NEWER
      RaycastResult hitToFire2DEventSystem = default(RaycastResult);
#endif

      //Log.Status ("TouchEventManager", "TwoFingerTransformedHandler: {0}", twoFingerMoveGesture.DeltaPosition);
      if (active)
      {
        TouchEventData dragEventToFire = null;
        Vector3 twoFingerScreenPosition = twoFingerMoveGesture.ScreenPosition;
        Ray rayForDrag = MainCamera.ScreenPointToRay(twoFingerScreenPosition);


        foreach (var kp in dragEvents)
        {
          if (kp.Key == 2)
          {
            //Adding Variables for 3D Touch Check
            Transform hitTransform3D = null;
            RaycastHit hit3D = default(RaycastHit);
            bool isHitOnLayer3D = Physics.Raycast(rayForDrag, out hit3D, Mathf.Infinity, kp.Key);
            if (isHitOnLayer3D)
            {
              hitTransform3D = hit3D.collider.transform;
            }

            //Adding Variables for 2D Touch Check for 2d Colliders
            Transform hitTransform2D = null;
            RaycastHit2D hit2D = Physics2D.Raycast(rayForDrag.origin, rayForDrag.direction, Mathf.Infinity, kp.Key);
            bool isHitOnLayer2D = false;
            if (hit2D.collider != null)
            {
              isHitOnLayer2D = true;
              hitTransform2D = hit2D.collider.transform;
            }

#if UNITY_4_6 || UNITY_5 || UNITY_5_3_OR_NEWER
            //Adding Variables for Event.System Touch for UI Elements
            Transform hitTransform2DEventSystem = null;
            bool isHitOnLayer2DEventSystem = false;
            RaycastResult hit2DEventSystem = default(RaycastResult);
            if (EventSystem.current != null)
            {
              PointerEventData pointerEventForTap = new PointerEventData(EventSystem.current);
              pointerEventForTap.position = twoFingerScreenPosition;
              List<RaycastResult> raycastResultListFor2DEventSystem = new List<RaycastResult>();
              EventSystem.current.RaycastAll(pointerEventForTap, raycastResultListFor2DEventSystem);
              foreach (RaycastResult itemRaycastResult in raycastResultListFor2DEventSystem)
              {

                isHitOnLayer2DEventSystem = kp.Value.Exists(element => (element.GameObjectAttached != null && element.GameObjectAttached.layer == itemRaycastResult.gameObject.layer));
                if (isHitOnLayer2DEventSystem)
                {
                  hit2DEventSystem = itemRaycastResult;
                  hitTransform2DEventSystem = itemRaycastResult.gameObject.transform;
                  break;
                }
              }
            }
#endif

            for (int i = 0; i < kp.Value.Count; ++i)
            {
              TouchEventData dragEventData = kp.Value[i];

              if (kp.Value[i].ColliderList == null && kp.Value[i].ColliderList2D == null && kp.Value[i].RectTransformList == null)
              {
                Log.Warning("TouchEventManager", "Removing invalid collider event receiver from TwoFingerDrag");
                kp.Value.RemoveAt(i--);
                continue;
              }

              if (string.IsNullOrEmpty(dragEventData.DragCallback))
              {
                Log.Warning("TouchEventManager", "Removing invalid event receiver from TwoFingerDrag");
                kp.Value.RemoveAt(i--);
                continue;
              }

              //If we can drag the object, we should check that whether there is a raycast or not!

              //3d Hit Check
              if (dragEventData.ColliderList != null && dragEventData.ColliderList.Length > 0)
              {
                if (dragEventData.IsInside && isHitOnLayer3D && Array.Exists(dragEventData.ColliderList, element => element.transform == hitTransform3D && element.gameObject.activeSelf))
                {
                  //Tapped inside the object
                  if (dragEventToFire == null)
                  {
                    dragEventToFire = dragEventData;
                    hitToFire3D = hit3D;
                    hitToFire2D = default(RaycastHit2D);
                    hitToFire2DEventSystem = default(RaycastResult);
#if ENABLE_DEBUGGING
                                        Log.Debug("TouchEventManager", "Inside - TwoFingerDrag Event Found 3D. itemCollider: {0}, callback: {1}, SortingLayer: {2}, isTapInside: {3} ",hitTransform3D, dragEventData.DragCallback, dragEventData.SortingLayer, dragEventData.IsInside);
#endif
                  }
                  else
                  {
                    if (dragEventData.SortingLayer > dragEventToFire.SortingLayer ||
                        (dragEventToFire.SortingLayer == dragEventData.SortingLayer && !dragEventToFire.IsInside))
                    {
                      dragEventToFire = dragEventData;
                      hitToFire3D = hit3D;
                      hitToFire2D = default(RaycastHit2D);
                      hitToFire2DEventSystem = default(RaycastResult);

#if ENABLE_DEBUGGING
                                            Log.Debug("TouchEventManager", "Inside - TwoFingerDrag Found 3D. itemCollider: {0}, callback: {1}, SortingLayer: {2}, isTapInside: {3} ",hitTransform3D, dragEventData.DragCallback, dragEventData.SortingLayer, dragEventData.IsInside);
#endif
                    }
                    else
                    {
                      //do nothing
                    }
                  }

                }
                else if (!dragEventData.IsInside && (!isHitOnLayer3D || !Array.Exists(dragEventData.ColliderList, element => element.transform == hitTransform3D && element.gameObject.activeSelf)))
                {
                  //Tapped outside the object
                  if (dragEventToFire == null)
                  {
                    dragEventToFire = dragEventData;
                    hitToFire3D = hit3D;
                    hitToFire2D = default(RaycastHit2D);
                    hitToFire2DEventSystem = default(RaycastResult);

#if ENABLE_DEBUGGING
                                        Log.Debug("TouchEventManager", "Outside - TwoFingerDrag Event Found 3D - outside. itemCollider: {0}, callback: {1}, SortingLayer: {2}, isTapInside: {3} ",hitTransform3D, dragEventData.DragCallback, dragEventData.SortingLayer, dragEventData.IsInside);
#endif
                  }
                  else
                  {
                    if (dragEventData.SortingLayer > dragEventToFire.SortingLayer ||
                        (dragEventToFire.SortingLayer == dragEventData.SortingLayer && !dragEventToFire.IsInside))
                    {
                      dragEventToFire = dragEventData;
                      hitToFire3D = hit3D;
                      hitToFire2D = default(RaycastHit2D);
                      hitToFire2DEventSystem = default(RaycastResult);

#if ENABLE_DEBUGGING
                                            Log.Debug("TouchEventManager", "Outside - TwoFingerDrag Event Found 3D - outside. itemCollider: {0}, callback: {1}, SortingLayer: {2}, isTapInside: {3} ",hitTransform3D, dragEventData.DragCallback, dragEventData.SortingLayer, dragEventData.IsInside);
#endif
                    }
                    else
                    {
                      //do nothing
                    }
                  }
                }
                else
                {
                  //do nothing
                }
              }

              //2d Hit Check
              if (dragEventData.ColliderList2D != null && dragEventData.ColliderList2D.Length > 0)
              {
                if (dragEventData.IsInside && isHitOnLayer2D && Array.Exists(dragEventData.ColliderList2D, element => element.transform == hitTransform2D && element.gameObject.activeSelf))
                {
                  //Tapped inside the object
                  if (dragEventToFire == null)
                  {
                    dragEventToFire = dragEventData;
                    hitToFire3D = default(RaycastHit);
                    hitToFire2D = hit2D;
                    hitToFire2DEventSystem = default(RaycastResult);

#if ENABLE_DEBUGGING
                                        Log.Debug("TouchEventManager", "Inside - TwoFingerDrag Event Found 2D. itemCollider: {0}, callback: {1}, SortingLayer: {2}, isTapInside: {3} ",hitTransform2D, dragEventData.DragCallback, dragEventData.SortingLayer, dragEventData.IsInside);
#endif
                  }
                  else
                  {
                    if (dragEventData.SortingLayer > dragEventToFire.SortingLayer ||
                        (dragEventToFire.SortingLayer == dragEventData.SortingLayer && !dragEventToFire.IsInside))
                    {
                      dragEventToFire = dragEventData;
                      hitToFire3D = default(RaycastHit);
                      hitToFire2D = hit2D;
                      hitToFire2DEventSystem = default(RaycastResult);

#if ENABLE_DEBUGGING
                                            Log.Debug("TouchEventManager", "Inside - TwoFingerDrag Event Found 2D. itemCollider: {0}, callback: {1}, SortingLayer: {2}, isTapInside: {3} ",hitTransform2D, dragEventData.DragCallback, dragEventData.SortingLayer, dragEventData.IsInside);
#endif
                    }
                    else
                    {
                      //do nothing
                    }
                  }

                }
                else if (!dragEventData.IsInside && (!isHitOnLayer2D || !Array.Exists(dragEventData.ColliderList2D, element => element.transform == hitTransform2D && element.gameObject.activeSelf)))
                {
                  //Tapped outside the object
                  if (dragEventToFire == null)
                  {
                    dragEventToFire = dragEventData;
                    hitToFire3D = default(RaycastHit);
                    hitToFire2D = hit2D;
                    hitToFire2DEventSystem = default(RaycastResult);

#if ENABLE_DEBUGGING
                                        Log.Debug("TouchEventManager", "Outside - TwoFingerDrag Event Found 2D. itemCollider: {0}, callback: {1}, SortingLayer: {2}, isTapInside: {3} ",hitTransform2D, dragEventData.DragCallback, dragEventData.SortingLayer, dragEventData.IsInside);
#endif
                  }
                  else
                  {
                    if (dragEventData.SortingLayer > dragEventToFire.SortingLayer)
                    {
                      dragEventToFire = dragEventData;
                      hitToFire3D = default(RaycastHit);
                      hitToFire2D = hit2D;
                      hitToFire2DEventSystem = default(RaycastResult);

#if ENABLE_DEBUGGING
                                            Log.Debug("TouchEventManager", "Outside - TwoFingerDrag Event Found 2D. itemCollider: {0}, callback: {1}, SortingLayer: {2}, isTapInside: {3} ",hitTransform2D, dragEventData.DragCallback, dragEventData.SortingLayer, dragEventData.IsInside);
#endif
                    }
                    else
                    {
                      //do nothing
                    }
                  }
                }
                else
                {
                  //do nothing
                }
              }

#if UNITY_4_6 || UNITY_5 || UNITY_5_3_OR_NEWER
              //2D UI Hit Check using EventSystem
              if (dragEventData.RectTransformList != null && dragEventData.RectTransformList.Length > 0)
              {
                if (dragEventData.IsInside && isHitOnLayer2DEventSystem && Array.Exists(dragEventData.RectTransformList, element => element.transform == hitTransform2DEventSystem && element.gameObject.activeSelf))
                {
                  //Tapped inside the object
                  if (dragEventToFire == null)
                  {
                    dragEventToFire = dragEventData;
                    hitToFire3D = default(RaycastHit);
                    hitToFire2D = default(RaycastHit2D);
                    hitToFire2DEventSystem = hit2DEventSystem;

#if ENABLE_DEBUGGING
                                        Log.Debug("TouchEventManager", "Inside - TwoFingerDrag Event Found 2D Event System. itemCollider: {0}, callback: {1}, SortingLayer: {2}, isTapInside: {3} ",hitTransform2DEventSystem, dragEventData.DragCallback, dragEventData.SortingLayer, dragEventData.IsInside);
#endif
                  }
                  else
                  {
                    if (dragEventData.SortingLayer > dragEventToFire.SortingLayer ||
                        (dragEventToFire.SortingLayer == dragEventData.SortingLayer && !dragEventToFire.IsInside))
                    {
                      dragEventToFire = dragEventData;
                      hitToFire3D = default(RaycastHit);
                      hitToFire2D = default(RaycastHit2D);
                      hitToFire2DEventSystem = hit2DEventSystem;

#if ENABLE_DEBUGGING
                                            Log.Debug("TouchEventManager", "Inside - TwoFingerDrag Event Found 2D Event System. itemCollider: {0}, callback: {1}, SortingLayer: {2}, isTapInside: {3} ",hitTransform2DEventSystem, dragEventData.DragCallback, dragEventData.SortingLayer, dragEventData.IsInside);
#endif
                    }
                    else
                    {
                      //do nothing
                    }
                  }

                }
                else if (!dragEventData.IsInside && (!isHitOnLayer2DEventSystem || !Array.Exists(dragEventData.RectTransformList, element => element.transform == hitTransform2DEventSystem && element.gameObject.activeSelf)))
                {
                  //Tapped outside the object
                  if (dragEventToFire == null)
                  {
                    dragEventToFire = dragEventData;
                    hitToFire3D = default(RaycastHit);
                    hitToFire2D = default(RaycastHit2D);
                    hitToFire2DEventSystem = hit2DEventSystem;

#if ENABLE_DEBUGGING
                                        Log.Debug("TouchEventManager", "Outside - TwoFingerDrag Event Found 2D Event System. itemCollider: {0}, callback: {1}, SortingLayer: {2}, isTapInside: {3} ",hitTransform2DEventSystem, dragEventData.DragCallback, dragEventData.SortingLayer, dragEventData.IsInside);
#endif
                  }
                  else
                  {
                    if (dragEventData.SortingLayer > dragEventToFire.SortingLayer)
                    {
                      dragEventToFire = dragEventData;
                      hitToFire3D = default(RaycastHit);
                      hitToFire2D = default(RaycastHit2D);
                      hitToFire2DEventSystem = hit2DEventSystem;

#if ENABLE_DEBUGGING
                                            Log.Debug("TouchEventManager", "Outside - TwoFingerDrag Event Found 2D Event System. itemCollider: {0}, callback: {1}, SortingLayer: {2}, isTapInside: {3} ",hitTransform2DEventSystem, dragEventData.DragCallback, dragEventData.SortingLayer, dragEventData.IsInside);
#endif
                    }
                    else
                    {
                      //do nothing
                    }
                  }
                }
                else
                {
                  //do nothing
                }
              }
#endif
            }
          }
        }

        if (dragEventToFire != null)
          EventManager.Instance.SendEvent(dragEventToFire.DragCallback, twoFingerMoveGesture, hitToFire3D, hitToFire2D, hitToFire2DEventSystem);

        EventManager.Instance.SendEvent("OnDragTwoFingerFullscreen", twoFingerMoveGesture);
      }
    }
    #endregion

    #region TapEvents - Register / UnRegister / Call 
    /// <summary>
    /// Registers the tap event. 
    /// 3d Colliders is the first priority
    /// 2d Collider is the second priority 
    /// 2d UI Event System is the third priority
    /// If object has 3d collider, and 2d collider and an UI element. 
    /// Touch is checking by priority order. 
    /// </summary>
    /// <returns><c>true</c>, if tap event was registered, <c>false</c> otherwise.</returns>
    /// <param name="gameObjectToTouch">Game object to touch.</param>
    /// <param name="callback">Callback.</param>
    /// <param name="SortingLayer">Sorting layer.</param>
    /// <param name="isTapInside">If set to <c>true</c> is tap inside.</param>
    /// <param name="layerMask">Layer mask.</param>
    public bool RegisterTapEvent(GameObject gameObjectToTouch, string callback, int SortingLayer = 0, bool isTapInside = true, LayerMask layerMask = default(LayerMask))
    {
      bool success = false;

      if (gameObjectToTouch != null)
      {
        if (!string.IsNullOrEmpty(callback))
        {
          Collider[] colliderList = gameObjectToTouch.GetComponentsInChildren<Collider>();

          if (colliderList != null && colliderList.Length > 0)
          {
            foreach (Collider itemCollider in colliderList)
            {
              int layerMaskAsKey = (layerMask != default(LayerMask)) ? layerMask.value : (1 << gameObjectToTouch.layer);

#if ENABLE_DEBUGGING
                            Log.Debug("TouchEventManager", "RegisterTapEvent for 3D. itemCollider: {0}, callback: {1}, SortingLayer: {2}, isTapInside: {3}",itemCollider, callback, SortingLayer, isTapInside);
#endif

              if (tapEvents.ContainsKey(layerMaskAsKey))
              {
                tapEvents[layerMaskAsKey].Add(new TouchEventData(itemCollider, callback, SortingLayer, isTapInside));
              }
              else
              {
                tapEvents[layerMaskAsKey] = new List<TouchEventData>() { new TouchEventData(itemCollider, callback, SortingLayer, isTapInside) };
              }
            }

            success = true;
          }
          else
          {
#if ENABLE_DEBUGGING
                        Log.Debug("TouchEventManager", "There is no 3D collider of given gameobjectToTouch");
#endif
          }

          if (!success)
          {
            Collider2D[] colliderList2D = gameObjectToTouch.GetComponentsInChildren<Collider2D>();
            if (colliderList2D != null && colliderList2D.Length > 0)
            {
              foreach (Collider2D itemCollider in colliderList2D)
              {
                int layerMaskAsKey = (layerMask != default(LayerMask)) ? layerMask.value : (1 << gameObjectToTouch.layer);

#if ENABLE_DEBUGGING
                                Log.Debug("TouchEventManager", "RegisterTapEvent For 2D. itemCollider: {0}, callback: {1}, SortingLayer: {2}, isTapInside: {3}",itemCollider, callback, SortingLayer, isTapInside);
#endif

                if (tapEvents.ContainsKey(layerMaskAsKey))
                {
                  tapEvents[layerMaskAsKey].Add(new TouchEventData(itemCollider, callback, SortingLayer, isTapInside));
                }
                else
                {
                  tapEvents[layerMaskAsKey] = new List<TouchEventData>() { new TouchEventData(itemCollider, callback, SortingLayer, isTapInside) };
                }
              }

              success = true;
            }
            else
            {
#if ENABLE_DEBUGGING
                            Log.Debug ("TouchEventManager", "There is no 2D collider of given gameobjectToTouch");
#endif
            }
          }
#if UNITY_4_6 || UNITY_5 || UNITY_5_3_OR_NEWER
          if (!success)
          {
            RectTransform[] rectTransformList = gameObjectToTouch.GetComponentsInChildren<RectTransform>(includeInactive: true);
            if (rectTransformList != null && rectTransformList.Length > 0)
            {
              foreach (RectTransform itemRectTransform in rectTransformList)
              {
                int layerMaskAsKey = (layerMask != default(LayerMask)) ? layerMask.value : (1 << itemRectTransform.gameObject.layer);

#if ENABLE_DEBUGGING
                                Log.Debug("TouchEventManager", "RegisterTapEvent For 2D Event System. itemRectTransform: {0}, callback: {1}, SortingLayer: {2}, isTapInside: {3}",itemRectTransform, callback, SortingLayer, isTapInside);
#endif

                if (tapEvents.ContainsKey(layerMaskAsKey))
                {
                  tapEvents[layerMaskAsKey].Add(new TouchEventData(itemRectTransform, callback, SortingLayer, isTapInside));
                }
                else
                {
                  tapEvents[layerMaskAsKey] = new List<TouchEventData>() { new TouchEventData(itemRectTransform, callback, SortingLayer, isTapInside) };
                }
              }

              success = true;
            }
            else
            {
#if ENABLE_DEBUGGING
                            Log.Debug ("TouchEventManager", "There is no Rect Transform of given gameobjectToTouch");
#endif
            }
          }
#endif

        }
        else
        {
          Log.Warning("TouchEventManager", "There is no callback for tap event registration");
        }
      }
      else
      {
        Log.Warning("TouchEventManager", "There is no gameobject for tap event registration");
      }

      return success;
    }

    /// <summary>
    /// Unregisters the tap event.
    /// </summary>
    /// <returns><c>true</c>, if tap event was unregistered, <c>false</c> otherwise.</returns>
    /// <param name="gameObjectToTouch">Game object to touch.</param>
    /// <param name="callback">Callback.</param>
    /// <param name="SortingLayer">Sorting layer.</param>
    /// <param name="isTapInside">If set to <c>true</c> is tap inside.</param>
    /// <param name="layerMask">Layer mask.</param>
    public bool UnregisterTapEvent(GameObject gameObjectToTouch, string callback, int SortingLayer = 0, bool isTapInside = true, LayerMask layerMask = default(LayerMask))
    {
      bool success = false;

      if (gameObjectToTouch != null)
      {
        if (!string.IsNullOrEmpty(callback))
        {
          int layerMaskAsKey = (layerMask != default(LayerMask)) ? layerMask.value : (1 << gameObjectToTouch.layer);

          if (tapEvents.ContainsKey(layerMaskAsKey))
          {
            success = true;
            Collider[] colliderList = gameObjectToTouch.GetComponentsInChildren<Collider>(includeInactive: true);
            if (colliderList != null && colliderList.Length > 0)
            {
              foreach (Collider itemCollider in colliderList)
              {
                int numberOfRemovedCallbacks = tapEvents[layerMaskAsKey].RemoveAll(
                                                   e =>
                    e.Collider == itemCollider &&
                                                   e.TapCallback == callback &&
                                                   e.SortingLayer == SortingLayer &&
                                                   e.IsInside == isTapInside);

                success &= (numberOfRemovedCallbacks > 0);
              }
            }
            else
            {
              success = false;
            }

            if (!success)
            {
              success = true;
              Collider2D[] colliderList2D = gameObjectToTouch.GetComponentsInChildren<Collider2D>(includeInactive: true);
              if (colliderList2D != null && colliderList2D.Length > 0)
              {
                foreach (Collider2D itemCollider2D in colliderList2D)
                {
                  int numberOfRemovedCallbacks = tapEvents[layerMaskAsKey].RemoveAll(
                      e =>
                      e.Collider2D == itemCollider2D &&
                      e.TapCallback == callback &&
                      e.SortingLayer == SortingLayer &&
                      e.IsInside == isTapInside);

                  success &= (numberOfRemovedCallbacks > 0);
                }
              }
              else
              {
                success = false;
              }
            }


#if UNITY_4_6 || UNITY_5 || UNITY_5_3_OR_NEWER
            if (!success)
            {
              success = true;
              RectTransform[] rectTransformList = gameObjectToTouch.GetComponentsInChildren<RectTransform>(includeInactive: true);
              if (rectTransformList != null && rectTransformList.Length > 0)
              {
                foreach (RectTransform itemRectTransform in rectTransformList)
                {
                  int numberOfRemovedCallbacks = tapEvents[layerMaskAsKey].RemoveAll(
                      e =>
                      e.RectTransform == itemRectTransform &&
                      e.TapCallback == callback &&
                      e.SortingLayer == SortingLayer &&
                      e.IsInside == isTapInside);

                  success &= (numberOfRemovedCallbacks > 0);
                }
              }
              else
              {
                success = false;
              }
            }
#endif
          }
        }
        else
        {
          Log.Warning("TouchEventManager", "There is no callback for tap event unregistration");
        }
      }
      else
      {
        Log.Warning("TouchEventManager", "There is no gameobject for tap event unregistration");
      }

      return success;
    }

    private void TapGesture_Tapped(object sender, System.EventArgs e)
    {
      if (active)
      {
#if ENABLE_DEBUGGING
                Log.Debug("TouchEventManager", "TapGesture_Tapped: {0} - {1}", tapGesture.ScreenPosition, tapGesture.NumTouches);
#endif

        TouchEventData tapEventToFire = null;

        RaycastHit hitToFire3D = default(RaycastHit);
        RaycastHit2D hitToFire2D = default(RaycastHit2D);
#if UNITY_4_6 || UNITY_5 || UNITY_5_3_OR_NEWER
        RaycastResult hitToFire2DEventSystem = default(RaycastResult);
#endif

        foreach (var kp in tapEvents)
        {
          //Adding Variables for 3D Tap Check
          Ray rayForTab = MainCamera.ScreenPointToRay(tapGesture.ScreenPosition);
          Transform hitTransform3D = null;
          RaycastHit hit3D = default(RaycastHit);
          bool isHitOnLayer3D = Physics.Raycast(rayForTab, out hit3D, Mathf.Infinity, kp.Key);
          if (isHitOnLayer3D)
          {
            hitTransform3D = hit3D.collider.transform;
          }

          //Adding Variables for 2D Tap Check for 2d Colliders
          Transform hitTransform2D = null;
          RaycastHit2D hit2D = Physics2D.Raycast(rayForTab.origin, rayForTab.direction, Mathf.Infinity, kp.Key);
          bool isHitOnLayer2D = false;
          if (hit2D.collider != null)
          {
            isHitOnLayer2D = true;
            hitTransform2D = hit2D.collider.transform;
          }

#if UNITY_4_6 || UNITY_5 || UNITY_5_3_OR_NEWER
          //Adding Variables for Event.System Tap for UI Elements
          Transform hitTransform2DEventSystem = null;
          bool isHitOnLayer2DEventSystem = false;
          RaycastResult hit2DEventSystem = default(RaycastResult);
          if (EventSystem.current != null)
          {
            PointerEventData pointerEventForTap = new PointerEventData(EventSystem.current);
            pointerEventForTap.position = tapGesture.ScreenPosition;
            List<RaycastResult> raycastResultListFor2DEventSystem = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerEventForTap, raycastResultListFor2DEventSystem);
            foreach (RaycastResult itemRaycastResult in raycastResultListFor2DEventSystem)
            {
              LayerMask layerMaskOfItem = 1 << itemRaycastResult.gameObject.layer;
              isHitOnLayer2DEventSystem = ((layerMaskOfItem.value & kp.Key) == layerMaskOfItem.value);

              if (isHitOnLayer2DEventSystem)
              {
                hit2DEventSystem = itemRaycastResult;
                hitTransform2DEventSystem = itemRaycastResult.gameObject.transform;
                break;
              }
            }
          }
#endif


          for (int i = 0; i < kp.Value.Count; ++i)
          {
            TouchEventData tapEventData = kp.Value[i];

            if (kp.Value[i].Collider == null && kp.Value[i].Collider2D == null && kp.Value[i].RectTransform == null && kp.Value[i].RectTransformList == null)
            {
              Log.Warning("TouchEventManager", "Removing invalid collider event receiver from TapEventList from {0}", kp.Value[i].ToString());
              kp.Value.RemoveAt(i--);
              continue;
            }

            if (string.IsNullOrEmpty(tapEventData.TapCallback))
            {
              Log.Warning("TouchEventManager", "Removing invalid event receiver from TapEventList {0}", kp.Value[i]);
              kp.Value.RemoveAt(i--);
              continue;
            }

            //3d Hit Check
            if (tapEventData.Collider != null)
            {
              if (isHitOnLayer3D && hitTransform3D == tapEventData.Collider.transform && tapEventData.IsInside)
              {
                //Tapped inside the object
                if (tapEventToFire == null)
                {
                  tapEventToFire = tapEventData;
                  hitToFire3D = hit3D;
                  hitToFire2D = default(RaycastHit2D);
                  hitToFire2DEventSystem = default(RaycastResult);
#if ENABLE_DEBUGGING
                                    Log.Debug("TouchEventManager", "Tap Event Found 3D. itemCollider: {0}, callback: {1}, SortingLayer: {2}, isTapInside: {3} ",hitTransform3D, tapEventData.TapCallback, tapEventData.SortingLayer, tapEventData.IsInside);
#endif
                }
                else
                {
                  if (tapEventData.SortingLayer > tapEventToFire.SortingLayer ||
                      (tapEventToFire.SortingLayer == tapEventData.SortingLayer && !tapEventToFire.IsInside))
                  {
                    tapEventToFire = tapEventData;
                    hitToFire3D = hit3D;
                    hitToFire2D = default(RaycastHit2D);
                    hitToFire2DEventSystem = default(RaycastResult);

#if ENABLE_DEBUGGING
                                        Log.Debug("TouchEventManager", "Tap Event Found 3D. itemCollider: {0}, callback: {1}, SortingLayer: {2}, isTapInside: {3} ",hitTransform3D, tapEventData.TapCallback, tapEventData.SortingLayer, tapEventData.IsInside);
#endif
                  }
                  else
                  {
                    //do nothing
                  }
                }

              }
              else if ((!isHitOnLayer3D || hitTransform3D != tapEventData.Collider.transform) && !tapEventData.IsInside)
              {
                //Tapped outside the object
                if (tapEventToFire == null)
                {
                  tapEventToFire = tapEventData;
                  hitToFire3D = hit3D;
                  hitToFire2D = default(RaycastHit2D);
                  hitToFire2DEventSystem = default(RaycastResult);

#if ENABLE_DEBUGGING
                                    Log.Debug("TouchEventManager", "Tap Event Found 3D. itemCollider: {0}, callback: {1}, SortingLayer: {2}, isTapInside: {3} ",hitTransform3D, tapEventData.TapCallback, tapEventData.SortingLayer, tapEventData.IsInside);
#endif
                }
                else
                {
                  if (tapEventData.SortingLayer > tapEventToFire.SortingLayer)
                  {
                    tapEventToFire = tapEventData;
                    hitToFire3D = hit3D;
                    hitToFire2D = default(RaycastHit2D);
                    hitToFire2DEventSystem = default(RaycastResult);

#if ENABLE_DEBUGGING
                                        Log.Debug("TouchEventManager", "Tap Event Found 3D. itemCollider: {0}, callback: {1}, SortingLayer: {2}, isTapInside: {3} ",hitTransform3D, tapEventData.TapCallback, tapEventData.SortingLayer, tapEventData.IsInside);
#endif
                  }
                  else
                  {
                    //do nothing
                  }
                }
              }
              else
              {
                //do nothing
              }
            }

            //2d Hit Check
            if (tapEventData.Collider2D != null)
            {
              if (isHitOnLayer2D && hitTransform2D == tapEventData.Collider2D.transform && tapEventData.IsInside)
              {
                //Tapped inside the object
                if (tapEventToFire == null)
                {
                  tapEventToFire = tapEventData;
                  hitToFire3D = default(RaycastHit);
                  hitToFire2D = hit2D;
                  hitToFire2DEventSystem = default(RaycastResult);

#if ENABLE_DEBUGGING
                                    Log.Debug("TouchEventManager", "Tap Event Found 2D. itemCollider: {0}, callback: {1}, SortingLayer: {2}, isTapInside: {3} ",hitTransform2D, tapEventData.TapCallback, tapEventData.SortingLayer, tapEventData.IsInside);
#endif
                }
                else
                {
                  if (tapEventData.SortingLayer > tapEventToFire.SortingLayer ||
                      (tapEventToFire.SortingLayer == tapEventData.SortingLayer && !tapEventToFire.IsInside))
                  {
                    tapEventToFire = tapEventData;
                    hitToFire3D = default(RaycastHit);
                    hitToFire2D = hit2D;
                    hitToFire2DEventSystem = default(RaycastResult);

#if ENABLE_DEBUGGING
                                        Log.Debug("TouchEventManager", "Tap Event Found 2D. itemCollider: {0}, callback: {1}, SortingLayer: {2}, isTapInside: {3} ",hitTransform2D, tapEventData.TapCallback, tapEventData.SortingLayer, tapEventData.IsInside);
#endif
                  }
                  else
                  {
                    //do nothing
                  }
                }

              }
              else if ((!isHitOnLayer2D || hitTransform2D != tapEventData.Collider2D.transform) && !tapEventData.IsInside)
              {
                //Tapped outside the object
                if (tapEventToFire == null)
                {
                  tapEventToFire = tapEventData;
                  hitToFire3D = default(RaycastHit);
                  hitToFire2D = hit2D;
                  hitToFire2DEventSystem = default(RaycastResult);

#if ENABLE_DEBUGGING
                                    Log.Debug("TouchEventManager", "Tap Event Found 2D. itemCollider: {0}, callback: {1}, SortingLayer: {2}, isTapInside: {3} ",hitTransform2D, tapEventData.TapCallback, tapEventData.SortingLayer, tapEventData.IsInside);
#endif
                }
                else
                {
                  if (tapEventData.SortingLayer > tapEventToFire.SortingLayer)
                  {
                    tapEventToFire = tapEventData;
                    hitToFire3D = default(RaycastHit);
                    hitToFire2D = hit2D;
                    hitToFire2DEventSystem = default(RaycastResult);

#if ENABLE_DEBUGGING
                                        Log.Debug("TouchEventManager", "Tap Event Found 2D. itemCollider: {0}, callback: {1}, SortingLayer: {2}, isTapInside: {3} ",hitTransform2D, tapEventData.TapCallback, tapEventData.SortingLayer, tapEventData.IsInside);
#endif
                  }
                  else
                  {
                    //do nothing
                  }
                }
              }
              else
              {
                //do nothing
              }
            }

#if UNITY_4_6 || UNITY_5 || UNITY_5_3_OR_NEWER
            //2D UI Hit Check using EventSystem
            if (tapEventData.RectTransform != null)
            {
              if (isHitOnLayer2DEventSystem && hitTransform2DEventSystem == tapEventData.RectTransform.transform && tapEventData.IsInside)
              {
                //Tapped inside the object
                if (tapEventToFire == null)
                {
                  tapEventToFire = tapEventData;
                  hitToFire3D = default(RaycastHit);
                  hitToFire2D = default(RaycastHit2D);
                  hitToFire2DEventSystem = hit2DEventSystem;

#if ENABLE_DEBUGGING
                                    Log.Debug("TouchEventManager", "Tap Event Found 2D Event System. itemCollider: {0}, callback: {1}, SortingLayer: {2}, isTapInside: {3} ",hitTransform2DEventSystem, tapEventData.TapCallback, tapEventData.SortingLayer, tapEventData.IsInside);
#endif
                }
                else
                {
                  if (tapEventData.SortingLayer > tapEventToFire.SortingLayer ||
                      (tapEventToFire.SortingLayer == tapEventData.SortingLayer && !tapEventToFire.IsInside))
                  {
                    tapEventToFire = tapEventData;
                    hitToFire3D = default(RaycastHit);
                    hitToFire2D = default(RaycastHit2D);
                    hitToFire2DEventSystem = hit2DEventSystem;

#if ENABLE_DEBUGGING
                                        Log.Debug("TouchEventManager", "Tap Event Found 2D Event System. itemCollider: {0}, callback: {1}, SortingLayer: {2}, isTapInside: {3} ",hitTransform2DEventSystem, tapEventData.TapCallback, tapEventData.SortingLayer, tapEventData.IsInside);
#endif
                  }
                  else
                  {
                    //do nothing
                  }
                }

              }
              else if ((!isHitOnLayer2DEventSystem || hitTransform2DEventSystem != tapEventData.RectTransform.transform) && !tapEventData.IsInside)
              {
                //Tapped outside the object
                if (tapEventToFire == null)
                {
                  tapEventToFire = tapEventData;
                  hitToFire3D = default(RaycastHit);
                  hitToFire2D = default(RaycastHit2D);
                  hitToFire2DEventSystem = hit2DEventSystem;

#if ENABLE_DEBUGGING
                                    Log.Debug("TouchEventManager", "Tap Event Found 2D Event System. itemCollider: {0}, callback: {1}, SortingLayer: {2}, isTapInside: {3} ",hitTransform2DEventSystem, tapEventData.TapCallback, tapEventData.SortingLayer, tapEventData.IsInside);
#endif
                }
                else
                {
                  if (tapEventData.SortingLayer > tapEventToFire.SortingLayer)
                  {
                    tapEventToFire = tapEventData;
                    hitToFire3D = default(RaycastHit);
                    hitToFire2D = default(RaycastHit2D);
                    hitToFire2DEventSystem = hit2DEventSystem;

#if ENABLE_DEBUGGING
                                        Log.Debug("TouchEventManager", "Tap Event Found 2D Event System. itemCollider: {0}, callback: {1}, SortingLayer: {2}, isTapInside: {3} ",hitTransform2DEventSystem, tapEventData.TapCallback, tapEventData.SortingLayer, tapEventData.IsInside);
#endif
                  }
                  else
                  {
                    //do nothing
                  }
                }
              }
              else
              {
                //do nothing
              }
            }
#endif

          }
        }

        if (tapEventToFire != null)
          EventManager.Instance.SendEvent(tapEventToFire.TapCallback, tapGesture, hitToFire3D, hitToFire2D, hitToFire2DEventSystem);

        EventManager.Instance.SendEvent("OnSingleTap", tapGesture);
      }
    }
    #endregion

    #region Double TapEvents - Register / UnRegister / Call 
    /// <summary>
    /// Registers the double tap event.
    /// </summary>
    /// <returns><c>true</c>, if double tap event was registered, <c>false</c> otherwise.</returns>
    /// <param name="gameObjectToTouch">Game object to touch.</param>
    /// <param name="callback">Callback.</param>
    /// <param name="SortingLayer">Sorting layer.</param>
    /// <param name="isDoubleTapInside">If set to <c>true</c> is double tap inside.</param>
    /// <param name="layerMask">Layer mask.</param>
    public bool RegisterDoubleTapEvent(GameObject gameObjectToTouch, string callback, int SortingLayer = 0, bool isDoubleTapInside = true, LayerMask layerMask = default(LayerMask))
    {
      bool success = false;

      if (gameObjectToTouch != null)
      {
        if (!string.IsNullOrEmpty(callback))
        {
          Collider[] colliderList = gameObjectToTouch.GetComponentsInChildren<Collider>();

          if (colliderList != null && colliderList.Length > 0)
          {
            foreach (Collider itemCollider in colliderList)
            {
              int layerMaskAsKey = (layerMask != default(LayerMask)) ? layerMask.value : (1 << gameObjectToTouch.layer);

#if ENABLE_DEBUGGING
                            Log.Debug("TouchEventManager", "RegisterDoubleTapEvent for 3D. itemCollider: {0}, callback: {1}, SortingLayer: {2}, isTapInside: {3}",itemCollider, callback, SortingLayer, isDoubleTapInside);
#endif

              if (doubleTapEvents.ContainsKey(layerMaskAsKey))
              {
                doubleTapEvents[layerMaskAsKey].Add(new TouchEventData(itemCollider, callback, SortingLayer, isDoubleTapInside));
              }
              else
              {
                doubleTapEvents[layerMaskAsKey] = new List<TouchEventData>() { new TouchEventData(itemCollider, callback, SortingLayer, isDoubleTapInside) };
              }
            }

            success = true;
          }
          else
          {
            Log.Warning("TouchEventManager", "There is no 3D collider of given gameobjectToTouch (double-tap)");
          }

          if (!success)
          {
            Collider2D[] colliderList2D = gameObjectToTouch.GetComponentsInChildren<Collider2D>();
            if (colliderList2D != null && colliderList2D.Length > 0)
            {
              foreach (Collider2D itemCollider in colliderList2D)
              {
                int layerMaskAsKey = (layerMask != default(LayerMask)) ? layerMask.value : (1 << gameObjectToTouch.layer);

#if ENABLE_DEBUGGING
                                Log.Debug("TouchEventManager", "RegisterDoubleTapEvent For 2D. itemCollider: {0}, callback: {1}, SortingLayer: {2}, isTapInside: {3}",itemCollider, callback, SortingLayer, isDoubleTapInside);
#endif

                if (doubleTapEvents.ContainsKey(layerMaskAsKey))
                {
                  doubleTapEvents[layerMaskAsKey].Add(new TouchEventData(itemCollider, callback, SortingLayer, isDoubleTapInside));
                }
                else
                {
                  doubleTapEvents[layerMaskAsKey] = new List<TouchEventData>() { new TouchEventData(itemCollider, callback, SortingLayer, isDoubleTapInside) };
                }
              }

              success = true;
            }
            else
            {
              Log.Warning("TouchEventManager", "There is no 2D collider of given gameobjectToTouch (double-tap)");
            }
          }

#if UNITY_4_6 || UNITY_5 || UNITY_5_3_OR_NEWER
          if (!success)
          {
            RectTransform[] rectTransformList = gameObjectToTouch.GetComponentsInChildren<RectTransform>(includeInactive: true);
            if (rectTransformList != null && rectTransformList.Length > 0)
            {
              foreach (RectTransform itemRectTransform in rectTransformList)
              {
                int layerMaskAsKey = (layerMask != default(LayerMask)) ? layerMask.value : (1 << itemRectTransform.gameObject.layer);

#if ENABLE_DEBUGGING
                                Log.Debug("TouchEventManager", "RegisterDoubleTapEvent For 2D Event System. itemRectTransform: {0}, callback: {1}, SortingLayer: {2}, isTapInside: {3}",itemRectTransform, callback, SortingLayer, isDoubleTapInside);
#endif

                if (doubleTapEvents.ContainsKey(layerMaskAsKey))
                {
                  doubleTapEvents[layerMaskAsKey].Add(new TouchEventData(itemRectTransform, callback, SortingLayer, isDoubleTapInside));
                }
                else
                {
                  doubleTapEvents[layerMaskAsKey] = new List<TouchEventData>() { new TouchEventData(itemRectTransform, callback, SortingLayer, isDoubleTapInside) };
                }
              }

              success = true;
            }
            else
            {
              Log.Warning("TouchEventManager", "There is no Rect Transform of given gameobjectToTouch (double-tap)");
            }
          }
#endif

        }
        else
        {
          Log.Warning("TouchEventManager", "There is no callback for double-tap event registration");
        }
      }
      else
      {
        Log.Warning("TouchEventManager", "There is no gameobject for double-tap event registration");
      }

      return success;
    }

    /// <summary>
    /// Unregisters the double tap event.
    /// </summary>
    /// <returns><c>true</c>, if double tap event was unregistered, <c>false</c> otherwise.</returns>
    /// <param name="gameObjectToTouch">Game object to touch.</param>
    /// <param name="callback">Callback.</param>
    /// <param name="SortingLayer">Sorting layer.</param>
    /// <param name="isDoubleTapInside">If set to <c>true</c> is double tap inside.</param>
    /// <param name="layerMask">Layer mask.</param>
    public bool UnregisterDoubleTapEvent(GameObject gameObjectToTouch, string callback, int SortingLayer = 0, bool isDoubleTapInside = true, LayerMask layerMask = default(LayerMask))
    {
      bool success = false;

      if (gameObjectToTouch != null)
      {
        if (!string.IsNullOrEmpty(callback))
        {
          int layerMaskAsKey = (layerMask != default(LayerMask)) ? layerMask.value : (1 << gameObjectToTouch.layer);

          if (doubleTapEvents.ContainsKey(layerMaskAsKey))
          {
            success = true;
            Collider[] colliderList = gameObjectToTouch.GetComponentsInChildren<Collider>(includeInactive: true);
            if (colliderList != null && colliderList.Length > 0)
            {
              foreach (Collider itemCollider in colliderList)
              {
                int numberOfRemovedCallbacks = doubleTapEvents[layerMaskAsKey].RemoveAll(
                    e =>
                    e.Collider == itemCollider &&
                    e.TapCallback == callback &&
                    e.SortingLayer == SortingLayer &&
                    e.IsInside == isDoubleTapInside);

                success &= (numberOfRemovedCallbacks > 0);
              }
            }
            else
            {
              success = false;
            }

            if (!success)
            {
              success = true;
              Collider2D[] colliderList2D = gameObjectToTouch.GetComponentsInChildren<Collider2D>(includeInactive: true);
              if (colliderList2D != null && colliderList2D.Length > 0)
              {
                foreach (Collider2D itemCollider2D in colliderList2D)
                {
                  int numberOfRemovedCallbacks = doubleTapEvents[layerMaskAsKey].RemoveAll(
                      e =>
                      e.Collider2D == itemCollider2D &&
                      e.TapCallback == callback &&
                      e.SortingLayer == SortingLayer &&
                      e.IsInside == isDoubleTapInside);

                  success &= (numberOfRemovedCallbacks > 0);
                }
              }
              else
              {
                success = false;
              }

            }
#if UNITY_4_6 || UNITY_5 || UNITY_5_3_OR_NEWER
            if (!success)
            {
              success = true;
              RectTransform[] rectTransformList = gameObjectToTouch.GetComponentsInChildren<RectTransform>(includeInactive: true);
              if (rectTransformList != null && rectTransformList.Length > 0)
              {
                foreach (RectTransform itemRectTransform in rectTransformList)
                {
                  int numberOfRemovedCallbacks = doubleTapEvents[layerMaskAsKey].RemoveAll(
                      e =>
                      e.RectTransform == itemRectTransform &&
                      e.TapCallback == callback &&
                      e.SortingLayer == SortingLayer &&
                      e.IsInside == isDoubleTapInside);

                  success &= (numberOfRemovedCallbacks > 0);
                }
              }
              else
              {
                success = false;
              }
            }
#endif
          }
        }
        else
        {
          Log.Warning("TouchEventManager", "There is no callback for double-tap event unregistration");
        }
      }
      else
      {
        Log.Warning("TouchEventManager", "There is no gameobject for double-tap event unregistration");
      }

      return success;
    }


    private void DoubleTapGesture_Tapped(object sender, System.EventArgs e)
    {
      if (active)
      {
#if ENABLE_DEBUGGING
                Log.Debug("TouchEventManager", "DoubleTapGesture_Tapped: {0} - {1}", doubleTapGesture.ScreenPosition, doubleTapGesture.NumTouches);
#endif

        TouchEventData tapEventToFire = null;

        RaycastHit hitToFire3D = default(RaycastHit);
        RaycastHit2D hitToFire2D = default(RaycastHit2D);
#if UNITY_4_6 || UNITY_5 || UNITY_5_3_OR_NEWER
        RaycastResult hitToFire2DEventSystem = default(RaycastResult);
#endif

        foreach (var kp in doubleTapEvents)
        {
          //Adding Variables for 3D Tap Check
          Ray rayForTab = MainCamera.ScreenPointToRay(doubleTapGesture.ScreenPosition);
          Transform hitTransform3D = null;
          RaycastHit hit3D = default(RaycastHit);
          bool isHitOnLayer3D = Physics.Raycast(rayForTab, out hit3D, Mathf.Infinity, kp.Key);
          if (isHitOnLayer3D)
          {
            hitTransform3D = hit3D.collider.transform;
          }

          //Adding Variables for 2D Tap Check for 2d Colliders
          Transform hitTransform2D = null;
          RaycastHit2D hit2D = Physics2D.Raycast(rayForTab.origin, rayForTab.direction, Mathf.Infinity, kp.Key);
          bool isHitOnLayer2D = false;
          if (hit2D.collider != null)
          {
            isHitOnLayer2D = true;
            hitTransform2D = hit2D.collider.transform;
          }

#if UNITY_4_6 || UNITY_5 || UNITY_5_3_OR_NEWER
          //Adding Variables for Event.System Tap for UI Elements
          Transform hitTransform2DEventSystem = null;
          bool isHitOnLayer2DEventSystem = false;
          RaycastResult hit2DEventSystem = default(RaycastResult);
          if (EventSystem.current != null)
          {
            PointerEventData pointerEventForTap = new PointerEventData(EventSystem.current);
            pointerEventForTap.position = doubleTapGesture.ScreenPosition;
            List<RaycastResult> raycastResultListFor2DEventSystem = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerEventForTap, raycastResultListFor2DEventSystem);
            foreach (RaycastResult itemRaycastResult in raycastResultListFor2DEventSystem)
            {

              LayerMask layerMaskOfItem = 1 << itemRaycastResult.gameObject.layer;
              isHitOnLayer2DEventSystem = ((layerMaskOfItem.value & kp.Key) == layerMaskOfItem.value);
              if (isHitOnLayer2DEventSystem)
              {
                hit2DEventSystem = itemRaycastResult;
                hitTransform2DEventSystem = itemRaycastResult.gameObject.transform;
                break;
              }
            }
          }
#endif


          for (int i = 0; i < kp.Value.Count; ++i)
          {
            TouchEventData tapEventData = kp.Value[i];

            if (kp.Value[i].Collider == null && kp.Value[i].Collider2D == null && kp.Value[i].RectTransform == null && kp.Value[i].RectTransformList == null)
            {
              Log.Warning("TouchEventManager", "Removing invalid collider event receiver from DoubleTapEventList");
              kp.Value.RemoveAt(i--);
              continue;
            }

            if (string.IsNullOrEmpty(tapEventData.TapCallback))
            {
              Log.Warning("TouchEventManager", "Removing invalid event receiver from DoubleTapEventList");
              kp.Value.RemoveAt(i--);
              continue;
            }

            //3d Hit Check
            if (tapEventData.Collider != null)
            {
              if (isHitOnLayer3D && hitTransform3D == tapEventData.Collider.transform && tapEventData.IsInside)
              {
                //Tapped inside the object
                if (tapEventToFire == null)
                {
                  tapEventToFire = tapEventData;
                  hitToFire3D = hit3D;
                  hitToFire2D = default(RaycastHit2D);
                  hitToFire2DEventSystem = default(RaycastResult);
#if ENABLE_DEBUGGING
                                    Log.Debug("TouchEventManager", "Double-Tap Event Found 3D. itemCollider: {0}, callback: {1}, SortingLayer: {2}, isTapInside: {3} ",hitTransform3D, tapEventData.TapCallback, tapEventData.SortingLayer, tapEventData.IsInside);
#endif
                }
                else
                {
                  if (tapEventData.SortingLayer > tapEventToFire.SortingLayer ||
                      (tapEventToFire.SortingLayer == tapEventData.SortingLayer && !tapEventToFire.IsInside))
                  {
                    tapEventToFire = tapEventData;
                    hitToFire3D = hit3D;
                    hitToFire2D = default(RaycastHit2D);
                    hitToFire2DEventSystem = default(RaycastResult);

#if ENABLE_DEBUGGING
                                        Log.Debug("TouchEventManager", "Double-Tap Event Found 3D. itemCollider: {0}, callback: {1}, SortingLayer: {2}, isTapInside: {3} ",hitTransform3D, tapEventData.TapCallback, tapEventData.SortingLayer, tapEventData.IsInside);
#endif
                  }
                  else
                  {
                    //do nothing
                  }
                }

              }
              else if ((!isHitOnLayer3D || hitTransform3D != tapEventData.Collider.transform) && !tapEventData.IsInside)
              {
                //Tapped outside the object
                if (tapEventToFire == null)
                {
                  tapEventToFire = tapEventData;
                  hitToFire3D = hit3D;
                  hitToFire2D = default(RaycastHit2D);
                  hitToFire2DEventSystem = default(RaycastResult);

#if ENABLE_DEBUGGING
                                    Log.Debug("TouchEventManager", "Double-Tap Event Found 3D. itemCollider: {0}, callback: {1}, SortingLayer: {2}, isTapInside: {3} ",hitTransform3D, tapEventData.TapCallback, tapEventData.SortingLayer, tapEventData.IsInside);
#endif
                }
                else
                {
                  if (tapEventData.SortingLayer > tapEventToFire.SortingLayer)
                  {
                    tapEventToFire = tapEventData;
                    hitToFire3D = hit3D;
                    hitToFire2D = default(RaycastHit2D);
                    hitToFire2DEventSystem = default(RaycastResult);

#if ENABLE_DEBUGGING
                                        Log.Debug("TouchEventManager", "Double-Tap Event Found 3D. itemCollider: {0}, callback: {1}, SortingLayer: {2}, isTapInside: {3} ",hitTransform3D, tapEventData.TapCallback, tapEventData.SortingLayer, tapEventData.IsInside);
#endif
                  }
                  else
                  {
                    //do nothing
                  }
                }
              }
              else
              {
                //do nothing
              }
            }

            //2d Hit Check
            if (tapEventData.Collider2D != null)
            {
              if (isHitOnLayer2D && hitTransform2D == tapEventData.Collider2D.transform && tapEventData.IsInside)
              {
                //Tapped inside the object
                if (tapEventToFire == null)
                {
                  tapEventToFire = tapEventData;
                  hitToFire3D = default(RaycastHit);
                  hitToFire2D = hit2D;
                  hitToFire2DEventSystem = default(RaycastResult);

#if ENABLE_DEBUGGING
                                    Log.Debug("TouchEventManager", "Double-Tap Event Found 2D. itemCollider: {0}, callback: {1}, SortingLayer: {2}, isTapInside: {3} ",hitTransform2D, tapEventData.TapCallback, tapEventData.SortingLayer, tapEventData.IsInside);
#endif
                }
                else
                {
                  if (tapEventData.SortingLayer > tapEventToFire.SortingLayer ||
                      (tapEventToFire.SortingLayer == tapEventData.SortingLayer && !tapEventToFire.IsInside))
                  {
                    tapEventToFire = tapEventData;
                    hitToFire3D = default(RaycastHit);
                    hitToFire2D = hit2D;
                    hitToFire2DEventSystem = default(RaycastResult);

#if ENABLE_DEBUGGING
                                        Log.Debug("TouchEventManager", "Double-Tap Event Found 2D. itemCollider: {0}, callback: {1}, SortingLayer: {2}, isTapInside: {3} ",hitTransform2D, tapEventData.TapCallback, tapEventData.SortingLayer, tapEventData.IsInside);
#endif
                  }
                  else
                  {
                    //do nothing
                  }
                }

              }
              else if ((!isHitOnLayer2D || hitTransform2D != tapEventData.Collider2D.transform) && !tapEventData.IsInside)
              {
                //Tapped outside the object
                if (tapEventToFire == null)
                {
                  tapEventToFire = tapEventData;
                  hitToFire3D = default(RaycastHit);
                  hitToFire2D = hit2D;
                  hitToFire2DEventSystem = default(RaycastResult);

#if ENABLE_DEBUGGING
                                    Log.Debug("TouchEventManager", "Double-Tap Event Found 2D. itemCollider: {0}, callback: {1}, SortingLayer: {2}, isTapInside: {3} ",hitTransform2D, tapEventData.TapCallback, tapEventData.SortingLayer, tapEventData.IsInside);
#endif
                }
                else
                {
                  if (tapEventData.SortingLayer > tapEventToFire.SortingLayer)
                  {
                    tapEventToFire = tapEventData;
                    hitToFire3D = default(RaycastHit);
                    hitToFire2D = hit2D;
                    hitToFire2DEventSystem = default(RaycastResult);

#if ENABLE_DEBUGGING
                                        Log.Debug("TouchEventManager", "Double-Tap Event Found 2D. itemCollider: {0}, callback: {1}, SortingLayer: {2}, isTapInside: {3} ",hitTransform2D, tapEventData.TapCallback, tapEventData.SortingLayer, tapEventData.IsInside);
#endif
                  }
                  else
                  {
                    //do nothing
                  }
                }
              }
              else
              {
                //do nothing
              }
            }

#if UNITY_4_6 || UNITY_5 || UNITY_5_3_OR_NEWER
            //2D UI Hit Check using EventSystem
            if (tapEventData.RectTransform != null)
            {
              if (isHitOnLayer2DEventSystem && hitTransform2DEventSystem == tapEventData.RectTransform.transform && tapEventData.IsInside)
              {
                //Tapped inside the object
                if (tapEventToFire == null)
                {
                  tapEventToFire = tapEventData;
                  hitToFire3D = default(RaycastHit);
                  hitToFire2D = default(RaycastHit2D);
                  hitToFire2DEventSystem = hit2DEventSystem;

#if ENABLE_DEBUGGING
                                    Log.Debug("TouchEventManager", "Double-Tap Event Found 2D Event System. itemCollider: {0}, callback: {1}, SortingLayer: {2}, isTapInside: {3} ",hitTransform2DEventSystem, tapEventData.TapCallback, tapEventData.SortingLayer, tapEventData.IsInside);
#endif
                }
                else
                {
                  if (tapEventData.SortingLayer > tapEventToFire.SortingLayer ||
                      (tapEventToFire.SortingLayer == tapEventData.SortingLayer && !tapEventToFire.IsInside))
                  {
                    tapEventToFire = tapEventData;
                    hitToFire3D = default(RaycastHit);
                    hitToFire2D = default(RaycastHit2D);
                    hitToFire2DEventSystem = hit2DEventSystem;

#if ENABLE_DEBUGGING
                                        Log.Debug("TouchEventManager", "Double-Tap Event Found 2D Event System. itemCollider: {0}, callback: {1}, SortingLayer: {2}, isTapInside: {3} ",hitTransform2DEventSystem, tapEventData.TapCallback, tapEventData.SortingLayer, tapEventData.IsInside);
#endif
                  }
                  else
                  {
                    //do nothing
                  }
                }

              }
              else if ((!isHitOnLayer2DEventSystem || hitTransform2DEventSystem != tapEventData.RectTransform.transform) && !tapEventData.IsInside)
              {
                //Tapped outside the object
                if (tapEventToFire == null)
                {
                  tapEventToFire = tapEventData;
                  hitToFire3D = default(RaycastHit);
                  hitToFire2D = default(RaycastHit2D);
                  hitToFire2DEventSystem = hit2DEventSystem;

#if ENABLE_DEBUGGING
                                    Log.Debug("TouchEventManager", "Double-Tap Event Found 2D Event System. itemCollider: {0}, callback: {1}, SortingLayer: {2}, isTapInside: {3} ",hitTransform2DEventSystem, tapEventData.TapCallback, tapEventData.SortingLayer, tapEventData.IsInside);
#endif
                }
                else
                {
                  if (tapEventData.SortingLayer > tapEventToFire.SortingLayer)
                  {
                    tapEventToFire = tapEventData;
                    hitToFire3D = default(RaycastHit);
                    hitToFire2D = default(RaycastHit2D);
                    hitToFire2DEventSystem = hit2DEventSystem;

#if ENABLE_DEBUGGING
                                        Log.Debug("TouchEventManager", "Double-Tap Event Found 2D Event System. itemCollider: {0}, callback: {1}, SortingLayer: {2}, isTapInside: {3} ",hitTransform2DEventSystem, tapEventData.TapCallback, tapEventData.SortingLayer, tapEventData.IsInside);
#endif
                  }
                  else
                  {
                    //do nothing
                  }
                }
              }
              else
              {
                //do nothing
              }
            }
#endif

          }
        }

        if (tapEventToFire != null)
          EventManager.Instance.SendEvent(tapEventToFire.TapCallback, doubleTapGesture, hitToFire3D, hitToFire2D, hitToFire2DEventSystem);

        EventManager.Instance.SendEvent("OnDoubleTap", doubleTapGesture);
      }
    }

    #endregion


    #region Three Tap Gesture Call

    private void ThreeTapGesture_Tapped(object sender, System.EventArgs e)
    {
      if (active)
      {
#if ENABLE_DEBUGGING
                Log.Debug("TouchEventManager", "ThreeTapGesture_Tapped: {0} - {1}", threeTapGesture.ScreenPosition, threeTapGesture.NumTouches);
#endif
        EventManager.Instance.SendEvent("OnTripleTap", threeTapGesture);
      }
    }

    #endregion

    #region PressGesture Events -  Call - There is no registration is sends automatically the press event

    private void PressGesturePressed(object sender, System.EventArgs e)
    {
#if ENABLE_DEBUGGING
            Log.Debug("TouchEventManager", "PressGesturePressed: {0} - {1}", pressGesture.ScreenPosition, pressGesture.NumTouches);
#endif

      EventManager.Instance.SendEvent("OnTouchPressedFullscreen", pressGesture);
    }

    #endregion

    #region Long Press Gesture Events

    private void LongPressGesturePressed(object sender, System.EventArgs e)
    {
#if ENABLE_DEBUGGING
            Log.Debug("TouchEventManager", "LongPressGesturePressed: {0} - {1}", longPressGesture.ScreenPosition, longPressGesture.NumTouches);
#endif

      EventManager.Instance.SendEvent("OnLongPressOneFinger", longPressGesture);
    }

    #endregion

    #region ReleaseGesture Events - Call - There is no registration is sends automatically the release event

    private void ReleaseGestureReleased(object sender, System.EventArgs e)
    {
#if ENABLE_DEBUGGING
            Log.Debug("TouchEventManager", "ReleaseGestureReleased: {0} - {1}", releaseGesture.ScreenPosition, releaseGesture.NumTouches);
#endif

      EventManager.Instance.SendEvent("OnTouchReleasedFullscreen", releaseGesture);
    }
    #endregion
  }

}
