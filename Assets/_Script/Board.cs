using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.ProBuilder.Shapes;
using UnityEngine.UIElements;

public class BoardCell : System.Object
{
    public int _x, _y;
    public Piece _piece;
}

public class Board : System.Object
{
    public int _width = 10;
    public int _height = 10;

    public List<BoardCell> _cells;
    public Pieces _pieces;
    uint _pieceIDCounter;

    public void Init(int newX, int newY)
    {

        _width = newX;
        _height = newY;
        _cells = new List<BoardCell>();
        _cells.Capacity = newX * newY;

        _pieces = new Pieces();
        _pieces._board = this;
        _pieceIDCounter = 1;

        for (int x = 0; x < _width; x++)
            for (int y = 0; y < _height; y++)
            {
                BoardCell cell = new BoardCell();
                cell._x = x;
                cell._y = y;
                _cells.Add(cell);
            }

        RTConsole.Log("board initted, " + _cells.Count + " cells.");
    }

    public Pieces GetPieces() { return _pieces; }

    public uint GetUniquePieceID() { return _pieceIDCounter++; }

    public bool IsInitted() { return _cells != null; }

    public bool IsValidCell(Vector2Int pos, bool mustBeEmpty)
    {
        if (pos.x < 0 || pos.x >= _width) return false;
        if (pos.y < 0 || pos.y >= _height) return false;

        if (mustBeEmpty && GetCell(pos)._piece != null)
        {
            return false; //not empty
        }

        return true;
    }

    public bool IsValidCell(Vector3 pos, bool mustBeEmpty)
    { 
        return IsValidCell(new Vector2Int((int)pos.x, (int)pos.y), mustBeEmpty);
    }


    public Piece GetCellPiece(Vector2Int gridPos)
    {
        var cell = GetCell(gridPos);
        if (cell == null) return null;

        return cell._piece;
    }
    public BoardCell GetCell(Vector2Int gridPos)
    {
        if (gridPos.x < 0 || gridPos.x >= _width) return null;
        if (gridPos.y < 0 || gridPos.y >= _height) return null;

        return _cells[gridPos.x + (gridPos.y * _width)];
    }

    public int GetIndexOfFirstFilledSpaceInCollumn(int collumn)
    {
        for (int y = 0; y < _height; y++)
        {
            if (GetCell(new Vector2Int(collumn, y))._piece != null)
            {
                return y;
            }
        }

        return _height-1;
    }

    public bool ShakePiecesDown()
    {
        //remove empty vertical space, shifting pieces down
        bool didShift = false;

        for (int x = 0; x < _width; x++)
        {
            //go through each collumn and removed empty spots vertically
            for (int y = _height-1; y >= 0 ; y--)
            {
                if (GetCell(new Vector2Int(x, y))._piece == null)
                {
                    //empty spot, shift everything above it down
                    for (int y2 = y; y2 >= 0; y2--)
                    {
                        if (GetCell(new Vector2Int(x, y2))._piece != null)
                        {
                            //shift it down
                            GetCell(new Vector2Int(x, y))._piece = GetCell(new Vector2Int(x, y2))._piece;
                            GetCell(new Vector2Int(x, y2))._piece = null;
                            GetCell(new Vector2Int(x, y))._piece.SetPos(new Vector2Int(x, y));
                            //GetCell(new Vector2Int(x, y))._piece._displayPieceScript.MoveTo(new Vector2Int(x, y), 0.3f, 0);
                            didShift = true;
                            break;
                        }
                    }
                }
            }
        }


        return didShift;
    }

    public void SetCellPiece(Vector2Int gridPos, Piece p)
    {
        if (GetCell(gridPos)._piece != null)
        {
            RTConsole.Log("Warning:piece already there?!?!");
        }

        /*
        if (p.gridPos.x != -1 && p._type != Piece.eType.ON_FEEDER)
        {
            //it seems to have a valid position.  Remove it from the old location?
            if (GetCell(p.gridPos)._piece == p)
            {
                //remove it
                GetCell(p.gridPos)._piece = null;
            }
            else
            {
                RTConsole.Log("Error, old piece he was on isn't really him.  Bug?");
            }
        }
        */

        p.SetPos(gridPos);
        GetCell(gridPos)._piece = p;
    }

