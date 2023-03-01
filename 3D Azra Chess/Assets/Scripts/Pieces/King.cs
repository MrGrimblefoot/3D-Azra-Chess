using System.Collections.Generic;
using UnityEngine;

public class King : Piece
{
    public override List<Vector2Int> GetAvailableMoves(ref Piece[,] board, int tileCountX, int tileCountY)
    {
        List<Vector2Int> r = new List<Vector2Int>();

        #region Up
        if(currentY + 1 < tileCountY)
        {
            if(board[currentX, currentY + 1] == null || board[currentX, currentY + 1].team != team)
            {
                r.Add(new Vector2Int(currentX, currentY + 1));
            }
        }
        #endregion

        #region Down
        if (currentY - 1 >= 0)
        {
            if (board[currentX, currentY - 1] == null || board[currentX, currentY - 1].team != team)
            {
                r.Add(new Vector2Int(currentX, currentY - 1));
            }
        }
        #endregion

        #region Right
        if (currentX + 1 < tileCountX)
        {
            //Right
            if (board[currentX + 1, currentY] == null) { r.Add(new Vector2Int(currentX + 1, currentY)); }
            else if(board[currentX + 1, currentY].team != team) { r.Add(new Vector2Int(currentX + 1, currentY)); }

            //Top Right
            if (currentY + 1 < tileCountY)
            {
                if (board[currentX + 1, currentY + 1] == null) { r.Add(new Vector2Int(currentX + 1, currentY + 1)); }
                else if (board[currentX + 1, currentY + 1].team != team) { r.Add(new Vector2Int(currentX + 1, currentY + 1)); }
            }

            //Bottom Right
            if (currentY - 1 >= 0)
            {
                if (board[currentX + 1, currentY - 1] == null) { r.Add(new Vector2Int(currentX + 1, currentY - 1)); }
                else if (board[currentX + 1, currentY - 1].team != team) { r.Add(new Vector2Int(currentX + 1, currentY - 1)); }
            }
        }
        #endregion

        #region Left
        if (currentX - 1 >= 0)
        {
            //Left
            if (board[currentX - 1, currentY] == null) { r.Add(new Vector2Int(currentX - 1, currentY)); }
            else if (board[currentX - 1, currentY].team != team) { r.Add(new Vector2Int(currentX - 1, currentY)); }

            //Top Left
            if (currentY + 1 < tileCountY)
            {
                if (board[currentX - 1, currentY + 1] == null) { r.Add(new Vector2Int(currentX - 1, currentY + 1)); }
                else if (board[currentX - 1, currentY + 1].team != team) { r.Add(new Vector2Int(currentX - 1, currentY + 1)); }
            }

            //Bottom Left
            if (currentY - 1 >= 0)
            {
                if (board[currentX - 1, currentY - 1] == null) { r.Add(new Vector2Int(currentX - 1, currentY - 1)); }
                else if (board[currentX - 1, currentY - 1].team != team) { r.Add(new Vector2Int(currentX - 1, currentY - 1)); }
            }

        }

        #endregion

        return r;
    }
}