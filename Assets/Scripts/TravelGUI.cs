using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TravelGUI : MonoBehaviour {
    LevelGen lg;
    Image image;
    Text villageNameText;
    public Sprite travelSpr;
    public Sprite goHomeSpr;

    bool IsHome {
        get {
            return lg.World.GetString("owner") == Login.User.Username;
        }
    }

	// Use this for initialization
	void Awake () {
        lg = FindObjectOfType<LevelGen>();
        image = GetComponent<Image>();
        villageNameText = transform.parent.FindChild("VillageText")
                .GetComponent<Text>();
	}
	
	// Update is called once per frame
	void Update () {
        string nameStr = lg.World.GetString("owner") + " Island";
	    if (IsHome) {
            image.sprite = travelSpr;
            nameStr += " (Yours!)";
        } else {
            image.sprite = goHomeSpr;
        }
        villageNameText.text = nameStr;
	}

    public void Travel () {
        lg.Travel(!IsHome);
    }
}
