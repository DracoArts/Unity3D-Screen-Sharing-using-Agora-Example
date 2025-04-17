
# Welcome to DracoArts

![Logo](https://dracoarts-logo.s3.eu-north-1.amazonaws.com/DracoArts.png)




# Screen Sharing using Agora in Unity 3D
Screen sharing functionality in Unity using Agora's SDK allows users to broadcast their device screens to other participants in real-time. This feature is essential for applications requiring remote collaboration, live demonstrations, or interactive broadcasting. The implementation differs between Android and iOS platforms due to their distinct operating system architectures and permission models.

## The screen sharing system consists of several key components:
## Agora SDK: 
- The core engine handling real-time communication

## Platform-Specific Capture Modules: 

- Different implementations for Android and iOS

## Unity Interface: 
- The bridge between native platform code and Unity's C# environment

## Permission Handlers: 
- Systems for managing platform-specific permissions

## Video/Audio Processing: 
- Components for encoding and transmitting screen content

# Prerequisites
- Before you begin, ensure you have:

- Unity 2019.4 LTS or later installed

- Agora developer account (sign up at agora.io)

- Basic knowledge of C# and Unity development

## Step 1: Set Up Agora in Unity

## 1.  Download Agora SDK
- Download the Agora Video SDK for Unity [the Agora website](https://docs.agora.io/en/sdks?platform=unity)

- Import the package into your Unity project (Assets > Import Package > Custom Package)

## 2. Configure Agora App ID & Token
 - Get your App ID from the Agora Console
 - Get your Token from the Agora Console
 - Create a script to store your App ID and Token


 # Android Implementation Description
### Key Characteristics
- Android screen sharing leverages the platform's built-in screen capture APIs, which require specific permissions and user consent. The implementation must account for:

### Permission Requirements:

- Foreground service permission for continuous capture

- Storage permissions for screen content access

- Camera/microphone permissions if combining with other media

###  Capture Mechanism:

- Uses Android's MediaProjection API

- Requires user interaction to initiate (cannot start programmatically)

- Creates a virtual display that mirrors screen content

### Performance Considerations:

- Screen resolution and frame rate impact performance

- Bitrate settings affect network usage

 - Orientation handling requires special attention
 ## Step 2: Step  Implement Screen Sharing
 ###  For Android
 #### Android Manifest Configuration
 - Create or modify your AndroidManifest.xml (in Assets/Plugins/Android)

 - Add required permissions:



       <uses-permission android:name="android.permission.READ_EXTERNAL_STORAGE" />
       <uses-permission android:name="android.permission.WRITE_EXTERNAL_STORAGE" />
       <uses-permission android:name="android.permission.RECORD_AUDIO" />
       <uses-permission android:name="android.permission.CAMERA" />
       <uses-permission android:name="android.permission.FOREGROUND_SERVICE" />
       <uses-permission android:name="android.permission.INTERNET" />
 ## iOS Implementation Description
### Key Characteristics
- iOS screen sharing requires a fundamentally different approach due to Apple's strict privacy controls:

### Broadcast Upload Extension:

- Separate process from main application

- Required for screen capture on iOS

- Runs in its own sandboxed environment

### System Integration:

- Appears in iOS Control Center during broadcasting

- User must explicitly enable sharing

 - Cannot be initiated programmatically

### Technical Constraints:

- Limited access to system resources

- Strict memory and CPU usage limits

- Requires careful handling of app state changes

### For iOS
### iOS Configuration
#### Modify your Info.plist to include necessary permissions:

       <key>NSCameraUsageDescription</key>
       <string>Camera permission description</string>
       <key>NSMicrophoneUsageDescription</key>
       <string>Microphone permission description</string>
      <key>UIApplicationSceneManifest</key>
       <dict>
       <key>UIApplicationSupportsMultipleScenes</key>
       <true/>
    </dict>

# Quality Control Parameters
-  Both implementations share common quality control aspects:

### Video Quality Settings:

- Resolution (typically 720p or 1080p)

- Frame rate (15-30 fps)

- Bitrate (adjustable based on network conditions)

### Audio Configuration:

- Sample rate (typically 44.1kHz or 48kHz)

- Channel configuration (mono/stereo)

- Bit depth (usually 16-bit)

### Network Adaptation:

- Automatic bitrate adjustment

- Frame prioritization during packet loss

- Error correction mechanisms

## Security and Privacy Aspects
### The implementation must address:

### User Consent:

- Clear indication when screen sharing is active

- Visual indicators during broadcast

- Easy termination mechanism

### Data Protection:

- Secure transmission of screen content

- Proper handling of sensitive information

- Compliance with platform-specific guidelines

### Platform Requirements:

- Android's foreground service notification

- iOS broadcast indicator

- Permission justification strings
## Usage/Examples
Simple Screen Share

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
## Images

#### TestScene
    

![](https://github.com/AzharKhemta/Gif-File-images/blob/main/ScreenShare%20Agora%20sdk.gif?raw=true)


## Authors

- [@MirHamzaHasan](https://github.com/MirHamzaHasan)
- [@WebSite](https://mirhamzahasan.com)


## 🔗 Links

[![linkedin](https://img.shields.io/badge/linkedin-0A66C2?style=for-the-badge&logo=linkedin&logoColor=white)](https://www.linkedin.com/company/mir-hamza-hasan/posts/?feedView=all/)
## Documentation

[Agora Sdk](https://docs.agora.io/en/)




## Tech Stack
**Client:** Unity,C#

**Plugin:**  Agora Unity Sdk



