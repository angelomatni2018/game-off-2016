using UnityEngine;
using System.Collections;

public class DNASlot : MonoBehaviour {
	public int gene;
	public int slot;
	public int defaultBaseValues;
	public Collider2D slotCol;
	DNA dna;
	Nucleobase nb;

	void Awake() {
		dna = GetComponentInParent<DNA> ();
		slotCol = GetComponent<Collider2D> ();
	}

	public void Setup(int baseVal) {
		GameObject nucleobase = GameObject.Instantiate (Resources.Load ("bases/nucleobase"), new Vector3(), Quaternion.identity, transform) as GameObject;
		nucleobase.transform.localPosition = slotCol.offset;
		//nucleobase.transform.localScale = Vector3.Scale (GeneValues.basePrefab.transform.localScale, nucleobase.transform.lossyScale);
		nucleobase.transform.localScale = Vector3.Scale (((GameObject)Resources.Load ("bases/nucleobase")).transform.localScale, new Vector3 (1/nucleobase.transform.parent.lossyScale.x, 1/nucleobase.transform.parent.lossyScale.y, 0));
		nucleobase.GetComponent<Nucleobase> ().value = baseVal;
		nb = nucleobase.GetComponent<Nucleobase>();
	}

	public void AddBase(Nucleobase b) {
		nb = b;
	}

	public void RemoveBase() {
		nb = null;
	}

	public Nucleobase GetBase() {
		return nb;
	}

	void OnTriggerEnter2D(Collider2D col) {
		if (col.tag.Equals ("Nucleobase")) {
			Nucleobase b = col.GetComponent<Nucleobase> ();
			//print ("Enter: " + gene + "  " + slot + "  " + b.value);
			b.QueueSlot(dna, gene, slot);
		}
	}

	void OnTriggerExit2D(Collider2D col) {
		if (col.tag.Equals ("Nucleobase")) {
			//print ("Exit: " + gene + "  " + slot + "  " + col.GetComponent<Nucleobase> ().value);
			col.GetComponent<Nucleobase> ().UnqueueSlot (dna);
		}
	}
}
