using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Linq;

public class SignGUI : MonoBehaviour {
    const float SIGN_READ_DIST_SQR = 4f;
    Player player;
    Text text;
    void Awake () {
        player = FindObjectOfType<Player>();
        text = GetComponent<Text>();
    }
	// Update is called once per frame
	void Update () {
        Sign[] signs = FindObjectsOfType<Sign>();
        double closestDist = signs.Max(
                (Sign s) => (s.transform.position - player.transform.position)
                .sqrMagnitude);
        if (closestDist < SIGN_READ_DIST_SQR) {
            Sign closest = signs.First((Sign s) =>
                    ((s.transform.position - player.transform.position)
                    .sqrMagnitude == closestDist));
            text.text = closest.owner + " says:\n" + closest.message;
        } else {
            text.text = "";
        }
	}
}
