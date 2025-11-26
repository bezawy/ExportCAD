using ExportCADX.geoutills;
using ExportCADX.ViewModels;
using ExportCADX.Views;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using DimensionSystem;
using GeometryAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

[Transaction(TransactionMode.Manual)]
public class MainCommand : IExternalCommand
{
    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
        try
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;

            // Create ViewModel with Revit context
            var viewModel = new ViewCommand(uidoc);

            UserControl1 userControl1 = new UserControl1();
            userControl1.DataContext = new ViewCommand(uidoc); 
            userControl1.ShowDialog();
         

            return Result.Succeeded;
        }
        catch (Exception ex)
        {
            message = ex.Message;
            return Result.Failed;
        }
    }
}

