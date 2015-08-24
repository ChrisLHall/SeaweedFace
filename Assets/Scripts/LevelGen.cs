using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using JsonOrg;
using KiiCorp.Cloud.Storage;

using System.Linq;

public class LevelGen : MonoBehaviour {
    GameObject islandPrefab;
    GameObject signPrefab;
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
        signPrefab = Resources.Load<GameObject>("Prefabs/Sign");
    }

	// Use this for initialization
    void Start () {
        System.DateTime queryStart = System.DateTime.UtcNow;
        KiiClause recentClause = KiiClause.GreaterThan("expires",
                                                       queryStart.Ticks);
        KiiClause mineClause = KiiClause.Equals("owner", Login.User.Username);
        KiiQuery recentQuery = new KiiQuery(KiiClause.And(recentClause,
                                                          mineClause));
        KiiQueryResult<KiiObject> result
                = Kii.Bucket("worlds").Query(recentQuery);
        if (result.Count == 0) {
            Debug.Log("Creating new world.");
            World = GenerateNewLevel();
        } else {
            KiiObject latest = result[0];
            World = latest;
            Debug.Log("Successfully loaded world.");
            PopulateWorld(latest.GetJsonArray("objects"));
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
        level.Save(false, SaveWorldCallback);
        return level;
    }

    void SaveWorldCallback (KiiObject worldObj, System.Exception e) {
        if (e != null && e as System.NullReferenceException == null) {
            Debug.LogError("Could not save world: " + e.Message);
            return;
        }
    }

    void PopulateWorld (JsonArray worldObjects) {
        for (int i = 0; i < worldObjects.Length(); i++) {
            JsonObject obj = worldObjects.GetJsonObject(i);
            if (obj.GetString("type") == "island") {
                PlaceIsland(obj);
            } else if (obj.GetString("type") == "sign") {
                PlaceSign(obj);
            }
        }
    }

    void ClearSpawnedObjects () {
        while (spawnedObjects.Count > 0) {
            GameObject obj = spawnedObjects[0];
            spawnedObjects.RemoveAt(0);
            Destroy(obj);
        }
    }

    public void AddSign (Vector3 pos, string owner, string message) {
        JsonObject newSign = CreateSign(pos, owner, message);
        AddObjectRefreshSave(newSign);
    }

    void AddObjectRefreshSave (JsonObject newObj) {
        World.Refresh();
        JsonArray ja = World.GetJsonArray("objects");
        ja.Put(newObj);
        World["objects"] = ja;
        ClearSpawnedObjects();
        PopulateWorld(World.GetJsonArray("objects"));
        World.Save();
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

    static JsonObject CreateSign(Vector3 position, string owner,
                                 string message) {
        JsonObject obj = new JsonObject();
        obj.Put("type", "sign");
        obj.Put("x", position.x);
        obj.Put("y", position.y);
        obj.Put("z", position.z);
        obj.Put("owner", owner);
        obj.Put("message", message);
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

    void PlaceSign (JsonObject signJson) {
        float x = (float) signJson.GetDouble("x");
        float y = (float) signJson.GetDouble("y");
        float z = (float) signJson.GetDouble("z");
        string owner = signJson.GetString("owner");
        string message = signJson.GetString("message");

        GameObject sign = Instantiate<GameObject>(signPrefab);
        sign.transform.position = new Vector3(x, y, z);
        Sign signComp = sign.GetComponent<Sign>();
        signComp.owner = owner;
        signComp.message = message;
        spawnedObjects.Add(sign);
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

    public void Travel (bool goHome) {
        Debug.LogError("WOT");
        System.DateTime queryStart = System.DateTime.UtcNow;
        KiiClause recentClause = KiiClause.GreaterThan("expires",
                                                       queryStart.Ticks);
        KiiQuery worldsQuery;
        if (goHome) {
            KiiClause mineClause = KiiClause.Equals("owner",
                                                    Login.User.Username);
            worldsQuery = new KiiQuery(KiiClause.And(recentClause,
                                                     mineClause));
        } else {
            KiiClause notMineClause = KiiClause.NotEquals("owner",
                                                          Login.User.Username);
            worldsQuery = new KiiQuery(KiiClause.And(recentClause,
                                                     notMineClause));
        }

        KiiQueryResult<KiiObject> result
            = Kii.Bucket("worlds").Query(worldsQuery);
        if (result.Count == 0) {
            if (!goHome) {
                Debug.Log("No other places! Going back home.");
                Travel(true);
            } else {
                Debug.LogError("Could not find home!");
            }
        } else {
            ClearSpawnedObjects();
            KiiObject latest = result[0];
            World = latest;
            Debug.Log("Successfully travelled " + (goHome ? "home." : "away."));
            PopulateWorld(latest.GetJsonArray("objects"));
        }
    }
}
