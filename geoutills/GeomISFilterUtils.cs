using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeometryAnalysis
{
    public class GeomISFilterUtils
    {

        #region walls
        public class WallSFilter : ISelectionFilter
            {
                public bool AllowElement(Element elem)
                {
                    return elem is Wall;
                }

                public bool AllowReference(Reference reference, XYZ position)
                {
                    return false; // restricts to walls only
                }
            }
        #endregion

        #region FamilyInstance
        public class FamilyISFilter : ISelectionFilter
            {
                public bool AllowElement(Element elem)
                {
                    return elem is FamilyInstance;
                }

                public bool AllowReference(Reference reference, XYZ position)
                {
                    return false; // restricts to family instances only
                }
            }
        #endregion

        #region Floors
        public class FloorISFilter : ISelectionFilter
        {
            public bool AllowElement(Element elem)
            {
                return elem is Floor;
            }

            public bool AllowReference(Reference reference, XYZ position)
            {
                return false; // restricts to floors only
            }
        }

        #endregion


    }

}
