//#define DEBUG_OnClick_PrintPositionAndDistanceToFreePos

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Block : MonoBehaviour
{
    public string letters { get; private set; }
    public Vector2 position { get; private set; }

    public void Start()
    {
        GetComponent<Button>().onClick.AddListener(OnClick);
    }

    public void OnClick()
    {
#if DEBUG_OnClick_PrintPositionAndDistanceToFreePos
        Debug.Log("Position: " + position +"\nDistance To FreePos: " + Vector2.Distance(BlockManager._I.freePos, position));
#endif
        if(Vector2.Distance(BlockManager._I.freePos, position) == 1)
        {
            LevelManager._I.lastMovedBlock = this;
            transform.position = BlockManager._I.locations[(int)(BlockManager._I.freePos.x + (BlockManager._I.freePos.y * 5))].position;
            Vector2 tempPos = position;
            position = BlockManager._I.freePos;
            BlockManager._I.ChangeFreePos(tempPos, this);
            LevelManager._I.TurnCountDecreasement();
            LevelInformationManager._I.RefreshTurnCount();
            LevelInformationManager._I.IsBackButtonNull();
            if(SoundManager._I.audioSource.isActiveAndEnabled)
                SoundManager._I.audioSource.PlayOneShot(SoundManager._I.Click);
        }
    }

    public void SetBlockAttributes(string newLetters, Vector2 position)
    {
        if(letters == null)
        {
            letters = newLetters;
            this.position = position;
            GameObject go = transform.GetChild(1).gameObject;
            go.GetComponent<Text>().text = letters;
        }
    }
}
