using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Linq;

public class TestSpawner : NetworkBehaviour
{
    public static TestSpawner instance;

    public GameObject itemPositionSet;
    public List<Transform> itemPosition = new List<Transform>(); //아이템 스폰할 위치가 들어가는 리스트 itemPositionSet의 자식 오브젝트를 찾아 리스트에 입력받는다.

    public List<GameObject> weaponDB = new List<GameObject>();
    public List<GameObject> accessoryDB = new List<GameObject>();
    public List<GameObject> potionDB = new List<GameObject>();

    public struct ItemDB
    {
        public List<GameObject> itemDB;
        public List<GameObject> S;
        public List<GameObject> A;
        public List<GameObject> B;
        public List<GameObject> C;
        public List<GameObject> D;
        public ItemDB(List<GameObject> _itemDB)
        {
            this.itemDB = _itemDB;
            S = new List<GameObject>();
            A = new List<GameObject>();
            B = new List<GameObject>();
            C = new List<GameObject>();
            D = new List<GameObject>();
        }

        public List<GameObject> Grade(int grade)
        {
            switch(grade)
            {
                case 0:
                    return S;
                case 1:
                    return A;
                case 2:
                    return B;
                case 3:
                    return C;
                case 4:
                    return D;
            }
            return D;
        }
    }
    public ItemDB weaponGradeDB;
    public ItemDB accessoryGradeDB;
    public ItemDB potionGradeDB;

    [Space(20)]
    private Vector4 spawnRange;

    public List<SpawnedItem> spawnedItem = new List<SpawnedItem>();


    private void Awake()
    {
        instance = this;
        InitializePosition();
        InitializeItemGrade();
    }

    public override void OnStartServer()
    {
        spawnRange = new Vector4(-5,5,-5,5);
        //DBlength = itemDB.Count;

        //Spawn();
        foreach(var area in itemPosition.Select((value, index)=>new{value, index}))
        {
            var _index = area.index;
            SpawnWeapon(2, area.value, _index);
            SpawnAccessory(2, area.value, _index);
            SpawnPotion(2, area.value, _index);
        }
    }

    public void InitializePosition()
    {
        foreach(Transform area in itemPositionSet.transform)
        {
            itemPosition.Add(area);
        }
    }

    public void InitializeItemGrade()
    {
        weaponGradeDB = new ItemDB(weaponDB);
        accessoryGradeDB = new ItemDB(accessoryDB);
        potionGradeDB = new ItemDB(potionDB);
        foreach(GameObject item in weaponDB)
        {
            switch(item.GetComponent<Item>().grade)
            {
                case (Item.Grade)0:
                    weaponGradeDB.S.Add(item);
                    break;
                case (Item.Grade)1:
                    weaponGradeDB.A.Add(item);
                    break;
                case (Item.Grade)2:
                    weaponGradeDB.B.Add(item);
                    break;
                case (Item.Grade)3:
                    weaponGradeDB.C.Add(item);
                    break;
                case (Item.Grade)4:
                    weaponGradeDB.D.Add(item);
                    break;
            }
        }
        foreach(GameObject item in accessoryDB)
        {
            switch(item.GetComponent<Item>().grade)
            {
                case (Item.Grade)0:
                    accessoryGradeDB.S.Add(item);
                    break;
                case (Item.Grade)1:
                    accessoryGradeDB.A.Add(item);
                    break;
                case (Item.Grade)2:
                    accessoryGradeDB.B.Add(item);
                    break;
                case (Item.Grade)3:
                    accessoryGradeDB.C.Add(item);
                    break;
                case (Item.Grade)4:
                    accessoryGradeDB.D.Add(item);
                    break;
            }
        }
        foreach(GameObject item in potionDB)
        {
            switch(item.GetComponent<Item>().grade)
            {
                case (Item.Grade)0:
                    potionGradeDB.S.Add(item);
                    break;
                case (Item.Grade)1:
                    potionGradeDB.A.Add(item);
                    break;
                case (Item.Grade)2:
                    potionGradeDB.B.Add(item);
                    break;
                case (Item.Grade)3:
                    potionGradeDB.C.Add(item);
                    break;
                case (Item.Grade)4:
                    potionGradeDB.D.Add(item);
                    break;
            }
        }
        
    }

