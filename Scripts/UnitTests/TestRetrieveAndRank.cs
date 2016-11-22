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
//#define TEST_CREATE_DELETE

using UnityEngine;
using System.Collections;
using IBM.Watson.DeveloperCloud.Services.RetrieveAndRank.v1;
using IBM.Watson.DeveloperCloud.Logging;
using IBM.Watson.DeveloperCloud.Utilities;

#pragma warning disable 0649
#pragma warning disable 0414

namespace IBM.Watson.DeveloperCloud.UnitTests
{
  public class TestRetrieveAndRank : UnitTest
  {
    RetrieveAndRank retrieveAndRank = new RetrieveAndRank();

    /// <summary>
    /// Enables full test of newly created configs and rankers.
    /// </summary>
    public bool IsFullTest = false;

    //private bool existingClusterDataRetrieved = false;
    //private bool existingRankerDataRetrieved = false;
    //private int numExistingClusters = 0;
    //private int numExistingConfigsProcessed = 0;
    //private int numExistingCollectionsProcessed = 0;
    //private List<ClusterInfo> existingClusterInfo = new List<ClusterInfo>();
    //private RankerInfoPayload[] existingRankers;

    private bool getClustersTested = false;
    private bool getClusterTested = false;
    private bool listClusterConfigsTested = false;
    private bool getClusterConfigTested = false;
    private bool listCollectionRequestTested = false;
    private bool standardSearchTested = false;
    private bool rankedSearchTested = false;
    private bool getRankersTested = false;
    private bool rankTested = false;
    private bool getRankerInfoTested = false;

    private string configToCreateName = "unity-integration-test-config";
    private string collectionToCreateName = "unity-integration-test-collection";
    private string createdRankerID;
    private string createdClusterID;

#if TEST_CREATE_DELETE
        private bool deleteClusterTested = false;
        private bool createClusterTested = false;
        private bool deleteClusterConfigTested = false;
        private bool uploadClusterConfigTested = false;
        private bool createCollectionRequestTested = false;
        private bool deleteCollectionRequestTested = false;
        private bool indexDocumentsTested = false;
        private bool createRankerTested = false;
        private bool deleteRankersTested = false;
        private string clusterToCreateName = "unity-integration-test-cluster";
        private string rankerToCreateName = "unity-integration-test-ranker";
        private bool isDoneWaiting = false;
#endif

    //  from config variables
    private string exampleClusterID;
    private string exampleConfigName;
    private string exampleRankerID;
    private string exampleCollectionName;

    private string integrationTestQuery = "What is the basic mechanisim of the transonic aileron buzz";

    private string[] fl = { "title", "id", "body", "author", "bibliography" };

    //  data paths to local files
    private string integrationTestClusterConfigPath;
    private string integrationTestRankerTrainingPath;
    private string integrationTestRankerAnswerDataPath;
    private string integrationTestIndexDataPath;

    private bool isClusterReady = false;
    private bool isRankerReady = false;

