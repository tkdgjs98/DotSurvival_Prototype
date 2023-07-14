using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class RoomPlayer : NetworkRoomPlayer
{
    [SyncVar]
    public string nickname;

    [Command]
    public void CmdSetNickname(string nick)
    {
        Debug.Log(nick);
        nickname = nick;
    }

    public void Start()
    {
        base.Start();

        if(isLocalPlayer)
        {
            CmdSetNickname(PlayerData.nickname);
        }
    }
}
