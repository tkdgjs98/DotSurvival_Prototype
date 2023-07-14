using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;


public class LocalUI : MonoBehaviour
{
    [SerializeField]
    private InputField nicknameInputField;
    [SerializeField]
    private GameObject createRoomUI;
    [SerializeField]
    private GameObject findRoomUI;
    
    public void OnClickCreateRoomButton()
    {
        if(nicknameInputField.text != "")
        {
            Debug.Log("CreateRoom");
            PlayerData.nickname = nicknameInputField.text;
            Debug.Log("input: "+PlayerData.nickname);
            createRoomUI.SetActive(true);
            gameObject.SetActive(false);
        }
        else
        {
            nicknameInputField.GetComponent<Animator>().SetTrigger("on");
        }
    }

    public void OnclickFindGameRoomButton()
    {
        Debug.Log("FindRoom");
        if(nicknameInputField.text != "")
        {
            PlayerData.nickname = nicknameInputField.text;
            Debug.Log("input: "+nicknameInputField.text);
            Debug.Log("input: "+PlayerData.nickname);
            findRoomUI.SetActive(true);
            gameObject.SetActive(false);
        }
        else
        {
            nicknameInputField.GetComponent<Animator>().SetTrigger("on");
        }
    }

    public void OnClickEnterGameRoomButton()
    {
        Debug.Log("EnterRoom");
        if(nicknameInputField.text != "")
        {
            PlayerData.nickname = nicknameInputField.text;
            var manager = RoomManager.singleton;
            manager.StartClient();
        }
        else
        {
            nicknameInputField.GetComponent<Animator>().SetTrigger("on");
        }
    }
    
}