    public Vector2Int GetRandomOpenPosition()
    {
        Vector2Int v = new Vector2Int();

        for (int tries = 0; tries < 1000; tries++)
        {
            v.x = Random.Range(0, _width);
            v.y = Random.Range(0, _height);

            if (GetCell(v)._piece == null)
            {
                return v;
            }
        }

        RTConsole.Log("Error, no empty cell!");
        v.x = -1;
        v.y = -1;
        return v;
    }

    const int CELL_INVALID = -1;
    const int CELL_UNCHECKED = 0;
    const int CELL_CHECKED_AND_NO_MATCH = 1;
    const int CELL_CHECKED_AND_MATCH = 2;

    int GetShadowCellValue(List<int> boardShadowMap, Vector2Int gridPos)
    {
        if (gridPos.x < 0 || gridPos.x >= _width) return -1;
        if (gridPos.y < 0 || gridPos.y >= _height) return -1;

        return boardShadowMap[gridPos.x + (gridPos.y * _width)];
    }

    void SetShadowCellValue(List<int> boardShadowMap, Vector2Int gridPos, int value)
    {
        boardShadowMap[gridPos.x + (gridPos.y * _width)] = value;
    }

    void CheckForCorrectJewel(ref bool directionBlocked, int x, int y, Piece.eColor matchColor, int lineLength, ref int sidesConnectedThisLoop, ref List<Vector2Int> matchLine, ref List<int> colorsUsed, bool bAddMatch)
    {
        if (directionBlocked) return;

        var cell = GetCell(new Vector2Int(x, y));
        if (cell == null)
        {
            directionBlocked = true;
            return; //out of bounds
        }
        Piece p = cell._piece;
        if (p != null && p._subType == Piece.eSubType.NORMAL && !p.bIsInactive)
        {
            if (matchColor == Piece.eColor.ANY || matchColor == p._color)
            {
                colorsUsed[(int)p._color]++;
                sidesConnectedThisLoop++;
                if (bAddMatch)
                    matchLine.Add(new Vector2Int(x, y));
                return;
            }
        }

        directionBlocked = true;
    }
    public List<Vector2Int> CheckForCompletedSummonedFiresFromBookend(int x, int y, Piece basePiece)
    {
        int minimumSidesConnectedForMatch = 3;
        bool bBookendColorDontMatter = true;

        List<Vector2Int> totalMatches = new List<Vector2Int>(); ;
        List<Vector2Int> matchLine = new List<Vector2Int>();
        Piece.eColor matchColor = basePiece._color;

        if (bBookendColorDontMatter)
        {
            matchColor = Piece.eColor.ANY;
        }

        matchLine.Add(new Vector2Int(x, y));

        //create a vector array of ints, we'll use this to add 1 to each color used from the eColor enum

        List<int> colorsUsed = new List<int>();
        // Add '0' value size times to the list
        for (int i = 0; i < (int)Piece.eColor.COLOR_COUNT; i++)
        {
            colorsUsed.Add(0);
        }

        int lineLength = 0;
        bool bFirstPass = true;

        bool topBlocked = false;
        bool rightBlocked = false;
        bool bottomBlocked = false;
        bool leftBlocked = false;


        while (true)
        {
            lineLength++;
            int sidesConnectedThisLoop = 0;
           
            //top
            CheckForCorrectJewel(ref topBlocked, x, y- lineLength, matchColor, lineLength, ref sidesConnectedThisLoop, ref matchLine, ref colorsUsed, !bFirstPass);
            //right
            CheckForCorrectJewel(ref rightBlocked, x + lineLength, y , matchColor, lineLength, ref sidesConnectedThisLoop, ref matchLine, ref colorsUsed,!bFirstPass);
            //bottom
            CheckForCorrectJewel(ref bottomBlocked, x, y +lineLength, matchColor, lineLength, ref sidesConnectedThisLoop, ref matchLine, ref colorsUsed, !bFirstPass);
            //left
            CheckForCorrectJewel(ref leftBlocked, x - lineLength, y, matchColor, lineLength, ref sidesConnectedThisLoop, ref matchLine, ref colorsUsed, !bFirstPass);

            if (bFirstPass)
            {
                //this is the first pass, did we do well enough to proceed?
                //scan through colorsUsed and figure out the number with the heighest number
                int highestColorCount = 0;
                int highestColorIndex = 0;
                for (int i = 0; i < colorsUsed.Count; i++)
                {
                    if (colorsUsed[i] > highestColorCount)
                    {
                        highestColorCount = colorsUsed[i];
                        highestColorIndex = i;
                    }
                }

                if (highestColorCount >= minimumSidesConnectedForMatch)
                {
                    //we have enough of the same color to continue.  Actually, let's re-do this step as we now know the color we can use
                    bFirstPass = false;
                    matchColor = (Piece.eColor)highestColorIndex;
                    lineLength = 0;
                    continue; 
                }
                else
                {
                    //FAIL
                    break;
                }
            } else
            {
                //not our first pass
                if (sidesConnectedThisLoop == 0)
                {
                    //guess we're done.
                    totalMatches = matchLine;
                    break;
                }
            }

        }

        return totalMatches;
    }

