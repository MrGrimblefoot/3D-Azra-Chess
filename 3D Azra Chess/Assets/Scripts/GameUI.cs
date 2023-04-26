using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameUI : MonoBehaviour
{
    public static GameUI Instance { set; get; }

    private Animator anim;
    [SerializeField] private GameObject board;

    private void Awake()
    {
        Instance = this;
        anim = GetComponent<Animator>();
    }

    public void OnLocalGameButton()
    {
        anim.SetTrigger("InGame");
        board.SetActive(true);
    }

    public void OnOnlineGameButton()
    {
        anim.SetTrigger("OnlineMenu");
    }

    public void OnOnlineHostButton()
    {
        anim.SetTrigger("HostWaitingMenu");
    }

    public void OnOnlineConnectButton()
    {
        Debug.Log("Connect to Game!");
    }

    public void OnOnlineBackButton()
    {
        anim.SetTrigger("StartMenu");
    }

    public void OnHostBackButton()
    {
        anim.SetTrigger("OnlineMenu");
    } 
    
    public void OnExitGame()
    {
        Application.Quit();
    }
}
