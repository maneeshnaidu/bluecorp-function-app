using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace bluecorp_function_app.Interfaces
{
    public interface IJsonToCsvMapper
    {
        void MapToCsv(LoadData loadData, string outputPath);
    }
}