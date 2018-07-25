using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.IO;
using UnityEngine.Networking;

[RequireComponent(typeof(RawImage))]
public class TextureAnimation : MonoBehaviour {

    #region Enum

    public enum enumAnimationPlayType : int
    {
        ONCE = 0,
        LOOP = 1,
        PINGPONG = 2
    }

    public enum enumAnimationPlayDirection : int
    {
        Forward = 0,
        Reverse = 1
    }

    #endregion
    //=========================================================================

    public RawImage m_Target = null;
    public string m_TextureName;
    public int m_TextureCount;
    public AudioSource m_TargetSound;
    
    public float m_Delay = 0;
    public float m_Duration = 1f;

    public enumAnimationPlayType m_PlayType = enumAnimationPlayType.ONCE;
    public enumAnimationPlayDirection m_PlayDirection = enumAnimationPlayDirection.Forward;

    public bool m_IsAutoRun = false;

    #region Private

    private List<Texture2D> m_TextureArray = new List<Texture2D>();

    private bool autoRunStarted = false;
    private bool isRunning = false;

    #endregion
    //=========================================================================
    #region Event

    private Action OnStartAnimationAction = null;
    private Action OnEndAnimationAction = null;

    #endregion
    //=========================================================================
    #region Properties

    public bool AutoRun
    {
        get { return this.m_IsAutoRun; }
        set { this.m_IsAutoRun = value; }
    }

    public bool IsPlaying
    {
        get { return this.isRunning; }
    }

    public int TextureCount
    {
        get
        {
            if (m_TextureArray == null || m_TextureArray.Count <= 0)
                return 0;
            return m_TextureArray.Count;
        }
    }

    public RectTransform RectTransform
    {
        get { return (RectTransform)this.transform; }
    }

    public bool _isMute = false;
    public bool isMute
    {
        get { return _isMute; }
        set
        {
            _isMute = value;
            if (m_TargetSound != null)
                m_TargetSound.mute = _isMute;
        }
    }

    #endregion
    //=========================================================================
    #region Unity events

    private void Awake()
    {
        AspectRatioFitter ratioFitter = this.gameObject.AddComponent<AspectRatioFitter>();
        ratioFitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
        ratioFitter.aspectRatio = 1.777778f;
    }

    private void Start()
    {   
        if (this.AutoRun && !this.IsPlaying && !this.autoRunStarted)
        {
            this.autoRunStarted = true;
            this.Play(null, null);
        }
    }
        
    #endregion
    //=========================================================================
    #region Member

    private string getFileURL(string url)
    {
        if (!string.IsNullOrEmpty(url))
        {
            if (url.StartsWith("http"))
            {
                // from WEB
                return url;
            }
            else
            {
                // from StreamingAssets                    
#if (UNITY_EDITOR || UNITY_STANDALONE_WIN)
                return Path.Combine("file:///" + Application.streamingAssetsPath, url);
#elif !UNITY_EDITOR && UNITY_IOS
                return Path.Combine("file:///" + Application.streamingAssetsPath, url);
#else
                return Path.Combine(Application.streamingAssetsPath, url);
#endif
            }
        }
        else
        {
            return url;
        }
    }

    private void LoadTexture(string texName, int texCnt, Action onComplete)
    {
        if (string.IsNullOrEmpty(texName))
        {
            Debug.LogErrorFormat("Invalid Texture name(path).");
            if (onComplete != null)
                onComplete();
            return;
        }
        if (texCnt <= 0)
        {
            Debug.LogErrorFormat("Invalid Texture count.");
            if (onComplete != null)
                onComplete();
            return;
        }

        StopAllCoroutines();
        StartCoroutine(CoLoadTexture(texName, texCnt, onComplete));
    }

