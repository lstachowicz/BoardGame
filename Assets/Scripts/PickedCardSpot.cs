using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickedCardSpot : MonoBehaviour
{
    public int pickedCardShift = 40;

    // Start is called before the first frame update
    void Start()
    {

    }

    void Update()
    {
        Board board = FindObjectOfType<Board>();
        PickedCardSpot picketCardSpotScript = GetComponent<PickedCardSpot>();


        int number_of_cards = board.mintPlayer.cards.Count;
        for (int i = 0; i < number_of_cards; ++i)
        {
            board.mintPlayer.cards[i].GetComponent<SpriteRenderer>().sortingOrder = i + 2;
            board.mintPlayer.cards[i].transform.position = new Vector3()
            {
                x = picketCardSpotScript.transform.position.x + i * pickedCardShift,
                y = picketCardSpotScript.transform.position.y,
                z = picketCardSpotScript.transform.position.z + (number_of_cards - i) + 1
            };

        }
    }
}
