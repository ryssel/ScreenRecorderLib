#nullable disable
namespace TestRepetitiveRecordings
{
    public static class PathHelper
    {
        public static void LogRotate(string path, int maxFiles = 10)
        {
            if (!File.Exists(path))
                return;

            var dir = Path.GetDirectoryName(path);
            var fn = Path.GetFileNameWithoutExtension(path);
            var ext = Path.GetExtension(path);

            var paths = new List<string> { path };
            paths.AddRange(Enumerable.Range(0, maxFiles).Select(i => Path.Combine(dir, fn + $".{i}{ext}")));
            Stack<(string from, string to)> moves = new Stack<(string, string)>();
            for (int i = 0; i < paths.Count - 1; i++)
            {
                moves.Push((paths[i], paths[i + 1]));
                if (File.Exists(paths[i]) && !File.Exists(paths[i + 1]))
                {
                    break;
                }
            }

            while (moves.Count > 0)
            {
                var move = moves.Pop();
                File.Copy(move.from, move.to, true);
                File.Delete(move.from);
            }
        }
    }
}
