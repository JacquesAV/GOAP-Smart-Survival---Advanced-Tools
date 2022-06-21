using UnityEngine;

/// <summary>
/// Custom class that handles the logic for a simple deltaTime timer.
/// </summary>
public class CustomTimer : MonoBehaviour
{
    /// <summary>
    /// The time remaining for the timer.
    /// </summary>
    public float TimeRemaining { get; private set; } = 0;

    /// <summary>
    /// If the timer is currently running.
    /// </summary>
    public bool TimerIsRunning { get; private set; } = false;

    /// <summary>
    /// Update is called once per frame.
    /// </summary>
    private void Update()
    {
        if (TimerIsRunning)
        {
            RunTimer();
        }
    }

    /// <summary>
    /// Logic for the timer.
    /// </summary>
    private void RunTimer()
    {
        // Continue to reduce time for as long as time remains.
        if (TimeRemaining > 0)
        {
            TimeRemaining -= Time.deltaTime;
        }
        else
        {
            // Reset and stop the timer.
            TimeRemaining = 0;
            TimerIsRunning = false;
        }
    }

    /// <summary>
    /// Starts the timer with a given time.
    /// </summary>
    /// <param name="time">The time that should be used in the timer.</param>
    public void StartTimer(float time)
    {
        // Set the time and begin the timer.
        TimeRemaining = time;
        TimerIsRunning = true;
    }

    /// <summary>
    /// Stops the timer early.
    /// </summary>
    public void StopTimer() => TimerIsRunning = false;
}