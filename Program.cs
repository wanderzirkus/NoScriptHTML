using System.IO.Enumeration;
using System.Reflection;
using HtmlAgilityPack;

namespace HTML_Components;

class Program
{
    static void Main(string[] args)
    {
        string sourceDirectory = args.FirstOrDefault() ?? Environment.CurrentDirectory;
        string distDirectory = Environment.CurrentDirectory;
        if (args.Length >= 2)
        {
            distDirectory = args[1];
        }

        Console.WriteLine("Executing build in: " + sourceDirectory);

        Copy.Process(new DirectoryInfo(sourceDirectory), new DirectoryInfo(distDirectory));

        string[] fileNames = Directory.GetFiles(sourceDirectory, "*.html", SearchOption.AllDirectories);

        IDictionary<string, HtmlDocument> htmlDocuments = new Dictionary<string, HtmlDocument>();
        foreach(string fileName in fileNames)
        {
            Console.WriteLine("- " + fileName);
            HtmlDocument htmlDocument = new HtmlDocument();
            htmlDocument.Load(fileName);
            htmlDocuments.Add(Path.GetRelativePath(sourceDirectory, fileName), htmlDocument);
        }

        Substitution substitution = new Substitution();
        substitution.Process(htmlDocuments);
        foreach (KeyValuePair<string, HtmlDocument> htmlDocument in htmlDocuments)
        {
            if (htmlDocument.Value.DocumentNode.HasChildNodes)
            {
                htmlDocument.Value.Save(Path.Combine(distDirectory, htmlDocument.Key));
            } else {
                File.Delete(Path.Combine(distDirectory, htmlDocument.Key));
            }
        }
        Console.WriteLine("build complete.");
    }
}
