using UnityEngine;
using UnityEngine.SceneManagement;

public enum SceneType
{
    NAMING,
    MATCHING,
    INGAME
}
public class GameManager : Singleton<GameManager>
{
    protected override bool IsDontDestroying => true;
    public string nickName;

    protected override void OnCreated()
    {
        Application.targetFrameRate = 60;
        Application.runInBackground = true;
        
        LoadNickName();
        OnReset();
    }
    
    private void LoadNickName()
    {
    }

    private void SaveNickName()
    {
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
            SaveNickName();
    }

    private void OnApplicationQuit()
    {
        SaveNickName();
    }

    protected override void OnReset()
    {
        SetResolution();
    }

    public void LoadScene(SceneType sceneType)
    {
        SceneManager.LoadScene((int)sceneType);
    }

    private void SetResolution()
    {
        int setWidth = 2000;
        int setHeight = 900;

        int deviceWidth = Screen.width;
        int deviceHeight = Screen.height;

        Screen.SetResolution(setWidth, (int)(((float)deviceHeight / deviceWidth) * setWidth), true);

        float screenMultiplier = (float)setWidth / setHeight;
        float deviceMultiplier = (float)deviceWidth / deviceHeight;

        if (screenMultiplier < deviceMultiplier)
        {
            float newWidth = screenMultiplier / deviceMultiplier;
            Camera.main.rect = new Rect((1f - newWidth) / 2f, 0f, newWidth, 1f);
        }
        else
        {
            float newHeight = deviceMultiplier / screenMultiplier;
            Camera.main.rect = new Rect(0f, (1f - newHeight) / 2f, 1f, newHeight);
        }
    }
}