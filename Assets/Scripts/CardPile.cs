using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class CardPile : MonoBehaviour
{
    public List<GameObject> prefabsCards = new List<GameObject>();

    public List<Card> cardsOnPile = new List<Card>();
    public List<Card> cardsReturned = new List<Card>();
    public List<Card> cardsInHands = new List<Card>();

    public bool cardSelected;

    public void Start()
    {
        int sortingLayer = GetComponent<SpriteRenderer>().sortingOrder;

        foreach (GameObject cardObject in prefabsCards)
        {
            GameObject gameObjectInstantiate = Instantiate(cardObject
                , new Vector3(transform.position.x, transform.position.y, transform.position.z)
                , Quaternion.identity);
            cardsOnPile.Add(gameObjectInstantiate.GetComponent<Card>());

            gameObjectInstantiate.transform.parent = transform;
        }

        ShufflePile();
    }

    private void ResetPile()
    {
        if (cardsOnPile.Count == 0)
        {
            cardsOnPile.AddRange(cardsReturned);
            cardsReturned.Clear();

            ShufflePile();
        }
    }

    private void OnMouseUp()
    {
        if (cardsOnPile.Count == 0)
        {
            ResetPile();
        }

        Board board = FindObjectOfType<Board>();
        if (board != null)
        {
            Card card = cardsOnPile[0];
            cardsOnPile.Remove(card);

            if (board.OnCardRequest(card, true))
            {
                cardsInHands.Add(card);
            }
            else
            {
                cardsOnPile.Insert(0, card);
            }
        }
    }

    public void ReturnCard(Card card)
    {
        card.Disabled();
        cardsReturned.Add(card);
        cardsInHands.Remove(card);
    }

    public void RequestCard()
    {
        if (cardsOnPile.Count == 0)
        {
            ResetPile();
        }

        Board board = FindObjectOfType<Board>();
        if (board != null)
        {
            Card card = cardsOnPile[0];
            cardsOnPile.Remove(card);

            if (board.OnCardRequest(card, false))
            {
                cardsInHands.Add(card);
            }
            else
            {
                cardsOnPile.Insert(0, card);
            }
        }
    }

    // https://stackoverflow.com/questions/273313/randomize-a-listt
    public void ShufflePile()
    {
        for (int i = 0; i < 2; ++i)
        {
            int n = cardsOnPile.Count;
            while (n > 1)
            {
                n--;
                int k = ThreadSafeRandom.ThisThreadsRandom.Next(n + 1);
                Card value = cardsOnPile[k];
                cardsOnPile[k] = cardsOnPile[n];
                cardsOnPile[n] = value;
            }
        }
    }

    // 
    public static class ThreadSafeRandom
    {
        [System.ThreadStatic]
        private static System.Random Local;

        public static System.Random ThisThreadsRandom
        {
            get { return Local ?? (Local = new System.Random(unchecked(System.Environment.TickCount * 31 + Thread.CurrentThread.ManagedThreadId))); }
        }
    }
}
