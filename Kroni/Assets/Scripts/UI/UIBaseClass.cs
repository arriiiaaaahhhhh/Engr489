using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIBaseClass : MonoBehaviour
{
    public static bool menuOpen = false;
    public static GameObject currentMenu;
    public GameObject menu;

    // Open or Close Menu Screens
    public void ToggleMenu()
    {
        if (!menuOpen)
        {
            Debug.Log("Open Menu");
            OpenMenu();
        }
        else if (CurrentMenuIsThis())
        {
            Debug.Log("Close Menu");
            CloseMenu();
        }
    }

    // If player opens menu, pause movement + oxygen + look around
    public void OpenMenu() // check that menu is NOT inventory, player should still be able to move for that.
    {
        Cursor.lockState = CursorLockMode.None;
        menu.SetActive(true);
        currentMenu = menu;
        menuOpen = true;
    }
    // Close menu, resume game
    public void CloseMenu()
    {
        Cursor.lockState = CursorLockMode.Locked;
        menu.SetActive(false);
        currentMenu = null;
        menuOpen = false;
    }

    public bool CurrentMenuIsThis() {
        return currentMenu == menu;
    }
}
