using System;
using System.Configuration;
using System.Windows;

namespace Case.IssueTracker.Data
{
  /// <summary>
  /// store user settings to file and retrieve them
  /// </summary>
  public static class UserSettings
  {
    //fallback server URL
    private const string Jiraservercase = "https://casedesigninc.atlassian.net";
    public static string Get(string key)
    {
      try
      {
        //put here the ID of your GUID custom field
        if (key == "guidfield")
          return "customfield_10900";


        Configuration config = GetConfig();

        if (config == null)
          return string.Empty;


        KeyValueConfigurationElement element = config.AppSettings.Settings[key];
        if (element != null)
        {
          string value = element.Value;
          if (string.IsNullOrEmpty(value) && key == "jiraserver")
            return Jiraservercase;
          if (!string.IsNullOrEmpty(value))
            return value;
        }
        else
        {
          config.AppSettings.Settings.Add(key, "");
          config.Save(ConfigurationSaveMode.Modified);
        }
      }
      catch (System.Exception ex1)
      {
        MessageBox.Show("exception: " + ex1);
      }
      return string.Empty;
    }
    public static void Set(string key, string value)
    {
      try
      {
        Configuration config = GetConfig();
        if (config == null)
          return;

        KeyValueConfigurationElement element = config.AppSettings.Settings[key];
        if (element != null)
          element.Value = value;
        else
          config.AppSettings.Settings.Add(key, value);

        config.Save(ConfigurationSaveMode.Modified);

      }
      catch (System.Exception ex1)
      {
        MessageBox.Show("exception: " + ex1);
      }
    }
    private static Configuration GetConfig()
    {

      string issuetracker = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CASE", "CASE Issue Tracker", "CASEIssueTracker.config");

      ExeConfigurationFileMap configMap = new ExeConfigurationFileMap();
      configMap.ExeConfigFilename = issuetracker;
      Configuration config = ConfigurationManager.OpenMappedExeConfiguration(configMap, ConfigurationUserLevel.None);

      return config;
    }
  }
}
