using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Case.IssueTracker.Data
{
  /// <summary>
  /// Check a JIRA response
  /// </summary>
  public static class RestCallback
  {
    public static bool Check(IRestResponse response)
    {
      try
      {
        if (null == response)
        {
          string error = "Please check your connection.";
          MessageBox.Show(error, "Unknown error", MessageBoxButton.OK, MessageBoxImage.Error);
          return false;
        }



        if (response.StatusCode != System.Net.HttpStatusCode.OK
            && response.StatusCode != System.Net.HttpStatusCode.Created
            && response.StatusCode != System.Net.HttpStatusCode.NoContent
            && response != null)
        {
          object ve;
          if (RestSharp.SimpleJson.TryDeserializeObject(response.Content, out ve))
          {
            ErrorMsg validationErrorResponse = RestSharp.SimpleJson.DeserializeObject<ErrorMsg>(response.Content);
            string error = "";
            if (validationErrorResponse.errorMessages.Any())
            {
              foreach (var str in validationErrorResponse.errorMessages)
              {
                error += str + "\n";
              }

            }
            error += (null != validationErrorResponse.errors && validationErrorResponse.errors.ToString().Replace(" ", "") != "{}") ? validationErrorResponse.errors.ToString() : "";
            MessageBox.Show(error, response.StatusDescription, MessageBoxButton.OK, MessageBoxImage.Error);
          }
          else
          {
            string error = response.StatusDescription;
            if (string.IsNullOrWhiteSpace(error))
              error = "Please check your connection.";
            MessageBox.Show(error, response.ResponseStatus.ToString(), MessageBoxButton.OK, MessageBoxImage.Error);
          }
          return false;
        }

      }
      catch (Exception ex1)
      {
        MessageBox.Show("exception: " + ex1);
      }

      return true;
    }
  }
}
