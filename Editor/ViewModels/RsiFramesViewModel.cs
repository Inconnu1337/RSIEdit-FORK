using System;
using Avalonia.Media.Imaging;
using Editor.Animation;
using SpaceWizards.RsiLib.Directions;
using ReactiveUI;

namespace Editor.ViewModels;

public class RsiFramesViewModel : ViewModelBase, IAnimatable
{
    private const int DirectionCount = 8;

    private Bitmap _full;
    private Bitmap _south;
    private Bitmap _north;
    private Bitmap _east;
    private Bitmap _west;
    private Bitmap _southEast;
    private Bitmap _southWest;
    private Bitmap _northEast;
    private Bitmap _northWest;
    private bool _showFull;
    private bool _showCardinals;
    private bool _showDiagonals;

    private readonly Bitmap?[]?[] _animationFrames = new Bitmap?[]?[DirectionCount];    private readonly float[][] _animationDelays = new float[DirectionCount][];
    private readonly int[] _currentFrame = new int[DirectionCount];
    private readonly float[] _elapsed = new float[DirectionCount];
    private bool _animationActive;

    public RsiFramesViewModel(Bitmap full, DirectionType? direction)
    {
        _full = full;
        _south = full;
        _north = full;
        _east = full;
        _west = full;
        _southEast = full;
        _southWest = full;
        _northEast = full;
        _northWest = full;

        SetDirections(direction);
        AnimationTicker.Subscribe(this);
    }

    public bool ShowFull
    {
        get => _showFull;
        set => this.RaiseAndSetIfChanged(ref _showFull, value);
    }

    public bool ShowCardinals
    {
        get => _showCardinals;
        set => this.RaiseAndSetIfChanged(ref _showCardinals, value);
    }

    public bool ShowDiagonals
    {
        get => _showDiagonals;
        set => this.RaiseAndSetIfChanged(ref _showDiagonals, value);
    }

    public Bitmap Full
    {
        get => _full;
        set => this.RaiseAndSetIfChanged(ref _full, value);
    }

    public Bitmap South
    {
        get => _south;
        set => this.RaiseAndSetIfChanged(ref _south, value);
    }

    public Bitmap North
    {
        get => _north;
        set => this.RaiseAndSetIfChanged(ref _north, value);
    }

    public Bitmap East
    {
        get => _east;
        set => this.RaiseAndSetIfChanged(ref _east, value);
    }

    public Bitmap West
    {
        get => _west;
        set => this.RaiseAndSetIfChanged(ref _west, value);
    }

    public Bitmap SouthEast
    {
        get => _southEast;
        set => this.RaiseAndSetIfChanged(ref _southEast, value);
    }

    public Bitmap SouthWest
    {
        get => _southWest;
        set => this.RaiseAndSetIfChanged(ref _southWest, value);
    }

    public Bitmap NorthEast
    {
        get => _northEast;
        set => this.RaiseAndSetIfChanged(ref _northEast, value);
    }

    public Bitmap NorthWest
    {
        get => _northWest;
        set => this.RaiseAndSetIfChanged(ref _northWest, value);
    }

    public void Set(Direction direction, Bitmap image)
    {
        switch (direction)
        {
            case Direction.South:
                Full = image;
                South = image;
                break;
            case Direction.North:
                North = image;
                break;
            case Direction.East:
                East = image;
                break;
            case Direction.West:
                West = image;
                break;
            case Direction.SouthEast:
                SouthEast = image;
                break;
            case Direction.SouthWest:
                SouthWest = image;
                break;
            case Direction.NorthEast:
                NorthEast = image;
                break;
            case Direction.NorthWest:
                NorthWest = image;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
        }
    }

    public void ClearAnimation()
    {
        _animationActive = false;
        for (var i = 0; i < DirectionCount; i++)
        {
            _animationFrames[i] = null;
            _animationDelays[i] = Array.Empty<float>();
            _currentFrame[i] = 0;
            _elapsed[i] = 0f;
        }
    }

    public void SetAnimation(Direction direction, Bitmap?[] frames, float[] delays)
    {
        var i = (int) direction;
        _animationFrames[i] = frames;
        _animationDelays[i] = delays;
        _currentFrame[i] = 0;
        _elapsed[i] = 0f;
        _animationActive = true;
    }

    public void Animate(TimeSpan delta)
    {
        if (!_animationActive)
            return;

        var seconds = (float) delta.TotalSeconds;

        for (var dir = 0; dir < DirectionCount; dir++)
        {
            var frames = _animationFrames[dir];
            var delays = _animationDelays[dir];
            if (frames == null || frames.Length <= 1 || delays.Length == 0)
                continue;

            _elapsed[dir] += seconds;
            var currentDelay = GetDelay(delays, _currentFrame[dir]);
            var guard = 0;
            var changed = false;
            while (_elapsed[dir] >= currentDelay && guard++ < frames.Length)
            {
                _elapsed[dir] -= currentDelay;
                _currentFrame[dir] = (_currentFrame[dir] + 1) % frames.Length;
                currentDelay = GetDelay(delays, _currentFrame[dir]);
                changed = true;
            }

            if (!changed)
                continue;

            var next = frames[_currentFrame[dir]];
            if (next != null)
                Set((Direction) dir, next);
        }
    }

    private static float GetDelay(float[] delays, int index)
    {
        if (index >= delays.Length)
            return 0.1f;
        var d = delays[index];
        return d > 0 ? d : 0.1f;
    }

    public void SetDirections(DirectionType? direction)
    {
        ShowFull = direction == DirectionType.None;
        ShowCardinals = direction == DirectionType.Cardinal || direction == DirectionType.Diagonal;
        ShowDiagonals = direction == DirectionType.Diagonal;
    }
}
