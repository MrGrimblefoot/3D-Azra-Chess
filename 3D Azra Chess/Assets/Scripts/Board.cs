using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

#region Enums
public enum SpecialMove
{
    None = 0,
    EnPassant,
    Castling,
    Promotion
}
#endregion

public class Board : MonoBehaviour
{
    #region Variables
    [Header("Visuals")]
    [SerializeField] private Material tileMaterial;
    [SerializeField] private float tileSize = 1;
    [SerializeField] private float yOffset = 0.2f;
    [SerializeField] private float deathSpacing = 0.3f;
    [SerializeField] private Vector3 boardCenter = Vector3.zero;
    [SerializeField] private GameObject victoryScreen;
    [SerializeField] private TextMeshProUGUI turnIndicator;
    [SerializeField] private string whiteTurnText;
    [SerializeField] private string blackTurnText;

    [Header("Prefabs & Materials")]
    [SerializeField] private GameObject[] prefabs;
    [SerializeField] private Material[] teamMaterials;

    // Logic
    private Piece[,] pieces;
    [SerializeField] private Piece currentlySelected;
    private List<Vector2Int> availableMoves = new List<Vector2Int>();
    private List<Piece> deadWhites = new List<Piece>();
    private List<Piece> deadBlacks = new List<Piece>();
    private const int TILE_COUNT_X = 8;
    private const int TILE_COUNT_Y = 8;
    private GameObject[,] tiles;
    private Camera currentCamera;
    private Vector2Int currentHover;
    private Vector2Int hitPosition;
    private Vector3 bounds;
    private bool isWhiteTurn;
    private SpecialMove specialMove;
    private List<Vector2Int[]> moveList = new List<Vector2Int[]>();
    #endregion

    #region MonoBehavior Functions
    void Awake()
    {
        isWhiteTurn = true;
        victoryScreen.SetActive(false);
        turnIndicator.gameObject.SetActive(true);

        GenerateAllTiles(tileSize, TILE_COUNT_X, TILE_COUNT_Y);
        SpawnAllPieces();
        PositionAllPieces();
    }

