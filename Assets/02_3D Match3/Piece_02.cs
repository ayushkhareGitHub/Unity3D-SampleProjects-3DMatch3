using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Piece_02 : MonoBehaviour
{
    private Color[] colors = new Color[6]
    {
        Color.red,
        Color.blue,
        Color.green,
        Color.white,
        Color.yellow,
        Color.magenta
    };

    public int index;
    public Vector2 coordinates;
    public bool destroyed;

    // Start is called before the first frame update
    void Start()
    {
        index = Random.Range(0, colors.Length);

        this.GetComponent<Renderer>().material.SetColor("_Color", colors[index]);
    }

    public bool IsNeighbour(Piece_02 otherPiece)
    {
        return Mathf.Abs(otherPiece.coordinates.x - this.coordinates.x) + Mathf.Abs(otherPiece.coordinates.y - this.coordinates.y) == 1;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
