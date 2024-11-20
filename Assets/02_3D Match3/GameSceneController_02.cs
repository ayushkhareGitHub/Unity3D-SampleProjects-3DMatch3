using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSceneController_02 : MonoBehaviour
{
    public int boardWidth = 6;
    public int boardHeight = 5;
    public float pieceSpacing = 1.3f;

    public Camera gameCamera;
    public Transform levelContainer;

    public GameObject piecePrefab;

    private int score;
    private float gameTimer;
    private bool gameOver;

    private Piece_02[,] board;
    private Piece_02 selectedPiece;

    // Start is called before the first frame update
    void Start()
    {
        BuildBoard();
    }

    private void BuildBoard()
    {
        board = new Piece_02[boardWidth, boardHeight];

        for(int y = 0; y < boardHeight; y++)
        {
            for (int x = 0; x < boardWidth; x++)
            {
                GameObject pieceObject = Instantiate(piecePrefab);
                pieceObject.transform.SetParent(levelContainer);
                pieceObject.transform.localPosition = new Vector3(
                    (-boardWidth*pieceSpacing)/2f+(pieceSpacing/2f)+x*pieceSpacing,
                    (-boardWidth*pieceSpacing)/2f+(pieceSpacing / 2f)+y*pieceSpacing,
                    0
                );

                Piece_02 piece = pieceObject.GetComponent<Piece_02>();
                piece.coordinates = new Vector2(x, y);

                board[x, y] = piece;
            }
        }
    }

    void Update()
    {
        ProcessInput();
    }

    private void ProcessInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Ray ray = gameCamera.ScreenPointToRay(Input.mousePosition);

            if(Physics.Raycast(ray, out hit, 100))
            {
                Debug.Log(hit.collider.name);

                if(hit.collider.gameObject.GetComponent<Piece_02>() != null)
                {
                    Piece_02 hitPiece = hit.collider.gameObject.GetComponent<Piece_02>();

                    if (selectedPiece == null)
                    {
                        selectedPiece = hitPiece;
                        iTween.ScaleTo(selectedPiece.gameObject, iTween.Hash(
                            "scale",Vector3.one*1.22f,
                            "isLocal",true,
                            "time",0.3f
                        ));
                    }
                    else
                    {
                        if (hitPiece == selectedPiece || hitPiece.IsNeighbour(selectedPiece) == false)
                        {
                            iTween.ScaleTo(selectedPiece.gameObject, iTween.Hash(
                                "scale", Vector3.one,
                                "isLocal", true,
                                "time", 0.3f
                            ));
                        }
                        else if (hitPiece.IsNeighbour(selectedPiece))
                        {
                            AttemptMatch(selectedPiece, hitPiece);
                        }
                        selectedPiece = null;
                    }
                }
            }
        }
    }

    private void AttemptMatch(Piece_02 piece_01, Piece_02 piece_02)
    {
        StartCoroutine(AttemptMatchRoutine(piece_01, piece_02));
    }

    private IEnumerator AttemptMatchRoutine(Piece_02 piece1,Piece_02 piece2)
    {
        iTween.Stop(piece1.gameObject);
        iTween.Stop(piece2.gameObject);

        piece1.transform.localScale = Vector3.one;
        piece2.transform.localScale = Vector3.one;

        Vector2 coordinates1 = piece1.coordinates;
        Vector2 coordinates2 = piece2.coordinates;

        Vector3 position1 = piece1.transform.position;
        Vector3 position2 = piece2.transform.position;

        iTween.MoveTo(piece1.gameObject, iTween.Hash(
            "position", position2,
            "isLocal", true,
            "time", 0.5f
        ));

        iTween.MoveTo(piece2.gameObject, iTween.Hash(
            "position", position1,
            "isLocal", true,
            "time", 0.5f
        ));

        piece1.coordinates = coordinates2;
        piece2.coordinates = coordinates1;

        board[(int)piece1.coordinates.x, (int)piece1.coordinates.y] = piece1;
        board[(int)piece2.coordinates.x, (int)piece2.coordinates.y] = piece2;

        yield return new WaitForSeconds(0.5f);

        List<Piece_02> matchingPieces = CheckMatch(piece1);
        if(matchingPieces.Count == 0)
        {
            matchingPieces = CheckMatch(piece2);
        }

        if(matchingPieces.Count < 3)
        {
            iTween.MoveTo(piece1.gameObject, iTween.Hash(
                "position", position1,
                "isLocal", true,
                "time", 0.5f
            ));

            iTween.MoveTo(piece2.gameObject, iTween.Hash(
                "position", position2,
                "isLocal", true,
                "time", 0.5f
            ));

            piece1.coordinates = coordinates1;
            piece2.coordinates = coordinates2;

            board[(int)piece1.coordinates.x, (int)piece1.coordinates.y] = piece1;
            board[(int)piece2.coordinates.x, (int)piece2.coordinates.y] = piece2;

            yield return new WaitForSeconds(1.0f);

            CheckGameOver();
        }
        else
        {
            foreach(Piece_02 piece in matchingPieces)
            {
                piece.destroyed = true;

                score += 100;
                iTween.ScaleTo(piece.gameObject, iTween.Hash(
                    "scale", Vector3.zero,
                    "isLocal", true,
                    "time", 0.25f
                ));
            }

            yield return new WaitForSeconds(0.25f);

            DropPieces();
            AddPieces();

            yield return new WaitForSeconds(1.0f);

            CheckGameOver();
        }
    }

    private List<Piece_02> CheckMatch(Piece_02 piece)
    {
        List<Piece_02> matchingNeighbours = new List<Piece_02>();

        int x = 0;
        int y = (int)piece.coordinates.y;
        bool reachedPiece = false;

        while (x < boardWidth)
        {
            if (!board[x, y].destroyed & board[x, y].index == piece.index)
            {
                matchingNeighbours.Add(board[x, y]);
                if (board[x, y] == piece)
                {
                    reachedPiece = true;
                }
            }
            else
            {
                if (!reachedPiece)
                {
                    matchingNeighbours.Clear();
                }
                else if (matchingNeighbours.Count >= 3)
                {
                    return matchingNeighbours;
                }
                else
                {
                    matchingNeighbours.Clear();
                }
            }

            x++;
        }

        if (matchingNeighbours.Count >= 3)
        {
            return matchingNeighbours;
        }

        x = (int)piece.coordinates.x;
        y = 0;

        reachedPiece = false;
        matchingNeighbours.Clear();

        while (y < boardHeight)
        {
            if (!board[x, y].destroyed & board[x, y].index == piece.index)
            {
                matchingNeighbours.Add(board[x, y]);
                if (board[x, y] == piece)
                {
                    reachedPiece = true;
                }
            }
            else
            {
                if (!reachedPiece)
                {
                    matchingNeighbours.Clear();
                }
                else if (matchingNeighbours.Count >= 3)
                {
                    return matchingNeighbours;
                }
                else
                {
                    matchingNeighbours.Clear();
                }
            }

            y++;
        }

        return matchingNeighbours;
    }

    private void DropPieces()
    {
        for (int y = 0; y < boardHeight; y++)
        {
            for (int x = 0; x < boardWidth; x++)
            {
                if (board[x, y].destroyed)
                {
                    bool dropped = false;
                    for(int j = y + 1; j < boardHeight && !dropped; j++)
                    {
                        Vector2 coordinates1 = board[x, y].coordinates;
                        Vector2 coordinates2 = board[x, y].coordinates;

                        board[x, y].coordinates = coordinates2;
                        board[x, y].coordinates = coordinates1;

                        iTween.MoveTo(board[x, y].gameObject, iTween.Hash(
                            "position", board[x, y].transform.position,
                            "isLocal", true,
                            "time", 0.25f
                        ));

                        board[x, y].transform.localPosition = board[x, j].transform.localPosition;

                        Piece_02 fallingPiece = board[x, j];
                        board[x, j] = board[x, y];
                        board[x, y] = fallingPiece;

                        dropped = true;
                    }
                }
            }
        }
    }

    private void AddPieces()
    {
        int firstY = -1;

        for (int y = 0; y < boardHeight; y++)
        {
            for (int x = 0; x < boardWidth; x++)
            {
                if (board[x, y].destroyed)
                {
                    if (firstY == -1)
                    {
                        firstY = y;
                    }

                    Piece_02 oldPiece = board[x, y];

                    GameObject pieceObject = Instantiate(piecePrefab);
                    pieceObject.transform.SetParent(levelContainer);
                    pieceObject.transform.localPosition = new Vector3(
                        oldPiece.transform.position.x,
                        6.0f,
                        0
                    );

                    iTween.MoveTo(pieceObject, iTween.Hash(
                        "position", oldPiece.transform.localPosition,
                        "isLocal", true,
                        "time", 0.25f,
                        "delay", 0.150f * (y-firstY)
                    ));

                    Piece_02 piece = pieceObject.GetComponent<Piece_02>();
                    piece.coordinates = oldPiece.coordinates;

                    board[x, y] = piece;

                    GameObject.Destroy(oldPiece.gameObject);
                }
            }
        }
    }

    private void CheckGameOver()
    {

    }
}
