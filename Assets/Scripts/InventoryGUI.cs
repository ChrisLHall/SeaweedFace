using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class InventoryGUI : MonoBehaviour {
    TapTarget tt;
    List<GameObject> children;

    const float MIN_SIGN_SPACING_SQR = 4f;
    const float MIN_ITEM_SPACING_SQR = 2f;

    bool holdingItem;
    string heldItemType;
    int heldItemInitVal;
    int heldItemLevel;

    InputField message;

    void Awake () {
        tt = FindObjectOfType<TapTarget>();
        children = new List<GameObject>();
        for (int index = 0; index < transform.childCount; index++) {
            children.Add(transform.GetChild(index).gameObject);
        }
        message = GetComponentInChildren<InputField>();

        holdingItem = false;
        heldItemType = "bush";
        heldItemInitVal = 1;
        heldItemLevel = 0;
    }

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	    if (tt.Active) {
            Show();
        } else {
            Hide();
        }
	}

    public void Show () {
        foreach (GameObject o in children) {
            o.SetActive(true);
        }
    }

    public void Hide () {
        foreach (GameObject o in children) {
            o.SetActive(false);
        }
    }

    bool FarFromOtherSigns (Vector3 position) {
        Sign[] signs = FindObjectsOfType<Sign>();
        foreach (Sign sign in signs) {
            if ((sign.transform.position - transform.position).sqrMagnitude
                    < MIN_SIGN_SPACING_SQR) {
                return false;
            }
        }
        return true;
    }

    public void PlaceSign () {
        if (!tt.Active) {
            return;
        }
        Vector3 signPos = tt.transform.position;
        if (FarFromOtherSigns(signPos)) {
            FindObjectOfType<LevelGen>().AddSign(signPos, Login.User.Username,
                                                 message.text);
        }
    }

    public void PickupDropItem () {
        if (!tt.Active) {
            return;
        }

        Item closest = FindClosestItem();
        if (holdingItem) {
            if (closest == null
                    || (tt.transform.position - closest.transform.position)
                    .sqrMagnitude > MIN_ITEM_SPACING_SQR) {
                // TODO @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
            }
        } else {

        }
    }

    Item FindClosestItem () {
        Item[] items = FindObjectsOfType<Item>();
        Item result = null;
        float minSqrDist = MIN_ITEM_SPACING_SQR + 1f;
        foreach (Item item in items) {
            float sqrMag = (item.transform.position
                            - transform.position).sqrMagnitude;
            if (sqrMag <= MIN_ITEM_SPACING_SQR && sqrMag < minSqrDist) {
                result = item;
                minSqrDist = sqrMag;
            }
        }
        return result;
    }
}
