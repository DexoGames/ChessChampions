using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "ChessPiece", menuName = "ChessPiece")]
public class ChessPiece : ScriptableObject
{
    public UnityEvent<Chessman, GameObject[,], int, bool> Moves;
}
