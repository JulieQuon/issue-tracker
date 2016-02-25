using System.Collections.Generic;

namespace Case.IssueTracker.Data
{
    public class Fields
    {
        public Resolution resolution { get; set; }
        public Priority priority { get; set; }
        public User creator { get; set; }
        public Project project { get; set; }
        public string summary { get; set; }
        public Status status { get; set; }
        public string created { get; set; }
        public string updated { get; set; }
        public string description { get; set; }
        public User assignee { get; set; }
        public Issuetype issuetype { get; set; }
        public List<Attachment> attachment { get; set; }
        public Comment comment { get; set; }
        public List<Component> components { get; set; }
        public string customfield_10900 { get; set; }
        public string customfield_11600 { get; set; }

        public string guid
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(customfield_10900))
                    return customfield_10900;
                if (!string.IsNullOrWhiteSpace(customfield_11600))
                    return customfield_11600;
                return string.Empty;
            }
        }
        //public List<User> customfield_11400 { get; set; }
        //public List<Component> components { get; set; }
       // public List<string> labels { get; set; }
    }


}
