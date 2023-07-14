using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : Item
{
    public enum GunType { HG = 1, RF = 2, SG = 3, AK = 4, GG = 5 };
    public GunType gunType;
    public float fireRate = 0;          
    public float reloadTime = 0;
    public float range = 0;
    public float damage = 0;
    public float addSpeed = 0;            
    public int currentBulletCount = 0;  
    public int maxBulletCount = 0;
    public Bullet bullet;
    public AudioClip audioFire;

    private void Awake()
    {
        this.type = Type.Weapon;
    }

    private void Start() {
        this.SetID();
    }

    public void InitializeGun()
    {
        this.fireRate = 0;          
        this.reloadTime = 0;
        this.range = 0;
        this.addSpeed = 0;        
        this.damage = 0;          
        this.currentBulletCount = 0;
        this.maxBulletCount = 0;
    }

    public void SetupGun(Gun gun)
    {
        this.fireRate = gun.fireRate;          
        this.reloadTime = gun.reloadTime;
        this.range = gun.range;
        this.addSpeed = gun.addSpeed;        
        this.damage = gun.damage;          
        this.currentBulletCount = gun.currentBulletCount;
        this.maxBulletCount = gun.maxBulletCount;
        this.audioFire = gun.audioFire;
        this.gunType = gun.gunType;
    }
    public override string Info()
    {
        return  itemID+"\n\n"+
                nameof(grade)+info(ref grade)+
                nameof(damage)+info(ref damage) +
                nameof(range)+info(ref range) +
                nameof(fireRate)+info(ref fireRate)
                ;
    }
}