    public List<Vector2Int> AddTouchingSameColorGemsTolist(List<Vector2Int> gemList)
    {
        if (gemList.DefaultIfEmpty().Count() == 0) return new List<Vector2Int>();

        //first, copy the current gem list to a new list
        List<Vector2Int> newGemList = new List<Vector2Int>();
        newGemList.AddRange(gemList);

        //now go through each gem on the gemList and add any touching gems that are the same color to the new
        //newGemList, if it isn't already on it.

        //check each direction
        Vector2Int[] directions = new Vector2Int[4];
        directions[0] = new Vector2Int(0, -1); //up
        directions[1] = new Vector2Int(1, 0); //right
        directions[2] = new Vector2Int(0, 1); //down
        directions[3] = new Vector2Int(-1, 0); //left


        for (int i = 0; i < gemList.Count; i++)
        {
            Vector2Int v = gemList[i];

            var originalCell = GetCellPiece(v);

            if (originalCell == null) continue;

            if (originalCell._subType == Piece.eSubType.BOOKEND)
            {
                //bookends don't count
                continue;
            }

            Piece.eColor color = originalCell._color;

           
            for (int d = 0; d < 4; d++)
            {
                Vector2Int v2 = v + directions[d];
                if (IsValidCell(v2, false))
                {
                    var cell = GetCellPiece(v2);
                    if (cell == null) continue;

                    //ignore bookends here too
                    if (cell._subType == Piece.eSubType.BOOKEND)
                    {
                        continue;
                    }
                    Piece.eColor color2 = cell._color;
                    if (color2 == color)
                    {
                        //add it to the new list
                        if (!newGemList.Contains(v2))
                        {
                            newGemList.Add(v2);
                            gemList.Add(v2); //we'll need to check this new cell's surrounding as well
                        }
                    }
                }
            }
        }

        //return the list we made
        return newGemList;
    }

    //this version checks if diamonds are growing out of bookends and detects that, doesn't require two bookends
    public List<Vector2Int> CheckForCompletedSummonedFires()
    {
         List<Vector2Int> totalMatches = new List<Vector2Int>(); ;

        //scan to find bookends
        for (int y=0; y < _height; y++)
        {
            for (int x=0; x < _width; x++)
            {
                Piece p = GetCell(new Vector2Int(x, y))._piece;

                if (p != null && !p.bIsInactive && p._subType == Piece.eSubType.BOOKEND)
                {
                    totalMatches = CheckForCompletedSummonedFiresFromBookend(x,y, p);

                    if (totalMatches.Count > 0)
                    {
                        //guess we found one, early drop out
                       
                        //print all matches to the log
                        string s = "Matches: ";
                        foreach (Vector2Int v in totalMatches)
                        {
                            s += v.ToString() + " ";
                        }
                        RTConsole.Log(s);
                        return totalMatches;
                    }
                }
            }
        }

        return totalMatches;
    }

