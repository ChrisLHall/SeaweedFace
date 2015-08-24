using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using JsonOrg;
using KiiCorp.Cloud.Storage;

using System.Linq;

public class LevelGen : MonoBehaviour {
    Player player;
    TapTarget targ;

    GameObject islandPrefab;
    GameObject signPrefab;
    GameObject itemPrefab;
    GameObject villagePrefab;
    const float MAX_XZ_OFFSET = 8f;
    const float MIN_SIZE = 4f;
    const float MAX_SIZE = 12f;
    const float MIN_HEIGHT = 3f;
    const float MAX_HEIGHT = 10f;
    const int NUM_ISLANDS = 4;

    const float EPSILON = 0.0001f;

    readonly string[] ITEMS = new string[] {
        "bush",
        "rocks"
    };
    readonly int[] ITEM_VALS = new int[] {
        1,
        3
    };

    List<GameObject> spawnedObjects;

    public KiiObject World { get; private set; }
    public int Prestige { get; private set; }

    void Awake () {
        player = FindObjectOfType<Player>();
        targ = FindObjectOfType<TapTarget>();

        spawnedObjects = new List<GameObject>();
        islandPrefab = Resources.Load<GameObject>("Prefabs/Island");
        signPrefab = Resources.Load<GameObject>("Prefabs/Sign");
        itemPrefab = Resources.Load<GameObject>("Prefabs/Item");
        villagePrefab = Resources.Load<GameObject>("Prefabs/Village");
        Prestige = 1;
    }

	// Use this for initialization
    void Start () {
        RecalcPlayerPrestige();
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
            Village.RecalculateAll();
        }
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    KiiObject GenerateNewLevel (bool createFakeUser = false) {
        KiiObject level = Kii.Bucket("worlds").NewKiiObject();
        System.DateTime expiration = System.DateTime.UtcNow.AddMinutes(20);
        JsonArray objects = new JsonArray();
        for (int i = 0; i < NUM_ISLANDS; i++) {
            JsonObject island = LevelGen.GenerateIsland();
            PlaceIsland(island);
            objects.Put(island);
        }
        
        int prestigeLeft = createFakeUser ? 5 : Prestige;
        int numVillages = NumVillagesSpawned(prestigeLeft);
        for (int i = 0; i < numVillages; i++) {
            JsonObject village = RandomGenVillage();
            PlaceVillage(village);
            objects.Put(village);
        }
        string username = createFakeUser
                ? ("NOBODY" + Mathf.FloorToInt(1000f * Random.value).ToString())
                : Login.User.Username;
        JsonObject helloSign = CreateSign(FindSuitableSpawn(),
                                          username,
                                          "Welcome to " + username
                                          + " Island!");
        PlaceSign(helloSign);
        objects.Put(helloSign);
        while (prestigeLeft > 0) {
            JsonObject item = RandomGenItem(prestigeLeft);
            prestigeLeft -= item.GetInt("initValue");
            PlaceItem(item);
            objects.Put(item);
        }

        level["owner"] = username;
        level["expires"] = expiration.Ticks;
        level["objects"] = objects;
        Village.RecalculateAll();
        level["worldPrestige"] = CountWorldPrestige();
        level.Save(false, SaveWorldCallback);
        return level;
    }

