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

        if (Input.GetKeyDown(KeyCode.Return))
        {
            OnPlayButton();
        }
    }


    public void OnPlayButton()
    {
        //startup game
        GameLogic.Get().StartGame();
        //set us to inactive
        RTUtil.SetActiveByNameIfExists("MainMenu", false);
    }
}
