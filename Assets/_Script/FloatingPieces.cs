
using UnityEngine;
using static Piece;
using UnityEngine.InputSystem;

//this class handles making pieces float down and be controllable.  I think I want
// more than one piece to be able to float down at a time, so I'll need to keep track of them

public class PieceData
{
    public GameObject _visualPiece;
    public Piece _piece;
    public Vector2Int vOffset;
    public Vector3 GetPosFromGrid()
    {
        return new Vector3(vOffset.x, vOffset.y, 0);
    }

}

public class FloatingPieces : MonoBehaviour
{

    public enum eRotPos
    {
        RIGHT,
        BOTTOM,
        LEFT,
        TOP,

        SIZE
    }

    eRotPos _rotPos = eRotPos.RIGHT;

    //array of the Piece objects we'll spawn as a group
    PieceData[] _pieceData;

    
    bool _bAllowSplittingOfPieces = true; //if true, they can break off pieces
    bool _bAllowFullRotation = true; //if false, we only allow shifting the blocks within the shape
    bool _bWantToRotateLeft = false;
    bool _bWantToRotateRight = false;

    float _fallTimer = 0;
    Vector3 _totalPiecesSize;
    bool _bDead = false;
    float _nextLeftMoveTime;
    float _nextRightMoveTime;
    float _nextDownMoveTime;
    float _ignoreKeyTimer;
    bool _bDirWasHeldDownLastFrame;
  
    const float C_IGNORE_KEY_TIME = 0.2f; //we don't allow movement right after this spawns to help stop false moves
    //for debugging, a way to cheat
    Piece.eColor _nextPieceColor = Piece.eColor.ANY;
    Piece.eSubType _nextPieceSubType = Piece.eSubType.NORMAL;

    Piece.eColor _nextPieceColor2 = Piece.eColor.ANY;
    Piece.eSubType _nextPieceSubType2 = Piece.eSubType.NORMAL;


    // Start is called before the first frame update
    void Start()
    {
        _ignoreKeyTimer = Time.fixedTime + C_IGNORE_KEY_TIME;
        ResetFallTimer();
        //spawn the pieces
        _pieceData = new PieceData[Config.Get().GetPieceCountGivenAtOnce()];
        
        for (int i = 0; i < Config.Get().GetPieceCountGivenAtOnce(); i++)
        {
            _pieceData[i] = new PieceData();
            _pieceData[i]._visualPiece = Instantiate(TableDisplay.Get().GetPiecePrefab(), transform);
            _pieceData[i]._piece = GameLogic.Get().GetPieceBag().GetNextPiece();
            _pieceData[i]._piece.SetPos(new Vector2Int(-1, -1)); //an illegal position

            if (i == 0)
            {
                //override piece?
                if (_nextPieceColor != Piece.eColor.ANY)
                {
                    _pieceData[i]._piece._color = _nextPieceColor;
                    _pieceData[i]._piece._subType = _nextPieceSubType;
                    _nextPieceColor = eColor.ANY;
                }
            }

            if (i == 1)
            {
                //override piece?
                if (_nextPieceColor2 != Piece.eColor.ANY)
                {
                    _pieceData[i]._piece._color = _nextPieceColor2;
                    _pieceData[i]._piece._subType = _nextPieceSubType2;
                    _nextPieceColor2 = eColor.ANY;
                }
            }

            _pieceData[i]._piece._netID = GameLogic.Get().GetBoard().GetUniquePieceID();
            _pieceData[i]._visualPiece.GetComponent<PieceDisplay>().SetAssociatedPiece(_pieceData[i]._piece);
        }

        SetPiecePosition(_rotPos);

        if (!IsValidLocationOffset(Vector3.zero))
        {
            //they can't fit it here.  Give up I guess (could try moving a little?)
            _bDead = true;

            RTAudioManager.Get().StopMusic();
            RTAudioManager.Get().PlayEx("gameover", 0.6f);

            RTMessageManager.Get().Schedule(2.0f, GameLogic.Get().OnPlayerIsDead);
            return;
        }

        //oh, I guess we better update our next pieces
        NextPieceDisplay.Get().UpdatePieces();

        //link to buttons we care about
        PlayerControls.Get().GetInput().Player.RotateLeft.started += OnRotateLeft;
        PlayerControls.Get().GetInput().Player.RotateRight.started += OnRotateRight;
    }

    public void OnRotateLeft(InputAction.CallbackContext value)
    {
        _bWantToRotateLeft = true;
    }

