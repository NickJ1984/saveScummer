using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IOlib.FileSystem.Service
{
    public interface IDateInfo
    {
        DateTime lastWriteDate { get; }
        DateTime loadDate { get; }
    }

    public interface IFSInfo
    {
        string name { get; }
        string fullPath { get; }
    }
}
