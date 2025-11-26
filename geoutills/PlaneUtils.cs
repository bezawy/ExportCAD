using Autodesk.Revit.DB;
using System.Collections.Generic;
using DimensionSystem;

namespace RevitAPI.GeometryUtils
{
    /// <summary>  
    /// Provides utility methods for working with planes in Revit.  
    /// </summary>  
    public static class PlaneUtils
    {
        /// <summary>  
        /// Visualizes the given plane in the Revit document by creating direct shapes representing the plane's boundaries and normal.  
        /// </summary>  
        /// <param name="plane">The plane to visualize.</param>  
        /// <param name="document">The Revit document where the visualization will be created.</param>  
        /// <param name="scale">The scale factor for the visualization. Default is 3.</param>  
        public static void Visualize(this Autodesk.Revit.DB.Plane plane, Document document, int scale = 3)
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


         /// <summary>
        /// Creates a thin solid representing the plane for boolean operations
        /// Uses the same geometry logic as the Visualize method
        /// </summary>
        /// <param name="plane">The plane to create solid from</param>
        /// <param name="scale">Scale factor for the plane size (default: 100)</param>
        /// <param name="thickness">Thickness of the plane solid (default: 0.01)</param>
        /// <returns>Solid representing the plane</returns>
        public static Solid CreateSolid(this Autodesk.Revit.DB.Plane plane, int scale = 100, double thickness = 0.01)
        {
            try
            {
                XYZ planeOrigin = plane.Origin;
                XYZ upperRightCorner = planeOrigin + (plane.XVec * scale) + (plane.YVec * scale);
                XYZ upperLeftCorner = planeOrigin - (plane.XVec * scale) + (plane.YVec * scale);
                XYZ bottomRightCorner = planeOrigin + (plane.XVec * scale) - (plane.YVec * scale);
                XYZ bottomLeftCorner = planeOrigin - (plane.XVec * scale) - (plane.YVec * scale);

                // Create bottom face profile (same logic as Visualize method)
                List<Curve> bottomProfile = new List<Curve>
                {
                    Line.CreateBound(upperRightCorner, upperLeftCorner),
                    Line.CreateBound(upperLeftCorner, bottomLeftCorner),
                    Line.CreateBound(bottomLeftCorner, bottomRightCorner),
                    Line.CreateBound(bottomRightCorner, upperRightCorner)
                };

                CurveLoop bottomLoop = CurveLoop.Create(bottomProfile);
                List<CurveLoop> bottomLoops = new List<CurveLoop> { bottomLoop };

                // Create solid by extrusion
                Solid planeSolid = GeometryCreationUtilities.CreateExtrusionGeometry(
                    bottomLoops, plane.Normal, thickness);

                return planeSolid;
            }
            catch
            {
                return null;
            }
        }
    }
}
