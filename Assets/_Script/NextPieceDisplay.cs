using System.Collections.Generic;
using UnityEngine;

public class NextPieceDisplay : MonoBehaviour
{
    const int C_NEXT_PIECE_COUNT = 6;

    //list of piecevisuals
    public List<PieceDisplay> _pieceVisuals = new List<PieceDisplay>();
    static NextPieceDisplay _this = null;

    static public NextPieceDisplay Get()
    {
        return _this;
    }

    private void Awake()
    {
        _this = this;
    }

    public static string GetName()
    {
        return Get().name;
    }
    // Start is called before the first frame update
    void Start()
    {
    }

    private void Reset()
    {
        //kill each piece gameobject on our list
        foreach (var p in _pieceVisuals)
        {
            Destroy(p.gameObject);
        }

        //also reset the list
        _pieceVisuals.Clear();
    }

    public void UpdatePieces()
    {
        Reset();

        if (C_NEXT_PIECE_COUNT > GameLogic.Get().GetPieceBag().C_MINIMUM_BAG_SIZE)
        {
            //assert an error
            Debug.LogError("It's possible there isn't enough pieces in the bag to display!");
            return;
        }


        int gemsPerRow = Config.Get().GetPieceCountGivenAtOnce();

        for (int i=0; i < C_NEXT_PIECE_COUNT/gemsPerRow; i++)
        {

            for (int j = 0; j < gemsPerRow; j++)
            {
                //create a new piece visual
                var pieceVisual = Instantiate(TableDisplay.Get().GetPiecePrefab(), transform);
                pieceVisual.transform.localPosition = new Vector3(-7 +(j*1), (4.5f) + (-i * 1.5f), 0);
                pieceVisual.transform.parent = TableDisplay.Get().GetPiecesFolder().transform;
                //get the script to the pieceVisual
                var pieceDisplay = pieceVisual.GetComponent<PieceDisplay>();
                pieceDisplay.SetAssociatedPiece(GameLogic.Get().GetPieceBag().PeekAtUpcomingPiece((i* gemsPerRow )+ j));

                //add to our internal list
                _pieceVisuals.Add(pieceDisplay);
            }
        }
    }
    // Update is called once per frame

    void Update()
    {
        
    }
}
