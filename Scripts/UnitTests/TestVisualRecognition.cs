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

using System.Collections;
using IBM.Watson.DeveloperCloud.Logging;
using System.Collections.Generic;
using IBM.Watson.DeveloperCloud.Utilities;
using System.IO;
using UnityEngine;
using System;
using IBM.Watson.DeveloperCloud.Services.VisualRecognition.v3;

#pragma warning disable 0414

namespace IBM.Watson.DeveloperCloud.UnitTests
{
  public class TestVisualRecognition : UnitTest
  {
    VisualRecognition visualRecognition = new VisualRecognition();
    bool trainClasifierTested = false;
    bool getClassifiersTested = false;
    bool findClassifierTested = false;
    bool getClassifierTested = false;
    bool updateClassifierTested = false;
    bool classifyGETTested = false;
    bool classifyPOSTTested = false;
    bool detectFacesGETTested = false;
    bool detectFacesPOSTTested = false;
    bool recognizeTextGETTested = false;
    bool recognizeTextPOSTTested = false;
    bool deleteClassifierTested = false;

    bool listCollectionsTested = false;
    bool createCollectionTested = false;
    bool deleteCollectionTested = false;
    bool retrieveCollectionDetailsTested = false;
    bool listImagesTested = false;
    bool addImagesToCollectionTested = false;
    bool deleteImageFromCollectionTested = false;
    bool listImageDetailsTested = false;
    bool deleteImageMetadataTested = false;
    bool listImageMetadataTested = false;
    bool findSimilarTested = false;


    bool trainClassifier = false;
    bool isClassifierReady = false;
    bool hasUpdatedClassifier = false;
    bool isUpdatedClassifierReady = false;
    string classifierId = null;
    string classifierName = "unity-integration-test-classifier";
    string className_Giraffe = "giraffe";
    string className_Turtle = "turtle";
    private string imageURL = "https://c2.staticflickr.com/2/1226/1173659064_8810a06fef_b.jpg";   //  giraffe image
    private string imageFaceURL = "https://upload.wikimedia.org/wikipedia/commons/e/e9/Official_portrait_of_Barack_Obama.jpg";    //  Obama image
    private string imageTextURL = "http://i.stack.imgur.com/ZS6nH.png";   //  image with text

    private string createdCollectionID;
    private string createdCollectionImage;

