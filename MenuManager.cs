using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    private static MenuManager _i;
    public static MenuManager _I { get { return _i; } }

    private void Awake()
    {
        if (_i == null)
            _i = this;
    }

    [SerializeField]
    private GameObject levelButtonPrefab;
    [SerializeField]
    private Transform levelButtonLocation;

    // Start is called before the first frame update
    void Start()
    {
        RefreshLevelButtons();
    }

    private void RefreshLevelButtons()
    {
        bool nextLevel = false;
        foreach (KeyValuePair<Level, bool> dict in LevelManager._I.levelsDict)
        {
            GameObject levelButton = Instantiate(levelButtonPrefab, levelButtonLocation);
            if (dict.Value == true || !nextLevel)
            {
                levelButton.transform.GetChild(0).gameObject.GetComponent<Text>().text = dict.Key.ToString();
                levelButton.GetComponent<Button>().interactable = true;
                levelButton.GetComponent<Button>().onClick.AddListener(delegate 
                {
                    if (!SoundManager._I.gameMusic.isPlaying && SoundManager._I.gameMusic.isActiveAndEnabled)
                        SoundManager._I.gameMusic.Play();
                    LevelManager._I.SetLevel(dict.Key); 
                });
                levelButton.transform.GetChild(1).gameObject.SetActive(false);
                
                if(dict.Value)
                    levelButton.GetComponent<Image>().color = new Color32(111, 248, 135, 255);
                else
                {
                    levelButton.GetComponent<Image>().color = new Color32(248, 245, 111, 255);
                    nextLevel = true;
                }
                    
            }
        }
    }
}
