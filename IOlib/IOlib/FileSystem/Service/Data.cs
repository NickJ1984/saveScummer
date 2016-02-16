using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IOlib.FileSystem.Service.Extensions;

namespace IOlib.FileSystem.Service
{
    public interface IData : IBaseObject
    {
        byte[] data { get; set; }
        double size { get; }
        bool isNull { get; }

        void clear();
    }

    [Serializable]
    public class Data : ABaseObject, IData
    {
        #region Переменные
        protected byte[] _data;
        #endregion
        #region Свойства
        public byte[] data
        {
            get { return _data; }
            set
            {
                if (value.isNullOrEmpty()) return;
                _data = value;
            }
        }
        public double size => data.Length;
        public bool isNull => data.isNullOrEmpty();
        #endregion
        #region Конструктор
        public Data()
            : base(ofsType.Data)
        {
            clear();
        }
        public Data(byte[] dataArray) 
            :this()
        {
            data = dataArray;
        }
        ~Data()
        {
            _data = null;
        }

        #endregion
        #region Методы
        public void clear()
        {
            _data = new byte[0];
        }
        #endregion
        #region Статические методы

        public static double convertSize(double size, measure unit)
        {
            int iUnit = (int) unit;
            if (iUnit == 0 || size <= 0) return size;
            iUnit--;
            return convertSize(size/1024, (measure)iUnit);
        }
        public static byte[] read(string fullFilePath)
        {
            if (!fullFilePath.isFileExist()) throw new FileNotFoundException();
            return File.ReadAllBytes(fullFilePath);
        }

        public static bool write(Data data, string fullFilePath, bool rewrite = false)
        {
            if (!rewrite && fullFilePath.isFileExist()) return false;
            if (data.isNull) return false;

            File.WriteAllBytes(fullFilePath, data.data);
            return true;
        }
        #endregion
        public enum measure
        {
            Bytes = 0,
            KBytes = 1,
            MBytes = 2,
            GBytes = 3
        }
    }
}
