using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    public GameObject Pawn;

    public Pawn CreatePawn()
    {
        return Instantiate(Pawn, transform.position, Quaternion.identity).GetComponentInParent<Pawn>();
    }
}
