using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using DimensionSystem;
using netDxf;
using netDxf.Entities;




namespace ExportCADX.geoutills
{
    public class netDxfEntityUtils
    {
        #region GetElementFromArc
        public static EntityObject GetElementFromArc(Autodesk.Revit.DB.Arc arc)
        {

            try
            {
                // Arc case

                if (arc.IsBound)
                {
                    // Start and end points (Revit XYZ → netDxf Vector3)
                    var start = new Vector3

                        (
                        arc.GetEndPoint(0).X.ConvertUnits(),
                        arc.GetEndPoint(0).Y.ConvertUnits(),
                        arc.GetEndPoint(0).Z.ConvertUnits()

                        );
                    var end = new Vector3
                        (
                        arc.GetEndPoint(1).X.ConvertUnits(),
                        arc.GetEndPoint(1).Y.ConvertUnits(),
                        arc.GetEndPoint(1).Z.ConvertUnits()
                        );

                    //mid 
                    var mid = new Vector3
                        (
                        arc.Tessellate()[1].X.ConvertUnits(),
                        arc.Tessellate()[1].Y.ConvertUnits(),
                        arc.Tessellate()[1].Z.ConvertUnits()
                        );

                    // NetDXF Arc: needs center, radius, start angle, end angle

                    var center = new Vector3
                        (
                        arc.Center.X.ConvertUnits(),
                        arc.Center.Y.ConvertUnits(),
                        arc.Center.Z.ConvertUnits()
                        );

                    double radius = arc.Radius.ConvertUnits();
                    // Get angles from vectors (relative to arc’s normal plane)

                    double startAngle = Math.Atan2(start.Y - center.Y, start.X - center.X) * 180 / Math.PI;

                    double endAngle = Math.Atan2(end.Y - center.Y, end.X - center.X) * 180 / Math.PI;

                    // Construct netDxf Arc
                    return new Arc(center, radius, startAngle, endAngle);
                    

                }
                else
                {
                    // Full circle case
                    var center = new Vector3
                        (
                        arc.Center.X.ConvertUnits(),
                        arc.Center.Y.ConvertUnits(),
                        arc.Center.Z.ConvertUnits()
                        );
                    double radius = arc.Radius.ConvertUnits();
                    // Construct netDxf Circle
                    return new Circle(center, radius)
                    {
                        Normal = new Vector3
                        (

                            arc.Normal.X.ConvertUnits(),
                            arc.Normal.Y.ConvertUnits(),
                            arc.Normal.Z.ConvertUnits()
                         )


                    };


                }
            }

            catch (Exception ex)
            {
                string err = ex.Message;
            }

            return null;


        }
        #endregion




        #region GetElementFromLine
        public static EntityObject GetElementFromLine(Autodesk.Revit.DB.Line revitLine)
        {

            try
            {
                if (revitLine.IsBound) // Bound → finite line
                {
                    var start = new Vector3(
                        revitLine.GetEndPoint(0).X.ConvertUnits(),
                        revitLine.GetEndPoint(0).Y.ConvertUnits(),
                        revitLine.GetEndPoint(0).Z.ConvertUnits());

                    var end = new Vector3(
                        revitLine.GetEndPoint(1).X.ConvertUnits(),
                        revitLine.GetEndPoint(1).Y.ConvertUnits(),
                        revitLine.GetEndPoint(1).Z.ConvertUnits());

                    return new Line(start, end);
                }

                else  // Unbound → treat as Ray
                {
                    var origin = new Vector3(
                        revitLine.Origin.X.ConvertUnits(),
                        revitLine.Origin.Y.ConvertUnits(),
                        revitLine.Origin.Z.ConvertUnits());

                    var direction = new Vector3(
                        revitLine.Direction.X.ConvertUnits(),
                        revitLine.Direction.Y.ConvertUnits(),
                        revitLine.Direction.Z.ConvertUnits());

                    return new Ray(origin, direction);




                }

            }

            catch (Exception ex)
            {
                string err = ex.Message;
            }

            return null;


        }



        #endregion


        #region GetElementFromPoint

        public static EntityObject GetElementFromPoint(Autodesk.Revit.DB.XYZ point)
        {
            try
            {
                var pt = new Vector3(
                    point.X.ConvertUnits(),
                    point.Y.ConvertUnits(),
                    point.Z.ConvertUnits());
                return new Point(pt);
            }
            catch (Exception ex)
            {
                string err = ex.Message;
            }
            return null;

        }

        #endregion


        #region GetElementFromEllipse

        public static EntityObject GetElementFromEllipse(Autodesk.Revit.DB.Ellipse ellipse)
        {
            try
            {
               
                var center = new Vector3
                        (
                        ellipse.Center.X.ConvertUnits(),
                        ellipse.Center.Y.ConvertUnits(),
                        ellipse.Center.Z.ConvertUnits()
                        );

                double radiusX = ellipse.RadiusX.ConvertUnits();
                double radiusY = ellipse.RadiusY.ConvertUnits();

                // Don't convert normal vector units - it's a direction vector, not a position
                var normal = new Vector3(
                    ellipse.Normal.X.ConvertUnits(),
                    ellipse.Normal.Y.ConvertUnits(),
                    ellipse.Normal.Z.ConvertUnits()
                );

                // Ensure the normal vector is normalized (unit length)
                normal = Vector3.Normalize(normal);

                return new Ellipse(center, radiusX, radiusY)
                {
                    Normal = normal
                };

            }
            catch (Exception ex)
            {
                string err = ex.Message;
            }
            return null;
        }


        #endregion


        // Create and return the Polyline3D with all vertices

        #region GetElementFromNurbSpline

        public static EntityObject GetElementFromNurbSpline(Autodesk.Revit.DB.NurbSpline nurbSpline)
        {
            try
            {
                var points = nurbSpline.Tessellate();

                if(points == null || points.Count < 2)
                {
                    return null;

                    // A polyline needs at least two vertices.
                }
                var polylineVertices = new List<Vector3>();
                foreach (var pt in points)
                {
                    polylineVertices.Add(new Vector3(
                        pt.X.ConvertUnits(),
                        pt.Y.ConvertUnits(),
                        pt.Z.ConvertUnits()));

                }

                // Create and return the Polyline3D with all vertices
                return new Polyline3D(polylineVertices);


            }
            catch (Exception ex)
            {
                string err = ex.Message;
            }
            return null;
        }



        #endregion


    }
}
