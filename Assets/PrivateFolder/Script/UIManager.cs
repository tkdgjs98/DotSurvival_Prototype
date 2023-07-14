using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    private GamePlayerController player;
    [SerializeField]
    private GunController gun;

    public GameObject AimPoint;
    public GameObject KDPanel;
    public GameObject DeathPanel;
    public AudioSource audioSrc;
    public Text text_Kill;
    public Text text_Death;
    public Text text_Bullet;
    public Text text_RespawnTimer;
    public float respawnTimer;
    public Slider reload_Bar;
    public Slider HP_Bar;
    public Image img_HP;
    public Image img_Fill;
    public Image img_Danger;
    public Image img_WeaponSlot;
    public Image img_Weapon;
    public GameObject RankPanel;
    public List<GameObject> rankSlot;
    bool activeRankPanel;
    bool rankInit;
    public GameObject Inventory;                        //인벤토리 객체. 인스펙터 설정 필요
    public Slot[] slot = new Slot[4];
    public Button[] button = new Button[4];
    bool activeInventory;                               //인벤토리 숨김 여부 검사 변수

    void Start()
    {
        player = gameObject.GetComponent<GamePlayerController>();
        gun = GetComponent<GunController>();
        InventoryInitialize();
        InitializeRankPanel();
        InitializeLocalUI();
        UpdateKD();
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Confined;     //인벤토리 초기화
    }
    // Update is called once per frame
    void Update()
    {
        AimMove();
        
        //UpdateKD();
        Check_CoolTime();
        Check_Reload();
        if (player.isLocalPlayer)
        {
            OnDeath_Panel();
            ManageSound();
        }
    }

    private void InitializeLocalUI()
    {
        if (!player.isLocalPlayer)
        {
            AimPoint.SetActive(false);
            KDPanel.SetActive(false);
            DeathPanel.SetActive(false);
            HP_Bar.gameObject.SetActive(false);
            img_Danger.gameObject.SetActive(false);
            img_WeaponSlot.gameObject.SetActive(false);
            img_Weapon.gameObject.SetActive(false);
            audioSrc.gameObject.SetActive(false);
            text_Bullet.gameObject.SetActive(false);
        }
    }

    //---------------------------------------------------------------------------------------------
    public void ShowInventoryUI()
    {
        Cursor.visible = !Cursor.visible;
        if(Cursor.visible)
            Cursor.lockState = CursorLockMode.None;
        else
            Cursor.lockState = CursorLockMode.Confined;
        activeInventory = !activeInventory;
        Inventory.SetActive(activeInventory);
    }
    
    private void InventoryInitialize()
    {
        activeInventory = false;
        Inventory.SetActive(activeInventory);

        for(int i=0; i<4; i++)
        {
            slot[i] = GetInventorySlot(i);
            slot[i].SlotChild_SetActive(false);
        }
    }

    public Slot GetInventorySlot(int index)
    {
        return Inventory.transform.GetChild(index).gameObject.GetComponent<Slot>();
    }
    public void SetInventorySlot(Item item, int index, bool visible)
    {
        slot[index].SetSlotItem(item);
        slot[index].SlotChild_SetActive(visible);
    }
    //---------------------------------------------------------------------------------------------
    public void ShowRankPanelUI()
    {
        UpdateRankPanel();
        activeRankPanel = !activeRankPanel;
        RankPanel.SetActive(activeRankPanel);
    }

    private void InitializeRankPanel()
    {
        activeRankPanel = false;
        RankPanel.SetActive(activeInventory);
        int i;
        
        for(i=1; i<9; i++)
        {
            RankPanel.transform.GetChild(i).gameObject.SetActive(false);
        }
/*
        i=1;
        foreach(var record in PlayManager.instance.getRankingList())
        {
            rankSlot.Add(RankPanel.transform.GetChild(i).gameObject);
            RankPanel.transform.GetChild(i).gameObject.SetActive(true);
            i++;
        }*/
        //UpdateRankPanel();
    }

    public void UpdateRankPanel()
    {
        Debug.Log("update panel");
        int i=1;
        if(rankInit==false)
        {
            foreach(var record in PlayManager.instance.getRankingList())
            {
                rankSlot.Add(RankPanel.transform.GetChild(i).gameObject);
                RankPanel.transform.GetChild(i).gameObject.SetActive(true);
                i++;
            }
            rankInit=true;
        }

        List<ScoreRecord> list = PlayManager.instance.getRankingList();
        i=0;
        foreach(var rs in rankSlot)
        {
            var rsChild = rs.transform.GetChild(1);
            GamePlayerController gp = list[i].getPlayer();

            //RankPanel.transform.GetChild(i).gameObject.SetActive(true);
            //int score = 8-list[i].getScore();
            int rank = i+1;
            
            rs.transform.GetChild(2).gameObject.GetComponent<Text>().text = rank.ToString();
            rsChild.GetChild(0).gameObject.GetComponent<Text>().text = gp.nickname;//list[i].getNickname();
            rsChild.GetChild(1).gameObject.GetComponent<Text>().text = gp.kill.ToString();
            rsChild.GetChild(2).gameObject.GetComponent<Text>().text = gp.death.ToString();
            rsChild.GetChild(3).gameObject.GetComponent<Text>().text = gp.totalDeal.ToString();
            i++;
        }
    }

    //---------------------------------------------------------------------------------------------

    public void SetReload_Bar()     // 리로드 게이지를 총의 장전 속도에 맞게 늘림
    {
        reload_Bar.maxValue = gun.GetCurrentGun().reloadTime;
    }

    public void UpdateHP_Bar()
    {
        if (!player.isLocalPlayer)
            return;
        HP_Bar.value = player.health;
        img_Danger.color = new Color(1, 1, 1, 1 - (player.health / player.hpMax));
        if (player.health <= 30 && player.health > 0)
        {
            img_HP.color = Color.red;
        }
        else
        {
            img_HP.color = new Color(1, 1, (float)player.health / player.hpMax);
            audioSrc.gameObject.SetActive(false);
        }
    }

    public void UpdateKD()
    {
        text_Kill.text = "    K : " + player.kill;
        text_Death.text = "  D : " + player.death;
    }

    public void UpdateWeaponSlot()
    {
        if (!player.isLocalPlayer)
            return;
        if (player.hasWeapons)
        {
            img_Weapon.gameObject.SetActive(true);
            text_Bullet.gameObject.SetActive(true);
            img_Weapon.GetComponent<Image>().sprite = gun.GetCurrentGun().gameObject.GetComponent<SpriteRenderer>().sprite;
            text_Bullet.text = gun.GetCurrentGun().currentBulletCount + " / " + gun.GetCurrentGun().maxBulletCount;
        }
        else
        {
            img_Weapon.gameObject.SetActive(false);
            text_Bullet.gameObject.SetActive(false);
        }
    }

    private void AimMove()
    {
        AimPoint.transform.position = Input.mousePosition;
    }

    private void Check_CoolTime()
    {
        if (player.curCool <= 0)
        {
            img_Fill.transform.parent.gameObject.SetActive(false);
        }
        else
        {
            img_Fill.transform.parent.gameObject.SetActive(true);
        }

        img_Fill.fillAmount = (player.curCool/ player.dashCoolTime);
    }

    private void Check_Reload()
    {
        if(!player.isLocalPlayer) return;
        if (gun.GetisReload())
        {
            //Debug.Log(reload_Bar.transform.parent.parent.GetComponent<GamePlayerController>().GetPlayerId());
            reload_Bar.gameObject.SetActive(true);
            reload_Bar.value += Time.deltaTime;    
        }
        else
            reload_Bar.gameObject.SetActive(false);
    }

    public uint GetPlayerId()
    {
        return player.GetPlayerId();
    }

    private void OnDeath_Panel()
    {
        if (player.isDead)
        {
            respawnTimer -= Time.deltaTime;
            text_RespawnTimer.text = TimerToText();
            DeathPanel.SetActive(true);
            img_Danger.gameObject.SetActive(false);
        }
        else
        {
            DeathPanel.SetActive(false);
            img_Danger.gameObject.SetActive(true);
            text_RespawnTimer.text = "";
        }
    }
    private string TimerToText() //타이머 텍스트 생성 함수
    {
        return ((int) (respawnTimer % 60)+1).ToString();
    }

    private void ManageSound()
    {
        if (player.health <= 70 && player.health > 0)
        {
            audioSrc.gameObject.SetActive(true);
            audioSrc.volume = 1f - (player.health / 100f);
            audioSrc.pitch = 1.4f - (player.health / 100f);
        }
        else
            audioSrc.gameObject.SetActive(false);
    }
}
