using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.IO;


namespace DimensionSystem
{
    public static class ExtensionMethods
    {

        #region Basic Extension Methods
        public static Point ToPoint(this XYZ point, Document doc)
        {
            var p = Point.Create(point);
            return p;
        }

        public static List<Solid> GetGeometry(this Element element)
        {
            Options options = new Options
            {
                ComputeReferences = true,
                // Ensure the view is set to the active view for geometry extraction context
                View = element.Document.ActiveView
            };

            GeometryElement geometryElement = element.get_Geometry(options);

            if (geometryElement == null)
            {
                return new List<Solid>();
            }

            List<Solid> allValidSolids = new List<Solid>();

            foreach (GeometryObject geomObj in geometryElement)
            {
                if (geomObj is Solid solid && solid.Faces.Size > 0 && solid.Volume > 0)
                {
                    allValidSolids.Add(solid);
                }
                else if (geomObj is GeometryInstance geomInstance)
                {
                    // For Family Instances, get the instance geometry and transform it
                    GeometryElement instanceGeometryElement = geomInstance.GetInstanceGeometry();
                    if (instanceGeometryElement != null)
                    {
                        foreach (GeometryObject instanceGeomObj in instanceGeometryElement)
                        {
                            if (instanceGeomObj is Solid instanceSolid && instanceSolid.Faces.Size > 0 && instanceSolid.Volume > 0)
                            {
                                // Apply the instance's transform to the solid
                                Solid transformedSolid = SolidUtils.CreateTransformed(instanceSolid, geomInstance.Transform);
                                allValidSolids.Add(transformedSolid);
                            }
                        }
                    }
                }
            }
            return allValidSolids;
        }

        public static void ShowGeometry(this List<GeometryObject> geometryObjects , Document document, bool withTransaction)
        {
            if(withTransaction)
            {
                using (Transaction transaction = new Transaction(document, "Test")) 
                {
                    transaction.Start();


                    transaction.Commit();

                DirectShape directShape =
                        
                        DirectShape.CreateElement(document, new ElementId(BuiltInCategory.OST_GenericModel));
                directShape.SetShape(geometryObjects.Where(e => e != null).ToList());


                };
             

            }


        }

        #endregion



        #region SlabBoundary Extension Methods

        public static IEnumerable<Level> GetElementsofType(this Document doc, System.Type type)
        {
            // Use a FilteredElementCollector to get all elements in the document.
            // Filter by the BuiltInCategory.OST_Levels category, which represents Level elements.
            // OfType<Level>() further filters the collection to ensure only Level objects are returned.
            // ToList() materializes the collection into a list.
            return new FilteredElementCollector(doc)
                       .OfCategory(BuiltInCategory.OST_Levels)
                       .WhereElementIsNotElementType() // Exclude type elements (e.g., Level heads)
                       .OfType<Level>()
                       .ToList();


        }

        
        public static XYZ ProjectPointOntoPlane(this Plane plane,  XYZ point)
        {
            XYZ vectorToPoint = point - plane.Origin;
            double dotProduct = vectorToPoint.DotProduct(plane.Normal);
            return point - dotProduct * plane.Normal;
        }

        #endregion



        #region netDxfEntity Extension Methods

        public static double ConvertUnits(this double valueInft)
        {

            return valueInft * 304.8;
        }



        public static void CreateFolder (this string folderName)
        {
            if(Directory.Exists(folderName))
            {
                foreach (var file in Directory.GetFiles(folderName))
                {
                    
                    File.Delete(file);

                }
            }


        }


        public static string RemoveInvalidChars(this string filename)
        {
            return string.Concat(filename.Split(System.IO.Path.GetInvalidFileNameChars()));

        }

        public static string GetFileName(this string header, string extension)
        {
            header = header.RemoveInvalidChars();
            return $"{DateTime.Now.Minute}_{DateTime.Now.Second}_{DateTime.Now.Millisecond}_{header}{extension}";
        }





        #endregion




        #region PlaneUtils Extension Methods
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

        public static DirectShape CreateDirectShape(
           this Autodesk.Revit.DB.Document document,
           GeometryObject geometryObject,
           BuiltInCategory builtInCategory = BuiltInCategory.OST_GenericModel)
        {
            var directShape = DirectShape.CreateElement(document, new Autodesk.Revit.DB.ElementId(builtInCategory));
            directShape.SetShape(new List<GeometryObject> { geometryObject });
            return directShape;
        }


        public static DirectShape CreateDirectShapes(
            this Autodesk.Revit.DB.Document doc,
            IEnumerable<GeometryObject> geometryObjects,
            BuiltInCategory builtInCategory = BuiltInCategory.OST_GenericModel)
        {
            var directShapes = DirectShape.CreateElement(doc, new Autodesk.Revit.DB.ElementId(builtInCategory));
            directShapes.SetShape(geometryObjects.ToList());
            return directShapes;
        }


       

        
        public static void PlaneUtils(this Autodesk.Revit.DB.Plane plane, Document document, int scale = 200)
        {
            XYZ planeOrigin = plane.Origin;
            XYZ upperRightCorner = planeOrigin + (plane.XVec * scale) + (plane.YVec * scale);
            XYZ upperLeftCorner = planeOrigin - (plane.XVec * scale) + (plane.YVec * scale);
            XYZ bottomRightCorner = planeOrigin + (plane.XVec * scale) - (plane.YVec * scale);
            XYZ bottomLeftCorner = planeOrigin - (plane.XVec * scale) - (plane.YVec * scale);

            List<GeometryObject> curves = new List<GeometryObject>();
            curves.Add(Line.CreateBound(upperRightCorner, upperLeftCorner));
            curves.Add(Line.CreateBound(upperLeftCorner, bottomLeftCorner));
            curves.Add(Line.CreateBound(bottomLeftCorner, bottomRightCorner));
            curves.Add(Line.CreateBound(bottomRightCorner, upperRightCorner));
            curves.Add(Line.CreateBound(planeOrigin, planeOrigin + plane.Normal));

            document.CreateDirectShapes(curves);
        }
        #endregion




    }
}
