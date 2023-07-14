using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class GunController : MonoBehaviour
{
    private GamePlayerController player;
    [SerializeField]
    private Gun currentGun;
    private AudioSource audioSource;

    private UIManager ui;
    private bool isReload = false;
    private float currentFireRate = 0;
    public AudioClip audioReload1;
    public AudioClip audioReload2;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        player = gameObject.GetComponent<GamePlayerController>();
        ui = GetComponent<UIManager>();
    }

    void Update()
    {
        GunFireRateCalc();
    }

    private void GunFireRateCalc()
    {
        if (currentFireRate > 0)
            currentFireRate -= Time.deltaTime;
    }

public int Fire()
    {
        if(currentFireRate <= 0)
        {
            if (currentGun.currentBulletCount > 0)
            {
                Shoot();
                return currentGun.currentBulletCount;
            }
            else
            {
                return -1;
            }
        }
        else
        {
            return -2;
        }
    }
    private void Shoot()
    {
        currentGun.currentBulletCount--;
        currentFireRate = currentGun.fireRate;
        ui.UpdateWeaponSlot();
    }

    public bool TryReload(bool isCalledByKeyCode_R)
    {
        if(isCalledByKeyCode_R)
        {
            if(isReload) return false;
        
            if(!(currentGun.currentBulletCount < currentGun.maxBulletCount)) return false;
            return true;
            
        }
        else
        {
            return true;
        }
    }
    public void ServerReload()
    {
        StartCoroutine(ReloadCoroutine());
    }
    public void ClientReload()
    {
        if(!player.isLocalPlayer) return;
        StartCoroutine(ReloadCoroutine());
    }

    IEnumerator ReloadCoroutine()
    {
        PlaySound("Reload1");
        isReload = true;
        yield return new WaitForSeconds(currentGun.reloadTime);
        currentGun.currentBulletCount = currentGun.maxBulletCount;
        ui.reload_Bar.value = 0;
        ui.UpdateWeaponSlot();
        currentFireRate = 0;
        isReload = false;
        PlaySound("Reload2");
    }

    public bool GetisReload()
    {
        return isReload;
    }
    public Gun GetCurrentGun()
    {
        return currentGun;
    }

    public void SetCurrentGun(Gun gun)
    {
        currentGun = gun;
    }

    private void PlaySound(string action)   // 효과음 출력 함수
    {
        switch (action)
        {
            case "Reload1":
                audioSource.clip = audioReload1;
                break;
            case "Reload2":
                audioSource.clip = audioReload2;
                break;
        }
        audioSource.PlayOneShot(audioSource.clip);
    }

    public uint GetPlayerId()
    {
        return player.GetPlayerId();
    }

    public void SetCurrentBulletCount(int _bulletCount)
    {
        currentGun.currentBulletCount = _bulletCount;
    }
}