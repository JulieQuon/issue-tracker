# Issue Tracker

The Issue Tracker is an advanced system that tracks building issues similar to the way issues are tracked in the software world. It was developed internally at [CASE](http://case-inc.com) and open sourced through [WeWork](http://wework.com).

While we hope some of our old and new friends continue to actively contribute to these apps on their own — we, as WeWork, will not be managing the GitHub community. We’ve included a description and instructions on how to access the open source code. After that, it’s up to you! 

By using the issue tracker for instance, a designer working directly in Navisworks can run a clash test, select a series of issues, and exchange them with the structural engineer. The engineer can then open the corresponding views in Revit, and see the clashing elements without having to exchange any geometry file. These issues are stored and viewable online on a JIRA instance, making it straightforward to manage their resolution, assign users and priority, comment, etc. 

The Issue Tracker is essentially made up of a set of clients that works directly within authoring and analysis tools such as Revit, Navisworks, Windows Standalone Client that consumes BCF files, and therefore Solibri and Tekla BIMSight outputs. 

The information is sent using the [BCF schema](https://github.com/BuildingSMART/BCF-XML), the open standard to exchange BIM information. As a matter of fact, each JIRA issue will be a building issue and contain descriptions such as camera location, snapshots, list of involved elements, etc. 

![](https://github.com/WeConnect/issue-tracker/blob/master/Assets/workflow.jpg)


As this system is quite advanced we are releasing it as code-only and not providing installers. Our hope is that a community will form and help update the app to support all BCF 2 features, integrate with more authoring and analysis tools, add user friendly installers, and more.

Huge thanks to [Matteo Cominetti](https://github.com/teocomi) for developing it!

More info on the Issue Tracker, download this PDF file: [Issue Tracker.pdf](https://github.com/WeConnect/issue-tracker/raw/master/Assets/CIT-%20CASE%20Issue%20Tracker%2020150129.pdf)


#Documentation

##Requirements

### General
.NET 4.5 is required by the application, it can be downloaded from: http://www.microsoft.com/net

###	JIRA specific
- A JIRA instance (hosted or self-hosted)
- JIRA REST API enabled
- The JIRA instance to have a custom field named GUID (its filed ID, eg _customfield10900_, needs to be set in the Case.IssueTracker > Data > UserSettings.cs file
- User accounts with permission to create issues in the designed JIRA projects

The following fields need to be accessible for the JIRA projects:
- Summary
- Key
- Created
- Updated
- Description
- Assignee
- Comment
- Attachment
- Reporter
- Status
- Priority
- Resolution
- Issuetype

###	Revit – IFC
To have successful exports from Revit 2014 to IFC it is necessary to:
- Update Revit to [Service Pack 2](http://knowledge.autodesk.com/support/revit-products/downloads/caas/downloads/content/revit-2014-update-release-2.html) – build number 20131024_2115  (requires [Service Pack 1](http://knowledge.autodesk.com/support/revit-products/downloads/caas/downloads/content/revit-2014-update-release-1.html)
- Install [IFC Exporter for Revit 2014](http://sourceforge.net/projects/ifcexporter/files/2014/)
- Install [IFC Exporter UI for Revit 2014](http://sourceforge.net/projects/ifcexporter/files/2014 UI/)

To have successful exports from Revit 2015 and 2016 to IFC it is necessary to:
- Install [IFC Exporter for Revit 2015](http://sourceforge.net/projects/ifcexporter/files/2015/) or [2016](https://sourceforge.net/projects/ifcexporter/files/2016/)

##Interface
User interface is consistent across the three clients and it's split in two panels. The JIRA panel lets the user connect to the JIRA server and manage, import, export issues. The BCF panel is for managing BCF reports (.bcfzip files) and BCF issues.

### Jira Panel
####Main Controls
![](https://github.com/WeConnect/issue-tracker/blob/master/Assets/jirapanel.png)

1. Tab control to switch between the JIRA and BCF panels
2. JIRA Project selection combo box
3. Issue List
4. Selected issue details (if multiple issues are selected, will display only the first)
5. Selected issue comments

####Actions
![](https://github.com/WeConnect/issue-tracker/blob/master/Assets/jirapanel2.png)

1. Opens the selected project in the browser
2. Reloads the list of projects and reselects the current one
3. Opens the Settings window
4. Changes relatively Assignee, Issue Type, Status or Priority for the selected issue or issues (change of status will only be applied to the first selected issue)
5. Opens the Snapshot in a new window
6. Creates a new view corresponding to the one defined in the Viewpoint. _Available only in the Revit and Navisworks Addins_
7. Opens a windows containing a list of attached elements
8. Creates a new Issue directly into JIRA (see specific page). Available only in the Revit and Naviswoks Addins
9. Deletes the selected issue or issues
10.	Exports the selected issues to BCF. Only issues that have a viewpoint.bcfv and a snapshot.png attachments will be exported.
11. Pagination for when there are more than 50 issues in the list
12. Adds a comment to the selected issue or issues
13. Expands the Filter section (see specific page)
14. Opens the selected issue in the browser

###BCF Panel
The BCF panel follows the same logic of the JIRA panel, but the project combo box is being replaced with a menu bar with controls to Save, Open and create New BCFs.

####Main Controls
![](https://github.com/WeConnect/issue-tracker/blob/master/Assets/bcfpanel.png)

####Actions
![](https://github.com/WeConnect/issue-tracker/blob/master/Assets/bcfpanel2.png)

Same as in the JIRA panel with some exceptions:
1. Creates a new report clearing the current list of issues. Available only in the Revit and Navisworks Addins
2. Prompts to save the current report as BCF
3. Opens an existing BCF report. **N.B. reports can be also opened by dragging them onto the application’s main window (it will gray out)** 
4. Prompts to upload the selected issues to JIRA
5. Deletes the selected issue or issues

##Settings
CIT has one main settings file editable through the “Settings” button within the applications and also project specific settings files available for Revit Projects. 
###Main Settings
Settings are stored in:

	%LocalAppData%\CASE\CASE Issue Tracker\CASEIssueTracker.config
    
therefore are individual for each user on the machine. Settings are unique for the three applications and will not be overwritten by application updates.

![](https://github.com/WeConnect/issue-tracker/blob/master/Assets/settings.png)

####JIRA Settings
By default, at every application launch a connection to JIRA will be established only if Username and Password are filled in. To use the application without the online JIRA functionality simply clear up Username and Password fields and then save the settings.

####BCF Settings
The designed username will be used when creating new comments if no connection to JIRA is established.

####General Settings
Takes effect only in Revit and Navisworks when clicking the “Open 3D View” button. It will either select or isolate the attached elements to the Viewpoint if existing in the open model.

### Hidden Settings
Hidden Settings are are optional Revit Projects Setting and not created by default. In order to set up a Revit project settings file it is necessary to create a .bcfconfig file in the same folder as the .rvt file named as the Revit file (including extension). Therefore, for a SampleProject.rvt it will be necessary to create a SampleProject.rvt.bcfconfig file in the same directory and it will only affect BCF files used on that project.
The content of the .bcfconfig  has to be structured as follows:

    <?xml version="1.0" encoding="utf-8"?>
  	<configuration>
  		<appSettings>
  			<add key="useDefaultZoom" value="0" />
  		</appSettings>
  	</configuration> 
    
Boolean: 0 false, 1 true. Defaul value = 0
Set it to 1 if working to/from Tekla so that Orthographic and Perspective views will be adjusted to its zoom factor. If not planning to exchange BCF files with Tekla set it to 1 so that views will not be scaled.

##Usage
Usage of the three applications is the same, although CIT for Windows does not provide functionality to create new issues and open viewpoints. Also, the way issues are created in CIT for Revit and Navisworks is different.

### General usage (CIT for Windows)

####Filters
![](https://github.com/WeConnect/issue-tracker/blob/master/Assets/filters.png)

From the JIRA tab, issues can be filtered by Issue Type, Status and Priority. After selecting one or more filters and clicking “Apply” the filter bar will be highlighted.
**N.B.
If a certain Issue Type, Status or Priority is being filtered out and a new issue with that Issue Type, Status or Priority is created it won’t be visible until that filter is cleared.**
Order, not being a real filter, is always applied to the query retrieving the list of issues.

####Export JIRA issues to BCF
From the JIRA panel, by selecting one or more issues and clicking the “Export to BCF” it is possible to create a new BCF report containing the issues.
It is important to keep in mind the following:
- Only issues that have a viewpoint.bcfv and a snapshot.png attachments will be exported
- Only 1 viewpoint.bcfv and 1 snapshot.png per issue are allowed
- The issue description will not be exported
- The BCF issue and comments creation dates will reflect JIRA’s creation dates
- The BCF comments author dates will reflect JIRA’s author
- Issue Type and Priority will not be exported
- The JIRA issue Status will be used as Verbal Status for each BCF comment
- Each BCF comment will have a Status set to UNKNOWN
- A new GUID will be generated for each issue that does not have one set as JIRA’s GUID custom field

####Upload BCF issues to JIRA
From the BCF panel, by selecting one or more issues and clicking the “Upload to JIRA” it is possible to create new JIRA issues. The user will be prompted to select a project and an Issue Type for the new issues.
It is important to keep in mind the following:
- Author of the issues and comments will be the user currently logged in with a JIRA account and not the one set in the BCF file
- Creation date and date of issues and comments will be the current time while the operation is processing
- Status and Verbal Status will not be stored in the JIRA issue
- Default priority and status set in JIRA will be applied to the new issues
- **If an issue with the same GUID is already existing in JIRA only new comments will be uploaded** (by new comments it is intended comments whose body text does not already appear in the selected issue)

###Use in Revit
The CIT for Revit can be accessed from the CASE ribbon panel.

![](https://github.com/WeConnect/issue-tracker/blob/master/Assets/ribbon.png) 

####Add an Issue
Adding a new issue to a JIRA project or to a BCF report follows the same procedure. The active view has to be either a 3D view (orthographic) or a camera view (perspective). After clicking the “Add Issue” button, the following window will appear:

![](https://github.com/WeConnect/issue-tracker/blob/master/Assets/addissue.png)

- An issue title is required
- The generated snapshot can be annotated clicking on “Annotate Snapshot” (N.B. you need to save and then close MS Paint for the changes to take effect)
- The “Attach elements to view” section lets the user decide which elements to attach to the issue. N.B. to attach selected elements, those needs to be selected before clicking the “Add Issue” button
- It is possible to change visual style of the snapshot by selecting a different one via the drop down menu
- It is also possible to load a local image via the “Browse” button
- Optionally a first comment can be added to the issue

####Open a 3D View
Opening a 3D view in Revit can only be done if the active view is not a perspective (camera) view; in that case switch to any other type of view (2D or 3D).
If the issue contains an orthographic view (as specified in the viewpoint file) it will be tried to open it into an existing orthographic view, orthewise a new one will be created.
If the issue contains a perspective view a new “BCFpersp” view will be created and used for the following perspective views.
Attached elements will be either selected or isolated accordingly to the settings.

###Use in Navisworks

####Add an Issue
Issues in Navisworks are added from existing Saved Viewpoints. The process of adding a new issue is different than in Revit since it’s meant to happen for more issues at the same time (i.e. batch creation after a clash test).

![](https://github.com/WeConnect/issue-tracker/blob/master/Assets/addissue2.png)

![](https://github.com/WeConnect/issue-tracker/blob/master/Assets/addissue3.png)

After Clicking “Add Issue” it is possible to select one or more Viewpoints to add as issues, the “Attach elements to view” section lets the user decide which elements to attach to the issue. N.B. to attach selected elements, those needs to be selected before clicking the “Add Issue” button. Only Selected Viewpoints will be used and their comments will be used as BCF or JIRA comments.

####Open a 3D View
Opening a 3D view works the same way as in Revit, but the active view will always be used and no view will be added to the “Save Viewpoints” panel.

###Model export from Revit

####Revit to Tekla
Please refer to the requirements chapter before proceeding. Go to: Revit>Export>IFC>Modify Setup…
**Mind to check “Include IFCSITE Elevation in the site local placement origin”.**

![](https://github.com/WeConnect/issue-tracker/blob/master/Assets/ifc.png)

####Revit to Navisworks
For optimal results, it is suggested to use the NWC export utility from within Revit and export as NWC or to use an IFC file generated as mentioned above.

## Project Structure
The IssueTracker.sln solution contains the following projects:
- Case.IssueTracker is the main project containing all the BCF logic and API calls
- Case.IssueTracker.Win is a windows standalone implementation, referencing and embedding the Case.IssueTracker code
- Case.IssueTracker.Revit and Case.IssueTracker.Navisworks are the Revit and Navisworks plugins, both referencing and embedding the Case.IssueTracker code. 

### Case.IssueTracker Project
Is the main project containing the core methods and UI elements, these are the followings.
Inside UserControls you have:

- BCFPanel.xaml that contains the BCF specific UI and methods
- JiraPanel.xaml that contains the JIRA specific UI and methods
- MainPanel.xaml that contains a TabControl with the BCFPanel and the JiraPanel and all shared UI and methos. This is the control referenced and embedded by the addins

Inside Classes you have:
- Jira.cs is the ViewModel that binds to all UI controls and cointans the list of JIRA porjects and issues
- BCF/BCF.cs the ViewModel for the open BCF file, binds to the BCFPanel.xmal

### Building 2014, 2015 and 2016 Addins
The .csproj file of the Revit and Navisworks addin have been edited adding Debug and Release configurations for the 2014, 2015 and 2016 versions of the softwares. Each configuration will reference the respective API.

#### Debugging
Inside the .csproj, post-build events have been added to copy the built dlls to the respective Revit or Navisworks path, in the correct year folder. To be able to debug correctly, for instance a Revit 2015 version of CIT, do the following:
- in VisualStudio, from the build configuration dropdown select Debug-2015
- open the Case.IssueTracker.Revit project properties, click on Debug
- set the Start Action to the path of the Revit 2015 .exe (should be C:\Program Files\Autodesk\Revit 2015\Revit.exe)

The Start Action will be remembered per build configuration, so the steps above are only needed once.

##Future developments and Limitations
Issue Tracking is fundametal for assuring quality of buildings, we hope CIT will play a key role in facilitating the adoption of modern tracking techniques even by small practices.

###Future developments
With the help of the community we hope to bring CIT to fully support BCF2 and  the BCF API so that it could connect to any BCF server. Also, we hope to see integrations with more authoring and analysis tools.

A similar effort to CIT is [BCFier](https://github.com/teocomi/BCFier), it was also started by Matteo Cominetti, but outside CASE/WeWork. Eventually the two projects could converge into a single one (this hasn't happened before due to IP limitations).

###Current Limitations
- The way BCF files are interpreted/created by software vendors is different, in particular concerning the FOV of perspective views and the scale of orthogonal views. Viewpoints might result in having different “zoom/scale” and location. CIT is optimized to work with Tekla BIM Sight and Navisworks, but full compatibility cannot be guaranteed with BCF files coming from other software
- No more than one snapshot/viewpoint are allowed per issue, as for BCF 1.0 schema
- No 2D views are allowed: issues cannot contain plan views, sections, details and elevations, as for BCF 1.0 schema
- No native tools as section boxes or crop regions are supported by BCF
- When uploading BCF files to JIRA the issue and comment dates will reflect the creation date on JIRA and not the one of the BCF file itself
- BCF assigns statuses to comments while JIRA to issues
- Attaching all the visible elements to a new issue from very big models in Navisworks and Revit could crash the application since it needs to loop through all the model’s elements
- A current limitation in the Revit API does not allow to access elements that are part of a linked model. Consequence is that attached elements cannot include linked models

## License
GNU GENERAL PUBLIC LICENSE v3
