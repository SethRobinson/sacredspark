/*
Source code by Seth A. Robinson for LD55
 */

//#define RT_NOAUDIO

using UnityEngine;
using DG.Tweening;

public class GameLogic : MonoBehaviour
{
    Board _board;
    TableDisplay _tableDisplay;
    public PieceBag _pieceBag;
    //keep track of the Entity we'll spawn
    GameObject _floatingPieces = null; //our instance we create

    Piece.eColor _nextPieceColor = Piece.eColor.ANY;
    Piece.eSubType _nextPieceSubType = Piece.eSubType.NORMAL;

    Piece.eColor _nextPieceColor2 = Piece.eColor.ANY;
    Piece.eSubType _nextPieceSubType2 = Piece.eSubType.NORMAL;
    int _score = 0;
    public TMPro.TMP_Text _scoreText;
    public TMPro.TMP_Text _levelText;
    float _level;

    int _comboMultiplier = 1;

    int _pieceClearCount = 0; //how many lines the player has completed in the current game
    int _linesNeededToIncreaseLevel = 10;


    public GameObject _floatingPiecesPrefab; //the prefab we copy it from
    public Board GetBoard() { return _board; }
    public float GetComboMultiplier() { return _comboMultiplier; }
    static GameLogic _this = null;

    static public GameLogic Get()
    {
        return _this;
    }


    public static string GetName()
    {
        return Get().name;
    }

    public void ResetComboMultiplier()
    {
        //Debug.Log("Resetting Multipliar");
        _comboMultiplier = 1;
    }
    
    public void IncreaseComboMultiplier()
    {
        _comboMultiplier = _comboMultiplier*2;
    }
    public PieceBag GetPieceBag()
    {
        return _pieceBag;
    }
    private void Awake()
    {
        _pieceBag = new PieceBag();
        Application.targetFrameRate = 20000;
        QualitySettings.vSyncCount = 0;
        //QualitySettings.antiAliasing = 4;
        /*
        Debug.unityLogger.filterLogType = LogType.Log;

        Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.ScriptOnly);
        Application.SetStackTraceLogType(LogType.Error, StackTraceLogType.ScriptOnly);
        Application.SetStackTraceLogType(LogType.Assert, StackTraceLogType.ScriptOnly);
        Application.SetStackTraceLogType(LogType.Exception, StackTraceLogType.ScriptOnly);
        Application.SetStackTraceLogType(LogType.Warning, StackTraceLogType.ScriptOnly);
        */
    }

    public void ShowTitleScreenFromIntroSplash()
    {
        //play music
        RTMessageManager.Get().Schedule(0.1f, RTAudioManager.Get().PlayMusic, "title_screen_music", 0.5f, 1.0f, true);
        ShowTitleScreen();
    }

    public void ShowTitleScreen()
    {
        Debug.Log("Showing title");
        //turn on the "MainMenu" object by name
        RTUtil.SetActiveByNameIfExists("MainMenu", true);
    }

    public void SpawnFloatingPieces()
    {
        if (_floatingPieces != null)
        {
            Debug.LogError("Why does a floating piece already exist?");
            return;
        }

        RTConsole.Log("Creating floating pieces");
        //create our instance from the prefab
        _floatingPieces = Instantiate(_floatingPiecesPrefab, _tableDisplay.GetPiecesFolder().transform);
        if (_nextPieceColor != Piece.eColor.ANY)
        {
            _floatingPieces.GetComponent<FloatingPieces>().SetNextPiece(_nextPieceColor, _nextPieceSubType, _nextPieceColor2, _nextPieceSubType2);
            _nextPieceColor = Piece.eColor.ANY;
            _nextPieceSubType = Piece.eSubType.NORMAL;
            _nextPieceColor2 = Piece.eColor.ANY;
            _nextPieceSubType2 = Piece.eSubType.NORMAL;
        }
        //make it start in the top middle
        _floatingPieces.transform.localPosition = new Vector3((int) (_board._width / 2)-1, 0, 0);
    }

    public void ModScore(int mod)
    {
        _score += mod;
        UpdateScore();
    }

    public void UpdateScore()
    {
       _scoreText.text = _score.ToString();
    }

    public float GetLevel()
    {
        return _level;
    }
    
    void UpdateLevelText()
    {
        _levelText.text = _level.ToString();
    }


    void CalculateLevel()
    {

        _level = 1+ (int)((float)_pieceClearCount / (float)_linesNeededToIncreaseLevel);
        UpdateLevelText();
    }

    public int GetPieceClearCount()
    {
        return _pieceClearCount;
    }
    public void SetPieceClearCount(int pieces)
    {
        _pieceClearCount = pieces;
        CalculateLevel();
    }
    public void ModPieceClearCount(int mod)
    {
        _pieceClearCount += mod;
        CalculateLevel();

    }

    public void AddPiece(int x, int y, Piece.eColor color, Piece.eSubType subType)
    {
        //if a piece is already there, delete it

        var oldp = _board.GetCellPiece(new Vector2Int(x, y));

        if (oldp != null)
        {
            Destroy(oldp._displayPieceScript.gameObject);
            oldp._displayPieceScript = null;

            _board.GetCell(oldp.gridPos)._piece = null;
        }

        Piece p = _board.GetPieces().AddPiece(_board.GetUniquePieceID());
        p._color = color;
        p._subType = subType;
        _board.SetCellPiece(new Vector2Int(x, y), p);

        //also add it visually
        _tableDisplay.AddPiece(p);
    }