    public override IEnumerator RunTest()
    {
      //  test get classifiers
      Log.Debug("TestVisualRecognition", "Getting all classifiers!");
      visualRecognition.GetClassifiers(OnGetClassifiers);
      while (!getClassifiersTested)
        yield return null;

      //  test find classifier
      Log.Debug("TestVisualRecognition", "Finding classifier {0}!", classifierName);
      visualRecognition.FindClassifier(OnFindClassifier, classifierName);
      while (!findClassifierTested)
        yield return null;

      if (trainClassifier)
      {
        //  test train classifier
        Log.Debug("TestVisualRecognition", "Training classifier!");
        string m_positiveExamplesPath = Application.dataPath + "/Watson/Examples/ServiceExamples/TestData/visual-recognition-classifiers/giraffe_positive_examples.zip";
        string m_negativeExamplesPath = Application.dataPath + "/Watson/Examples/ServiceExamples/TestData/visual-recognition-classifiers/negative_examples.zip";
        Dictionary<string, string> positiveExamples = new Dictionary<string, string>();
        positiveExamples.Add(className_Giraffe, m_positiveExamplesPath);
        Test(visualRecognition.TrainClassifier(OnTrainClassifier, classifierName, positiveExamples, m_negativeExamplesPath));
        while (!trainClasifierTested)
          yield return null;
      }

      //  Wait until classifier is ready
      if (!isClassifierReady)
      {
        Log.Debug("TestVisualRecognition", "Checking classifier {0} status!", classifierId);
        CheckClassifierStatus(OnCheckClassifierStatus);
        while (!isClassifierReady)
          yield return null;
      }

      if (!string.IsNullOrEmpty(classifierId))
      {
        //  test get classifier
        Log.Debug("TestVisualRecognition", "Getting classifier {0}!", classifierId);
        visualRecognition.GetClassifier(OnGetClassifier, classifierId);
        while (!getClassifierTested)
          yield return null;

        //  Update classifier
        Log.Debug("TestVisualRecognition", "Updating classifier {0}", classifierId);
        string m_positiveUpdated = Application.dataPath + "/Watson/Examples/ServiceExamples/TestData/visual-recognition-classifiers/turtle_positive_examples.zip";
        Dictionary<string, string> positiveUpdatedExamples = new Dictionary<string, string>();
        positiveUpdatedExamples.Add(className_Turtle, m_positiveUpdated);
        visualRecognition.UpdateClassifier(OnUpdateClassifier, classifierId, classifierName, positiveUpdatedExamples);
        while (!updateClassifierTested)
          yield return null;

        //  Wait for updated classifier to be ready.
        Log.Debug("TestVisualRecognition", "Checking updated classifier {0} status!", classifierId);
        CheckClassifierStatus(OnCheckUpdatedClassifierStatus);
        while (!isUpdatedClassifierReady)
          yield return null;

        string[] m_owners = { "IBM", "me" };
        string[] m_classifierIds = { "default", classifierId };

        //  test classify image get
        Log.Debug("TestVisualRecognition", "Classifying image using GET!");
        visualRecognition.Classify(OnClassifyGet, imageURL, m_owners, m_classifierIds);
        while (!classifyGETTested)
          yield return null;

        //  test classify image post
        Log.Debug("TestVisualRecognition", "Classifying image using POST!");
        string m_classifyImagePath = Application.dataPath + "/Watson/Examples/ServiceExamples/TestData/visual-recognition-classifiers/giraffe_to_classify.jpg";
        visualRecognition.Classify(m_classifyImagePath, OnClassifyPost, m_owners, m_classifierIds);
        while (!classifyPOSTTested)
          yield return null;
      }

      //  test detect faces get
      Log.Debug("TestVisualRecognition", "Detecting face image using GET!");
      visualRecognition.DetectFaces(OnDetectFacesGet, imageFaceURL);
      while (!detectFacesGETTested)
        yield return null;

      //  test detect faces post
      Log.Debug("TestVisualRecognition", "Detecting face image using POST!");
      string m_detectFaceImagePath = Application.dataPath + "/Watson/Examples/ServiceExamples/TestData/visual-recognition-classifiers/obama.jpg";
      visualRecognition.DetectFaces(m_detectFaceImagePath, OnDetectFacesPost);
      while (!detectFacesPOSTTested)
        yield return null;

      //  test recognize text get
      Log.Debug("TestVisualRecognition", "Recognizing text image using GET!");
      visualRecognition.RecognizeText(OnRecognizeTextGet, imageTextURL);
      while (!recognizeTextGETTested)
        yield return null;

      //  test recognize text post
      Log.Debug("TestVisualRecognition", "Recognizing text image using POST!");
      string m_recognizeTextImagePath = Application.dataPath + "/Watson/Examples/ServiceExamples/TestData/visual-recognition-classifiers/from_platos_apology.png";
      visualRecognition.RecognizeText(m_recognizeTextImagePath, OnRecognizeTextPost);
      while (!recognizeTextPOSTTested)
        yield return null;

      //  test delete classifier
      Log.Debug("TestVisualRecognition", "Deleting classifier {0}!", classifierId);
      visualRecognition.DeleteClassifier(OnDeleteClassifier, classifierId);
      while (!deleteClassifierTested)
        yield return null;

      //  test list collections
      Log.Debug("TestVisualRecognition", "Attempting to list collections!");
      visualRecognition.GetCollections(OnGetCollections);
      while (!listCollectionsTested)
        yield return null;

      //  test create collection
      Log.Debug("TestVisualRecognition", "Attempting to create collection!");
      visualRecognition.CreateCollection(OnCreateCollection, "unity-integration-test-collection");
      while (!createCollectionTested)
        yield return null;

      //  test retrive collection details
      Log.Debug("TestVisualRecognition", "Attempting to retrieve collection details!");
      visualRecognition.GetCollection(OnGetCollection, createdCollectionID);
      while (!retrieveCollectionDetailsTested)
        yield return null;

      //  test add images to collection
      Log.Debug("TestVisualRecognition", "Attempting to add images to collection!");
      string m_collectionImagePath = Application.dataPath + "/Watson/Examples/ServiceExamples/TestData/visual-recognition-classifiers/giraffe_to_classify.jpg";
      Dictionary<string, string> imageMetadata = new Dictionary<string, string>();
      imageMetadata.Add("key1", "value1");
      imageMetadata.Add("key2", "value2");
      imageMetadata.Add("key3", "value3");
      visualRecognition.AddCollectionImage(OnAddImageToCollection, createdCollectionID, m_collectionImagePath, imageMetadata);
      while (!addImagesToCollectionTested)
        yield return null;

      //  test list images
      Log.Debug("TestVisualRecognition", "Attempting to list images!");
      visualRecognition.GetCollectionImages(OnGetCollectionImages, createdCollectionID);
      while (!listImagesTested)
        yield return null;

      //  test list image details
      Log.Debug("TestVisualRecognition", "Attempting to list image details!");
      visualRecognition.GetImage(OnGetImage, createdCollectionID, createdCollectionImage);
      while (!listImageDetailsTested)
        yield return null;

      //  test list image metadata
      Log.Debug("TestVisualRecognition", "Attempting to list image metadata!");
      visualRecognition.GetMetadata(OnGetMetadata, createdCollectionID, createdCollectionImage);
      while (!listImageMetadataTested)
        yield return null;

      //  test find similar
      Log.Debug("TestVisualRecognition", "Attempting to find similar!");
      visualRecognition.FindSimilar(OnFindSimilar, createdCollectionID, m_collectionImagePath);
      while (!findSimilarTested)
        yield return null;

      //  test delete image metadata
      Log.Debug("TestVisualRecognition", "Attempting to delete metadata!");
      visualRecognition.DeleteCollectionImageMetadata(OnDeleteMetadata, createdCollectionID, createdCollectionImage);
      while (!deleteImageMetadataTested)
        yield return null;

      //  test delete image from collection
      Log.Debug("TestVisualRecognition", "Attempting to delete image from collection!");
      visualRecognition.DeleteCollectionImage(OnDeleteCollectionImage, createdCollectionID, createdCollectionImage);
      while (!deleteImageFromCollectionTested)
        yield return null;

      //  test delete collection
      Log.Debug("TestVisualRecognition", "Attempting to delete collection!");
      visualRecognition.DeleteCollection(OnDeleteCollection, createdCollectionID);
      while (!deleteCollectionTested)
        yield return null;
      yield break;
    }

