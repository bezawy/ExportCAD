using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using DimensionSystem;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;



namespace Methods

{


    public static class GeneralMethods
    {
        
        public static void ShowTask(string name)
        {
            TaskDialog.Show("title", name);

        } // showtask

        public static void Visualise(IList<GeometryObject> Geomtryobject, Document document)
        {
            DirectShape.CreateElement(document, new ElementId(BuiltInCategory.OST_GenericModel)).SetShape(Geomtryobject);

        } // show visualise

        public static double Round(double Number)
        {
            return Math.Round(Number, 5);

        }  //Round numbers to equal

        public static Solid GetSolid(Element Element)
        {
            return Element.get_Geometry(new Options())
                .OfType<Solid>()
                .FirstOrDefault(s => s != null && s.Volume > 0);
        } // region Soild

        #region  Edge_face 
        public static XYZ GetFaceOrigin(Face face)
        {
            List<XYZ> Points = face.EdgeLoops.get_Item(0).Cast<Edge>().
                Select(e => e.AsCurve().Evaluate(0.5, true)).ToList();
            return Getaveragepoint(Points);
        }
        #endregion

        public static XYZ Getaveragepoint(List<XYZ> points)
        {
            double Xaverage = points.Select(p => p.X).ToList().Average();
            double Yaverage = points.Select(p => p.Y).ToList().Average();
            double Zaverage = points.Select(p => p.Z).ToList().Average();

            return new XYZ(Xaverage, Yaverage, Zaverage);


        }  // get averagepoints


       

        #region Rebar

        //public static void SetConstraintToCoverDistance(Rebar Rebar, RebarHandleType HandleType, RebarConstraintType ConstraintType,
        //    ConstrainDistance DistanceType, double Distance, Element Host = null)
        //{
        //    RebarConstraintsManager ConstrainManager = Rebar.GetRebarConstraintsManager();

        //    RebarConstrainedHandle handle = ConstrainManager.GetAllHandles()
        //          .Where(h => h.GetHandleType() == HandleType).First();
        //    List<RebarConstraint> constrain = new List<RebarConstraint>(); //To know about it if it's host not null
        //    if (Host != null)
        //    {
        //        constrain = ConstrainManager.
        //         GetConstraintCandidatesForHandle(handle, Host.Id).ToList();

        //    }
        //    else
        //    {
        //        constrain = ConstrainManager.
        //       GetConstraintCandidatesForHandle(handle).ToList();
        //    }


        //    List<RebarConstraint> StartConstraintsToCover =
        //        constrain.Where(e => e.GetConstraintType() == ConstraintType).ToList();

        //    List<double> TocoverDistance = new List<double>();
        //    if (ConstraintType == RebarConstraintType.ToCover)
        //    {
        //        TocoverDistance = StartConstraintsToCover.Select(w => Math.Abs(w.GetDistanceToTargetCover())).ToList();
        //    }
        //    else if (ConstraintType == RebarConstraintType.FixedDistanceToHostFace)
        //    {
        //        TocoverDistance = StartConstraintsToCover.Select(v => Math.Abs(v.GetDistanceToTargetHostFace())).ToList();

        //    }



        //    RebarConstraint Constraint;
        //    if (DistanceType == ConstrainDistance.min)
        //    {
        //        Constraint = StartConstraintsToCover[TocoverDistance.IndexOf(TocoverDistance.Min())];
        //    }

        //    else
        //    {
        //        Constraint = StartConstraintsToCover[TocoverDistance.IndexOf(TocoverDistance.Max())];

        //    }



        //    Constraint.SetDistanceToTargetCover(Distance);

        //    ConstrainManager.SetPreferredConstraintForHandle(handle, Constraint);

        //    doc.Regenerate();

        //} // Set Constraint to Cover By Distance

        #endregion



        #region Helper Method for Centroid Calculation
        public static XYZ GetCentroid(List<XYZ> points)
        {
            if (points == null || points.Count == 0)
                return XYZ.Zero;

            double x = 0, y = 0, z = 0;
            foreach (XYZ point in points)
            {
                x += point.X;
                y += point.Y;
                z += point.Z;
            }

            int count = points.Count;
            return new XYZ(x / count, y / count, z / count);
        }
        #endregion

        


    }

}

