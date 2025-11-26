using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExportCADX.Models
{
    public class ExportRequest
    {

        public string ElementType { get; set; }   // e.g. "Walls"
        public string Format { get; set; }        // "DXF", "DWG", "XML"
        public string FolderPath { get; set; }    // folder to write files


    }
}
