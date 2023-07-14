using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using UnityEngine.SceneManagement;

public class GamePlayerController : NetworkBehaviour {

    private GunController gc;
    private UIManager UI;
    public UIManager getUI()
    {
        return UI;
    }
    private Transform tr;
    [SerializeField]
    private Transform firePos;
    private BoxCollider2D col;
    private Animator animator;
    private Camera cam;
    private AudioSource audioSource;

    private float hAxis;
    private float vAxis;
    private Vector3 mousePos;
    private Vector3 dashDirection;
    private Vector3 target;
    private Vector3 moveVec;

    private GameObject nearObject;
    private GameObject weaponPoint;
    private GameObject playerGun;
    [SerializeField]
    private GameObject bulletPrefab;
    [SerializeField]
    private Item[] itemBuffer = new Item[4];
    private bool isDash = false;
    private float timeSpan = 0f;
    private float speed = 4f;
    private float defence = 100f;
    [SyncVar]
    public float totalDeal;
    private readonly float checkTime = 0.25f;
    private readonly float dashSpeed = 15f;

    [SyncVar]
    public bool isDead = false;
    [SyncVar]                           //수정-------------------------------------------------------------------------
    public bool pause;                  
    public bool hasWeapons;
    [SyncVar(hook = "OnChangeHealth")]
    public float health = 100f;
    public float curCool = 0f;
    public float dashCoolTime = 3f;
    [SyncVar]
    public float respawnTime = 10f;
    public float hpMax = 100f;
    [SyncVar(hook = "OnChangeKill")]
    public int kill = 0;
    public int death = 0;
    public int dashStack = 1;
    public int maxDashStack = 1;

    [SyncVar(hook = "OnChangeNickname")]
    public string nickname;
    public Text text_nickname;
    [Command]
    public void CmdSetNickname(string nick)
    {
        nickname = nick;
    }
    public void OnChangeNickname(string oldNick, string newNick)
    {
        text_nickname.text =  nickname;
    }

    public AudioClip audioEquipWeapon;
    public AudioClip audioEquipAccessory;
    public AudioClip audioDrink;
    public AudioClip audioDash;
    public AudioClip audioHit;
    public AudioClip audioInventory;
    public AudioClip audioDie;
    public AudioClip audioRespawn;


