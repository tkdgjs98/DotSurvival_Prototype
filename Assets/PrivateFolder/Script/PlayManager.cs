using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;
using System.Linq;

public class PlayManager : NetworkBehaviour
{
    public static PlayManager instance;
    public GameObject ScoreRecordPrefab;

    [SerializeField]
    private List<GamePlayerController> GamePlayerList = new List<GamePlayerController>();
    int playerCount;
    [SerializeField]
    private Text timerText;
    private float timer = 300f;
    private bool playing;

    private int spawnStack; //게임 도중 아이템 스폰할 때 사용할 변수

    void Awake()
    {
        instance = this;
        spawnStack = 0;
        GamePlayerList.Clear();
    }
    private void Start()
    {
        if(!isServer)return;
        StartCoroutine(GameReady());
    }

    // Update is called once per frame
    void Update()
    {
        GameTime();
        ClientTimer();
    }
    public void AddPlayer(GamePlayerController _player)
    {
        GamePlayerList.Add(_player);
    }

    private IEnumerator GameReady()
    {
        RpcNotPlaying();
        PauseAllCharacter(true);
        var manager = NetworkManager.singleton as RoomManager;
        while(manager.roomSlots.Count != GamePlayerList.Count)
        {
            yield return null;
        }
        InitializeRanking();

        yield return new WaitForSeconds(2f);
        Debug.Log("start");
        PauseAllCharacter(false);
        RpcPlaying();
    }

    private void GameTime()//게임 타임 조작 함수
    {   
        if(!isServer)return;
        if(!playing) return;
        if(timer > 0)
        {
            timer -= Time.deltaTime;
            timerText.text = TimerToText();
            if( timer<=225f && spawnStack==0 ) //일정 시간이 되었을 때 게임에 적용할 사항(아이템 스폰 등)
            {
                spawnStack++;
                Debug.Log("spawn");
                for(int i=0; i<11; i++)
                {
                    SpawnWeapon(2,TestSpawner.instance.itemPosition[i],i); //아이템 스폰
                    SpawnAccessory(2,TestSpawner.instance.itemPosition[i],i);
                    SpawnPotion(2, TestSpawner.instance.itemPosition[i], i);
                }
            }
            if( timer<=150f && spawnStack==1 ) //일정 시간이 되었을 때 게임에 적용할 사항(아이템 스폰 등)
            {
                spawnStack++;
                Debug.Log("spawn");
                for(int i=0; i<11; i++)
                {
                    SpawnWeapon(2,TestSpawner.instance.itemPosition[i],i); //아이템 스폰
                    SpawnAccessory(2,TestSpawner.instance.itemPosition[i],i);
                    SpawnPotion(2, TestSpawner.instance.itemPosition[i], i);
                }
            }
            if( timer<=75f && spawnStack==2 ) //일정 시간이 되었을 때 게임에 적용할 사항(아이템 스폰 등)
            {
                spawnStack++;
                Debug.Log("spawn");
                for(int i=0; i<11; i++)
                {
                    SpawnWeapon(2,TestSpawner.instance.itemPosition[i],i); //아이템 스폰
                    SpawnAccessory(2,TestSpawner.instance.itemPosition[i],i);
                    SpawnPotion(2, TestSpawner.instance.itemPosition[i], i);
                }
            }
        }
        else
        {
            EndTheGame();
        }
    }

    private string TimerToText() //타이머 텍스트 생성 함수
    {
        int sec = ((int) timer % 60);
        string secText = sec<10 ? "0"+sec.ToString():sec.ToString();
        return ((int) timer/60 % 60).ToString()+":"+secText;
    }
    [ClientRpc]
    private void RpcPlaying()
    {
        playing = true;
    }
    [ClientRpc]
    private void RpcNotPlaying()
    {
        playing = false;
    }
    private void ClientTimer()
    {
        if(isServer)return;
        if(!playing) return;
        if(timer > 0)
        {
            timer -= Time.deltaTime;
            timerText.text = TimerToText();
        }
    }
    [Server]
    private void EndTheGame() //게임이 끝날때 실행할 함수
    {
        RpcNotPlaying();
        Debug.Log("End the Game!");
        PauseAllCharacter(true);
        ScoreLog();
    }
    [Server]
    private void PauseAllCharacter(bool pause)
    {
        foreach(var player in GamePlayerList)
        {
            player.RpcPauseCharacter(pause); //캐릭터 멈추기
        }
    }
    public void ScoreLog() //플레이어 점수 로그 함수. ui로 변경해야함
    {
        Debug.Log("Score");
        foreach(var player in GamePlayerList)
        {
            Debug.Log("id: "+player.netId+", name: "+player.nickname+", kill: "+player.kill+", death: "+player.death+", totalDeal: "+player.totalDeal);
        }
    }