    public void OnRotateRight(InputAction.CallbackContext value)
    {
        _bWantToRotateRight = true;
    }

    public void SetNextPiece(Piece.eColor color, Piece.eSubType subType,Piece.eColor color2, Piece.eSubType subType2)
    {
        _nextPieceColor = color;
        _nextPieceSubType = subType;
        _nextPieceColor2 = color2;
        _nextPieceSubType2 = subType2;
    }   

    void SetPiecePosition(eRotPos pos)
    {

        if (FallingPieceCount() == 1)
        {
            //we only have one piece, this doesn't make sense
            for (int i = 0; i < Config.Get().GetPieceCountGivenAtOnce(); i++)
            {
                if (_pieceData[i] != null)
                {
                    _pieceData[i].vOffset = new Vector2Int(0, 0);
                }
            }
            UpdateFallingPieceVisuals();

            return;
        }
        //for each piece
      if (pos == eRotPos.RIGHT)
        {
            _pieceData[0].vOffset = new Vector2Int(0, 0);
            _pieceData[1].vOffset = new Vector2Int(1, 0);
        }

        if (pos == eRotPos.LEFT)
        {
            _pieceData[0].vOffset = new Vector2Int(0, 0);
            _pieceData[1].vOffset = new Vector2Int(-1, 0);
        }

        if (pos == eRotPos.TOP)
        {
            _pieceData[0].vOffset = new Vector2Int(0, 0);
            _pieceData[1].vOffset = new Vector2Int(0, 1);
        }
        if (pos == eRotPos.BOTTOM)
        {
            _pieceData[0].vOffset = new Vector2Int(0, 0);
            _pieceData[1].vOffset = new Vector2Int(0, -1);
        }

        UpdateFallingPieceVisuals();

        //show debug into
        //RTConsole.Log("RotPos: " + pos.ToString());
    }

    void UpdateFallingPieceVisuals()
    {
        //update the visuals themselves

        for (int i = 0; i < Config.Get().GetPieceCountGivenAtOnce(); i++)
        {
            if (_pieceData[i] != null)
            {
                _pieceData[i]._visualPiece.transform.localPosition = new Vector3(_pieceData[i].vOffset.x, _pieceData[i].vOffset.y, 0);
            }
        }
    }
    bool IsValidLocationOffset(Vector3 vOffset)
    {
        for (int i = 0; i < Config.Get().GetPieceCountGivenAtOnce(); i++)
        {
            if (_pieceData[i] != null)
            {
                if (!GameLogic.Get().GetBoard().IsValidCell(transform.localPosition + _pieceData[i].GetPosFromGrid() + vOffset, true))
                {
                    return false;
                }
            }
        }

        return true;
    }
   
    void ConvertPieceToRealBlock(int i)
    {
        Vector2Int gridPos = new Vector2Int((int)transform.localPosition.x + (int)_pieceData[i]._visualPiece.transform.localPosition.x,
            (int)transform.localPosition.y + (int)_pieceData[i]._visualPiece.transform.localPosition.y);
        GameLogic.Get().GetBoard().SetCellPiece(gridPos, _pieceData[i]._piece);
        TableDisplay.Get().AddPiece(_pieceData[i]._piece); //make it visual as we're about to trash these

        //clear piece data, destroy the _visualPiece
        Destroy(_pieceData[i]._visualPiece);
        _pieceData[i]._piece = null;
        _pieceData[i] = null;
    }

    void MovePieces(Vector3 vOffset)
    {
        //reject move if any piece moves into an illegal thing horizontally

        int piecesPlaced = 0;
        int piecesPlacedThisScan = 0;

        if (vOffset.y != 0)
        {
            //they are moving down. (it's not like we can move up)
            for (int i = 0; i < Config.Get().GetPieceCountGivenAtOnce(); i++)
            {
                if (_pieceData[i] != null)
                {
                    if (!GameLogic.Get().GetBoard().IsValidCell(transform.localPosition + _pieceData[i].GetPosFromGrid() + vOffset, true))
                    {
                        //they hit something.  Let's make this piece stop here
                        piecesPlaced++;
                        piecesPlacedThisScan++;

                        if (piecesPlacedThisScan == 1)
                        {
                            RTAudioManager.Get().Play("place");
                        }
                        ConvertPieceToRealBlock(i);
                    }
                }
                else
                {
                    piecesPlaced++; //well, it WAS placed, earlier
                }
            }
        }


        if (piecesPlaced > 0)
        {
            if (_rotPos == eRotPos.BOTTOM || _rotPos == eRotPos.TOP || !_bAllowSplittingOfPieces)
            {
                //both blocks need to be placed
                piecesPlaced = Config.Get().GetPieceCountGivenAtOnce();

                for (int i = 0; i < Config.Get().GetPieceCountGivenAtOnce(); i++)
                {
                    if (_pieceData[i] != null)
                    {
                        ConvertPieceToRealBlock(i);
                        piecesPlacedThisScan++;
                    }
                }
            }

            if (piecesPlaced == Config.Get().GetPieceCountGivenAtOnce())
            {
                GameLogic.Get().CheckBoardForCompletions();
                GameLogic.Get().OnFloatingPieceWasPlaced();
                return; //done
            }
        }

        if (piecesPlaced > 0)
        {
            //var matches = GameLogic.Get().GetBoard().CheckForCompletedBookends();
            GameLogic.Get().CheckBoardForCompletions();
            //return;
        }
            //move each piece down by 1.0
            transform.localPosition += vOffset;
    }

