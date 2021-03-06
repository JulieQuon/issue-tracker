﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;

//more references
using System.IO;
using IOPath = System.IO.Path;
using System.ComponentModel;
using System.Collections.ObjectModel;
using Case.IssueTracker.Data;
using Case.IssueTracker.Windows;
using Ionic.Zip;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Reflection;
using Case.IssueTracker.Data.JIRA;
using RestSharp;
using System.Windows.Input;

namespace Case.IssueTracker.UserControls
{
  public partial class MainPanel : UserControl
  {

    public Jira jira = new Jira();

    public MainPanel()
    {
      InitializeComponent();
      JiraClient.Waiter = jiraPan.waiter;

      // Assign "this" to JiraPanel for passing auto-complete items
      jiraPan.SetMainPanel(this);

      string codeBase = System.Reflection.Assembly.GetExecutingAssembly().CodeBase;
      UriBuilder uri = new UriBuilder(codeBase);
      string path = Uri.UnescapeDataString(uri.Path);

      //if (!File.Exists(Path.Combine(Path.GetDirectoryName(path), "Ionic.Zip.dll")))
      //{
      //  MessageBox.Show("Required Ionic.Zip.dll Not Found");
      //  this.IsEnabled = false;
      //  return;
      //}
      //if (!File.Exists(Path.Combine(Path.GetDirectoryName(path), "RestSharp.dll")))
      //{
      //  MessageBox.Show("Required RestSharp.dll Not Found");
      //  this.IsEnabled = false;
      //  return;
      //}

      version.Content = "v" + Assembly.GetExecutingAssembly().GetName().Version.ToString();
      try
      {
        #region registering event
        //BCF events
        bcfPan.DelIssueBtn.Click += new RoutedEventHandler(DelBCFIssue);
        bcfPan.UpIssueBtn.Click += new RoutedEventHandler(UploadBCFIssues);
        bcfPan.AddCommBtn.Click += new RoutedEventHandler(AddBCFComment);
        bcfPan.DelCommBtn.Click += new RoutedEventHandler(DelBCFComm);
        bcfPan.NewBCFBtn.Click += new RoutedEventHandler(NewBCF);
        bcfPan.SaveBCFBtn.Click += new RoutedEventHandler(SaveBCF2);
        bcfPan.OpenBCFBtn.Click += new RoutedEventHandler(OpenBCFFile);
        bcfPan.ComponentsShowBCFEH += new EventHandler<IntArg>(ComponentsShowBCF);
        bcfPan.OpenImageBtn.Click += new RoutedEventHandler(OpenImage);
        //JIRA events
        jiraPan.DelIssueBtn.Click += new RoutedEventHandler(DelJiraIssueButt_Click);
        jiraPan.ExpIssueBtn.Click += new RoutedEventHandler(ExportJiraIssueToBcf2);
        jiraPan.ConncetBtn.Click += new RoutedEventHandler(connectClick);
        jiraPan.RefreshBtn.Click += new RoutedEventHandler(refresh);
        jiraPan.projCombo.SelectionChanged += new SelectionChangedEventHandler(projCombo_SelectionChanged);
        jiraPan.OpenLinkIssueBtn.Click += new RoutedEventHandler(OpenLink);
        jiraPan.OpenLinkProjBtn.Click += new RoutedEventHandler(OpenLink);
        jiraPan.AddCommBtn.Click += new RoutedEventHandler(AddJiraComment);
        jiraPan.OpenImageBtn.Click += new RoutedEventHandler(OpenImage);
        jiraPan.ComponentsShowBCFEH += new EventHandler<IntArg>(ComponentsShowJira);
        jiraPan.NextIssues.Click += new RoutedEventHandler(GetNextIssues);
        jiraPan.PrevIssues.Click += new RoutedEventHandler(GetPrevIssues);
        jiraPan.ApplyFilters.Click += new RoutedEventHandler(ApplyFiltersClick);
        jiraPan.ChangeType.Click += new RoutedEventHandler(ChangeType_Click);
        jiraPan.ChangeAssign.Click += new RoutedEventHandler(ChangeAssign_Click);
        jiraPan.ChangePriority.Click += new RoutedEventHandler(ChangePriority_Click);
        jiraPan.ChangeStatus.Click += new RoutedEventHandler(ChangeStatus_Click);
        jiraPan.ChangeComponents.Click += new RoutedEventHandler(ChangeComponents_Click);
        #endregion
        DataContext = jira;
        //initialize
        jira.ProjectsCollection = new ObservableCollection<Project>();
        jira.IssuesCollection = new ObservableCollection<Issue>();
        NewBCF(null, null);
        tabControl.SelectedIndex = SettingsTabIndexGet();
        connectToJira();
      }
      catch (System.Exception ex1)
      {
        MessageBox.Show("exception: " + ex1);
      }
    }

    #region JIRA
    private void refresh(object sender, RoutedEventArgs e)
    {

      getProjects();
    }
    private void getProjects()
    {
      try
      {
        var request = new RestRequest("issue/createmeta?expand=projects.issuetypes.fields", Method.GET);

        request.AddHeader("Content-Type", "application/json");
        request.RequestFormat = RestSharp.DataFormat.Json;

        List<RestRequest> requests = new List<RestRequest>();
        requests.Add(request);
        BackgroundJira bj = new BackgroundJira();
        bj.WorkerComplete += new EventHandler<ResponseArg>(getProjectsCompleted);
        bj.Start<Projects>(requests);

      }
      catch (System.Exception ex1)
      {
        MessageBox.Show("exception: " + ex1, "Error!");
      }
    }

