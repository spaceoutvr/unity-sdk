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

using IBM.Watson.DeveloperCloud.Utilities;
using IBM.Watson.DeveloperCloud.Services.NaturalLanguageClassifier.v1;
using IBM.Watson.DeveloperCloud.Logging;
using UnityEditor;
using UnityEngine;
using System.IO;
using System;
using System.Collections.Generic;
using System.Text;
using FullSerializer;

namespace IBM.Watson.DeveloperCloud.Editor
{

    class NaturalLanguageClassifierEditor : EditorWindow
    {
        #region Private Types
        private class ClassifierData
        {
            [fsIgnore]
            public string FileName { get; set; }
            public bool Expanded { get; set; }
            public bool InstancesExpanded { get; set; }
            public bool ClassesExpanded { get; set; }
            public string Name { get; set; }
            public string Language { get; set; }
            public Dictionary<string, List<string>> Data { get; set; }
            public Dictionary<string, bool> DataExpanded { get; set; }

            public void Import(string filename)
            {
                if (Data == null)
                    Data = new Dictionary<string, List<string>>();

                string[] lines = File.ReadAllLines(filename);
                foreach (var line in lines)
                {
                    int nSeperator = line.LastIndexOf(',');
                    if (nSeperator < 0)
                        continue;

                    string c = line.Substring(nSeperator + 1);
                    string phrase = line.Substring(0, nSeperator);

                    if (!Data.ContainsKey(c))
                        Data[c] = new List<string>();
                    Data[c].Add(phrase);
                }
            }

            public string Export()
            {
                StringBuilder sb = new StringBuilder();
                foreach (var kp in Data)
                {
                    foreach (var p in kp.Value)
                    {
                        sb.Append(p + "," + kp.Key + "\n");
                    }
                }

                return sb.ToString();
            }

            public void Save(string filename)
            {
                fsData data = null;
                fsResult r = sserializer.TrySerialize(typeof(ClassifierData), this, out data);
                if (!r.Succeeded)
                    throw new Exception("Failed to serialize ClassifierData: " + r.FormattedMessages);

                File.WriteAllText(filename, fsJsonPrinter.PrettyJson(data));
                FileName = filename;

                AssetDatabase.Refresh();
            }

            public void Save()
            {
                Save(FileName);
            }

            public bool Load(string filename)
            {
                try
                {
                    string json = File.ReadAllText(filename);

                    fsData data = null;
                    fsResult r = fsJsonParser.Parse(json, out data);
                    if (!r.Succeeded)
                        throw new Exception(r.FormattedMessages);

                    object obj = this;
                    r = sserializer.TryDeserialize(data, obj.GetType(), ref obj);
                    if (!r.Succeeded)
                        throw new Exception(r.FormattedMessages);
                }
                catch (Exception e)
                {
                    Log.Error("NaturalLanguageClassifierEditor", "Failed to load classifier data {1}: {0}", e.ToString(), filename);
                    return false;
                }

                FileName = filename;
                return true;
            }
        };
        #endregion

        private void OnEnable()
        {
#if UNITY_5
            titleContent.text = "Natural Language Classifier Editor";
#endif
            watsonIcon = (Texture2D)Resources.Load(Constants.Resources.WATSON_ICON, typeof(Texture2D));

            Runnable.EnableRunnableInEditor();
        }

        [MenuItem("Watson/Natural Language Classifier Editor", false, 2)]
        private static void EditConfig()
        {
            GetWindow<NaturalLanguageClassifierEditor>().Show();
        }

        private string classifiersFolder = null;
        private Texture watsonIcon = null;
        private Vector2 scrollPos = Vector2.zero;
        private NaturalLanguageClassifier naturalLanguageClassifier = new NaturalLanguageClassifier();
        private Classifiers classifiers = null;
        private static fsSerializer sserializer = new fsSerializer();
        private List<ClassifierData> classifierData = null;
        private string newClassifierName = null;
        private string newClassifierLang = "en";
        private bool refreshing = false;

        private void OnGetClassifiers(Classifiers receivedClassifiers)
        {
            refreshing = false;
            classifiers = receivedClassifiers;
            foreach (var c in classifiers.classifiers)
            {
                naturalLanguageClassifier.GetClassifier(c.classifier_id, OnGetClassifier);
            }
        }

