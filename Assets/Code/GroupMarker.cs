using UnityEngine;
using System.Collections;

public class GroupMarker : MonoBehaviour {

	SpriteRenderer sRend;
	Color color;
	static float FADE_RATE = 2f;

	void Start () {
		sRend = GetComponent<SpriteRenderer> ();	
		color = sRend.color;
	}
	
	// Update is called once per frame
	void Update () {
		float newA = color.a + Time.deltaTime * FADE_RATE;
		if (newA > 1) {
			newA = 2 - newA;
			FADE_RATE *= -1;
		} else if (newA < 0) {
			newA *= -1;
			FADE_RATE *= -1;
		}
		color.a = newA;
		sRend.color = color;
	}
}
