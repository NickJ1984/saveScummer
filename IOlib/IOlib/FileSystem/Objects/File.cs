using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IOlib.FileSystem;
using IOlib.FileSystem.Objects.FileBase;
using IOlib.FileSystem.Service;
using IOlib.FileSystem.Service.Extensions;

namespace IOlib.FileSystem.Objects.FileBase
{
    #region Интерфейс файла
    public interface IFile : IBaseObject, IFSInfo, IDateInfo
    {
        double size { get; }
        bool read();
        bool write(string fullFilePath);
        bool write();
        void clear();
    }
    #endregion
    #region Абстрактный класс файла
    [Serializable]
    public abstract class AFile : ABaseObject, IFile
    {
        #region Переменные
        private string _fullPath;
        private string _name;
        protected Data _data;
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
        public virtual double size => _data.size;
        public virtual DateTime lastWriteDate { get; protected set; }
        public virtual DateTime loadDate { get; protected set; }
        #endregion
        #region Конструктор

        public AFile() : base(ofsType.File)
        {
            _fullPath = null;
            _name = null;
            _data = new Data();
            lastWriteDate = loadDate.initialDate();
        }

        ~AFile()
        {
            _data = null;
            _fullPath = null;
            _name = null;
        }
        #endregion
        #region Методы
        public virtual bool read()
        {
            if (!_data.isNull || string.IsNullOrEmpty(fullPath)) return false;
            if (!File.Exists(fullPath)) throw new FileNotFoundException();
            lastWriteDate = File.GetLastWriteTime(fullPath);
            _data.data = Data.read(fullPath);
            loadDate = DateTime.Now;
            return true;
        }
        public virtual bool write()
        {
            return write(fullPath);
        }
        public virtual bool write(string fullFilePath)
        {
            if (_data.isNull || string.IsNullOrEmpty(fullFilePath)) return false;
            if (fullFilePath != fullPath)
            {
                if (!Path.IsPathRooted(fullFilePath)) return false;
                fullFilePath = Path.GetFullPath(fullFilePath);
            }
            Data.write(_data, fullFilePath, true);
            return true;
        }
        public virtual void clear()
        {
            _data.clear();
            lastWriteDate = loadDate.initialDate();
        }
        #endregion
    } 
    #endregion

}

namespace IOlib.FileSystem.Objects
{
    [Serializable]
    public class fsFile : AFile
    {
        public fsFile()
            :this(null)
        { }
        public fsFile(string fullFilePath)
            : base()
        {
            this.fullPath = fullFilePath;
        }

        public override string ToString()
        {
            Data.measure m = Data.measure.KBytes;
            return string.
                Format
                ("File name: {0} Path: {1} Size: {2:0.##} {5} LastWrite: {3:dd.MM.yyyy} Loaded: {4:dd.MM.yyyy}",
                    name, fullPath, Data.convertSize(size, m), lastWriteDate, loadDate, m);
        }

        public override int GetHashCode()
        {
            return ID.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj is IFile)
            {
                IFile fObj = obj as IFile;
                return ID == fObj.ID;
            }
            else return false;
        }
    }
}