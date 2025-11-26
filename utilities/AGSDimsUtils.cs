using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DimensionSystem
{
    public static class AGSDimsUtils
    {
        public static Dimension CreateDimensionAlongLine(Document doc, Line dimline, ReferenceArray referenceArray)
        {
            Dimension NewDim = doc.Create.NewDimension(doc.ActiveView, dimline, referenceArray);
            return NewDim;   
        }

    }
}
