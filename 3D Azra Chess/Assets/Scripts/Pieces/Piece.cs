using UnityEngine;

public enum PieceType
{
    None = 0,
    Pawn = 1,
    Rook = 2,
    Knight = 3,
    Bishop = 4,
    Queen = 5,
    King = 6
}

public class Piece : MonoBehaviour
{
    [Tooltip("0 = White, 1 = Black")]
    public int team;
    public int currentX;
    public int currentY;
    public PieceType type;

    private Vector3 desiredPosition;
    private Vector3 desiredScale;
}
