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

#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;
using IBM.Watson.DeveloperCloud.Widgets;
using System.Collections.Generic;

namespace IBM.Watson.DeveloperCloud.Editor
{
    public static class WidgetConnector
    {
        [MenuItem("Watson/Widgets/Resolve Connections", false, 4)]
        private static void AutoConnectWidgets()
        {
            Widget[] widgets = Object.FindObjectsOfType<Widget>();
            foreach (var widget in widgets)
                widget.ResolveConnections();
        }
    };

    [CustomPropertyDrawer(typeof(Widget.Input))]
    public class WidgetInputDrawer : PropertyDrawer
    {
        const float rows = 3;
        private bool isExpanded = true;
        private static Dictionary<string, bool> expandedStates = new Dictionary<string, bool>();

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (!expandedStates.ContainsKey(property.propertyPath))
                expandedStates[property.propertyPath] = true;

            if (expandedStates[property.propertyPath])
                return base.GetPropertyHeight(property, label) * rows;
            else
                return base.GetPropertyHeight(property, label);
        }

        public override void OnGUI(Rect pos, SerializedProperty property, GUIContent label)
        {
            Widget.Input target = DrawerHelper.GetParent(property) as Widget.Input;
            if (target == null)
                return;
            if (target.Owner == null)
                target.Owner = property.serializedObject.targetObject as Widget;

            //EditorGUIUtility.LookLikeControls();
            bool expanded_state = expandedStates[property.propertyPath];
            bool expanded = EditorGUI.Foldout(isExpanded ? new Rect(pos.x, pos.y, pos.width / 2, pos.height / rows) : pos, expanded_state, label);
            if (expanded_state)
            {
                EditorGUI.indentLevel += 1;
                EditorGUI.LabelField(new Rect(pos.x, pos.y += pos.height / rows, pos.width, pos.height / rows),
                    "Input Name: " + target.FullInputName);
                EditorGUI.LabelField(new Rect(pos.x, pos.y += pos.height / rows, pos.width, pos.height / rows),
                    "Data Type: " + target.DataTypeName);
                EditorGUI.indentLevel -= 1;
            }
            expandedStates[property.propertyPath] = expanded;
        }
    }
}

#endif
