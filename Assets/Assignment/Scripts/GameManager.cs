using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance => instance;
    private static GameManager instance;
    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    [SerializeField] private GameObject[] levels;
    [SerializeField] private int currentLevel = 0;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private GameObject levelFailedPanel, levelSuccessPanel;
    [SerializeField] private AdsManager adsManager;
    public static event Action OnToggleGridLines;
    private Level currentActiveLevel;
    private delegate void CurrentLevelFunc(int x);
    private CurrentLevelFunc currentLevelFunc;
    private void Start()
    {
        SpawnLevel(currentLevel);
        //adsManager.bannerAds.ShowBannerAd();
        StartCoroutine(ShowBannerAd());
    }
    IEnumerator ShowBannerAd()
    {
        yield return new WaitForSeconds(5f);
        adsManager.bannerAds.ShowBannerAd();
    }

    public void SetTimer(float time)
    {
        string text = string.Format("{0:00}:{1:00}", Mathf.FloorToInt(time / 60f), Mathf.FloorToInt(time % 60));
        Debug.Log(text);
        Debug.Log(time);
        timerText.SetText(text);
    }
    public void ToggleLines()
    {
        Debug.Log("Lines toggled");
        OnToggleGridLines?.Invoke();
    }
    private void SpawnLevel(int index)
    {
        if(index <= 0)
        {
            index = 0;
        }
        if (index >= levels.Length)
        {
            index = 0;
        }
        if (currentActiveLevel != null)
        {
            Destroy(currentActiveLevel.gameObject);
        }
        currentActiveLevel = Instantiate(levels[index]).GetComponent<Level>();
        currentActiveLevel.Init(this);
        Camera.main.transform.localPosition = currentActiveLevel.CameraPos;
        adsManager.bannerAds.ShowBannerAd();
    }
    public void SpawnNextLevel()
    {
        currentLevel++;
        if(currentLevel >= levels.Length)
        {
            currentLevel = 0;
        }
        adsManager.interstitialAds.ShowInterstitialAd();
    }
    public void RestartLevel()
    {
        adsManager.interstitialAds.ShowInterstitialAd();
    }
    public void DisplayLevelFailed(Level level)
    {
        adsManager.bannerAds.HideBannerAd();
        levelFailedPanel.SetActive(true);
    }
    public void DisplayLevelSuccess(Level level)
    {
        adsManager.bannerAds.HideBannerAd();
        levelSuccessPanel.SetActive(true);
    }

    public void CloseInterstitialAd()
    {
        SpawnLevel(currentLevel);
    }
}

