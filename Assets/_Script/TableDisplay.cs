using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TableDisplay : MonoBehaviour
{
    Board _board;
    public Renderer _rend;
    public GameObject _piecesFolder;
    public GameObject _piecePrefab;
    public GameObject _flamePrefab;
    public GameObject _scoreOverlayPrefab;

    // Start is called before the first frame update
    static TableDisplay _this;
    void Awake()
    {
        _this = this;
    }
    public static TableDisplay Get() { return _this; }
    public GameObject GetPiecePrefab() { return _piecePrefab; }
    public GameObject GetPiecesFolder() { return _piecesFolder; }
    void Start()
    {
    }

    public Vector2Int WorldToGrid(Vector3 worldPos)
    {
        if (_board == null) return new Vector2Int(0, 0); //bad data
        //upper left of the board should be 0,0
        return new Vector2Int(
           Mathf.RoundToInt(worldPos.x - _piecesFolder.transform.position.x),
           Mathf.RoundToInt((_board._height - (worldPos.y + _piecesFolder.transform.position.y)) - 1)
           );
    }

    public void Setup(Board newBoard)
    {
        _board = newBoard;
        _rend.transform.localScale = new Vector3(_board._width, _board._width, 1);
        _rend.material.SetTextureScale("_MainTex", new Vector2(_board._width, _board._width));

        for (int i = 0; i < _piecesFolder.transform.childCount; i++)
        {
            GameObject.Destroy(_piecesFolder.transform.GetChild(i).gameObject);
        }

        //offset the pieces gameobject so 0,0 puts things on the 0,0 square
        float tileOffset = 0.5f;
        Vector3 vPos = _piecesFolder.transform.position;
        vPos.x = -(_board._width / 2 - tileOffset);
        vPos.y = _board._height / 2 - tileOffset;

        _piecesFolder.transform.position = vPos;
        GameLogic.Get().ResetComboMultiplier();
        UpdateCameraZoom();
    }

    public void AddPiece(Piece p)
    {
        GameObject pieceObj = Instantiate<GameObject>(_piecePrefab, _piecesFolder.transform);
        pieceObj.transform.localPosition = new Vector3(p.gridPos.x, p.gridPos.y, 0);
        p._displayPieceScript = pieceObj.GetComponent<PieceDisplay>();
        p._displayPieceScript.SetAssociatedPiece(p);
    }

    public void KillPiece(Piece p)
    {
        //play a sfx
        RTAudioManager.Get().PlayEx("flame", 1.0f, 1.0f, false, 0.04f);
        Destroy(p._displayPieceScript.gameObject);
        p._displayPieceScript = null;
        _board.GetCell(p.gridPos)._piece = null;
    }

    public void ShakePiecesDown()
    {

        if (_board.ShakePiecesDown())
        {
            //rebuild the board
            foreach (BoardCell cell in _board._cells)
            {
                if (cell._piece != null)
                {
                    cell._piece._displayPieceScript.MoveTo(cell._piece.gridPos, 0.3f, 0);
                }
            }

            //better check to see if this shift has made anything cool happen, schedule a call to GameLogic.Get().CheckBoardForCompletions() in 0.3f
            RTMessageManager.Get().Schedule(0.3f, GameLogic.Get().CheckBoardForCompletions);
        }
    }

    public void SpawnEffectsOverlayAtCoord(Vector2Int coord, int score)
    {

        //spawn flame anim prefab
        GameObject flameObj = Instantiate<GameObject>(_flamePrefab.gameObject, _piecesFolder.transform);
        var vlocalPos = flameObj.transform.localPosition;
        flameObj.transform.localPosition = new Vector3(coord.x, coord.y, vlocalPos.z);

        if (score > 0)
        {
            GameObject scoreObj = Instantiate<GameObject>(_scoreOverlayPrefab.gameObject, _piecesFolder.transform);

            vlocalPos = scoreObj.transform.localPosition;
            scoreObj.transform.localPosition = new Vector3(coord.x, coord.y, vlocalPos.z);
            string extra = "";
            if (GameLogic.Get().GetComboMultiplier() > 1)
            {
                extra = " (X" + ((int)GameLogic.Get().GetComboMultiplier()).ToString() + "!)";
            }
            scoreObj.GetComponent<ScoreOverlay>().SetText(score.ToString() + extra);
        }
    }

    public void CompletedLine(List<Vector2Int> matches)
    {
        float delaySeconds = 0;
        const float clearAnimSpeedSeconds = 0.1f;

        //delete all matching coords
        foreach (Vector2Int v in matches)
        {
            Piece p = _board.GetCell(v)._piece;
            if (p != null)
            {
                p.bIsInactive = true;
                RTMessageManager.Get().Schedule(delaySeconds, this.KillPiece, p);
                delaySeconds += clearAnimSpeedSeconds;

                //calculate distance from first match to this one
                float distance = Mathf.Max(Mathf.Abs(v.x - matches[0].x), Mathf.Abs(v.y - matches[0].y));
                int score = (int)(1.0f * distance * distance * GameLogic.Get().GetComboMultiplier()*GameLogic.Get().GetLevel());

                RTMessageManager.Get().Schedule(delaySeconds, GameLogic.Get().ModScore, score);
                RTMessageManager.Get().Schedule(delaySeconds, GameLogic.Get().ModPieceClearCount, 1);
                RTMessageManager.Get().Schedule(delaySeconds, this.SpawnEffectsOverlayAtCoord, v, score);

            }
        }

        if (matches.Count > 0)
        {
            delaySeconds += 0.2f; //little extra

            RTMessageManager.Get().Schedule(delaySeconds, this.ShakePiecesDown);
            RTMessageManager.Get().Schedule(delaySeconds, GameLogic.Get().IncreaseComboMultiplier);
        }
    }
    void UpdateCameraZoom()
    {
        //change camera to correct zoom
        if (Camera.main == null || _board == null) return;

        float extraHeight = 2;

        Camera.main.orthographicSize = (_board._height / 2) + extraHeight;

        //adjust for portrait mode so the board will still fit
        if (Screen.width < Screen.height)
        {
            float difference = Screen.height - Screen.width;
            Camera.main.orthographicSize += ((difference / 100) * 1.5f);
        }

        //print("Cam rect: " + Camera.main.rect);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
