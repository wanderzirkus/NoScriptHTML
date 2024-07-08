public class Copy : ICopy
{
    public void Process(string sourcePath, string targetPath)
    {
        Process(new DirectoryInfo(sourcePath), new DirectoryInfo(targetPath));
    }

    public void Process(DirectoryInfo source, DirectoryInfo target)
    {
        if (Directory.Exists(target.FullName) == false)
        {
            Directory.CreateDirectory(target.FullName);
        }

        foreach (FileInfo fileInfo in source.GetFiles())
        {
            if (fileInfo.Attributes.HasFlag(FileAttributes.Hidden))
            {
                continue;
            }
            fileInfo.CopyTo(Path.Combine(target.ToString(), fileInfo.Name), true);
        }

        foreach (DirectoryInfo directory in source.GetDirectories())
        {
            if (directory.Attributes.HasFlag(FileAttributes.Hidden))
            {
                continue;
            }
            DirectoryInfo nextTargetDir = target.CreateSubdirectory(directory.Name);
            Process(directory, nextTargetDir);
        }
    }
}