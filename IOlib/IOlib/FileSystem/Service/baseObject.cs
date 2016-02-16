using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IOlib.FileSystem.Service.Extensions;

namespace IOlib.FileSystem.Service
{
    

    public enum ofsType
    {
        File = 1,
        Folder = 2,
        Data = 3,
        DataProcessor = 4
    }

    public interface IBaseObject
    {
        string ID { get; }
        ofsType type { get; }
    }

    [Serializable]
    public abstract class ABaseObject : IBaseObject
    {
        public readonly string ID;
        public readonly ofsType type;

        string IBaseObject.ID => this.ID;
        ofsType IBaseObject.type => this.type;

        public ABaseObject(ofsType objectType)
        {
            if(!objectType.isDefined()) throw new ArgumentException("Неправильное значение параметра {0}", nameof(objectType));
            type = objectType;
            ID = Guid.NewGuid().ToString();
        }
    }
}