    private void Awake() {
        weaponPoint = this.transform.GetChild(0).gameObject;
        playerGun = weaponPoint.transform.GetChild(0).gameObject;
        gc = GetComponent<GunController>();
        tr = GetComponent<Transform>();
        animator = GetComponent<Animator>();
        col = GetComponent<BoxCollider2D>();
        UI = gameObject.GetComponent<UIManager>();
        audioSource = gameObject.GetComponent<AudioSource>();
        respawnTime = 5f;//리스폰 시간 초기화
        totalDeal = 0;
        for(int i = 1; i<4; i++)
        {
            itemBuffer[i] = null;
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        if(hasAuthority)
        {
            cam = Camera.main;
            cam.GetComponent<AudioListener>().enabled = false;
            cam.transform.SetParent(transform);
            cam.transform.localPosition = new Vector3(0f, 0f, -7f);
            cam.orthographicSize = 2.5f;
            gameObject.AddComponent<AudioListener>();
        }
        if(isLocalPlayer)
        {
            CmdSetNickname(PlayerData.nickname);
            
        }
        PlayManager.instance.AddPlayer(this);
        if(isServer)RpcPauseCharacter(true);
    }
    

    // Update is called once per frame
    void Update()
    {
        if(!isLocalPlayer)
        {
            return;
        }

        /* ------- (Input Section) ------- */
        GetInput();

        if(Input.GetKeyDown(KeyCode.E))
        {
            UI.ShowInventoryUI();
            PlaySound("Inventory");
        }
        if(Input.GetKeyDown(KeyCode.Tab))
        {
            UI.ShowRankPanelUI();
            CmdRankLog();
        }

        if(isDead) return;

        if (Input.GetMouseButtonDown(1) && isDash == false && dashStack > 0)
        {
            isDash = true;
            dashStack--;
            PlaySound("Dash");
            if (dashStack < maxDashStack)
            {
                StartCoroutine(CoolTime(dashCoolTime));
            }
        }
        switch(gc.GetCurrentGun().gunType)
        {
            case (Gun.GunType)1:
            case (Gun.GunType)2:
            case (Gun.GunType)3:
                if (Input.GetMouseButtonDown(0) && hasWeapons)
                    CmdFire(firePos.position, firePos.rotation);
                break;
            case (Gun.GunType)4:
            case (Gun.GunType)5:
                if (Input.GetMouseButton(0) && hasWeapons)
                    CmdFire(firePos.position, firePos.rotation);
                break;
        }
        if (Input.GetKeyDown(KeyCode.R) && hasWeapons)
        {
            CmdTryReload();
        }
        if (Input.GetKeyDown(KeyCode.F))
        {
            CmdInteraction();
        }
        if(Input.GetKeyDown(KeyCode.Alpha1))
        {
            if(itemBuffer[0] == null) return;
            CmdRemoveWeapon();
        }
        if(Input.GetKeyDown(KeyCode.Alpha2))
        {
            if(itemBuffer[1] == null) return;
            CmdRemoveAccessory(1);
        }
        if(Input.GetKeyDown(KeyCode.Alpha3))
        {
            if(itemBuffer[2] == null) return;
            CmdRemoveAccessory(2);
        }
        if(Input.GetKeyDown(KeyCode.Alpha4))
        {
            if(itemBuffer[3] == null) return;
            CmdRemoveAccessory(3);
        }



/* ----- Debug Section ----- */
        /*
        if(Input.GetKeyDown(KeyCode.G))
        {
            //CmdIdLog(this.netId);
            Debug.Log(respawnTime);
            CmdScoreLog();
        }
        if(Input.GetKeyDown(KeyCode.H))
        {
            CmdTakeDamage(20, gameObject.GetComponent<GamePlayerController>());
        }
        */
    }
    
    [Command]
    public void CmdRankLog()
    {
        string log = PlayManager.instance.RankLog();
        RpcRankLog(log);
    }
    [ClientRpc]
    public void RpcRankLog(string log)
    {
        Debug.Log(log);
    }
    [Command]
    public void CmdTakeDamage(float _damage, GamePlayerController _player)
    {
        TakeDamage(_damage, _player);
    }

    [Command]
    public void CmdScoreLog(uint _Id, float _health, int _kill, int _death)
    {
        RpcScoreLog(_Id, _health, _kill, _death);
    }
    [ClientRpc]
    public void RpcScoreLog(uint _Id, float _health, int _kill, int _death)
    {
        Debug.Log("Id: " + _Id + " HP: " + _health + " Kill: " + _kill + " Death: " + _death);
    }
    [Command]
    public void CmdScoreLog()
    {
        PlayManager.instance.ScoreLog();
    }

    void FixedUpdate()
    {
        if (tr.localScale.x<=0)
            {
                text_nickname.transform.localScale = new Vector3(-0.013f, 0.013f, -1f);
            }
        else
            {
                text_nickname.transform.localScale = new Vector3(0.013f, 0.013f, 1f);
            }
        if(!isLocalPlayer)
        {
            return;
        }
        animator.SetBool("move", false);
        mousePos = cam.ScreenToWorldPoint(Input.mousePosition + new Vector3(0, 0, 15f));

        if(isDead||pause) return; //조건문 최적화
        PlayerMove();
        PlayerDash();
        AimSight();
        if (weaponPoint.transform.childCount != 0)
        {
            weaponPoint.transform.GetChild(0).gameObject.transform.localPosition = new Vector3(0.15f, 0f, 0f);
            AimWeaponPoint();
        }
    }

    private void GetInput()
    {
        hAxis = Input.GetAxisRaw("Horizontal");
        vAxis = Input.GetAxisRaw("Vertical");
    }
    
    private void PlayerMove()
    {
        if(/*!isDead && */!isDash)
        {
            moveVec = new Vector3(hAxis, vAxis, 0).normalized;

            tr.position += speed * Time.deltaTime * moveVec;
            animator.SetBool("move", moveVec != Vector3.zero);
        }
    }
    private void AimSight()
    {
//        if(!isDead)
//        {
            if (Input.mousePosition.x <= Screen.width / 2)
                tr.localScale = new Vector3(-1.5f, 1.5f, 1.5f);
            else
                tr.localScale = new Vector3(1.5f, 1.5f, 1.5f);
//        }
    }

    private void AimWeaponPoint()
    {
        Vector3 mPosition = Input.mousePosition;
        Vector3 oPosition = weaponPoint.transform.position;
        mPosition.z = oPosition.z - Camera.main.transform.position.z;
        target = Camera.main.ScreenToWorldPoint(mPosition);
        float angle = Mathf.Atan2(target.y - oPosition.y, target.x - oPosition.x) * Mathf.Rad2Deg;

        weaponPoint.transform.rotation = Quaternion.Euler(0f, 0f, angle);
        if (Input.mousePosition.x <= Screen.width / 2)
            {
                weaponPoint.transform.localScale = new Vector3(-1f, -1f, 1f);
                text_nickname.transform.localScale = new Vector3(-0.013f, 0.013f, -1f);
            }
        else
            {
                weaponPoint.transform.localScale = new Vector3(1f, 1f, 1f);
                text_nickname.transform.localScale = new Vector3(0.013f, 0.013f, 1f);
            }

    }

    private void PlayerDash()
    {
        if(isDash == true)
        {
            timeSpan += Time.deltaTime;
            if(timeSpan < checkTime)
            {
                dashDirection = new Vector3(mousePos.x - tr.position.x, mousePos.y - tr.position.y, 0).normalized;
                tr.position += dashSpeed * Time.deltaTime * dashDirection;
            }
            else if (timeSpan > checkTime)
            {
                timeSpan = 0;
                isDash = false;
            }
        }
    }

    IEnumerator CoolTime(float cool)
    {
        while(cool > 0.0f)
        {
            cool -= Time.deltaTime;
            curCool = cool;
            if (cool < 0.0f)
            {
                dashStack++;
            }       
            yield return new WaitForFixedUpdate();
        }
    }

    [Command]
    private void CmdInteraction()
    {
        if(nearObject != null)
        {
            if(nearObject.CompareTag("Item"))
            {
                GameObject _itemGO = nearObject;
                Item item = nearObject.GetComponent<Item>();

                switch (item.type)
                {
                    case (Item.Type)1:
                        if(hasWeapons == false)
                        {
                            GetWeapon(_itemGO);
                        }
                        else
                        {
                            RemoveWeapon();
                            GetWeapon(_itemGO);
                        }
                        break;
                    case (Item.Type)2:
                        int accessoryPoint = GetAccessoryPoint();
                        if(!IsinInventory(item)&&accessoryPoint<=3)
                        {
                            GetAccessory(_itemGO, accessoryPoint);
                        }
                        else
                        {
                            if(IsinInventory(item)) Debug.Log("already has it");
                            if(accessoryPoint>3) Debug.Log("full accessory");
                        }
                        break;
                    case (Item.Type)3:
                        DrinkPotion(_itemGO);
                        break;
                }
            }
            nearObject = null;
        }
    }
    
    [ClientRpc]
    public void GetWeapon(GameObject itemGO)
    {
        Item item= itemGO.GetComponent<Item>();
        item.isUsed = true;
        hasWeapons = true;
        itemBuffer[0] = item;
        playerGun.GetComponent<SpriteRenderer>().sprite= item.GetComponent<SpriteRenderer>().sprite;
        playerGun.GetComponent<Gun>().SetupGun(itemGO.GetComponent<Gun>());
        UI.SetInventorySlot(itemBuffer[0], 0, true);
        UI.SetReload_Bar();
        UI.UpdateWeaponSlot();
        itemBuffer[0].gameObject.SetActive(false);
        RefreshAbillity();     // 캐릭터 스펙 갱신
        PlaySound("EquipWeapon");
    }
    [ClientRpc]
    public void RemoveWeapon()
    {
        hasWeapons = false;
        playerGun.GetComponent<SpriteRenderer>().sprite = null;
        playerGun.GetComponent<Gun>().InitializeGun();
        UI.slot[0].OnClickRemoveKey();
        UI.UpdateWeaponSlot();
        itemBuffer[0].transform.localScale = new Vector3(4f, 4f, 4f);
        itemBuffer[0].transform.position = tr.position;
        itemBuffer[0].gameObject.SetActive(true);
        itemBuffer[0] = null;
        RefreshAbillity();     // 캐릭터 스펙 갱신
    }
    [Command]
    public void CmdRemoveWeapon()
    {
        RemoveWeapon();
    }
    [ClientRpc]
    public void GetAccessory(GameObject itemGO, int accessoryPoint)
    {
        Item item = itemGO.GetComponent<Item>();
        item.isUsed = true;
        itemBuffer[accessoryPoint] = item;
        UI.SetInventorySlot(itemBuffer[accessoryPoint], accessoryPoint, true);
        itemBuffer[accessoryPoint].gameObject.SetActive(false);
        RefreshAbillity();     // 캐릭터 스펙 갱신
        PlaySound("EquipAccessory");
    }
    [ClientRpc]
    public void RemoveAccessory(int accessoryPoint)
    {
        UI.slot[accessoryPoint].OnClickRemoveKey();
        itemBuffer[accessoryPoint].transform.localScale = new Vector3(4f, 4f, 4f);
        itemBuffer[accessoryPoint].transform.position = tr.position;
        itemBuffer[accessoryPoint].gameObject.SetActive(true);
        itemBuffer[accessoryPoint] = null;
        RefreshAbillity();
    }
    [Command]
    public void CmdRemoveAccessory(int accessoryPoint)
    {
        RemoveAccessory(accessoryPoint);
        //RefreshAbillity(); // 캐릭터 스펙 갱신
    }

    [ClientRpc]
    private void DrinkPotion(GameObject itemGO)
    {
        if (health == hpMax)
            return;
        Potion p = itemGO.GetComponent<Potion>();
        health += p.recoveryHP;
        if (health > hpMax)
            health = hpMax;
        UI.UpdateHP_Bar();
        PlaySound("Drink");
        p.gameObject.SetActive(false);
    }

    private bool IsinInventory(Item searchItem)
    {
        for(int i=1; i<4; i++)
        {
            if(itemBuffer[i] == null){ i++; continue; }
            if(itemBuffer[i].itemID == searchItem.itemID) return true;
        }
        return false;
    }
    private int GetAccessoryPoint()
    {
        for(int i=1; i<4; i++)
        {
            //if(UI.GetInventorySlot(i).GetSlotItemID() == null) return i;
            if(itemBuffer[i] == null) return i;
        }
        return 4;
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Item")){
            nearObject = other.gameObject;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Item"))
            nearObject = null;
    }

    public int respawnTimer;
    private void StartRespawn() //OnChangeHealth() 함수에서 호출
    {
        if(isServer)StartCoroutine(RespawnCoroutine());
    }
    private IEnumerator RespawnCoroutine()
    {
        RpcRespawnTimer();
        isDead = true;
        yield return new WaitForSeconds(respawnTime);
        health = hpMax;
        animator.SetBool("die", false);
        col.enabled = true;
        weaponPoint.SetActive(true);
        RpcRevive();
        isDead = false;
    }
    [ClientRpc]
    private void RpcRevive()
    {
        if(isServer)return;
        animator.SetBool("die", false);
        col.enabled = true;
        weaponPoint.SetActive(true);
        PlaySound("Respawn");
    }
    [ClientRpc]
    private void RpcRespawnTimer()
    {
        if(!isLocalPlayer)return;
        UI.respawnTimer = respawnTime;
    }
    // Bullet 스크립트에서 플레이어와 충돌처리를 할 수 있어서 여기서는 OnTriggerEnter2D가 필요 없음.
    // 대신 Bullet 스크립트에서 TakeDamage() 함수 호출하는 것으로 변경.
    public void TakeDamage(float _damage, GamePlayerController enemy)
    {
        if (!isServer) return;
        Debug.Log(this.netId + " is damaged " + _damage + " by " + enemy.GetPlayerId());

        float deal = _damage * (defence / 100);
        health -= deal;
        enemy.totalDeal += deal;

        PlaySound("Hit");
        if (health <= 0)
        {
            PlayManager.instance.UpdateRecordKD(this.netId, false);
            enemy.TakeKill();
        }
    }

    // OnChangeHealth()함수는 health를 SyncVar로 설정하여 변수 동기화 후 저절로 수행되는 함수. (40, 41 line 참고.)
    // UI UPdateHP_Bar() 함수를 호출. 더이상 Update() 함수 내에서 항시 수행 x.
    private void OnChangeHealth(float oldHealth, float newHealth)
    {
        //if(!isLocalPlayer) return;
        Debug.Log("Hooked");
        UI.UpdateHP_Bar();
        if (health <= 0)
        {
            TakeDeath();
            StartRespawn();
        }
    }

    private void OnChangeKill(int oldKIll, int newKill)
    {
        UI.UpdateKD();
    }

    private void TakeKill()    //아직 안됨. netId만으로 탐색하는 기능이 아직 없음
    {
        this.kill++;
        PlayManager.instance.UpdateRecordKD(this.netId, true);
        Debug.Log(this.netId + "kill");
    }

    private void TakeDeath()
    {
        health = 0;
        death++;
        animator.SetBool("die", true);
        isDead = true;
        Debug.Log(nickname+" is dead: "+isDead);
        weaponPoint.SetActive(false);
        UI.UpdateKD();
        col.enabled = false;
        PlaySound("Die");
    }

    public GameObject myBullet;
    [Command]
    private void CmdFire(Vector2 _position, Quaternion _rotation)
    {
        if(gc.GetisReload())return;
        int fire = gc.Fire();
        if(fire==-2)return;
        else if(fire==-1)
        {
            if(!gc.TryReload(true)) return;
            gc.ServerReload();
            RpcTryReload();
        }
        else if(fire>=0)
        {/*
            myBullet = Instantiate(bulletPrefab, firePos.position, firePos.rotation);
            myBullet.GetComponent<Bullet>().SetupBullet(gameObject.GetComponent<GamePlayerController>(), playerGun.GetComponent<Gun>()); ;
            //Debug.Log("myBullet: " + myBullet.GetComponent<Bullet>().GetOwnerId());
            NetworkServer.Spawn(myBullet);
            */

            myBullet = BulletPool.GetBullet(playerGun.GetComponent<Gun>());
            myBullet.transform.position = _position;
            myBullet.transform.rotation = _rotation;
            myBullet.GetComponent<Bullet>().SetupBullet(gameObject.GetComponent<GamePlayerController>(), playerGun.GetComponent<Gun>()); ;
            myBullet.SetActive(true);
            myBullet.GetComponent<Rigidbody2D>().velocity = myBullet.transform.right*20.0f;
            PlaySound("Fire");
            RpcFire(myBullet, _position, _rotation, fire);
        }
    }
    [ClientRpc]
    private void RpcFire(GameObject _myBullet, Vector2 _position, Quaternion _rotation, int _bulletCount)
    {
        if(isServer)return;
        Debug.Log("rpcfire");
        myBullet = _myBullet;
        myBullet.transform.position = _position;
        myBullet.transform.rotation = _rotation;
        myBullet.GetComponent<Bullet>().SetupBullet(gameObject.GetComponent<GamePlayerController>(), playerGun.GetComponent<Gun>());
        myBullet.SetActive(true);
        myBullet.GetComponent<Rigidbody2D>().velocity = myBullet.transform.right*20.0f;
        gc.SetCurrentBulletCount(_bulletCount);
        UI.UpdateWeaponSlot();
        PlaySound("Fire");
    }

    public void ReturnBullet(GameObject _bullet)
    {
        if(!isServer)return;
        _bullet.SetActive(false);
        RpcReturnBullet(_bullet);
        BulletPool.ReturnBullet(_bullet);
    }
    [ClientRpc]
    public void RpcReturnBullet(GameObject _bullet)
    {
        if(isServer)return;
        _bullet.SetActive(false);
    }

    [Command]
    public void CmdTryReload()
    {
        if(!gc.TryReload(true)) return;
        gc.ServerReload();
        RpcTryReload();
    }
    [ClientRpc]
    public void RpcTryReload()
    {
        if(!isLocalPlayer) return;
        gc.ClientReload();
    }


    [Command]
    public void CmdIdLog(uint clientId) // G키를 누르면 서버에서 클라이언트의 netId를 출력함.
    {
        //Debug.Log(clientId);
        RpcIdLog(clientId);
    }
    [ClientRpc]
    public void RpcIdLog(uint clientId)
    {
        //if(isServer) return;
        Debug.Log(clientId);
    }
    public uint GetPlayerId()
    {
        return this.netId;
    }

    private void PlaySound(string action)   // 효과음 출력 함수
    {
        switch(action)
        {
            case "EquipWeapon":
                audioSource.clip = audioEquipWeapon;
                break;
            case "EquipAccessory":
                audioSource.clip = audioEquipAccessory;
                break;
            case "Drink":
                audioSource.clip = audioDrink;
                break;
            case "Dash":
                audioSource.clip = audioDash;
                break;
            case "Hit":
                audioSource.clip = audioHit;
                break;
            case "Inventory":
                audioSource.clip = audioInventory;
                break;
            case "Die":
                audioSource.clip = audioDie;
                break;
            case "Fire":
                audioSource.clip = gc.GetCurrentGun().audioFire;
                break;
            case "Respawn":
                audioSource.clip = audioRespawn;
                break;
        }
        audioSource.PlayOneShot(audioSource.clip);
    }

    private void RefreshAbillity() // 아이템 습득이 감지되면 아이템 버퍼를 읽고 캐릭터 스펙에 반영
    {
        defence = 100f;
        hpMax = 100f;
        speed = 4f;
        speed += gc.GetCurrentGun().addSpeed;
        for(int i = 1; i < 4; i++)
        {
            Accessory acc = (Accessory)itemBuffer[i];
            if (acc == null)
                continue;
            defence -= acc.defence;
            hpMax += acc.addMaxHP;
            speed += acc.addSpeed;
        }
    }
    [ClientRpc]
    public void RpcPauseCharacter(bool _pause)
    {
        col.enabled = !_pause;
        this.pause = _pause;
        //weaponPoint.SetActive(false);
    }
}