    void SaveWorldCallback (KiiObject worldObj, System.Exception e) {
        if (e != null && e as System.NullReferenceException == null) {
            Debug.LogError("Could not save the world: " + e.Message);
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
            } else if (obj.GetString("type") == "item") {
                PlaceItem(obj);
            } else if (obj.GetString("type") == "village") {
                PlaceVillage(obj);
            }
        }
    }

    void ClearSpawnedObjects () {
        while (spawnedObjects.Count > 0) {
            GameObject obj = spawnedObjects[0];
            spawnedObjects.RemoveAt(0);
            obj.SetActive(false);
            Destroy(obj);
        }
    }

    public void AddSign (Vector3 pos, string owner, string message) {
        JsonObject newSign = CreateSign(pos, owner, message);
        AddObjectRefreshSave(newSign);
    }

    public void AddItem (Vector3 pos, string type, int initVal) {
        JsonObject newItem = CreateItem(pos, type, initVal, 0);
        AddObjectRefreshSave(newItem);
    }

    public void AddVillage (Vector3 pos) {
        JsonObject newVillage = CreateVillage(pos);
        AddObjectRefreshSave(newVillage);
    }

    public void UpdateItem (Vector3 pos, string type, int initVal,
                            int newLevel) {
        JsonObject newItem = CreateItem(pos, type, initVal, newLevel);
        AddObjectRefreshSave(newItem, true);
    }

    public void RefreshWorld () {
        World.Refresh();
        ClearSpawnedObjects();
        PopulateWorld(World.GetJsonArray("objects"));
        Village.RecalculateAll();
        RecalcPlayerPrestige();
    }

    void AddObjectRefreshSave (JsonObject newObj, bool tryReplace = false) {
        World.Refresh();
        JsonArray ja = World.GetJsonArray("objects");
        JsonArray newJA = new JsonArray();
        if (tryReplace) {
            Vector3 currentPos = new Vector3((float) newObj.GetDouble("x"),
                                             (float) newObj.GetDouble("y"),
                                             (float) newObj.GetDouble("z"));
            for (int i = 0; i < ja.Length(); i++) {
                JsonObject o = ja.GetJsonObject(i);
                if (o.GetString("type") != "item"
                        && o.GetString("type") != "sign") {
                    newJA.Put(o);
                    continue;
                }
                Vector3 pos = new Vector3((float) o.GetDouble("x"),
                                          (float) o.GetDouble("y"),
                                          (float) o.GetDouble("z"));
                if ((pos - currentPos).sqrMagnitude < EPSILON
                        && newObj.GetString("type") == o.GetString("type")) {
                    // Skip if position and type are equal
                    continue;
                }
                newJA.Put(o);
            }
        } else {
            newJA = ja;
        }
        newJA.Put(newObj);
        World["objects"] = newJA;
        ClearSpawnedObjects();
        PopulateWorld(World.GetJsonArray("objects"));

        Village.RecalculateAll();
        World["worldPrestige"] = CountWorldPrestige();
        World.Save();
        RecalcPlayerPrestige();
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

    static JsonObject CreateItem(Vector3 position, string itemType,
                                 int initValue, int level) {
        JsonObject obj = new JsonObject();
        obj.Put("type", "item");
        obj.Put("itemType", itemType);
        obj.Put("x", position.x);
        obj.Put("y", position.y);
        obj.Put("z", position.z);
        obj.Put("initValue", initValue);
        obj.Put("level", level);
        return obj;
    }

    static JsonObject CreateVillage(Vector3 position) {
        JsonObject obj = new JsonObject();
        obj.Put("type", "village");
        obj.Put("x", position.x);
        obj.Put("y", position.y);
        obj.Put("z", position.z);
        return obj;
    }

    JsonObject RandomGenItem(int maxVal) {
        if (maxVal <= 0) {
            Debug.LogError("Tried to generate an item of value 0");
            return null;
        }
        int index;
        while (true) {
            index = Mathf.FloorToInt(Random.value * ITEMS.Length);
            if (ITEM_VALS[index] <= maxVal) {
                break;
            }
        }
        Vector3 position = FindSuitableSpawn();
        return CreateItem(position, ITEMS[index], ITEM_VALS[index], 0);
    }

    JsonObject RandomGenVillage() {
        Vector3 position = FindSuitableSpawn();
        return CreateVillage(position);
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

    void PlaceItem (JsonObject itemJson) {
        string type = itemJson.GetString("itemType");
        float x = (float) itemJson.GetDouble("x");
        float y = (float) itemJson.GetDouble("y");
        float z = (float) itemJson.GetDouble("z");
        int initValue = itemJson.GetInt("initValue");
        int level = itemJson.GetInt("level");
        
        GameObject item = Instantiate<GameObject>(itemPrefab);
        item.transform.position = new Vector3(x, y, z);
        Item itemComp = item.GetComponent<Item>();
        itemComp.itemName = type;
        itemComp.initValue = initValue;
        itemComp.level = level;
        spawnedObjects.Add(item);
    }

    void PlaceVillage (JsonObject villageJson) {
        float x = (float) villageJson.GetDouble("x");
        float y = (float) villageJson.GetDouble("y");
        float z = (float) villageJson.GetDouble("z");
        
        GameObject village = Instantiate<GameObject>(villagePrefab);
        village.transform.position = new Vector3(x, y, z);
        spawnedObjects.Add(village);
    }

    public Vector3 FindSuitableSpawn () {
        Vector3 tempSpawn = Vector3.zero;
        for (int iter = 0; iter < 1000; iter++) {
            float maxCoord = MAX_XZ_OFFSET + MAX_SIZE / 2f;
            float xPos = -maxCoord + 2f * maxCoord * Random.value;
            float zPos = -maxCoord + 2f * maxCoord * Random.value;
            Vector3 rayStart = new Vector3(xPos, MAX_HEIGHT + 1f, zPos);
            RaycastHit hit;
            Physics.Raycast(new Ray(rayStart, Vector3.down), out hit);
            // When a GameObject is destroyed, it's == operator is overloaded
            // to compare true to null
            if (hit.collider.gameObject.name == "Island(Clone)"
                    && hit.collider.gameObject.activeInHierarchy) {
                tempSpawn = hit.point;
                break;
            }
        }
        return tempSpawn;
    }

    public void Travel (bool goHome) {
        RecalcPlayerPrestige();
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

        targ.Deselect();
        if (goHome && result.Count == 0) {
            Debug.LogError("Could not find home! Making a new one.");
            ClearSpawnedObjects();
            World = GenerateNewLevel();
        } else if (!goHome && result.Count < 5) {
            Debug.Log("Traveling to new anonymous world.");
            ClearSpawnedObjects();
            World = GenerateNewLevel(true);
        } else {
            ClearSpawnedObjects();
            int i = Mathf.FloorToInt(Random.value * result.Count);
            KiiObject latest = result[i];
            World = latest;
            Debug.Log("Successfully travelled " + (goHome ? "home." : "away."));
            PopulateWorld(latest.GetJsonArray("objects"));
            Village.RecalculateAll();
        }
        player.FindStartPos();
    }

    void RecalcPlayerPrestige () {
        // TODO
        Prestige = 3;
    }

    /** Please only call this after Village.RecalculateAll(). */
    int CountWorldPrestige () {
        Village[] villages = FindObjectsOfType<Village>();
        int result = 0;
        foreach (Village v in villages) {
            if (v.CanDefend) {
                result++;
            }
        }
        return result;
    }

    static int NumVillagesSpawned (int prestige) {
        return 3 + Mathf.FloorToInt(prestige / 10f);
    }
}