    public override IEnumerator RunTest()
    {
      integrationTestClusterConfigPath = Application.dataPath + "/Watson/Examples/ServiceExamples/TestData/RetrieveAndRank/cranfield_solr_config.zip";
      integrationTestRankerTrainingPath = Application.dataPath + "/Watson/Examples/ServiceExamples/TestData/RetrieveAndRank/ranker_training_data.csv";
      integrationTestRankerAnswerDataPath = Application.dataPath + "/Watson/Examples/ServiceExamples/TestData/RetrieveAndRank/ranker_answer_data.csv";
      integrationTestIndexDataPath = Application.dataPath + "/Watson/Examples/ServiceExamples/TestData/RetrieveAndRank/cranfield_data.json";

      exampleClusterID = Config.Instance.GetVariableValue("RetrieveAndRank_IntegrationTestClusterID");
      exampleConfigName = Config.Instance.GetVariableValue("RetrieveAndRank_IntegrationTestConfigName");
      exampleRankerID = Config.Instance.GetVariableValue("RetrieveAndRank_IntegrationTestRankerID");
      exampleCollectionName = Config.Instance.GetVariableValue("RetrieveAndRank_IntegrationTestCollectionName");

      #region delete existing
      ////  Get existing cluster data.
      //Log.Debug("TestRetrieveAndRank", "Getting existing clusters.");
      //retrieveAndRank.GetClusters(OnGetExistingClusters);
      //while (!existingClusterDataRetrieved)
      //    yield return null;

      ////  get existing config data.
      //Log.Debug("TestRetrieveAndRank", "Getting existing configs.");
      //foreach (ClusterInfo cluster in existingClusterInfo)
      //    retrieveAndRank.GetClusterConfigs(OnGetExistingConfigs, cluster.Cluster.solr_cluster_id, cluster.Cluster.solr_cluster_id);
      //while (numExistingConfigsProcessed < existingClusterInfo.Count)
      //    yield return null;

      ////  get existing collection data.
      //Log.Debug("TestRetrieveAndRank", "Getting existing collections.");
      //foreach (ClusterInfo cluster in existingClusterInfo)
      //    retrieveAndRank.ForwardCollectionRequest(OnGetExistingCollections, cluster.Cluster.solr_cluster_id, CollectionsAction.LIST, null, null, cluster.Cluster.solr_cluster_id);
      //while (numExistingCollectionsProcessed < existingClusterInfo.Count)
      //    yield return null;

      ////  get existing ranker data.
      //Log.Debug("TestRetrieveAndRank", "Getting existing rankers.");
      //retrieveAndRank.GetRankers(OnGetExistingRankers);
      //while (!existingRankerDataRetrieved)
      //    yield return null;

      ////  Cleanup old data
      //Log.Debug("TestRetrieveAndRank", "Cleaning existing data.");
      //foreach (ClusterInfo cluster in existingClusterInfo)
      //{
      //    //  Delete collections
      //    Log.Debug("TestRetriveAndRank", "Attempting to delete extra collections!");
      //    if (cluster.Collections != null && cluster.Collections.Length > 0)
      //        foreach (string collection in cluster.Collections)
      //        {
      //            if (collection == collectionToCreateName)
      //            {
      //                Log.Debug("TestRetrieveAndRank", "Deleting collection {0}.", collection);
      //                retrieveAndRank.ForwardCollectionRequest(OnDeleteExistingCollection, cluster.Cluster.solr_cluster_id, CollectionsAction.DELETE, collection);
      //            }
      //    }

      //    //	Wait before deleting config
      //    isDoneWaiting = false;
      //    Runnable.Run(WaitUp(5f));
      //    while (!isDoneWaiting)
      //        yield return null;

      //    //  Delete config
      //    Log.Debug("TestRetriveAndRank", "Attempting to delete extra configs!");
      //    if (cluster.Configs != null && cluster.Configs.Length > 0)
      //        foreach (string config in cluster.Configs)
      //        {
      //            if (config == configToCreateName)
      //            {
      //                Log.Debug("TestRetrieveAndRank", "Deleting config {0}.", config);
      //                retrieveAndRank.DeleteClusterConfig(OnDeleteExistingConfig, cluster.Cluster.solr_cluster_id, config);
      //            }
      //        }

      //    //while (numExistingConfigsProcessed > 0)
      //    //    yield return null;

      //    //	Wait before deleting cluster
      //    isDoneWaiting = false;
      //    Runnable.Run(WaitUp(5f));
      //    while (!isDoneWaiting)
      //        yield return null;

      //    //  Delete cluster
      //    Log.Debug("TestRetriveAndRank", "Attempting to delete extra clusters!");
      //    if (cluster.Cluster.cluster_name == clusterToCreateName)
      //    {
      //        Log.Debug("TestRetrieveAndRank", "Deleting cluster {0}.", cluster.Cluster.solr_cluster_id);
      //        retrieveAndRank.DeleteCluster(OnDeleteExistingCluster, cluster.Cluster.solr_cluster_id);
      //    }
      //}

      //while (numExistingClusters > 0)
      //    yield return null;

      ////	Wait before deleting ranker
      //isDoneWaiting = false;
      //Runnable.Run(WaitUp(5f));
      //while (!isDoneWaiting)
      //    yield return null;

      ////  Delete rankers
      //foreach (RankerInfoPayload ranker in existingRankers)
      //{
      //    if (ranker.name == rankerToCreateName)
      //    {
      //        Log.Debug("TestRetrieveAndRank", "Deleting ranker {0}.", ranker.ranker_id);
      //        retrieveAndRank.DeleteRanker(OnDeleteExistingRanker, ranker.ranker_id);
      //    }
      //}
      #endregion

      //  Get clusters
      Log.Debug("TestRetrieveAndRank", "*** Attempting to get clusters!");
      retrieveAndRank.GetClusters(OnGetClusters);
      while (!getClustersTested)
        yield return null;

#if TEST_CREATE_DELETE
            //  Create cluster
            Log.Debug("TestRetrieveAndRank", "*** Attempting to create cluster!");
            retrieveAndRank.CreateCluster(OnCreateCluster, clusterToCreateName, "1");
            while (!createClusterTested)
                yield return null;
#endif
      //  Get created cluster
      Log.Debug("TestRetrieveAndRank", "*** Attempting to get created cluster {0}!", IsFullTest ? createdClusterID : exampleClusterID);
      retrieveAndRank.GetCluster(OnGetCluster, IsFullTest ? createdClusterID : exampleClusterID);
      while (!getClusterTested || !isClusterReady)
        yield return null;

      //  List cluster configs
      if (!listClusterConfigsTested)
      {
        Log.Debug("TestRetrieveAndRank", "*** Attempting to get cluster configs for {0}!", IsFullTest ? createdClusterID : exampleClusterID);
        retrieveAndRank.GetClusterConfigs(OnGetClusterConfigs, IsFullTest ? createdClusterID : exampleClusterID);
        while (!listClusterConfigsTested)
          yield return null;
      }

#if TEST_CREATE_DELETE
            //  Upload cluster config
            Log.Debug("TestRetrieveAndRank", "*** Attempting to upload cluster {0} config {1}!", IsFullTest ? createdClusterID : exampleClusterID, configToCreateName);
            retrieveAndRank.UploadClusterConfig(OnUploadClusterConfig, IsFullTest ? createdClusterID : exampleClusterID, configToCreateName, integrationTestClusterConfigPath);
            while (!uploadClusterConfigTested)
                yield return null;
#endif
      //  Get cluster config
      Log.Debug("TestRetrieveAndRank", "*** Attempting to get cluster {0} config {1}!", IsFullTest ? createdClusterID : exampleClusterID, IsFullTest ? configToCreateName : exampleConfigName);
      retrieveAndRank.GetClusterConfig(OnGetClusterConfig, IsFullTest ? createdClusterID : exampleClusterID, IsFullTest ? configToCreateName : exampleConfigName);
      while (!getClusterConfigTested)
        yield return null;
#if TEST_CREATE_DELETE
            //  Create Collection
            Log.Debug("TestRetrieveAndRank", "*** Attempting to create collection!");
            retrieveAndRank.ForwardCollectionRequest(OnCreateCollections, IsFullTest ? createdClusterID : exampleClusterID, CollectionsAction.CREATE, collectionToCreateName, IsFullTest ? configToCreateName : exampleConfigName);
            while (!createCollectionRequestTested)
                yield return null;
#endif
      //  List Collections
      Log.Debug("TestRetrieveAndRank", "*** Attempting to list collections!");
      if (!listCollectionRequestTested)
      {
        retrieveAndRank.ForwardCollectionRequest(OnListCollections, IsFullTest ? createdClusterID : exampleClusterID, CollectionsAction.LIST);
        while (!listCollectionRequestTested)
          yield return null;
      }

#if TEST_CREATE_DELETE
            //  Index documents
            Log.Debug("TestRetrieveAndRank", "*** Attempting to index documents!");
            retrieveAndRank.IndexDocuments(OnIndexDocuments, integrationTestIndexDataPath, IsFullTest ? createdClusterID : exampleClusterID, collectionToCreateName);
            while (!indexDocumentsTested)
                yield return null;
#endif

      //  Get rankers
      if (!getRankersTested)
      {
        Log.Debug("TestRetrieveAndRank", "*** Attempting to get rankers!");
        retrieveAndRank.GetRankers(OnGetRankers);
        while (!getRankersTested)
          yield return null;
      }

#if TEST_CREATE_DELETE
            //  Create ranker
            Log.Debug("TestRetrieveAndRank", "*** Attempting to create rankers!");
            retrieveAndRank.CreateRanker(OnCreateRanker, integrationTestRankerTrainingPath, rankerToCreateName);
            while (!createRankerTested)
                yield return null;
#endif
      //  Get ranker info
      Log.Debug("TestRetrieveAndRank", "*** Attempting to get Ranker Info!");
      retrieveAndRank.GetRanker(OnGetRanker, IsFullTest ? createdRankerID : exampleRankerID);
      while (!getRankerInfoTested || !isRankerReady)
        yield return null;

      //  Rank
      Log.Debug("TestRetrieveAndRank", "*** Attempting to rank!");
      retrieveAndRank.Rank(OnRank, IsFullTest ? createdRankerID : exampleRankerID, integrationTestRankerAnswerDataPath);
      while (!rankTested)
        yield return null;

      //  Standard Search
      Log.Debug("TestRetrieveAndRank", "*** Attempting to search!");
      retrieveAndRank.Search(OnStandardSearch, IsFullTest ? createdClusterID : exampleClusterID, IsFullTest ? collectionToCreateName : exampleCollectionName, integrationTestQuery, fl);
      while (!standardSearchTested)
        yield return null;

      //  Ranked Search
      //Log.Debug("TestRetrieveAndRank", "*** Attempting to search!");
      //retrieveAndRank.Search(OnRankedSearch, IsFullTest ? createdClusterID : exampleClusterID, IsFullTest ? collectionToCreateName : exampleCollectionName, integrationTestQuery, fl, true, exampleRankerID);
      //while (!rankedSearchTested)
      //    yield return null;

#if TEST_CREATE_DELETE
            //	Wait before deleting ranker
            Runnable.Run(WaitUp(5f));
            while (!isDoneWaiting)
                yield return null;

            //  Delete ranker
            if (!deleteRankersTested)
            {
                Log.Debug("ExampleRetriveAndRank", "*** Attempting to delete ranker {0}, {1}!", rankerToCreateName, createdRankerID);
                retrieveAndRank.DeleteRanker(OnDeleteRanker, createdRankerID);
                while (!deleteRankersTested)
                    yield return null;
            }

            //	Wait before deleting collection
            isDoneWaiting = false;
            Runnable.Run(WaitUp(5f));
            while (!isDoneWaiting)
                yield return null;

            //  Delete Collection request
            if (!deleteCollectionRequestTested)
            {
                Log.Debug("TestRetrieveAndRank", "*** Attempting to delete collection!");
                retrieveAndRank.ForwardCollectionRequest(OnDeleteCollections, IsFullTest ? createdClusterID : exampleClusterID, CollectionsAction.DELETE, collectionToCreateName);
                while (!deleteCollectionRequestTested)
                    yield return null;
            }

            //	Wait before deleting config
            isDoneWaiting = false;
            Runnable.Run(WaitUp(5f));
            while (!isDoneWaiting)
                yield return null;

            //  Delete cluster config
            if (!deleteClusterConfigTested)
            {
                Log.Debug("TestRetrieveAndRank", "** Attempting to delete config {1} from cluster {0}!", IsFullTest ? createdClusterID : exampleClusterID, configToCreateName);
                retrieveAndRank.DeleteClusterConfig(OnDeleteClusterConfig, IsFullTest ? createdClusterID : exampleClusterID, configToCreateName);
                while (!deleteClusterConfigTested)
                    yield return null;
            }

			//	Wait before deleting cluster
            isDoneWaiting = false;
			Runnable.Run(WaitUp(5f));
			while (!isDoneWaiting)
				yield return null;

            //  Delete cluster
            if (!deleteClusterTested)
            {
                Log.Debug("TestRetrieveAndRank", "*** Attempting to delete cluster {0}!", createdClusterID);
                retrieveAndRank.DeleteCluster(OnDeleteCluster, createdClusterID);
                while (!deleteClusterTested)
                    yield return null;
            }
#endif
      yield break;
    }

#if TEST_CREATE_DELETE
		private IEnumerator WaitUp(float waitTime)
		{
			yield return new WaitForSeconds(waitTime);
			isDoneWaiting = true;
		}
#endif

