using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows;
using Case.IssueTracker.Data;
using Case.IssueTracker.Revit.Data;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

namespace Case.IssueTracker.Revit.Entry
{
  /// <summary>
  /// Obfuscation Ignore for External Interface
  /// </summary>
  [Obfuscation(Exclude = true, ApplyToMembers = false)]
  public class ExtOpenView : IExternalEventHandler
  {

    //public Tuple<ViewOrientation3D, double, string, string> touple;
    public VisualizationInfo v;

    /// <summary>
    /// External Event Implementation
    /// </summary>
    /// <param name="app"></param>
    public void Execute(UIApplication app)
    {

      try
      {

        UIDocument uidoc = app.ActiveUIDocument;
        Document doc = uidoc.Document;
        //Selection m_elementsToHide = uidoc.Selection; //SelElementSet.Create();

        List<ElementId> elementids = new List<ElementId>();



        // IS ORTHOGONAL
        if (v.OrthogonalCamera != null)
        {
          if (v.OrthogonalCamera.ViewToWorldScale == null || v.OrthogonalCamera.CameraViewPoint == null || v.OrthogonalCamera.CameraUpVector == null || v.OrthogonalCamera.CameraDirection == null)
            return;
          //type = "OrthogonalCamera";
          var zoom = UnitUtils.ConvertToInternalUnits(v.OrthogonalCamera.ViewToWorldScale, DisplayUnitType.DUT_METERS);
          var CameraDirection = Utils.GetXYZ(v.OrthogonalCamera.CameraDirection.X, v.OrthogonalCamera.CameraDirection.Y, v.OrthogonalCamera.CameraDirection.Z);
          var CameraUpVector = Utils.GetXYZ(v.OrthogonalCamera.CameraUpVector.X, v.OrthogonalCamera.CameraUpVector.Y, v.OrthogonalCamera.CameraUpVector.Z);
          var CameraViewPoint = Utils.GetXYZ(v.OrthogonalCamera.CameraViewPoint.X, v.OrthogonalCamera.CameraViewPoint.Y, v.OrthogonalCamera.CameraViewPoint.Z);
          var orient3d = Utils.ConvertBasePoint(doc, CameraViewPoint, CameraDirection, CameraUpVector, true);


          View3D orthoView = null;
          //if active view is 3d ortho use it
          if (doc.ActiveView.ViewType == ViewType.ThreeD)
          {
            View3D ActiveView3D = doc.ActiveView as View3D;
            if (!ActiveView3D.IsPerspective)
              orthoView = ActiveView3D;
          }
          if (orthoView == null)
          {
            IEnumerable<View3D> viewcollector3D = get3DViews(doc);
            //try to use default 3D view
            if (viewcollector3D.Any() && viewcollector3D.Where(o => o.Name == "{3D}" || o.Name == "BCFortho").Any())
              orthoView = viewcollector3D.Where(o => o.Name == "{3D}" || o.Name == "BCFortho").First();


          }
          using (Transaction trans = new Transaction(uidoc.Document))
          {
            if (trans.Start("Open orthogonal view") == TransactionStatus.Started)
            {
              //create a new 3d ortho view 
              if (orthoView == null)
              {
                orthoView = View3D.CreateIsometric(doc, getFamilyViews(doc).First().Id);
                orthoView.Name = "BCFortho";
              }

              orthoView.SetOrientation(orient3d);
              trans.Commit();
            }
          }
          uidoc.ActiveView = orthoView;
          //adjust view rectangle

          // **** CUSTOM VALUE FOR TEKLA **** //
          // double x = touple.Item2
          // **** CUSTOM VALUE FOR TEKLA **** //
          double customZoomValue = (ProjectSettings.Get("useDefaultZoom", doc.PathName) == "1") ? 1 : 2.5;
          double x = zoom / customZoomValue;
          XYZ m_xyzTl = uidoc.ActiveView.Origin.Add(uidoc.ActiveView.UpDirection.Multiply(x)).Subtract(uidoc.ActiveView.RightDirection.Multiply(x));
          XYZ m_xyzBr = uidoc.ActiveView.Origin.Subtract(uidoc.ActiveView.UpDirection.Multiply(x)).Add(uidoc.ActiveView.RightDirection.Multiply(x));
          uidoc.GetOpenUIViews().First().ZoomAndCenterRectangle(m_xyzTl, m_xyzBr);
        }

        else if (v.PerspectiveCamera != null)
        {
          if (v.PerspectiveCamera.FieldOfView == null || v.PerspectiveCamera.CameraViewPoint == null || v.PerspectiveCamera.CameraUpVector == null || v.PerspectiveCamera.CameraDirection == null)
            return;

          var zoom = v.PerspectiveCamera.FieldOfView;
          double z1 = 18 / Math.Tan(zoom / 2 * Math.PI / 180);//focale 1
          double z = 18 / Math.Tan(25 / 2 * Math.PI / 180);//focale, da controllare il 18, vedi PDF
          double factor = z1 - z;

          var CameraDirection = Utils.GetXYZ(v.PerspectiveCamera.CameraDirection.X, v.PerspectiveCamera.CameraDirection.Y, v.PerspectiveCamera.CameraDirection.Z);
          var CameraUpVector = Utils.GetXYZ(v.PerspectiveCamera.CameraUpVector.X, v.PerspectiveCamera.CameraUpVector.Y, v.PerspectiveCamera.CameraUpVector.Z);
          XYZ oldO = Utils.GetXYZ(v.PerspectiveCamera.CameraViewPoint.X, v.PerspectiveCamera.CameraViewPoint.Y, v.PerspectiveCamera.CameraViewPoint.Z);
          var CameraViewPoint = (oldO.Subtract(CameraDirection.Divide(factor)));
          var orient3d = Utils.ConvertBasePoint(doc, CameraViewPoint, CameraDirection, CameraUpVector, true);



          View3D perspView = null;

          IEnumerable<View3D> viewcollector3D = get3DViews(doc);
          if (viewcollector3D.Any() && viewcollector3D.Where(o => o.Name == "BCFpersp").Any())
            perspView = viewcollector3D.Where(o => o.Name == "BCFpersp").First();
          using (Transaction trans = new Transaction(uidoc.Document))
          {
            if (trans.Start("Open perspective view") == TransactionStatus.Started)
            {
              if (null == perspView)
              {
                perspView = View3D.CreatePerspective(doc, getFamilyViews(doc).First().Id);
                perspView.Name = "BCFpersp";
              }

              perspView.SetOrientation(orient3d);

              // turn off the far clip plane with standard parameter API 
              if (perspView.get_Parameter(BuiltInParameter.VIEWER_BOUND_ACTIVE_FAR).HasValue)
              {
                Parameter m_farClip = perspView.get_Parameter(BuiltInParameter.VIEWER_BOUND_ACTIVE_FAR);
                m_farClip.Set(0);

              }
              perspView.CropBoxActive = true;
              perspView.CropBoxVisible = true;

              trans.Commit();
            }
          }
          uidoc.ActiveView = perspView;
        }
        else if (v.SheetCamera != null)//sheet
        {
          //using (Transaction trans = new Transaction(uidoc.Document))
          //{
          //    if (trans.Start("Open sheet view") == TransactionStatus.Started)
          //    {
          IEnumerable<View> viewcollectorSheet = getSheets(doc, v.SheetCamera.SheetID);
          if (!viewcollectorSheet.Any())
          {
            MessageBox.Show("No Sheet with Id=" + v.SheetCamera.SheetID + " found.");
            return;
          }
          uidoc.ActiveView = viewcollectorSheet.First();
          uidoc.RefreshActiveView();

          //        trans.Commit();
          //    }
          //}
          XYZ m_xyzTl = new XYZ(v.SheetCamera.TopLeft.X, v.SheetCamera.TopLeft.Y,
                      v.SheetCamera.TopLeft.Z);
          XYZ m_xyzBr = new XYZ(v.SheetCamera.BottomRight.X, v.SheetCamera.BottomRight.Y,
                      v.SheetCamera.BottomRight.Z);
          uidoc.GetOpenUIViews().First().ZoomAndCenterRectangle(m_xyzTl, m_xyzBr);

        }
        else
        {
          return;
        }
        //select/hide elements
        if (v.Components != null && v.Components.Any())
        {
          FilteredElementCollector collector = new FilteredElementCollector(doc, doc.ActiveView.Id).WhereElementIsNotElementType();
          ICollection<ElementId> collection = collector.ToElementIds();
          foreach (var e in v.Components)
          {
            var bcfguid = IfcGuid.FromIfcGUID(e.IfcGuid);
            var ids = collection.Where(o => bcfguid == ExportUtils.GetExportId(doc, o));
            if (ids.Any())
            {
              //m_elementsToHide.Add(doc.GetElement(ids.First()));
              elementids.Add(ids.First());
            }
          }
          if (null != elementids && elementids.Count > 0)
          {
            //do transaction only if there is something to hide/select
            using (Transaction trans = new Transaction(uidoc.Document))
            {
              if (trans.Start("Apply visibility/selection") == TransactionStatus.Started)
              {

                if (UserSettings.Get("selattachedelems") == "0")
                {
                  uidoc.ActiveView.IsolateElementsTemporary(elementids);
                }
                else
                {
#if Version2014
                  uidoc.Selection.Elements.Clear();
                  foreach (var elementid in elementids)
                  {
                    uidoc.Selection.Elements.Add(doc.GetElement(elementid));
                  }
#elif Version2015
 uidoc.Selection.SetElementIds(elementids);
#elif Version2016
                  uidoc.Selection.SetElementIds(elementids);
#endif
                }

              }
              trans.Commit();
            }
          }
        }


        uidoc.RefreshActiveView();
      }
      catch (Exception ex)
      {
        TaskDialog.Show("Error!", "exception: " + ex);
      }
    }
    private System.Collections.Generic.IEnumerable<ViewFamilyType> getFamilyViews(Document doc)
    {

      return from elem in new FilteredElementCollector(doc).OfClass(typeof(ViewFamilyType))
             let type = elem as ViewFamilyType
             where type.ViewFamily == ViewFamily.ThreeDimensional
             select type;
    }
    private IEnumerable<View3D> get3DViews(Document doc)
    {
      return from elem in new FilteredElementCollector(doc).OfClass(typeof(View3D))
             let view = elem as View3D
             select view;
    }
    private IEnumerable<View> getSheets(Document doc, int id)
    {
      ElementId eid = new ElementId(id);
      return from elem in new FilteredElementCollector(doc).OfClass(typeof(View))
             let view = elem as View
             where view.Id == eid
             select view;
    }
    private string ToBcfViewName(string name)
    {
      Regex rgx = new Regex("[^a-zA-Z0-9 -]");
      name = rgx.Replace(name, "");
      return "BCF-" + name;
    }



    public string GetName()
    {
      return "Open 3D View";
    }
    // returns XYZ and ZOOM/FOV value


    public static string MakeUniqueFileName(string file, System.Collections.Generic.IEnumerable<View3D> views)
    {
      string fn;

      for (int i = 0; ; ++i)
      {
        fn = file + i.ToString();
        System.Collections.Generic.IEnumerable<View3D> m_nviews = from elem in views
                                                                  let type = elem as View3D
                                                                  where type.Name == fn
                                                                  select type;

        if (m_nviews.Count() == 0)
          return fn;
      }
    }

    private bool convertToBool(string s)
    {
      return s != "" && Convert.ToBoolean(s);
    }
  }

}