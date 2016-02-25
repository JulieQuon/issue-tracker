using System.Collections.Generic;

namespace Case.IssueTracker.Data
{
    public class ProjectStatuses
    {
        public string self { get; set; }
        public string id { get; set; }
        public string name { get; set; }
        public bool subtask { get; set; }
        public List<Status> statuses { get; set; }
    }
}
