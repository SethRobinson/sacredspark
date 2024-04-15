using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;

public class ScoreOverlay : MonoBehaviour
{
    public TMPro.TMP_Text _scoreText;

    public void SetText(string text)
    {
        _scoreText.text = text;
    }
    
    // Start is called before the first frame update
    void Start()
    {
        
    
        // Ensure the text color is set to use alpha (just in case)
        _scoreText.color = new Color(_scoreText.color.r, _scoreText.color.g, _scoreText.color.b, 1);

        _scoreText.DOFade(0, 0.3f).SetDelay(0.7f);
        Destroy(gameObject, 1.0f);
        //make the number float up at a slow pace
        transform.DOMoveY(transform.position.y + 1.0f, 1.0f).SetEase(Ease.OutQuad);


    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
