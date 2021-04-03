namespace TehGM.Fsriev
{
    public enum CommandOutputMode
    {
        /// <summary>Show command output using <see cref="System.Console"/> class.</summary>
        Console = 1 << 0,
        /// <summary>Show command output using the logging library.</summary>
        Log = 1 << 1
    }
}