    void ResetFallTimer()
    {

        float slowestSpeed = 1.5f; //this is the starting speed on level one
        float fastestSpeed = 0.1f;
        float maxLevel = 14;
        
        float curLevel = GameLogic.Get().GetLevel(); //between 1 and maxLevel, but we'll cap it at maxLevel

        float speed = slowestSpeed - (slowestSpeed - fastestSpeed) * (curLevel / maxLevel);
        if (speed < fastestSpeed) speed = fastestSpeed;
        // RTConsole.Log("Level: " + curLevel.ToString() + " Speed: " + speed.ToString()+" ratio: "+(curLevel/maxLevel));

        _fallTimer = Time.fixedTime + speed;
    }
    //make the piece move down by 1.0 every _blockFallSpeedInSeconds interval

    void Update()
    {
        if (_bDead) return;

        if (_fallTimer < Time.fixedTime)
        {
            MovePieces(new Vector3(0, 1, 0));
            ResetFallTimer();
        }

        HandleHorizontalMovement();

        if (_ignoreKeyTimer < Time.fixedTime)
        {
            HandleVerticalMovement();
        }
        HandleRotation();
        HandleQuickDrop();

        if (PlayerControls.Get().GetInput().Player.Back.WasPressedThisFrame())
        {
            RTAudioManager.Get().StopMusic();
            RTMessageManager.Get().Schedule(0.0f, GameLogic.Get().OnPlayerIsDead);
        }

    }

    void HandleHorizontalMovement()
    {

        const float blockMoveSfxVolMod = 0.4f;
        
        float repeatRate = 0.1f;  // Time in seconds between moves when key is held

        if (!_bDirWasHeldDownLastFrame)
        {
            //first movement has a longer delay, feels more like tetris controls
            repeatRate = 0.22f;
        }

      
        if (PlayerControls.Get().vDir.x < 0 || PlayerControls.Get().vDir.x > 0)
        {
            _bDirWasHeldDownLastFrame = true;
        } else
        {
            _bDirWasHeldDownLastFrame = false;
        }

        if (PlayerControls.Get().bLeftOrRightPressedThisFrame)
        {
            //they actually tapped the button on the dpad so yes, they can move right now
            _nextLeftMoveTime = 0;
            _nextRightMoveTime = 0;
        }

        if (PlayerControls.Get().vDir.x < 0 && Time.time >= _nextLeftMoveTime)
        {
            if (IsValidLocationOffset(new Vector3(-1, 0, 0)))
            {
       
                RTAudioManager.Get().PlayEx("move", blockMoveSfxVolMod);

                MovePieces(new Vector3(-1, 0, 0));
                _nextLeftMoveTime = Time.time + repeatRate;
            }
        }

        if (PlayerControls.Get().vDir.x > 0 && Time.time >= _nextRightMoveTime)
        {
            if (IsValidLocationOffset(new Vector3(1, 0, 0)))
            {
      
                RTAudioManager.Get().PlayEx("move", blockMoveSfxVolMod);
                MovePieces(new Vector3(1, 0, 0));
                _nextRightMoveTime = Time.time + repeatRate;
            }
        }
    }

    void DropBlocksDownOne()
    {
        float downRepeatRate = 0.05f;  // More frequent as typically down is a quick move

        if (PlayerControls.Get().bUpOrDownThisFrame)
        {
            downRepeatRate = 0.1f;
        }
         MovePieces(new Vector3(0, 1, 0));
        _nextDownMoveTime = Time.time + downRepeatRate;
    }
    void HandleVerticalMovement()
    {
        if (PlayerControls.Get().bUpOrDownThisFrame)
        {
            _nextDownMoveTime = 0;
        }

        if (PlayerControls.Get().vDir.y < 0 && Time.time >= _nextDownMoveTime)
        {
            DropBlocksDownOne();
        }
    }