    //if diamonds are between two "bookends", we detect that both horizontally and vertically
    public List<Vector2Int> CheckForCompletedBookends()
    {
        int minimumLengthRequiredForMatch = 3;
        bool bBookendColorDontMatter = true;

        List<Vector2Int> matchLine = new List<Vector2Int>();
        Piece.eColor matchColor = Piece.eColor.PURPLE;
        List<Vector2Int> totalMatches = new List<Vector2Int>(); ;

        for (int y = 0; y < _height; y++)
        {
            matchLine.Clear();
          
            for (int x = 0; x < _width; x++)
            {
                Piece p = GetCell(new Vector2Int(x, y))._piece;

                if (p == null)
                {
                    matchLine.Clear();
                    continue;
                }

                if (matchLine.Count > 0)
                {
                    if (p._color == matchColor || p._color == Piece.eColor.ANY || matchColor == Piece.eColor.ANY|| (bBookendColorDontMatter && p._subType == Piece.eSubType.BOOKEND))
                    {
                        if (p._subType == Piece.eSubType.NORMAL)
                        {
                            //line is getting longer...
                            //we have now chosen a color
                            matchColor = p._color;
                            matchLine.Add(new Vector2Int(x, y));
                        }
                        if (p._subType == Piece.eSubType.BOOKEND)
                        {
                            if (matchLine.Count >= minimumLengthRequiredForMatch-1)
                            {
                                //ended match with correct color
                                matchLine.Add(new Vector2Int(x, y));
                                //copy matches to the end of totalmatches
                                totalMatches.AddRange(matchLine);
                                matchLine.Clear();

                                //allow the end to be used again for something
                                matchLine.Add(new Vector2Int(x, y));
                            } else
                            {
                                matchLine.Clear();
                                //allow the end to be used again for something
                                matchLine.Add(new Vector2Int(x, y));
                            }

                        }
                    }
                    else
                    {
                        matchLine.Clear();
                    }
                }
                else
                {
                    //starting a new line?

                    //check for matches
                    if (p._subType == Piece.eSubType.BOOKEND)
                    {
                        //start of a match
                        matchColor = p._color;
                        if (bBookendColorDontMatter)
                        {
                            matchColor = Piece.eColor.ANY;
                        }
                        
                        matchLine.Add(new Vector2Int(x, y));
                    }
                }

            }
        }
        
     
        //same thing, but check for vertical matches
        for (int x = 0; x < _width; x++)
        {
            matchLine.Clear();

            for (int y = 0; y < _height; y++)
            {
                Piece p = GetCell(new Vector2Int(x, y))._piece;

                if (p == null)
                {
                    matchLine.Clear();
                    continue;
                }

                if (matchLine.Count > 0)
                {
                    if (p._color == matchColor || p._color == Piece.eColor.ANY || matchColor == Piece.eColor.ANY || (bBookendColorDontMatter && p._subType == Piece.eSubType.BOOKEND))
                    {
                        if (p._subType == Piece.eSubType.NORMAL)
                        {
                            //we have now chosen a color
                            matchColor = p._color;
                            //line is getting longer...
                            matchLine.Add(new Vector2Int(x, y));
                        }
                        if (p._subType == Piece.eSubType.BOOKEND)
                        {
                            if (matchLine.Count >= minimumLengthRequiredForMatch-1)
                            {
                                //ended match with correct color
                                matchLine.Add(new Vector2Int(x, y));
                                //copy matches to the end of totalmatches
                                totalMatches.AddRange(matchLine);
                                matchLine.Clear();

                                //allow the end to be used again for something
                                matchLine.Add(new Vector2Int(x, y));
                            }
                            else
                            {
                                matchLine.Clear();
                                //allow the end to be used again for something
                                matchLine.Add(new Vector2Int(x, y));
                            }
                        }
                    }
                    else
                    {
                        matchLine.Clear();
                    }
                }
                else
                {
                    //starting a new line?

                    //check for matches
                    if (p._subType == Piece.eSubType.BOOKEND)
                    {
                        //start of a match
                        matchColor = p._color;
                        if (bBookendColorDontMatter)
                        {
                            matchColor = Piece.eColor.ANY;
                        }
                        matchLine.Add(new Vector2Int(x, y));
                    }
                }

            }
        }


        //normally I'd separate the effects from the logic, but for this simple game, I'm going to do it all in one place

        //first scan whole board by each row

        return totalMatches;
    }
}
