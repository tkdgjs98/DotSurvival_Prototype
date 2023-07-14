using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Potion : Item
{
    public float recoveryHP;

    private void Awake()
    {
        this.type = Type.Potion;
    }

    private void Start()
    {
        this.SetID();
    }

    public override string Info()
    {
        return null;
    }
}