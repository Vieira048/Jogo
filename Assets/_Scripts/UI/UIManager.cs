using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    [Header("------包含面板 (Painéis)------")]
    public CharacterMenu characterMenu;
    public CharacterHUD characterHUD;
    public FloatingTextManager floatingTextManager;

    [Header("------特殊状态机 (Animações)------")]
    public Animator deathMenuAnim;

    [Header("------Configurações de Cena------")]
    public GameObject controlesMobile;
    public GameObject menuObjetoPrincipal;

    private void Start()
    {
        if (deathMenuAnim != null)
            deathMenuAnim.gameObject.SetActive(false);

        UIUpdate();
    }

    private void OnEnable() { SceneManager.sceneLoaded += OnSceneLoaded; }
    private void OnDisable() { SceneManager.sceneLoaded -= OnSceneLoaded; }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Inicio" || scene.name == "Creditos")
        {
            if (controlesMobile != null) controlesMobile.SetActive(false);
            if (characterHUD != null) characterHUD.gameObject.SetActive(false);
            if (menuObjetoPrincipal != null) menuObjetoPrincipal.SetActive(false);
            Time.timeScale = 1f;
        }
        else
        {
            if (controlesMobile != null) controlesMobile.SetActive(true);
            if (characterHUD != null) characterHUD.gameObject.SetActive(true);
        }
    }

    public void UIUpdate()
    {
        if (characterMenu != null) characterMenu.UpdateMenu();
        if (characterHUD != null) characterHUD.UpdateHUD();
    }

    public void ShowText(string msg, int fontSize, Color color, Vector3 position, Vector3 motion, float duration)
    {
        if (floatingTextManager != null)
            floatingTextManager.Show(msg, fontSize, color, position, motion, duration);
    }

    public void HideDeathAnimation()
    {
        if (deathMenuAnim != null)
        {
            deathMenuAnim.SetTrigger("Hide");
            deathMenuAnim.gameObject.SetActive(false);
        }
    }

    public void ShowDeathAnimation()
    {
        if (deathMenuAnim != null)
        {
            deathMenuAnim.gameObject.SetActive(true);
            deathMenuAnim.SetTrigger("Show");
        }
    }

    public void QuitGame() { Application.Quit(); }

    public void QuitToMainMenu()
    {
        Time.timeScale = 1f;

        if (menuObjetoPrincipal != null)
            menuObjetoPrincipal.SetActive(false);

        if (characterHUD != null)
            characterHUD.gameObject.SetActive(false);

        if (controlesMobile != null)
            controlesMobile.SetActive(false);

        SceneManager.LoadScene("Inicio");
    }
}