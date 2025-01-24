// See https://aka.ms/new-console-template for more information

using File = System.IO.File;
using System;
using System.IO;
using System.Xml.Linq;

class Program
{
    static void Main(string[] args)
    {
        string filesDirectory = Path.Combine(Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory)?.Parent?.Parent?.Parent?.FullName ?? "", "Files");

        // File paths
        string xmlFilePath = Path.Combine(filesDirectory, "lookup.xml");
        string scriptFilePath = Path.Combine(filesDirectory, "script.txt");
        string outputFilePath = Path.Combine(filesDirectory, "modified_script.txt");
        
        XDocument xmlDoc = XDocument.Load(xmlFilePath);
        string scriptContent = File.ReadAllText(scriptFilePath);

        foreach (var tableMapping in xmlDoc.Descendants("TableNames"))
        {
            // Get Source and Destination table names
            string sourceTable = tableMapping.Attribute("Source")?.Value;
            string destinationTable = tableMapping.Attribute("Destination")?.Value;

            if (!string.IsNullOrEmpty(sourceTable) && !string.IsNullOrEmpty(destinationTable))
            {
                // Replace table names only when they are standalone (e.g., FROM MTE or JOIN MTE) (skip aliases)
                scriptContent = scriptContent.Replace($" {sourceTable} ", $" {destinationTable} ");
            }

            var columnMappings = tableMapping.Elements("Columns").Elements("Column");
            foreach (var columnMapping in columnMappings)
            {
                string sourceColumn = columnMapping.Attribute("Source")?.Value;
                string destinationColumn = columnMapping.Attribute("Destination")?.Value;

                if (!string.IsNullOrEmpty(sourceColumn) && !string.IsNullOrEmpty(destinationColumn))
                {
                    string sourcePattern = $"{sourceTable}.{sourceColumn}";
                    string destinationPattern = $"{destinationTable}.{destinationColumn}";
                    scriptContent = scriptContent.Replace(sourcePattern, destinationPattern);
                    
                    scriptContent = scriptContent.Replace($" {sourceColumn} ", $" {destinationColumn} ");
                }
            }
        }


        File.WriteAllText(outputFilePath, scriptContent);

        Console.WriteLine("Script processing complete. Modified script saved to: " + outputFilePath);
    }
}
