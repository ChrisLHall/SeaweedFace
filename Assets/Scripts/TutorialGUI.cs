using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TutorialGUI : MonoBehaviour {
    public Sprite[] screens;

    int index;
    GameObject imageObj;
    Image img;

    void Awake () {
        index = -1;
        img = GetComponentInChildren<Image>();
        imageObj = img.gameObject;
    }

	// Use this for initialization
	void Start () {
        imageObj.SetActive(false);
	}
	
	public void Advance () {
        index++;
        if (index >= screens.Length) {
            index = -1;
            imageObj.SetActive(false);
        } else {
            imageObj.SetActive(true);
            img.sprite = screens[index];
        }
    }
}
