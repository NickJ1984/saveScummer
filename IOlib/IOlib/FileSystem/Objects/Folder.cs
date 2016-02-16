using IOlib.FileSystem.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IOlib.FileSystem.Service.Extensions;
using System.IO;
using IOlib.FileSystem.Objects.FolderBase;

namespace IOlib.FileSystem.Objects.FolderBase
{
    #region Интерфейс папки
    public interface IFolder : IBaseObject, IFSInfo, IDateInfo
    {
        fsFile this[int index] { get; }
        int fileCount { get; }
        double size { get; }
        bool read();
        bool write(string path);
        bool write();
        void clear();
    } 
    #endregion
    #region Абстрактный класс папки
    [Serializable]
    public abstract class AFolder : ABaseObject, IFolder
    {
        #region Индексатор
        public virtual fsFile this[int index]
        {
            get
            {
                if (!files.isInRange(index)) throw new IndexOutOfRangeException();
                return files[index];
            }
        }
        #endregion
        #region Переменные
        protected fsFile[] files;
        private string _fullPath;
        private string _name;
        private double _size;
        #endregion
        #region Свойства
        public virtual string fullPath
        {
            get { return _fullPath; }
            set
            {
                if (string.IsNullOrEmpty(value) || !Path.IsPathRooted(value)) return;
                _fullPath = Path.GetFullPath(value);
                _name = Path.GetFileName(_fullPath);
            }
        }
        public virtual string name => _name;
        public virtual double size => _size;
        public virtual DateTime lastWriteDate { get; protected set; }
        public virtual DateTime loadDate { get; protected set; }
        public virtual int fileCount => files.Length;
        #endregion
        #region Конструктор

        public AFolder()
            : base(ofsType.Folder)
        {
            _fullPath = _name = null;
            clear();
        }
        ~AFolder()
        {
            _fullPath = _name = null;
            files = null;
        }
        #endregion
        #region Методы
        public virtual void clear()
        {
            lastWriteDate = loadDate.initialDate();
            _size = 0;
            files = new fsFile[0];
        }
        public virtual bool read()
        {
            if (fileCount != 0 || !fullPath.isDirectoryExist()) return false;

            DirectoryInfo dInfo = new DirectoryInfo(fullPath);
            lastWriteDate = dInfo.LastWriteTime;

            FileInfo[] pFiles = dInfo.GetFiles();
            files = new fsFile[pFiles.Length];

            for (int i = 0; i < pFiles.Length; i++)
            {
                files[i] = new fsFile(pFiles[i].FullName);
                if (!files[i].read()) throw new fileNotReadException(files[i].fullPath);
            }

            loadDate = DateTime.Now;
            calculateSize();

            return true;
        }

        public virtual bool write()
        {
            return write(fullPath);
        }

        public virtual bool write(string path)
        {
            if (string.IsNullOrEmpty(path)) return false;
            if (path != fullPath)
            {
                if (!Path.IsPathRooted(path)) return false;
                path = Path.GetFullPath(path);
            }

            Directory.CreateDirectory(path);

            for (int i = 0; i < fileCount; i++)
                if (!files[i].write(Path.Combine(path + '\\' + files[i].name))) throw new fileNotWrittenException(files[i].fullPath);

            return true;
        }

        private void calculateSize()
        {
            if (files.isNullOrEmpty()) _size = 0;
            _size = files.Sum(val => val.size);
        }
        #endregion
        #region Исключения
        public class fileNotWrittenException : ApplicationException
        {
            public fileNotWrittenException(string fullFilePath)
                : base(string.Format("Файл {0} не был записан", fullFilePath))
            { }
        }
        public class fileNotReadException : ApplicationException
        {
            public fileNotReadException(string fullFilePath)
                : base(string.Format("Файл {0} не был прочитан", fullFilePath))
            { }
        }
        #endregion
    } 
    #endregion
}

namespace IOlib.FileSystem.Objects
{
    [Serializable]
    public class fsFolder : AFolder
    {
        public fsFolder()
            :this(null)
        { }
        public fsFolder(string path)
            : base()
        {
            fullPath = path;
        }
    }
}
