using UnityEngine;
using System.Collections.Generic;
using FMOD.Studio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [SerializeField] FMODUnity.EventReference tongueOut;
    [SerializeField] FMODUnity.EventReference tongueIn;

    [SerializeField] FMODUnity.EventReference jump;
    [SerializeField] FMODUnity.EventReference dash;

    [SerializeField] FMODUnity.EventReference buttonClick;
    [SerializeField] FMODUnity.EventReference doorClose;

    [SerializeField] FMODUnity.EventReference boxHit;
    [SerializeField] FMODUnity.EventReference boxBump;

    [SerializeField] FMODUnity.EventReference BGM;
    [SerializeField] FMODUnity.EventReference victoryMusic;

    EventInstance bgmInstance;

    /*Dictionary<string, FMODUnity.EventReference> stringToSound = new()
    {
        { "tongueIn", tongueIn },
        { "tongueOut", tongueOut },

        { "jump", jump },
        { "dash", dash },

        { "buttonClick", buttonClick },
        { "doorClose", doorClose },

        { "boxHit", boxHit },
        { "boxBump", boxBump },

        { "BGM", BGM },
    };*/

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != null)
        {
            Destroy(this);
        }

        DontDestroyOnLoad(this);
    }

    private void Start()
    {
        bgmInstance = FMODUnity.RuntimeManager.CreateInstance(BGM);
        PlayBGM();
    }

    public void PlayBGM()
    {
        bgmInstance.start();
    }

    public void StopBGM()
    {
        bgmInstance.stop(STOP_MODE.ALLOWFADEOUT);
    }

    public void SetBGMParameter(string parameter, float value)
    {
        bgmInstance.setParameterByName(parameter, value);
    }

    public void SetSFXParameter(float value)
    {
        FMODUnity.RuntimeManager.StudioSystem.setParameterByName("SFX Volume", value);
    }

    public void PlayVictoryMusic()
    {
        FMODUnity.RuntimeManager.PlayOneShot(victoryMusic);
        Debug.Log("playing victory music");
    }

    public void PlayTongueIn()
    {
        FMODUnity.RuntimeManager.PlayOneShot(tongueIn);
    }

    public void PlayTongueOut()
    {
        FMODUnity.RuntimeManager.PlayOneShot(tongueOut);
    }

    public void PlayJump()
    {
        FMODUnity.RuntimeManager.PlayOneShot(jump);
    }

    public void PlayDash()
    {
        FMODUnity.RuntimeManager.PlayOneShot(dash);
    }

    public void PlayButtonClick()
    {
        FMODUnity.RuntimeManager.PlayOneShot(buttonClick);
    }

    public void PlayDoorClose()
    {
        FMODUnity.RuntimeManager.PlayOneShot(doorClose);
    }

    public void PlayBoxHit()
    {
        FMODUnity.RuntimeManager.PlayOneShot(boxHit);
    }

    public void PlayBoxBump()
    {
        FMODUnity.RuntimeManager.PlayOneShot(boxBump);
    }
}
