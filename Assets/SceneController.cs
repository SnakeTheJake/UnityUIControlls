using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MJH.UIComponents;


public class SceneController : MonoBehaviour {

    public SwipeCard card;

	// Use this for initialization
	public void Start () {
        if (card)
            card.onSwiped += OnCardSwipe;
	}
	
    private void OnCardSwipe(bool confirmed)
    {
        if (confirmed)
            print("Swiped right, good looking!");
        else
            print("Swiped left, no deal!");
    }

}