    private IEnumerator CoLoadTexture(string _texName, int _texCnt, Action _onEnded)
    {
        m_TextureArray.Clear();
        float startTime = Time.realtimeSinceStartup;
        for (int i = 0; i < _texCnt; i++)
        {
            string fileName = string.Format("{0}-{1:D2}.png", _texName, i);            
            string filePath = getFileURL(fileName);

            //using (UnityWebRequest www = new UnityWebRequest(filePath))
            //{
            //    www.downloadHandler = new DownloadHandlerTexture(true);
            //    yield return www.Send();

            //    if (!www.isError)
            //    {                    
            //        m_TextureArray.Add(((DownloadHandlerTexture)www.downloadHandler).texture);
            //    }
            //    else
            //    {
            //        Rg.Log.WriteError("{0} load failed", fileName);
            //    }
            //}

            using (WWW www = new WWW(filePath))
            {
                yield return www;

                m_TextureArray.Add(www.texture);
            }
        }
        Debug.LogFormat("Loaded done : {0}", Time.realtimeSinceStartup - startTime);

        if (_onEnded != null)
            _onEnded();
    }

    public bool ValidTarget()
    {
        if (m_TextureArray == null || m_TextureArray.Count <= 0)
            return false;
        if (m_Target == null)
            return false;

        return true;
    }

    public void Play(Action OnStart, Action OnEnded)
    {
        if (!enabled || !gameObject.activeSelf || !gameObject.activeInHierarchy)
            return;

        if (this.IsPlaying)
        {
            this.Stop();
            return;
        }

        OnStartAnimationAction = OnStart;
        OnEndAnimationAction = OnEnded;

        LoadTexture(m_TextureName, m_TextureCount, () =>
        {
            StartCoroutine("Execute");
        });
    }

    public void Stop()
    {
        if (!isRunning)
            return;

        StopCoroutine(Execute());
        isRunning = false;        
    }

    private IEnumerator Execute()
    {
        if (!ValidTarget())
            yield break;

        this.isRunning = true;
                
        var length = Mathf.Max(m_Duration, 0.03f);

        var startTime = Time.realtimeSinceStartup;

        var direction = (this.m_PlayDirection == enumAnimationPlayDirection.Forward) ? 1 : -1;
        var lastFrameIndex = (direction == 1) ? 0 : TextureCount - 1;

        if (OnStartAnimationAction != null)
            OnStartAnimationAction();

        m_Target.color = Color.white;

        setFrame(lastFrameIndex);

        if (m_TargetSound != null)
            m_TargetSound.Play();

        while (true)
        {
            yield return null;

            var maxFrameIndex = TextureCount - 1;

            var timeNow = Time.realtimeSinceStartup;
            var elapsed = timeNow - startTime;

            // Determine the index of the current animation frame
            var frameIndex = Mathf.RoundToInt(Mathf.Clamp01(elapsed / length) * maxFrameIndex);

            if (elapsed >= length)
            {
                switch (this.m_PlayType)
                {
                    case enumAnimationPlayType.LOOP:
                        startTime = timeNow;
                        frameIndex = 0;
                        break;
                    case enumAnimationPlayType.PINGPONG:
                        startTime = timeNow;
                        direction *= -1;
                        frameIndex = 0;
                        break;
                    case enumAnimationPlayType.ONCE:
                        isRunning = false;
                        if (OnEndAnimationAction != null)
                            OnEndAnimationAction();
                        yield break;
                }

            }

            if (direction == -1)
            {
                frameIndex = maxFrameIndex - frameIndex;
            }

            // Set the current animation frame on the sprite
            if (lastFrameIndex != frameIndex)
            {
                lastFrameIndex = frameIndex;
                setFrame(frameIndex);
            }

        }

    }

    private void setFrame(int frameIndex)
    {
        if (!ValidTarget())
            return;

        // Clamp the frame index
        frameIndex = Mathf.Max(0, Mathf.Min(frameIndex, TextureCount - 1));

        this.m_Target.texture = m_TextureArray[frameIndex];        
    }
    
    #endregion
}
