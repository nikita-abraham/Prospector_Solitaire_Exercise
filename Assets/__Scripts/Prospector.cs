using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class Prospector : MonoBehaviour {

	static public Prospector 	S;
	public Deck					deck;
	public TextAsset			deckXML;

	public Layout layout;
	public TextAsset layoutXML;
	public Vector3 layoutCenter;
	public float xOffset = 3;
	public float yOffset = -2.5f;
	public Transform layoutAnchor;

	public CardProspector target;
	public List<CardProspector> tableau;
	public List<CardProspector> discardPile;

	void Awake(){
		S = this;
	}

	public List<CardProspector> drawPile;

	void Start() {
		deck = GetComponent<Deck> ();
		deck.InitDeck (deckXML.text);
		Deck.Shuffle(ref deck.cards); // this shuffles the deck
		//the ref keyword passes a reference to deck.cards, which allows 
		//deck.cards to be modified by Deck.Shuffle()

		layout = GetComponent<Layout> (); //get the layout
		layout.ReadLayout (layoutXML.text); //pass LayoutXML to it
		drawPile = ConvertListCardsToListCardProspectors (deck.cards);
		LayoutGame ();
	}

	//the draw function will pull a single card from the drawPile and return it 
	CardProspector Draw() {
		CardProspector cd = drawPile [0]; //pull the 0th CardProspector
		drawPile.RemoveAt (0); //then remove it from List<> drawPile
		return(cd);
	}

	//layoutGame() positions the initial tableau of cards
	void LayoutGame() {
		//create an empty GameObject to serve as an anchor for the tableau 
		if (layoutAnchor == null) {
			GameObject tGO = new GameObject ("_LayoutAnchor");
			//create an empty gameobject names _LayoutAnchor in the hierarchy
			layoutAnchor = tGO.transform;
			layoutAnchor.transform.position = layoutCenter; //position it
		}

		CardProspector cp;
		foreach (SlotDef tSD in layout.slotDefs) {
			// ^ Iterate through all the SlotDefs in the layout.slotDefs as tSD
			cp = Draw (); // Pull a card from the top (beginning) of the drawpile
			cp.faceUp = tSD.faceUp;  // Set it's faceUp to the value in SlotDef
			cp.transform.parent = layoutAnchor;  // Make its parent layoutAnchor
			//This replaces the previous parent: deck.deckAnchor, which appears
			//as _Deck in the Hierarchy when teh scene is playing.
			cp.transform.localPosition = new Vector3 (
				layout.multiplier.x * tSD.x,
				layout.multiplier.y * tSD.y,
				-tSD.layerID);
			// ^ Set the localPosition of the card based on slotDef
			cp.layoutID = tSD.id;
			cp.slotDef = tSD;
			cp.state = CardState.tableau;
			// CardProspectors in the tableau have the state CardState.tableau

			cp.SetSortingLayerName(tSD.layerName); //set the sorting layer

			tableau.Add (cp); // Add ths CardProspector to the List<> tableau
		}
	}


	List<CardProspector> ConvertListCardsToListCardProspectors (List<Card> lCD) {
		List<CardProspector> lCP = new List<CardProspector>();
		CardProspector tCP;
		foreach (Card tCD in lCD) {
			tCP = tCD as CardProspector;
			lCP.Add(tCP);
		}
		return (lCP);
	}

}
