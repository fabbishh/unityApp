using Gpm.WebView;
using OneDevApp.CustomTabPlugin;
using System.Collections.Generic;
using Ugi.PlayInstallReferrerPlugin;
using UnityEngine;

public class BrowserManager : MonoBehaviour
{
    public System.Action<string, YandexAppMetricaRequestDeviceIDError?> OnYandexRequest;
    public ChromeCustomTab chromeTab;
    private string appmetrica_device_id = "";
    private string _url;

    private void Awake()
    {
        OnYandexRequest += OnRequestAccepted;
    }

    public void OnRequestAccepted(string var1, YandexAppMetricaRequestDeviceIDError? var2)
    {
        appmetrica_device_id = var1;
    }

    public async void OpenBrowser(BrowserType browserType, string geo, string webViewLink)
    {
        var firebaseUniqueUserId = await Firebase.Analytics.FirebaseAnalytics.GetAnalyticsInstanceIdAsync();

        AppMetrica.Instance.RequestAppMetricaDeviceID(OnYandexRequest);
        while (appmetrica_device_id == "")
        {
            await System.Threading.Tasks.Task.Delay(100);
        }

        var advertisingId = GetAdvertisingId();

        _url = GetFilledLink(webViewLink, firebaseUniqueUserId, appmetrica_device_id, advertisingId, browserType, geo);

        if (browserType == BrowserType.ChromeTabs)
        {
            chromeTab.canReopenChromeTabs = true;
            chromeTab.url = _url;
            OpenCustomChromeTab(_url);
        }
        else if (browserType == BrowserType.WebView)
        {
            OpenWebView(_url);
        }

        PreferencesBridge.SaveValue("isOpenedWebView", true);
        chromeTab.StartNotifications();

    }

    private string GetAdvertisingId()
    {
        string advertisingId = "";

        AndroidJavaClass up = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject currentActivity = up.GetStatic<AndroidJavaObject>("currentActivity");
        AndroidJavaClass client = new AndroidJavaClass("com.google.android.gms.ads.identifier.AdvertisingIdClient");
        AndroidJavaObject adInfo = client.CallStatic<AndroidJavaObject>("getAdvertisingIdInfo", currentActivity);
        advertisingId = adInfo.Call<string>("getId").ToString();

        return advertisingId;
    }

    private void OpenCustomChromeTab(string url)
    {
        chromeTab.OpenCustomTab(url, "#1e81b0", "#1e81b0", false, false);
    }

    private void OpenWebView(string url)
    {
        GpmWebView.ShowUrl(
            url,
            new GpmWebViewRequest.Configuration()
            {
                style = GpmWebViewStyle.FULLSCREEN,
                orientation = GpmOrientation.UNSPECIFIED,
                isClearCookie = false,
                isClearCache = false,
                isNavigationBarVisible = false,
                navigationBarColor = "#333333",
                title = "",
                isBackButtonVisible = true,
                isForwardButtonVisible = true,
                supportMultipleWindows = true,
#if UNITY_IOS
            contentMode = GpmWebViewContentMode.MOBILE
#endif
            },
            // See the end of the code example
            OnCallback,
            new List<string>()
            {
            "USER_ CUSTOM_SCHEME"
            });
    }

    private void OnCallback(
    GpmWebViewCallback.CallbackType callbackType,
    string data,
    GpmWebViewError error)
    {
        Debug.Log("OnCallback: " + callbackType);
        switch (callbackType)
        {
            case GpmWebViewCallback.CallbackType.Open:
                if (error != null)
                {
                    Debug.LogFormat("Fail to open WebView. Error:{0}", error);
                }
                break;
            case GpmWebViewCallback.CallbackType.Close:
                OpenWebView(_url);
                if (error != null)
                {
                    Debug.LogFormat("Fail to close WebView. Error:{0}", error);
                }
                break;
            case GpmWebViewCallback.CallbackType.PageLoad:
                if (string.IsNullOrEmpty(data) == false)
                {
                    Debug.LogFormat("Loaded Page:{0}", data);
                }
                break;
            case GpmWebViewCallback.CallbackType.MultiWindowOpen:
                Debug.Log("MultiWindowOpen");
                break;
            case GpmWebViewCallback.CallbackType.MultiWindowClose:
                Debug.Log("MultiWindowClose");
                break;
            case GpmWebViewCallback.CallbackType.Scheme:
                if (error == null)
                {
                    if (data.Equals("USER_ CUSTOM_SCHEME") == true || data.Contains("CUSTOM_SCHEME") == true)
                    {
                        Debug.Log(string.Format("scheme:{0}", data));
                    }
                }
                else
                {
                    Debug.Log(string.Format("Fail to custom scheme. Error:{0}", error));
                }
                break;
            case GpmWebViewCallback.CallbackType.GoBack:
                Debug.Log("GoBack");
                break;
            case GpmWebViewCallback.CallbackType.GoForward:
                Debug.Log("GoForward");
                break;
            case GpmWebViewCallback.CallbackType.ExecuteJavascript:
                Debug.LogFormat("ExecuteJavascript data : {0}, error : {1}", data, error);
                break;
        }
    }

    private string GetFilledLink(string rawLink, string firebaseUserId, string appmetricaUserId, string advertisingId, BrowserType browserType, string country)
    {
        string referrer = null;
        while (referrer == null)
        {
            PlayInstallReferrer.GetInstallReferrerInfo((installReferrerDetails) =>
            {
                if (installReferrerDetails.InstallReferrer != null)
                {
                    referrer = installReferrerDetails.InstallReferrer;
                }
                else
                {
                    referrer = "referrer";
                }
            });
        }


        var link = rawLink.Replace("{user_id}", firebaseUserId)
                          .Replace("{appmetrica_device_id}", appmetricaUserId)
                          .Replace("{guid}", advertisingId)
                          .Replace("{referer}", referrer)
                          .Replace("{versionapp}", Application.version)
                          .Replace("{country}", country)
                          .Replace("{browser}", browserType.ToString());

        return link;
    }

}