    private void getProjectsCompleted(object sender, ResponseArg e)
    {
      try
      {
        IRestResponse<Projects> response = e.Responses.Last() as IRestResponse<Projects>;
        int projIndex = jiraPan.projIndex;

        if (RestCallback.Check(response) && response.Data.projects != null && response.Data.projects.Any())
        {
          jira.ProjectsCollection = new ObservableCollection<Project>();
          foreach (var project in response.Data.projects)
          {
            jira.ProjectsCollection.Add(project);
          }
          getPriorities();
          if (projIndex == -1) //just opened the app
          {
            string projstring = UserSettings.Get("currentproj");
            projIndex = (!string.IsNullOrWhiteSpace(projstring)) ? Convert.ToInt16(projstring) : 0;

          }

          jiraPan.projIndex = (jira.ProjectsCollection.Count <= projIndex) ? 0 : projIndex;
        }
      }
      catch (System.Exception ex1)
      {
        MessageBox.Show("exception: " + ex1, "Error!");
      }
    }
    private void projCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      try
      {
        //clear filters!
        jiraPan.clearFilters_Click(null, null);

        if (jiraPan.projIndex == -1)
          return;
        getIssues(0);
        //filters
        jira.TypesCollection = new ObservableCollection<Issuetype>();
        jira.ProjectsCollection[jiraPan.projIndex].issuetypes.OrderBy(o => o.id);
        foreach (var item in jira.ProjectsCollection[jiraPan.projIndex].issuetypes)
          jira.TypesCollection.Add(item);
        getStatuses();
        //getPriorities();
        updateTypeFilter();

        if (jira.ProjectsCollection[jiraPan.projIndex].issuetypes[0].fields.components == null)
          jiraPan.ChangeComponentsLabel.Visibility =
              jiraPan.ChangeComponents.Visibility = Visibility.Collapsed;
        else
        {
          jiraPan.ChangeComponentsLabel.Visibility = jiraPan.ChangeComponents.Visibility = Visibility.Visible;
          getComponents();
        }


      }
      catch (System.Exception ex1)
      {
        MessageBox.Show("exception: " + ex1, "Error!");
      }
    }
    private void getIssues(int startAt)//project has changed!!
    {
      try
      {
        if (jiraPan.projIndex == -1)
          return;


        var request = new RestRequest();
        string fields = "&fields=summary,key,created,updated,description,assignee,comment,attachment,creator,status,priority,resolution,issuetype,components," + UserSettings.Get("guidfield") + "&startAt=" + startAt;
        string query = "search?jql=project=" + jira.ProjectsCollection[jiraPan.projIndex].key + jiraPan.Assignation + jiraPan.Creator + jiraPan.Filters + jiraPan.Order + fields;

        request = new RestRequest(query, Method.GET);
        request.AddHeader("Content-Type", "application/json");
        request.RequestFormat = RestSharp.DataFormat.Json;
        if (jiraPan.IsFilterActive)
        {
          jiraPan.filterExpander.Header = "Filters - FILTERS ARE ACTIVE";
          jiraPan.filterExpander.Background = System.Windows.Media.Brushes.LightGoldenrodYellow;
        }
        else
        {
          jiraPan.filterExpander.Header = "Filters";
          jiraPan.filterExpander.Background = System.Windows.Media.Brushes.WhiteSmoke;
        }
        //MessageBox.Show(query);

        List<RestRequest> requests = new List<RestRequest>();
        requests.Add(request);
        BackgroundJira bj = new BackgroundJira();
        bj.WorkerComplete += new EventHandler<ResponseArg>(getIssuesCompleted);
        bj.Start<Issues>(requests);

      }
      catch (System.Exception ex1)
      {
        MessageBox.Show("exception: " + ex1);
      }
    }
    private void getIssuesCompleted(object sender, ResponseArg e)
    {
      try
      {
        jira.IssuesCollection = new ObservableCollection<Issue>();
        IRestResponse<Issues> response = e.Responses.Last() as IRestResponse<Issues>;
        if (RestCallback.Check(response) && response.Data.issues != null && response.Data.issues.Any())
        {

          jira.maxResults = response.Data.maxResults;
          jira.Total = response.Data.total;
          jira.startAt = response.Data.startAt;


          foreach (var issue in response.Data.issues)
          {
            issue.fields.comment.comments = issue.fields.comment.comments.OrderByDescending(o => o.created).ToList();
            jira.IssuesCollection.Add(issue);
          }
          jiraPan.listIndex = 0;
          jiraPan.issueCount.Content = String.Format("{0}-{1} of {2} {3}",
              jira.startAt + 1, jira.startAt + jira.IssuesCollection.Count(), jira.Total, (jira.IssuesCollection.Count() > 1) ? "Issues" : "Issue");

          jiraPan.NextIssues.IsEnabled = (jira.startAt + jira.maxResults < jira.Total) ? true : false;
          jiraPan.PrevIssues.IsEnabled = (jira.startAt != 0) ? true : false;
        }
        jiraPan.lastSynced.Text = "Last synced: " + DateTime.Now.ToShortTimeString();
      }
      catch (System.Exception ex1)
      {
        MessageBox.Show("exception: " + ex1);
      }
    }
    private void GetNextIssues(object sender, RoutedEventArgs e)
    {
      if (jira.startAt + jira.maxResults < jira.Total)//I have more Issues I can load
      {
        getIssues(jira.startAt + jira.maxResults);
      }
    }
    private void GetPrevIssues(object sender, RoutedEventArgs e)
    {
      if (jira.startAt != 0)//I have more Issues I can load
      {
        getIssues(jira.startAt - jira.maxResults);
      }
    }
    private void updateTypeFilter()
    {
      List<String> types = jira.TypesCollection.Select(item => item.name).ToList();
      jiraPan.typefilter.updateList(types);
    }
    public void getComponents()
    {
      try
      {
        // GET STATUSES
        jira.ComponentsCollection = new ObservableCollection<Data.Component>();
        var request = new RestRequest("project/" + jira.ProjectsCollection[jiraPan.projIndex].key + "/components", Method.GET);
        request.AddHeader("Content-Type", "application/json");
        request.RequestFormat = RestSharp.DataFormat.Json;
        request.OnBeforeDeserialization = resp => { resp.ContentType = "application/json"; };

        List<RestRequest> requests = new List<RestRequest>();
        requests.Add(request);
        BackgroundJira bj = new BackgroundJira();
        bj.WorkerComplete += new EventHandler<ResponseArg>(getComponentsCompleted);
        bj.Start<List<Data.Component>>(requests);
      }
      catch (System.Exception ex1)
      {
        MessageBox.Show("exception: " + ex1);
      }
    }
    private void getComponentsCompleted(object sender, ResponseArg e)
    {
      try
      {
        IRestResponse<List<Data.Component>> response = e.Responses.Last() as IRestResponse<List<Data.Component>>;
        if (RestCallback.Check(response) && response.Data != null && response.Data.Any())
        {
          List<String> stats = response.Data.Select(item => item.name).ToList();
          jiraPan.componentfilter.updateList(stats);
          foreach (var p in response.Data)
          {
            jira.ComponentsCollection.Add(p);
          }
        }

      }
      catch (System.Exception ex1)
      {
        MessageBox.Show("exception: " + ex1);
      }
    }
    private void getPriorities()
    {
      try
      {
        // GET STATUSES
        jira.PrioritiesCollection = new ObservableCollection<Priority>();
        var request = new RestRequest("priority", Method.GET);
        request.AddHeader("Content-Type", "application/json");
        request.RequestFormat = RestSharp.DataFormat.Json;
        request.OnBeforeDeserialization = resp => { resp.ContentType = "application/json"; };

        List<RestRequest> requests = new List<RestRequest>();
        requests.Add(request);
        BackgroundJira bj = new BackgroundJira();
        bj.WorkerComplete += new EventHandler<ResponseArg>(getPrioritiesCompleted);
        bj.Start<List<Priority>>(requests);

      }
      catch (System.Exception ex1)
      {
        MessageBox.Show("exception: " + ex1);
      }
    }
    private void getPrioritiesCompleted(object sender, ResponseArg e)
    {
      try
      {
        IRestResponse<List<Priority>> response = e.Responses.Last() as IRestResponse<List<Priority>>;
        if (RestCallback.Check(response) && response.Data != null && response.Data.Any())
        {
          List<String> stats = response.Data.Select(item => item.name).ToList();
          jiraPan.priorityfilter.updateList(stats);
          foreach (var p in response.Data)
          {
            jira.PrioritiesCollection.Add(p);
          }
        }
      }
      catch (System.Exception ex1)
      {
        MessageBox.Show("exception: " + ex1);
      }
    }
    private void getStatuses()
    {
      try
      {
        // GET STATUSES
        var request = new RestRequest("project/" + jira.ProjectsCollection[jiraPan.projIndex].key + "/statuses", Method.GET);
        request.AddHeader("Content-Type", "application/json");
        request.RequestFormat = RestSharp.DataFormat.Json;
        request.OnBeforeDeserialization = resp => { resp.ContentType = "application/json"; };

        List<RestRequest> requests = new List<RestRequest>();
        requests.Add(request);
        BackgroundJira bj = new BackgroundJira();
        bj.WorkerComplete += new EventHandler<ResponseArg>(getStatusesCompleted);
        bj.Start<List<ProjectStatuses>>(requests);

      }
      catch (System.Exception ex1)
      {
        MessageBox.Show("exception: " + ex1);
      }
    }
    private void getStatusesCompleted(object sender, ResponseArg e)
    {
      try
      {
        IRestResponse<List<ProjectStatuses>> response = e.Responses.Last() as IRestResponse<List<ProjectStatuses>>;
        if (RestCallback.Check(response) && response.Data[0] != null && response.Data[0].statuses != null && response.Data[0].statuses.Any())
        {
          response.Data[0].statuses.OrderBy(o => o.id);

          List<String> stats = response.Data[0].statuses.Select(item => item.name).ToList();
          jiraPan.statusfilter.updateList(stats);
        }

      }
      catch (System.Exception ex1)
      {
        MessageBox.Show("exception: " + ex1);
      }
    }

    public void getLabels(string labelToSearch)
    {
      try
      {
        string queryString = string.Format("jql/autocompletedata/suggestions?fieldName=labels&fieldValue={0}", labelToSearch);
        var request = new RestRequest(queryString, Method.GET);
        request.AddHeader("Content-Type", "application/json");
        request.RequestFormat = RestSharp.DataFormat.Json;
        request.OnBeforeDeserialization = resp => { resp.ContentType = "application/json"; };

        List<RestRequest> requests = new List<RestRequest>();
        requests.Add(request);
        BackgroundJira bj = new BackgroundJira();
        bj.WorkerComplete += new EventHandler<ResponseArg>(getLabelsCompleted);
        bj.Start<AutoCompleteQuery>(requests);

      }
      catch (System.Exception ex1)
      {
        MessageBox.Show("exception: " + ex1.Message);
      }
    }

    private void getLabelsCompleted(object sender, ResponseArg e)
    {
      try
      {
        IRestResponse<AutoCompleteQuery> response = e.Responses.Last() as IRestResponse<AutoCompleteQuery>;
        if (RestCallback.Check(response) && response.Data != null)
        {
          if (response.Data.results != null)
          {
            if (response.Data.results.Count() > 0)
            {
              List<string> items = new List<string>();
              response.Data.results.ToList().ForEach(result => items.Add(result.value));
              jiraPan.SetAutoCompleteItems(items);
            }
          }
        }
      }
      catch (System.Exception ex1)
      {
        MessageBox.Show("exception: " + ex1);
      }
    }

