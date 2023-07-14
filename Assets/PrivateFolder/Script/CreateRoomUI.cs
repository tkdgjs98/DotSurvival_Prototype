using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class CreateRoomUI : MonoBehaviour
{
    [SerializeField]
    private InputField roomNameInputField;
    [SerializeField]
    private List<Button> maxPlayerCountButtons;

    private CreateGameRoomData roomData;

    // Start is called before the first frame update
    void Start()
    {
        roomData = new CreateGameRoomData() { maxPlayerCount = 8};
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UpdateMaxPlayerCount(int count)
    {
        roomData.maxPlayerCount = count;
        Debug.Log(roomData.maxPlayerCount);
    }

    public void CreateRoom()
    {
        var manager = RoomManager.singleton;
        manager.maxConnections = roomData.maxPlayerCount;
        Debug.Log(manager.maxConnections);
    }
}

public class CreateGameRoomData
{
    public string roomName;
    public int maxPlayerCount;
}