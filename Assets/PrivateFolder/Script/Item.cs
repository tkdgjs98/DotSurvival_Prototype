using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public abstract class Item : MonoBehaviour 
{
    public string itemID;
    public enum Type { Weapon = 1, Accessory = 2, Potion = 3 };
    public Type type;
    public enum Grade { S = 0, A = 1, B = 2, C = 3, D = 4 };
    public Grade grade;
    public bool isUsed;
    public Transform spawnedArea;

    protected void SetID()
    {
        this.itemID = this.name;
    }
    public abstract string Info();
    protected string info<T>(ref T val)
    {
        return ": "+val+"\n";
    }

    private void OnTriggerEnter2D(Collider2D collision) //아이템이 생성될 때 벽이나 오브젝트와 충돌할 시 위치 조정
    {
        if(isUsed)return;
        if(collision.CompareTag("Item") || collision.CompareTag("Bullet") || collision.CompareTag("Player"))return;
        else if(collision.CompareTag("Wall"))
        {
            Debug.Log("wall");
            transform.position = getAreaIndex().position; //(getAreaIndex().position - transform.position)/2;
            Debug.Log(getAreaIndex().name);
        }
        else if(IsObject(collision.gameObject)) 
        {
            Debug.Log("object");
            //transform.position += transform.position - collision.gameObject.transform.position;
            transform.position = getAreaIndex().position;
            Debug.Log(getAreaIndex().name);
        }
    }

    public bool IsObject(GameObject obj)
    {
        if(obj.transform.parent.parent.gameObject.CompareTag("Object"))return true;
        else return false;
    }
    public Transform getAreaIndex()
    {
        return TestSpawner.instance.getSpawnedArea(gameObject.GetComponent<NetworkIdentity>().netId);
    }
}
