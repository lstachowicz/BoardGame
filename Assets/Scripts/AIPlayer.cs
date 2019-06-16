using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

public class AIPlayer : MonoBehaviour
{
    public Board board;

    public Board.Player player;
    public CardPile cardPile;
    public GameObject cardHolder;

    private void Awake()
    {
    }

    private void Start()
    {
        board = FindObjectOfType<Board>();
        cardPile = FindObjectOfType<CardPile>();
    }

    private void Update()
    {
        if (board.currentPlayer == player)
        {
            switch(board.gameState)
            {
                case Board.GameState.TakeCardFromPile:
                {
                    cardPile = FindObjectOfType<CardPile>();
                    cardPile.RequestCard();
                        player.cards[player.cards.Count - 1].transform.position = new Vector3(cardHolder.transform.position.x,
                            cardHolder.transform.position.y, player.cards[player.cards.Count - 1].transform.position.z);
                    break;
                }
                case Board.GameState.PickCardAndPawnToPlay:
                    {
                        Pawn selectedPawn = null;
                        Card selectedCard = null;

                        // First find if we can attactk human player. If so, attack him.
                        foreach (Card card in player.cards)
                        {
                            if (card.pawnSelector == Card.PawnSelector.InGamePawn
                                && card.sideSelector == Card.SideSelector.Opponenet
                                && card.direction == Card.Direction.None)
                            {
                                Board.Player opponent = (board.mintPlayer == player ? board.violetPlayer : board.mintPlayer);
                                foreach (Pawn pawn in opponent.pawns)
                                {
                                    if (pawn.InGame())
                                    {
                                        selectedPawn = pawn;
                                        break;
                                    }
                                }
                                selectedCard = card;
                                break;
                            }
                        }

                        if (selectedPawn != null && selectedCard != null)
                        {
                            board.OnCardSelected(selectedCard, false);
                            board.OnPawnSelected(selectedPawn, false);
                            break;
                        }

                        selectedCard = null;
                        selectedPawn = null;

                        // First check if player should go forward or backword.
                        foreach (Pawn pawn in player.pawns)
                        {
                            float EPSILON = 0.1f;
                            if (!pawn.Finished() && pawn.InGame()
                                && System.Math.Abs(pawn.transform.position.x - player.entry.transform.position.x) < EPSILON
                                && System.Math.Abs(pawn.transform.position.y - player.entry.transform.position.y) < EPSILON)
                            {
                                selectedPawn = pawn;
                                break;
                            }
                        }

                        if (selectedPawn != null)
                        {
                            foreach (Card card in player.cards)
                            {
                                if (card.pawnSelector == Card.PawnSelector.InGamePawn
                                    && card.sideSelector == Card.SideSelector.Own
                                    && card.direction == Card.Direction.Back)
                                {
                                    selectedCard = card;
                                    break;
                                }
                            }
                        }

                        if (selectedPawn != null && selectedCard != null)
                        {
                            board.OnCardSelected(selectedCard, false);
                            board.OnPawnSelected(selectedPawn, false);
                            break;
                        }

                        selectedCard = null;
                        selectedPawn = null;

                        // Move player forward if possible
                        foreach (Pawn pawn in player.pawns)
                        {
                            if (!pawn.Finished() && pawn.InGame())
                            {
                                selectedPawn = pawn;
                                break;
                            }
                        }

                        foreach (Card card in player.cards)
                        {
                            if ((card.pawnSelector == Card.PawnSelector.InGamePawn
                                || card.pawnSelector == Card.PawnSelector.Both)
                                && card.sideSelector == Card.SideSelector.Own
                                && card.direction == Card.Direction.Forward)
                            {
                                selectedCard = card;
                                break;
                            }
                        }

                        if (selectedPawn != null && selectedCard != null)
                        {
                            board.OnCardSelected(selectedCard, false);
                            board.OnPawnSelected(selectedPawn, false);
                            break;
                        }

                        selectedCard = null;
                        selectedPawn = null;


                        // Just try to spawn if we can't do anything
                        foreach (Pawn pawn in player.pawns)
                        {
                            if (!pawn.Finished() && pawn.InSpawn())
                            {
                                selectedPawn = pawn;
                                break;
                            }
                        }

                        foreach (Card card in player.cards)
                        {
                            if (card.pawnSelector == Card.PawnSelector.SpawnPawn
                                || card.pawnSelector == Card.PawnSelector.Both)
                            {
                                selectedCard = card;
                                break;
                            }
                        }

                        if (selectedPawn != null && selectedCard != null)
                        {
                            board.OnCardSelected(selectedCard, false);
                            board.OnPawnSelected(selectedPawn, false);
                            break;
                        }

                        selectedCard = null;
                        selectedPawn = null;

                        // We can't do anything throw random card and random pawn
                        foreach (Pawn pawn in player.pawns)
                        {
                            if (!pawn.Finished())
                            {
                                selectedPawn = pawn;
                                break;
                            }
                        }

                        selectedCard = player.cards[0];

                        board.OnCardSelected(selectedCard, false);
                        board.OnPawnSelected(selectedPawn, false);
                        break;
                }
                default:
                {
                    break;

                }
            }
        }
    }
}