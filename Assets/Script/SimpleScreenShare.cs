using System;
using UnityEngine;
using UnityEngine.UI;
using Agora.Rtc;

public class SimpleScreenShare : MonoBehaviour
{
    [Header("Agora Settings")]
    [SerializeField] private string _appID = "YOUR_APP_ID";
    [SerializeField] private string _channelName = "test";
    [SerializeField] private string _token = "";

    [Header("UI Elements")]
    [SerializeField] private RawImage _screenView;
    [SerializeField] private Button _startShareBtn;
    [SerializeField] private Button _stopShareBtn;

    private IRtcEngine _rtcEngine;
    private bool _isSharing = false;
    private VideoSurface _videoSurface;

    private void Start()
    {
        _startShareBtn.onClick.AddListener(StartScreenShare);
        _stopShareBtn.onClick.AddListener(StopScreenShare);
        _stopShareBtn.interactable = false;

        InitAgoraEngine();
    }

    private void InitAgoraEngine()
    {
        _rtcEngine = RtcEngine.CreateAgoraRtcEngine();
        RtcEngineContext context = new RtcEngineContext(
            _appID, 
            0,
            CHANNEL_PROFILE_TYPE.CHANNEL_PROFILE_LIVE_BROADCASTING,
            AUDIO_SCENARIO_TYPE.AUDIO_SCENARIO_DEFAULT
        );
        _rtcEngine.Initialize(context);
        _rtcEngine.SetClientRole(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);
    }

    private void StartScreenShare()
    {
        if (_isSharing) return;

        // Join channel
        _rtcEngine.JoinChannel(_token, _channelName);

        // Start screen capture
#if UNITY_ANDROID || UNITY_IPHONE
        var parameters = new ScreenCaptureParameters2
        {
            captureAudio = true,
            captureVideo = true
        };
        _rtcEngine.StartScreenCapture(parameters);
#else
        // For Windows/Mac - capture primary display
        _rtcEngine.StartScreenCaptureByDisplayId(0, default(Rectangle), 
            new ScreenCaptureParameters { 
                captureMouseCursor = true, 
                frameRate = 30 
            });
#endif

        // Set up video view
        SetupLocalVideo();

        _isSharing = true;
        _startShareBtn.interactable = false;
        _stopShareBtn.interactable = true;
    }

    private void StopScreenShare()
    {
        if (!_isSharing) return;

        // Disable video surface first
        if (_videoSurface != null)
        {
            _videoSurface.SetEnable(false);
        }

        // Stop screen capture and leave channel
        _rtcEngine.StopScreenCapture();
        _rtcEngine.LeaveChannel();

        // Clear the RawImage texture safely
        if (_screenView != null)
        {
            _screenView.texture = null;
        }

        _isSharing = false;
        _startShareBtn.interactable = true;
        _stopShareBtn.interactable = false;
    }

    private void SetupLocalVideo()
    {
        // Remove existing VideoSurface if it exists
        if (_videoSurface != null)
        {
            Destroy(_videoSurface);
        }

        // Create new VideoSurface
        _videoSurface = _screenView.gameObject.AddComponent<VideoSurface>();
        _videoSurface.SetForUser(0, "", VIDEO_SOURCE_TYPE.VIDEO_SOURCE_SCREEN);
        _videoSurface.SetEnable(true);
    }

    private void OnDestroy()
    {
        if (_isSharing)
        {
            StopScreenShare();
        }
        
        if (_rtcEngine != null)
        {
            _rtcEngine.Dispose();
            _rtcEngine = null;
        }
    }
}