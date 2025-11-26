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

namespace RevitAPI.Extensions
{
    /// <summary>  
    /// Provides extension methods for the Autodesk Revit Document class.  
    /// </summary>  
    public static class DocumentExtensions
    {
        /// <summary>  
        /// Runs the specified action within a Revit transaction.  
        /// </summary>  
        /// <param name="document">The Revit document.</param>  
        /// <param name="action">The action to execute.</param>  
        public static void Run(this Autodesk.Revit.DB.Document document, Action action)
        {
            using (Autodesk.Revit.DB.Transaction tr = new Autodesk.Revit.DB.Transaction(document, "Transaction"))
            {
                try
                {
                    tr.Start();
                    action.Invoke();
                    tr.Commit();
                }
                catch (Exception ex)
                {
                    tr.RollBack();
                    TaskDialog.Show("Revit", $"Error: {ex.Message}");
                }
            }
            return;
        }

        /// <summary>  
        /// Creates a point at the specified coordinates in the Revit document.  
        /// </summary>  
        /// <param name="document">The Revit document.</param>  
        /// <param name="x">The X coordinate of the point.</param>  
        /// <param name="y">The Y coordinate of the point.</param>  
        /// <param name="z">The Z coordinate of the point.</param>  
        public static void CreatePoint(this Autodesk.Revit.DB.Document document, int x = 0, int y = 0, int z = 0)
        {
            document.Run(() =>
            {
                Point pnt = Point.Create(new XYZ(x, y, z));
                DirectShape ds = DirectShape.CreateElement(document, new Autodesk.Revit.DB.ElementId(BuiltInCategory.OST_GenericModel));
                ds.SetShape(new List<GeometryObject> { pnt });
            });
            return;
        }

        /// <summary>  
        /// Creates a point at the specified XYZ location in the Revit document.  
        /// </summary>  
        /// <param name="document">The Revit document.</param>  
        /// <param name="xyzPnt">The XYZ location of the point.</param>  
        public static void CreatePoint(this Autodesk.Revit.DB.Document document, XYZ xyzPnt)
        {
            document.Run(() =>
            {
                Point pnt = Point.Create(xyzPnt);
                DirectShape ds = DirectShape.CreateElement(document, new Autodesk.Revit.DB.ElementId(BuiltInCategory.OST_GenericModel));
                ds.SetShape(new List<GeometryObject> { pnt });
            });
            return;
        }

        /// <summary>  
        /// Creates a line between two points in the Revit document.  
        /// </summary>  
        /// <param name="document">The Revit document.</param>  
        /// <param name="staPnt">The start point of the line.</param>  
        /// <param name="endPnt">The end point of the line.</param>  
        public static void CreateLine(this Autodesk.Revit.DB.Document document, XYZ staPnt, XYZ endPnt)
        {
            document.Run(() =>
            {
                Line ln = Line.CreateBound(staPnt, endPnt);
                DirectShape ds = DirectShape.CreateElement(document, new Autodesk.Revit.DB.ElementId(BuiltInCategory.OST_GenericModel));
                ds.SetShape(new List<GeometryObject> { ln });
            });
            return;
        }

        /// <summary>  
        /// Creates a DirectShape element in the Revit document.  
        /// </summary>  
        /// <param name="document">The Revit document.</param>  
        /// <param name="geometryObject">The geometry object to use for the DirectShape.</param>  
        /// <param name="builtInCategory">The built-in category for the DirectShape.</param>  
        /// <returns>The created DirectShape element.</returns>  
        public static DirectShape CreateDirectShape(
            this Autodesk.Revit.DB.Document document,
            GeometryObject geometryObject,
            BuiltInCategory builtInCategory = BuiltInCategory.OST_GenericModel)
        {
            var directShape = DirectShape.CreateElement(document, new Autodesk.Revit.DB.ElementId(builtInCategory));
            directShape.SetShape(new List<GeometryObject> { geometryObject });
            return directShape;
        }

        /// <summary>  
        /// Creates a DirectShape element with multiple geometry objects in the Revit document.  
        /// </summary>  
        /// <param name="doc">The Revit document.</param>  
        /// <param name="geometryObjects">The geometry objects to use for the DirectShape.</param>  
        /// <param name="builtInCategory">The built-in category for the DirectShape.</param>  
        /// <returns>The created DirectShape element.</returns>  
        public static DirectShape CreateDirectShapes(
            this Autodesk.Revit.DB.Document doc,
            IEnumerable<GeometryObject> geometryObjects,
            BuiltInCategory builtInCategory = BuiltInCategory.OST_GenericModel)
        {
            var directShapes = DirectShape.CreateElement(doc, new Autodesk.Revit.DB.ElementId(builtInCategory));
            directShapes.SetShape(geometryObjects.ToList());
            return directShapes;
        }

        /// <summary>  
        /// Retrieves all levels in the Revit document.  
        /// </summary>  
        /// <param name="document">The Revit document.</param>  
        /// <returns>A list of levels in the document.</returns>  
        public static List<Level> GetLevels(this Autodesk.Revit.DB.Document document)
        {
            return new FilteredElementCollector(document)
                .OfClass(typeof(Level))
                .Cast<Level>()
                .ToList();
        }

        /// <summary>  
        /// Retrieves elements of a specified type from the Revit document.  
        /// </summary>  
        /// <typeparam name="TElement">The type of elements to retrieve.</typeparam>  
        /// <param name="document">The Revit document.</param>  
        /// <param name="validate">An optional validation function for the elements.</param>  
        /// <returns>A list of elements of the specified type.</returns>  
        public static List<TElement> GetElementByTypes<TElement>(
            this Autodesk.Revit.DB.Document document,
            Func<TElement, bool> validate = null)
                where TElement : class
        {
            validate = validate ?? (e => true);
            var elements = new FilteredElementCollector(document)
                .OfClass(typeof(TElement))
                .Cast<TElement>()
                .Where(e => validate(e))
                .ToList();
            return elements;
        }
    }
}