        private void OnGetClassifier(Classifier details)
        {
            foreach (var c in classifiers.classifiers)
                if (c.classifier_id == details.classifier_id)
                {
                    c.status = details.status;
                    c.status_description = details.status_description;
                }
        }

        private void OnDeleteClassifier(bool success)
        {
            if (!success)
                Log.Error("Natural Language Classifier Trainer", "Failed to delete classifier.");
            else
                OnRefresh();
        }

        private void OnClassiferTrained(Classifier classifier)
        {
            if (classifier == null)
                EditorUtility.DisplayDialog("ERROR", "Failed to train classifier.", "OK");
            else
                OnRefresh();
        }

        private static string FindDirectory(string check, string name)
        {
            foreach (var d in Directory.GetDirectories(check))
            {
                string dir = d.Replace("\\", "/");        // normalize the slashes
                if (dir.EndsWith(name))
                    return d;

                string found = FindDirectory(d, name);
                if (found != null)
                    return found;
            }

            return null;
        }

        private void OnRefresh()
        {
            if (!refreshing)
            {
                classifiersFolder = Path.Combine(Application.dataPath, Config.Instance.ClassifierDirectory);
                if (!Directory.Exists(classifiersFolder))
                    Directory.CreateDirectory(classifiersFolder);

                classifierData = new List<ClassifierData>();
                foreach (var file in Directory.GetFiles(classifiersFolder, "*.json"))
                {
                    ClassifierData data = new ClassifierData();
                    if (data.Load(file))
                        classifierData.Add(data);
                }

                if (!naturalLanguageClassifier.GetClassifiers(OnGetClassifiers))
                    Log.Error("Natural Language Classifier Trainer", "Failed to request classifiers, please make sure your NaturalLanguageClassifierV1 service has credentials configured.");
                else
                    refreshing = true;
            }
        }

        private string newClassName = string.Empty;
        private string newPhrase = string.Empty;
        private bool displayClassifiers = false;
        private bool m_handleRepaintError = false;

