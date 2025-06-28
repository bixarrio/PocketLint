using PocketLint.Core.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PocketLint.Core.Data;
public class Animation
{
    #region Properties and Fields

    private const float MAX_FRAMERATE = 60f;

    public string Name { get; private set; }
    public List<byte> FrameIndices { get; private set; }
    public float FrameRate { get; private set; }
    public AnimationEndBehaviour EndBehaviour { get; private set; }

    public int FrameCount => FrameIndices.Count;

    #endregion

    #region ctor

    public Animation(string name, List<byte> frameIndices, float frameRate, AnimationEndBehaviour endBehaviour)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name cannot be null or empty", nameof(name));
        if (frameIndices == null || frameIndices.Count == 0) throw new ArgumentException("Frame indices cannot be null or empty", nameof(frameIndices));
        if (frameRate < 0 || frameRate > MAX_FRAMERATE) throw new ArgumentException($"Frame rate must be between 0 and {MAX_FRAMERATE}", nameof(frameRate));
        if (frameIndices.Any(fi => fi < 0 || fi > SpriteSheet.SPRITE_COUNT - 1)) throw new ArgumentException($"All frame indices must be between 0 and {SpriteSheet.SPRITE_COUNT - 1}", nameof(frameIndices));

        Name = name;
        FrameIndices = frameIndices;
        FrameRate = frameRate;
        EndBehaviour = endBehaviour;
    }

    #endregion

    #region Public Methods

    public static List<byte> FromKeyframes(List<(int framePosition, byte index)> keyframes, int totalFrames)
    {
        if (keyframes == null || keyframes.Count == 0) throw new ArgumentException("Keyframes cannot be null or empty.", nameof(keyframes));
        if (totalFrames <= 0) throw new ArgumentException("TotalFrames must be positive.", nameof(totalFrames));
        if (keyframes.Any(kf => kf.index < 0 || kf.index > SpriteSheet.SPRITE_COUNT - 1)) throw new ArgumentException($"Keyframe indices must be between 0 and {SpriteSheet.SPRITE_COUNT - 1}.", nameof(keyframes));
        if (keyframes.Any(kf => kf.framePosition < 0 || kf.framePosition >= totalFrames)) throw new ArgumentException($"Frame positions must be between 0 and {totalFrames - 1}.", nameof(keyframes));
        if (keyframes.Select(kf => kf.framePosition).Distinct().Count() != keyframes.Count) throw new ArgumentException("Frame positions must be unique.", nameof(keyframes));
        if (keyframes[0].framePosition != 0) throw new ArgumentException("First keyframe must be at position 0.", nameof(keyframes));

        var frameIndices = new List<byte>();
        var sortedKeyframes = keyframes.OrderBy(kf => kf.framePosition).ToList();
        int currentKeyframeIndex = 0;

        for (int i = 0; i < totalFrames; i++)
        {
            if (currentKeyframeIndex < sortedKeyframes.Count - 1 && i >= sortedKeyframes[currentKeyframeIndex + 1].framePosition)
            {
                currentKeyframeIndex++;
            }
            frameIndices.Add(sortedKeyframes[currentKeyframeIndex].index);
        }

        return frameIndices;
    }

    #endregion
}
public enum AnimationEndBehaviour
{
    /// <summary>
    /// Loop the animation (default)
    /// </summary>
    Loop,
    /// <summary>
    /// Hold on the last frame
    /// </summary>
    Hold,
    /// <summary>
    /// Reset to first frame and stop
    /// </summary>
    Reset
}
