using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace IOlib
{
    /// <summary>
    /// Статический класс управления файловым вводом выводом
    /// </summary>
    public static class IOmanager
    {
        /// <summary>
        /// Копирование директории.
        /// </summary>
        /// <param name="sourceDirectory">Полный путь к копируемой директории.</param>
        /// <param name="destinationPath">Путь назначения.</param>
        /// <param name="copySubdirectories">Истина - копировать все поддиректории.</param>
        public static void copyDirectory(string sourceDirectory, string destinationPath, bool copySubdirectories)
        {
            DirectoryInfo dir = new DirectoryInfo(sourceDirectory);
            
            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirectory);
            }

            if (!Directory.Exists(destinationPath)) Directory.CreateDirectory(destinationPath);

            FileInfo[] files = dir.GetFiles();
            for (int i = 0; i < files.Length; i++) files[i].CopyTo(Path.Combine(destinationPath, files[i].Name));

            if (copySubdirectories)
            {
                DirectoryInfo[] subs = dir.GetDirectories();
                for (int i = 0; i < subs.Length; i++)
                    copyDirectory(subs[i].FullName, Path.Combine(destinationPath, subs[i].Name), copySubdirectories);
            }
        }
        /// <summary>
        /// Копирование директории с поддиректориями.
        /// </summary>
        /// <param name="sourceDirectory">Полный путь к копируемой директории.</param>
        /// <param name="destinationDirectory">Полный путь назначения.</param>
        public static void copyDirectory(string sourceDirectory, string destinationDirectory)
        { copyDirectory(sourceDirectory, destinationDirectory, true); }

        /// <summary>
        /// Перемещение директории.
        /// </summary>
        /// <param name="sourceDirectory">Полный путь к перемещаемой директории.</param>
        /// <param name="destinationPath">Путь назначения.</param>
        public static void moveDirectory(string sourceDirectory, string destinationPath)
        {
            bool exception = false;

            try
            {
                copyDirectory(sourceDirectory, destinationPath);
            }
            catch(Exception ex)
            {
                exception = true;
                throw;
            }
            if (!exception) Directory.Delete(sourceDirectory);
        }
    }
}