    #region delete existing handlers
    //private void OnDeleteExistingCollection(CollectionsResponse resp, string data)
    //{
    //    Test(resp != null);

    //    if(resp != null)
    //        numExistingCollectionsProcessed--;

    //    deleteCollectionRequestTested = true;
    //}

    //private void OnDeleteExistingConfig(bool success, string data)
    //{
    //    Test(success);

    //    if(success)
    //        numExistingConfigsProcessed--;

    //    deleteClusterConfigTested = true;
    //}

    //private void OnDeleteExistingCluster(bool success, string data)
    //{

    //    Log.Debug("TestRetrieveAndRank", "OnDeleteExistingCluster Success = {0}!", success);

    //    if (success)
    //    {
    //        Log.Debug("TestRetrieveAndRank", "Deleted existing cluster!");
    //        deleteClusterTested = true;
    //        numExistingClusters--;
    //    }
    //}

    //private void OnDeleteExistingRanker(bool success, string data)
    //{
    //    Test(success);
    //    if (success)
    //    {
    //        Log.Debug("TestRetrieveAndRank", "Deleted existing ranker!");
    //        //numExistingRankers--;
    //        deleteRankersTested = true;
    //    }
    //}

    //private void OnGetExistingClusters(SolrClusterListResponse resp, string data)
    //{
    //    Test(resp != null);

