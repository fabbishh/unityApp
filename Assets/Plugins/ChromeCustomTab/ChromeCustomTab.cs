using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OneDevApp.CustomTabPlugin
{
    public class ChromeCustomTab : MonoBehaviour
    {
        public float checkPeriod;
        public float currentTimerValue;

        [HideInInspector] public bool canReopenChromeTabs;
        [HideInInspector] public string url;

        private void Update()
        {
            if (canReopenChromeTabs)
            {
                if (currentTimerValue > 0)
                {
                    currentTimerValue -= Time.deltaTime;
                }
                else
                {
                    currentTimerValue = checkPeriod;
                    OpenCustomTab(url, "#1e81b0", "#1e81b0", false, false);
                }
            }
        }

        public void OpenCustomTab(string urlToLaunch, string colorCode, string secColorCode, bool shouldReopen = false, bool showTitle = false, bool showUrlBar = false)
        {
            if (Application.platform == RuntimePlatform.Android)
            {
                using (var javaUnityPlayer = new AndroidJavaClass(m_unityMainActivity))
                {
                    using (var mContext = javaUnityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                    {
                        using (var jc = new AndroidJavaClass("com.onedevapp.customchrometabs.CustomTabPlugin"))
                        {
                            this.url = urlToLaunch;
                            var mAuthManager = jc.CallStatic<AndroidJavaObject>("getInstance");
                            mAuthManager.Call<AndroidJavaObject>("setActivity", mContext);
                            mAuthManager.Call<AndroidJavaObject>("setUrl", urlToLaunch);
                            mAuthManager.Call<AndroidJavaObject>("setColorString", colorCode);
                            mAuthManager.Call<AndroidJavaObject>("setSecondaryColorString", secColorCode);
                            mAuthManager.Call<AndroidJavaObject>("setReopenStatus", shouldReopen);
                            mAuthManager.Call<AndroidJavaObject>("ToggleShowTitle", showTitle);
                            mAuthManager.Call<AndroidJavaObject>("ToggleUrlBarHiding", showUrlBar);
                            mAuthManager.Call("openCustomTab");
                        }
                    }
                }
            }
        }

        public void StartNotifications()
        {
            if (Application.platform == RuntimePlatform.Android)
            {
                using (var javaUnityPlayer = new AndroidJavaClass(m_unityMainActivity))
                {
                    using (var mContext = javaUnityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                    {
                        using (var jc = new AndroidJavaClass("com.onedevapp.customchrometabs.NotificationPlugin"))
                        {
                            try
                            {
                                var mAuthManager = jc.CallStatic<AndroidJavaObject>("getInstance");
                                mAuthManager.Call<AndroidJavaObject>("setActivity", mContext);
                                mAuthManager.Call("startNotifications");
                            }
                            catch (System.Exception e)
                            {
                                Debug.LogError(e.Message);
                                throw;
                            }
                        }
                    }
                }
            }
        }

#pragma warning disable 0414
        /// <summary>
        /// UnityMainActivity current activity name or main activity name
        /// Modify only if this UnityPlayer.java class is extends or used any other default class
        /// </summary>
        [Tooltip("Android Launcher Activity")]
        [SerializeField]
        private string m_unityMainActivity = "com.unity3d.player.UnityPlayer";

#pragma warning restore 0414
    }
}
