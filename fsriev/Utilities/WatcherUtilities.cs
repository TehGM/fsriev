using TehGM.Fsriev.Services;

namespace TehGM.Fsriev
{
    public static class WatcherUtilities
    {
        public const string DefaultName = "Unnamed Watcher";

        public static string GetName(this WatcherOptions options)
            => GetName(options.Name);

        public static string GetName(this Watcher watcher)
            => GetName(watcher.Name);

        public static string GetName(string name)
            => string.IsNullOrWhiteSpace(name) ? DefaultName : name.Trim();
    }
}