    private void OnFindClassifier(GetClassifiersPerClassifierVerbose classifier, string customData)
    {
      if (classifier != null)
      {
        Log.Status("TestVisualRecognition", "Find Result, Classifier ID: {0}, Status: {1}", classifier.classifier_id, classifier.status);
        if (classifier.status == "ready")
        {
          trainClassifier = false;
          isClassifierReady = true;
          classifierId = classifier.classifier_id;
        }
        else
        {
          trainClassifier = false;
        }
      }
      else
      {
        trainClassifier = true;
      }

      findClassifierTested = true;
    }

    private void OnTrainClassifier(GetClassifiersPerClassifierVerbose classifier, string customData)
    {
      Test(classifier != null);
      if (classifier != null)
      {
        Log.Status("TestVisualRecognition", "Classifier ID: {0}, Classifier name: {1}, Status: {2}", classifier.classifier_id, classifier.name, classifier.status);
        //  store classifier id
        classifierId = classifier.classifier_id;
      }

      trainClasifierTested = true;
    }

    private void OnGetClassifiers(GetClassifiersTopLevelBrief classifiers, string customData)
    {
      Test(classifiers != null);
      if (classifiers != null && classifiers.classifiers.Length > 0)
      {
        Log.Debug("TestVisualRecognition", "{0} classifiers found!", classifiers.classifiers.Length);
        //                foreach(GetClassifiersPerClassifierBrief classifier in classifiers.classifiers)
        //                {
        //                    Log.Debug("TestVisualRecognition", "Classifier: " + classifier.name + ", " + classifier.classifier_id);
        //                }
      }
      else
      {
        Log.Debug("TestVisualRecognition", "Failed to get classifiers!");
      }

      getClassifiersTested = true;
    }