        private void OnGUI()
        {
            if (Event.current.type == EventType.repaint && !m_handleRepaintError)
            {
                m_handleRepaintError = true;
                return;
            }

            GUILayout.Label(watsonIcon);

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
            EditorGUILayout.BeginVertical();

            if (refreshing)
            {
                EditorGUI.BeginDisabledGroup(true);
                GUILayout.Button("Refreshing...");
                EditorGUI.EndDisabledGroup();
            }
            else if (classifierData == null || GUILayout.Button("Refresh"))
                OnRefresh();

            EditorGUILayout.LabelField("Classifiers:");
            //EditorGUI.indentLevel += 1;

            if (classifierData != null)
            {
                ClassifierData deleteClassifier = null;
                foreach (var data in classifierData)
                {
                    EditorGUILayout.BeginHorizontal();

                    bool expanded = data.Expanded;
                    data.Expanded = EditorGUILayout.Foldout(expanded, data.Name + " [Language: " + data.Language + "]");
                    if (data.Expanded != expanded)
                        data.Save();

                    if (GUILayout.Button("Import", GUILayout.Width(100)))
                    {
                        var path = EditorUtility.OpenFilePanel("Select Training File", "", "csv");
                        if (!string.IsNullOrEmpty(path))
                        {
                            try
                            {
                                data.Import(path);
                            }
                            catch
                            {
                                EditorUtility.DisplayDialog("Error", "Failed to load training data: " + path, "OK");
                            }
                        }
                    }
                    if (GUILayout.Button("Export", GUILayout.Width(100)))
                    {
                        var path = EditorUtility.SaveFilePanel("Export Training file", Application.dataPath, "", "csv");
                        if (!string.IsNullOrEmpty(path))
                            File.WriteAllText(path, data.Export());
                    }
                    if (GUILayout.Button("Save", GUILayout.Width(100)))
                        data.Save();
                    if (GUILayout.Button("Delete", GUILayout.Width(100)))
                    {
                        if (EditorUtility.DisplayDialog("Confirm", "Please confirm you want to delete classifier: " + data.Name, "Yes", "No"))
                            deleteClassifier = data;
                    }
                    if (GUILayout.Button("Train", GUILayout.Width(100)))
                    {
                        string classifierName = data.Name + "/" + DateTime.Now.ToString();

                        if (EditorUtility.DisplayDialog("Confirm", "Please confirm you want to train a new instance: " + classifierName, "Yes", "No"))
                        {
                            if (!naturalLanguageClassifier.TrainClassifier(classifierName, data.Language, data.Export(), OnClassiferTrained))
                                EditorUtility.DisplayDialog("Error", "Failed to train classifier.", "OK");
                        }
                    }
                    EditorGUILayout.EndHorizontal();

                    if (expanded)
                    {
                        EditorGUI.indentLevel += 1;

                        bool instancesExpanded = data.InstancesExpanded;
                        data.InstancesExpanded = EditorGUILayout.Foldout(instancesExpanded, "Instances");
                        if (instancesExpanded != data.InstancesExpanded)
                            data.Save();

                        if (instancesExpanded)
                        {
                            EditorGUI.indentLevel += 1;
                            if (classifiers != null)
                            {
                                for (int i = 0; i < classifiers.classifiers.Length; ++i)
                                {
                                    Classifier cl = classifiers.classifiers[i];
                                    if (!cl.name.StartsWith(data.Name + "/"))
                                        continue;

                                    EditorGUILayout.BeginHorizontal();
                                    EditorGUILayout.LabelField("Name: " + cl.name);
                                    if (GUILayout.Button("Delete", GUILayout.Width(100)))
                                    {
                                        if (EditorUtility.DisplayDialog("Confirm", string.Format("Confirm delete of classifier {0}", cl.classifier_id), "YES", "NO")
                                            && !naturalLanguageClassifier.DeleteClassifer(cl.classifier_id, OnDeleteClassifier))
                                        {
                                            EditorUtility.DisplayDialog("Error", "Failed to delete classifier.", "OK");
                                        }
                                    }
                                    EditorGUILayout.EndHorizontal();

                                    EditorGUI.indentLevel += 1;
                                    EditorGUILayout.LabelField("ID: " + cl.classifier_id);
                                    EditorGUILayout.LabelField("Created: " + cl.created.ToString());
                                    EditorGUILayout.LabelField("Status: " + cl.status);

                                    EditorGUI.indentLevel -= 1;
                                }
                            }
                            EditorGUI.indentLevel -= 1;
                        }

                        if (data.Data == null)
                            data.Data = new Dictionary<string, List<string>>();
                        if (data.DataExpanded == null)
                            data.DataExpanded = new Dictionary<string, bool>();

                        bool classesExpanded = data.ClassesExpanded;
                        data.ClassesExpanded = EditorGUILayout.Foldout(classesExpanded, "Classes");
                        if (classesExpanded != data.ClassesExpanded)
                            data.Save();

                        if (classesExpanded)
                        {
                            EditorGUI.indentLevel += 1;

                            EditorGUILayout.BeginHorizontal();
                            newClassName = EditorGUILayout.TextField(newClassName);
                            EditorGUI.BeginDisabledGroup(string.IsNullOrEmpty(newClassName));

                            GUI.SetNextControlName("AddClass");
                            if (GUILayout.Button("Add Class", GUILayout.Width(100)))
                            {
                                data.Data[newClassName] = new List<string>();
                                data.Save();

                                newClassName = string.Empty;
                                GUI.FocusControl("AddClass");
                            }
                            EditorGUI.EndDisabledGroup();
                            EditorGUILayout.EndHorizontal();

                            string deleteClass = string.Empty;
                            foreach (var kp in data.Data)
                            {
                                bool classExpanded = true;
                                data.DataExpanded.TryGetValue(kp.Key, out classExpanded);

                                EditorGUILayout.BeginHorizontal();
                                data.DataExpanded[kp.Key] = EditorGUILayout.Foldout(classExpanded, "Class: " + kp.Key);
                                if (classExpanded != data.DataExpanded[kp.Key])
                                    data.Save();

                                if (GUILayout.Button("Delete", GUILayout.Width(100)))
                                {
                                    if (EditorUtility.DisplayDialog("Confirm", "Please confirm you want to delete class: " + kp.Key, "Yes", "No"))
                                        deleteClass = kp.Key;
                                }
                                EditorGUILayout.EndHorizontal();

                                if (classExpanded)
                                {
                                    EditorGUI.indentLevel += 1;

                                    EditorGUILayout.BeginHorizontal();
                                    newPhrase = EditorGUILayout.TextField(newPhrase);
                                    EditorGUI.BeginDisabledGroup(string.IsNullOrEmpty(newPhrase));

                                    GUI.SetNextControlName("AddPhrase");
                                    if (GUILayout.Button("Add Phrase", GUILayout.Width(100)))
                                    {
                                        kp.Value.Add(newPhrase);
                                        data.Save();

                                        newPhrase = string.Empty;
                                        GUI.FocusControl("AddPhrase");
                                    }
                                    EditorGUI.EndDisabledGroup();
                                    EditorGUILayout.EndHorizontal();

                                    for (int i = 0; i < kp.Value.Count; ++i)
                                    {
                                        EditorGUILayout.BeginHorizontal();
                                        kp.Value[i] = EditorGUILayout.TextField(kp.Value[i]);

                                        if (GUILayout.Button("Delete", GUILayout.Width(100)))
                                            kp.Value.RemoveAt(i--);

                                        EditorGUILayout.EndHorizontal();
                                    }

                                    EditorGUI.indentLevel -= 1;
                                }
                            }

                            if (!string.IsNullOrEmpty(deleteClass))
                            {
                                data.Data.Remove(deleteClass);
                                data.DataExpanded.Remove(deleteClass);
                                data.Save();
                            }

                            EditorGUI.indentLevel -= 1;
                        }

                        EditorGUI.indentLevel -= 1;
                    }
                }

                if (deleteClassifier != null)
                {
                    File.Delete(deleteClassifier.FileName);
                    classifierData.Remove(deleteClassifier);

                    AssetDatabase.Refresh();
                }
            }
            //EditorGUI.indentLevel -= 1;

            EditorGUILayout.LabelField("Create Classifier:");
            EditorGUI.indentLevel += 1;

            newClassifierName = EditorGUILayout.TextField("Name", newClassifierName);
            newClassifierLang = EditorGUILayout.TextField("Language", newClassifierLang);

            EditorGUI.BeginDisabledGroup(string.IsNullOrEmpty(newClassifierLang) || string.IsNullOrEmpty(newClassifierName));
            if (GUILayout.Button("Create"))
            {
                newClassifierName = newClassifierName.Replace("/", "_");

                string classifierFile = classifiersFolder + "/" + newClassifierName + ".json";
                if (!File.Exists(classifierFile)
                    || EditorUtility.DisplayDialog("Confirm", string.Format("Classifier file {0} already exists, are you sure you wish to overwrite?", classifierFile), "YES", "NO"))
                {
                    ClassifierData newClassifier = new ClassifierData();
                    newClassifier.Name = newClassifierName;
                    newClassifier.Language = newClassifierLang;
                    newClassifier.Save(classifierFile);
                    newClassifierName = string.Empty;

                    OnRefresh();
                }
            }
            EditorGUI.EndDisabledGroup();
            EditorGUI.indentLevel -= 1;


            bool showAllClassifiers = displayClassifiers;
            displayClassifiers = EditorGUILayout.Foldout(showAllClassifiers, "All Classifier Instances");

            if (showAllClassifiers)
            {
                EditorGUI.indentLevel += 1;

                if (classifiers != null)
                {
                    for (int i = 0; i < classifiers.classifiers.Length; ++i)
                    {
                        Classifier cl = classifiers.classifiers[i];

                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("Name: " + cl.name);
                        if (GUILayout.Button("Delete", GUILayout.Width(100)))
                        {
                            if (EditorUtility.DisplayDialog("Confirm", string.Format("Confirm delete of classifier {0}", cl.classifier_id), "YES", "NO")
                                && !naturalLanguageClassifier.DeleteClassifer(cl.classifier_id, OnDeleteClassifier))
                            {
                                EditorUtility.DisplayDialog("Error", "Failed to delete classifier.", "OK");
                            }
                        }
                        EditorGUILayout.EndHorizontal();

                        EditorGUI.indentLevel += 1;
                        EditorGUILayout.LabelField("ID: " + cl.classifier_id);
                        EditorGUILayout.LabelField("Created: " + cl.created.ToString());
                        EditorGUILayout.LabelField("Status: " + cl.status);

                        EditorGUI.indentLevel -= 1;
                    }
                }
                EditorGUI.indentLevel -= 1;
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndScrollView();
        }
    }
}

#endif
