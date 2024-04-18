using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RTIntroSplash : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void OnCloseButtonClicked()
    {
       // Debug.Log("Close button clicked");
        //GameObject.Destroy(gameObject);
        RTUtil.SetActiveByNameIfExists("RTIntroSplash", false);
        GameLogic.Get().ShowTitleScreen();
    }
    public void OnLogoClicked()
    {
        //Debug.Log("Clicked logo, opening website");
        RTUtil.PopupUnblockOpenURL("https://www.rtsoft.com");

    }
    // Update is called once per frame
    void Update()
    {

        if (PlayerControls.Get().GetInput().Player.Start.WasPressedThisFrame()
            || PlayerControls.Get().GetInput().Player.RotateLeft.WasPressedThisFrame()
            || PlayerControls.Get().GetInput().Player.RotateRight.WasPressedThisFrame())
        { 
            OnCloseButtonClicked();
        }
    }
}