    //    if (resp != null)
    //    {
    //        foreach (SolrClusterResponse cluster in resp.clusters)
    //        {
    //                Log.Debug("TestRetriveAndRank", "Adding existing cluster {0}.", cluster.cluster_name);
    //                ClusterInfo clusterInfo = new ClusterInfo();
    //                clusterInfo.Cluster = cluster;
    //                existingClusterInfo.Add(clusterInfo);
    //            if (cluster.cluster_name == clusterToCreateName)
    //            {
    //                numExistingClusters++;
    //            }
    //        }
    //    }

    //    existingClusterDataRetrieved = true;
    //    getClustersTested = true;
    //}

    //private void OnGetExistingConfigs(SolrConfigList resp, string data)
    //{
    //    Test(resp != null);

    //    if (resp != null)
    //    {
    //        foreach (ClusterInfo cluster in existingClusterInfo)
    //            if (cluster.Cluster.solr_cluster_id == data)
    //            {
    //                foreach (string config in resp.solr_configs)
    //                    Log.Debug("TestRetriveAndRank", "Adding config {0} to cluster {0}.", config, cluster.Cluster.solr_cluster_id);

    //                cluster.Configs = resp.solr_configs;
    //            }
    //    }

    //    numExistingConfigsProcessed++;
    //    listClusterConfigsTested = true;
    //}