    public void ForceNextPiece(Piece.eColor color, Piece.eSubType subType, Piece.eColor color2, Piece.eSubType subType2)
    {
       _nextPieceColor = color;
       _nextPieceSubType = subType;
       _nextPieceColor2 = color2;
       _nextPieceSubType2 = subType2;
    }
    public void AddSetup1ToStartup()
    {

        AddJunkToStartup();

        AddPiece(4,5, Piece.eColor.PURPLE, Piece.eSubType.NORMAL);
        AddPiece(5, 5, Piece.eColor.PURPLE, Piece.eSubType.NORMAL);
        AddPiece(6, 5, Piece.eColor.PURPLE, Piece.eSubType.NORMAL);

        ForceNextPiece(Piece.eColor.PURPLE, Piece.eSubType.NORMAL, Piece.eColor.PURPLE, Piece.eSubType.BOOKEND);
        //AddPiece(4, 9, Piece.eColor.RED, Piece.eSubType.NORMAL);
    }
    public void AddJunkToStartup()
    {

        //add pieces and setup main board
        for (int x = 0; x < _board._width; x++)
        {
            for (int y = _board._height / 2; y < _board._height; y++)
            {
                Piece p = _board.GetPieces().AddPiece(_board.GetUniquePieceID());
                p._color = p.GetRandomPieceColor(Config.Get()._colorsOnBoard);
                _board.SetCellPiece(new Vector2Int(x, y), p);

                //also add it visually
                _tableDisplay.AddPiece(p);
            }
        }

    }

    public void StopGame()
    {
        if (_floatingPieces != null)
        {
            Destroy(_floatingPieces);
            _floatingPieces = null;
        }
    }
    public void StartGame()
    {
        RTUtil.SetActiveByNameIfExists("ScoreCanvas", true);
        Debug.Log("Starting game");
        _score = 0;
        SetPieceClearCount(0);

        UpdateScore();
        _board.Init(10, 10);
        _tableDisplay.Setup(_board);
        //add pieces to the feeder and setup its display
        //AddJunkToStartup();
        //AddSetup1ToStartup();

        NextPieceDisplay.Get().UpdatePieces();
        SpawnFloatingPieces();
        RTMessageManager.Get().Schedule(0.1f, RTAudioManager.Get().PlayMusic, "playing_music", 0.1f, 1.0f, true);
    }


    public void OnFloatingPieceWasPlaced()
    {
        //kill us
        Destroy(_floatingPieces);
        _floatingPieces = null;
        SpawnFloatingPieces();
    }


    public void CheckBoardForCompletions()
    {
        var matches = GameLogic.Get().GetBoard().CheckForCompletedSummonedFires();
        matches = GameLogic.Get().GetBoard().AddTouchingSameColorGemsTolist(matches);

        if (matches.Count > 0)
        {
            //freeze game while doing a little anim
            //pause?
            TableDisplay.Get().CompletedLine(matches);
        } else
        {
            //no more combo I guess
            GameLogic.Get().ResetComboMultiplier();
        }
    }
    public void OnPlayerIsDead()
    {
        Destroy(_floatingPieces);
        _floatingPieces = null;
        //StartGame();
        //play music with RTMessageManager
        RTMessageManager.Get().Schedule(0.1f, RTAudioManager.Get().PlayMusic, "gameover_music", 0.5f, 1.0f, true);
        ShowTitleScreen();
    }

    // Use this for initialization
    void Start()
    {
        _this = this;

        DOTween.Init(true, true, LogBehaviour.Verbose).SetCapacity(200, 20);

#if RT_NOAUDIO
		AudioListener.pause = true;
#endif

        /*
              if (RTUtil.DoesCommandLineWordExist("-runfullserver"))
                {
                    print("Detected -runfullserver flag");
                    _isServer = true;
                }
            */

        RTConsole.Get().SetShowUnityDebugLogInConsole(true);
       
        //RTEventManager.Get().Schedule(RTAudioManager.GetName(), "PlayMusic", 1, "intro");
        string version = "Unity V "+ Application.unityVersion+" :";

        #if RT_BETA
        print ("Beta build detected!");
#endif

        RTConsole.Get().SetMirrorToDebugLog(true);
        RTConsole.Log("Initting the board");
        _board = new Board();
        _tableDisplay = GameObject.Find("Table").GetComponent<TableDisplay>();
        // RTAudioManager.Get().SetDefaultMusicVol(0.4f);
        //StartGame();
    }

   
	void OnApplicationQuit() 
	{
        // Make sure prefs are saved before quitting.
        //PlayerPrefs.Save();
        RTConsole.Log("Application quitting normally");

//        NetworkTransport.Shutdown();
        print("QUITTING!");
    }
    

    private void OnDestroy()
    {
        print("Game logic destroyed");
    }
    
    // Update is called once per frame
    void Update () 
    {
    }

}
