using System.Collections.Generic;
using UnityEngine;

public class Piece : System.Object
{

    public enum eType
    {
        ON_BOARD,
        ON_FEEDER
    }

    public enum eSubType
    {
        NORMAL,
        BOOKEND
    }

    public enum eColor
    {
        PURPLE,
        BLUE,
        ORANGE,
        RED,
        ANY, //special case, means it can match any color

        COLOR_COUNT
    }

    public string GetColorString()
    {
        switch (_color)
        {
            case eColor.PURPLE:
                return "Red";
            case eColor.BLUE:
                return "Blue";
            default:
                return "Something";
        }
    }

    public Piece.eColor GetRandomPieceColor(int maxColors)
    {
        //randomize the color
        Piece.eColor color = Piece.eColor.BLUE;

        switch (Random.Range(0, maxColors))
        {
            case 0:
                color = Piece.eColor.PURPLE; break;
            case 1:
                color = Piece.eColor.BLUE; break;
            case 2:
                color = Piece.eColor.ORANGE; break;
            case 3:
                color = Piece.eColor.RED; break;
            default:
                //error
                Debug.Log("Error, bad color");
                break;         
        }

        return color;
    }


    public Piece() { }

    public Piece(Piece p)
    {
        _name = p._name;
        _netID = p._netID;
        _score = p._score;
        _color = p._color;
        _type = p._type;
        _subType = p._subType;
    }

    public void SetPos(Vector2Int pos) { gridPos = pos; }

    public eColor _color = eColor.PURPLE;
    public string _name;
    public uint _netID;
    public Vector2Int gridPos { get; private set; }
    //public PieceDisplay _displayPieceScript; //only used by client
    public bool _isHuman;
    public int _score;
    public eType _type = eType.ON_BOARD;
    public eSubType _subType = eSubType.NORMAL;
    public PieceDisplay _displayPieceScript; //only used by client
    public bool bIsInactive = false;

}

public class Pieces : System.Object
{
    public Dictionary<uint, Piece> _pieces = new Dictionary<uint, Piece>();
    public Board _board;

    public Piece AddPiece(uint netID)
    {
        Piece p = new Piece();
        p.SetPos(new Vector2Int(-1, -1)); //an illegal position
        p._netID = netID;
        _pieces.Add(netID, p);
        return p;
    }

    public void RemovePiece(uint netID)
    {
        Piece p = GetPiece(netID);
        if (p.gridPos != new Vector2Int(-1, -1))
        {
            //it's on the board...
            BoardCell cell = _board.GetCell(p.gridPos);
            if (cell._piece == p)
            {
                cell._piece = null; //won't exist anymore
            }
            else
            {
                RTConsole.Log("Couldn't remove piece " + p.gridPos + " from board correctly");
            }
        }
        _pieces.Remove(netID);
    }

    public Piece GetPiece(uint netID)
    {
        Piece item;
        _pieces.TryGetValue(netID, out item);
        return item;
    }

}
