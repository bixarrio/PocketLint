using PocketLint.Core.Data;
using PocketLint.Core.Logging;
using PocketLint.Core.TimeSystem;
using System;

namespace PocketLint.Core.Components;
public class Animator : Component
{
    #region Properties and Fields

    private bool _isPlaying;
    private int _currentFrame;
    private float _animationTime;
    private Animation _animation;
    private SpriteRenderer _spriteRenderer;

    public bool IsPlaying => _isPlaying;

    #endregion

    #region Public Methods

    public override void Init()
    {
        _spriteRenderer = EntityManager.GetComponent<SpriteRenderer>(EntityId);
        if (_spriteRenderer == null)
            Logger.Warn($"No SpriteRenderer found on {EntityManager.GetEntity(EntityId).Name}");
        _animation = null;
        _isPlaying = false;
        _currentFrame = 0;
        _animationTime = 0;
    }

    public override void Update()
    {
        if (!IsPlaying || _animation == null || _spriteRenderer == null) return;
        _animationTime += Time.DeltaTime;
        UpdateCurrentFrame();
        UpdateSpriteRenderer();
    }

    public void Play(Animation clip)
    {
        if (clip == null) return;

        if (_spriteRenderer == null) _spriteRenderer = EntityManager.GetComponent<SpriteRenderer>(EntityId);
        if (_spriteRenderer == null) return;

        Logger.Log($"Playing animation '{clip.Name}'");

        _animation = clip;
        _isPlaying = true;
        _currentFrame = 0;
        _animationTime = 0;
        UpdateSpriteRenderer();
    }

    public void Pause() => _isPlaying = false;
    public void Stop()
    {
        if (_animation == null || _spriteRenderer == null) return;
        _currentFrame = 0;
        _animationTime = 0;
        _isPlaying = false;
        UpdateSpriteRenderer();
    }

    #endregion

    #region Private Methods

    private void UpdateCurrentFrame()
    {
        var frameDuration = 1f / _animation.FrameRate;
        var frameCount = _animation.FrameCount;
        if (_animation.EndBehaviour == AnimationEndBehaviour.Loop)
            _currentFrame = (int)(_animationTime / frameDuration) % frameCount;
        else
        {
            _currentFrame = Math.Min((int)(_animationTime / frameDuration), frameCount - 1);
            if (_currentFrame == frameCount - 1)
            {
                if (_animation.EndBehaviour == AnimationEndBehaviour.Reset) _currentFrame = 0;
                _isPlaying = false;
            }
        }
    }

    private void UpdateSpriteRenderer()
    {
        if (_animation == null || _spriteRenderer == null) return;
        _spriteRenderer.SpriteIndex = _animation.FrameIndices[_currentFrame];
    }

    #endregion
}