    private void Update()
    {
        if (!currentCamera)
        {
            currentCamera = Camera.main;
            return;
        }

        if (isWhiteTurn) { turnIndicator.text = whiteTurnText; }
        else { turnIndicator.text = blackTurnText; }

        RaycastHit info;
        Ray ray = currentCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out info, 100, LayerMask.GetMask("Tile", "Hover", "Highlight")))
        {
            // Get the indexes of the tile i've hit
            hitPosition = LookupTileIndex(info.collider.gameObject);

            // If we're hovering a tile after not hovering any tiles
            if (currentHover == -Vector2Int.one)
            {
                currentHover = hitPosition;
                tiles[hitPosition.x, hitPosition.y].layer = 7;
            }

            // If we were already hovering a tile, change the previous one
            if (currentHover != hitPosition)
            {
                tiles[currentHover.x, currentHover.y].layer = (ContainsValidMoves(ref availableMoves, currentHover)) ? 8 : 6; ;
                currentHover = hitPosition;
                tiles[hitPosition.x, hitPosition.y].layer = 7;
            }

            // If we click the mouse
            if (Input.GetMouseButtonDown(0))
            {
                if (currentlySelected != null)
                {
                    Vector2Int previousPosition = new Vector2Int(currentlySelected.currentX, currentlySelected.currentY);

                    bool validMove = MoveTo(currentlySelected, hitPosition.x, hitPosition.y);
                    if (!validMove) { currentlySelected.SetPosition(GetTileCenter(previousPosition.x, previousPosition.y)); }

                    currentlySelected = null;
                    RemoveHighlightTiles();
                    return;
                }

                if (currentlySelected == null && pieces[hitPosition.x, hitPosition.y] != null)
                {
                    // Is it our turn?
                    if ((pieces[hitPosition.x, hitPosition.y].team == 0 && isWhiteTurn) || (pieces[hitPosition.x, hitPosition.y].team == 1 && !isWhiteTurn))
                    {
                        currentlySelected = pieces[hitPosition.x, hitPosition.y];
                        //Get a list of avaliable moves, and highlight tiles as well
                        availableMoves = currentlySelected.GetAvailableMoves(ref pieces, TILE_COUNT_X, TILE_COUNT_Y);
                        //Get a list of special moves as well
                        specialMove = currentlySelected.GetSpecialMoves(ref pieces, ref moveList, ref availableMoves);

                        HighlightTiles();
                    }
                }
            }
        }
        else
        {
            if (currentHover != -Vector2Int.one)
            {
                tiles[currentHover.x, currentHover.y].layer = (ContainsValidMoves(ref availableMoves, currentHover)) ? 8 : 6;
                currentHover = -Vector2Int.one;
            }

            if(currentlySelected && Input.GetMouseButtonDown(0))
            {
                currentlySelected.SetPosition(GetTileCenter(currentlySelected.currentX, currentlySelected.currentY));
                currentlySelected = null;
                RemoveHighlightTiles();
            }
        }
    }

    #endregion

    #region Generate Board
    private void GenerateAllTiles(float tileSize, int tileCountX, int tileCountY)
    {
        yOffset += transform.position.y;
        bounds = new Vector3((tileCountX / 2) * tileSize, 0, (tileCountX / 2) * tileSize) + boardCenter;

        tiles = new GameObject[tileCountX, tileCountY];
        for (int x = 0; x < tileCountX; x++)
            for (int y = 0; y < tileCountY; y++)
                tiles[x, y] = GenerateSingleTile(tileSize, x, y);
    }
    private GameObject GenerateSingleTile(float tileSize, int x, int y)
    {
        GameObject tileObject = new GameObject($"X:{x}, Y:{y}");
        tileObject.transform.parent = transform;

        Mesh mesh = new Mesh();
        tileObject.AddComponent<MeshFilter>().mesh = mesh;
        tileObject.AddComponent<MeshRenderer>().material = tileMaterial;

        Vector3[] vertices = new Vector3[4];
        vertices[0] = new Vector3(x * tileSize, yOffset, y * tileSize) - bounds;
        vertices[1] = new Vector3(x * tileSize, yOffset, (y + 1) * tileSize) - bounds;
        vertices[2] = new Vector3((x + 1) * tileSize, yOffset, y * tileSize) - bounds;
        vertices[3] = new Vector3((x + 1) * tileSize, yOffset, (y + 1) * tileSize) - bounds;

        int[] tris = new int[] {0, 1, 2, 1, 3, 2 };

        mesh.vertices = vertices;
        mesh.triangles = tris;

        tileObject.layer = LayerMask.NameToLayer("Tile");
        mesh.RecalculateNormals();

        tileObject.AddComponent<BoxCollider>();

        return tileObject;
    }
    #endregion

    #region Piece Spawning
    private void SpawnAllPieces()
    {
        pieces = new Piece[TILE_COUNT_X, TILE_COUNT_Y];

        int whiteTeam = 0; int blackTeam = 1;

        //White Team
        pieces[0, 0] = SpawnSinglePiece(PieceType.Rook, whiteTeam);
        pieces[1, 0] = SpawnSinglePiece(PieceType.Knight, whiteTeam);
        pieces[2, 0] = SpawnSinglePiece(PieceType.Bishop, whiteTeam);
        pieces[3, 0] = SpawnSinglePiece(PieceType.Queen, whiteTeam);
        pieces[4, 0] = SpawnSinglePiece(PieceType.King, whiteTeam);
        pieces[5, 0] = SpawnSinglePiece(PieceType.Bishop, whiteTeam);
        pieces[6, 0] = SpawnSinglePiece(PieceType.Knight, whiteTeam);
        pieces[7, 0] = SpawnSinglePiece(PieceType.Rook, whiteTeam);
        for (int i = 0; i < TILE_COUNT_X; i++)
            pieces[i, 1] = SpawnSinglePiece(PieceType.Pawn, whiteTeam);

        //Black Team
        pieces[0, 7] = SpawnSinglePiece(PieceType.Rook, blackTeam);
        pieces[1, 7] = SpawnSinglePiece(PieceType.Knight, blackTeam);
        pieces[2, 7] = SpawnSinglePiece(PieceType.Bishop, blackTeam);
        pieces[3, 7] = SpawnSinglePiece(PieceType.Queen, blackTeam);
        pieces[4, 7] = SpawnSinglePiece(PieceType.King, blackTeam);
        pieces[5, 7] = SpawnSinglePiece(PieceType.Bishop, blackTeam);
        pieces[6, 7] = SpawnSinglePiece(PieceType.Knight, blackTeam);
        pieces[7, 7] = SpawnSinglePiece(PieceType.Rook, blackTeam);
        for (int i = 0; i < TILE_COUNT_X; i++)
            pieces[i, 6] = SpawnSinglePiece(PieceType.Pawn, blackTeam);
    }

    private Piece SpawnSinglePiece(PieceType type, int team)
    {
        Piece p = Instantiate(prefabs[(int)type - 1], transform).GetComponent<Piece>();

        p.type = type;
        p.team = team;
        p.GetComponent<MeshRenderer>().material = teamMaterials[team];
        //p.GetComponent<MeshRenderer>().material = teamMaterials[((team == 0) ? 0 : 6) + ((int)type - 1)]; If there are unique materials for each piece.

        return p;
    }
    #endregion

    #region Position Pieces
    private void PositionAllPieces()
    {
        for (int x = 0; x < TILE_COUNT_X; x++)
            for (int y = 0; y < TILE_COUNT_Y; y++)
                if (pieces[x, y] != null)
                    PositionSinglePiece(x, y, true);
    }

    private void PositionSinglePiece(int x, int y, bool force = false)
    {
        pieces[x, y].currentX = x;
        pieces[x, y].currentY = y;
        pieces[x, y].SetPosition(GetTileCenter(x, y), force);
    }

    private Vector3 GetTileCenter(int x, int y)
    {
        return new Vector3(x * tileSize, yOffset, y * tileSize) - bounds + new Vector3(tileSize / 2, 0, tileSize / 2);
    }
    #endregion

    #region Highlight Tiles
    private void HighlightTiles()
    {
        for (int i = 0; i < availableMoves.Count; i++)
        {
            tiles[availableMoves[i].x, availableMoves[i].y].layer = LayerMask.NameToLayer("Highlight");
        }
    }

    private void RemoveHighlightTiles()
    {
        for (int i = 0; i < availableMoves.Count; i++)
        {
            tiles[availableMoves[i].x, availableMoves[i].y].layer = LayerMask.NameToLayer("Tile");
        }

        availableMoves.Clear();
    }
    #endregion

    #region CheckMate
    private void CheckMate(int team)
    {
        DisplayVictoryUI(team);
    }

    private void DisplayVictoryUI(int winningTeam)
    {
        victoryScreen.SetActive(true);
        turnIndicator.gameObject.SetActive(false);
        victoryScreen.transform.GetChild(winningTeam).gameObject.SetActive(true);
    }

    public void OnRestartButton()
    {
        //Take care of UI
        victoryScreen.transform.GetChild(0).gameObject.SetActive(false);
        victoryScreen.transform.GetChild(1).gameObject.SetActive(false);
        victoryScreen.SetActive(false);
        turnIndicator.gameObject.SetActive(true);

        //Field Reset
        currentlySelected = null;
        availableMoves.Clear();
        moveList.Clear();

        //Clean up
        for (int x = 0; x < TILE_COUNT_X; x++)
        {
            for (int y = 0; y < TILE_COUNT_Y; y++)
            {
                if(pieces[x, y] != null) { Destroy(pieces[x, y].gameObject); }
                pieces[x, y] = null;
            }
        }

        for (int i = 0; i < deadWhites.Count; i++) { Destroy(deadWhites[i].gameObject); }
        for (int i = 0; i < deadBlacks.Count; i++) { Destroy(deadBlacks[i].gameObject); }

        deadWhites.Clear();
        deadBlacks.Clear();

        SpawnAllPieces();
        PositionAllPieces();
        isWhiteTurn = true;
    }

    public void OnExitButton()
    {
        print("Closing application!");
        Application.Quit();
    }
    #endregion

    #region Special Moves
    private void ProcessSpecialMove()
    {
        if(specialMove == SpecialMove.EnPassant)
        {
            var newMove = moveList[moveList.Count - 1];
            Piece myPawn = pieces[newMove[1].x, newMove[1].y];
            var targetPawnPosition = moveList[moveList.Count - 2];
            Piece enemyPawn = pieces[targetPawnPosition[1].x, targetPawnPosition[1].y];

            if (myPawn.currentX == enemyPawn.currentX)
            {
                if(myPawn.currentY == enemyPawn.currentY - 1 || myPawn.currentY == enemyPawn.currentY + 1)
                {
                    if(enemyPawn.team == 0)
                    {
                        deadWhites.Add(enemyPawn);
                        enemyPawn.SetScale(Vector3.one * 0.1f);
                        enemyPawn.SetPosition(new Vector3(8 * tileSize, yOffset, -1 * tileSize)
                            - bounds
                            + new Vector3(tileSize / 2, 0, tileSize / 2)
                            + (Vector3.forward * deathSpacing) * deadWhites.Count);
                    }
                    else
                    {
                        deadBlacks.Add(enemyPawn);
                        enemyPawn.SetScale(Vector3.one * 0.1f);
                        enemyPawn.SetPosition(new Vector3(-1 * tileSize, yOffset, 8 * tileSize)
                            - bounds
                            + new Vector3(tileSize / 2, 0, tileSize / 2)
                            + (Vector3.back * deathSpacing) * deadBlacks.Count);
                    }

                    pieces[enemyPawn.currentX, enemyPawn.currentY] = null;
                }
            }
        }
    }
    #endregion

    #region Operations
    private bool ContainsValidMoves(ref List<Vector2Int> moves, Vector2Int pos)
    {
        for (int i = 0; i < moves.Count; i++)
        {
            if(moves[i].x == pos.x && moves[i].y == pos.y) { return true; }
        }

        return false;
    }

    private bool MoveTo(Piece cp, int x, int y)
    {
        if(!ContainsValidMoves(ref availableMoves, new Vector2Int(x, y))) { return false; }

        Vector2Int previousPosition = new Vector2Int(cp.currentX, cp.currentY);

        // Is there another piece on the target position
        if(pieces[x, y] != null)
        {
            Piece ocp = pieces[x, y];

            if(cp.team == ocp.team) { return false; }

            // if it's the enemy team
            // change things here so an attack & death animation happens and stuff
            if(ocp.team == 0)
            {
                if(ocp.type == PieceType.King) { CheckMate(1); }

                deadWhites.Add(ocp);
                ocp.SetScale(Vector3.one * 0.1f);
                ocp.SetPosition(new Vector3(8 * tileSize, yOffset, -1 * tileSize)
                    - bounds
                    + new Vector3(tileSize / 2, 0, tileSize / 2)
                    + (Vector3.forward * deathSpacing) * deadWhites.Count);
            }
            else
            {
                if (ocp.type == PieceType.King) { CheckMate(0); }

                deadBlacks.Add(ocp);
                ocp.SetScale(Vector3.one * 0.1f);
                ocp.SetPosition(new Vector3(-1 * tileSize, yOffset, 8 * tileSize)
                    - bounds
                    + new Vector3(tileSize / 2, 0, tileSize / 2)
                    + (Vector3.back * deathSpacing) * deadBlacks.Count);
            }

        }

        pieces[x, y] = cp;
        pieces[previousPosition.x, previousPosition.y] = null;

        PositionSinglePiece(x, y);

        isWhiteTurn = !isWhiteTurn;
        moveList.Add(new Vector2Int[] { previousPosition, new Vector2Int(x, y)});

        ProcessSpecialMove();

        return true;
    }

    private Vector2Int LookupTileIndex(GameObject hitInfo)
    {
        for (int x = 0; x < TILE_COUNT_X; x++)
            for (int y = 0; y < TILE_COUNT_Y; y++)
                if(tiles[x,y] == hitInfo) { return new Vector2Int(x,y);}
        
        return -Vector2Int.one; //Invalid
    }
    #endregion
}
