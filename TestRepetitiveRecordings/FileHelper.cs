namespace TestRepetitiveRecordings;

static class FileHelper
{
    private static readonly NLog.Logger Log = NLog.LogManager.GetCurrentClassLogger();

    public static string EnsureExists(string path)
    {
        if (path == null) throw new ArgumentNullException(nameof(path));
        try
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, $"Unable to ensure folder exist: {path}");
        }

        return path;
    }
}