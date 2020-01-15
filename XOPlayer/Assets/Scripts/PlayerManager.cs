using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#else
using System.Runtime.InteropServices;
#endif
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class PlayerManager : MonoBehaviour
{
    public GameObject noFileBar;
    public GameObject toolBar;
    public Camera mainCamera;
    public GameObject normalPlayer;
    public Material video2DMaterial;
    public Material videoPanoramicMaterial;
    public Slider slider;

    enum PlayMode
    {
        kNormal,
        kPanoramic180,
        kPanoramic360,
    }

#if !UNITY_EDITOR
    [DllImport("user32.dll")]
    private static extern void OpenFileDialog();
#endif
    private VideoPlayer mPlayer;
    private RenderTexture mVideoTexture;
    private PlayMode mMode = PlayMode.kNormal;
    private bool mIgnoreValueChanged = false;

    // Start is called before the first frame update
    void Start()
    {
        toolBar.SetActive(false);
        mPlayer = GetComponent<VideoPlayer>();
        mPlayer.prepareCompleted += onPrepareCompleted;
        mPlayer.loopPointReached += onLoopPointReached;
    }

    // Update is called once per frame
    void Update()
    {
        mIgnoreValueChanged = true;
        if (mPlayer.length <= 0)
        {
            slider.value = 0;
        }
        else
        {
            slider.value = (float)(mPlayer.time / mPlayer.length);
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
        switch (mMode)
        {
            case PlayMode.kNormal:
                mMode = PlayMode.kPanoramic180;
                break;
            case PlayMode.kPanoramic180:
                mMode = PlayMode.kPanoramic360;
                break;
            case PlayMode.kPanoramic360:
                mMode = PlayMode.kNormal;
                break;
        }
        applyMode();
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
            mPlayer.time = mPlayer.length * slider.value;
        }
    }
}
