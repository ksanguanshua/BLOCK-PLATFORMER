using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    EventSystem eventSystem;
    [System.Serializable]
    public class MenuPanel
    {
        public string panelName;
        [SerializeField] public bool open;
        public Selectable startingSelection;
        public GameObject panelObject;
        public List<GameObject> interactables;
        public List<GameObject> openButtons;
        public List<GameObject> closeButtons;
        public List<string> coPanels;
        public List<string> inactiveCoPanels;
        public bool animated;
        public float timeToEnd;
    }

    [System.Serializable]
    public class Buttons
    {
        public string name;
        public GameObject gameObject;
        public string transitionToScene;
        public bool quitProgram;
        public bool exitPause;
    }

    public string playSceneName;
    public bool pause;
    public bool currentlyPaused;
    public List<MenuPanel> panels;
    public List<Buttons> buttons;
    public List<Slider> sliders;



    void Start()
    {
        Time.timeScale = 1;
        eventSystem = GameObject.Find("EventSystem").GetComponent<EventSystem>();

        //Cursor.visible = false;
        //Cursor.lockState = CursorLockMode.Locked;

        StartSlideChecker();

        // ensure everything is closed at start
        foreach (var p in panels)
        {
            if (p.panelObject != null)
                p.panelObject.SetActive(false);

            SetInteractableList(p.interactables, false);
        }

        if (pause)
        {

        }
        else
        {
            // Enable the "MainMenu" panel automatically
            MenuPanel home = GetPanel("MainMenu");
            if (home != null)
            {
                if (home.panelObject != null)
                    home.panelObject.SetActive(true);

                SetInteractableList(home.interactables, true);
            }
        }
    }

    // Call this from any UI Button, sending the button GameObject
    public void ButtonPressed(GameObject button)
    {
        string btnName = button.name;

        foreach (Buttons but in buttons)
        {
            if (but.name == btnName)
            {
                if (but.exitPause)
                {
                    Pause();
                    return;
                }
                else if (but.quitProgram)
                {
                    Application.Quit();
                    return;
                }
                else
                {
                    print(but.name);
                    print(but.transitionToScene);
                    SceneManager.LoadScene(but.transitionToScene);
                    return;
                }
            }
        }

        // THESE ARE BASICALLY OVERRIDDEN BUT I DONT WANNA MESS WITH THE CORE GAMEPLAY

        // Play button
        if (btnName == "Play")
        {
            SceneManager.LoadScene(playSceneName);
            return;
        }

        // Quit button
        if (btnName == "Quit")
        {
            Application.Quit();
            return;
        }

        if (pause && btnName == "Resume")
        {
            Pause();
        }

        // Check if button opens a panel
        foreach (var panel in panels)
        {
            if (panel.openButtons != null && panel.openButtons.Contains(button))
            {
                OpenPanel(panel.panelName);
                return;
            }
        }

        // Check close buttons for each panel
        foreach (var panel in panels)
        {
            if (panel.closeButtons != null && panel.closeButtons.Contains(button))
            {
                ClosePanel(panel.panelName);
                return;
            }
        }
    }

    // ------------------------------------------------------------
    // Panel Opening Logic
    // ------------------------------------------------------------
    public void OpenPanel(string name)
    {
        MenuPanel panel = GetPanel(name);
        if (panel == null) return;

        // enable this panel
        if (panel.panelObject != null)
        {
            if (panel.animated && !panel.open)
            {
                panel.panelObject.SetActive(true);
                Animator anim = panel.panelObject.GetComponent<Animator>();
                anim.SetTrigger("Open");
                eventSystem.SetSelectedGameObject(panel.startingSelection.gameObject);
                //StartCoroutine(AnimationStart(panel.timeToEnd, panel));
            }
            else
            {
                panel.panelObject.SetActive(true);
                //eventsystem selector
                eventSystem.SetSelectedGameObject(panel.startingSelection.gameObject);
            }
            panel.open = true;
        }

        // make interactables active
        SetInteractableList(panel.interactables, true);

        // Close all other panels unless listed as a coPanel
        foreach (var other in panels)
        {
            if (other.panelName == name) continue;

            bool stayOpen = panel.coPanels != null && panel.coPanels.Contains(other.panelName);
            bool stayOpenDisabled = panel.inactiveCoPanels != null && panel.inactiveCoPanels.Contains(other.panelName);

            if (!stayOpen && !stayOpenDisabled)
            {
                if (other.panelObject != null)
                {
                    if (other.animated && other.open == true)
                    {
                        Animator anim = other.panelObject.GetComponent<Animator>();
                        anim.SetTrigger("Close");
                        StartCoroutine(AnimationEnd(other.timeToEnd, other.panelObject));
                    }
                    else
                    {
                        other.panelObject.SetActive(false);
                    }
                    other.open = false;
                }

                SetInteractableList(other.interactables, false);
            }
            else if (stayOpenDisabled)
            {
                SetInteractableList(other.interactables, false);
            }
        }
    }

    // ------------------------------------------------------------
    // Panel Closing Logic
    // ------------------------------------------------------------
    public void ClosePanel(string name)
    {
        MenuPanel panel = GetPanel(name);
        if (panel == null) return;

        // disable its object and interaction
        if (panel.panelObject != null)
        {
            if (panel.animated && panel.open)
            {
                Animator anim = panel.panelObject.GetComponent<Animator>();
                anim.SetTrigger("Close");
                StartCoroutine(AnimationEnd(panel.timeToEnd, panel.panelObject));
            }
            else
            {
                panel.panelObject.SetActive(false);
            }
            panel.open = false;
        }

        SetInteractableList(panel.interactables, false);

        // Check if *any* other panel is still open
        foreach (var p in panels)
        {
            if (p.panelObject != null && p.panelObject.activeSelf)
            {
                break;
            }
        }
    }

    // ------------------------------------------------------------
    // Utility
    // ------------------------------------------------------------

    public IEnumerator AnimationEnd(float timeToEnd, GameObject panel)
    {
        yield return new WaitForSeconds(timeToEnd);
        panel.SetActive(false);
    }
    public IEnumerator AnimationStart(float timeToEnd, MenuPanel panel)
    {
        yield return new WaitForSeconds(timeToEnd);
        eventSystem.SetSelectedGameObject(panel.startingSelection.gameObject);
    }

    MenuPanel GetPanel(string name)
    {
        foreach (var p in panels)
            if (p.panelName == name)
                return p;

        return null;
    }

    void SetInteractableList(List<GameObject> list, bool state)
    {
        if (list == null) return;

        foreach (var obj in list)
        {
            if (obj == null) continue;

            var btn = obj.GetComponent<Button>();
            if (btn != null) { btn.interactable = state; continue; }

            var slider = obj.GetComponent<Slider>();
            if (slider != null) { slider.interactable = state; continue; }

            var toggle = obj.GetComponent<Toggle>();
            if (toggle != null) { toggle.interactable = state; continue; }
        }
    }

    void Update()
    {
        SlideChecker(); // you could refactor for onvaluechange but IDK how that unity event works
        if (pause && Input.GetKeyDown(KeyCode.Escape))
        {
            Pause();
        }
    }

    void StartSlideChecker()
    {
        if (sliders.Count > 0)
        {
            foreach (Slider s in sliders)
            {
                if (s != null)
                {
                    if (s.name == "MUSIC")
                    {
                        var e = PlayerPrefs.GetFloat("MusicVolume", 0.5f);
                        //print(e);
                        s.value = e;
                    }
                    else if (s.name == "SFX")
                    {
                        var e = PlayerPrefs.GetFloat("SFXVolume", 0.5f);
                        //print(e);
                        s.value = e;
                    }
                }
            }
        }
    }
    void SlideChecker()
    {
        if (sliders.Count > 0)
        {
            foreach (Slider s in sliders)
            {
                if (s != null)
                {
                    if (s.name == "MUSIC")
                    {
                        float value = s.value;
                        PlayerPrefs.SetFloat("MusicVolume", value);
                        AudioManager.instance.SetBGMParameter("Volume", value);
                        PlayerPrefs.Save();
                    }
                    else if (s.name == "SFX")
                    {
                        float value = s.value;
                        PlayerPrefs.SetFloat("SFXVolume", value);
                        AudioManager.instance.SetSFXParameter(value);
                        PlayerPrefs.Save();

                    }
                }
            }
        }
    }

    public void OnPause(InputValue value)
    {
        print("heehee");
    }

    public void Pause()
    {
        if (currentlyPaused == true)
        {
            ClosePanel("PauseMenu");
            currentlyPaused = false;
            Time.timeScale = 1;
        }
        else if (currentlyPaused == false)
        {
            OpenPanel("PauseMenu");
            currentlyPaused = true;
            Time.timeScale = 0;
        }
    }
}