    public int SpawnProbability()
    { 
        int rand = Random.Range(0,100);
        if(rand<40)
        {
            return 4;
        }
        else if(rand<68)
        {
            return 3;
        }
        else if(rand<86)
        {
            return 2;
        }
        else if(rand<96)
        {
            return 1;
        }
        else
        {
            return 0;
        }
    }


    public void SpawnWeapon(int count, Transform area, int areaIndex)
    {
        int rand;

        for(int i=0;i<count;i++)
        {
            var spawnPosition = new Vector2(
                area.position.x + Random.Range(-3f,3f),
                area.position.y + Random.Range(-3f,3f)
            );
            if(spawnPosition.x < -19) spawnPosition.x = -19;

            var DB = weaponGradeDB.Grade(SpawnProbability());
            rand = Random.Range(0,DB.Count);

            GameObject go = Instantiate(DB[rand], spawnPosition, Quaternion.identity);
            go.name = DB[rand].name;
            //go.GetComponent<Item>().spawnedArea = area;
            NetworkServer.Spawn(go);
            SpawnedItem si = new SpawnedItem(go.GetComponent<NetworkIdentity>().netId,areaIndex);
            spawnedItem.Add(si);
        }
    }   

    public void SpawnAccessory(int count, Transform area, int areaIndex)
    {
        int rand;
        
        for(int i=0;i<count;i++)
        {
            var spawnPosition = new Vector2(
                area.position.x + Random.Range(-3f,3f),
                area.position.y + Random.Range(-3f,3f)
            );
            if(spawnPosition.x < -19) spawnPosition.x = -19;

            var DB = accessoryGradeDB.Grade(SpawnProbability());
            rand = Random.Range(0,DB.Count);

            GameObject go = Instantiate(DB[rand], spawnPosition, Quaternion.identity);
            go.name = DB[rand].name;
            //go.GetComponent<Item>().spawnedArea = area;
            NetworkServer.Spawn(go);
            SpawnedItem si = new SpawnedItem(go.GetComponent<NetworkIdentity>().netId,areaIndex);
            spawnedItem.Add(si);
        }
    }

    public void SpawnPotion(int count, Transform area, int areaIndex)
    {
        int rand;
        
        for(int i=0;i<count;i++)
        {
            var spawnPosition = new Vector2(
                area.position.x + Random.Range(-3f,3f),
                area.position.y + Random.Range(-3f,3f)
            );
            if(spawnPosition.x < -19) spawnPosition.x = -19;
            rand = Random.Range(0,potionDB.Count);

            GameObject go = Instantiate(potionDB[rand], spawnPosition, Quaternion.identity);
            go.name = potionDB[rand].name;
            //go.GetComponent<Item>().spawnedArea = area;
            NetworkServer.Spawn(go);
            SpawnedItem si = new SpawnedItem(go.GetComponent<NetworkIdentity>().netId,areaIndex);
            spawnedItem.Add(si);
        }
    }

    public Transform getSpawnedArea(uint _itemId)
    {
        int index = 0;
        foreach(var item in spawnedItem)
        {
            if(_itemId!=item.getItemId())
            {
                continue;
            }
            else
            {
                index = item.getAreaIndex();
                break;
            }
        }
        return itemPosition[index];
    }
    
    public struct SpawnedItem 
    { 
        uint itemId;
        int  areaIndex;

        public SpawnedItem(uint _itemId, int _areaIndex) 
        { 
            this.itemId = _itemId; 
            this.areaIndex = _areaIndex;
        }

        public uint getItemId()
        {
            return this.itemId;
        }
        public int getAreaIndex()
        {
            return this.areaIndex;
        }

    } 
}