    //private void OnGetExistingCollections(CollectionsResponse resp, string data)
    //{
    //    Test(resp != null);

    //    if (resp != null)
    //    {
    //        foreach (ClusterInfo cluster in existingClusterInfo)
    //            if (cluster.Cluster.solr_cluster_id == data)
    //            {
    //                foreach (string collection in resp.collections)
    //                    Log.Debug("TestRetriveAndRank", "Adding collection {0} to cluster {0}.", collection, cluster.Cluster.solr_cluster_id);

    //                cluster.Collections = resp.collections;
    //            }
    //    }

    //    numExistingCollectionsProcessed++;
    //    listCollectionRequestTested = true;
    //}

    //private void OnGetExistingRankers(ListRankersPayload resp, string data)
    //{
    //    Test(resp != null);

    //    if (resp != null)
    //    {
    //        existingRankers = resp.rankers;
    //    }

    //    existingRankerDataRetrieved = true;
    //    getRankersTested = true;
    //}
    #endregion

    private void OnGetClusters(SolrClusterListResponse resp, string data)
    {
      Test(resp != null);

      if (resp != null)
        foreach (SolrClusterResponse cluster in resp.clusters)
          Log.Debug("TestRetrieveAndRank", "OnGetClusters | cluster name: {0}, size: {1}, ID: {2}, status: {3}.", cluster.cluster_name, cluster.cluster_size, cluster.solr_cluster_id, cluster.solr_cluster_status);
      else
        Log.Debug("TestRetrieveAndRank", "OnGetClusters | Get Cluster Response is null!");

      getClustersTested = true;
    }

#if TEST_CREATE_DELETE
        private void OnCreateCluster(SolrClusterResponse resp, string data)
        {
            Test(resp != null);

            if (resp != null)
            {
                Log.Debug("TestRetrieveAndRank", "OnCreateCluster | name: {0}, size: {1}, ID: {2}, status: {3}.", resp.cluster_name, resp.cluster_size, resp.solr_cluster_id, resp.solr_cluster_status);
                createdClusterID = resp.solr_cluster_id;
            }
            else
                Log.Debug("TestRetrieveAndRank", "OnCreateCluster | Get Cluster Response is null!");

            createClusterTested = true;
        }
#endif

    private void OnGetCluster(SolrClusterResponse resp, string data)
    {
      Test(resp != null);

      if (resp != null)
      {
        Log.Debug("TestRetrieveAndRank", "OnGetCluster | name: {0}, size: {1}, ID: {2}, status: {3}.", resp.cluster_name, resp.cluster_size, resp.solr_cluster_id, resp.solr_cluster_status);

        if (resp.solr_cluster_status != "READY")
          retrieveAndRank.GetCluster(OnGetCluster, IsFullTest ? createdClusterID : exampleClusterID);
        else
          isClusterReady = true;
      }
      else
        Log.Debug("TestRetrieveAndRank", "OnGetCluster | Get Cluster Response is null!");

      getClusterTested = true;
    }

#if TEST_CREATE_DELETE
        private void OnDeleteCluster(bool success, string data)
        {
            Test(success);

            if (success)
                Log.Debug("TestRetrieveAndRank", "OnDeleteCluster | Success!");
            else
                Log.Debug("TestRetrieveAndRank", "OnDeleteCluster | Failure!");

            deleteClusterTested = true;
        }
#endif

