using UnityEngine;
using System.Collections;

public class Nucleobase : MonoBehaviour {
	public int value;
	Vector3 mouseOffset;
	[HideInInspector]
	public Vector3 startLoc;
	Vector3 offset;
	Vector4 bounds;

	DNA d;
	int g;
	int s;

	void Start() {
		startLoc = transform.position;
		offset = new Vector3 (0, GetComponent<SpriteRenderer> ().bounds.size.y, 0);
		bounds = GameObject.Find ("LabCanvas").GetComponent<UIFramework> ().nbInInventBounds;
		// Used to color the nucleobase correctly
		SpriteRenderer modelRend = GeneValues.Nucleobases[value].GetComponent<SpriteRenderer>();
		GetComponent<SpriteRenderer> ().sprite = modelRend.sprite;
		GetComponent<SpriteRenderer> ().color = modelRend.color;
	}

	void Update() {
		transform.rotation = Quaternion.identity;
	}

	public void SetStartLocToOffsetOfPos() {
		startLoc = transform.position + offset;
	}

	public void SetBounds(Vector4 b) {
		bounds = b;
	}

	void OnMouseDown() {
		mouseOffset = gameObject.transform.position - Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0));
		Cursor.visible = false;
		if (d)
			d.RemoveGene (g, s);
	}

	void OnMouseDrag() 
	{ 
		Vector3 curScreenPoint = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0);
		Vector3 curPosition = Camera.main.ScreenToWorldPoint(curScreenPoint) + mouseOffset;
		if (BoundedPosition (curPosition)) {
			transform.position = curPosition;
		}
	}

	bool BoundedPosition(Vector3 pos) {
		return pos.x >= bounds.x && pos.y >= bounds.y && pos.x <= bounds.x + bounds.z && pos.y <= bounds.y + bounds.w;
	}

	void OnMouseUp()
	{
		Cursor.visible = true;
		if (d) {
			d.AddToGene (g, s, this);
		} else {
			transform.position = startLoc;
		}
	}

	public void QueueSlot(DNA dna, int gene, int slot) {
		if (d != dna) {
			d = dna;
			g = gene;
			s = slot;
		}
		//print ("Queueing: " + d + "  " + g + "  " + s);
	}

	public void UnqueueSlot(DNA dna) {
		if (d == dna)
			d = null;
		//print ("Unqueueing: " + d + "  " + g + "  " + s);
	}

}

