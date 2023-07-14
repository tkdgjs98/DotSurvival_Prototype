using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class BulletPool : NetworkBehaviour
{
    public static BulletPool instance;
    public GameObject BulletPrefab;
    [SerializeField]
    private Queue<GameObject> poolingBulletQueue;

    public int startSize = 15;

    private void Awake()
    {
       instance = this;
    }

    public override void OnStartServer()
    {
        Initialize();
    }

    private void Initialize()
    {
        poolingBulletQueue = new Queue<GameObject>();

        for(int i=0; i<startSize; i++)
        {
            poolingBulletQueue.Enqueue(CreateNewBullet());
        }
    }

    private GameObject CreateNewBullet()
    {
        GameObject newBullet = Instantiate(BulletPrefab,new Vector2(100,100),transform.rotation);
        newBullet.transform.SetParent(transform);
        NetworkServer.Spawn(newBullet);
        return newBullet;
    }

    public static GameObject GetBullet(Gun playerGun)
    {
        if(instance.poolingBulletQueue.Count > 0)
        {
            var obj = instance.poolingBulletQueue.Dequeue();
            obj.transform.SetParent(null);
            return obj;
        }
        else
        {
            var newObj = instance.CreateNewBullet();
            newObj.transform.SetParent(null);
            return newObj;
        }
    }

    public static void ReturnBullet(GameObject obj)
    {
        Debug.Log("ReturnBullet");
        obj.transform.SetParent(instance.transform);
        instance.poolingBulletQueue.Enqueue(obj);
    }
}
