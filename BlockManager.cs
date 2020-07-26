//#define DEBUG_ReadRightBlocks
//#define DEBUG_ReadBelowBlocks

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockManager : MonoBehaviour
{
    private static BlockManager _i;
    public static BlockManager _I { get { return _i; } }
    private void Awake()
    {
        if (_i == null)
            _i = this;

    }

    private string[] list;
    private string answer;

    public GameObject preBlock;
    public GameObject parentObject;
    public Transform[] locations;

    public bool win { get; private set; }

    public Block[,] blockMap { get; private set; }
    private List<GameObject> horizontalAnimatedBlocks;
    private List<GameObject> antiHorizontalAnimatedBlocks;
    private List<GameObject> verticalAnimatedBlocks;
    private List<GameObject> antiVerticalAnimatedBlocks;

    public Vector2 freePos { get; private set; }

    private void Start()
    {
        horizontalAnimatedBlocks = new List<GameObject>();
        antiHorizontalAnimatedBlocks = new List<GameObject>();
        verticalAnimatedBlocks = new List<GameObject>();
        antiVerticalAnimatedBlocks = new List<GameObject>();
        blockMap = new Block[5,8];
        list = LevelManager._I.currentLevel.Blocks;
        answer = LevelManager._I.currentLevel.Answer;
        win = false;

        int blockCount = 0;
        foreach(string s in list)
        {
            //if first string is not null block instantiates, else freePos declared.
            if(s != "null")
            {
                GameObject go = Instantiate(preBlock, parentObject.transform);
                go.GetComponent<Block>().SetBlockAttributes(s, new Vector2(blockCount % 5, blockCount / 5));
                blockMap[blockCount % 5, blockCount / 5] = go.GetComponent<Block>();
                go.transform.position = locations[blockCount++].transform.position;
                go.name = blockCount % 5 + "," + blockCount / 5;
            }
            else
            {
                freePos = new Vector2(blockCount % 5, blockCount / 5);
                blockMap[blockCount % 5, blockCount / 5] = null;
                blockCount++;
            }
        }
        LevelInformationManager._I.OpenQuestionPanel();
        LevelInformationManager._I.getBackTurnButton.onClick.AddListener(LevelManager._I.GetBackMove);
        LevelInformationManager._I.SyncAudioSettings();
    }

    //This method declared to change freePos and blocks call this if any block change the position. 
    public void ChangeFreePos(Vector2 newFreePos, Block block)
    {
        blockMap[(int)freePos.x, (int)freePos.y] = block;
        isBlockHasAnswer(blockMap[(int)freePos.x, (int)freePos.y].position);
        blockMap[(int)newFreePos.x, (int)newFreePos.y] = null;
        freePos = newFreePos;
    }

    private void isBlockHasAnswer(Vector2 position)
    {
        if(answer.Contains(blockMap[(int)position.x, (int)position.y].letters))
        {
            if (ReadHorizontalBlocks(position) && ReadVerticalBlocks(position))
            {
                StartCoroutine(blockAnimations(blockMap[(int)position.x, (int)position.y].gameObject));
                win = true;
            }
            else if (ReadHorizontalBlocks(position) || ReadVerticalBlocks(position))
            {
                StartCoroutine(blockAnimations(blockMap[(int)position.x, (int)position.y].gameObject));
                win = true;
            }
        }    
    }

    private bool ReadHorizontalBlocks(Vector2 position)
    {
        string readingBlock = blockMap[(int)position.x, (int)position.y].letters;

        for(int i = (int)position.x + 1; i < 5; i++)
        {
            if (answer.Contains(readingBlock + blockMap[i, (int)position.y].letters))
            {
                if(!horizontalAnimatedBlocks.Contains(blockMap[i, (int)position.y].gameObject))
                horizontalAnimatedBlocks.Add(blockMap[i, (int)position.y].gameObject);
                readingBlock += blockMap[i, (int)position.y].letters;
            }
            else
                break;
        }

        for(int i = (int)position.x - 1; i > -1; i--)
        {
            if (answer.Contains(blockMap[i, (int)position.y].letters + readingBlock))
            {
                if(!antiHorizontalAnimatedBlocks.Contains(blockMap[i, (int)position.y].gameObject))
                    antiHorizontalAnimatedBlocks.Add(blockMap[i, (int)position.y].gameObject);
                readingBlock = blockMap[i, (int)position.y].letters + readingBlock;
            }
            else
                break;
        }

#if DEBUG_ReadRightBlocks
        Debug.Log("ReadRightBlocks: " + readingBlock);
#endif

        if (readingBlock == answer)
            return true;

        horizontalAnimatedBlocks.Clear();
        antiHorizontalAnimatedBlocks.Clear();
        return false;
    }

    private bool ReadVerticalBlocks(Vector2 position)
    {
        string readingBlock = blockMap[(int)position.x, (int)position.y].letters;

        for (int i = (int)position.y - 1; i > -1; i--)
        {
            if (answer.Contains(readingBlock + blockMap[(int)position.x, i].letters))
            {
                if(!verticalAnimatedBlocks.Contains(blockMap[(int)position.x, i].gameObject))
                    verticalAnimatedBlocks.Add(blockMap[(int)position.x, i].gameObject);
                readingBlock += blockMap[(int)position.x, i].letters;
            }
            else
                break;
        }

        for(int i = (int)position.y + 1; i < 8; i++)
        {
            if (answer.Contains(blockMap[(int)position.x, i].letters + readingBlock))
            {
                if(!antiVerticalAnimatedBlocks.Contains(blockMap[(int)position.x, i].gameObject))
                    antiVerticalAnimatedBlocks.Add(blockMap[(int)position.x, i].gameObject);
                readingBlock = blockMap[(int)position.x, i].letters + readingBlock;
            }
            else
                break;
        }
#if DEBUG_ReadBelowBlocks
            Debug.Log("ReadBelowBlocks: " + readingBlock);
#endif
        if (readingBlock == answer)
            return true;

        verticalAnimatedBlocks.Clear();
        antiVerticalAnimatedBlocks.Clear();
        return false;
    }

    private IEnumerator blockAnimations(GameObject mainBlock)
    {
        SoundManager._I.gameMusic.Stop();
        mainBlock.GetComponent<Animator>().Play("doneAnimationMain");
        if (SoundManager._I.audioSource.isActiveAndEnabled)
            SoundManager._I.audioSource.PlayOneShot(SoundManager._I.BlockFilling);
        yield return new WaitForSeconds(mainBlock.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).length / 2);
        
        while(true)
        {
            if (horizontalAnimatedBlocks.Count == 0 && antiHorizontalAnimatedBlocks.Count == 0 && verticalAnimatedBlocks.Count == 0 && antiVerticalAnimatedBlocks.Count == 0)
                break;

            if (horizontalAnimatedBlocks.Count != 0)
            {
                horizontalAnimatedBlocks[0].GetComponent<Animator>().Play("doneAnimationHorizontal");
                horizontalAnimatedBlocks.RemoveAt(0);
            }

            if(antiHorizontalAnimatedBlocks.Count != 0)
            {
                antiHorizontalAnimatedBlocks[0].GetComponent<Animator>().Play("doneAnimationAntiHorizontal");
                antiHorizontalAnimatedBlocks.RemoveAt(0);
            }

            if(verticalAnimatedBlocks.Count != 0)
            {
                verticalAnimatedBlocks[0].GetComponent<Animator>().Play("doneAnimationVertical");
                verticalAnimatedBlocks.RemoveAt(0);
            }

            if (antiVerticalAnimatedBlocks.Count != 0)
            {
                antiVerticalAnimatedBlocks[0].GetComponent<Animator>().Play("doneAnimationAntiVertical");
                antiVerticalAnimatedBlocks.RemoveAt(0);
            }
            if (SoundManager._I.audioSource.isActiveAndEnabled)
                SoundManager._I.audioSource.PlayOneShot(SoundManager._I.BlockFilling);
            yield return new WaitForSeconds(mainBlock.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).length);
        }
        yield return new WaitForSeconds(1f);
        LevelManager._I.LevelDoneEvent.Invoke();
    }

}
