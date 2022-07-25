namespace UAlbion.Formats.MapEvents;

public enum EventContextStatus
{
    Running,
    Waiting,
    Breakpoint, // Waiting for the debugger to proceed
    Ready,      // Wait complete, ready to transition back to Running
    Completing, // Set while the completion callback is in progress
    Complete
}