    private void OnGetClassifier(GetClassifiersPerClassifierVerbose classifier, string customData)
    {
      Test(classifier != null);
      if (classifier != null)
      {
        Log.Debug("TestVisualRecognition", "Classifier {0} found! Classifier name: {1}", classifier.classifier_id, classifier.name);
        foreach (Class classifierClass in classifier.classes)
          if (classifierClass._class == className_Turtle)
            hasUpdatedClassifier = true;
      }

      getClassifierTested = true;
    }

    private void OnUpdateClassifier(GetClassifiersPerClassifierVerbose classifier, string customData)
    {
      if (classifier != null)
      {
        Log.Status("TestVisualRecognition", "Classifier ID: {0}, Classifier name: {1}, Status: {2}", classifier.classifier_id, classifier.name, classifier.status);
        foreach (Class classifierClass in classifier.classes)
          if (classifierClass._class == className_Turtle)
            hasUpdatedClassifier = true;
        //  store classifier id
        //classifierId = classifier.classifier_id;
      }

      updateClassifierTested = true;
    }

    private void OnClassifyGet(ClassifyTopLevelMultiple classify, string customData)
    {
      Test(classify != null);
      if (classify != null)
      {
        Log.Debug("TestVisualRecognition", "ClassifyImage GET images processed: " + classify.images_processed);
        foreach (ClassifyTopLevelSingle image in classify.images)
        {
          Log.Debug("TestVisualRecognition", "\tClassifyImage GET source_url: " + image.source_url + ", resolved_url: " + image.resolved_url);
          foreach (ClassifyPerClassifier classifier in image.classifiers)
          {
            Log.Debug("TestVisualRecognition", "\t\tClassifyImage GET classifier_id: " + classifier.classifier_id + ", name: " + classifier.name);
            foreach (ClassResult classResult in classifier.classes)
              Log.Debug("TestVisualRecognition", "\t\t\tClassifyImage GET class: " + classResult.m_class + ", score: " + classResult.score + ", type_hierarchy: " + classResult.type_hierarchy);
          }
        }

        classifyGETTested = true;
      }
      else
      {
        Log.Debug("TestVisualRecognition", "Classification GET failed!");
      }
    }

    private void OnClassifyPost(ClassifyTopLevelMultiple classify, string customData)
    {
      Test(classify != null);
      if (classify != null)
      {
        Log.Debug("TestVisualRecognition", "ClassifyImage POST images processed: " + classify.images_processed);
        foreach (ClassifyTopLevelSingle image in classify.images)
        {
          Log.Debug("TestVisualRecognition", "\tClassifyImage POST source_url: " + image.source_url + ", resolved_url: " + image.resolved_url);
          foreach (ClassifyPerClassifier classifier in image.classifiers)
          {
            Log.Debug("TestVisualRecognition", "\t\tClassifyImage POST classifier_id: " + classifier.classifier_id + ", name: " + classifier.name);
            foreach (ClassResult classResult in classifier.classes)
              Log.Debug("TestVisualRecognition", "\t\t\tClassifyImage POST class: " + classResult.m_class + ", score: " + classResult.score + ", type_hierarchy: " + classResult.type_hierarchy);
          }
        }

        classifyPOSTTested = true;
      }
      else
      {
        Log.Debug("TestVisualRecognition", "Classification POST failed!");
      }
    }

    private void OnDetectFacesGet(FacesTopLevelMultiple multipleImages, string customData)
    {
      Test(multipleImages != null);
      if (multipleImages != null)
      {
        Log.Debug("TestVisualRecognition", "DetectFaces GET  images processed: {0}", multipleImages.images_processed);
        foreach (FacesTopLevelSingle faces in multipleImages.images)
        {
          Log.Debug("TestVisualRecognition", "\tDetectFaces GET  source_url: {0}, resolved_url: {1}", faces.source_url, faces.resolved_url);
          foreach (OneFaceResult face in faces.faces)
          {
            if(face.face_location != null)
              Log.Debug("TestVisualRecognition", "\t\tDetectFaces GET Face location: {0}, {1}, {2}, {3}", face.face_location.left, face.face_location.top, face.face_location.width, face.face_location.height);
            if(face.gender != null)
              Log.Debug("TestVisualRecognition", "\t\tDetectFaces GET Gender: {0}, Score: {1}", face.gender.gender, face.gender.score);
            if(face.age != null)
              Log.Debug("TestVisualRecognition", "\t\tDetectFaces GET Age Min: {0}, Age Max: {1}, Score: {2}", face.age.min, face.age.max, face.age.score);
            if (face.identity != null)
              Log.Debug("TestVisualRecognition", "\t\tDetectFaces GET Name: {0}, Score: {1}, Type Heiarchy: {2}", face.identity.name, face.identity.score, face.identity.type_hierarchy);
          }
        }

        detectFacesGETTested = true;
      }
      else
      {
        Log.Debug("ExampleVisualRecognition", "DetectFaces GET Detect faces failed!");
      }
    }

