using System;
using Avalonia.Media.Imaging;
using Editor.Animation;
using Editor.Extensions;
using Editor.Models.RSI;
using Microsoft.Toolkit.Diagnostics;
using ReactiveUI;
using SixLabors.ImageSharp.Processing;
using ImageSharpSize = SixLabors.ImageSharp.Size;

namespace Editor.ViewModels;

public class RsiStateViewModel : ViewModelBase, IAnimatable
{
    private static readonly ResizeOptions PreviewResizeOptions = new()
    {
        Mode = ResizeMode.Max,
        Position = AnchorPositionMode.Center,
        Size = new ImageSharpSize(96, 96),
        Sampler = KnownResamplers.NearestNeighbor
    };

    private string _name;

    private Bitmap[]? _animationFrames;
    private float[]? _animationDelays;
    private int _currentFrame;
    private float _elapsed;

    public RsiStateViewModel(RsiImage image)
    {
        Guard.IsNotNull(image, "image");
        Image = image;
        _name = image.State.Name;

        TryStartAnimation();
    }

    public RsiImage Image { get; }

    public string Name
    {
        get => _name;
        set
        {
            this.RaiseAndSetIfChanged(ref _name, value);
            Image.State.Name = value;
        }
    }

    public void RefreshAnimation()
    {
        AnimationTicker.Unsubscribe(this);
        _animationFrames = null;
        _animationDelays = null;
        _currentFrame = 0;
        _elapsed = 0f;

        TryStartAnimation();
    }

    private void TryStartAnimation()
    {
        var state = Image.State;
        if (state.DelayLength <= 1 || state.Delays is not { Count: > 0 } delays || delays[0].Count <= 1)
            return;

        var southDelays = delays[0];
        var frameCount = southDelays.Count;
        var bitmaps = new Bitmap[frameCount];
        var validFrames = 0;

        for (var i = 0; i < frameCount; i++)
        {
            var frame = state.Frames[0, i];
            if (frame == null)
                continue;

            bitmaps[i] = frame.ToBitmap(PreviewResizeOptions);
            validFrames++;
        }

        if (validFrames <= 1)
            return;

        _animationFrames = bitmaps;
        _animationDelays = new float[frameCount];
        for (var i = 0; i < frameCount; i++)
            _animationDelays[i] = southDelays[i];

        AnimationTicker.Subscribe(this);
    }

    public void Animate(TimeSpan delta)
    {
        if (_animationFrames == null || _animationDelays == null)
            return;

        _elapsed += (float) delta.TotalSeconds;

        var currentDelay = GetDelay(_currentFrame);
        var guard = 0;
        while (_elapsed >= currentDelay && guard++ < _animationFrames.Length)
        {
            _elapsed -= currentDelay;
            _currentFrame = (_currentFrame + 1) % _animationFrames.Length;
            currentDelay = GetDelay(_currentFrame);
        }

        var next = _animationFrames[_currentFrame];
        if (next != null && !ReferenceEquals(Image.Preview, next))
            Image.Preview = next;
    }

    private float GetDelay(int index)
    {
        var d = _animationDelays![index];
        return d > 0 ? d : 0.1f;
    }
}
