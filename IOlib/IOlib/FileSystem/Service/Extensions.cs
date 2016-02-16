using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IOlib.FileSystem.Service;

namespace IOlib.FileSystem.Service.Extensions
{
    
    public static class classExtensions
    {
        internal static bool isDefined(this ofsType self)
        {
            return Enum.IsDefined(typeof(ofsType), self);
        }

        internal static bool isNullOrEmpty<TIn>(this TIn[] self)
        {
            return self == null || self.Length == 0;
        }

        internal static bool isInRange<TIn>(this TIn[] self, int index)
        {
            return index >= 0 && index < self.Length;
        }
        internal static bool isFileExist(this string fullFilePath)
        {
            return !string.IsNullOrEmpty(fullFilePath) && File.Exists(fullFilePath);
        }
        internal static bool isDirectoryExist(this string fullDirectoryPath)
        {
            return !string.IsNullOrEmpty(fullDirectoryPath) && Directory.Exists(fullDirectoryPath);
        }

        internal static DateTime initialDate(this DateTime self)
        {
            return new DateTime(1,1,1);
        }
    }
}
