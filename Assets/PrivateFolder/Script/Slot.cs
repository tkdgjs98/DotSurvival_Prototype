using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Slot : MonoBehaviour
{
    //[SerializeField]
    //private string slotItemID;
    //private PlayerController player;

    public enum Child {ICON, RBTN, INFO}
    private GameObject icon;
    private GameObject rbtn;
    private GameObject info;
    
    private void Awake() {
        //player = gameObject.GetComponent<PlayerController>();
        //slotItemID = null;
        icon = GetSlotChild(Child.ICON);
        rbtn = GetSlotChild(Child.RBTN);
        info = GetSlotChild(Child.INFO);
    }
    /*
    public string GetSlotItemID()
    {
        return slotItemID;
    }
*/
    private GameObject GetSlotChild(Child index)
    {
        return this.gameObject.transform.GetChild((int)index).gameObject;
    }

    public void SetSlotItem(Item item)
    {
        //slotItemID = item.itemID;
        icon.GetComponent<Image>().sprite = item.GetComponent<SpriteRenderer>().sprite;
        info.GetComponent<Text>().text = item.Info();
    }
    public void SetSlotItem()
    {
        //slotItemID = null;
        icon.GetComponent<Image>().sprite = null;
        info.GetComponent<Text>().text = null;
    }

    public void SlotChild_SetActive(bool visible)
    {
        icon.SetActive(visible);
        rbtn.SetActive(visible);
        info.SetActive(visible);
    }
    public void OnClickRemoveKey()
    {
        SlotChild_SetActive(false);
        SetSlotItem();
    }
}
