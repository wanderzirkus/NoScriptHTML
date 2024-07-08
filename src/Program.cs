using System.IO.Enumeration;
using System.Reflection;
using HtmlAgilityPack;
using Microsoft.Extensions.DependencyInjection;

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

        ServiceCollection services = new ServiceCollection();
        IServiceProvider serviceProvider = services
            .AddSingleton<ICopy, Copy>()
            .AddSingleton<IProcessFiles, Substitution>()
            .AddSingleton<IBuilder, Builder>()
            .BuildServiceProvider();

        serviceProvider
            .GetService<IBuilder>()?
            .Build(sourceDirectory, distDirectory);  
    }
}
