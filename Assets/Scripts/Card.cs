using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Card : MonoBehaviour
{
    public enum Direction
    {
        Forward,
        Back,
        None
    }

    public enum SideSelector
    {
        Own,
        Opponenet
    }

    public enum PawnSelector
    {
        InGamePawn,
        SpawnPawn,
        Both
    }

    public SideSelector sideSelector = SideSelector.Own;
    public PawnSelector pawnSelector = PawnSelector.InGamePawn;
    public Direction direction = Direction.Forward;

    public int jumps = 0;

    public bool IsInGame()
    {
        return GetComponent<Collider2D>().enabled;
    }

    public void Enabled()
    {
        GetComponent<Collider2D>().enabled = true;
        GetComponent<SpriteRenderer>().enabled = true;
    }

    public void Disabled()
    {
        GetComponent<Collider2D>().enabled = false;
        GetComponent<SpriteRenderer>().enabled = false;
    }

    private void Start()
    {
        Disabled();
        MoveToBottom();
    }

    public void MoveToTop()
    {
        GetComponent<SpriteRenderer>().sortingOrder = 2;
    }

    public void MoveToBottom()
    {
        GetComponent<SpriteRenderer>().sortingOrder = FindObjectOfType<CardPile>().GetComponent<SpriteRenderer>().sortingOrder - 1;
    }

    private void OnMouseUp()
    {
        Debug.Log("Picked card " + this);

        Board board = FindObjectOfType<Board>();
        if (board != null)
        {
            board.OnCardSelected(this, true);
        }
    }
}
