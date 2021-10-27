using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Reflection;
using UnityEngine.Scripting;

#if UNITY_IOS
using System.Runtime.InteropServices;
#endif

namespace Tabtale.TTPlugins {

    /// <summary>
    /// This class provides initialization of all plugins
    /// </summary>
	public class TTPCore {

        /// <summary>
        /// Event for pausing game music
        /// </summary>
        public static event System.Action<bool> PauseGameMusicEvent;

        /// <summary>
        /// Event for starting a new session
        /// </summary>
        public static event System.Action OnNewTTPSessionEvent;

        /// <summary>
        /// Event for should ask for IDFA
        /// </summary>
        public static event System.Action<bool> OnShouldAskForIDFA;

        /// <summary>
        /// Indicates developing mode status
        /// </summary>
        public static bool DevMode
        {
            get
            {
#if TTP_DEV_MODE
                return true;
#else
                return false;
#endif
            }
        }

        private static TTPIncludedServicesScriptableObject _includedServices;

        public static TTPIncludedServicesScriptableObject IncludedServices
        {
            private set { _includedServices = value; }
            get { return _includedServices; }
        }

        /// <summary>
        /// Define always to use required location
        /// </summary>
        /// <param name="location">Loaction for service</param>
        public static void setGeoServiceAlwaysReturnLocation(string location)
		{
            Debug.Log("TTP_Core::setGeoServiceAlwaysReturnLocation" + location);
#if UNITY_IOS && !TTP_DEV_MODE
            if (Impl != null){
                IosImpl iOSImpl = (IosImpl)Impl;
                iOSImpl.SetGeoServiceAlwaysReturnedLocation(location);
                Debug.Log("TTP_Core::setGeoServiceAlwaysReturnLocation location setted.");
			}
			else
			{
                Debug.LogError("TTP_Core::setGeoServiceAlwaysReturnLocation Impl is null.");
            }

#else
            Debug.LogError("Feature unavailible unless in iOS");
#endif
        }

        /// <summary>
        /// Remove stored always-to-use location
        /// </summary>
        public static void clearGeoServiceAlwaysReturnLocation()
        {
#if UNITY_IOS && !TTP_DEV_MODE
            if (Impl != null)
            {
                IosImpl iOSImpl = (IosImpl)Impl;
                iOSImpl.ClearGeoServiceAlwaysReturnedLocation();
                Debug.Log("TTP_Core::clearGeoServiceAlwaysReturnLocation location setted.");
            }
            else
            {
                Debug.LogError("TTP_Core::clearGeoServiceAlwaysReturnLocation Impl is null.");
            }

#else
            Debug.LogError("Feature unavailible unless in iOS");
#endif
        }

        private static bool _initialized = false;

        /// <summary>
        /// Setting up and initialize all services defined in the service list
        /// </summary>
        public static void Setup()
        {
            if (!_initialized)
            {

                Debug.Log("TTPCore::Setup");
                IncludedServices = Resources.Load<TTPIncludedServicesScriptableObject>("ttpIncludedServices");
                foreach (string clsName in CLASS_LIST)
                {
                    System.Type type = System.Type.GetType(clsName);
                    if (type != null)
                    {
                        TTPluginsGameObject.AddComponent(type);
                        Debug.Log("TTPCore::Setup: Added " + clsName + " to TTPluginsGameObject");
                    }
                    else
                    {
                        Debug.Log("TTPCore::Setup: Couldn't find " + clsName);
                    }
                }
                TTPluginsGameObject.AddComponent(typeof(CoreDelegate));
                ((ITTPCore)Impl).Setup();
#if UNITY_IOS
                Debug.Log("TTPCore::Setup: OnShouldAskForIDFA " + (OnShouldAskForIDFA != null ? "implemented" : "not implemented"));
                if (OnShouldAskForIDFA != null)
                {
                    Debug.Log("TTPCore::Setup: InvokeAskForIDFACoro");
                    TTPluginsGameObject.GetComponent<MonoBehaviour>()
                        .StartCoroutine(InvokeAskForIDFACoro(ShouldAskForIDFA()));
                }
                else 
                {
                    if (ShouldAskForIDFA())
                    {
                        Debug.Log("TTPCore::Setup: disclaimer started");
                        ATTDisclaimer.GetInstance().StartDisclaimer(() =>
                        {
                            Debug.Log("TTPCore::Setup: disclaimer finished");
                            AskForIDFA();
                        });
                    }
                    else 
                    {
                        Debug.Log("TTPCore::Setup: shouldn't ask for IDFA");
                    }
                }
#endif
                InitDeltaDnaAgent();
                InitBilling();
                _initialized = true;
            }
            else
            {
                Debug.LogWarning("TTPCore::Setup:: was called already in this lifecycle.");
            }
        }

