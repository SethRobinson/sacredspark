using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SpriteRow
{
    public Sprite[] sprites;
}
public class PieceDisplay : MonoBehaviour
{
   
    //remember our Sprite
    public SpriteRenderer spriteRenderer;
    // Start is called before the first frame update
    Vector2Int _actualPos = new Vector2Int(-1, -1);
    Piece _piece;
    Tweener _moveTween, _scaleTween, _moveTween2;

    [SerializeField]
    private SpriteRow[] rows;  // This will show up in the inspector with expandable rows


    public Vector3 GetPosFromGrid()
    {
        return new Vector3(_actualPos.x, _actualPos.y, 0);
    }
    public void MoveTo(Vector2Int newPos, float timeToTake, float preMoveDelay = 0)
    {

        // print(" moving piece.  Is white: " + _isWhite + " piece is white: " + _piece._isWhite);

        if (newPos == _actualPos)
        {
            //print("Ignoring movement, already there");
            return;
        }

        Vector3 pos = transform.localPosition;

        if (_moveTween != null)
        {
            _moveTween.Kill(true);

            //kill(false) should work, but the next tween goes to wrong place.. so I kill true (letting it finish),
            //them manually move it back to where it should be before starting the next tween
            transform.localPosition = pos;
        }

        pos.x = newPos.x;
        pos.y = newPos.y;

        _moveTween = transform.DOLocalMove(pos, timeToTake).OnKill(() => _moveTween = null);
        _moveTween.SetDelay(preMoveDelay);
        _moveTween.SetEase(Ease.InOutQuad);
        _actualPos = newPos;
        //transform.localPosition = pos;
    }

    public void MoveTo(Vector3 newPos, float timeToTake, float preMoveDelay = 0)
    {

        Vector3 pos = transform.localPosition;
        Vector3 scale = transform.localScale;

        if (_moveTween2 != null)
        {
            _moveTween2.Kill(true);

            //kill(false) should work, but the next tween goes to wrong place.. so I kill true (letting it finish),
            //them manually move it back to where it should be before starting the next tween
            transform.localPosition = pos;
        }

        if (_scaleTween != null)
        {
            _scaleTween.Kill(true);
        }

        _moveTween2 = transform.DOLocalMove(newPos, timeToTake).OnKill(() => _moveTween2 = null);
        _moveTween2.SetDelay(preMoveDelay);
        _moveTween2.SetEase(Ease.InOutQuad);

        _scaleTween = transform.DOScale(scale * 1.2f, timeToTake).OnKill(() => _scaleTween = null);
        _scaleTween.SetDelay(preMoveDelay);
        _scaleTween.SetEase(Ease.InOutQuad);

    }

    public void SetVisualsToPieceType()
    {
        if (_piece._subType == Piece.eSubType.NORMAL) 
        {
            int colorID = (int)_piece._color;
            spriteRenderer.sprite = rows[colorID].sprites[0];
        }


        if (_piece._subType == Piece.eSubType.BOOKEND)
        {
            spriteRenderer.sprite = rows[(int)_piece._color].sprites[1];
        }
    }

    public void SetAssociatedPiece(Piece p)
    {
        _piece = p;
        SetVisualsToPieceType();
    }
    
    void Start()
    {
        
    }
    private void OnDestroy()
    {
        if (_moveTween != null)
        {
            _moveTween.Kill(true);
        }

        if (_moveTween2 != null)
        {
            _moveTween2.Kill(true);
        }

        if (_scaleTween != null)
        {
            _scaleTween.Kill(true);
        }

    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
