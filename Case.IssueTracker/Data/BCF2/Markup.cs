﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:2.0.50727.5485
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Xml.Serialization;

// 
// This source code was auto-generated by xsd, Version=2.0.50727.3038.
// 

namespace Case.IssueTracker.Data.BCF2
{
  /// <remarks/>
  [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
  [Serializable()]
  [System.Diagnostics.DebuggerStepThroughAttribute()]
  [DesignerCategory("code")]
  [XmlType(AnonymousType = true)]
  [XmlRoot(Namespace = "", IsNullable = false)]
  public partial class Markup : INotifyPropertyChanged
  {

    private List<HeaderFile> headerField;

    private Topic topicField;

    private ObservableCollection<Comment> commentField;

    private ObservableCollection<ViewPoint> viewpointsField;


    /// <remarks/>
    [XmlArray(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    [XmlArrayItem("File", Form = System.Xml.Schema.XmlSchemaForm.Unqualified,
      IsNullable = false)]
    public List<HeaderFile> Header
    {
      get { return this.headerField; }
      set { this.headerField = value; }
    }

    /// <remarks/>
    [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public Topic Topic
    {
      get { return this.topicField; }
      set { this.topicField = value; }
    }

    /// <remarks/>
    [XmlElement("Comment", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public ObservableCollection<Comment> Comment
    {
      get { return this.commentField; }
      set
      {
        this.commentField = value;
        NotifyPropertyChanged("Comment");
      }
    }

    /// <remarks/>
    [XmlElement("Viewpoints", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public ObservableCollection<ViewPoint> Viewpoints
    {
      get { return this.viewpointsField; }
      set
      {
        this.viewpointsField = value;
        NotifyPropertyChanged("Viewpoints");

      }
    }
    /// <summary>
    /// Generates ViewCommentobjects from View and Comments Dynamically
    /// Could be removed by implmenting a proper MVVM model
    /// But this approach simplifies things a lot 
    /// </summary>

    [XmlIgnore()]
    public ObservableCollection<ViewComment> ViewComments
    {
      get
      {
        var viewCommentsField = new ObservableCollection<ViewComment>();
        foreach (var viewpoint in Viewpoints)
        {
          var vc = new ViewComment
          {
            Viewpoint = viewpoint,
            Comments = new ObservableCollection<Comment>(Comment.Where(x => x.Viewpoint != null && x.Viewpoint.Guid == viewpoint.Guid))
          };
          viewCommentsField.Add(vc);
        }
        var vcEmpty = new ViewComment
        {
          Comments =
            new ObservableCollection<Comment>(Comment.Where(x => !Viewpoints.Any(v => x.Viewpoint != null && v.Guid == x.Viewpoint.Guid)))
        };
        viewCommentsField.Add(vcEmpty);
        return viewCommentsField;
      }
    }

    [XmlIgnore()]
    public string LastCommentStatus
    {
      get
      {
        if (Comment == null || !Comment.Any())
          return "";

        return Comment.LastOrDefault().Status;
      }   
    }

    [XmlIgnore()]
    public string LastCommentVerbalStatus
    {
      get
      {
        if (Comment == null || !Comment.Any())
          return "";

        return Comment.LastOrDefault().VerbalStatus;
      }
    }

    //parameterless constructor needed
    public Markup()
    {
    }
    public Markup(DateTime created)
    {
      Topic = new Topic();
      Comment = new ObservableCollection<Comment>();
      Viewpoints = new ObservableCollection<ViewPoint>();
      RegisterEvents();
      Header = new List<HeaderFile> { new HeaderFile { Date = created, DateSpecified = true } };
    }
    //when Views or comments change refresh the ViewComments
    public void RegisterEvents()
    {
      if (Viewpoints != null)
        Viewpoints.CollectionChanged += delegate(object sender, NotifyCollectionChangedEventArgs args) { NotifyPropertyChanged("ViewComments"); };
      if (Comment != null)
        Comment.CollectionChanged += CommentOnCollectionChanged;
    }

    private void CommentOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
    {
      NotifyPropertyChanged("ViewComments");
      NotifyPropertyChanged("LastCommentStatus");
      NotifyPropertyChanged("LastCommentVerbalStatus"); 
    }

    

    [field: NonSerialized]
    public event PropertyChangedEventHandler PropertyChanged;
    private void NotifyPropertyChanged(String info)
    {
      if (PropertyChanged != null)
      {
        PropertyChanged(this, new PropertyChangedEventArgs(info));
      }
    }
  }

  /// <remarks/>
  [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
  [Serializable()]
  [System.Diagnostics.DebuggerStepThroughAttribute()]
  [DesignerCategory("code")]
  [XmlType(AnonymousType = true)]
  public partial class HeaderFile
  {

    private string filenameField;

    private System.DateTime dateField;

    private bool dateFieldSpecified;

    private string referenceField;

    private string ifcProjectField;

    private string ifcSpatialStructureElementField;

    private bool isExternalField;

    public HeaderFile()
    {
      this.isExternalField = true;
    }

    /// <remarks/>
    [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string Filename
    {
      get { return this.filenameField; }
      set { this.filenameField = value; }
    }

    /// <remarks/>
    [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public System.DateTime Date
    {
      get { return this.dateField; }
      set { this.dateField = value; }
    }

    /// <remarks/>
    [XmlIgnore()]
    public bool DateSpecified
    {
      get { return this.dateFieldSpecified; }
      set { this.dateFieldSpecified = value; }
    }

    /// <remarks/>
    [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string Reference
    {
      get { return this.referenceField; }
      set { this.referenceField = value; }
    }

    /// <remarks/>
    [XmlAttribute()]
    public string IfcProject
    {
      get { return this.ifcProjectField; }
      set { this.ifcProjectField = value; }
    }

    /// <remarks/>
    [XmlAttribute()]
    public string IfcSpatialStructureElement
    {
      get { return this.ifcSpatialStructureElementField; }
      set { this.ifcSpatialStructureElementField = value; }
    }

    /// <remarks/>
    [XmlAttribute()]
    [DefaultValue(true)]
    public bool isExternal
    {
      get { return this.isExternalField; }
      set { this.isExternalField = value; }
    }
  }

  /// <remarks/>
  [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
  [Serializable()]
  [System.Diagnostics.DebuggerStepThroughAttribute()]
  [DesignerCategory("code")]
  public partial class ViewPoint : INotifyPropertyChanged
  {

    private string viewpointField;

    private string snapshotField;

    private string guidField;

    /// <remarks/>
    [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string Viewpoint
    {
      get { return this.viewpointField; }
      set { this.viewpointField = value; }
    }

    /// <remarks/>
    [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string Snapshot
    {
      get { return this.snapshotField; }
      set { this.snapshotField = value; }
    }

    /// <remarks/>
    [XmlAttribute()]
    public string Guid
    {
      get { return this.guidField; }
      set { this.guidField = value; }
    }
    public ViewPoint()
    {
    }
    public ViewPoint(bool isFirst)
    {
      Guid = System.Guid.NewGuid().ToString();
      if (isFirst)
      {
        Viewpoint = "viewpoint.bcfv";
        Snapshot = "snapshot.png";
      }
      else
      {
        Viewpoint = Guid + ".bcfv";
        Snapshot = Guid + ".png";
      }

    }

    [field: NonSerialized]
    public event PropertyChangedEventHandler PropertyChanged;
    private void NotifyPropertyChanged(String info)
    {
      if (PropertyChanged != null)
      {
        PropertyChanged(this, new PropertyChangedEventArgs(info));
      }
    }
  }

  /// <remarks/>
  [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
  [Serializable()]
  [System.Diagnostics.DebuggerStepThroughAttribute()]
  [DesignerCategory("code")]
  public partial class Comment
  {

    private string verbalStatusField;

    private string statusField;

    private System.DateTime dateField;

    private string authorField;

    private string comment1Field;

    private CommentTopic topicField;

    private CommentViewpoint viewpointField;

    private CommentReplyToComment replyToCommentField;

    private System.DateTime modifiedDateField;

    private bool modifiedDateFieldSpecified;

    private string modifiedAuthorField;

    private string guidField;

    public Comment()
    {
      this.statusField = "Unknown";
    }

    /// <remarks/>
    [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string VerbalStatus
    {
      get { return this.verbalStatusField; }
      set { this.verbalStatusField = value; }
    }

    /// <remarks/>
    [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string Status
    {
      get { return this.statusField; }
      set { this.statusField = value; }
    }

    /// <remarks/>
    [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public System.DateTime Date
    {
      get { return this.dateField; }
      set { this.dateField = value; }
    }

    /// <remarks/>
    [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string Author
    {
      get { return this.authorField; }
      set { this.authorField = value; }
    }

    /// <remarks/>
    [XmlElement("Comment", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string Comment1
    {
      get { return this.comment1Field; }
      set { this.comment1Field = value; }
    }

    /// <remarks/>
    [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public CommentTopic Topic
    {
      get { return this.topicField; }
      set { this.topicField = value; }
    }

    /// <remarks/>
    [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public CommentViewpoint Viewpoint
    {
      get { return this.viewpointField; }
      set { this.viewpointField = value; }
    }

    /// <remarks/>
    [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public CommentReplyToComment ReplyToComment
    {
      get { return this.replyToCommentField; }
      set { this.replyToCommentField = value; }
    }

    /// <remarks/>
    [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public System.DateTime ModifiedDate
    {
      get { return this.modifiedDateField; }
      set { this.modifiedDateField = value; }
    }

    /// <remarks/>
    [XmlIgnore()]
    public bool ModifiedDateSpecified
    {
      get { return this.modifiedDateFieldSpecified; }
      set { this.modifiedDateFieldSpecified = value; }
    }

    /// <remarks/>
    [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string ModifiedAuthor
    {
      get { return this.modifiedAuthorField; }
      set { this.modifiedAuthorField = value; }
    }

    /// <remarks/>
    [XmlAttribute()]
    public string Guid
    {
      get { return this.guidField; }
      set { this.guidField = value; }
    }
  }

  /// <remarks/>
  [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
  [Serializable()]
  [System.Diagnostics.DebuggerStepThroughAttribute()]
  [DesignerCategory("code")]
  [XmlType(AnonymousType = true)]
  public partial class CommentTopic
  {

    private string guidField;

    /// <remarks/>
    [XmlAttribute()]
    public string Guid
    {
      get { return this.guidField; }
      set { this.guidField = value; }
    }
  }

  /// <remarks/>
  [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
  [Serializable()]
  [System.Diagnostics.DebuggerStepThroughAttribute()]
  [DesignerCategory("code")]
  [XmlType(AnonymousType = true)]
  public partial class CommentViewpoint
  {

    private string guidField;

    /// <remarks/>
    [XmlAttribute()]
    public string Guid
    {
      get { return this.guidField; }
      set { this.guidField = value; }
    }
  }

  /// <remarks/>
  [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
  [Serializable()]
  [System.Diagnostics.DebuggerStepThroughAttribute()]
  [DesignerCategory("code")]
  [XmlType(AnonymousType = true)]
  public partial class CommentReplyToComment
  {

    private string guidField;

    /// <remarks/>
    [XmlAttribute()]
    public string Guid
    {
      get { return this.guidField; }
      set { this.guidField = value; }
    }
  }

  /// <remarks/>
  [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
  [Serializable()]
  [System.Diagnostics.DebuggerStepThroughAttribute()]
  [DesignerCategory("code")]
  public partial class BimSnippet
  {

    private string referenceField;

    private string referenceSchemaField;

    private string snippetTypeField;

    private bool isExternalField;

    public BimSnippet()
    {
      this.isExternalField = false;
    }

    /// <remarks/>
    [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string Reference
    {
      get { return this.referenceField; }
      set { this.referenceField = value; }
    }

    /// <remarks/>
    [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string ReferenceSchema
    {
      get { return this.referenceSchemaField; }
      set { this.referenceSchemaField = value; }
    }

    /// <remarks/>
    [XmlAttribute()]
    public string SnippetType
    {
      get { return this.snippetTypeField; }
      set { this.snippetTypeField = value; }
    }

    /// <remarks/>
    [XmlAttribute()]
    [DefaultValue(false)]
    public bool isExternal
    {
      get { return this.isExternalField; }
      set { this.isExternalField = value; }
    }
  }

  /// <remarks/>
  [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
  [Serializable()]
  [System.Diagnostics.DebuggerStepThroughAttribute()]
  [DesignerCategory("code")]
  public partial class Topic
  {

    private string referenceLinkField;

    private string titleField;

    private string priorityField;

    private string indexField;

    private string[] labelsField;

    private System.DateTime creationDateField;

    private bool creationDateFieldSpecified;

    private string creationAuthorField;

    private System.DateTime modifiedDateField;

    private bool modifiedDateFieldSpecified;

    private string modifiedAuthorField;

    private string assignedToField;

    private string descriptionField;

    private BimSnippet bimSnippetField;

    private TopicDocumentReferences[] documentReferencesField;

    private TopicRelatedTopics[] relatedTopicsField;

    private string guidField;

    private string topicTypeField;

    private string topicStatusField;

    /// <remarks/>
    [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string ReferenceLink
    {
      get { return this.referenceLinkField; }
      set { this.referenceLinkField = value; }
    }

    /// <remarks/>
    [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string Title
    {
      get { return this.titleField; }
      set { this.titleField = value; }
    }

    /// <remarks/>
    [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string Priority
    {
      get { return this.priorityField; }
      set { this.priorityField = value; }
    }

    /// <remarks/>
    [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified,
      DataType = "integer")]
    public string Index
    {
      get { return this.indexField; }
      set { this.indexField = value; }
    }

    /// <remarks/>
    [XmlElement("Labels", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string[] Labels
    {
      get { return this.labelsField; }
      set { this.labelsField = value; }
    }

    /// <remarks/>
    [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public System.DateTime CreationDate
    {
      get { return this.creationDateField; }
      set { this.creationDateField = value; }
    }

    /// <remarks/>
    [XmlIgnore()]
    public bool CreationDateSpecified
    {
      get { return this.creationDateFieldSpecified; }
      set { this.creationDateFieldSpecified = value; }
    }

    /// <remarks/>
    [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string CreationAuthor
    {
      get { return this.creationAuthorField; }
      set { this.creationAuthorField = value; }
    }

    /// <remarks/>
    [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public System.DateTime ModifiedDate
    {
      get { return this.modifiedDateField; }
      set { this.modifiedDateField = value; }
    }

    /// <remarks/>
    [XmlIgnore()]
    public bool ModifiedDateSpecified
    {
      get { return this.modifiedDateFieldSpecified; }
      set { this.modifiedDateFieldSpecified = value; }
    }

    /// <remarks/>
    [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string ModifiedAuthor
    {
      get { return this.modifiedAuthorField; }
      set { this.modifiedAuthorField = value; }
    }

    /// <remarks/>
    [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string AssignedTo
    {
      get { return this.assignedToField; }
      set { this.assignedToField = value; }
    }

    /// <remarks/>
    [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string Description
    {
      get { return this.descriptionField; }
      set { this.descriptionField = value; }
    }

    /// <remarks/>
    [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public BimSnippet BimSnippet
    {
      get { return this.bimSnippetField; }
      set { this.bimSnippetField = value; }
    }

    /// <remarks/>
    [XmlElement("DocumentReferences",
      Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public TopicDocumentReferences[] DocumentReferences
    {
      get { return this.documentReferencesField; }
      set { this.documentReferencesField = value; }
    }

    /// <remarks/>
    [XmlElement("RelatedTopics", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public TopicRelatedTopics[] RelatedTopics
    {
      get { return this.relatedTopicsField; }
      set { this.relatedTopicsField = value; }
    }

    /// <remarks/>
    [XmlAttribute()]
    public string Guid
    {
      get { return this.guidField; }
      set { this.guidField = value; }
    }

    /// <remarks/>
    [XmlAttribute()]
    public string TopicType
    {
      get { return this.topicTypeField; }
      set { this.topicTypeField = value; }
    }

    /// <remarks/>
    [XmlAttribute()]
    public string TopicStatus
    {
      get { return this.topicStatusField; }
      set { this.topicStatusField = value; }
    }

    public Topic()
    {
      Guid = System.Guid.NewGuid().ToString();
      CreationDate = DateTime.Now;
      ModifiedDate = CreationDate;
    }
  }

  /// <remarks/>
  [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
  [Serializable()]
  [System.Diagnostics.DebuggerStepThroughAttribute()]
  [DesignerCategory("code")]
  [XmlType(AnonymousType = true)]
  public partial class TopicDocumentReferences
  {

    private string referencedDocumentField;

    private string descriptionField;

    private string guidField;

    private bool isExternalField;

    public TopicDocumentReferences()
    {
      this.isExternalField = false;
    }

    /// <remarks/>
    [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string ReferencedDocument
    {
      get { return this.referencedDocumentField; }
      set { this.referencedDocumentField = value; }
    }

    /// <remarks/>
    [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string Description
    {
      get { return this.descriptionField; }
      set { this.descriptionField = value; }
    }

    /// <remarks/>
    [XmlAttribute()]
    public string Guid
    {
      get { return this.guidField; }
      set { this.guidField = value; }
    }

    /// <remarks/>
    [XmlAttribute()]
    [DefaultValue(false)]
    public bool isExternal
    {
      get { return this.isExternalField; }
      set { this.isExternalField = value; }
    }
  }

  /// <remarks/>
  [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
  [Serializable()]
  [System.Diagnostics.DebuggerStepThroughAttribute()]
  [DesignerCategory("code")]
  [XmlType(AnonymousType = true)]
  public partial class TopicRelatedTopics
  {

    private string guidField;

    /// <remarks/>
    [XmlAttribute()]
    public string Guid
    {
      get { return this.guidField; }
      set { this.guidField = value; }
    }
  }
}
