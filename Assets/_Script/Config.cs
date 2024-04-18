using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class Config : MonoBehaviour
{  
    public static bool _isTestMode = false; //could do anything, _testMode is checked by random functions
    public const float _clientVersion = 0.1f;
    int _pieceCountGivenAtOnce = 2;
    public bool _demoMode = true; //special mode that overlays info at all times so this is easier to understand for C2 Gamedev night on 4/19/2024
    public int _colorsOnBoard;
    static Config _this;
    public List<AudioClip> _audioClips; //any audio added will be seen by RTAudioManager and can be played like:
    //RTAudioManager.Get().Play("marble1");

    //RTAudioManager.Get().PlayEx("climb", 0.1f, 1.0f, true, 0.1f);

    //RTMessageManager.Get().Schedule(2, RTAudioManager.Get().Play, "crap.wav"); //plays crap.wav from Resources dir in 2 seconds

    //Note that you have to include ALL optional parms, and clearly define int vs float (via the f at the end)
    //RTMessageManager.Get().Schedule(1, RTAudioManager.Get().PlayEx, "blip_lose2", 1.0f, 1.0f, false, 0.0f);

    void Awake()
    {
#if RT_BETA

#endif
        _this = this;
        _colorsOnBoard = 2;
        RTAudioManager.Get().AddClipsToLibrary(_audioClips);

    }
    public int GetPieceCountGivenAtOnce() { return _pieceCountGivenAtOnce; }
    static public Config Get() { return _this; }

   

}
