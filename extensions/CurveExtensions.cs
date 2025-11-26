using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.Creation;
using System.Xml.Linq;
using RevitAPI.Extensions;


namespace VisualizePL.Extensions
{
    public static class CurveExtensions
    {

        public static void VisualizeCurve(this Curve curve, Autodesk.Revit.DB.Document document)
        {
            document.CreateDirectShapes(new List<GeometryObject>() { curve});

        }
            
           
    }
}
