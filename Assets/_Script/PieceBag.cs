using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class PieceBag : System.Object
{
    // Start is called before the first frame update
    
    List<Piece> _pieces = new List<Piece>();

    int C_MAX_PIECES_IN_BAG = 10;
    public int C_MINIMUM_BAG_SIZE = 6;


    public void AddToBag()
    {
        List<Piece> tempBag = new List<Piece>();

        for (int i = 0; i < C_MAX_PIECES_IN_BAG; i++)
        {
            var p = new Piece();
            p._color = p.GetRandomPieceColor(Config.Get()._colorsOnBoard);
            p._subType = Piece.eSubType.NORMAL;
            tempBag.Add(p);
        }

        //change 1 to a bookend
        tempBag[0]._subType = Piece.eSubType.BOOKEND;

        Shuffle(tempBag);

        //append tempBag's items onto _pieces
        _pieces.AddRange(tempBag);
    }

    public void Reset()
    {
        Debug.Log("Resetting piece bag");
        
        //delete any existing pieces
        _pieces.Clear();
        AddToBag();

        //shuffle the pieces into random slots

     
    }
    private void Shuffle<T>(List<T> list)
    {
        System.Random rng = new System.Random();
        int n = list.Count;
        for (int i = n - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            T value = list[i];
            list[i] = list[j];
            list[j] = value;
        }
    }

    public Piece PeekAtUpcomingPiece(int indexFromTop)
    {
        if (_pieces.Count < C_MINIMUM_BAG_SIZE)
        {
            AddToBag();
        }

        if (indexFromTop >= _pieces.Count)
        {
            return null;
        }

        return _pieces[indexFromTop];
    }
    public Piece GetNextPiece()
    {
        if (_pieces.Count < C_MINIMUM_BAG_SIZE )
        {
            AddToBag();
        }

        //pop off the first piece
        var p = _pieces[0];
        _pieces.RemoveAt(0);
        return p;
    }

  
}