        /// <summary>
        /// Indicates that back button pressed
        /// </summary>
        /// <returns>True - if back button pressed</returns>
        public static bool OnBackPressed()
        {
            if(Impl != null)
            {
                return ((ITTPCore)Impl).OnBackPressed();
            }
            return false;
        }

        /// <summary>
        /// Indicates that internet is available
        /// </summary>
        /// <returns>True - if internet is available</returns>
        public static bool IsConnectedToTheInternet()
        {
            if (Impl != null)
                return ((ITTPCore)Impl).IsConnectedToTheInternet();
            return false;
        }

        public static void AskForIDFA()
        {
#if UNITY_IOS && !TTP_DEV_MODE
            var iosImpl = Impl as IosImpl;
            if (iosImpl != null)
            {
                iosImpl.AskForIDFA();
            }
#endif
        }

        public static void RefuseToAskForIDFA()
        {
#if UNITY_IOS && !TTP_DEV_MODE
            var iosImpl = Impl as IosImpl;
            if (iosImpl != null)
            {
                iosImpl.RefuseToAskForIDFA();
            }
#endif
        }

        private static IEnumerator InvokeAskForIDFACoro(bool shouldAskForIDFA)
        {
            yield return new WaitForEndOfFrame();
            if(OnShouldAskForIDFA != null)
                OnShouldAskForIDFA.Invoke(shouldAskForIDFA);
        }

        private static bool ShouldAskForIDFA()
        {
#if UNITY_IOS && !TTP_DEV_MODE
            var iosImpl = Impl as IosImpl;
            if (iosImpl != null)
            {
                return iosImpl.ShouldWaitForIDFA();
            }
#endif
            return false;
        }

        /// <summary>
        /// Initialize billing service
        /// </summary>
        private static void InitBilling()
        {
            System.Type billingType = System.Type.GetType("Tabtale.TTPlugins.TTPBilling");
            if(billingType != null)
            {
                MethodInfo method = billingType.GetMethod("InternalInit", BindingFlags.NonPublic | BindingFlags.Static);
                if(method != null)
                {
                    method.Invoke(null, null);
                }
                else
                {
                    Debug.LogWarning("TTPCore::InitBilling: method InternalInit not found");
                }
            }
            else
            {
                Debug.Log("TTPCore::InitBilling: TTPBilling not found");
            }
        }

