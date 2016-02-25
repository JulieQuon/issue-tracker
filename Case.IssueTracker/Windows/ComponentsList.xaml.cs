using System.Windows;
using Case.IssueTracker.Data;

namespace Case.IssueTracker.Windows
{
    /// <summary>
    /// Interaction logic for Components.xaml
    /// </summary>
    public partial class ComponentsList : Window
    {
        public ComponentsList(Component[] components)
        {
            InitializeComponent();

            componentsList.ItemsSource = components;

           
            //ObservableCollection<BCFComponent> ComponentsCollection = new ObservableCollection<BCFComponent>();
           
            //if (v.Element("VisualizationInfo").Elements("Components").Any() && v.Element("VisualizationInfo").Elements("Components").Elements("Component").Any())
            //{

            //    IEnumerable<BCFComponent> result = from c in v.Element("VisualizationInfo").Elements("Components").Elements("Component")
            //                                          select new BCFComponent()
            //                                          {
            //                                              ifcguid = (string)c.Attribute("IfcGuid"),
            //                                              authoringid = (string)c.Element("AuthoringToolId"),
            //                                              origsystem = (string)c.Element("OriginatingSystem")
            //                                          };

                //foreach (var item in v.Element("VisualizationInfo").Elements("Components").Elements("Component"))
                //{
                //    BCFComponent bc = new BCFComponent();
                //    bc.ifcguid = (item.Attributes("IfcGuid").Any()) ? item.Attributes("IfcGuid").First().Value.ToString() : "";
                //    bc.authoringid = (item.Elements("AuthoringToolId").Any()) ? item.Element("AuthoringToolId").Value.ToString() : "";
                //    bc.origsystem = (item.Elements("OriginatingSystem").Any()) ? item.Element("OriginatingSystem").Value.ToString() : "";
                //    ComponentsCollection.Add(bc);
                //}
                //componentsList.ItemsSource = result;
                //componentsList.Items.Refresh();
            //}
            //MessageBox.Show(ComponentsCollection.Count().ToString());
            
        }
    }
}