    private void OnGetClusterConfigs(SolrConfigList resp, string data)
    {
      Test(resp != null);

      if (resp != null)
      {
        if (resp.solr_configs.Length == 0)
          Log.Debug("TestRetrieveAndRank", "OnGetClusterConfigs | no cluster configs!");

        foreach (string config in resp.solr_configs)
          Log.Debug("TestRetrieveAndRank", "OnGetClusterConfigs | solr_config: " + config);
      }
      else
        Log.Debug("TestRetrieveAndRank", "OnGetClustersConfigs | Get Cluster Configs Response is null!");

      listClusterConfigsTested = true;
    }

#if TEST_CREATE_DELETE
        private void OnUploadClusterConfig(UploadResponse resp, string data)
        {
            Test(resp != null);

            if (resp != null)
                Log.Debug("TestRetrieveAndRank", "OnUploadClusterConfig | Success! {0}, {1}", resp.message, resp.statusCode);
            else
                Log.Debug("TestRetrieveAndRank", "OnUploadClusterConfig | Failure!");

            uploadClusterConfigTested = true;
        }

        private void OnDeleteClusterConfig(bool success, string data)
        {
            Test(success);

            if (success)
                Log.Debug("TestRetrieveAndRank", "OnDeleteClusterConfig | Success!");
            else
                Log.Debug("TestRetrieveAndRank", "OnDeleteClusterConfig | Failure!");

            deleteClusterConfigTested = true;
        }
#endif

    private void OnGetClusterConfig(byte[] respData, string data)
    {
      Test(respData != null);

      if (respData != null)
      {
        Log.Debug("TestRetrieveAndRank", "OnGetClusterConfig | success!");
      }
      else
        Log.Debug("TestRetrieveAndRank", "OnGetClusterConfig | respData is null!");

      getClusterConfigTested = true;
    }

#if TEST_CREATE_DELETE
        private void OnCreateCollections(CollectionsResponse resp, string data)
        {
            Test(resp != null);

            if (resp != null)
            {
                if (resp.responseHeader != null)
                    Log.Debug("TestRetrieveAndRank", "OnCreateCollections | status: {0}, QTime: {1}.", resp.responseHeader.status, resp.responseHeader.QTime);
                if (resp.collections != null)
                {
                    if (resp.collections.Length == 0)
                        Log.Debug("TestRetrieveAndRank", "OnCreateCollections | There are no collections!");
                    else
                        foreach (string collection in resp.collections)
                            Log.Debug("TestRetrieveAndRank", "\tOnCreateCollections | collection: {0}", collection);
                }
            }
            else
                Log.Debug("TestRetrieveAndRank", "OnCreateCollections | GetCollections Response is null!");

            createCollectionRequestTested = true;
        }
#endif

    private void OnListCollections(CollectionsResponse resp, string data)
    {
      Test(resp != null);

      if (resp != null)
      {
        if (resp.responseHeader != null)
          Log.Debug("TestRetrieveAndRank", "OnListCollections | status: {0}, QTime: {1}.", resp.responseHeader.status, resp.responseHeader.QTime);
        if (resp.collections != null)
        {
          if (resp.collections.Length == 0)
            Log.Debug("TestRetrieveAndRank", "OnListCollections | There are no collections!");
          else
            foreach (string collection in resp.collections)
            {
              Log.Debug("TestRetrieveAndRank", "\tOnListCollections | collection: {0}", collection);

              if (collection == (IsFullTest ? collectionToCreateName : exampleCollectionName))
                Log.Debug("TestRetrieveAndRank", "\t\tOnListCollections | created collection found!: {0}", collection);
              else
                Log.Debug("TestRetrieveAndRank", "\t\tOnListCollections | created collection not found!: {0}", collection);
            }
        }
      }
      else
        Log.Debug("TestRetrieveAndRank", "OnListCollections | GetCollections Response is null!");

      listCollectionRequestTested = true;
    }

#if TEST_CREATE_DELETE
        private void OnDeleteCollections(CollectionsResponse resp, string data)
        {
            Test(resp != null);

            if (resp != null)
            {
                if (resp.responseHeader != null)
                    Log.Debug("TestRetrieveAndRank", "OnDeleteCollections | status: {0}, QTime: {1}.", resp.responseHeader.status, resp.responseHeader.QTime);
                if (resp.collections != null)
                {
                    if (resp.collections.Length == 0)
                        Log.Debug("TestRetrieveAndRank", "OnDeleteCollections | There are no collections!");
                    else
                        foreach (string collection in resp.collections)
                            Log.Debug("TestRetrieveAndRank", "\tOnDeleteCollections | collection: {0}", collection);
                }
            }
            else
                Log.Debug("TestRetrieveAndRank", "OnDeleteCollections | GetCollections Response is null!");

            deleteCollectionRequestTested = true;
        }
#endif

