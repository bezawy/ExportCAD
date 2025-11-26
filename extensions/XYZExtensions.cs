using Autodesk.Revit.DB;
using RevitAPI.Extensions;
using System.Collections.Generic;
namespace CoreRevitModule.Extensions
{
    public static class XYZExtensions
    {
        public static void Visualize(this XYZ point, Document doc)
        {
            // Fix: CreateDirectShape expects a single GeometryObject, not a list.  
            doc.CreateDirectShape(Point.Create(point));
        }

        /// <summary>  
        /// This method is used to get a curve from vector  
        /// </summary>  
        /// <param name="vector">Given vector</param>  
        /// <param name="origin">Origin</param>  
        /// <param name="length">Length</param>  
        /// <returns></returns>  
        public static Curve AsCurve(this XYZ vector, XYZ origin = null, double? length = null)
        {
            // Fix for CS8370: Replace coalescing assignment with explicit null checks  
            if (origin == null)
            {
                origin = XYZ.Zero;
            }
            if (length == null)
            {
                length = vector.GetLength();
            }

            return Line.CreateBound(
              origin,
              origin.MoveAlongVector(vector.Normalize(), length.GetValueOrDefault()));
        }

        /// <summary>  
        /// This method is used to move point along a given vector and distance  
        /// </summary>  
        /// <param name="pointToMove">Point to move</param>  
        /// <param name="vector">Given vector</param>  
        /// <param name="distance">Given distance</param>  
        /// <returns></returns>  
        public static XYZ MoveAlongVector(
          this XYZ pointToMove, XYZ vector, double distance) => pointToMove.Add(vector * distance);

        /// <summary>  
        /// This method is used to move point along a given vector  
        /// </summary>  
        /// <param name="pointToMove">Point to move</param>  
        /// <param name="vector">Given vector</param>  
        /// <returns>Moved point</returns>  
        public static XYZ MoveAlongVector(this XYZ pointToMove, XYZ vector)
        {
            return pointToMove.Add(vector);
        }

        /// <summary>  
        /// This method is used to get normalized vector by curve  
        /// </summary>  
        /// <param name="curve"></param>  
        /// <returns></returns>  
        public static XYZ ToNormalizedVector(this Curve curve)
        {
            return (curve.GetEndPoint(1) - curve.GetEndPoint(0)).Normalize();
        }
    }
}