using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;

namespace DimensionSystem
{


    public static class AGSVisualizationUtils
    {
        public static void VisualizeGeometryList(Document doc, IList<GeometryObject> geomtryobject)
        {
            DirectShape.CreateElement(doc, new ElementId(BuiltInCategory.OST_GenericModel)).SetShape(geomtryobject);
        }

        public static void VisualizeGeometry(Document doc, GeometryObject geomtryobject)
        {
            DirectShape.CreateElement(doc, new ElementId(BuiltInCategory.OST_GenericModel)).SetShape(new List<GeometryObject>{ geomtryobject });
        }

        public static void VisualizePoint(Document doc, XYZ point)
        {
            // Create a small, bounded line to represent the point.
            // A line is a valid geometry for DirectShape.
            double lineLength = 0.1; // Adjust the length as needed
            XYZ p1 = point - XYZ.BasisX.Multiply(lineLength / 2);
            XYZ p2 = point + XYZ.BasisX.Multiply(lineLength / 2);
            Line line = Line.CreateBound(p1, p2);
            VisualizeGeometry(doc, line);

        }

        public static void VisualizePoints(Document doc, IList<XYZ> points)
        {
            foreach (var point in points) 
            {
                var p = Point.Create(point);
                VisualizeGeometry(doc, p);
            }
        }

        public static void VisualizePoints(Document doc, Point projectedpoint, params XYZ [] points)
        {
            foreach (var point in points)
            {
                var p = Point.Create(point);
                VisualizeGeometry(doc, p);
            }
        }

        public static void VisualizePointByPicking(UIDocument uidoc)
        {
            Document doc = uidoc.Document;
            Point p = Point.Create(uidoc.Selection.PickPoint());
            VisualizeGeometry(doc, p);

        }

       

    }
}