        /// <summary>
        /// Initialize delta dna agent
        /// </summary>
        private static void InitDeltaDnaAgent()
        {
            System.Type deltaDnaAgentType = System.Type.GetType("Tabtale.TTPlugins.TTPDeltaDnaAgent");
            if (deltaDnaAgentType != null)
            {
                MethodInfo method = deltaDnaAgentType.GetMethod("InternalInit", BindingFlags.NonPublic | BindingFlags.Static);
                if (method != null)
                {
                    method.Invoke(null, null);
                }
                else
                {
                    Debug.LogWarning("TTPCore::InitDeltaDnaAgent: method InternalInit not found");
                }
            }
            else
            {
                Debug.Log("TTPCore::InitDeltaDnaAgent: TTPDeltaDnaAgent not found");
            }
        }
        /// <summary>
        /// Private list of all services is used for reflection in setup process
        /// </summary>
        private static string[] CLASS_LIST = {
            "Tabtale.TTPlugins.TTPPrivacySettings+PrivacySettingsDelegate",
            "Tabtale.TTPlugins.TTPBilling+BillingDelegate",
            "Tabtale.TTPlugins.TTPInterstitials+InterstitialsDelegate",
            "Tabtale.TTPlugins.TTPRewardedAds+RewardedAdsDelegate",
            "Tabtale.TTPlugins.TTPAnalytics+AnalyticsDelegate",
            "Tabtale.TTPlugins.TTPBanners+BannersDelegate",
            "Tabtale.TTPlugins.TTPPromotion+PromotionDelegate",
            "Tabtale.TTPlugins.TTPNativeCampaign+NativeCampaignDelegate",
            "Tabtale.TTPlugins.TTPCrossPromotion+CrossPromotionDelegate",
            "Tabtale.TTPlugins.TTPSocial+SocialDelegate",
            "Tabtale.TTPlugins.TTPCrossDevicePersistency+CDPDelegate",
            "Tabtale.TTPlugins.TTPOnDemandsResources+OnDemandResourcesDelegate",
            "Tabtale.TTPlugins.TTPDeltaDnaAgent+DeltaDnaAgentDelegate",
            "Tabtale.TTPlugins.TTPOpenAds+OpenAdsDelegate",
            "Tabtale.TTPlugins.TTPRewardedInterstitials+RewardedInterstitialsDelegate",
            "Tabtale.TTPlugins.ATTDisclaimer+DisclaimerDelegate"
        };

        private interface ITTPCore {
            void Setup();
            bool OnBackPressed();
            bool IsConnectedToTheInternet();
        }

		public interface ITTPInternalService {
		}

		public interface ITTPCoreInternal {
#if UNITY_ANDROID
			AndroidJavaObject GetServiceJavaObject(string serviceClassPath);
            AndroidJavaObject GetCurrentActivity();
#endif
#if UNITY_IOS
            void CallCrash();
#endif
            string GetPackageInfo();
            string GetConfigurationJson(string serviceName);

		}

#if UNITY_IOS && !TTP_DEV_MODE
	    private class IosImpl : ITTPCore, ITTPCoreInternal, ITTPInternalService {

	        [DllImport("__Internal")]
 	        private static extern void ttpCrashApp();

            [DllImport("__Internal")]
 	        private static extern void ttpSetGeoServiceAlwaysReturnedLocation(string location);

            [DllImport("__Internal")]
 	        private static extern void ttpClearGeoServiceAlwaysReturnedLocation();

	        [DllImport("__Internal")]
            private static extern void ttpSetup();

            [DllImport("__Internal")]
            private static extern string ttpGetPackageInfo();

            [DllImport("__Internal")]
            private static extern string ttpGetConfigurationJson(string serviceName);

            [DllImport("__Internal")]
            private static extern bool ttpIsConnected();

            [DllImport("__Internal")]
            private static extern void ttpAskForIDFA();

            [DllImport("__Internal")]
            private static extern void ttpRefuseToAskForIDFA();

            [DllImport("__Internal")]
            private static extern bool ttpShouldWaitForIDFA();

            public void Setup()
            {
                ttpSetup();
            }

            public string GetPackageInfo()
            {
                return ttpGetPackageInfo();
            }

            public bool OnBackPressed() { return false; }

            public string GetConfigurationJson(string serviceName)
            {
                return ttpGetConfigurationJson(serviceName);
            }

            public void CallCrash()
            {
                Debug.Log("TTPCore::IosImpl:CallCrash");
		        ttpCrashApp();
            }

            public bool IsConnectedToTheInternet()
            {
                return ttpIsConnected();
            }

