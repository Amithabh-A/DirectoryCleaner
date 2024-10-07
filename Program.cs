using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

class Program
{
  static async Task Main(string[] args)
  {
    if (args.Length != 1)
    {
      Console.WriteLine("Usage: dotnet run <directory-path>");
      return;
    }

    string directoryPath = args[0];

    if (!Directory.Exists(directoryPath))
    {
      Console.WriteLine($"Directory does not exist: {directoryPath}");
      return;
    }

    var fileHashes = new Dictionary<string, List<string>>();

    // Get all files in the directory
    var files = Directory.GetFiles(directoryPath);

    // Generate hashes
    foreach (var file in files)
    {
      var hash = await ComputeFileHashAsync(file);
      if (!fileHashes.ContainsKey(hash))
      {
        fileHashes[hash] = new List<string>();
      }
      fileHashes[hash].Add(file);
    }

    // Delete the newest file for each hash collision
    foreach (var hashEntry in fileHashes)
    {
      var fileList = hashEntry.Value;
      if (fileList.Count > 1)
      {
        // Sort files by creation time descending
        var filesToDelete = fileList.OrderByDescending(f => new FileInfo(f).CreationTime).ToList();
        // Keep the oldest file and delete the rest
        for (int i = 0; i < filesToDelete.Count - 1; i++)
        {
          Console.WriteLine($"Deleting file: {filesToDelete[i]} (Duplicate of {filesToDelete.Last()})");
          File.Delete(filesToDelete[i]);
        }
      }
    }
  }

  private static async Task<string> ComputeFileHashAsync(string filePath)
  {
    using (var sha256 = SHA256.Create())
    {
      using (var stream = File.OpenRead(filePath))
      {
        var hash = await sha256.ComputeHashAsync(stream);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
      }
    }
  }
}

