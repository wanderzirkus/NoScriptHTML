public class Builder : IBuilder
{
    private readonly ICopy myCopy;
    private IEnumerable<IProcessFiles> myFileProcessors;

    public Builder(ICopy copy, IEnumerable<IProcessFiles> fileProcessors)
    {
        myCopy = copy;
        myFileProcessors = fileProcessors;
    }

    public void Build(string sourceDirectory, string distDirectory)
    {
        Console.WriteLine("Executing build in: " + sourceDirectory);

        myCopy.Process(sourceDirectory, distDirectory);
        foreach(IProcessFiles fileProcessor in myFileProcessors)
        {
            fileProcessor.Process(sourceDirectory, distDirectory);
        }
        
        Console.WriteLine("build complete.");
    }
}