            public void SetGeoServiceAlwaysReturnedLocation(string location)
            {
                ttpSetGeoServiceAlwaysReturnedLocation(location);
            }

            public void ClearGeoServiceAlwaysReturnedLocation()
            {
                ttpClearGeoServiceAlwaysReturnedLocation();
            }

            public void AskForIDFA()
            {
                ttpAskForIDFA();
            }

            public void RefuseToAskForIDFA()
            {
                ttpRefuseToAskForIDFA();
            }

            public bool ShouldWaitForIDFA()
            {
                return ttpShouldWaitForIDFA();
            }

        }
#endif

#if UNITY_ANDROID
		private class AndroidImpl : ITTPCore, ITTPCoreInternal, ITTPInternalService {

			private AndroidJavaObject _serviceManager;
            private AndroidJavaObject _activityObject;

            private AndroidJavaObject ServiceManager
            {
                get
                {
                    if (_serviceManager == null)
                    {
                        if (_activityObject != null)
                            _serviceManager = _activityObject.Call<AndroidJavaObject>("getServiceManager");
                        else
                            Debug.LogError("TTPCore::AndroidImpl:GetServiceJavaObject could not get instance of TTPUnityMainActivity.");
                    }
                    if(_serviceManager == null)
                        Debug.LogError("TTPCore::AndroidImpl:GetServiceJavaObject could not get instance of native android service manager.");
                    return _serviceManager;
                }
            }

			public AndroidImpl() {
                Debug.Log("TTPCore::AndroidImpl created");
                AndroidJavaClass activityCls = new AndroidJavaClass("com.tabtale.ttplugins.ttpunity.TTPUnityMainActivity");
				if(activityCls != null){
                    _activityObject = activityCls.CallStatic<AndroidJavaObject>("getActivityInstance");
				}
				else {
					Debug.LogError("TTPCore::AndroidImpl: could not find class of native android service manager.");
				}
			}

			public AndroidJavaObject GetServiceJavaObject(string serviceGetMethod) {
                if(ServiceManager != null)
                {
                    return _serviceManager.Call<AndroidJavaObject>(serviceGetMethod);
                }
                return null;
			}

            public void Setup()
            {
                Debug.Log("TTPCore::AndroidImpl::Setup");
                if (_activityObject != null){
                    _activityObject.Call("setup");
                }
                else {
                    Debug.LogError("TTPCore::AndroidImpl:Setup could not get instance of TTPUnityMainActivity.");
                }
            }

            public string GetPackageInfo()
            {
                if (_activityObject != null)
                {
                    return _activityObject.Call<string>("getPackageInfo");
                }
                else
                {
                    Debug.LogError("TTPCore::AndroidImpl:GetPackageInfo could not get instance of TTPUnityMainActivity.");
                }
                return "";
            }

            public bool OnBackPressed()
            {
                bool rateUsHandledBackPress = false;
                bool nativeHandledBackPress = false;
                System.Type rateUsClsType = System.Type.GetType("Tabtale.TTPlugins.TTPRateUs");
                if (rateUsClsType != null)
                {
                    MethodInfo method = rateUsClsType.GetMethod("HandleAndroidBackPressed", BindingFlags.NonPublic | BindingFlags.Static);
                    if (method != null)
                    {
                        rateUsHandledBackPress = (bool)method.Invoke(null, null);
                    }
                }
                if (ServiceManager != null)
                {
                    nativeHandledBackPress = ServiceManager.Call<bool>("onBackPressed");
                }
                return rateUsHandledBackPress || nativeHandledBackPress;
            }

            public string GetConfigurationJson(string serviceName)
            {
                if (ServiceManager != null)
                {
                    AndroidJavaObject ttpConfiguration = ServiceManager.Call<AndroidJavaObject>("getTtpConfiguration");
                    if(ttpConfiguration != null)
                    {
                        AndroidJavaObject jsonObject = ttpConfiguration.Call<AndroidJavaObject>("getConfiguration", serviceName);
                        if(jsonObject != null)
                        {
                            return jsonObject.Call<string>("toString");
                        }
                    }
                }
                return null;
            }

