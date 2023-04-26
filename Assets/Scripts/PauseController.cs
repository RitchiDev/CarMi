using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PauseController : MonoBehaviour
{
    private PlayerInput m_Input;
    private InputAction m_PauseAction;

    [SerializeField] private GameObject m_PauseMenu;
    [SerializeField] private Selectable m_FirstSelectedOnPause;

    private void Awake()
    {
        m_Input = new PlayerInput();
    }

    private void OnEnable()
    {
        m_PauseAction = m_Input.Player.Pause;
        m_PauseAction.Enable();

        m_PauseAction.canceled += UpdatePausedState;
    }

    private void UpdatePausedState(InputAction.CallbackContext context)
    {
        if (GameManager.Instance.GetCurrentGameState() == GameState.Victory)
        {
            return;
        }

        switch (GameManager.Instance.GetCurrentGameState())
        {
            case GameState.Playing:

                PauseGame();

                break;
            case GameState.Paused:

                ResumeGame();

                break;
            case GameState.Victory:


                break;
            default:
                break;
        }

    }

    public void ResumeGame()
    {
        m_PauseMenu.SetActive(false);
        GameManager.Instance.SetGameState(GameState.Playing);
        GameManager.Instance.SetGameTime(1f);
    }

    private void PauseGame()
    {
        if(m_FirstSelectedOnPause != null)
        {
            EventSystem.current.SetSelectedGameObject(m_FirstSelectedOnPause.gameObject);
        }

        if(m_PauseMenu != null)
        {
            m_PauseMenu.SetActive(true);
        }

        GameManager.Instance.SetGameState(GameState.Paused);
        GameManager.Instance.SetGameTime(0f);
    }
}