    public void SpawnWeapon(int count, Transform area, int areaIndex)
    {
        TestSpawner.instance.SpawnWeapon(count, area, areaIndex);
    }
    public void SpawnAccessory(int count, Transform area, int areaIndex)
    {
        TestSpawner.instance.SpawnAccessory(count, area, areaIndex);
    }
    public void SpawnPotion(int count, Transform area, int areaIndex)
    {
        TestSpawner.instance.SpawnPotion(count, area, areaIndex);
    }
    

    [SerializeField]
    private List<ScoreRecord> RankingList = new List<ScoreRecord>();
    public List<ScoreRecord> getRankingList()
    {
        return RankingList;
    }
    private void InitializeRanking()
    {
        foreach(var player in GamePlayerList)
        {
            GameObject newRecord = Instantiate(ScoreRecordPrefab);
            newRecord.GetComponent<ScoreRecord>().setPlayer(player);
            NetworkServer.Spawn(newRecord);

            RankingList.Add(newRecord.GetComponent<ScoreRecord>());
            RpcInitializeRanking(newRecord, player.gameObject);
        }
    }

    [ClientRpc]
    public void RpcInitializeRanking(GameObject _newRecord, GameObject _player)
    {
        if(isServer)return;
        GameObject newRecord = _newRecord;
        newRecord.GetComponent<ScoreRecord>().setPlayer(_player.GetComponent<GamePlayerController>());
        RankingList.Add(newRecord.GetComponent<ScoreRecord>());
    }

    public void UpdateRecordKD(uint _netId, bool iskiller)
    {
        int myRank = FindRankByNetId(_netId);
        if(myRank==-1)return;
        if(iskiller)
        {
            RankingList[myRank].PlusScore(1);
        }
        TryRankingUp(myRank);
    }
    [Server]
    public void TryRankingUp(int myRank)
    {
        if(myRank == -1) return;
        Debug.Log("killerRank :"+myRank);
        RankingUp(RankingList[myRank], myRank, 1);
    }
    [Server]
    public int FindRankByNetId(uint _netId)
    {
        foreach(var record in RankingList.Select((value,index)=>new{value,index}))
        {
            if(record.value.getPlayer().netId==_netId)
            {
                return record.index;
            }
        }
        return -1;
    }
    private void RankingUp(ScoreRecord record, int myRank, int pointer)
    {
        if(myRank-pointer <0)
        {
            UpdateRanking(myRank, pointer-1);
            return;
        }
        else
        {
            if(RankingList[myRank-pointer].getScore() > RankingList[myRank].getScore()) //나보다 높은 랭크였던 사람보다 점수가 낮을 때.
            {
                UpdateRanking(myRank, pointer-1); //이번 포인터보단 낮은 랭크로 업데이트.
                return;
            }
            else if(RankingList[myRank-pointer].getScore() == RankingList[myRank].getScore()) //나보다 높은 랭크였던 사람과 점수가 같을 때.
            {
                if(RankingList[myRank-pointer].getPlayer().death <= RankingList[myRank].getPlayer().death) //내 데스가 많을 때.
                {
                    UpdateRanking(myRank, pointer-1); //이번 포인터보단 낮은 랭크로 업데이트.
                    return;
                }
                else    //내 데스가 적을 때
                {
                    //랭크 업(재귀)
                    RankingUp(RankingList[myRank], myRank, pointer+1);
                }
            }
            else//나보다 높은 랭크였던 사람보다 점수가 높을 때.
            {
                //랭크 업(재귀)
                RankingUp(RankingList[myRank], myRank, pointer+1);
            }
        }

        
    }
    private void UpdateRanking(int myRank, int pointer)
    {
        RpcUpdateRanking(myRank,pointer);
        /*
        if(pointer==0)
        {
            return;
        }
        else
        {
            RpcUpdateRanking(myRank,pointer);
        }*/
    }
    [ClientRpc]
    public void RpcUpdateRanking(int myRank, int pointer)
    {
        ScoreRecord sr = RankingList[myRank];
        RankingList.Insert(myRank-pointer, sr);
        Debug.Log(RankLog());
        RankingList.RemoveAt(myRank+1);
        Debug.Log(RankLog());
        UpdatePanel(sr);
    }
    public void UpdatePanel(ScoreRecord sr)
    {
        sr.getPlayer().getUI().UpdateRankPanel();
    }
    public string RankLog()
    {
        string log="";
        foreach(var record in RankingList)
        {
            GamePlayerController player = record.getPlayer();
            //Debug.Log("id: "+record.getNetId()+", name: "+record.getNickname()+", kill: "+player.kill+", death: "+player.death+", totalDeal: "+player.totalDeal);
            log += "id: "+player.netId+", name: "+player.nickname+", score: "+record.getScore()+", kill: "+player.kill+", death: "+player.death+", totalDeal: "+player.totalDeal+"\n";
        }
        return log;
    }
    
}