    private void connectClick(object sender, RoutedEventArgs e)
    {
      if (jira.Self != null && !string.IsNullOrWhiteSpace(jira.Self.key))
        disconnectJira();
      else
      {
        if (string.IsNullOrWhiteSpace(UserSettings.Get("username")) || string.IsNullOrWhiteSpace(UserSettings.Get("password")))
        {
          MessageBox.Show("Please set a Username and Password in the Settings dialog.", "Missing Information",
              MessageBoxButton.OK, MessageBoxImage.Stop);
          return;
        }
        connectToJira();
      }
    }
    private void connectToJira()
    {
      try
      {

        string usernameS = UserSettings.Get("username");
        string passwordS = DataProtector.DecryptData(UserSettings.Get("password"));
        string jiraserver = UserSettings.Get("jiraserver");

        if (string.IsNullOrWhiteSpace(usernameS) || string.IsNullOrWhiteSpace(passwordS) || string.IsNullOrWhiteSpace(jiraserver))
        {
          disconnectJira();
          return;
        }
        List<RestRequest> requests = new List<RestRequest>();

        JiraClient.Client = new RestClient(jiraserver);
        JiraClient.Client.CookieContainer = new CookieContainer();
        var request = new RestRequest("rest/auth/1/session", Method.POST);
        request.AddHeader("Content-Type", "application/json");
        request.RequestFormat = RestSharp.DataFormat.Json;
        // request.Timeout = 4000;

        request.AddBody(new
        {
          username = usernameS,
          password = passwordS
        });

        requests.Add(request);



        var request2 = new RestRequest("rest/api/2/myself", Method.GET);
        request2.AddHeader("Content-Type", "application/json");
        request2.RequestFormat = RestSharp.DataFormat.Json;


        requests.Add(request2);
        BackgroundJira bj = new BackgroundJira();
        bj.WorkerComplete += new EventHandler<ResponseArg>(connectToJiraCompleted);
        bj.Start<Self>(requests, false);


      }
      catch (System.Exception ex1)
      {
        MessageBox.Show("exception: " + ex1);
      }


    }

