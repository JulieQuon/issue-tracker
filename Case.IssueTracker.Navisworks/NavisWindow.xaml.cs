using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.IO;
using System.Windows.Controls;
using Case.IssueTracker.Data;
using Autodesk.Navisworks.Api;
using ComBridge = Autodesk.Navisworks.Api.ComApi.ComApiBridge;
using ComApi = Autodesk.Navisworks.Api.Interop.ComApi;

namespace Case.IssueTracker.Navisworks
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class NavisWindow : UserControl
  {
    List<SavedViewpoint> _savedViewpoints = new List<SavedViewpoint>();
    List<ModelItem> _elementList;
    readonly Document _oDoc = Autodesk.Navisworks.Api.Application.ActiveDocument;

    public NavisWindow()
    {
      InitializeComponent();
      mainPan.bcfPan.AddIssueBtn.Click += new RoutedEventHandler(AddIssueBCF);
      mainPan.jiraPan.AddIssueBtn.Click += new RoutedEventHandler(AddIssueJira);
      mainPan.bcfPan.Open3dViewBtn.Click += new RoutedEventHandler(Open3dViewBCF);
      mainPan.jiraPan.Open3dViewBtn.Click += new RoutedEventHandler(Open3dViewJira);



    }
    private void AddIssueJira(object sender, EventArgs e)
    {
      try
      {
        string path = Path.Combine(Path.GetTempPath(), "BCFtemp", Path.GetRandomFileName());
        Tuple<List<IssueBCF>, List<Issue>> tup = AddIssue(path, false);
        if (tup == null)
          return;
        List<IssueBCF> issues = tup.Item1;
        List<Issue> issuesJira = tup.Item2;

        if (issues != null && issues.Any())
          mainPan.doUploadIssue(issues, path, true, mainPan.jiraPan.projIndex, issuesJira);
      }

      catch (System.Exception ex1)
      {
        MessageBox.Show("exception: " + ex1);
      }

    }
    private void AddIssueBCF(object sender, EventArgs e)
    {
      try
      {

        Tuple<List<IssueBCF>, List<Issue>> tup = AddIssue(mainPan.jira.Bcf.path, true);
        if (tup == null)
          return;
        List<IssueBCF> issues = tup.Item1; ;
        //int typeInt = tup.Item2;
        if (issues != null && issues.Any())
        {
          foreach (var i in issues)
          {
            mainPan.jira.Bcf.Issues.Add(i);
          }
          mainPan.jira.Bcf.HasBeenSaved = false;

        }
      }

      catch (System.Exception ex1)
      {
        MessageBox.Show("exception: " + ex1);
      }
    }
    private Tuple<List<IssueBCF>, List<Issue>> AddIssue(string path, bool isBcf)
    {
      try
      {
        // set image export settings
        ComApi.InwOaPropertyVec options = ComBridge.State.GetIOPluginOptions("lcodpimage");
        // configure the option "export.image.format" to export png and image size
        foreach (ComApi.InwOaProperty opt in options.Properties())
        {
          if (opt.name == "export.image.format")
            opt.value = "lcodpexpng";
          if (opt.name == "export.image.width")
            opt.value = 1600;
          if (opt.name == "export.image.height")
            opt.value = 900;

        }

        _savedViewpoints = new List<SavedViewpoint>();

        foreach (SavedItem oSI in _oDoc.SavedViewpoints.ToSavedItemCollection())
        {
          RecurseItems(oSI);
        }

        var types = new ObservableCollection<Issuetype>();
        var assignees = new List<User>();
        var components = new ObservableCollection<Component>();
        var priorities = new ObservableCollection<Priority>();
        var noCom = true;
        var noPrior = true;
        var noAssign = true;

        if (!isBcf)
        {
          types = mainPan.jira.TypesCollection;
          assignees = mainPan.getAssigneesProj();
          components = mainPan.jira.ComponentsCollection;
          priorities = mainPan.jira.PrioritiesCollection;
          noCom =
              mainPan.jira.ProjectsCollection[mainPan.jiraPan.projIndex].issuetypes[0].fields.components ==
              null;
          noPrior =
              mainPan.jira.ProjectsCollection[mainPan.jiraPan.projIndex].issuetypes[0].fields.priority ==
              null;
          noAssign =
              mainPan.jira.ProjectsCollection[mainPan.jiraPan.projIndex].issuetypes[0].fields.assignee ==
              null;

        }



        AddIssueNavis ain = new AddIssueNavis(_savedViewpoints, types, assignees, components, priorities, noCom, noPrior, noAssign);
        if (isBcf)
          ain.JiraFieldsBox.Visibility = System.Windows.Visibility.Collapsed;
        ain.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
        ain.ShowDialog();
        if (ain.DialogResult.HasValue && ain.DialogResult.Value)
        {
          int elemCheck = 2;
          if (ain.all.IsChecked.Value)
            elemCheck = 0;
          else if (ain.selected.IsChecked.Value)
            elemCheck = 1;

          List<SavedViewpoint> savedViewpointsImport = new List<SavedViewpoint>();

          for (int i = 0; i < ain.issueList.SelectedItems.Count; i++)
          {
            int index = ain.issueList.Items.IndexOf(ain.issueList.SelectedItems[i]);
            savedViewpointsImport.Add(_savedViewpoints[index]);
          }
          if (!savedViewpointsImport.Any())
            return null;
          //get selection only once!
          if (elemCheck == 1)
            _elementList = _oDoc.CurrentSelection.SelectedItems.Where(o => o.InstanceGuid != Guid.Empty).ToList<ModelItem>();

          List<IssueBCF> issues = new List<IssueBCF>();
          List<Issue> issuesJira = new List<Issue>();
          foreach (var sv in savedViewpointsImport)
          {
            Issue issueJira = new Issue();
            if (!isBcf)
            {
              issueJira.fields = new Fields();
              issueJira.fields.issuetype = (Issuetype)ain.issueTypeCombo.SelectedItem;
              issueJira.fields.priority = (Priority)ain.priorityCombo.SelectedItem;
              if (!string.IsNullOrEmpty(ain.ChangeAssign.Content.ToString()) &&
                  ain.ChangeAssign.Content.ToString() != "none")
              {
                issueJira.fields.assignee = new User();
                issueJira.fields.assignee.name = ain.ChangeAssign.Content.ToString();
              }

              if (ain.SelectedComponents != null && ain.SelectedComponents.Any())
              {
                issueJira.fields.components = ain.SelectedComponents;
              }
            }

            IssueBCF issue = new IssueBCF();
            string folderIssue = Path.Combine(path, issue.guid.ToString());
            if (!Directory.Exists(folderIssue))
              Directory.CreateDirectory(folderIssue);

            issue.snapshot = Path.Combine(folderIssue, "snapshot.png");
            // set the currtent saved viewpoint and then generate sna and BCF viewpoint
            _oDoc.SavedViewpoints.CurrentSavedViewpoint = sv;
            issue.viewpoint = generateViewpoint(sv.Viewpoint, elemCheck);
            generateSnapshot(folderIssue);

            issue.markup.Topic.Title = sv.DisplayName;
            issue.markup.Header[0].IfcProject = "";
            string projFilename = !string.IsNullOrEmpty(_oDoc.FileName) ? System.IO.Path.GetFileName(_oDoc.FileName) : "";
            issue.markup.Header[0].Filename = projFilename;
            issue.markup.Header[0].Date = DateTime.Now;

            //comment
            if (sv.Comments.Any())
            {

              foreach (var comm in sv.Comments)
              {
                var c = new CommentBCF
                {
                  Comment1 = comm.Body,
                  Topic = new CommentTopic { Guid = issue.guid.ToString() }
                };
                ;
                c.Date = DateTime.Now;
                c.VerbalStatus = comm.Status.ToString();
                c.Author = (string.IsNullOrWhiteSpace(mainPan.jira.Self.displayName)) ? UserSettings.Get("BCFusername") : mainPan.jira.Self.displayName;
                issue.markup.Comment.Add(c);
              }
            }
            issues.Add(issue);
            issuesJira.Add(issueJira);
          } // end foreach
          return new Tuple<List<IssueBCF>, List<Issue>>(issues, issuesJira);
        }
      }

      catch (Exception ex)
      {
        MessageBox.Show(ex.ToString());
      }
      return null;
    }
    public void generateSnapshot(string folderIssue)
    {
      try
      {
        string snapshot = Path.Combine(folderIssue, "snapshot.png");

        // get the state of COM
        ComApi.InwOpState10 oState = ComBridge.State;
        // get the IO plugin for image
        ComApi.InwOaPropertyVec options = oState.GetIOPluginOptions("lcodpimage");

        //export the viewpoint to the image
        oState.DriveIOPlugin("lcodpimage", snapshot, options);
        System.Drawing.Bitmap oBitmap = new System.Drawing.Bitmap(snapshot);
        System.IO.MemoryStream ImageStream = new System.IO.MemoryStream();
        oBitmap.Save(ImageStream, System.Drawing.Imaging.ImageFormat.Jpeg);
        oBitmap.Dispose();

      }
      catch (System.Exception ex1)
      {
        MessageBox.Show("exception: " + ex1);
      }
    }
    private VisualizationInfo generateViewpoint(Viewpoint oVP, int elemCheck)
    {
      double units = GetGunits();
      VisualizationInfo v = new VisualizationInfo();
      try
      {

        Vector3D vi = getViewDir(oVP);
        Vector3D up = getViewUp(oVP);
        Point3D center = new Point3D(oVP.Position.X / units, oVP.Position.Y / units, oVP.Position.Z / units);
        double zoomValue = 1;


        oVP = oVP.CreateCopy();
        if (!oVP.HasFocalDistance)
          oVP.FocalDistance = 1;

        if (oVP.Projection == ViewpointProjection.Orthographic) //IS ORTHO
        {

          double dist = oVP.VerticalExtentAtFocalDistance / 2 / units;
          zoomValue = 3.125 * dist / (up.Length * 1.25);
         v.OrthogonalCamera = new OrthogonalCamera();
          v.OrthogonalCamera.CameraViewPoint.X = center.X;
          v.OrthogonalCamera.CameraViewPoint.Y = center.Y;
          v.OrthogonalCamera.CameraViewPoint.Z = center.Z;
          v.OrthogonalCamera.CameraUpVector.X = up.X;
          v.OrthogonalCamera.CameraUpVector.Y = up.Y;
          v.OrthogonalCamera.CameraUpVector.Z = up.Z;
          v.OrthogonalCamera.CameraDirection.X = vi.X;
          v.OrthogonalCamera.CameraDirection.Y = vi.Y;
          v.OrthogonalCamera.CameraDirection.Z = vi.Z;
          v.OrthogonalCamera.ViewToWorldScale = zoomValue;
        }
        else // it is a perspective view
        {
          zoomValue = oVP.FocalDistance;

          v.PerspectiveCamera = new PerspectiveCamera();
          v.PerspectiveCamera.CameraViewPoint.X = center.X;
          v.PerspectiveCamera.CameraViewPoint.Y = center.Y;
          v.PerspectiveCamera.CameraViewPoint.Z = center.Z;
          v.PerspectiveCamera.CameraUpVector.X = up.X;
          v.PerspectiveCamera.CameraUpVector.Y = up.Y;
          v.PerspectiveCamera.CameraUpVector.Z = up.Z;
          v.PerspectiveCamera.CameraDirection.X = vi.X;
          v.PerspectiveCamera.CameraDirection.Y = vi.Y;
          v.PerspectiveCamera.CameraDirection.Z = vi.Z;
          v.PerspectiveCamera.FieldOfView = zoomValue;
        }


        if (elemCheck == 0)//visible (0)
          _elementList = _oDoc.Models.First.RootItem.DescendantsAndSelf.Where(o => o.InstanceGuid != Guid.Empty && ChechHidden(o.AncestorsAndSelf) && o.FindFirstGeometry() != null && !o.FindFirstGeometry().Item.IsHidden).ToList<ModelItem>();

        if (null != _elementList && _elementList.Any() && elemCheck != 2)//not if none (2)
        {
          v.Components = new Data.Component[_elementList.Count];
          string appname = Autodesk.Navisworks.Api.Application.Title;
          for (var i = 0; i < _elementList.Count; i++)
          {
            string ifcguid = IfcGuid.ToIfcGuid(_elementList.ElementAt(i).InstanceGuid).ToString();
            v.Components[i] = new Case.IssueTracker.Data.Component(appname, "", ifcguid);

          }
        }
      }
      catch (Exception ex)
      {
        MessageBox.Show(ex.ToString());
      }
      return v;
    }

    private bool ChechHidden(ModelItemEnumerableCollection items)
    {
      if (items.Any(o => o.IsHidden))
        return false; //an anchestor is hidden, so it the item
      return true; // all anchestors are visible


    }
    private void RecurseItems(SavedItem oSI)
    {
      try
      {
        Autodesk.Navisworks.Api.GroupItem group = oSI as Autodesk.Navisworks.Api.GroupItem;
        if (null != group)//is a group
        {
          foreach (SavedItem oSII in group.Children)
          {
            RecurseItems(oSII);
          }
        }
        else
        {
          _savedViewpoints.Add((SavedViewpoint)oSI);
        }
      }
      catch (Exception ex)
      {
        MessageBox.Show(ex.ToString());
      }

    }
    private void Open3dViewBCF(object sender, EventArgs e)
    {
      try
      {
        VisualizationInfo v = mainPan.jira.Bcf.Issues[mainPan.bcfPan.listIndex].viewpoint;
        Open3DView(v);
      }
      catch (System.Exception ex1)
      {
        MessageBox.Show("exception: " + ex1);
      }
    }
    private void Open3dViewJira(object sender, EventArgs e)
    {
      try
      {
        VisualizationInfo v = mainPan.getVisInfo();
        if (null != v)
          Open3DView(v);
      }
      catch (System.Exception ex1)
      {
        MessageBox.Show("exception: " + ex1);
      }
    }
    private void Open3DView(VisualizationInfo v)
    {
      try
      {
        //    {
        //      
        Tuple<Point3D, Vector3D, Vector3D, ViewpointProjection, double> tuple = GetViewCoordinates(v);

        if (tuple == null)
        {
          MessageBox.Show("Viewpoint not formatted correctly.", "Viewpoint Error", MessageBoxButton.OK, MessageBoxImage.Error);
          return;
        }

        Document oDoc = Autodesk.Navisworks.Api.Application.ActiveDocument;

        // get current viewpoint
        Viewpoint oCopyVP = new Viewpoint();

        oCopyVP.AlignDirection(tuple.Item3);
        oCopyVP.AlignUp(tuple.Item2);
        oCopyVP.Projection = tuple.Item4;

        const double TEKLA = 1.25;

        double x = tuple.Item5 / TEKLA;

        if (oCopyVP.Projection == ViewpointProjection.Orthographic)
        {

          oCopyVP.Position = tuple.Item1;
          oCopyVP.FocalDistance = 1;
          //top center point of view
          Point3D xyzTL = oCopyVP.Position.Add(tuple.Item2.Multiply(x));
          oCopyVP.SetExtentsAtFocalDistance(1, xyzTL.DistanceTo(oCopyVP.Position));
        }
        else
        {
          oCopyVP.FocalDistance = tuple.Item5;
          oCopyVP.Position = tuple.Item1;
        }

        oDoc.CurrentViewpoint.CopyFrom(oCopyVP);

        if (v.Components != null && v.Components.Any())
        {
          List<ModelItem> attachedElems = new List<ModelItem>();

          List<ModelItem> elems = oDoc.Models.First.RootItem.DescendantsAndSelf.ToList<ModelItem>();


          foreach (var item in elems.Where(o => o.InstanceGuid != Guid.Empty))
          {
            string ifcguid = IfcGuid.ToIfcGuid(item.InstanceGuid).ToString();
            if (v.Components.Any(o => o.IfcGuid == ifcguid))
              attachedElems.Add(item);

          }
          if (attachedElems.Any())//avoid to hide everything if no elements matches
          {
            if (UserSettings.Get("selattachedelems") == "0")
            {
              List<ModelItem> elemsVisible = new List<ModelItem>();
              foreach (var item in attachedElems)
              {
                elemsVisible.AddRange(item.AncestorsAndSelf);
              }
              foreach (var item in elemsVisible)
                elems.Remove(item);

              oDoc.Models.ResetAllHidden();
              oDoc.Models.SetHidden(elems, true);
            }
            else
            {
              oDoc.CurrentSelection.Clear();
              oDoc.CurrentSelection.AddRange(attachedElems);
            }
          }

        }
      }
      catch (Exception ex)
      {
        MessageBox.Show(ex.ToString());
      }

    }
    public Tuple<Point3D, Vector3D, Vector3D, Autodesk.Navisworks.Api.ViewpointProjection, double> GetViewCoordinates(VisualizationInfo viewport)
    {
      try
      {
        double units = GetGunits();

        Point3D Position = new Point3D();
        Vector3D VectorUp = new Vector3D();
        Vector3D VectorTo = new Vector3D();
        ViewpointProjection vp = ViewpointProjection.Perspective;
        double zoom = 0;
        if (viewport.OrthogonalCamera != null)
        {
          if (viewport.OrthogonalCamera.CameraViewPoint == null || viewport.OrthogonalCamera.CameraUpVector == null || viewport.OrthogonalCamera.CameraDirection == null)
            return null;

          vp = ViewpointProjection.Orthographic;
          zoom = units * viewport.OrthogonalCamera.ViewToWorldScale;
          Position = GetXYZ(viewport.OrthogonalCamera.CameraViewPoint.X, viewport.OrthogonalCamera.CameraViewPoint.Y, viewport.OrthogonalCamera.CameraViewPoint.Z);
          VectorUp = GetXYZ(viewport.OrthogonalCamera.CameraUpVector.X, viewport.OrthogonalCamera.CameraUpVector.Y, viewport.OrthogonalCamera.CameraUpVector.Z).ToVector3D().Normalize();
          VectorTo = GetXYZ(viewport.OrthogonalCamera.CameraDirection.X, viewport.OrthogonalCamera.CameraDirection.Y, viewport.OrthogonalCamera.CameraDirection.Z).ToVector3D().Normalize();
        }
        else if (viewport.PerspectiveCamera != null)
        {
          if (viewport.PerspectiveCamera.CameraViewPoint == null || viewport.PerspectiveCamera.CameraUpVector == null || viewport.PerspectiveCamera.CameraDirection == null)
            return null;

          zoom = viewport.PerspectiveCamera.FieldOfView;
          Position = GetXYZ(viewport.PerspectiveCamera.CameraViewPoint.X, viewport.PerspectiveCamera.CameraViewPoint.Y, viewport.PerspectiveCamera.CameraViewPoint.Z);
          VectorUp = GetXYZ(viewport.PerspectiveCamera.CameraUpVector.X, viewport.PerspectiveCamera.CameraUpVector.Y, viewport.PerspectiveCamera.CameraUpVector.Z).ToVector3D().Normalize();
          VectorTo = GetXYZ(viewport.PerspectiveCamera.CameraDirection.X, viewport.PerspectiveCamera.CameraDirection.Y, viewport.PerspectiveCamera.CameraDirection.Z).ToVector3D().Normalize();

        }
        else
          return null;

        return new Tuple<Point3D, Vector3D, Vector3D, ViewpointProjection, double>(Position, VectorUp, VectorTo, vp, zoom);
      }
      catch (Exception ex)
      {
        MessageBox.Show(ex.ToString());
      }
      return null;
    }
    private Point3D GetXYZ(double x, double y, double z)
    {
      double units = GetGunits();

      Point3D myXYZ = new Point3D(x * units, y * units, z * units);
      return myXYZ;
    }
    
    #region math rotations and stuff
    private Vector3D getViewDir(Viewpoint oVP)
    {
      double units = GetGunits();

      Rotation3D oRot = oVP.Rotation;
      // calculate view direction
      Rotation3D oNegtiveZ = new Rotation3D(0, 0, -1, 0);
      Rotation3D otempRot = MultiplyRotation3D(oNegtiveZ, oRot.Invert());
      Rotation3D oViewDirRot = MultiplyRotation3D(oRot, otempRot);
      // get view direction
      Vector3D oViewDir = new Vector3D(oViewDirRot.A, oViewDirRot.B, oViewDirRot.C);

      return oViewDir.Normalize();
    }
    private Vector3D getViewUp(Viewpoint oVP)
    {
      double units = GetGunits();

      Rotation3D oRot = oVP.Rotation;
      // calculate view direction
      Rotation3D oNegtiveZ = new Rotation3D(0, 1, 0, 0);
      Rotation3D otempRot = MultiplyRotation3D(oNegtiveZ, oRot.Invert());
      Rotation3D oViewDirRot = MultiplyRotation3D(oRot, otempRot);
      // get view direction
      Vector3D oViewDir = new Vector3D(oViewDirRot.A, oViewDirRot.B, oViewDirRot.C);

      return oViewDir.Normalize();
    }

    // help function: Multiply two Rotation3D
    private Rotation3D MultiplyRotation3D(
        Rotation3D r2,
        Rotation3D r1)
    {

      Rotation3D oRot =
          new Rotation3D(r2.D * r1.A + r2.A * r1.D +
                              r2.B * r1.C - r2.C * r1.B,
                          r2.D * r1.B + r2.B * r1.D +
                              r2.C * r1.A - r2.A * r1.C,
                          r2.D * r1.C + r2.C * r1.D +
                              r2.A * r1.B - r2.B * r1.A,
                          r2.D * r1.D - r2.A * r1.A -
                              r2.B * r1.B - r2.C * r1.C);

      oRot.Normalize();

      return oRot;

    }
    private double GetGunits()
    {
      string units = _oDoc.Units.ToString();
      double factor = 1;
      switch (units)
      {
        case "Centimeters":
          factor = 100;
          break;
        case "Feet":
          factor = 3.28084;
          break;
        case "Inches":
          factor = 39.3701;
          break;
        case "Kilometers":
          factor = 0.001;
          break;
        case "Meters":
          factor = 1;
          break;
        case "Micrometers":
          factor = 1000000;
          break;
        case "Miles":
          factor = 0.000621371;
          break;
        case "Millimeters":
          factor = 1000;
          break;
        case "Mils":
          factor = 39370.0787;
          break;
        case "Yards":
          factor = 1.09361;
          break;
        default:
          MessageBox.Show("Units " + units + " not recognized.");
          factor = 1;
          break;
      }
      return factor;
    }

    #endregion
  }
}
