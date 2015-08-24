using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

public class InputMgr : MonoBehaviour {
    GraphicRaycaster gr;
    public bool JustTapped {get; private set;}
    public Vector2 TapPos {get; private set;}
    public Vector2 NormalizedTapPos {get; private set;}
    public float TapLength {get; private set;}
    const float MAX_TAP_DIST = 25f;

    Vector2 tapStart;
    float tapStartTime;
    bool pressing;

	// Use this for initialization
	void Awake () {
        gr = GameObject.Find("GUICanvas").GetComponent<GraphicRaycaster>();
        JustTapped = false;
        TapPos = Vector2.zero;
        NormalizedTapPos = Vector2.zero;
        TapLength = 0f;
        tapStart = Vector2.zero;
        tapStartTime = Time.time;
        pressing = false;
	}
	
	// Update is called once per frame
	void Update () {
        ClearTaps();
        if (Input.touchCount == 1) {
            Touch t = Input.GetTouch(0);
            if (t.phase == TouchPhase.Began) {
                Pressed(t.position);
            } else if (t.phase == TouchPhase.Ended) {
                Released(t.position);
            }
        } else if (Input.GetMouseButtonDown(0)) {
            Pressed(new Vector2(Input.mousePosition.x, Input.mousePosition.y));
        } else if (Input.GetMouseButtonUp(0)) {
            Released(new Vector2(Input.mousePosition.x, Input.mousePosition.y));
        }
	}

    void ClearTaps () {
        JustTapped = false;
    }

    void Pressed (Vector2 screenPos) {
        tapStart = screenPos;
        tapStartTime = Time.time;

        PointerEventData ped 
            = new PointerEventData(FindObjectOfType<EventSystem>());
        ped.position = screenPos;
        List<RaycastResult> results = new List<RaycastResult>();
        gr.Raycast(ped, results);
        if (results.Count > 0) {
            return;
        }
        pressing = true;
    }

    void Released (Vector2 screenPos) {
        if (!pressing) {
            return;
        }
        pressing = false;
        if ((screenPos - tapStart).magnitude > MAX_TAP_DIST) {
            return;
        }
        JustTapped = true;
        TapPos = tapStart;
        NormalizedTapPos = new Vector2(TapPos.x / Screen.width,
                                       TapPos.y / Screen.height);
        TapLength = Time.time - tapStartTime;
    }
}