    private void connectToJiraCompleted(object sender, ResponseArg e)
    {
      try
      {
        IRestResponse<Self> response = e.Responses.Last() as IRestResponse<Self>;

        if ((response.StatusCode != System.Net.HttpStatusCode.OK
            && response.StatusCode != System.Net.HttpStatusCode.Created
            && response.StatusCode != System.Net.HttpStatusCode.NoContent)
            || response == null)
        {
          MessageBox.Show("Error logging into Jira server.\n" +
          "Please check your Jira username and password in the Issue Tracker Settings.\n", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
          disconnectJira();
        }
        else
        {
          string jiraserver = UserSettings.Get("jiraserver");
          JiraClient.Client.BaseUrl = new Uri(jiraserver + "/rest/api/2/");
          jira.Self = response.Data;
          jiraPan.ConncetBtn.Content = "Disconnect";

          getProjects(); // FINALLY SYNC
        }
      }
      catch (System.Exception ex1)
      {
        MessageBox.Show("exception: " + ex1);
      }
    }

    private void disconnectJira()
    {
      try
      {
        jira.Self = new Self();
        jira.ProjectsCollection = new ObservableCollection<Project>();
        jira.IssuesCollection = new ObservableCollection<Issue>();
        jiraPan.lastSynced.Text = "Logged out";
        jiraPan.ConncetBtn.Content = "Connect";
        jiraPan.projIndex = -1;


      }
      catch (System.Exception ex1)
      {
        MessageBox.Show("exception: " + ex1);
      }

    }
    private void ComponentsShowJira(object sender, IntArg e)
    {
      try
      {
        int index = e.Myint;
        string sv = jira.IssuesCollection[index].viewpoint;
        if (string.IsNullOrEmpty(sv))
        {
          MessageBox.Show("No viewpoint found.", "No Viewpoint", MessageBoxButton.OK, MessageBoxImage.Error);
          return;
        }
        var client = new RestClient();
        client.CookieContainer = JiraClient.Client.CookieContainer;
        var request3 = new RestRequest(sv, Method.GET);
        var response3 = client.Execute(request3);

        if (!RestCallback.Check(response3))
          return;


        VisualizationInfo viewpoint = deserializeView(response3.Content);
        if (null != viewpoint)
          ComponentsShow(viewpoint.Components);
      }
      catch (System.Exception ex1)
      {
        MessageBox.Show("exception: " + ex1);
      }
    }
    private void AddJiraComment(object sender, RoutedEventArgs e)
    {
      try
      {
        if (jiraPan.issueList.SelectedItems.Count > 1)
          if (MessageBox.Show("Action will apply to all the " + jiraPan.issueList.SelectedItems.Count + " selected issues,\n are you sure to continue?", "Multiple Items", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
            return;


        AddComment ac = new AddComment();
        ac.commentStatusBox.Visibility = System.Windows.Visibility.Collapsed;
        ac.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
        ac.ShowDialog();
        if (ac.DialogResult.HasValue && ac.DialogResult.Value)
        {
          List<RestRequest> requests = new List<RestRequest>();

          for (int i = 0; i < jiraPan.issueList.SelectedItems.Count; i++)
          {
            int index = jiraPan.issueList.Items.IndexOf(jiraPan.issueList.SelectedItems[i]);

            //Post the comment
            var request = new RestRequest("issue/" + jira.IssuesCollection[index].key + "/comment", Method.POST);
            request.AddHeader("Content-Type", "application/json");
            request.RequestFormat = RestSharp.DataFormat.Json;
            var newcomment = new { body = ac.comment.Text };
            request.AddBody(newcomment);

            requests.Add(request);
          }
          BackgroundJira bj = new BackgroundJira();
          bj.WorkerComplete += new EventHandler<ResponseArg>(AddJiraCommentCompleted);
          bj.Start<Comment2>(requests);
        }
      }
      catch (System.Exception ex1)
      {
        MessageBox.Show("exception: " + ex1);
      }
    }
    private void AddJiraCommentCompleted(object sender, ResponseArg e)
    {
      try
      {
        getIssues(jira.startAt);

      }
      catch (System.Exception ex1)
      {
        MessageBox.Show("exception: " + ex1);
      }
    }
    private void DelJiraIssueButt_Click(object sender, RoutedEventArgs e)
    {
      if (jiraPan.listIndex == -1)
      {
        MessageBox.Show("No Issue selected", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        return;
      }
      MessageBoxResult answer = MessageBox.Show("Are you sure you want to\nDelete the selected Issue(s)?", "Delete Issue?",
                     MessageBoxButton.YesNo, MessageBoxImage.Question);
      if (answer == MessageBoxResult.No)
        return;

      List<RestRequest> requests = new List<RestRequest>();

      BackgroundJira bj = new BackgroundJira();
      bj.WorkerComplete += new EventHandler<ResponseArg>(DelJiraIssueButt_ClickCompleted);


      for (int i = 0; i < jiraPan.issueList.SelectedItems.Count; i++)
      {
        int index = jiraPan.issueList.Items.IndexOf(jiraPan.issueList.SelectedItems[i]);


        var request = new RestRequest("issue/" + jira.IssuesCollection[index].id + ".json", Method.DELETE);
        request.AddHeader("Content-Type", "application/json");
        request.RequestFormat = RestSharp.DataFormat.Json;
        requests.Add(request);
      }

      bj.Start<Comment2>(requests);


    }
    private void DelJiraIssueButt_ClickCompleted(object sender, ResponseArg e)
    {
      try
      {
        IRestResponse response = e.Responses.Last();
        getIssues(0);
        jiraPan.listIndex = 0;
      }
      catch (System.Exception ex1)
      {
        MessageBox.Show("exception: " + ex1);
      }
    }
    private void ExportJiraIssue(object sender, RoutedEventArgs e)
    {
      try
      {
        if (jiraPan.issueList.SelectedItems.Count == 0)
        {
          MessageBox.Show("Please select one or more Isses first.", "No Issue selected", MessageBoxButton.OK, MessageBoxImage.Error);
          return;
        }
        // Show save file dialog box
        string filename = SaveDialog("Jira Export " + DateTime.Now.ToShortDateString().Replace("/", "-"));

        // Process save file dialog box results
        if (!string.IsNullOrWhiteSpace(filename))
        {


          string ReportFolder = IOPath.Combine(IOPath.GetTempPath(), "BCFtemp", IOPath.GetRandomFileName());
          if (!Directory.Exists(ReportFolder))
            Directory.CreateDirectory(ReportFolder);

          int errors = 0;

          foreach (object t in jiraPan.issueList.SelectedItems)
          {
            int index = jiraPan.issueList.Items.IndexOf(t);
            Issue issue = jira.IssuesCollection[index];
            if (issue.viewpoint == "" || issue.snapshotFull == "")
            {
              errors++;
              continue;
            }

            Guid guid = Guid.NewGuid();
            if (!string.IsNullOrWhiteSpace(issue.fields.guid))
              Guid.TryParse(issue.fields.guid, out guid);
            string g = guid.ToString();

            if (!Directory.Exists(IOPath.Combine(ReportFolder, g)))
              Directory.CreateDirectory(IOPath.Combine(ReportFolder, g));

            XDocument markup = new XDocument(
                new XElement("Markup",
                    new XElement("Header",
                        new XElement("File", new XAttribute("IfcProject", ""),
                            new XElement("Filename", "Jira Export"),
                            new XElement("Date",
                                DateTime.Parse(issue.fields.created).ToString("yyyy-MM-ddTHH':'mm':'sszzz")))),
                    new XElement("Topic", new XAttribute("Guid", g),
                        new XElement("ReferenceLink"),
                        new XElement("Title", issue.fields.summary))));

            issue.fields.comment.comments.Reverse();
            foreach (var comm in issue.fields.comment.comments)
            {
              markup.Element("Markup")
                  .Add(new XElement("Comment", new XAttribute("Guid", Guid.NewGuid().ToString()),
                      new XElement("VerbalStatus", issue.fields.status.name),
                      new XElement("Status", "Unknown"),
                      new XElement("Date",
                          DateTime.Parse(comm.created).ToString("yyyy-MM-ddTHH':'mm':'sszzz")),
                      new XElement("Author", comm.author.displayName),
                      new XElement("Comment", comm.body),
                      new XElement("Topic", new XAttribute("Guid", g))));

            }
            saveSnapshotViewpoint(issue.viewpoint, IOPath.Combine(ReportFolder, g, "viewpoint.bcfv"));
            saveSnapshotViewpoint(issue.snapshotFull, IOPath.Combine(ReportFolder, g, "snapshot.png"));
            markup.Save(IOPath.Combine(ReportFolder, g, "markup.bcf"));
          }

          if (errors != 0)
          {
            MessageBox.Show(errors + " Issue/s were not exported because not formatted correctly.",
                "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);

            if (errors == jiraPan.issueList.SelectedItems.Count)
            {
              DeleteDirectory(ReportFolder);
              return;
            }
          }


          if (File.Exists(filename))
            File.Delete(filename);


          using (Ionic.Zip.ZipFile zip = new Ionic.Zip.ZipFile())
          {
            zip.AddDirectory(ReportFolder);
            zip.Save(filename);
          }

          DeleteDirectory(ReportFolder);
          if (File.Exists(filename))
          {
            string argument = @"/select, " + filename;
            System.Diagnostics.Process.Start("explorer.exe", argument);
          }

        }
      }
      catch (System.Exception ex1)
      {
        MessageBox.Show("exception: " + ex1);
      }
    }
    private void ExportJiraIssueToBcf2(object sender, RoutedEventArgs e)
    {
      try
      {
        if (jiraPan.issueList.SelectedItems.Count == 0)
        {
          MessageBox.Show("Please select one or more Isses first.", "No Issue selected", MessageBoxButton.OK, MessageBoxImage.Error);
          return;
        }

        if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
        {
          MessageBoxResult result = MessageBox.Show("Do you want to save as BCF 1.0 format? (not recommended)", "Save as BCF 1.0", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);
          if (result == MessageBoxResult.Yes)
          {
            ExportJiraIssue(sender, e);
            return;
          }
        }

        // Save to BCF 2.0
        BcfAdapter.SaveBcf2FromJira(this);
      }
      catch (System.Exception ex1)
      {
        MessageBox.Show("exception: " + ex1);
      }
    }
    public void saveSnapshotViewpoint(string ca, string path)
    {
      var client = new RestClient();
      client.CookieContainer = JiraClient.Client.CookieContainer;
      var request = new RestRequest(ca, Method.GET);

      var response = client.Execute(request);
      if (RestCallback.Check(response))
        File.WriteAllBytes(path, response.RawBytes);

    }
    private void OpenLink(object sender, RoutedEventArgs e)
    {
      string content = (string)((Button)sender).Tag;
      System.Diagnostics.Process.Start(UserSettings.Get("jiraserver") + "/browse/" + content);
    }
    private void ApplyFiltersClick(object sender, RoutedEventArgs e)
    {
      getIssues(jira.startAt);
    }
    #region chande metadata
    public void ChangeStatus_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        if (jiraPan.issueList.SelectedItems.Count > 1)
          if (
              MessageBox.Show("Action will apply ONLY to the first Issue selected.", "Multiple Items",
                  MessageBoxButton.OK, MessageBoxImage.Warning) == MessageBoxResult.No)
            return;
        // GET STATUSES
        var transitions = JiraUtils.GeTransitions(jira.IssuesCollection[jiraPan.issueList.SelectedIndex].key);

        if (null == transitions)
          return;

        ChangeValue cv = new ChangeValue();
        cv.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        transitions = transitions.OrderBy(o => o.name).ToList();
        cv.valuesList.ItemsSource = transitions;
        int indexitem = jiraPan.issueList.SelectedIndex;
        cv.valuesList.SelectedIndex = 0;
        cv.Title = "Change Status";
        DataTemplate statusTemplate = cv.FindResource("statusTemplate") as DataTemplate;
        cv.valuesList.ItemTemplate = statusTemplate;

        cv.ShowDialog();
        if (cv.DialogResult.HasValue && cv.DialogResult.Value)
        {
          JiraUtils.SetTransition(jira.IssuesCollection[jiraPan.issueList.SelectedIndex].key,
              transitions[cv.valuesList.SelectedIndex].id,
              new EventHandler<ResponseArg>(ChangeValue_Completed));
        }
      }
      catch (System.Exception ex1)
      {
        MessageBox.Show("exception: " + ex1);
      }

    }
    public void ChangeComponents_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        if (jiraPan.issueList.SelectedItems.Count > 1)
          if (
              MessageBox.Show("Action will apply to all the selected items,\n sure to continue?",
                  "Multiple Items", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
            return;

        int oldindex = jiraPan.issueList.SelectedIndex;
        ChangeValue cv = new ChangeValue();
        cv.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        List<Data.Component> components =
            jira.ComponentsCollection.ToList<Data.Component>();
        cv.valuesList.ItemsSource = components;
        cv.valuesList.SelectionMode = SelectionMode.Multiple;
        cv.valuesList.SelectedIndex = -1;
        int indexitem = jiraPan.issueList.SelectedIndex;
        if (jira.IssuesCollection[indexitem].fields.components != null &&
            jira.IssuesCollection[indexitem].fields.components.Any())
        {

          foreach (var o in jira.IssuesCollection[indexitem].fields.components)
          {
            var selindex = Data.IndexByName.Get(o.name, "name", components);
            if (selindex != -1)
              cv.valuesList.SelectedItems.Add(cv.valuesList.Items[selindex]);
          }
        }

        cv.Title = "Change Components";
        DataTemplate componentTemplate = cv.FindResource("componentTemplate") as DataTemplate;
        cv.valuesList.ItemTemplate = componentTemplate;
        cv.ShowDialog();
        if (cv.DialogResult.HasValue && cv.DialogResult.Value)
        {
          List<RestRequest> requests = new List<RestRequest>();
          components = new List<Data.Component>();

          foreach (var c in cv.valuesList.SelectedItems)
          {
            Data.Component cc = c as Data.Component;
            components.Add(cc);

          }

          for (int i = 0; i < jiraPan.issueList.SelectedItems.Count; i++)
          {
            int index = jiraPan.issueList.Items.IndexOf(jiraPan.issueList.SelectedItems[i]);
            var request = new RestRequest("issue/" + jira.IssuesCollection[index].key, Method.PUT);
            request.AddHeader("Content-Type", "application/json");
            request.RequestFormat = RestSharp.DataFormat.Json;
            var newissue =
                new
                {
                  fields = new
                  {
                    components = components
                  }
                };
            request.AddBody(newissue);
            requests.Add(request);
          }
          BackgroundJira bj = new BackgroundJira();
          bj.WorkerComplete += new EventHandler<ResponseArg>(ChangeValue_Completed);
          bj.Start<Issue>(requests);
        }
      }
      catch (System.Exception ex1)
      {
        MessageBox.Show("exception: " + ex1);
      }

    }
    public void ChangePriority_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        if (jiraPan.issueList.SelectedItems.Count > 1)
          if (
              MessageBox.Show(
                  "Action will apply to all the " + jiraPan.issueList.SelectedItems.Count +
                  " selected issues,\n are you sure to continue?", "Multiple Items", MessageBoxButton.YesNo,
                  MessageBoxImage.Warning) == MessageBoxResult.No)
            return;

        ChangeValue cv = new ChangeValue();
        cv.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        cv.valuesList.ItemsSource = jira.PrioritiesCollection;
        ;
        int indexitem = jiraPan.issueList.SelectedIndex;
        cv.valuesList.SelectedIndex = (jira.IssuesCollection[indexitem].fields.priority != null)
            ? Data.IndexByName.Get(jira.IssuesCollection[indexitem].fields.priority.name, "name",
                jira.PrioritiesCollection.ToList<Priority>())
            : -1;
        cv.Title = "Change Priority";
        DataTemplate priorityTemplate = cv.FindResource("priorityTemplate") as DataTemplate;
        cv.valuesList.ItemTemplate = priorityTemplate;
        cv.ShowDialog();
        if (cv.DialogResult.HasValue && cv.DialogResult.Value)
        {
          List<string> keys = new List<string>();
          //get selected issues
          for (int i = 0; i < jiraPan.issueList.SelectedItems.Count; i++)
          {
            int index = jiraPan.issueList.Items.IndexOf(jiraPan.issueList.SelectedItems[i]);
            keys.Add(jira.IssuesCollection[index].key);
          }
          JiraUtils.SetPriorities(keys, jira.PrioritiesCollection[cv.valuesList.SelectedIndex].id, ChangeValue_Completed);
        }
      }
      catch (System.Exception ex1)
      {
        MessageBox.Show("exception: " + ex1);
      }

    }
    public void ChangeType_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        if (jiraPan.issueList.SelectedItems.Count > 1)
          if (MessageBox.Show("Action will apply to all the " + jiraPan.issueList.SelectedItems.Count + " selected issues,\n are you sure to continue?", "Multiple Items", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
            return;

        ChangeValue cv = new ChangeValue();
        cv.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        cv.valuesList.ItemsSource = jira.TypesCollection;
        int indexitem = jiraPan.issueList.SelectedIndex;
        cv.valuesList.SelectedIndex = Data.IndexByName.Get(jira.IssuesCollection[indexitem].fields.issuetype.name, "name", jira.TypesCollection.ToList<Issuetype>());
        cv.Title = "Change Issue Type";
        DataTemplate priorityTemplate = cv.FindResource("priorityTemplate") as DataTemplate;
        cv.valuesList.ItemTemplate = priorityTemplate;
        cv.ShowDialog();
        if (!cv.DialogResult.HasValue || !cv.DialogResult.Value)
          return;

        List<RestRequest> requests = new List<RestRequest>();
        for (int i = 0; i < jiraPan.issueList.SelectedItems.Count; i++)
        {
          int index = jiraPan.issueList.Items.IndexOf(jiraPan.issueList.SelectedItems[i]);



          string trid = jira.TypesCollection[cv.valuesList.SelectedIndex].id;
          var request = new RestRequest("issue/" + jira.IssuesCollection[index].key, Method.PUT);
          request.AddHeader("Content-Type", "application/json");
          request.RequestFormat = RestSharp.DataFormat.Json;

          var newissue =
                  new
                  {
                    fields = new
                    {
                      issuetype = new
                      {
                        id = trid
                      }

                    }
                  };
          request.AddBody(newissue);
          requests.Add(request);
        }
        BackgroundJira bj = new BackgroundJira();
        bj.WorkerComplete += new EventHandler<ResponseArg>(ChangeValue_Completed);
        bj.Start<Issuetype>(requests);

      }
      catch (System.Exception ex1)
      {
        MessageBox.Show("exception: " + ex1);
      }

    }
    private void ChangeAssign_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        if (jiraPan.issueList.SelectedItems.Count > 1)
          if (MessageBox.Show("Action will apply to all the " + jiraPan.issueList.SelectedItems.Count + " selected issues,\n are you sure to continue?", "Multiple Items", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
            return;

        // GET STATUSES
        List<User> assignees = getAssigneesIssue();
        if (!assignees.Any())
        {
          MessageBox.Show("You don't have permission to Assign people to this Issue");
          return;
        }
        ChangeAssignee cv = new ChangeAssignee(); cv.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        cv.SetList(assignees);

        int indexitem = jiraPan.issueList.SelectedIndex;
        cv.valuesList.SelectedIndex = (jira.IssuesCollection[indexitem].fields.assignee != null) ? Data.IndexByName.Get(jira.IssuesCollection[indexitem].fields.assignee.emailAddress, "emailAddress", assignees) : -1;

        cv.Title = "Assign to";

        cv.ShowDialog();
        if (cv.DialogResult.HasValue && cv.DialogResult.Value)
        {
          List<RestRequest> requests = new List<RestRequest>();

          for (int i = 0; i < jiraPan.issueList.SelectedItems.Count; i++)
          {
            int index = jiraPan.issueList.Items.IndexOf(jiraPan.issueList.SelectedItems[i]);


            string user = (cv.valuesList.SelectedIndex == -1) ? null : ((User)cv.valuesList.SelectedItem).name;
            var request2 = new RestRequest("issue/" + jira.IssuesCollection[index].key + "/assignee", Method.PUT);
            request2.AddHeader("Content-Type", "application/json");
            request2.RequestFormat = RestSharp.DataFormat.Json;
            var newissue =
                    new
                    {
                      name = user
                    };
            request2.AddBody(newissue);
            requests.Add(request2);

          }

          BackgroundJira bj = new BackgroundJira();
          bj.WorkerComplete += new EventHandler<ResponseArg>(ChangeValue_Completed);
          bj.Start<User>(requests);
        }
      }
      catch (System.Exception ex1)
      {
        MessageBox.Show("exception: " + ex1);
      }

    }
    private void ChangeValue_Completed(object sender, ResponseArg e)
    {
      IRestResponse response = e.Responses.Last();

      getIssues(jira.startAt);


    }
    public List<User> getAssigneesIssue()
    {
      List<User> userlist = new List<User>();

      if (jiraPan.issueList.Items.Count != 0)
      {
        var maxresults = 1000;
        for (var i = 0; i < 100; i++)
        {
          var apicall = "user/assignable/search?issueKey=" +
                    jira.IssuesCollection[jiraPan.issueList.SelectedIndex].key + "&maxResults=" + maxresults + "&startAt=" + (i * maxresults);
          var request = new RestRequest(apicall, Method.GET);
          request.AddHeader("Content-Type", "application/json");
          request.RequestFormat = RestSharp.DataFormat.Json;
          request.OnBeforeDeserialization = resp => { resp.ContentType = "application/json"; };
          var response = JiraClient.Client.Execute<List<User>>(request);
          if (!RestCallback.Check(response) || !response.Data.Any())
            break;

          userlist.AddRange(response.Data);
          if (response.Data.Count < maxresults)
            break;
        }
      }
      else
      {
        MessageBox.Show("You are adding the first issue in the project!", "Case Issue Tracker");
      }

      return userlist;
    }
    public List<User> getAssigneesProj()
    {
      List<User> userlist = new List<User>();
      var maxresults = 1000;
      for (var i = 0; i < 100; i++)
      {
        var apicall = "user/assignable/search?project=" +
                  jira.ProjectsCollection[jiraPan.projIndex].key + "&maxResults=" + maxresults + "&startAt=" + (i * maxresults);
        var request = new RestRequest(apicall, Method.GET);
        request.AddHeader("Content-Type", "application/json");
        request.RequestFormat = RestSharp.DataFormat.Json;
        request.OnBeforeDeserialization = resp => { resp.ContentType = "application/json"; };
        var response = JiraClient.Client.Execute<List<User>>(request);
        if (!RestCallback.Check(response) || !response.Data.Any())
          break;

        userlist.AddRange(response.Data);
        if (response.Data.Count < maxresults)
          break;
      }
      return userlist;
    }
    #endregion
    #endregion
    #region BCF
    private void AddBCFComment(object sender, RoutedEventArgs e)
    {
      try
      {
        if (bcfPan.issueList.SelectedItems.Count > 1)
          if (MessageBox.Show("Action will apply to all the " + bcfPan.issueList.SelectedItems.Count + " selected issues,\n are you sure to continue?", "Multiple Items", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
            return;

        AddComment ac = new AddComment();
        ac.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
        ac.ShowDialog();
        if (ac.DialogResult.HasValue && ac.DialogResult.Value)
        {
          CommentBCF c = new CommentBCF();
          c.Guid = Guid.NewGuid().ToString();
          c.Comment1 = ac.comment.Text;
          c.Topic = new CommentTopic();
          c.Topic.Guid = (string.IsNullOrWhiteSpace(jira.Bcf.Issues[bcfPan.listIndex].markup.Topic.Guid)) ? Guid.Empty.ToString() : jira.Bcf.Issues[bcfPan.listIndex].markup.Topic.Guid;
          c.Date = DateTime.Now;
          c.VerbalStatus = ac.VerbalStatus.Text;
          c.Status = CommentStatus.Unknown;
          c.Author = (string.IsNullOrWhiteSpace(jira.Self.displayName)) ? UserSettings.Get("BCFusername") : jira.Self.displayName;
          for (int i = 0; i < bcfPan.issueList.SelectedItems.Count; i++)
          {
            int index = bcfPan.issueList.Items.IndexOf(bcfPan.issueList.SelectedItems[i]);
            jira.Bcf.Issues[index].markup.Comment.Add(c);
            jira.Bcf.Issues[index].markup.Comment.Move(jira.Bcf.Issues[index].markup.Comment.Count - 1, 0);
          }
          jira.Bcf.HasBeenSaved = false;
        }
      }
      catch (System.Exception ex1)
      {
        MessageBox.Show("exception: " + ex1);
      }
    }
    private void DelBCFComm(object sender, RoutedEventArgs e)
    {
      if (bcfPan.issueList.SelectedIndex == -1)
      {
        MessageBox.Show("No Issue selected", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        return;
      }
      if (bcfPan.commentList.SelectedIndex == -1)
      {
        MessageBox.Show("No Comment selected", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        return;
      }
      MessageBoxResult answer = MessageBox.Show("Are you sure you want to\nDelete the selected Comment(s)?", "Delete Issue?",
                    MessageBoxButton.YesNo, MessageBoxImage.Question);
      if (answer == MessageBoxResult.No)
        return;

      List<int> indices = new List<int>();
      for (int i = 0; i < bcfPan.commentList.SelectedItems.Count; i++)
      {
        int index = bcfPan.commentList.Items.IndexOf(bcfPan.commentList.SelectedItems[i]);
        indices.Add(index);
      }
      indices.Sort();
      indices.Reverse();
      foreach (var i in indices)
      {
        jira.Bcf.Issues[bcfPan.listIndex].markup.Comment.RemoveAt(i);
      }

      jira.Bcf.HasBeenSaved = false;
    }
    private void DelBCFIssue(object sender, RoutedEventArgs e)
    {
      if (bcfPan.listIndex == -1)
      {
        MessageBox.Show("No Issue selected", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        return;
      }
      MessageBoxResult answer = MessageBox.Show("Are you sure you want to\nDelete the selected Issue(s)?", "Delete Issue?",
                    MessageBoxButton.YesNo, MessageBoxImage.Question);
      if (answer == MessageBoxResult.No)
        return;

      List<int> indices = new List<int>();
      for (int i = 0; i < bcfPan.issueList.SelectedItems.Count; i++)
      {
        int index = bcfPan.issueList.Items.IndexOf(bcfPan.issueList.SelectedItems[i]);
        indices.Add(index);
      }
      indices.Sort();
      indices.Reverse();
      foreach (var i in indices)
      {
        DeleteDirectory(IOPath.Combine(jira.Bcf.path, jira.Bcf.Issues[i].guid.ToString()));
        jira.Bcf.Issues.RemoveAt(i);
      }

      jira.Bcf.HasBeenSaved = false;
      if (!jira.Bcf.Issues.Any())
      {
        jira.Bcf.HasBeenSaved = true;
        NewBCF(null, null);
      }


    }
    private void SaveBCF(object sender, RoutedEventArgs e)
    {
      try
      {
        if (jira.Bcf.Issues.Count == 0)
        {
          MessageBox.Show("The current BCF Report is empty.", "No Issue", MessageBoxButton.OK, MessageBoxImage.Error);
          return;
        }

        // Show save file dialog box
        string filename = SaveDialog(jira.Bcf.Filename);

        // Process save file dialog box results
        if (!string.IsNullOrWhiteSpace(filename))
        {
          this.IsEnabled = false;

          XmlSerializer serializerV = new XmlSerializer(typeof(VisualizationInfo));
          XmlSerializer serializerM = new XmlSerializer(typeof(Markup));
          foreach (var issue in jira.Bcf.Issues)
          {
            issue.markup.Comment = new ObservableCollection<CommentBCF>(issue.markup.Comment.Reverse());
            // Serialize the object, and close the TextWriter
            Stream writerV = new FileStream(IOPath.Combine(jira.Bcf.path, issue.guid.ToString(), "viewpoint.bcfv"), FileMode.Create);
            serializerV.Serialize(writerV, issue.viewpoint);
            writerV.Close();

            Stream writerM = new FileStream(IOPath.Combine(jira.Bcf.path, issue.guid.ToString(), "markup.bcf"), FileMode.Create);
            serializerM.Serialize(writerM, issue.markup);
            writerM.Close();
          }

          if (File.Exists(filename))
            File.Delete(filename);

          using (Ionic.Zip.ZipFile zip = new Ionic.Zip.ZipFile())
          {
            zip.AddDirectory(jira.Bcf.path);
            zip.Save(filename);
          }

          Uri uri2 = new Uri(filename);
          string reportname = IOPath.GetFileName(uri2.LocalPath);

          if (File.Exists(uri2.LocalPath))
          {
            string argument = @"/select, " + uri2.LocalPath;
            System.Diagnostics.Process.Start("explorer.exe", argument);
          }

          jira.Bcf.HasBeenSaved = true;
          jira.Bcf.Filename = reportname;

          this.IsEnabled = true;
        }
      }
      catch (System.Exception ex1)
      {
        MessageBox.Show("exception: " + ex1);
      }
    }
    private void SaveBCF2(object sender, RoutedEventArgs e)
    {
      try
      {
        if (jira.Bcf.Issues.Count == 0)
        {
          MessageBox.Show("The current BCF Report is empty.", "No Issue", MessageBoxButton.OK, MessageBoxImage.Error);
          return;
        }

        if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
        {
          MessageBoxResult result = MessageBox.Show("Do you want to save as BCF 1.0 format? (not recommended)", "Save as BCF 1.0", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);
          if (result == MessageBoxResult.Yes)
          {
            SaveBCF(sender, e);
            return;
          }
        }

        // Save to BCF 2.0
        Case.IssueTracker.Data.BcfAdapter.SaveBcf2FromBcf1(jira.Bcf);
      }
      catch (System.Exception ex1)
      {
        MessageBox.Show("exception: " + ex1);
      }
    }
    private bool CheckSaveBCF()
    {
      try
      {
        if (jira.Bcf != null && !jira.Bcf.HasBeenSaved && jira.Bcf.Issues.Any())
        {
          MessageBoxResult answer = MessageBox.Show("The current Report has been modified.\nDo you want to save changes?", "Save Report?",
          MessageBoxButton.YesNoCancel, MessageBoxImage.Question, MessageBoxResult.Cancel);
          if (answer == MessageBoxResult.Yes)
          {
            SaveBCF(null, null);
            return false;
          }
          else if (answer == MessageBoxResult.Cancel)
          {
            return false;
          }
        }
      }
      catch (System.Exception ex1)
      {
        MessageBox.Show("exception: " + ex1);
      }
      return true;
    }
    private void OpenBCFFile(object sender, RoutedEventArgs e)
    {
      try
      {
        if (!CheckSaveBCF())
          return;

        Microsoft.Win32.OpenFileDialog openFileDialog1 = new Microsoft.Win32.OpenFileDialog();
        openFileDialog1.Filter = "BIM Collaboration Format (*.bcfzip)|*.bcfzip";
        openFileDialog1.DefaultExt = ".bcfzip";
        openFileDialog1.RestoreDirectory = true;
        Nullable<bool> result = openFileDialog1.ShowDialog(); // Show the dialog.

        if (result == true) // Test result.
        {
          OpenBCF(openFileDialog1.FileName);

        }
      }
      catch (System.Exception ex1)
      {
        MessageBox.Show("exception: " + ex1);
      }

    }
    private void UploadBCFIssues(object sender, RoutedEventArgs e)
    {
      try
      {
        //check if connected
        if (string.IsNullOrWhiteSpace(jira.Self.key))
        {
          MessageBox.Show("You need to be conncected to Jira in order to upload Issues,\n please check your settings.", "Not connected", MessageBoxButton.OK, MessageBoxImage.Error);
          return;
        }
        else if (jira.ProjectsCollection.Count == 0)
        {
          MessageBox.Show("No project found,\n please your permissions on Jira.", "No project found", MessageBoxButton.OK, MessageBoxImage.Error);
          return;
        }
        else if (bcfPan.issueList.SelectedItems.Count == 0)
        {
          MessageBox.Show("Please select one or more Isses first.", "No Issue selected", MessageBoxButton.OK, MessageBoxImage.Error);
          return;
        }

        UploadBCF ub = new UploadBCF();
        ub.projCombo.ItemsSource = jira.ProjectsCollection;
        ub.itemCount = bcfPan.issueList.SelectedItems.Count;
        ub.projIndex = jiraPan.projIndex;
        ub.setValues();
        ub.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
        ub.ShowDialog();
        if (ub.DialogResult.HasValue && ub.DialogResult.Value)
        {
          List<IssueBCF> issues = new List<IssueBCF>();
          List<Issue> issuesJira = new List<Issue>();

          for (int i = 0; i < bcfPan.issueList.SelectedItems.Count; i++)
          {
            int index = bcfPan.issueList.Items.IndexOf(bcfPan.issueList.SelectedItems[i]);
            var issueJira = new Issue();
            issueJira.fields = new Fields();
            issueJira.fields.issuetype = (Issuetype)ub.issueTypeCombo.SelectedItem;
            issues.Add(jira.Bcf.Issues[index]);
            issuesJira.Add(issueJira);
          }

          doUploadIssue(issues, jira.Bcf.path, false, ub.projCombo.SelectedIndex, issuesJira);

        }
      }
      catch (System.Exception ex1)
      {
        MessageBox.Show("exception: " + ex1);
      }

    }
    private void UploadBCFIssueComplete(object sender, IntArg e)
    {
      //select the projects issues were uploaded to and refresh
      if (jiraPan.projIndex != e.Myint)
        jiraPan.projIndex = e.Myint;
      else
        getIssues(0);

      this.IsEnabled = true;
      tabControl.SelectedIndex = 0;

    }
    private void ComponentsShowBCF(object sender, IntArg e)
    {
      try
      {
        int index = e.Myint;
        var v = jira.Bcf.Issues[index].viewpoint;
        if (null == v)
        {
          MessageBox.Show("No viewpoint found.", "No Viewpoint", MessageBoxButton.OK, MessageBoxImage.Error);
          return;
        }
        ComponentsShow(v.Components);
      }
      catch (System.Exception ex1)
      {
        MessageBox.Show("exception: " + ex1);
      }
    }
    public void OpenBCF(string bcfzipfile)
    {
      try
      {
        this.IsEnabled = false;

        NewBCF(null, null);
        var filename = new DirectoryInfo(bcfzipfile);
        jira.Bcf.Filename = filename.Name;


        using (ZipFile zip = ZipFile.Read(bcfzipfile))
        {
          zip.ExtractAll(jira.Bcf.path);
        }

        var dir = new DirectoryInfo(jira.Bcf.path);

        // Check BCF version
        bool isBCF2 = false;
        foreach (var file in dir.GetFiles())
        {
          if (File.Exists(IOPath.Combine(dir.FullName, "bcf.version")))
          {
            // This is a BCF 2.0 file
            isBCF2 = true;
          }
        }

        int errorCount = 0;
        //ADD ISSUES FOR EACH SUBFOLDER
        foreach (var folder in dir.GetDirectories())
        {
          //BCF ISSUE is not complete
          if (!File.Exists(IOPath.Combine(folder.FullName, "snapshot.png")) || !File.Exists(IOPath.Combine(folder.FullName, "markup.bcf")) || !File.Exists(IOPath.Combine(folder.FullName, "viewpoint.bcfv")))
          {
            errorCount++;
            continue;
          }

          IssueBCF i = null;

          FileStream viewpointFile = new FileStream(IOPath.Combine(folder.FullName, "viewpoint.bcfv"), FileMode.Open);
          FileStream markupFile = new FileStream(IOPath.Combine(folder.FullName, "markup.bcf"), FileMode.Open);

          if (isBCF2)
          {
            XmlSerializer serializerS = new XmlSerializer(typeof(Case.IssueTracker.Data.BCF2.VisualizationInfo));
            Case.IssueTracker.Data.BCF2.VisualizationInfo viewpoint = serializerS.Deserialize(viewpointFile) as Case.IssueTracker.Data.BCF2.VisualizationInfo;

            XmlSerializer serializerM = new XmlSerializer(typeof(Case.IssueTracker.Data.BCF2.Markup));
            Case.IssueTracker.Data.BCF2.Markup markup = serializerM.Deserialize(markupFile) as Case.IssueTracker.Data.BCF2.Markup;

            if (markup != null && viewpoint != null)
            {
              i = BcfAdapter.LoadBcf1IssueFromBcf2(markup, viewpoint);
              if (Guid.Empty == i.guid)
              {
                i.guid = new Guid(folder.Name);
              }
            }
          }
          else
          {
            i = new IssueBCF();
            i.guid = new Guid(folder.Name);

            XmlSerializer serializerS = new XmlSerializer(typeof(VisualizationInfo));
            i.viewpoint = serializerS.Deserialize(viewpointFile) as VisualizationInfo;

            XmlSerializer serializerM = new XmlSerializer(typeof(Markup));
            i.markup = serializerM.Deserialize(markupFile) as Markup;
          }

          viewpointFile.Close();
          markupFile.Close();

          i.markup.Comment = new ObservableCollection<CommentBCF>(i.markup.Comment.OrderByDescending(o => o.Date));
          i.snapshot = IOPath.Combine(folder.FullName, "snapshot.png");

          jira.Bcf.Issues.Add(i);
        }

        if (jira.Bcf.Issues.Any())
          bcfPan.listIndex = 0;

        this.IsEnabled = true;
        tabControl.SelectedIndex = 1;

        if (errorCount > 0)
        {
          MessageBox.Show(errorCount + " Issue/s were not imported because of missing viewpoint and snapshot.",
              "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
        }


      }
      catch (System.Exception ex1)
      {
        MessageBox.Show("exception: " + ex1);
      }



    }
    public void NewBCF(object sender, RoutedEventArgs e)
    {
      try
      {
        if (!CheckSaveBCF())
          return;

        if (jira.Bcf != null && Directory.Exists(jira.Bcf.path))
          DeleteDirectory(jira.Bcf.path);

        jira.Bcf = new BCF();
      }
      catch (System.Exception ex1)
      {
        MessageBox.Show("exception: " + ex1);
      }
    }
    public CommentStatus getStatus(string status)
    {
      CommentStatus cs;
      switch (status)
      {
        case "Error":
          cs = CommentStatus.Error;
          break;
        case "Warning":
          cs = CommentStatus.Warning;
          break;
        case "Info":
          cs = CommentStatus.Info;
          break;
        case "Unknown":
          cs = CommentStatus.Unknown;
          break;
        default:
          cs = CommentStatus.Unknown;
          break;
      }
      return cs;
    }
    #endregion
    #region private common
    private VisualizationInfo deserializeView(string content)
    {
      try
      {
        if (string.IsNullOrWhiteSpace(content))
          return null;

        VisualizationInfo viewpoint = new VisualizationInfo();

        using (TextReader sr = new StringReader(content.Replace(((char)0xFEFF).ToString(), "")))
        {
          XmlSerializer serializerS = new XmlSerializer(typeof(VisualizationInfo));
          viewpoint = serializerS.Deserialize(sr) as VisualizationInfo;
        }
        return viewpoint;
      }
      catch (Exception ex1)
      {
        MessageBox.Show("exception: " + ex1);
      }
      return null;
    }
    private int SettingsTabIndexGet()
    {
      string indexS = UserSettings.Get("currenttab");
      int index = (!string.IsNullOrWhiteSpace(indexS)) ? Convert.ToInt16(indexS) : 0;
      return index;
    }
    private string SaveDialog(string filename)
    {

      Microsoft.Win32.SaveFileDialog saveFileDialog = new Microsoft.Win32.SaveFileDialog();
      saveFileDialog.Title = "Save as BCF report file...";
      saveFileDialog.FileName = filename; // Default file name
      saveFileDialog.DefaultExt = ".bcfzip"; // Default file extension
      saveFileDialog.Filter = "BIM Collaboration Format (*.bcfzip)|*.bcfzip"; // Filter files by extension

      //if it goes fine I return the filename, otherwise null
      Nullable<bool> result = saveFileDialog.ShowDialog();
      if (result == true && saveFileDialog.FileName != "")
        return saveFileDialog.FileName;

      return null;
    }
    private void SettingsClick(object sender, RoutedEventArgs e)
    {
      Settings s = new Settings();
      try
      {
        s.username.Text = UserSettings.Get("username");
        s.psw.Password = DataProtector.DecryptData(UserSettings.Get("password"));
        s.jiraserver.Text = UserSettings.Get("jiraserver");
        s.BCFusername.Text = UserSettings.Get("BCFusername");
        string oldselattachedelems = UserSettings.Get("selattachedelems");
        if (oldselattachedelems == "0")
          s.IsolateAttachElems.IsChecked = true;




        s.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        s.ShowDialog();
        if (s.DialogResult.HasValue && s.DialogResult.Value)
        {
          UserSettings.Set("username", s.username.Text);
          UserSettings.Set("password", DataProtector.EncryptData(s.psw.Password));
          UserSettings.Set("jiraserver", s.jiraserver.Text);
          UserSettings.Set("BCFusername", s.BCFusername.Text);
          UserSettings.Set("selattachedelems", (s.IsolateAttachElems.IsChecked.Value) ? "0" : "1");
        }
      }
      catch (Exception ex1)
      {
        MessageBox.Show("exception: " + ex1);
      }

    }
    private void OpenImage(object sender, RoutedEventArgs e)
    {
      try
      {
        string content = (string)((Button)sender).Tag;

        if (content != "")
        {
          string ext = IOPath.GetExtension(content);
          if (ext == ".jpg" || ext == ".jpeg" || ext == ".png" || ext == ".gif" || ext == ".bmp")
          {
            SnapWin win = new SnapWin(content);
            win.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            win.ShowDialog();
          }
        }
      }
      catch (System.Exception ex1)
      {
        MessageBox.Show("exception: " + ex1);
      }
    }
    private void ComponentsShow(Case.IssueTracker.Data.Component[] components)
    {
      try
      {
        if (null != components && components.Any())
        {
          ComponentsList cl = new ComponentsList(components);
          cl.Show();
        }
        else
          MessageBox.Show("No component attached to the selected Issue", "No Component", MessageBoxButton.OK, MessageBoxImage.Hand);
      }
      catch (System.Exception ex1)
      {
        MessageBox.Show("exception: " + ex1);
      }
    }

    private void Window_DragEnter(object sender, DragEventArgs e)
    {
      whitespace.Visibility = System.Windows.Visibility.Visible;

    }
    private void Window_DragLeave(object sender, DragEventArgs e)
    {
      whitespace.Visibility = System.Windows.Visibility.Hidden;
    }
    private void Window_Drop(object sender, DragEventArgs e)
    {
      whitespace.Visibility = System.Windows.Visibility.Hidden;
      if (e.Data.GetDataPresent(DataFormats.FileDrop))
      {
        // Note that you can have more than one file.
        string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
        foreach (var f in files)
        {
          OpenBCF(f);
        }
      }
    }
    private void Window_DragOver(object sender, DragEventArgs e)
    {
      bool dropEnabled = true;

      if (e.Data.GetDataPresent(DataFormats.FileDrop, true))
      {
        string[] filenames = e.Data.GetData(DataFormats.FileDrop, true) as string[];

        foreach (string filename in filenames)
        {
          if (IOPath.GetExtension(filename).ToUpperInvariant() != ".BCFZIP")
          {
            dropEnabled = false;
            break;
          }
        }
        tabControl.SelectedIndex = 1;
      }
      else
      {
        dropEnabled = false;
      }


      if (!dropEnabled)
      {
        e.Effects = DragDropEffects.None;
        e.Handled = true;
      }
    }

    #endregion
    #region public various methods
    public VisualizationInfo getVisInfo()
    {
      try
      {
        var request = new RestRequest(jira.IssuesCollection[jiraPan.listIndex].viewpoint, Method.GET);

        var client = new RestClient();
        client.CookieContainer = JiraClient.Client.CookieContainer;

        var response = client.Execute(request);
        if (RestCallback.Check(response))
        {
          if (string.IsNullOrWhiteSpace(response.Content))
            return null;

          VisualizationInfo viewpoint = deserializeView(response.Content);
          if (null != viewpoint)
            return viewpoint;
        }
      }
      catch (Exception ex1)
      {
        MessageBox.Show("exception: " + ex1);
      }
      return null;
    }
    public void doUploadIssue(List<IssueBCF> issues, string path, bool delAfter, int projIndex, List<Issue> issuesJira)
    {
      try
      {
        this.IsEnabled = false;

        JiraUploader ju = new JiraUploader(
            jira.ProjectsCollection[projIndex].key,
            issuesJira, //issuetype relative to the selected project
            issues, path, projIndex, delAfter);
        ju.uploadComplete += new EventHandler<IntArg>(UploadBCFIssueComplete);
      }
      catch (Exception ex1)
      {
        MessageBox.Show("exception: " + ex1);
      }

    }
    public string projID
    {
      get { return jira.ProjectsCollection[jiraPan.projIndex].id; }
    }
    public void setButtonVisib(bool addIssue, bool open3Dview, bool newBCFreport)
    {
      jiraPan.AddIssueBtn.Visibility = (addIssue) ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
      bcfPan.AddIssueBtn.Visibility = (addIssue) ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;

      jiraPan.Open3dViewBtn.Visibility = (open3Dview) ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
      bcfPan.Open3dViewBtn.Visibility = (open3Dview) ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;

      bcfPan.NewBCFBtn.Visibility = (newBCFreport) ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
    }
    public bool onClosing(CancelEventArgs e)
    {
      try
      {
        int index = tabControl.SelectedIndex;
        if (!CheckSaveBCF())
          return true;

        //save current tab
        if (index != -1)
          UserSettings.Set("currenttab", index.ToString());
        //save current project
        if (jiraPan.projIndex != -1)
          UserSettings.Set("currentproj", jiraPan.projIndex.ToString());

        if (jira.Bcf != null && Directory.Exists(jira.Bcf.path))
          DeleteDirectory(jira.Bcf.path);
      }
      catch (System.Exception ex1)
      {
        MessageBox.Show("exception: " + ex1);
      }

      return false;
    }
    public void DeleteDirectory(string target_dir)
    {
      try
      {
        if (Directory.Exists(target_dir))
        {
          string[] files = Directory.GetFiles(target_dir);
          string[] dirs = Directory.GetDirectories(target_dir);
          foreach (string file in files)
          {
            File.SetAttributes(file, FileAttributes.Normal);
            File.Delete(file);
          }

          foreach (string dir in dirs)
          {
            DeleteDirectory(dir);
          }
          Directory.Delete(target_dir, false);
        }
      }
      catch (System.Exception ex1)
      {
        MessageBox.Show("exception: " + ex1);
      }
    }
    private int IndexByName<T>(string propertyValue, string propertyName, List<T> mylist) where T : new()
    {
      if (mylist.Count == 0)
        return -1;
      Type t = mylist[0].GetType();
      PropertyInfo prop = t.GetProperty(propertyName);
      for (var i = 0; i < mylist.Count; i++)
      {
        string theValue = prop.GetValue(mylist[i], null).ToString();
        if (theValue == propertyValue)
          return i;
      }
      return -1;
    }
    #endregion

    private void HelpBtn_OnClick(object sender, RoutedEventArgs e)
    {
      System.Diagnostics.Process.Start(@"https://github.com/WeConnect/issue-tracker");
    }


  }
}
