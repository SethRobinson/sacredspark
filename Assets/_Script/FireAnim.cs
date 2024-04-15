using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireAnim : MonoBehaviour
{
    public Sprite[] sprites;

    //we'll show the sprites as an anim by hand
    public SpriteRenderer spriteRenderer;
    public float _timeBetweenFrames = 0.03f;
    public float _timeOfNextFrame = 0;
    public int _curFrame = 0;

    // Start is called before the first frame update
    void Start()
    {
        //setup the timer
        SetTimeToNextFrame();
    }

    void SetTimeToNextFrame()
    {
        _timeOfNextFrame = Time.time + _timeBetweenFrames;
    }
    // Update is called once per frame
    void Update()
    {
        //time to update the frame yet?
        if (Time.time > _timeOfNextFrame)
        {
            //yes, update the frame
            _curFrame++;
            if (_curFrame >= sprites.Length-1)
            {
                //done, kill us
                Destroy(gameObject);
                return; 
            }
            spriteRenderer.sprite = sprites[_curFrame];
            SetTimeToNextFrame();
        }
    }
}
