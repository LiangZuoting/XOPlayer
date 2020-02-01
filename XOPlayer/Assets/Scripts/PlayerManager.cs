using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class PlayerManager : MonoBehaviour
{
    public GameObject noFileBar;
    public GameObject toolBar;
    public GameObject playBtn;
    public GameObject pauseBtn;
    public GameObject stopBtn;
    public Camera mainCamera;
    public GameObject normalPlayer;
    public Material video2DMaterial;
    public Material videoPanoramicMaterial;
    public Slider progressSlider;
    public GameObject volumeBar;
    public Slider volumeSlider;
    public GameObject modePanel;

    enum PlayMode
    {
        kNormal,
        kPanoramic180,
        kPanoramic360,
    }

    private VideoPlayer mPlayer;
    private RenderTexture mVideoTexture;
    private PlayMode mMode = PlayMode.kNormal;
    private bool mIgnoreValueChanged = false;

    // Start is called before the first frame update
    void Start()
    {
        toolBar.SetActive(false);
        modePanel.SetActive(false);
        volumeBar.SetActive(false);
        mPlayer = GetComponent<VideoPlayer>();
        mPlayer.SetDirectAudioVolume(0, 0.5f);
        volumeSlider.value = 0.5f;
        mPlayer.prepareCompleted += onPrepareCompleted;
        mPlayer.loopPointReached += onLoopPointReached;
    }

    // Update is called once per frame
    void Update()
    {
        mIgnoreValueChanged = true;
        if (mPlayer.length <= 0)
        {
            progressSlider.value = 0;
        }
        else
        {
            progressSlider.value = (float)(mPlayer.time / mPlayer.length);
        }
        mIgnoreValueChanged = false;

        if (mMode != PlayMode.kNormal && Input.GetMouseButton(0))
        {
            mainCamera.gameObject.GetComponent<CameraManager>().RotateAround();
        }
    }

    public void OnOpenFile()
    {
#if UNITY_EDITOR
        var filePath = EditorUtility.OpenFilePanel("Choose a video file", "", "mp4,flv");
#else
        System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog();
        ofd.Filter = "Video Files(*.mp4;*.flv)|*.mp4;*.flv";
        ofd.RestoreDirectory = true;
        if (ofd.ShowDialog() != System.Windows.Forms.DialogResult.OK)
        {
            return;
        }
        var filePath = ofd.FileName;
#endif
        if (filePath.Length != 0)
        {
            noFileBar.SetActive(false);

            mMode = PlayMode.kNormal;
            mPlayer.source = VideoSource.Url;
            mPlayer.url = filePath;
            mPlayer.Prepare();
        }
    }

    public void OnSwitchMode()
    {
        volumeBar.SetActive(false);
        modePanel.SetActive(!modePanel.activeSelf);
    }

    private void applyMode()
    {
        if (mMode == PlayMode.kNormal)
        {
            RenderSettings.skybox = null;
            videoPanoramicMaterial.mainTexture = null;
            video2DMaterial.mainTexture = mVideoTexture;
            // 动态创建的RenderTexture，必须搭配动态设置的material!
            // 且不能放在Start()中设置！
            normalPlayer.GetComponent<Image>().material = video2DMaterial;
            normalPlayer.SetActive(true);
        }
        else
        {
            normalPlayer.SetActive(false);
            video2DMaterial.mainTexture = null;
            videoPanoramicMaterial.mainTexture = mVideoTexture;
            if (mMode == PlayMode.kPanoramic180)
            {
                videoPanoramicMaterial.SetFloat("_ImageType", 1f);
                
            }
            else
            {
                videoPanoramicMaterial.SetFloat("_ImageType", 0f);
            }
            RenderSettings.skybox = videoPanoramicMaterial;
        }
    }

    private void onPrepareCompleted(VideoPlayer videoPlayer)
    {
        toolBar.SetActive(true);
        playBtn.SetActive(false);
        pauseBtn.SetActive(true);
        mVideoTexture = new RenderTexture((int)mPlayer.width, (int)mPlayer.height, 0, RenderTextureFormat.ARGB32);
        mPlayer.targetTexture = mVideoTexture;
        applyMode();
        mPlayer.Play();
    }

    private void onLoopPointReached(VideoPlayer videoPlayer)
    {
        toolBar.SetActive(false);
        noFileBar.SetActive(true);
    }

    public void OnSliderValueChanged()
    {
        if (mPlayer.isPlaying && !mIgnoreValueChanged)
        {
            mPlayer.time = mPlayer.length * progressSlider.value;
        }
    }

    public void OnVolumeChanged()
    {
        mPlayer.SetDirectAudioVolume(0, volumeSlider.value);
    }

    public void OnBtn2D()
    {
        mMode = PlayMode.kNormal;
        applyMode();
        modePanel.SetActive(false);
    }

    public void OnBtn180()
    {
        mMode = PlayMode.kPanoramic180;
        applyMode();
        modePanel.SetActive(false);
    }

    public void OnBtn360()
    {
        mMode = PlayMode.kPanoramic360;
        applyMode();
        modePanel.SetActive(false);
    }

    public void OnResume()
    {
        if (mPlayer.isPrepared)
        {
            playBtn.SetActive(false);
            pauseBtn.SetActive(true);
            mPlayer.Play();
        }
        else
        {
            OnOpenFile();
        }
    }

    public void OnPause()
    {
        playBtn.SetActive(true);
        pauseBtn.SetActive(false);
        mPlayer.Pause();
    }

    public void OnStop()
    {
        noFileBar.SetActive(true);
        toolBar.SetActive(false);
        playBtn.SetActive(true);
        pauseBtn.SetActive(false);
        mPlayer.Stop();
        // 1.将画面重置为蓝色背景
        // 2.kNormal模式下，重置是必要的，否则重新打开一个文件后，画面不会更新。
        normalPlayer.GetComponent<Image>().material = null;
        RenderSettings.skybox = null;
    }

    public void OnVolume()
    {
        modePanel.SetActive(false);
        volumeBar.SetActive(!volumeBar.activeSelf);
    }

    public void OnPointerUp()
    {
        if (mPlayer.isPrepared)
        {
            modePanel.SetActive(false);
            volumeBar.SetActive(false);
            toolBar.SetActive(!toolBar.activeSelf);
        }
    }
}