            public AndroidJavaObject GetCurrentActivity()
            {
                return _activityObject;
            }

            public bool IsConnectedToTheInternet()
            {
                if (ServiceManager != null)
                {
                    AndroidJavaObject ttpConnectivityManager = ServiceManager.Call<AndroidJavaObject>("getConnectivityManager");
                    if(ttpConnectivityManager != null)
                    {
                        return ttpConnectivityManager.Call<bool>("isConnectedToTheInternet");
                    }
                }
                return false;
            }
        }
#endif

        private class EditorImpl : ITTPCore, ITTPInternalService, ITTPCoreInternal
        {
            public void Setup() {

				System.Type privacySettingsClsType = System.Type.GetType("Tabtale.TTPlugins.TTPPrivacySettings");
				if (privacySettingsClsType != null)
				{
					MethodInfo method = privacySettingsClsType.GetMethod("TriggerOnConsentModeReady", BindingFlags.NonPublic | BindingFlags.Static);
					if (method != null)
					{
						method.Invoke(null, null);
					}
				}

#if UNITY_EDITOR
                System.Type bannersClsType = System.Type.GetType("Tabtale.TTPlugins.TTPBanners");
                if (bannersClsType != null)
                {
                    MethodInfo method = bannersClsType.GetMethod("TriggerOnBannersReady", BindingFlags.NonPublic | BindingFlags.Static);
                    if (method != null)
                    {
                        method.Invoke(null, null);
                    }
                }
#endif

            }
            public bool OnBackPressed() {
                bool rateUsHandledBackPress = false;
                System.Type rateUsClsType = System.Type.GetType("Tabtale.TTPlugins.TTPRateUs");
                if (rateUsClsType != null)
                {
                    MethodInfo method = rateUsClsType.GetMethod("HandleAndroidBackPressed", BindingFlags.NonPublic | BindingFlags.Static);
                    if (method != null)
                    {
                        rateUsHandledBackPress = (bool)method.Invoke(null, null);
                    }
                }
                return rateUsHandledBackPress;
            }
#if UNITY_ANDROID
            public AndroidJavaObject GetServiceJavaObject(string serviceClassPath) { return null;}
#endif
#if UNITY_IOS
            public void CallCrash()
            {
                Debug.Log("TTPCore::EditorImpl:CallCrash");
            }
#endif
            public string GetPackageInfo()
            {
                return "";
            }
            public string GetConfigurationJson(string serviceName)
            {
                return TTPUtils.ReadStreamingAssetsFile("ttp/configurations/" + serviceName + ".json");
            }

            public AndroidJavaObject GetCurrentActivity()
            {
                return null;
            }

            public bool IsConnectedToTheInternet()
            {
                return true;
            }
        }

		private static ITTPInternalService _impl;
		public static ITTPInternalService Impl {
			get {

                if (_impl == null) {
                    if (DevMode)
                    {
                        _impl = new EditorImpl();
                    }
                    else if (UnityEngine.Application.platform == UnityEngine.RuntimePlatform.Android ||
					    UnityEngine.Application.platform == UnityEngine.RuntimePlatform.IPhonePlayer) {
#if UNITY_ANDROID
						_impl = new AndroidImpl ();
#endif
#if UNITY_IOS && !TTP_DEV_MODE
						_impl = new IosImpl();
#endif
                    }
                    else {
						_impl = new EditorImpl ();
					}
				}
				if (_impl == null) {
					Debug.LogError ("TTPCore::Impl: failed to create native impl");
				}
				return _impl;
			}
		}

