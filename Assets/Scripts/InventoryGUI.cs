using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class InventoryGUI : MonoBehaviour {
    TapTarget tt;
    LevelGen lg;
    List<GameObject> children;

    const float MIN_SIGN_SPACING_SQR = 4f;
    const float MIN_ITEM_SPACING_SQR = 2f;

    bool holdingItem;
    string heldItemType;
    int heldItemInitVal;

    bool ttWasJustActive;
    Vector3 lastTTPos;
    InputField message;
    Image itemImage;
    Image pickupDropIcon;
    Text valueText;
    public Sprite pickupSprite;
    public Sprite dropSprite;

    public AudioClip pickupSound;
    public AudioClip dropSound;

    void Awake () {
        holdingItem = false;
        heldItemType = "bush";
        heldItemInitVal = 1;

        ttWasJustActive = false;
    }

	// Use this for initialization
    void Start () {
        tt = FindObjectOfType<TapTarget>();
        lg = FindObjectOfType<LevelGen>();
        children = new List<GameObject>();
        for (int index = 0; index < transform.childCount; index++) {
            children.Add(transform.GetChild(index).gameObject);
        }
        message = GetComponentInChildren<InputField>();
        
        itemImage = transform.FindChild("ItemImage").GetComponent<Image>();
        pickupDropIcon = itemImage.transform.FindChild("PickupDropImage")
                .GetComponent<Image>();
        valueText = itemImage.transform.FindChild("ItemValue")
                .GetComponent<Text>();

        lastTTPos = tt.transform.position;
	}
	
	// Update is called once per frame
	void Update () {
	    if (tt.Active) {
            Show();
        } else {
            Hide();
        }

        if (tt.Active
                && (!ttWasJustActive || lastTTPos != tt.transform.position)) {
            UpdateItemGUI();
        }

        ttWasJustActive = tt.Active;
        lastTTPos = tt.transform.position;
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
        if (FarFromOtherSigns(signPos) && message.text.Trim().Length > 0) {
            FindObjectOfType<LevelGen>().AddSign(signPos, Login.User.Username,
                                                 message.text);
            message.text = "";
        }
    }

    void UpdateItemGUI ()
    {
        if (holdingItem) {
            itemImage.sprite = Resources.Load<Sprite> ("Sprites/" + heldItemType);
            valueText.text = heldItemInitVal.ToString ();
            itemImage.color = Color.white;
        }
        else {
            valueText.text = "";
            itemImage.color = Color.clear;
        }
        Item closest = FindClosestItem ();
        if (CanPlaceItemHere (closest, tt.transform.position)) {
            pickupDropIcon.sprite = dropSprite;
            pickupDropIcon.color = Color.white;
        }
        else
            if (CanPickupItemHere (closest, tt.transform.position)) {
                pickupDropIcon.sprite = pickupSprite;
                pickupDropIcon.color = Color.white;
                itemImage.sprite = Resources.Load<Sprite> ("Sprites/" + closest.itemName);
                valueText.text = closest.initValue.ToString ();
                itemImage.color = Color.white;
            }
            else {
                pickupDropIcon.color = Color.clear;
            }
    }

    public void PickupDropItem () {
        if (!tt.Active) {
            return;
        }

        Item closest = FindClosestItem();
        if (CanPlaceItemHere(closest, tt.transform.position)) {
            lg.AddItem(tt.transform.position, heldItemType,
                       heldItemInitVal);
            holdingItem = false;
            UpdateItemGUI();
            AudioSource audio = GetComponent<AudioSource>();
            audio.clip = dropSound;
            audio.Play();
        } else if (CanPickupItemHere(closest, tt.transform.position)) {
            lg.UpdateItem(closest.transform.position, closest.itemName,
                          closest.initValue, closest.level + 1);
            holdingItem = true;
            heldItemType = closest.itemName;
            heldItemInitVal = closest.initValue;
            UpdateItemGUI();
            AudioSource audio = GetComponent<AudioSource>();
            audio.clip = pickupSound;
            audio.Play();
        }
    }

    Item FindClosestItem () {
        Item[] items = FindObjectsOfType<Item>();
        Item result = null;
        float minSqrDist = MIN_ITEM_SPACING_SQR + 1f;
        foreach (Item item in items) {
            float sqrMag = (item.transform.position
                            - tt.transform.position).sqrMagnitude;
            if (sqrMag <= MIN_ITEM_SPACING_SQR && sqrMag < minSqrDist) {
                result = item;
                minSqrDist = sqrMag;
            }
        }
        return result;
    }

    bool CanPickupItemHere (Item closestItem, Vector3 selectPos) {
        return !holdingItem
                && lg.World.GetString("owner") != Login.User.Username
                && closestItem != null
                && (selectPos - closestItem.transform.position)
                .sqrMagnitude <= MIN_ITEM_SPACING_SQR;
    }

    bool CanPlaceItemHere (Item closestOtherItem, Vector3 selectPos) {
        return holdingItem
                && lg.World.GetString("owner") == Login.User.Username
                && (closestOtherItem == null
                || (selectPos - closestOtherItem.transform.position)
                .sqrMagnitude > MIN_ITEM_SPACING_SQR);
    }
}
