using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Accessory : Item
{
    public float defence;
    public float addMaxHP;
    public float addSpeed;
    private void Awake()
    {
        this.type = Type.Accessory;
    }

    private void Start() {
        this.SetID();
    }
    public override string Info()
    {
        return  itemID+"\n\n"+
                nameof(grade)+info(ref grade)+
                nameof(defence)+info(ref defence) +
                nameof(addMaxHP)+info(ref addMaxHP) +
                nameof(addSpeed)+info(ref addSpeed)
                ;
    }
}
