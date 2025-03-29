using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Windows.Storage;

namespace LowLand.Utils
{
    public class FileUtils
    {
        public static string GenerateUniqueFileName(string originalFileName, string targetDirectory)
        {
            string fileExtension = Path.GetExtension(originalFileName);
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(originalFileName);

            // Generate a new unique name by appending GUID or timestamp
            // Ensure the name is unique (in case of collisions)
            string uniqueName;
            do
            {
                uniqueName = $"{fileNameWithoutExtension}_{Guid.NewGuid()}{fileExtension}";
            } while (File.Exists(Path.Combine(targetDirectory, uniqueName)));

            return uniqueName;
        }

        public static string SaveImage(string filePath)
        {
            // Get the path to the app's LocalFolder
            var installedLocation = ApplicationData.GetDefault().LocalFolder.Path;

            // Get file name from filePath
            string fileName = Path.GetFileName(filePath);

            // Get a unique file name
            string uniqueFileName = GenerateUniqueFileName(fileName, installedLocation);

            try
            {
                File.Copy(filePath, Path.Combine(installedLocation, uniqueFileName));
            }
            catch (Exception e)
            {
                Debug.WriteLine("Error while copying image: " + e.Message);
                return "";
            }

            return uniqueFileName;
        }

        public static bool DeleteImage(string fileName)
        {
            // Get the path to the app's LocalFolder
            var installedLocation = ApplicationData.GetDefault().LocalFolder.Path;
            try
            {
                File.Delete(Path.Combine(installedLocation, fileName));
                return true;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }

            return false;
        }
    }
}
