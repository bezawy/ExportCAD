
using ExportCADX.geoutills;
using ExportCADX.Models;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using CommunityToolkit.Mvvm.Collections;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GeometryAnalysis;
using Methods;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Linq;

namespace ExportCADX.ViewModels
{
    public partial class ViewCommand : ObservableObject
    {
        private readonly UIDocument _uidoc;
        private readonly Document _doc;
        private bool flag;

        public ViewCommand(UIDocument uidoc)
        {
            _uidoc = uidoc;
            _doc = _uidoc.Document;


            ExportingType = new ObservableCollection<string>
            {
                "Walls",
                "Windows",
                "Structural Framing"
            };
        
            IsExportEnabled = true;
            SelectedViewType = ExportingType[0];
            ExportFolderPath = "C:\\Temp\\Exports";
            IsDwgExport = true; // This will make the DWG radio button selected by default

        }


        [ObservableProperty] private ObservableCollection<string> exportingType;

        [ObservableProperty] private string selectedViewType;

        [ObservableProperty] private bool isExportEnabled;

        [ObservableProperty]  private string exportFolderPath;

        // --- Radio Button Handling
        [ObservableProperty]  private bool isDxfExport;

        [ObservableProperty] private bool isDwgExport;

        [ObservableProperty]  private bool isXMLExport;

        [ObservableProperty] private string format;
        partial void OnIsDxfExportChanged(bool value)
        {
            if(flag) return;

            if (value)
            {
                flag = true;
                IsDwgExport =  false;
                IsXMLExport = false;
                
                format = "XML";
                flag = false;
            }


        }
       



        [RelayCommand]
        private void BrowseFolder()
        {

            var dialog = new FolderBrowserDialog
            {
                Description = "Select an export folder",
            };


            if(dialog.ShowDialog() == DialogResult.OK) 
            
            { ExportFolderPath = dialog.SelectedPath; }

        }

        [RelayCommand]

        /// <summary>
        /// OK command is just a flag for closing the window.
        /// Actual Revit selection/export logic will run in Execute().
        /// </summary>
        private void Ok()
        {
            string selectedFormat = IsDxfExport ? "DXF" : (IsDwgExport ? "DWG" : "XML");

            var request = new ExportRequest
            {
                ElementType = SelectedViewType,
                Format = selectedFormat,
                FolderPath = ExportFolderPath

            };


            try
            {
                RunExport(request);

                TaskDialog.Show("Export", $"Export complete!\n\nType: {request.ElementType}\nFormat: {request.Format}\nFolder: {request.FolderPath}");
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException)
            {
                // user cancelled selection
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Error", $"Export failed: {ex.Message}");
            }
        }

        private void RunExport(ExportRequest request)
        {
            DXFGeomExporter.Init(request.FolderPath);

            if (request.Format == "DXF")
            {
                if (request.ElementType == "Walls")
                {
                    var refs = _uidoc.Selection.PickObjects(Autodesk.Revit.UI.Selection.ObjectType.Element,new GeomISFilterUtils.WallSFilter(), "Select Walls to Export");
                    var walls = refs.Select(r => _doc.GetElement(r) as Wall).Where(w => w != null).ToList();
                    DXFGeomExporter.ExportWallsByFaces(walls, "Walls");
                }

                else if (request.ElementType == "Windows")
                {
                  var refs = _uidoc.Selection.PickObjects(Autodesk.Revit.UI.Selection.ObjectType.Element, new GeomISFilterUtils.FamilyISFilter(), "Select Windows to Export");
                  var windows = refs.Select(r => _doc.GetElement(r) as FamilyInstance).Where(f => f != null).ToList();
                  DXFGeomExporter.ExportFamilyInstancesByFaces(windows, "Windows", false);
                }

                else if (request.ElementType == "Structural Framing")
                {
                    var refs = _uidoc.Selection.PickObjects(Autodesk.Revit.UI.Selection.ObjectType.Element, new GeomISFilterUtils.FamilyISFilter(), "Select Beams/Framing to Export");
                      var beams = refs.Select(r => _doc.GetElement(r) as FamilyInstance).Where(f => f != null).ToList();
                      DXFGeomExporter.ExportFamilyInstancesByFaces(beams, "Framing", false);
                }


            }

            else if (request.Format == "DWG")
            {
                TaskDialog.Show("Export", "DWG export not implemented yet.");
            }
            else if (request.Format == "XML")
            {
                TaskDialog.Show("Export", "XML export not implemented yet.");
            }



        }

    }
}