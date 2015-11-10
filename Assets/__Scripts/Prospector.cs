﻿using UnityEngine;
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

		//set up the initial target card
		MoveToTarget (Draw ());

		//set up the draw pile
		UpdateDrawPile ();

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

	//CardClicked is called any time a card in the game is clicked
	public void CardClicked(CardProspector cd) {
		//the reaction is determined by the state of the clicked card
		switch (cd.state) {
		case CardState.target:
			//clicking the target card does nothing
			break;
		case CardState.drawpile:
			//clicking any card in the drawPile will draw the next card
			MoveToDiscard (target); //moves the target to the discardPile
			MoveToTarget (Draw ()); //moves the next drawn card to the target
			UpdateDrawPile (); //restacks the drawPile
			break;
		case CardState.tableau:
			//clicking a card in the tableau will check if its a valid play
			break;
		}
	}

	// Moves the current target to the discardPile
	void MoveToDiscard(CardProspector cd) {
		// Set the satte of the card to discard
		cd.state = CardState.discard;
		discardPile.Add (cd); // Add it to the discardPile List<>
		cd.transform.parent = layoutAnchor; // Update it's transform parent
		cd.transform.localPosition = new Vector3 (
			layout.multiplier.x * layout.discardPile.x,
			layout.multiplier.y * layout.discardPile.y,
			-layout.discardPile.layerID + 0.5f);
		// ^ Position it on the discardPile
		cd.faceUp = true;
		// Place it on top of the pile for depth sorting
		cd.SetSortingLayerName (layout.discardPile.layerName);
		cd.SetSortOrder (-100 + discardPile.Count);
	}
	
	// Make cd the new target card
	void MoveToTarget (CardProspector cd) {
		// If there is currently a target card, move it to discardPile
		if (target != null) MoveToDiscard (target);
		target = cd; // cd is the new target
		cd.state = CardState.target;
		cd.transform.parent = layoutAnchor;
		// Move to the target position
		cd.transform.localPosition = new Vector3(
			layout.multiplier.x * layout.discardPile.x,
			layout.multiplier.y * layout.discardPile.y,
			-layout.discardPile.layerID);
		cd.faceUp = true; // Make it face-up
		// Set the depth sorting
		cd.SetSortingLayerName (layout.discardPile.layerName);
		cd.SetSortOrder (0);
	}
	
	// Arrange all the cards of the drawPile to show how many are left
	void UpdateDrawPile () {
		CardProspector cd;
		// Go through all the cards of the drawPile
		for (int i=0; i<drawPile.Count; i++) {
			cd = drawPile[i];
			cd.transform.parent = layoutAnchor;
			// Position it correctly with the layout.drawPile.stagger
			Vector2 dpStagger = layout.drawPile.stagger;
			cd.transform.localPosition = new Vector3(
				layout.multiplier.x * (layout.drawPile.x + i*dpStagger.x),
				layout.multiplier.y * (layout.drawPile.y + i*dpStagger.y),
				-layout.drawPile.layerID + 0.1f*i);
			cd.faceUp = false; // Make them all face down
			cd.state = CardState.drawpile;
			// Set depth sorting
			cd.SetSortingLayerName(layout.drawPile.layerName);
			cd.SetSortOrder(-10*i);
		}
	}

}
