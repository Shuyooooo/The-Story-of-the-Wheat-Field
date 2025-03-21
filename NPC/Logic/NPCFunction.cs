using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCFunction : MonoBehaviour
{
    public InventoryBag_SO shopData;
    private bool isOpen;//√Ê∞Â
   

    private void Update()
    {
        if (isOpen && Input.GetKey(KeyCode.Escape))
        {
            //πÿ±’…ÃµÍ
            CloseShop();
        }
    }

    public void OpenShop()
    {       
        isOpen = true;
        EventHandler.CallBaseBagOpenEvent(SlotType.Shop, shopData);
        EventHandler.CallUpdateGameStateEvent(GameState.Pause);
    }

    public void CloseShop()
    {
        isOpen = false;
        EventHandler.CallBaseBagCloseEvent(SlotType.Shop, shopData);
        EventHandler.CallUpdateGameStateEvent(GameState.Gameplay);
    }

   
}
