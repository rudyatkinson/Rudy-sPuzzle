using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelManager : MonoBehaviour
{
    private static LevelManager _i;
    public static LevelManager _I { get { return _i; } }
    private void Awake()
    {
        if (_i == null)
            _i = this;
        if (levelsDict == null)
            levelsDict = new Dictionary<Level, bool>();

        GameObject[] objs = GameObject.FindGameObjectsWithTag("LevelManager");

        if (objs.Length > 1)
        {
            Destroy(this.gameObject);
        }

        DontDestroyOnLoad(this.gameObject);
        LevelDoneEvent = new UnityEvent();
        LevelDoneEvent.AddListener(DoneCurrentLevel);
    }

    [SerializeField]
    private Level[] levels;
    public Dictionary<Level, bool> levelsDict { get; private set; }

    public Level currentLevel { get; private set; }
    public int turnCount { get; private set; }
    public UnityEvent LevelDoneEvent { get; private set; }

    public Block lastMovedBlock;
    public int timesUsedGetBackMove { get; private set; }

    private void Start()
    {
        foreach (Level l in levels)
        {
            if (!levelsDict.ContainsKey(l))
                levelsDict.Add(l, false);
        }
        bool[] savedLevels = SaveSystem.LoadData();
        if (savedLevels != null)
        {
            for(int i = 0; i < levelsDict.Count(); i++)
                levelsDict[levelsDict.ElementAt(i).Key] = savedLevels[i];
        }
    }

    public void SetLevel(Level level)
    {
        currentLevel = level;
        turnCount = currentLevel.Turn;
        ChangeSceneWith(2);
        timesUsedGetBackMove = 0;
    }

    public void TurnCountDecreasement()
    {
        if(--turnCount == 0 && !BlockManager._I.win)
        {
            LevelInformationManager._I.LevelFailedEvent.Invoke();
            SoundManager._I.gameMusic.Stop();
            if (SoundManager._I.audioSource.isActiveAndEnabled)
                SoundManager._I.audioSource.PlayOneShot(SoundManager._I.Lose);
        }
    }

    public void GetBackMove()
    {
        if (lastMovedBlock != null)
        {
            turnCount += 2;
            lastMovedBlock.OnClick();
            lastMovedBlock = null;
            LevelInformationManager._I.RefreshTurnCount();
            LevelInformationManager._I.IsBackButtonNull();
            timesUsedGetBackMove++;
        }
    }

    public void LeaveLevel()
    {
        ChangeSceneWith(1);
    }

    private void DoneCurrentLevel()
    {
        levelsDict[currentLevel] = true;
        CustomAnalytics.LevelDone();
        if (SoundManager._I.audioSource.isActiveAndEnabled)
            SoundManager._I.audioSource.PlayOneShot(SoundManager._I.Win);
        LevelInformationManager._I.LevelDoneEvent.Invoke();
        SaveSystem.SaveData(levelsDict);
    }

    private static void ChangeSceneWith(int sceneIndex)
    {
        SceneManager.LoadScene(sceneIndex, LoadSceneMode.Single);
    }
}
