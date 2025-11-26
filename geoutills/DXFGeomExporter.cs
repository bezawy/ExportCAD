using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using netDxf;
using netDxf.Entities;
using DimensionSystem;
using Autodesk.Revit.DB;

namespace ExportCADX.geoutills
{
    public class DXFGeomExporter
    {

        /// <summary>
        /// Needed units for export
        /// 
        #region Needed units for export
        public enum ExportUnits
        {
            /// <summary>
            /// Футы
            /// </summary>
            Ft = 0,

            /// <summary>
            /// Миллиметры
            /// </summary>
            Mm = 1
        }

       public static ExportUnits units = ExportUnits.Ft;

        public static string FolderName = @"C:\Temp";



        /// <param name="folderName">Full path to the folder where files will be saved. The default path is "C:\Temp\"</param>

        public static void Init(string folderName)
        {
            if (string.IsNullOrWhiteSpace(folderName))
                FolderName = @"C:\Temp";
            else
                FolderName = folderName;

            // ensure folder exists
            FolderName.CreateFolder();
        }

        public static void Init (string folderName, ExportUnits _units ,bool clearfolder)
        {
            FolderName = string.IsNullOrWhiteSpace(folderName) ? @"C:\Temp" : folderName;
        units = _units;

        if (clearfolder)
            FolderName.CreateFolder();

        }

        #endregion


        #region netDXF helper Export Methods

        public static void ExportCurves (IEnumerable<Curve> curves , string header)
        {
            try
            {
                FolderName.CreateFolder();

                // Create a new DXF document
                DxfDocument dxf = new DxfDocument();

                foreach (var curve in curves)
                {

                    // Handle Line
                    #region Line handling
                    if (curve is Autodesk.Revit.DB.Line revitLine)
                    {
                        var entity = netDxfEntityUtils.GetElementFromLine(revitLine);

                       
                         if (entity != null)
                         {
                            dxf.Entities.Add(entity);
                        
                         }


                    }
                    #endregion


                    // Handle Arc (includes full circle case
                    #region Arc handling
                    else if (curve is Autodesk.Revit.DB.Arc arc)
                    {
                        var entity = netDxfEntityUtils.GetElementFromArc(arc);
                        if (entity != null)
                        {
                            dxf.Entities.Add(entity);
                        }
                    }

                    #endregion


                    // Handle Point



                }

                // Save DXF file
                string fileName = header.GetFileName(".Dxf"); 

                string filePath = Path.Combine(FolderName, fileName);

                dxf.Save(filePath);



            }

            catch (Exception ex)
            {
                string err = ex.Message;
            }

        }

        public static void ExportCurve(Curve curve , string header)
        {
            FolderName.CreateFolder();

            // Create a new DXF document
            DxfDocument dxf = new DxfDocument();

            foreach (var c in new List<Curve > { curve})
            {

                EntityObject entity =
                    c is Autodesk.Revit.DB.Line revitLine ?
                 netDxfEntityUtils.GetElementFromLine(revitLine) :
                    c is Autodesk.Revit.DB.Arc arc ?
                    netDxfEntityUtils.GetElementFromArc(arc) :
                    c is Autodesk.Revit.DB.Ellipse ellipse ?
                    netDxfEntityUtils.GetElementFromEllipse(ellipse) :
                   
                    null;
                if (entity != null)
                    {
                    dxf.Entities.Add(entity);
                }


            }

            // Save DXF file

            string fileName = header.GetFileName(".Dxf");
            string filePath = Path.Combine(FolderName, fileName);
            dxf.Save(filePath);


        }


        #endregion


        #region Needed walls ,familyinstances



        public static void ExportWallsByFaces(IEnumerable<Wall> walls, string header)
        {
            Options options = new Options();
            List<Curve> curves = new List<Curve>();
            foreach (Wall wall in walls)
            {
                IEnumerable<GeometryObject> geometry = wall.get_Geometry(options);
                foreach (GeometryObject geometryObject in geometry)
                {
                    if (geometryObject is Autodesk.Revit.DB.Solid solid)
                    {
                        foreach (Face face in solid.Faces)
                        {
                            foreach (EdgeArray edgeArray in face.EdgeLoops)
                            {
                                foreach (Edge edge in edgeArray)
                                    curves.Add(edge.AsCurve());
                            }
                        }
                    }
                }
            }

            ExportCurves(curves, header);
        }
        public static void ExportWallByFaces(Wall wall, string header)
        {
            Options options = new Options();
            List<Curve> curves = new List<Curve>();

            IEnumerable<GeometryObject> geometry = wall.get_Geometry(options);
            foreach (GeometryObject geometryObject in geometry)
            {
                if (geometryObject is Autodesk.Revit.DB.Solid solid)
                {
                    foreach (Face face in solid.Faces)
                    {
                        foreach (EdgeArray edgeArray in face.EdgeLoops)
                        {
                            foreach (Edge edge in edgeArray)
                                curves.Add(edge.AsCurve());
                        }
                    }
                }
            }

            ExportCurves(curves, header);
        }


        public static void ExportFamilyInstancesByFaces(IEnumerable<FamilyInstance> families,string header, bool includeNonVisibleObjects)
  
        {
            try
            {
                Options options = new Options
                {
                    IncludeNonVisibleObjects = includeNonVisibleObjects,
                    DetailLevel = ViewDetailLevel.Fine
                };

                List<Curve> curves = new List<Curve>();

                foreach (FamilyInstance familyInstance in families)
                {
                    var familyCurves = GetCurvesFromFamilyGeo(familyInstance, options);
                    curves.AddRange(familyCurves);
                }

                if (curves.Any())
                {
                    ExportCurves(curves, header);
                }
            }
            catch (Exception ex)
            {
                string err = ex.Message;
            }
        }


