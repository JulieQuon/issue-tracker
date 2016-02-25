using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Case.IssueTracker.UserControls;
using RestSharp;

namespace Case.IssueTracker.Data
{
    public static class JiraClient
    {
        private static RestClient client;
        public static RestClient Client
        {
            get
            {
                return client;
            }

            set
            {
                client = value;
            }
        }

        public static Waiter Waiter;
    }
}
