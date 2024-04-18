using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        if (!RTUtil.IsOnMobile())
        {
            //guess we don't need the button overlays
            RTUtil.SetActiveByNameIfExists("ControlCanvas", false);
        }
    }

    // Update is called once per frame
    void Update()
    {

        if (PlayerControls.Get().GetInput().Player.Start.WasPressedThisFrame()
            || PlayerControls.Get().GetInput().Player.RotateLeft.WasPressedThisFrame()
            || PlayerControls.Get().GetInput().Player.RotateRight.WasPressedThisFrame())
        {
            OnPlayButton();
        }
        if (PlayerControls.Get().GetInput().Player.Back.WasPressedThisFrame())
        {
            OnInfoButton();
        }

    }


    public void OnPlayButton()
    {
        //startup game
        GameLogic.Get().StartGame();
        //set us to inactive
        RTUtil.SetActiveByNameIfExists("MainMenu", false);
    }

    public void OnInfoButton()
    {
        //startup game
        GameLogic.Get().StopGame();
        //set us to inactive
        RTUtil.SetActiveByNameIfExists("MainMenu", false);
        RTUtil.SetActiveByNameIfExists("RTIntroSplash", true);
        RTAudioManager.Get().StopMusic();
    }
}
