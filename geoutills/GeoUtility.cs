using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DimensionSystem;
using RevitAPI.GeometryUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Plane = Autodesk.Revit.DB.Plane;

namespace GeometryAnalysis
{
    /// <summary>
    /// Utility class for geometric analysis and retrieval from Revit elements.
    /// This class centralizes a variety of helpful methods for geometry manipulation.
    /// </summary>
    /// 

    


    public static class GeoUtility
    {
        #region General methods 
        private const double Precision = 0.00001;

        public static bool IsEqual(double d1, double d2) => Math.Abs(d1 - d2) < Precision;

        public static bool IsEqual(XYZ first, XYZ second) =>
            IsEqual(first.X, second.X) && IsEqual(first.Y, second.Y) && IsEqual(first.Z, second.Z);

        public static XYZ UnitVector(XYZ vector)
        {
            double length = vector.GetLength();
            return IsEqual(length, 0) ? XYZ.Zero : vector.Normalize();
        }

        public static XYZ SubXYZ(XYZ p1, XYZ p2) => p1.Subtract(p2);

        public static XYZ AddXYZ(XYZ p1, XYZ p2) => p1.Add(p2);

        public static XYZ MultiplyVector(XYZ vector, double rate) => vector.Multiply(rate);

        public static XYZ TransformPoint(XYZ point, Transform transform) => transform.OfPoint(point);

        public static double DotMatrix(XYZ p1, XYZ p2) => p1.DotProduct(p2);

        public static XYZ CrossProduct(XYZ v1, XYZ v2) => v1.CrossProduct(v2);
        #endregion


        #region SlabBoundary

        public static bool FindLineIntersection(Line l1, Line l2, out XYZ intersection)
        {
            intersection = null;
            XYZ p1 = l1.GetEndPoint(0);
            XYZ p2 = l1.GetEndPoint(1);
            XYZ p3 = l2.GetEndPoint(0);
            XYZ p4 = l2.GetEndPoint(1);

            XYZ v1 = SubXYZ(p2, p1);
            XYZ v2 = SubXYZ(p4, p3);

            XYZ cross1 = CrossProduct(v1, v2);
            double cross1LengthSq = cross1.GetLength() * cross1.GetLength();

            if (IsEqual(cross1LengthSq, 0)) return false;

            XYZ v3 = SubXYZ(p3, p1);
            double t = CrossProduct(v3, v2).DotProduct(cross1) / cross1LengthSq;

            intersection = AddXYZ(p1, MultiplyVector(v1, t));
            return true;
        }

        public static bool IsPointOnLineSegment(XYZ point, Line line)
        {
            double dist1 = line.GetEndPoint(0).DistanceTo(point);
            double dist2 = line.GetEndPoint(1).DistanceTo(point);
            double lineLength = line.Length;
            return IsEqual(dist1 + dist2, lineLength);
        }

        // ----------------------------------------------------------------------
        // NEW METHODS FOR SHORTEST EDGE ANALYSIS
        // ----------------------------------------------------------------------


        public static bool FindShortestEdgeInfo( this Solid solid,out Edge shortestEdge,out XYZ midpoint,out XYZ perpendicularVector)
        {
            shortestEdge = null;
            midpoint = null;
            perpendicularVector = null;

            if (solid == null || solid.Edges.Size == 0)
            {
                return false;
            }

             shortestEdge = solid.Edges
             .Cast<Edge>()
            .OrderBy(e =>
            {
          var pts = e.Tessellate();
           double len = 0;
            for (int i = 0; i < pts.Count - 1; i++)
             len += pts[i].DistanceTo(pts[i + 1]); 

            return len; }) .FirstOrDefault();           

            if (shortestEdge != null)
            {
                //IList<XYZ> points = shortestEdge.Tessellate();
                XYZ startPoint = shortestEdge.Tessellate().First();
                XYZ endPoint = shortestEdge.Tessellate().Last();


                // Correct midpoint calculation
                midpoint = (startPoint + endPoint) / 2.0;

                // Calculate the edge direction vector (parallel to edge)
                XYZ edgeDirection = (endPoint - startPoint).Normalize();

                // The perpendicular vector should be the edge direction itself
                // because we want a plane perpendicular to the edge
                perpendicularVector = edgeDirection;

                return true;


            }

            return false;
        }
         
         

        public static List<Line> GetPlaneIntersectionLines(Solid solid, Plane plane, int planeScale = 100)
        {
            List<Line> intersectionLines = new List<Line>();

            try
            {
                // Create a thin plane solid for intersection using PlaneUtils
                Solid planeSolid = plane.CreateSolid(planeScale);

                if (planeSolid == null)
                    return intersectionLines;

                // Execute boolean intersection
                Solid intersectionSolid = BooleanOperationsUtils.ExecuteBooleanOperation(
                    solid, planeSolid, BooleanOperationsType.Intersect);

                if (intersectionSolid != null && intersectionSolid.Volume > 0)
                {
                    // Extract edges from intersection solid
                    foreach (Edge edge in intersectionSolid.Edges)
                    {
                        Curve edgeCurve = edge.AsCurve();
                        if (edgeCurve is Line line)
                        {
                            intersectionLines.Add(line);
                        }
                        else if (edgeCurve != null)
                        {
                            // For non-linear curves, tessellate and create line segments
                            var tessellatedPoints = edgeCurve.Tessellate();
                            for (int i = 0; i < tessellatedPoints.Count - 1; i++)
                            {
                                try
                                {
                                    Line segment = Line.CreateBound(tessellatedPoints[i], tessellatedPoints[i + 1]);
                                    intersectionLines.Add(segment);
                                }
                                catch
                                {
                                    // Skip invalid line segments
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions during boolean operations
                System.Diagnostics.Debug.WriteLine($"Error in plane intersection: {ex.Message}");
            }

            return intersectionLines;
        }


        #endregion

        
    }
}