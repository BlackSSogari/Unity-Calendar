using UnityEngine;
using System.Collections;
using System;
using UnityEngine.Events;
using UnityEngine.UI;

public class UISpriteAnimation : MonoBehaviour
{

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
    public Image m_Target = null;

    public Sprite[] m_SpriteArray;

    public float m_Delay = 0;
    public float m_Duration = 1f;

    public enumAnimationPlayType m_PlayType = enumAnimationPlayType.ONCE;
    public enumAnimationPlayDirection m_PlayDirection = enumAnimationPlayDirection.Forward;

    public bool m_IsAutoRun = false;

    #region Private

    private bool autoRunStarted = false;
    private bool isRunning = false;

    #endregion
    //=========================================================================
    #region Event

    [Serializable]
    public class UISpriteAnimationEvent : UnityEvent { }

    //[Serializable]
    //public class UISpriteAnimationFrameEvent : UnityEvent<int> { }

    [SerializeField]
    public UISpriteAnimationEvent onSpriteAnimationStart = new UISpriteAnimationEvent();
    [SerializeField]
    public UISpriteAnimationEvent onSpriteAnimationEnded = new UISpriteAnimationEvent();
    //[SerializeField]
    //public UISpriteAnimationFrameEvent onSpriteAnimationFrameChanged = new UISpriteAnimationFrameEvent();
    public Action<int> onSpriteAnimationFrameChanged = null;

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

    public int SpriteCount 
    {
        get 
        {
            if (m_SpriteArray == null || m_SpriteArray.Length <= 0)
                return 0;
            return m_SpriteArray.Length; 
        }
    }

    public RectTransform RectTransform 
    {
        get { return (RectTransform)this.transform; }
    }

    #endregion
    //=========================================================================
    #region Unity events
                
    void LateUpdate()
    {
        if (this.AutoRun && !this.IsPlaying && !this.autoRunStarted)
        {
            this.autoRunStarted = true;
            this.Play();
        }
    }

    #endregion
    //=========================================================================
    #region Member

    public bool ValidTarget()
    {
        if (m_SpriteArray == null || m_SpriteArray.Length <= 0)
            return false;
        if (m_Target == null)
            return false;

        return true;
    }

    public void Play()
    {
        if (!ValidTarget())
            this.Stop();

        if (this.IsPlaying)
            this.Stop();

        if (!enabled || !gameObject.activeSelf || !gameObject.activeInHierarchy)
            return;

        StartCoroutine("Execute");
    }

    public void Stop()
    {
        if (!isRunning)
            return;

        StopCoroutine(Execute());
        isRunning = false;

        onStopped();
    }

    private IEnumerator Execute()
    {
        if (!ValidTarget())
            yield break;
        
        this.isRunning = true;

        onStarted();

        var length = Mathf.Max(m_Duration, 0.03f);

        var startTime = Time.realtimeSinceStartup;

        var direction = (this.m_PlayDirection == enumAnimationPlayDirection.Forward) ? 1 : -1;
        var lastFrameIndex = (direction == 1) ? 0 : SpriteCount - 1;

        setFrame(lastFrameIndex);

        while (true)
        {
            yield return null;

            var maxFrameIndex = SpriteCount - 1;

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
                        onStopped();
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
        frameIndex = Mathf.Max(0, Mathf.Min(frameIndex, SpriteCount - 1));

        this.m_Target.overrideSprite = m_SpriteArray[frameIndex];

        onChangeFrame(frameIndex);
    }

    private void onStarted()
    {
        if (onSpriteAnimationStart.GetPersistentEventCount() > 0) onSpriteAnimationStart.Invoke();
    }

    private void onStopped()
    {
        if (onSpriteAnimationEnded.GetPersistentEventCount() > 0) onSpriteAnimationEnded.Invoke();
    }

    private void onChangeFrame(int frameIndex)
    {
        //if (onSpriteAnimationFrameChanged.GetPersistentEventCount() > 0) onSpriteAnimationFrameChanged.Invoke(frameIndex);
        if (onSpriteAnimationFrameChanged != null) onSpriteAnimationFrameChanged(frameIndex);
    }

    #endregion
}