    private void OnDetectFacesPost(FacesTopLevelMultiple multipleImages, string customData)
    {
      Test(multipleImages != null);
      if (multipleImages != null)
      {
        Log.Debug("TestVisualRecognition", "DetectFaces POST  images processed: {0}", multipleImages.images_processed);
        foreach (FacesTopLevelSingle faces in multipleImages.images)
        {
          Log.Debug("TestVisualRecognition", "\tDetectFaces POST  source_url: {0}, resolved_url: {1}", faces.source_url, faces.resolved_url);
          foreach (OneFaceResult face in faces.faces)
          {
            if (face.face_location != null)
              Log.Debug("TestVisualRecognition", "\t\tDetectFaces POST Face location: {0}, {1}, {2}, {3}", face.face_location.left, face.face_location.top, face.face_location.width, face.face_location.height);
            if (face.gender != null)
              Log.Debug("TestVisualRecognition", "\t\tDetectFaces POST Gender: {0}, Score: {1}", face.gender.gender, face.gender.score);
            if (face.age != null)
              Log.Debug("TestVisualRecognition", "\t\tDetectFaces POST Age Min: {0}, Age Max: {1}, Score: {2}", face.age.min, face.age.max, face.age.score);
            if(face.identity != null)
              Log.Debug("TestVisualRecognition", "\t\tDetectFaces POST Name: {0}, Score: {1}, Type Heiarchy: {2}", face.identity.name, face.identity.score, face.identity.type_hierarchy);
          }
        }

        detectFacesPOSTTested = true;
      }
      else
      {
        Log.Debug("ExampleVisualRecognition", "DetectFaces POST Detect faces failed!");
      }
    }

    private void OnRecognizeTextGet(TextRecogTopLevelMultiple multipleImages, string customData)
    {
      Test(multipleImages != null);
      if (multipleImages != null)
      {
        Log.Debug("TestVisualRecognition", "RecognizeText GET images processed: {0}", multipleImages.images_processed);
        foreach (TextRecogTopLevelSingle texts in multipleImages.images)
        {
          Log.Debug("TestVisualRecognition", "\tRecognizeText GET source_url: {0}, resolved_url: {1}", texts.source_url, texts.resolved_url);
          Log.Debug("TestVisualRecognition", "\tRecognizeText GET text: {0}", texts.text);
          //                    foreach(TextRecogOneWord text in texts.words)
          //                    {
          //                        Log.Debug("TestVisualRecognition", "\t\tRecognizeText GET text location: {0}, {1}, {2}, {3}", text.location.left, text.location.top, text.location.width, text.location.height);
          //                        Log.Debug("TestVisualRecognition", "\t\tRecognizeText GET Line number: {0}", text.line_number);
          //                        Log.Debug("TestVisualRecognition", "\t\tRecognizeText GET word: {0}, Score: {1}", text.word, text.score);
          //                    }
        }

        recognizeTextGETTested = true;
      }
      else
      {
        Log.Debug("ExampleVisualRecognition", "RecognizeText GET failed!");
      }
    }

    private void OnRecognizeTextPost(TextRecogTopLevelMultiple multipleImages, string customData)
    {
      Test(multipleImages != null);
      if (multipleImages != null)
      {
        Log.Debug("TestVisualRecognition", "RecognizeText POST images processed: {0}", multipleImages.images_processed);
        foreach (TextRecogTopLevelSingle texts in multipleImages.images)
        {
          Log.Debug("TestVisualRecognition", "\tRecognizeText POST source_url: {0}, resolved_url: {1}", texts.source_url, texts.resolved_url);
          Log.Debug("TestVisualRecognition", "\tRecognizeText POST text: {0}", texts.text);
          //                    foreach(TextRecogOneWord text in texts.words)
          //                    {
          //                        Log.Debug("TestVisualRecognition", "\t\tRecognizeText POST text location: {0}, {1}, {2}, {3}", text.location.left, text.location.top, text.location.width, text.location.height);
          //                        Log.Debug("TestVisualRecognition", "\t\tRecognizeText POST Line number: {0}", text.line_number);
          //                        Log.Debug("TestVisualRecognition", "\t\tRecognizeText POST word: {0}, Score: {1}", text.word, text.score);
          //                    }
        }

        recognizeTextPOSTTested = true;
      }
      else
      {
        Log.Debug("ExampleVisualRecognition", "RecognizeText POST failed!");
      }
    }