    int FallingPieceCount()
    {
        int pieceCount = 0;

        for (int i = 0; i < Config.Get().GetPieceCountGivenAtOnce(); i++)
        {
            if (_pieceData[i] != null)
            {
                pieceCount++;
            }
        }
        return pieceCount;
    }

    void ChangeOrder(int changeBY)
    {
        RTAudioManager.Get().Play("rotate");

        if (FallingPieceCount() < 2) return;

        //swap _pieceData[0].vOffset with _pieceData[1].vOffset
        Vector2Int temp = _pieceData[0].vOffset;
        _pieceData[0].vOffset = _pieceData[1].vOffset;
        _pieceData[1].vOffset = temp;
        UpdateFallingPieceVisuals();
    }

    void HandleRotation()
    {
        eRotPos originalRotation = _rotPos;  // Store the original rotation before any changes

        if (_bAllowFullRotation)
        {
            if (_bWantToRotateLeft)
            {
                _bWantToRotateLeft = false;

                RotatePiece(originalRotation, true);
            }
            if (_bWantToRotateRight)
            {
                _bWantToRotateRight = false;
                RotatePiece(originalRotation, false);
            }
        } else
        {
            //only allow alternative the block types

            if (_bWantToRotateLeft)
            {
                _bWantToRotateLeft = false;
                {
                    ChangeOrder(1);
                }
                if (_bWantToRotateRight)
                {
                    _bWantToRotateRight = false;
                    ChangeOrder(1);
                }
            }
            
        }
    }

    void RotatePiece(eRotPos originalRotation, bool clockwise)
    {
        bool bAllowUnlimitedTetrisStyleTimingAfterRotation = true;

        RTAudioManager.Get().Play("rotate");
        
        if (FallingPieceCount() < 2) return; //no reason to rotate at this point
        
        if (clockwise)
        {
            _rotPos = (eRotPos)(((int)_rotPos + 1) % (int)eRotPos.SIZE);
        }
        else
        {
            _rotPos = (eRotPos)(((int)_rotPos - 1 + (int)eRotPos.SIZE) % (int)eRotPos.SIZE);
        }
        SetPiecePosition(_rotPos);
        bool bValid = false;


        if (IsValidLocationOffset(Vector3.zero))
        {
            bValid = true;
        }

        //bounce off left obstacle
        if (!bValid && IsValidLocationOffset(new Vector3(1, 0, 0)))
        {
            bValid = true;
            MovePieces(new Vector3(1, 0, 0));
        }

        //bounce off right obstacle
        if (!bValid && IsValidLocationOffset(new Vector3(-1, 0, 0)))
        {
            bValid = true;
            MovePieces(new Vector3(-1, 0, 0));
        }
        //up
        if (!bValid && IsValidLocationOffset(new Vector3(0, -1, 0)))
        {
  
            bValid = true;
            MovePieces(new Vector3(0, -1, 0));
        }

        if (!bValid && IsValidLocationOffset(new Vector3(0, 1, 0)))
        {
      
            bValid = true;
            MovePieces(new Vector3(0, 1, 0));
        }

        //if there is a tile below us, allow unlimited time:
        if (bAllowUnlimitedTetrisStyleTimingAfterRotation && !IsValidLocationOffset(new Vector3(0, 1, 0)))
        {
            ResetFallTimer(); //tetris style, being nice and giving unlimited time when rotating
        }

        ResetFallTimer(); //tetris style, being nice and giving unlimited time when rotating


        if (!bValid)
        {
            //we give up
            _rotPos = originalRotation;
            SetPiecePosition(_rotPos);
        }
    }

    void HandleQuickDrop()
    {
        if (PlayerControls.Get().vDir.y > 0 && PlayerControls.Get().bUpOrDownThisFrame)
        {
            QuickDrop();
        }
    }

    void QuickDrop()
    {
      
            //RTAudioManager.Get().Play("rotate");
            while (FallingPieceCount() > 0)
            {
                MovePieces(new Vector3(0, 1, 0));
            }
            //ResetFallTimer(); //this would give us a second to move after it hits the bottom, but that feels...wrong
            _fallTimer = 0; //instantly place the block

          
    }

}