    private void OnGetRankers(ListRankersPayload resp, string data)
    {
      Test(resp != null);

      if (resp != null)
      {
        if (resp.rankers.Length == 0)
          Log.Debug("TestRetrieveAndRank", "OnGetRankers | no rankers!");
        foreach (RankerInfoPayload ranker in resp.rankers)
          Log.Debug("TestRetrieveAndRank", "\tOnGetRankers | ranker name: {0}, ID: {1}, created: {2}, url: {3}.", ranker.name, ranker.ranker_id, ranker.created, ranker.url);
      }
      else
        Log.Debug("TestRetrieveAndRank", "OnGetRankers | Get Ranker Response is null!");

      getRankersTested = true;
    }

#if TEST_CREATE_DELETE
        private void OnCreateRanker(RankerStatusPayload resp, string data)
        {
            Test(resp != null);

            if (resp != null)
            {
                Log.Debug("TestRetrieveAndRank", "OnCreateRanker | ID: {0}, url: {1}, name: {2}, created: {3}, status: {4}, statusDescription: {5}.", resp.ranker_id, resp.url, resp.name, resp.created, resp.status, resp.status_description);
                createdRankerID = resp.ranker_id;
            }
            else
                Log.Debug("TestRetrieveAndRank", "OnCreateRanker | Get Cluster Response is null!");

            createRankerTested = true;
        }
#endif

    private void OnGetRanker(RankerStatusPayload resp, string data)
    {
      Test(resp != null);

      if (resp != null)
      {
        Log.Debug("TestRetrieveAndRank", "GetRanker | ranker_id: {0}, url: {1}, name: {2}, created: {3}, status: {4}, status_description: {5}.", resp.ranker_id, resp.url, resp.name, resp.created, resp.status, resp.status_description);
        if (resp.status != "Available")
          retrieveAndRank.GetRanker(OnGetRanker, IsFullTest ? createdRankerID : exampleRankerID);
        else
          isRankerReady = true;
      }
      else
        Log.Debug("TestRetrieveAndRank", "GetRanker | GetRanker response is null!");

      getRankerInfoTested = true;
    }

    private void OnRank(RankerOutputPayload resp, string data)
    {
      Test(resp != null);

      if (resp != null)
      {
        Log.Debug("TestRetrieveAndRank", "OnRank | ID: {0}, url: {1}, top_answer: {2}.", resp.ranker_id, resp.url, resp.top_answer);
        if (resp.answers != null)
          if (resp.answers.Length == 0)
            Log.Debug("TestRetrieveAndRank", "\tThere are no answers!");
          else
            foreach (RankedAnswer answer in resp.answers)
              Log.Debug("TestRetrieveAndRank", "\tOnRank | answerID: {0}, score: {1}, confidence: {2}.", answer.answer_id, answer.score, answer.confidence);
      }
      else
        Log.Debug("TestRetrieveAndRank", "OnRank | Rank response is null!");

      rankTested = true;
    }

#if TEST_CREATE_DELETE
        private void OnDeleteRanker(bool success, string data)
        {
            Test(success);

            if (success)
            {
                Log.Debug("TestRetrieveAndRank", "OnDeleteRanker | Success!");
            }
            else
            {
                Log.Debug("TestRetrieveAndRank", "OnDeleteRanker | Failure!");
            }

            deleteRankersTested = true;
        }

        private void OnIndexDocuments(IndexResponse resp, string data)
        {
            Test(resp != null);

            if (resp != null)
            {
                if (resp.responseHeader != null)
                    Log.Debug("TestRetrieveAndRank", "OnIndexDocuments | status: {0}, QTime: {1}", resp.responseHeader.status, resp.responseHeader.QTime);
                else
                    Log.Debug("TestRetrieveAndRank", "OnIndexDocuments | Response header is null!");
            }
            else
            {
                Log.Debug("TestRetrieveAndRank", "OnIndexDocuments | response is null!");
            }

            indexDocumentsTested = true;
        }
#endif

