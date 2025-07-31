using StarterAssets;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public Transform player;
    public Menu mainMenu;
    public Menu pauseMenu;
    public Menu inventoryMenu;
    public GameObject activationIndicator;

    private Stack<Menu> activeMenus = new Stack<Menu>();
    private StarterAssetsInputs playerInput;


    private void Start()
    {
        playerInput = player.GetComponent<StarterAssetsInputs>();
        //TODO
        //if(SceneManager.GetActiveScene().buildIndex == 0)
        //{
        //    EnterMenu(mainMenu);
        //}
        //else
        //{
        //    ExitMenu(mainMenu);
        //}
    }

    private void Update()
    {
        //ToggleControls();
        if(playerInput.pause)
        {
            playerInput.pause = false;
            ToggleMenu(pauseMenu);
        }

        if(playerInput.inventory)
        {
            playerInput.inventory = false;
            ToggleMenu(inventoryMenu);
        }

        if(playerInput.menuLeft || playerInput.menuRight)
        {
            if(inventoryMenu.isActiveAndEnabled)
            {
                int index = playerInput.menuLeft ? -1 : 1;
                inventoryMenu.GetComponent<EquipmentMenu>().Navigate(index);
            }
            playerInput.menuLeft = false;
            playerInput.menuRight = false;
        }

        if(activeMenus.Count > 0)
        {
            DisableCharacterContols();
        }
    }

    void DisableCharacterContols()
    {
        playerInput.aim = false;
        playerInput.activate = false;
        playerInput.dodge = false;
        playerInput.heavyAttack = false;
        playerInput.lightAttack = false;
        playerInput.reload = false;
    }

    void EnterMenu(Menu menu)
    {
        if(activeMenus.TryPeek(out Menu currentMenu))
            ExitMenu(currentMenu);

        EventSystem.current.SetSelectedGameObject(null);
        menu.gameObject.SetActive(true);

        EventSystem.current.SetSelectedGameObject(menu.firstButton);
        menu.OnEnterMenu();

        if(menu.pauseTime)
        {
            Time.timeScale = menu.timeScale;
        }

        activeMenus.Push(menu);
    }

    void ExitMenu(Menu menu)
    {
        EventSystem.current.SetSelectedGameObject(null);
        menu.OnExitMenu();
        menu.gameObject.SetActive(false);

        if(Time.timeScale < 1)
        {
            Time.timeScale = 1;
        }
    }

    void ToggleControls()
    {
        if(activeMenus.Count >= 1)
        {
            player.GetComponent<PlayerInput>().enabled = false;
        }
        else
        {
            AnimatorStateInfo state = player.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0);
            if(state.IsTag("Static"))
            {
                player.GetComponent<PlayerInput>().enabled = false;
            }
            else
            {
                player.GetComponent<PlayerInput>().enabled = true;
            }
        }
    }

    void BackMenu()
    {
        if(activeMenus.Count == 0)
        {
            print("[WARNING]: Trying to go back a menu when none are active...");
        }
        else
        {
            Menu current = activeMenus.Pop();
            ExitMenu(current);

            if(activeMenus.Count >= 1)
            {
                Menu parent = activeMenus.Pop();
                EnterMenu(parent);
            }
        }
    }

    void ToggleMenu(Menu menu)
    {
        if(!menu.isActiveAndEnabled)
            EnterMenu(menu);
        else
            BackMenu();
    }

    public void SetActivationIndicator(bool activeState)
    {
        activationIndicator.SetActive(activeState);
    }

    #region Menu Button Clicks
    public void ResumePause()
    {
        BackMenu();
    }

    public void RestartPause()
    {
        Time.timeScale = 1;
        string currentSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentSceneName);
    }

    public void SubMenuButtonClick(Menu menu)
    {
        if(!menu.isActiveAndEnabled)
            EnterMenu(menu);
    }
    #endregion
}
