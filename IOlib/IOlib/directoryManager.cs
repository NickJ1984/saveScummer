using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.Serialization;


namespace IOlib
{

    #region extension class
    internal static class myExt//everything tested
    {
        internal static bool uniqKey<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, bool upperCase = false)
            where TKey : IConvertible
        {
            if(upperCase)
            {
                if (typeof(TKey) != typeof(string)) throw new Exception("Wrong key type");

                string[] keys = dict.Keys.Select(elem => ((string)Convert.ChangeType(elem, typeof(string))).ToUpper()).ToArray();
                string sKey = ((string)Convert.ChangeType(key, typeof(string))).ToUpper();

                return !Array.Exists(keys, k => k == sKey);
            }
            return !dict.Keys.Contains(key);
        }
        internal static bool uniqValue<TKey, TValue>(this IDictionary<TKey, TValue> dict, TValue value, bool upperCase = false)
            where TValue : IConvertible
        {
            if (upperCase)
            {
                if (typeof(TValue) != typeof(string)) throw new Exception("Wrong value type");

                string[] values = dict.Values.Select(elem => ((string)Convert.ChangeType(elem, typeof(string))).ToUpper()).ToArray();
                string sVal = ((string)Convert.ChangeType(value, typeof(string))).ToUpper();

                return !Array.Exists(values, k => k == sVal);
            }
            return !dict.Values.Contains(value);
        }
        internal static T[] deleteElement<T>(this T[] instance, int index)
        {
            int length = instance.Length;
            if (length == 0 || index < 0) return instance;
            T[] newArray = instance.Where((obj, i) => i != index).ToArray();
            return newArray;
        }
        internal static T[] addElement<T>(this T[] instance, int elementsNumber)
        {
            if (elementsNumber <= 0) return instance;
            T[] newArray = new T[instance.Length + elementsNumber];
            Array.Copy(instance, newArray, instance.Length);
            return newArray;
        }
    }

    #endregion

    public class testDict
    {
        public bool uVal(Dictionary<string,string> dictionary, string value, bool up = false)
        {
            return dictionary.uniqValue(value, up);
        }
        public bool uKey(Dictionary<string, string> dictionary, string key, bool up = false)
        {
            return dictionary.uniqKey(key, up);
        }
    }
    public class testArr
    {
        public T[] add<T>(T[] array, int count) { return array.addElement(count); }
        public T[] del<T>(T[] array, int index) { return array.deleteElement(index); }
    }
    public class directoryManager
    {
        private const string defKey = "default";
        private string defDestination;
        private string defSource;

        private List<string> tags;
        private Dictionary<string, string> destDir;
        private Dictionary<string, string> srcDir;
        private Dictionary<string, dirInfo[]> storedDirs;


        public directoryManager()
        {
            tags = new List<string>();
            destDir = new Dictionary<string, string>();
            srcDir = new Dictionary<string, string>();
            storedDirs = new Dictionary<string, dirInfo[]>();
        }
        public directoryManager(string defaultSourcePath, string defaultDestinationPath)
            :this()
        {
            paramCheck(defaultDestinationPath);
            paramCheck(defaultSourcePath);

            if (!dirExist(defaultSourcePath)) throw new sourceDirectoryNotExistException();

            defDestination = defaultDestinationPath;
            defSource = defaultSourcePath;

            if (!dirExist(defDestination)) Directory.CreateDirectory(defDestination);
        }


        public void addTag(string tag)
        {
            paramCheck(tag);
            if (destDir.uniqKey(tag.ToUpper())) throw new tagAlreadyExistException();


            tags.Add(tag);


            tag = up(tag);

            destDir.Add(tag, 
                destDir.uniqValue(defDestination, true) ? 
                defDestination : path(defDestination, tag));
            storedDirs.Add(tag, new dirInfo[0]);
            srcDir.Add(tag, defSource);
        }

        public void setSource(string tag, string sourceDir)
        {
            paramCheck(tag);
            paramCheck(sourceDir);

            tag = up(tag);

            if (srcDir.uniqKey(tag)) throw new tagNotExistException();
            if (up(srcDir[tag]) == up(sourceDir)) return;


            srcDir[tag] = sourceDir;
        }
        public void setDestination(string tag, string destinationDir)
        {
            paramCheck(tag);
            paramCheck(destinationDir);

            tag = up(tag);

            if (srcDir.uniqKey(tag)) throw new tagNotExistException();
            if (up(destDir[tag]) == up(destinationDir)) return;


            string temp = destDir[tag];
            destDir[tag] = destinationDir;

            moveStored(tag, temp, destinationDir);
        }
        public void saveFolder(string tag, string folderName)
        {
            paramCheck(tag);
            paramCheck(folderName);
            tag = up(tag);

            if (!tagExist(tag)) throw new tagNotExistException();
            if (!dirExist(folderName)) throw new DirectoryNotFoundException();


            string pth = path(srcDir[tag], folderName);
            int index = 0;

            if (storedDirs[tag].Length > 0)
            {
                string newName = up(folderName);
                index = Array.FindIndex(storedDirs[tag].Select(di => up(di.name)).ToArray(), name => name == newName);
            }

            dirInfo newDInfo = new dirInfo(new DirectoryInfo(path(srcDir[tag], folderName)));
            dirInfo[] newInfo = storedDirs[tag];

            if (index >= 0)
            {
                Directory.Delete(path(destDir[tag], storedDirs[tag][index].name));
            }
            else
            {
                index = storedDirs[tag].Length;
                Array.Resize<dirInfo>(ref newInfo, newInfo.Length + 1);
            }

            
            IOmanager.copyDirectory(pth, path(destDir[tag], newDInfo.name));
            newDInfo.storedTime = DateTime.Now;
            newInfo[index] = newDInfo;
            storedDirs[tag] = newInfo;
        }
        public void loadFolder(string tag, string storedFolderName)
        {
            paramCheck(tag);
            paramCheck(storedFolderName);
            tag = up(tag);
            if (!tagExist(tag)) throw new tagNotExistException();

            string sfnUp = up(storedFolderName);
            int index = Array.FindIndex(storedDirs[tag], dir => dir.name.ToUpper() == sfnUp);
            if (index < 0) throw new storedFolderNotExistException();

            string fPath = path(destDir[tag], storedDirs[tag][index].name);
            if (!dirExist(fPath))
            {
                storedDirs[tag] = storedDirs[tag].deleteElement(index);
                throw new DirectoryNotFoundException();
            }
            string dPath = path(srcDir[tag], storedDirs[tag][index].name);
            if (!dirExist(dPath)) throw new DirectoryNotFoundException();

            Directory.Delete(dPath);
            IOmanager.copyDirectory(fPath, dPath);
        }

        #region service
        
        private void moveStored(string tag, string oldPath, string newPath)
        {
            tag = up(tag);

            if (storedDirs[tag].Length <= 0) return;

            Func<string, string> Old = name => path(oldPath, name);
            Func<string, string> New = name => path(newPath, name);

            for (int i = 0; i < storedDirs[tag].Length; i++)
                IOmanager.moveDirectory(Old(storedDirs[tag][i].name), New(storedDirs[tag][i].name));
        }
        private void paramCheck(string parameter)
        {
             if (parameter == null || parameter == "") throw new wrongParameterValueException();
        }
        
        private bool isUniq(string[] array, string sValue, bool upper = true)
        {
            if (upper)
            {
                sValue = sValue.ToUpper();
                array = array.Select(s => s.ToUpper()).ToArray();
            } 
            return !(Array.Exists(array, s => s == sValue));
        }
        private string up(string tag) { return tag.ToUpper(); }
        private string path(string directory, string name)
        { return Path.Combine(directory, name); }
        private bool tagExist(string tag)
        { return !srcDir.uniqKey(up(tag)); }
        private bool dirExist(string directory)
        { return Directory.Exists(directory); }
        #endregion
        #region exceptions
        /// <summary>
        /// Класс исключения: "Исходная папка не существует".
        /// </summary>
        public class sourceDirectoryNotExistException : ApplicationException
        {
            private const string msg = "Исходная папка не существует.";
            /// <summary>
            /// Конструктор исключения: "Исходная папка не существует".
            /// </summary>
            public sourceDirectoryNotExistException() : base(msg)
            { }
        }
        /// <summary>
        /// Класс исключения: "Сохраненная папка не найдена".
        /// </summary>
        public class storedFolderNotFoundException : ApplicationException
        {
            private const string msg = "Сохраненная папка не найдена.";
            /// <summary>
            /// Конструктор исключения: "Сохраненная папка не найдена".
            /// </summary>
            public storedFolderNotFoundException() : base(msg)
            { }
        }
        /// <summary>
        /// Класс исключения: "Сохраненная папка с таким именем отсутствует в хранилище".
        /// </summary>
        public class storedFolderNotExistException : ApplicationException
        {
            private const string msg = "Сохраненная папка с таким именем отсутствует в хранилище";
            /// <summary>
            /// Конструктор исключения: "Сохраненная папка с таким именем отсутствует в хранилище".
            /// </summary>
            public storedFolderNotExistException() : base(msg)
            { }
        }
        /// <summary>
        /// Класс исключения: "Тэг с таким именем не существует".
        /// </summary>
        public class tagNotExistException : ApplicationException
        {
            private const string msg = "Тэг с таким именем не существует.";
            /// <summary>
            /// Конструктор исключения: "Тэг с таким именем не существует".
            /// </summary>
            public tagNotExistException() : base(msg)
            { }
        }
        /// <summary>
        /// Класс исключения: "Тэг с таким именем уже существует в хранилище".
        /// </summary>
        public class tagAlreadyExistException : ApplicationException
        {
            private const string msg = "Тэг с таким именем уже существует в хранилище";
            /// <summary>
            /// Конструктор исключения: "Тэг с таким именем уже существует в хранилище".
            /// </summary>
            public tagAlreadyExistException() : base(msg)
            { }
        }
        /// <summary>
        /// Класс исключения: "Некорректное значение параметра метода".
        /// </summary>
        public class wrongParameterValueException : ApplicationException
        {
            private const string msg = "Некорректное значение параметра метода";
            /// <summary>
            /// Конструктор исключения: "Некорректное значение параметра метода".
            /// </summary>
            public wrongParameterValueException() : base(msg)
            { }
        }
        #endregion
        #region structures
        private struct dirInfo
        {
            public string name;
            public DateTime lastWriteTime;
            public DateTime storedTime;

            public dirInfo(string name, DateTime lastWriteTime, DateTime storedTime)
            {
                this.name = name;
                this.lastWriteTime = lastWriteTime;
                this.storedTime = storedTime;
            }
            public dirInfo(DirectoryInfo info, DateTime storedTime)
                :this(info.Name, info.LastWriteTime, storedTime)
            { }
            public dirInfo(DirectoryInfo info)
                :this(info, DateTime.Now)
            { }

        }
        #endregion
    }
}
