using System.Collections.Generic;
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
    public float smoothingMoveSpeed = 10;
    public float smoothingScaleSpeed = 10;
    public PieceType type;

    private Vector3 desiredPosition;
    private Vector3 desiredScale = new Vector3(0.16f, 0.16f, 0.16f);

    private void Start()
    {
        if(team == 0) { transform.rotation = Quaternion.Euler(-90, -90, 0); }
        else{ transform.rotation = Quaternion.Euler(-90, 90, 0); }
    }

    private void Update()
    {
        transform.position = Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime * smoothingMoveSpeed);
        transform.localScale = Vector3.Lerp(transform.localScale, desiredScale, Time.deltaTime * smoothingScaleSpeed);
    }

    public virtual List<Vector2Int> GetAvailableMoves(ref Piece[,] board, int tileCountX, int tileCountY)
    {
        List<Vector2Int> r = new List<Vector2Int>();

        //Test moves:
        r.Add(new Vector2Int(3,3));
        r.Add(new Vector2Int(3,4));
        r.Add(new Vector2Int(4,3));
        r.Add(new Vector2Int(4,4));

        return r;
    }

    public virtual void SetPosition(Vector3 position, bool force = false)
    {
        desiredPosition = position;
        if (force) { transform.position = desiredPosition; }
    }
    public virtual void SetScale(Vector3 scale, bool force = false)
    {
        desiredScale = scale;
        if (force) { transform.localScale = desiredScale; }
    }
}