    private void OnDeleteClassifier(bool success, string customData)
    {
      if (success)
      {
        visualRecognition.FindClassifier(OnDeleteClassifierFinal, classifierName);
      }

      deleteClassifierTested = true;
      Test(success);
    }

    private void CheckClassifierStatus(VisualRecognition.OnGetClassifier callback, string customData = default(string))
    {
      if (!visualRecognition.GetClassifier(callback, classifierId))
        Log.Debug("TestVisualRecognition", "Get classifier failed!");
    }

    private void OnCheckClassifierStatus(GetClassifiersPerClassifierVerbose classifier, string customData)
    {
      Log.Debug("TestVisualRecognition", "classifier {0} is {1}!", classifier.classifier_id, classifier.status);
      if (classifier.status == "unavailable" || classifier.status == "failed")
      {
        Log.Debug("TestVisualRecognition", "Deleting classifier!");
        //  classifier failed - delete!
        if (!visualRecognition.DeleteClassifier(OnCheckClassifierStatusDelete, classifier.classifier_id))
          Log.Debug("TestVisualRecognition", "Failed to delete classifier {0}!", classifierId);

      }
      else if (classifier.status == "training")
      {
        CheckClassifierStatus(OnCheckClassifierStatus);
      }
      else if (classifier.status == "ready")
      {
        isClassifierReady = true;
        classifierId = classifier.classifier_id;
      }
    }

    private void OnCheckUpdatedClassifierStatus(GetClassifiersPerClassifierVerbose classifier, string customData = default(string))
    {
      Log.Debug("TestVisualRecognition", "classifier {0} is {1}!", classifier.classifier_id, classifier.status);

      if (classifier.status == "retraining")
      {
        CheckClassifierStatus(OnCheckUpdatedClassifierStatus);
      }
      else if (classifier.status == "ready")
      {
        isUpdatedClassifierReady = true;
      }
    }

    private void OnCheckClassifierStatusDelete(bool success, string customData)
    {
      if (success)
      {
        //  train classifier again!
        trainClasifierTested = false;
      }
    }

    private void OnDeleteClassifierFinal(GetClassifiersPerClassifierVerbose classifier, string customData)
    {
      if (classifier == null)
      {
        Log.Debug("TestVisualRecognition", "Classifier not found! Delete sucessful!");
      }
      else
      {
        Log.Debug("TestVisualRecognition", "Classifier {0} found! Delete failed!", classifier.name);
      }
      Test(classifier == null);
    }

    private void OnGetCollections(GetCollections collections, string customData)
    {
      if (collections != null)
      {
        Log.Debug("TestVisualRecognition", "Get Collections succeeded!");
        foreach (CreateCollection collection in collections.collections)
        {
          Log.Debug("TestVisualRecognition", "collectionID: {0} | collection name: {1} | number of images: {2}", collection.collection_id, collection.name, collection.images);
        }
      }
      else
      {
        Log.Debug("TestVisualRecognition", "Get Collections failed!");
      }

      listCollectionsTested = true;
      Test(collections != null);
    }

    private void OnCreateCollection(CreateCollection collection, string customData)
    {
      if (collection != null)
      {
        Log.Debug("TestVisualRecognition", "Create Collection succeeded!");
        Log.Debug("TestVisualRecognition", "collectionID: {0} | collection name: {1} | collection images: {2}", collection.collection_id, collection.name, collection.images);

        createdCollectionID = collection.collection_id;
      }
      else
      {
        Log.Debug("TestVisualRecognition", "Create Collection failed!");
      }

      createCollectionTested = true;
      Test(collection != null);
    }