        /// <summary>
        /// Use this class to have events coming back from native code
        /// </summary>
        public class TTPGameObject : MonoBehaviour
        {
            /// <summary>
            /// Apllication became active event
            /// </summary>
            public event System.Action OnApplicationFocusEvent;

            /// <summary>
            /// Application paused event
            /// </summary>
            public event System.Action OnApplicationPauseEvent;

            private void Start()
            {
                DontDestroyOnLoad(this);
            }

            private void OnApplicationFocus(bool focus)
            {
                Debug.Log("TTPCore::TTPGameObject:OnApplicationFocus:focus=" + focus);
                if (focus && OnApplicationFocusEvent != null)
                    OnApplicationFocusEvent();
            }

            private void OnApplicationPause(bool pause)
            {
                Debug.Log("TTPCore::TTPGameObject:OnApplicationPause:pause=" + pause);
                if (pause && OnApplicationPauseEvent != null)
                    OnApplicationPauseEvent();
            }

            private void OnDestroy()
            {
                Debug.Log("TTPCore::TTPGameObject:OnDestroy:");
            }
        }

        private static GameObject _ttpGameObject;
        private static GameObject TTPluginsGameObject
        {
            get
            {
                if (_ttpGameObject == null)
                {
                    _ttpGameObject = new GameObject("TTPluginsGameObject");
                    _ttpGameObject.AddComponent<TTPGameObject>();
                }
                return _ttpGameObject;
            }
        }
        [Preserve]
        static GameObject GetTTPGameObject()
        {
            return TTPluginsGameObject;
        }

        private static TTPSoundMgr _soundMgr;

        /// <summary>
        /// A singleton of sound manager
        /// </summary>
        public static ITTPInternalService SoundMgr
        {
            get
            {
                if (_soundMgr == null)
                    _soundMgr = new TTPSoundMgr();
                return _soundMgr;
            }
        }

        /// <summary>
        /// This class provides sound management for interstitials, banners and rewarded ads
        /// </summary>
        public class TTPSoundMgr : ITTPInternalService
        {
            /// <summary>
            /// Enumeration of callers interstitials, banners and rewarded ads
            /// </summary>
            public enum Caller
            {
                INTERSTITIAL, REWARDED_ADS, BANNERS, OPEN_ADS, REWARDED_INTER
            }

            private Dictionary<Caller, bool> _musicPauseDic;

            /// <summary>
            /// Notify t pause or resume music for required caller
            /// </summary>
            /// <param name="pause">True - if needs to pause music</param>
            /// <param name="caller">Kind of caller interstitials, banners and rewarded ads</param>
            public void PauseGameMusic(bool pause, Caller caller)
            {
                if (_musicPauseDic == null)
                {
                    _musicPauseDic = new Dictionary<Caller, bool>();
                }
                _musicPauseDic[caller] = pause;
                bool allUnpaused = true;
                foreach (KeyValuePair<Caller, bool> kvPair in _musicPauseDic)
                {
                    Debug.Log("TTPSoundMgr::PauseGameMusic: current music pause dictionary entry :: " + kvPair.Key + ", " + kvPair.Value);
                    if (kvPair.Value)
                    {
                        allUnpaused = false;
                    }

                }
                bool shouldCallEvent = pause || (!pause && allUnpaused);
                if (shouldCallEvent)
                {
                    if (PauseGameMusicEvent != null)
                        PauseGameMusicEvent(pause);
                }
            }
        }

        /// <summary>
        /// This class provides notifications about changes using events.
        /// Add this class as a unity component for compatibility with SendUnityMessage.
        /// </summary>
        public class CoreDelegate : MonoBehaviour
        {
            /// <summary>
            /// Notify about new session
            /// </summary>
            /// <param name="message">A message for new session</param>
            public void OnNewTTPSession(string message)
            {
                Debug.Log("CoreDelegate:: OnNewTTPSession");
                if(OnNewTTPSessionEvent != null)
                {
                    OnNewTTPSessionEvent();
                }

            }
        }
    }
}
