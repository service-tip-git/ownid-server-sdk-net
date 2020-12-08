namespace OwnID.Flow.Steps
{
    /// <summary>
    ///     Describes step action that should be performed as a result
    /// </summary>
    public enum ActionType
    {
        Polling,
        Callback,
        Finish,
        GoToNext
    }
}