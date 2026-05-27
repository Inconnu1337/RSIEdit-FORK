using System;
using System.Collections.Generic;
using Avalonia.Threading;

namespace Editor.Animation;

public interface IAnimatable
{
    void Animate(TimeSpan delta);
}

public static class AnimationTicker
{
    private static readonly TimeSpan Interval = TimeSpan.FromMilliseconds(50);
    private static readonly List<WeakReference<IAnimatable>> Subscribers = new();
    private static DispatcherTimer? _timer;
    private static DateTime _lastTick;

    public static void Subscribe(IAnimatable target)
    {
        EnsureStarted();
        Subscribers.Add(new WeakReference<IAnimatable>(target));
    }

    public static void Unsubscribe(IAnimatable target)
    {
        for (var i = Subscribers.Count - 1; i >= 0; i--)
        {
            if (!Subscribers[i].TryGetTarget(out var existing) || ReferenceEquals(existing, target))
            {
                Subscribers.RemoveAt(i);
            }
        }
    }

    private static void EnsureStarted()
    {
        if (_timer != null)
            return;

        _lastTick = DateTime.UtcNow;
        _timer = new DispatcherTimer(Interval, DispatcherPriority.Background, OnTick);
        _timer.Start();
    }

    private static void OnTick(object? sender, EventArgs e)
    {
        var now = DateTime.UtcNow;
        var delta = now - _lastTick;
        _lastTick = now;

        for (var i = Subscribers.Count - 1; i >= 0; i--)
        {
            if (Subscribers[i].TryGetTarget(out var target))
            {
                try
                {
                    target.Animate(delta);
                }
                catch
                {
                    // ignored
                }
            }
            else
            {
                Subscribers.RemoveAt(i);
            }
        }
    }
}
