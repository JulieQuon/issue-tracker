using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System.Xml.Serialization;
using System.IO;
using System.Text.RegularExpressions;
using Case.IssueTracker.Windows;
using System.Windows;
using RestSharp;

namespace Case.IssueTracker.Data
{
  class JiraUploader
  {
    private string projectKey = "";
    private List<Issue> issuesJira = new List<Issue>();
    private List<IssueBCF> issues = new List<IssueBCF>();
    private string path = "";
    private bool delAfter;
    ProgressWin progressWin = new ProgressWin();
    int projIndex = 0;

    //EVENTS
    public event EventHandler<IntArg> uploadComplete;

    private int uploadErrors = 0;

    BackgroundWorker worker = new BackgroundWorker();


    public JiraUploader(string pk, List<Issue> _issuesJira, List<IssueBCF> i, string p, int pi, bool _delAfter)
    {
      //initialize
      projectKey = pk;
      issuesJira = _issuesJira;
      issues = i;
      path = p;
      projIndex = pi;
      delAfter = _delAfter;

      progressWin.WindowStartupLocation = WindowStartupLocation.CenterScreen;
      progressWin.Show();
      progressWin.SetProgress(0, getProgressString(0));
      progressWin.killWorker += new EventHandler(worker_Kill);
      //bg worker


      worker.WorkerReportsProgress = true;
      worker.WorkerSupportsCancellation = true;
      worker.DoWork += new DoWorkEventHandler(worker_DoWork);
      worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(worker_RunWorkerCompleted);
      worker.ProgressChanged += new ProgressChangedEventHandler(worker_ProgressChanged);
      worker.RunWorkerAsync();
      worker.Dispose();


    }
    void worker_DoWork(object sender, DoWorkEventArgs e)
    {
      BackgroundWorker worker = (BackgroundWorker)sender;

      XmlSerializer serializerV = new XmlSerializer(typeof(VisualizationInfo));

      for (int i = 0; i < issues.Count(); i++)
      {
        try
        {
          IssueBCF issue = issues[i];
          worker.ReportProgress((100 * i + 1) / issues.Count(), getProgressString(i + 1));// HAS TO BE OUT OF THE DISPATCHER!
                                                                                          // check status on each step
          if (worker.CancellationPending == true)
          {
            e.Cancel = true;
            return; // abort work, if it's cancelled
          }

          //CHECK IF ALREADY EXISTING
          // could use the expression: cf[11600] ~ "aaaa"
          // = operator not supported
          string fields = " AND  GUID~" + issue.guid.ToString() + "&fields=key,comment";
          string query = "search?jql=project=" + projectKey + fields;

          var request4 = new RestRequest(query, Method.GET);
          request4.AddHeader("Content-Type", "application/json");
          request4.RequestFormat = RestSharp.DataFormat.Json;
          var response4 = JiraClient.Client.Execute<Issues>(request4);

          if (!RestCallback.Check(response4))
            break;

          //DOESN'T exist already
          if (!response4.Data.issues.Any())
          {

            string snapshot = Path.Combine(path, issue.guid.ToString(), "snapshot.png");
            string viewpoint = Path.Combine(path, issue.guid.ToString(), "viewpoint.bcfv");
            string key = "";


            //update view - it might be a new issue
            // Serialize the object, and close the TextWriter
            Stream writerV = new FileStream(viewpoint, FileMode.Create);
            serializerV.Serialize(writerV, issue.viewpoint);
            writerV.Close();


            var request = new RestRequest("issue", Method.POST);
            request.AddHeader("Content-Type", "application/json");
            request.RequestFormat = RestSharp.DataFormat.Json;


            var newissue =
                new
                {

                  fields = new Dictionary<string, object>()

                };
            newissue.fields.Add("project", new { key = projectKey });
            newissue.fields.Add("summary", (string.IsNullOrWhiteSpace(issue.markup.Topic.Title)) ? "no title" : issue.markup.Topic.Title);
            newissue.fields.Add("issuetype", new { id = issuesJira[i].fields.issuetype.id });
            newissue.fields.Add(UserSettings.Get("guidfield"), issue.guid.ToString());

            if (issuesJira[i].fields.assignee != null)
              newissue.fields.Add("assignee", new { name = issuesJira[i].fields.assignee.name });

            if (issuesJira[i].fields.priority != null)
              newissue.fields.Add("priority", new { id = issuesJira[i].fields.priority.id });

            if (issuesJira[i].fields.components != null && issuesJira[i].fields.components.Any())
              newissue.fields.Add("components", issuesJira[i].fields.components);


            request.AddBody(newissue);
            var response = JiraClient.Client.Execute(request);

            var responseIssue = new Issue();
            if (RestCallback.Check(response))
            {
              responseIssue = RestSharp.SimpleJson.DeserializeObject<Issue>(response.Content);
              key = responseIssue.key;//attach and comment sent to the new issue
            }
            else
            {
              uploadErrors++;
              break;
            }

            //upload viewpoint and snapshot
            var request2 = new RestRequest("issue/" + key + "/attachments", Method.POST);
            request2.AddHeader("X-Atlassian-Token", "nocheck");
            request2.RequestFormat = RestSharp.DataFormat.Json;
            request2.AddFile("file", File.ReadAllBytes(snapshot), "snapshot.png");
            request2.AddFile("file", File.ReadAllBytes(viewpoint), "viewpoint.bcfv");
            var response2 = JiraClient.Client.Execute(request2);
            RestCallback.Check(response2);

            //ADD COMMENTS
            if (issue.markup.Comment.Any())
            {
              issue.markup.Comment = new System.Collections.ObjectModel.ObservableCollection<CommentBCF>(issue.markup.Comment.Reverse());
              foreach (var c in issue.markup.Comment)
              {
                var request3 = new RestRequest("issue/" + key + "/comment", Method.POST);
                request3.AddHeader("Content-Type", "application/json");
                request3.RequestFormat = RestSharp.DataFormat.Json;
                var newcomment = new { body = c.Comment1 };
                request3.AddBody(newcomment);
                var response3 = JiraClient.Client.Execute<Comment2>(request3);
                if (!RestCallback.Check(response3))
                  break;
              }
            }
            if (i == issues.Count() - 1)
            {
              worker.ReportProgress(100, getProgressString(i + 1));
            }
          }
          else //UPDATE ISSUE
          {
            var oldIssue = response4.Data.issues.First();
            if (issue.markup.Comment.Any())
            {
              issue.markup.Comment = new System.Collections.ObjectModel.ObservableCollection<CommentBCF>(issue.markup.Comment.Reverse());
              foreach (var c in issue.markup.Comment)
              {
                string normalized1 = Regex.Replace(c.Comment1, @"\s", "");
                if (oldIssue.fields.comment.comments.Any(o => Regex.Replace(o.body, @"\s", "").Equals(normalized1, StringComparison.OrdinalIgnoreCase)))
                  continue;

                var request3 = new RestRequest("issue/" + oldIssue.key + "/comment", Method.POST);
                request3.AddHeader("Content-Type", "application/json");
                request3.RequestFormat = RestSharp.DataFormat.Json;
                var newcomment = new { body = c.Comment1 };
                request3.AddBody(newcomment);
                var response3 = JiraClient.Client.Execute<Comment2>(request3);
                if (!RestCallback.Check(response3))
                  break;
              }
            }
          }

        } // END TRY


        catch (System.Exception ex1)
        {
          MessageBox.Show("exception: " + ex1);
        }


      }// END LOOP
    }
    private string getProgressString(int i)
    {
      string s = (issues.Count > 1) ? "s" : "";
      string desc = string.Format("Uploading {0}/{1} Issue{2} to Jira...",
                                              i,
                                              issues.Count,
                                              s);
      return desc;
    }
    private void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
    {

      progressWin.SetProgress(Math.Min(e.ProgressPercentage, 100), e.UserState.ToString());
    }
    void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
    {
      progressWin.Close();
      if (uploadErrors > 0)
      {
        string s = (uploadErrors > 1) ? "s" : "";
        MessageBox.Show(string.Format("{0} Issue{1} had errors!", uploadErrors, s), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
      }
      if (e.Cancelled)
      {
        MessageBox.Show("Upload Canceled!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
      }

      if (uploadComplete != null)
      {

        uploadComplete(this, new IntArg(projIndex));
      }
      if (delAfter)
        DeleteDirectory(path);
    }
    void worker_Kill(object sender, EventArgs e)
    {

      if (worker.IsBusy)
      {
        MessageBox.Show("exception: ");
        worker.CancelAsync();
      }

    }

    private void DeleteDirectory(string target_dir)
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
  }
}
