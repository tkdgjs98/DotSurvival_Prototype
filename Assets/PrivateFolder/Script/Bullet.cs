using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    private GamePlayerController owner;
    private Gun currentGun;
    private Animator anim;
    public uint ownerId;
    public bool isAlive;
    public bool isHit;
    public float range;
    public float damage;

    private void Awake()
    {
        InitializeBullet();
        anim = GetComponent<Animator>();
    }

    private void FixedUpdate()
    {
        if(range < 0)
        {
            StartCoroutine(ReturnBulletCoroutine());
        }
        if (isAlive && !isHit)
        {
            range -= 3*Time.deltaTime;
        }
    }

    public void InitializeBullet()
    {
        owner = null;
        ownerId = 0;
        isAlive = false;
        isHit = false;
        currentGun = null;
        range = 0;
        damage = 0;
    }
    public void SetupBullet(GamePlayerController _owner, Gun playerGun)
    {
        owner = _owner;
        ownerId = _owner.GetPlayerId();
        isAlive = true;
        currentGun = playerGun;
        range = currentGun.range;
        damage = currentGun.damage;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Item") || collision.CompareTag("Bullet"))
            return;
        if (collision.CompareTag("Player"))  // 총알이 플레이어와 충돌
        {
            GamePlayerController gpc = collision.gameObject.GetComponent<GamePlayerController>();
            if (ownerId == gpc.GetPlayerId()) // 총알 주인과 충돌한 것이면 return.
            {
                Debug.Log("My owner!");
                return;
            }
            gpc.TakeDamage(damage, owner);
            //BulletHit();
        }
        //Debug.Log("bullet hit wall!");
        BulletHit();
    }

    public void BulletHit()
    {
        gameObject.GetComponent<Rigidbody2D>().velocity = new Vector2(0, 0);
        isHit = true;
        anim.SetTrigger("Hit");
        //ReturnBullet();
        StartCoroutine(ReturnBulletCoroutine());
        Debug.Log("Hit");
    }

    IEnumerator ReturnBulletCoroutine()
    {
        float returnDelay;
        returnDelay = (isHit == true) ? 0.25f : 0;
        yield return new WaitForSeconds(returnDelay);
        owner.ReturnBullet(gameObject);
        anim.SetTrigger("Return");
        Debug.Log("routine");
        InitializeBullet();
    }
    public uint GetOwnerId()
    {
        return ownerId;
    }
}