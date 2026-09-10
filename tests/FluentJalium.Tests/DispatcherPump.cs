using Jalium.UI.Threading;

namespace FluentJalium.Tests;

/// <summary>
/// Resumes await continuations that <see cref="Jalium.UI.Application"/> posts to the UI dispatcher.
/// </summary>
/// <remarks>
/// Creating an <see cref="Jalium.UI.Application"/> installs a dispatcher-backed
/// <see cref="System.Threading.SynchronizationContext"/>, so a continuation only runs while a
/// dispatcher frame is being pumped. Headless tests never start the real message loop, therefore
/// awaiting a control task directly would block until the test host gives up.
/// </remarks>
internal static class DispatcherPump
{
    public static Task<T> Run<T>(Task<T> task, int timeoutMilliseconds = 5000)
    {
        PumpUntilCompleted(task, timeoutMilliseconds);
        return task;
    }

    public static Task Run(Task task, int timeoutMilliseconds = 5000)
    {
        PumpUntilCompleted(task, timeoutMilliseconds);
        return task;
    }

    private static void PumpUntilCompleted(Task task, int timeoutMilliseconds)
    {
        if (task.IsCompleted)
        {
            return;
        }

        var frame = new DispatcherFrame();
        Action<Task, object?> stop = static (_, state) => ((DispatcherFrame)state!).Continue = false;

        task.ContinueWith(stop, frame, CancellationToken.None, TaskContinuationOptions.None, TaskScheduler.Default);
        Task.Delay(timeoutMilliseconds).ContinueWith(stop, frame, CancellationToken.None, TaskContinuationOptions.None, TaskScheduler.Default);

        Dispatcher.PushFrame(frame);

        if (!task.IsCompleted)
        {
            throw new TimeoutException(
                $"The dispatcher stayed idle for {timeoutMilliseconds} ms without completing {task}.");
        }
    }
}
