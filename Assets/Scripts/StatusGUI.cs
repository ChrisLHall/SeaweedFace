using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class StatusGUI : MonoBehaviour {
    LevelGen lg;
    Text text;
    void Awake () {
        lg = FindObjectOfType<LevelGen>();
        text = GetComponent<Text>();
    }
    // Update is called once per frame
    void Update () {
        System.DateTime exp
                = new System.DateTime(lg.World.GetLong("expires"));
        System.TimeSpan ts = System.DateTime.UtcNow - exp;
        text.text = "My prestige: " + lg.Prestige + "\n"
                + "World ends: " + ts.ToString().Substring(0, 11);
    }
}
