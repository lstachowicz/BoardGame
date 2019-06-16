using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class Pawn : MonoBehaviour
{

    private Waypoint mPosition;

    public Waypoint position {
        get
        {
            return mPosition;
        }

        set
        {
            if (value != null)
            {
                transform.position = new Vector3(value.transform.position.x
                    , value.transform.position.y, transform.position.z);
            }

            mPosition = value;
        }
    }

    public void Move(int jumps, Card.Direction direction, Board.Player.Type player)
    {
        if (position == null)
        {
            return;
        }
        
        foreach(Pawn pawns in FindObjectOfType<Board>().mintPlayer.pawns)
        {
            pawns.GetComponent<SpriteRenderer>().sortingOrder = 3;
        }
        foreach (Pawn pawns in FindObjectOfType<Board>().violetPlayer.pawns)
        {
            pawns.GetComponent<SpriteRenderer>().sortingOrder = 3;
        }

        for (int i = 0; i < jumps; ++i)
        {
            Debug.Log("Moving " + jumps + " in direction" + direction + " by player " + player);

            if (direction == Card.Direction.Forward
                && position.NextWaypoint() != null)
            {
                foreach (Waypoint next_position in position.NextWaypoint())
                {
                    if (next_position.IsPreferedBy(player))
                    {
                        position = next_position;
                        this.GetComponent<SpriteRenderer>().sortingOrder = 4;
                        break;
                    }
                }
            }
            else if (direction == Card.Direction.Back
                && position.PreviousWaypoint() != null)
            {
                position = position.PreviousWaypoint()[0];
                this.GetComponent<SpriteRenderer>().sortingOrder = 4;
            }
        }
    }

    public bool InGame()
    {
        return position != null;
    }

    public bool InSpawn()
    {
        return position == null;
    }

    public bool Finished()
    {
        return position != null && position.NextWaypoint().Count == 0;
    }

    private void Awake()
    {
        position = null;
    }

    public void Disabled()
    {
        GetComponent<Collider2D>().enabled = false;
    }

    private void OnMouseUp()
    {
        Board board = FindObjectOfType<Board>();
        if (board != null)
        {
            board.OnPawnSelected(this, true);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Pawn pawn = collision.gameObject.GetComponent<Pawn>();

        if (pawn.GetComponent<SpriteRenderer>().sortingOrder == 3)
        {
            FindObjectOfType<Board>().OnPawnRemove(pawn);
        }
    }
}
