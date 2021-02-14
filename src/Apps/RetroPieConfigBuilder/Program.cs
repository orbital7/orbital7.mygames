using System;
using System.IO;

namespace RetroPieConfigBuilder
{
    class Program
    {
        static void Main(string[] args)
        {
            const string DEFAULT_FILE_NAME = "default.cfg";
            var templatesPath = @"C:\Users\josh\Dropbox\Gaming\RetroPie\RetroStation2\templates";
            var outputPath = @"C:\Users\josh\Dropbox\Gaming\Roms\arcade\gameconfigs\retroclassic-gve";

            var defaultConfig = Configuration.ParseFile(
                Path.Combine(outputPath, DEFAULT_FILE_NAME));

            foreach (var templateFilePath in Directory.GetFiles(templatesPath))
            {
                var filename = Path.GetFileName(templateFilePath);
                if (filename.ToLower() != DEFAULT_FILE_NAME)
                {
                    var outputFilePath = Path.Combine(outputPath, filename);
                    defaultConfig.Write(
                        templateFilePath,
                        outputFilePath);
                }
            }
        }
    }
}