    private void OnStandardSearch(SearchResponse resp, string data)
    {
      Test(resp != null);

      if (resp != null)
      {
        if (resp.responseHeader != null)
        {
          Log.Debug("TestRetrieveAndRank", "Search | status: {0}, QTime: {1}.", resp.responseHeader.status, resp.responseHeader.QTime);
          if (resp.responseHeader._params != null)
            Log.Debug("TestRetrieveAndRank", "\tSearch | params.q: {0}, params.fl: {1}, params.wt: {2}.", resp.responseHeader._params.q, resp.responseHeader._params.fl, resp.responseHeader._params.wt);
          else
            Log.Debug("TestRetrieveAndRank", "Search | responseHeader.params is null!");
        }
        else
        {
          Log.Debug("TestRetrieveAndRank", "Search | response header is null!");
        }

        if (resp.response != null)
        {
          Log.Debug("TestRetrieveAndRank", "Search | numFound: {0}, start: {1}.", resp.response.numFound, resp.response.start);
          if (resp.response.docs != null)
          {
            if (resp.response.docs.Length == 0)
              Log.Debug("TestRetrieveAndRank", "Search | There are no docs!");
            else
              foreach (Doc doc in resp.response.docs)
              {
                Log.Debug("TestRetrieveAndRank", "\tSearch | id: {0}.", doc.id);

                if (!string.IsNullOrEmpty(doc.title))
                {
                  Log.Debug("ExampleRetrieveAndRank", "Search | title: {0}", doc.title);
                }
                else
                {
                  Log.Debug("ExampleRetrieveAndRank", "Search | title is null");
                }

                if (!string.IsNullOrEmpty(doc.author))
                {
                  Log.Debug("ExampleRetrieveAndRank", "Search | author: {0}", doc.author);
                }
                else
                {
                  Log.Debug("ExampleRetrieveAndRank", "Search | Authors is null");
                }

                if (!string.IsNullOrEmpty(doc.body))
                {
                  Log.Debug("ExampleRetrieveAndRank", "Search | body: {0}.", doc.body);
                }
                else
                {
                  Log.Debug("ExampleRetrieveAndRank", "Search | Body is null");
                }

                if (!string.IsNullOrEmpty(doc.bibliography))
                {
                  Log.Debug("ExampleRetrieveAndRank", "Search | bibliography: {0}.", doc.bibliography);
                }
                else
                {
                  Log.Debug("ExampleRetrieveAndRank", "Search | Bibliography is null");
                }
              }
          }
          else
          {
            Log.Debug("TestRetrieveAndRank", "Search | docs are null!");
          }
        }
        else
        {
          Log.Debug("TestRetrieveAndRank", "Search | response is null!");
        }
      }
      else
      {
        Log.Debug("TestRetrieveAndRank", "Search response is null!");
      }

      standardSearchTested = true;
    }

    private void OnRankedSearch(SearchResponse resp, string data)
    {
      Test(resp != null);

      if (resp != null)
      {
        if (resp.responseHeader != null)
        {
          Log.Debug("TestRetrieveAndRank", "Search | status: {0}, QTime: {1}.", resp.responseHeader.status, resp.responseHeader.QTime);
          if (resp.responseHeader._params != null)
            Log.Debug("TestRetrieveAndRank", "\tSearch | params.q: {0}, params.fl: {1}, params.wt: {2}.", resp.responseHeader._params.q, resp.responseHeader._params.fl, resp.responseHeader._params.wt);
          else
            Log.Debug("TestRetrieveAndRank", "Search | responseHeader.params is null!");
        }
        else
        {
          Log.Debug("TestRetrieveAndRank", "Search | response header is null!");
        }

        if (resp.response != null)
        {
          Log.Debug("TestRetrieveAndRank", "Search | numFound: {0}, start: {1}.", resp.response.numFound, resp.response.start);
          if (resp.response.docs != null)
          {
            if (resp.response.docs.Length == 0)
              Log.Debug("TestRetrieveAndRank", "Search | There are no docs!");
            else
              foreach (Doc doc in resp.response.docs)
              {
                Log.Debug("TestRetrieveAndRank", "\tSearch | id: {0}.", doc.id);

                if (!string.IsNullOrEmpty(doc.title))
                {
                  Log.Debug("ExampleRetrieveAndRank", "Search | title: {0}", doc.title);
                }
                else
                {
                  Log.Debug("ExampleRetrieveAndRank", "Search | title is null");
                }

                if (!string.IsNullOrEmpty(doc.author))
                {
                  Log.Debug("ExampleRetrieveAndRank", "Search | author: {0}", doc.author);
                }
                else
                {
                  Log.Debug("ExampleRetrieveAndRank", "Search | Authors is null");
                }

                if (!string.IsNullOrEmpty(doc.body))
                {
                  Log.Debug("ExampleRetrieveAndRank", "Search | body: {0}.", doc.body);
                }
                else
                {
                  Log.Debug("ExampleRetrieveAndRank", "Search | Body is null");
                }

                if (!string.IsNullOrEmpty(doc.bibliography))
                {
                  Log.Debug("ExampleRetrieveAndRank", "Search | bibliography: {0}.", doc.bibliography);
                }
                else
                {
                  Log.Debug("ExampleRetrieveAndRank", "Search | Bibliography is null");
                }
              }
          }
          else
          {
            Log.Debug("TestRetrieveAndRank", "Search | docs are null!");
          }
        }
        else
        {
          Log.Debug("TestRetrieveAndRank", "Search | response is null!");
        }
      }
      else
      {
        Log.Debug("TestRetrieveAndRank", "Search response is null!");
      }

      rankedSearchTested = true;
    }
  }
}