        public static void ExportFamilyInstanceByFaces( FamilyInstance familyInstance,string header,bool includeNonVisibleObjects)
        {
            try
            {
                Options options = new Options
                {
                    IncludeNonVisibleObjects = includeNonVisibleObjects,
                    DetailLevel = ViewDetailLevel.Fine
                };

                List<Curve> curves = GetCurvesFromFamilyGeo(familyInstance, options).ToList();

                if (curves.Any())
                {
                    ExportCurves(curves, header);
                }
            }
            catch (Exception ex)
            {
                string err = ex.Message;
            }
        }

        #endregion

        #region Extract GeometryObjects 

        public static void ExportSolidsByFaces(IEnumerable<Autodesk.Revit.DB.Solid> solids, string header)
        {
           FolderName.CreateFolder();
            List<Face> faces = new List<Face>();
            foreach (Autodesk.Revit.DB.Solid solid in solids)
            {
                foreach (Face solidFace in solid.Faces)
                {
                    faces.Add(solidFace);
                }
            }

            if (faces.Any())
                ExportFaces((IEnumerable<PlanarFace>)faces, header);
        }

        public static void ExportSolid(Autodesk.Revit.DB.Solid solid, string header)
        {
           FolderName.CreateFolder();
            List<Face> faces = new List<Face>();

            foreach (Face solidFace in solid.Faces)
            {
                faces.Add(solidFace);
            }

            if (faces.Any())
                ExportFaces((IEnumerable<PlanarFace>)faces, header);
        }
        public static void ExportFace(Face face, string header)
        {
            FolderName.CreateFolder();
            List<Curve> wallCurves = new List<Curve>();

            EdgeArrayArray edgeArrayArray = face.EdgeLoops;
            foreach (EdgeArray edgeArray in edgeArrayArray)
            {
                foreach (Edge edge in edgeArray)
                {
                    wallCurves.Add(edge.AsCurve());
                }
            }

            ExportCurves(wallCurves, header);

        }
        public static void ExportFaces(IEnumerable<PlanarFace> planarFaces , string header)
        {
            FolderName.CreateFolder();
            List<Curve> wallCurves = new List<Curve>();

            foreach (PlanarFace planarFace in planarFaces)
            {
                EdgeArrayArray edgeArrayArray = planarFace.EdgeLoops;
                foreach (EdgeArray edgeArray in edgeArrayArray)
                {
                    foreach (Edge edge in edgeArray)
                    {
                        wallCurves.Add(edge.AsCurve());
                    }
                }
            }

            ExportCurves(wallCurves, header);

        }
        public static void ExportEdges(IEnumerable<Edge> edges, string header)
        {
            FolderName.CreateFolder();
            List<Curve> curves = new List<Curve>(); // empty list of curves

            foreach (Edge edge in edges)
            {
                curves.Add(edge.AsCurve());
            }

            ExportCurves(curves, header);

        }


        public static IEnumerable<Curve> GetCurvesFromFamilyGeo(FamilyInstance family, Options options)
        {
            var curves = new List<Curve>();

            try
            {
                // Get the family's geometry element
                var geometryElement = family.get_Geometry(options);
                if (geometryElement == null)
                {
                    return curves;
                }

                // Process geometry recursively WITHOUT applying transform
                foreach (GeometryObject geometryObject in geometryElement)
                {
                    curves.AddRange(ProcessGeometryObject(geometryObject));
                }
            }
            catch (Exception ex)
            {
                string err = ex.Message;
            }

            return curves;
        }

        public static List<Curve> ProcessGeometryObject(GeometryObject geometryObject)
        {
            var curves = new List<Curve>();

            try
            {
                // Handle solid geometry
                if (geometryObject is Autodesk.Revit.DB.Solid solid && solid.Volume > 0)
                {
                    foreach (Face solidFace in solid.Faces)
                    {
                        foreach (EdgeArray edgeArray in solidFace.EdgeLoops)
                        {
                            foreach (Edge edge in edgeArray)
                            {
                                var curve = edge.AsCurve();
                                if (curve != null)
                                {
                                    curves.Add(curve);
                                }
                            }
                        }
                    }
                }
                // Handle face geometry directly
                else if (geometryObject is Face face)
                {
                    foreach (EdgeArray edgeArray in face.EdgeLoops)
                    {
                        foreach (Edge edge in edgeArray)
                        {
                            var curve = edge.AsCurve();
                            if (curve != null)
                            {
                                curves.Add(curve);
                            }
                        }
                    }
                }
                // Handle nested geometry instances (important for families!)
                else if (geometryObject is GeometryInstance instance)
                {
                    var instanceGeometry = instance.GetInstanceGeometry();
                    if (instanceGeometry != null)
                    {
                        foreach (GeometryObject nestedObject in instanceGeometry)
                        {
                            curves.AddRange(ProcessGeometryObject(nestedObject));
                        }
                    }
                }
                // Handle curve geometry directly
                else if (geometryObject is Curve curve)
                {
                    curves.Add(curve);
                }
                // Handle lines directly
                else if (geometryObject is Autodesk.Revit.DB.Line line)
                {
                    curves.Add(line);
                }
                // Handle arcs directly
                else if (geometryObject is Autodesk.Revit.DB.Arc arc)
                {
                    curves.Add(arc);
                }
            }
            catch (Exception ex)
            {
                string err = ex.Message;
            }

            return curves;
        }

        #endregion

    }
}
