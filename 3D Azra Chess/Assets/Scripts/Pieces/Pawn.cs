using System.Collections.Generic;
using UnityEngine;

public class Pawn : Piece
{
    public override List<Vector2Int> GetAvailableMoves(ref Piece[,] board, int tileCountX, int tileCountY)
    {
        List<Vector2Int> r = new List<Vector2Int>();

        #region Get Direction
        //             If white: up. else: down.
        int direction = (team == 0) ? 1 : -1;
        #endregion

        #region Moving Normally
        if (board[currentX, currentY + direction] == null)
        {
            r.Add(new Vector2Int(currentX, currentY + direction));
        }
        #endregion

        #region Starting Position
        if (board[currentX, currentY + direction] == null)
        {
            //White team
            if(team == 0 && currentY == 1 && board[currentX, currentY + (direction * 2)] == null)
            {
                r.Add(new Vector2Int(currentX, currentY + (direction * 2)));
            }
            if(team == 1 && currentY == 6 && board[currentX, currentY + (direction * 2)] == null)
            {
                r.Add(new Vector2Int(currentX, currentY + (direction * 2)));
            }
        }
        #endregion

        #region Attacking
        //If attacking on the left
        if (currentX != 0)
        {
            if (board[currentX - 1, currentY + direction] != null && board[currentX - 1, currentY + direction].team != team)
            {
                r.Add(new Vector2Int(currentX - 1, currentY + direction));
            }
        }
        //If attacking on the right
        if (currentX != tileCountX - 1)
        {
            if (board[currentX + 1, currentY + direction] != null && board[currentX + 1, currentY + direction].team != team)
            {
                r.Add(new Vector2Int(currentX + 1, currentY + direction));
            }
        }
        #endregion

        return r;
    }

    public override SpecialMove GetSpecialMoves(ref Piece[,] board, ref List<Vector2Int[]> moveList, ref List<Vector2Int> availableMoves)
    {
        int direction = (team == 0) ? 1 : -1;

        if((team == 0 && currentY == 6) || (team == 1 && currentY == 1)) { return SpecialMove.Promotion; }

        #region En Passant
        if (moveList.Count > 0)
        {
            Vector2Int[] lastMove = moveList[moveList.Count - 1];
            if(board[lastMove[1].x, lastMove[1].y].type == PieceType.Pawn) //If last piece was a pawn
            {
                //If the last move was +2 on the y axis in either direction (Mathf.Abs make it so we don't need to worry about neg or pos):
                if (Mathf.Abs(lastMove[0].y - lastMove[1].y) == 2)
                {
                    if(board[lastMove[1].x, lastMove[1].y].team != team) //If the move was on the other team (Kind of redundant because of turn mechanic, but I'm keeping it.)
                    {
                        if(lastMove[1].y == currentY)// If both pawns are on the same Y
                        {
                            if(lastMove[1].x == currentX - 1) //If they landed to my left
                            {
                                availableMoves.Add(new Vector2Int(currentX - 1, currentY + direction));
                                return SpecialMove.EnPassant;
                            }

                            if (lastMove[1].x == currentX + 1) //If they landed to my right
                            {
                                availableMoves.Add(new Vector2Int(currentX + 1, currentY + direction));
                                return SpecialMove.EnPassant;
                            }
                        }
                    }
                }
            }
        }
        #endregion

        return SpecialMove.None;
    }
}