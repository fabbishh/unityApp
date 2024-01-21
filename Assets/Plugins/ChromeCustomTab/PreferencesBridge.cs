using UnityEngine;

public class PreferencesBridge : MonoBehaviour
{
    public static void SaveValue(string key, bool value)
    {
        using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
            AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            AndroidJavaClass sharedPreferencesHelperClass = new AndroidJavaClass("com.onedevapp.customchrometabs.PreferencesHelper");
            sharedPreferencesHelperClass.CallStatic("saveValue", currentActivity, key, value);
        }
    }

    public static string GetValue(string key)
    {
        using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
            AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            AndroidJavaClass sharedPreferencesHelperClass = new AndroidJavaClass("com.onedevapp.customchrometabs.PreferencesHelper");
            return sharedPreferencesHelperClass.CallStatic<string>("getValue", currentActivity, key);
        }
    }
}