    private void OnDeleteCollection(bool success, string customData)
    {
      if (success)
        Log.Debug("TestVisualRecognition", "Delete Collection succeeded!");
      else
        Log.Debug("TestVisualRecognition", "Delete Collection failed!");

      deleteCollectionTested = true;
      Test(success);
    }

    private void OnGetCollection(CreateCollection collection, string customData)
    {
      if (collection != null)
      {
        Log.Debug("TestVisualRecognition", "Get Collection succeded!");
        Log.Debug("TestVisualRecognition", "collectionID: {0} | collection name: {1} | collection images: {2}", collection.collection_id, collection.name, collection.images);
      }
      else
      {
        Log.Debug("TestVisualRecognition", "Get Collection failed!");

      }

      retrieveCollectionDetailsTested = true;
      Test(collection != null);
    }

    private void OnGetCollectionImages(GetCollectionImages collections, string customData)
    {
      if (collections != null)
      {
        Log.Debug("TestVisualRecognition", "Get Collections succeded!");
        foreach (GetCollectionsBrief collection in collections.images)
          Log.Debug("TestVisualRecognition", "imageID: {0} | image file: {1} | image metadataOnGetCollections: {2}", collection.image_id, collection.image_file, collection.metadata.ToString());
      }
      else
      {
        Log.Debug("TestVisualRecognition", "Get Collections failed!");
      }

      Test(collections != null);
      listImagesTested = true;
    }

    private void OnAddImageToCollection(CollectionsConfig images, string customData)
    {
      if (images != null)
      {
        Log.Debug("TestVisualRecognition", "Add image to collection succeeded!");
        createdCollectionImage = images.images[0].image_id;
        Log.Debug("TestVisualRecognition", "images processed: {0}", images.images_processed);
        foreach (CollectionImagesConfig image in images.images)
          Log.Debug("TestVisualRecognition", "imageID: {0} | image_file: {1} | image metadata: {1}", image.image_id, image.image_file, image.metadata.ToString());
      }
      else
      {
        Log.Debug("TestVisualRecognition", "Add image to collection failed!");
      }

      Test(images != null);
      addImagesToCollectionTested = true;
    }

    private void OnDeleteCollectionImage(bool success, string customData)
    {
      if (success)
        Log.Debug("TestVisualRecognition", "Delete collection image succeeded!");
      else
        Log.Debug("TestVisualRecognition", "Delete collection image failed!");

      Test(success);
      deleteImageFromCollectionTested = true;
    }

    private void OnGetImage(GetCollectionsBrief image, string customData)
    {
      if (image != null)
      {
        Log.Debug("TestVisualRecognition", "GetImage succeeded!");
        Log.Debug("TestVisualRecognition", "imageID: {0} | created: {1} | image_file: {2} | metadata: {3}", image.image_id, image.created, image.image_file, image.metadata);
      }
      else
      {
        Log.Debug("TestVisualRecognition", "GetImage failed!");
      }

      Test(image != null);
      listImageDetailsTested = true;

    }

    private void OnDeleteMetadata(bool success, string customData)
    {
      if (success)
        Log.Debug("TestVisualRecognition", "Delete image metadata succeeded!");
      else
        Log.Debug("TestVisualRecognition", "Delete image metadata failed!");

      Test(success);
      deleteImageMetadataTested = true;
    }

    private void OnGetMetadata(object responseObject, string customData)
    {
      if (responseObject != null)
        Log.Debug("TestVisualRecognition", "ResponseObject: {0}", responseObject);

      Test(responseObject != null);
      listImageMetadataTested = true;
    }

    private void OnFindSimilar(SimilarImagesConfig images, string customData)
    {
      if (images != null)
      {
        Log.Debug("TestVisualRecognition", "GetSimilar succeeded!");
        Log.Debug("TestVisualRecognition", "images processed: {0}", images.images_processed);
        foreach (SimilarImageConfig image in images.similar_images)
          Log.Debug("TestVisualRecognition", "image ID: {0} | image file: {1} | score: {2} | metadata: {3}", image.image_id, image.image_file, image.score, image.metadata.ToString());
      }
      else
      {
        Log.Debug("TestVisualRecognition", "GetSimilar failed!");
      }

      Test(images != null);
      findSimilarTested = true;
    }
  }
}
