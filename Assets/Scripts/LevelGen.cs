using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using JsonOrg;
using KiiCorp.Cloud.Storage;

using System.Linq;

public class LevelGen : MonoBehaviour {
    GameObject islandPrefab;
    const float MAX_XZ_OFFSET = 8f;
    const float MIN_SIZE = 4f;
    const float MAX_SIZE = 12f;
    const float MIN_HEIGHT = 2f;
    const float MAX_HEIGHT = 8f;
    const int NUM_ISLANDS = 4;

    List<GameObject> spawnedObjects;

    public KiiObject World { get; private set; }

    void Awake () {
        spawnedObjects = new List<GameObject>();
        islandPrefab = Resources.Load<GameObject>("Prefabs/Island");
    }

	// Use this for initialization
    void Start () {
        System.DateTime queryStart = System.DateTime.UtcNow;
        KiiQuery recentQuery = new KiiQuery(
                KiiClause.GreaterThan("expires", queryStart.Ticks));
        KiiQueryResult<KiiObject> result
                = Login.User.Bucket("worlds").Query(recentQuery);
        if (result.Count == 0) {
            Debug.Log("Creating new world.");
            World = GenerateNewLevel();
        } else {
            double lastExp = result.Max(
                    (KiiObject k) => k.GetDouble("expires"));
            KiiObject latest = result.First(
                    (KiiObject k) => (k.GetDouble("expires") == lastExp));
            System.Uri worldUri = new System.Uri(latest.GetString("worldId"));
            Debug.LogWarning(worldUri.ToString());
            KiiObject world = KiiObject.CreateByUri(worldUri);
            world.Refresh();
            World = world;
            Debug.Log("Successfully loaded world.");
            PopulateWorld(world.GetJsonArray("objects"));
        }
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    KiiObject GenerateNewLevel () {
        KiiObject level = Kii.Bucket("worlds").NewKiiObject();
        System.DateTime expiration = System.DateTime.UtcNow.AddHours(0.5);
        JsonArray objects = new JsonArray();
        for (int i = 0; i < NUM_ISLANDS; i++) {
            JsonObject island = LevelGen.GenerateIsland();
            PlaceIsland(island);
            objects.Put(island);
        }

        level["owner"] = Login.User.Username;
        level["expires"] = expiration.Ticks;
        level["objects"] = objects;
        level.Save(false, SaveWorldInUser);
        return level;
    }

    void SaveWorldInUser (KiiObject worldObj, System.Exception e) {
        if (e != null && e as System.NullReferenceException == null) {
            Debug.LogError("Could not save world: " + e.Message);
            return;
        }
        KiiObject levelRecord = Login.User.Bucket("worlds").NewKiiObject();
        levelRecord["worldId"] = worldObj.Uri;
        levelRecord["expires"] = worldObj["expires"];
        levelRecord.Save(false);
    }

    void PopulateWorld (JsonArray worldObjects) {
        for (int i = 0; i < worldObjects.Length(); i++) {
            JsonObject obj = worldObjects.GetJsonObject(i);
            if (obj.GetString("type") == "island") {
                PlaceIsland(obj);
            }
        }
    }

    static JsonObject GenerateIsland () {
        float size = MIN_SIZE + (MAX_SIZE - MIN_SIZE) * Random.value;
        float height = MIN_HEIGHT + (MAX_HEIGHT - MIN_HEIGHT)
                * Random.value;
        float xPos = -MAX_XZ_OFFSET + 2f * MAX_XZ_OFFSET * Random.value;
        float zPos = -MAX_XZ_OFFSET + 2f * MAX_XZ_OFFSET * Random.value;
        JsonObject obj = new JsonObject();
        obj.Put("type", "island");
        obj.Put("size", size);
        obj.Put("height", height);
        obj.Put("xPos", xPos);
        obj.Put("zPos", zPos);
        return obj;
    }

    void PlaceIsland (JsonObject islandJson) {
        float size = (float) islandJson.GetDouble("size");
        float height = (float) islandJson.GetDouble("height");
        float xPos = (float) islandJson.GetDouble("xPos");
        float zPos = (float) islandJson.GetDouble("zPos");

        GameObject island = Instantiate<GameObject>(islandPrefab);
        island.transform.position = new Vector3(xPos, 0f, zPos);
        island.transform.localScale = new Vector3(size, height * 2f, size);
        spawnedObjects.Add(island);
    }

    public Vector3 FindSuitableSpawn () {
        Vector3 tempSpawn = Vector3.zero;
        for (int iter = 0; iter < 100; iter++) {
            float maxCoord = MAX_XZ_OFFSET + MAX_SIZE / 2f;
            float xPos = -maxCoord + 2f * maxCoord * Random.value;
            float zPos = -maxCoord + 2f * maxCoord * Random.value;
            Vector3 rayStart = new Vector3(xPos, MAX_HEIGHT + 1f, zPos);
            RaycastHit hit;
            Physics.Raycast(new Ray(rayStart, Vector3.down), out hit);
            if (hit.collider.gameObject.name == "Island(Clone)") {
                tempSpawn = hit.point;
                break;
            }
        }
        return tempSpawn;
    }
}
