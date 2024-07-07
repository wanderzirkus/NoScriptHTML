public static class Copy
{
    public static void Process(DirectoryInfo source, DirectoryInfo target)
    {
        if (Directory.Exists(target.FullName) == false)
        {
            Directory.CreateDirectory(target.FullName);
        }

        foreach (FileInfo fi in source.GetFiles())
        {
            if (fi.Attributes.HasFlag(FileAttributes.Hidden))
            {
                continue;
            }
            fi.CopyTo(Path.Combine(target.ToString(), fi.Name), true);
        }

        foreach (DirectoryInfo diSourceDir in source.GetDirectories())
        {
            if (diSourceDir.Attributes.HasFlag(FileAttributes.Hidden))
            {
                continue;
            }
            DirectoryInfo nextTargetDir = target.CreateSubdirectory(diSourceDir.Name);
            Process(diSourceDir, nextTargetDir);
        }
    }
}