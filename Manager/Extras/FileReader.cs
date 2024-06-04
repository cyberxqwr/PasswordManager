using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manager.Extras
{
    internal class FileReader : IDisposable
    {

        // Disposable

        bool is_disposed = false;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            this.is_disposed = true;
        }

        public static string ReadFileToString(string fileName)
        {
            string filePath = Path.Combine(GetTempFolderPath(), fileName);

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"Failas nerastas: {filePath}");
            }

            string content;
            using (StreamReader reader = new StreamReader(filePath))
            {
                content = reader.ReadToEnd();
            }

            return content;
        }

        public static string ReadJSON(string fileName)
        {

            string filePath = Path.Combine(GetTempFolderPath(), fileName);
            
            return File.ReadAllText(filePath);


        }

        public static void WriteToFile(string fileName, string body)
        {
            string tempFolderPath = Path.GetTempPath();
            string filePath = Path.Combine(tempFolderPath, fileName);

            try
            {
                File.WriteAllText(filePath, body);
            }
            catch (Exception ex)
            {

                Console.WriteLine("Klaida irasant faila: " + ex.Message);
            }
        }

        public static void WriteBytesToFile(string fileName, byte[] bytes)
        {
            string tempFolderPath = Path.GetTempPath();
            string filePath = Path.Combine(tempFolderPath, fileName);

            try
            {
                File.WriteAllBytes(filePath, bytes);
            }
            catch (Exception ex)
            {

                Console.WriteLine("Klaida irasant faila: " + ex.Message);
            }

        }

        public static string GetTempFolderPath()
        {
            return Path.GetTempPath();
        }

        public static bool FileExists(string fileName)
        {

            string filePath = Path.Combine(GetTempFolderPath(), fileName);
            return File.Exists(filePath);
        }

        public static void CreateFile(string fileName)
        {

            string filePath = Path.Combine(GetTempFolderPath(), fileName);
            File.Create(filePath);
        }

        public static string UserFile(string fileName)
        {

            return Path.Combine(GetTempFolderPath(), fileName);
        }
    }
}
