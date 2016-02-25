using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Case.IssueTracker.Data
{
    // This is a class to convert data between BCF 1.0 and 2.0 
    public static class BcfAdapter
    {
        /// <summary>
        /// Save BCF 1.0 as BCF 2.0
        /// </summary>
        /// <param name="bcf1">BCF 1.0 file</param>
        public static void SaveBcf2FromBcf1(BCF bcf1)
        {
            try
            {
                BCF2.BcfFile bcf2 = new BCF2.BcfFile();
                bcf2.TempPath = bcf1.path; // replace temp path with BCF 1.0's

                // bcf2.ProjectName = ;    // from Jira?
                // bcf2.ProjectId = ;      // from Jira?

                // Add issues (markups)
                foreach (IssueBCF bcf1Issue in bcf1.Issues)
                {
                    // Convert header files
                    List<BCF2.HeaderFile> bcf2Headers = new List<BCF2.HeaderFile>();
                    if (bcf1Issue.markup.Header != null)
                    {                        
                        foreach (HeaderFile bcf1Header in bcf1Issue.markup.Header)
                        {
                            bcf2Headers.Add(new BCF2.HeaderFile()
                            {
                                Date = bcf1Header.Date,
                                Filename = bcf1Header.Filename,
                                IfcProject = bcf1Header.IfcProject,
                                IfcSpatialStructureElement = bcf1Header.IfcSpatialStructureElement,
                                isExternal = true, // default true for now
                                Reference = "" // default empty for now
                            });
                        }
                    }

                    // Convert Comments
                    ObservableCollection<BCF2.Comment> bcf2Comments = new ObservableCollection<BCF2.Comment>();
                    if (bcf1Issue.markup.Comment != null)
                    {                        
                        foreach (CommentBCF bcf1Comment in bcf1Issue.markup.Comment)
                        {
                            if (bcf1Comment != null)
                            {
                                bcf2Comments.Add(new BCF2.Comment()
                                {
                                    Author = bcf1Comment.Author,
                                    Comment1 = bcf1Comment.Comment1,
                                    Date = bcf1Comment.Date,
                                    Guid = bcf1Comment.Guid,
                                    ModifiedAuthor = bcf1Comment.Author,  // default the same as author for now
                                  //ModifiedDate = null,   // mismatch attribute
                                    ReplyToComment = null, // mismatch attribute
                                    Status = bcf1Comment.Status.ToString(),
                                    Topic = new BCF2.CommentTopic() { Guid = bcf1Issue.markup.Topic.Guid }, // all referenced to markup's topic
                                    VerbalStatus = bcf1Comment.VerbalStatus,
                                    Viewpoint = null //  mismatch attribute
                                });
                            }
                        }
                    }                   

                    // Convert Topic
                    BCF2.Topic bcf2Topic = new BCF2.Topic()
                    {
                        AssignedTo = null,  // mismatch attribute
                        BimSnippet = null,  // mismatch attribute
                        CreationAuthor = null,  // mismatch attribute
                        //CreationDate = null,  // mismatch attribute
                        Description = null,  // mismatch attribute
                        DocumentReferences = null,  // mismatch attribute
                        Guid = bcf1Issue.markup.Topic.Guid,
                        Index = null,  // mismatch attribute
                        Labels = null,  // mismatch attribute
                        ModifiedAuthor = null,  // mismatch attribute
                        //ModifiedDate = ,  // mismatch attribute
                        Priority = null,  // mismatch attribute
                        ReferenceLink = bcf1Issue.markup.Topic.ReferenceLink,
                        RelatedTopics = null,  // mismatch attribute
                        Title = bcf1Issue.markup.Topic.Title,
                        TopicStatus = null,  // mismatch attribute
                        TopicType = null  // mismatch attribute
                    };

                    // Convert ClippingPlane
                    List<BCF2.ClippingPlane> bcf2ClippingPlanes = new List<BCF2.ClippingPlane>();
                    if (bcf1Issue.viewpoint.ClippingPlanes != null)
                    {                        
                        foreach (ClippingPlane bcf1ClippingPlane in bcf1Issue.viewpoint.ClippingPlanes)
                        {
                            if (bcf1ClippingPlane != null)
                            {
                                bcf2ClippingPlanes.Add(new BCF2.ClippingPlane()
                                {
                                    Direction = new BCF2.Direction()
                                    {
                                        X = bcf1ClippingPlane.Direction.X,
                                        Y = bcf1ClippingPlane.Direction.Y,
                                        Z = bcf1ClippingPlane.Direction.Z
                                    },
                                    Location = new BCF2.Point()
                                    {
                                        X = bcf1ClippingPlane.Location.X,
                                        Y = bcf1ClippingPlane.Location.Y,
                                        Z = bcf1ClippingPlane.Location.Z
                                    }
                                });
                            }
                        }
                    }                    

                    // Convert Components
                    List<BCF2.Component> bcf2Components = new List<BCF2.Component>();
                    if (bcf1Issue.viewpoint.Components != null)
                    {                        
                        foreach (Component bcf1Component in bcf1Issue.viewpoint.Components)
                        {
                            if (bcf1Component != null)
                            {
                                bcf2Components.Add(new BCF2.Component()
                                {
                                    AuthoringToolId = bcf1Component.AuthoringToolId,
                                    // Color = bcf1Component,    // mismatch attribute
                                    IfcGuid = bcf1Component.IfcGuid,
                                    OriginatingSystem = bcf1Component.OriginatingSystem
                                    // Selected = bcf1Component,   // mismatch attribute
                                    // Visible = bcf1Component    // mismatch attribute
                                });
                            }
                        }
                    }                    

                    // Convert Lines
                    List<BCF2.Line> bcf2Lines = new List<BCF2.Line>();
                    if (bcf1Issue.viewpoint.Lines != null)
                    {                        
                        foreach (Line bcf1Line in bcf1Issue.viewpoint.Lines)
                        {
                            if (bcf1Line != null)
                            {
                                bcf2Lines.Add(new BCF2.Line()
                                {
                                    StartPoint = new BCF2.Point()
                                    {
                                        X = bcf1Line.StartPoint.X,
                                        Y = bcf1Line.StartPoint.Y,
                                        Z = bcf1Line.StartPoint.Z
                                    },
                                    EndPoint = new BCF2.Point()
                                    {
                                        X = bcf1Line.EndPoint.X,
                                        Y = bcf1Line.EndPoint.Y,
                                        Z = bcf1Line.EndPoint.Z
                                    }
                                });
                            }
                        }
                    }
                   
                    // Convert VisualizationInfo
                    BCF2.VisualizationInfo bcf2VizInfo = new BCF2.VisualizationInfo()
                    {
                        Bitmaps = null, // default null 
                        ClippingPlanes = bcf2ClippingPlanes.ToArray(),
                        Components = bcf2Components,
                        Lines = bcf2Lines.ToArray(),
                        OrthogonalCamera = bcf1Issue.viewpoint.OrthogonalCamera == null ? null : new BCF2.OrthogonalCamera()
                        {
                            CameraDirection = new BCF2.Direction()
                            {
                                X = bcf1Issue.viewpoint.OrthogonalCamera.CameraDirection.X,
                                Y = bcf1Issue.viewpoint.OrthogonalCamera.CameraDirection.Y,
                                Z = bcf1Issue.viewpoint.OrthogonalCamera.CameraDirection.Z
                            },
                            CameraUpVector = new BCF2.Direction()
                            {
                                X = bcf1Issue.viewpoint.OrthogonalCamera.CameraUpVector.X,
                                Y = bcf1Issue.viewpoint.OrthogonalCamera.CameraUpVector.Y,
                                Z = bcf1Issue.viewpoint.OrthogonalCamera.CameraUpVector.Z
                            },
                            CameraViewPoint = new BCF2.Point()
                            {
                                X = bcf1Issue.viewpoint.OrthogonalCamera.CameraViewPoint.X,
                                Y = bcf1Issue.viewpoint.OrthogonalCamera.CameraViewPoint.Y,
                                Z = bcf1Issue.viewpoint.OrthogonalCamera.CameraViewPoint.Z
                            },
                            ViewToWorldScale = bcf1Issue.viewpoint.OrthogonalCamera.ViewToWorldScale
                        },
                        PerspectiveCamera = bcf1Issue.viewpoint.PerspectiveCamera == null ? null : new BCF2.PerspectiveCamera()
                        {
                            CameraDirection = new BCF2.Direction()
                            {
                                X = bcf1Issue.viewpoint.PerspectiveCamera.CameraDirection.X,
                                Y = bcf1Issue.viewpoint.PerspectiveCamera.CameraDirection.Y,
                                Z = bcf1Issue.viewpoint.PerspectiveCamera.CameraDirection.Z
                            },
                            CameraUpVector = new BCF2.Direction()
                            {
                                X = bcf1Issue.viewpoint.PerspectiveCamera.CameraUpVector.X,
                                Y = bcf1Issue.viewpoint.PerspectiveCamera.CameraUpVector.Y,
                                Z = bcf1Issue.viewpoint.PerspectiveCamera.CameraUpVector.Z
                            },
                            CameraViewPoint = new BCF2.Point()
                            {
                                X = bcf1Issue.viewpoint.PerspectiveCamera.CameraViewPoint.X,
                                Y = bcf1Issue.viewpoint.PerspectiveCamera.CameraViewPoint.Y,
                                Z = bcf1Issue.viewpoint.PerspectiveCamera.CameraViewPoint.Z
                            },
                            FieldOfView = bcf1Issue.viewpoint.PerspectiveCamera.FieldOfView
                        },
                        SheetCamera = bcf1Issue.viewpoint.SheetCamera == null ? null : new BCF2.SheetCamera()
                        {
                            SheetID = bcf1Issue.viewpoint.SheetCamera.SheetID,
                            SheetName = null, // default null
                            TopLeft = new BCF2.Point()
                            {
                                X = bcf1Issue.viewpoint.SheetCamera.TopLeft.X,
                                Y = bcf1Issue.viewpoint.SheetCamera.TopLeft.Y,
                                Z = bcf1Issue.viewpoint.SheetCamera.TopLeft.Z
                            },
                            BottomRight = new BCF2.Point()
                            {
                                X = bcf1Issue.viewpoint.SheetCamera.BottomRight.X,
                                Y = bcf1Issue.viewpoint.SheetCamera.BottomRight.Y,
                                Z = bcf1Issue.viewpoint.SheetCamera.BottomRight.Z
                            }
                        }
                    };

                    // Convert viewpoints
                    // BCF 1.0 can only have one viewpoint
                    ObservableCollection<BCF2.ViewPoint> bcf2ViewPoints = new ObservableCollection<BCF2.ViewPoint>();
                    bcf2ViewPoints.Add(new BCF2.ViewPoint()
                    {
                        //Guid = null,    // no guid for viewpoint
                        Snapshot = bcf1Issue.snapshot,
                        Viewpoint = "viewpoint.bcfv",
                        VisInfo = bcf2VizInfo
                    });

                    // Add BCF 2.0 issues/markups
                    bcf2.Issues.Add(new BCF2.Markup()
                    {
                        Header = bcf2Headers,
                        Comment = bcf2Comments,
                        Topic = bcf2Topic,
                        Viewpoints = bcf2ViewPoints
                    });
                }

                // Save BCF 2.0 file
                BCF2.BcfContainer.SaveBcfFile(bcf2);
                bcf1.HasBeenSaved = true;
            }
            catch (Exception ex)
            {
                // Get stack trace for the exception with source file information
                var st = new System.Diagnostics.StackTrace(ex, true);
                // Get the top stack frame
                var frame = st.GetFrame(0);
                // Get the line number from the stack frame
                var line = frame.GetFileLineNumber();
                System.Windows.MessageBox.Show("Exception:" + line + "=====" + ex.ToString());
            }            
            
        }

        /// <summary>
        /// Save Jira issues as BCF 2.0
        /// </summary>
        /// <param name="jiraPan"></param>
        public static void SaveBcf2FromJira(UserControls.MainPanel mainPan)
        {
            try
            {
                BCF2.BcfFile bcf2 = new BCF2.BcfFile();
                string ReportFolder = Path.Combine(Path.GetTempPath(), "BCFtemp", Path.GetRandomFileName());
                bcf2.TempPath = ReportFolder;

                bcf2.ProjectName = ((Project)(mainPan.jiraPan.projCombo.SelectedItem)).name;
                //bcf2.ProjectId = ;      // Is there a guid for a Jira project?

                int errors = 0;

                // Add issues (markups)
                foreach (object t in mainPan.jiraPan.issueList.SelectedItems)
                {                    
                    int index = mainPan.jiraPan.issueList.Items.IndexOf(t);
                    Issue issue = mainPan.jira.IssuesCollection[index];
                    if (issue.viewpoint == "" || issue.snapshotFull == "")
                    {
                        errors++;
                        continue;
                    }

                    // Create temp. folder
                    string issueGuid = issue.fields.guid;
                    if (!Directory.Exists(Path.Combine(ReportFolder, issueGuid)))
                        Directory.CreateDirectory(Path.Combine(ReportFolder, issueGuid));

                    // Convert header files
                    List<BCF2.HeaderFile> bcf2Headers = new List<BCF2.HeaderFile>();
                    bcf2Headers.Add(new BCF2.HeaderFile()
                    {
                        Date = issue.fields.created == null ? new DateTime() : DateTime.Parse(issue.fields.created),
                        Filename = "Jira Export " + DateTime.Now.ToShortDateString().Replace("/", "-"),
                        isExternal = true, // default true for now
                        Reference = "" // default empty for now
                    });

                    // Convert Comments
                    ObservableCollection<BCF2.Comment> bcf2Comments = new ObservableCollection<BCF2.Comment>();
                    foreach (var comm in issue.fields.comment.comments)
                    {
                        if (comm != null)
                        {
                            bcf2Comments.Add(new BCF2.Comment()
                            {
                                Author = comm.author == null ? null : comm.author.displayName,
                                Comment1 = comm.body == null ? null : comm.body,
                                Date = comm.created == null ? new DateTime() : DateTime.Parse(comm.created),
                                Guid = Guid.NewGuid().ToString(),
                                ModifiedAuthor = comm.updateAuthor == null ? null : comm.updateAuthor.displayName,
                                ModifiedDate = comm.updated == null ? new DateTime() : DateTime.Parse(comm.updated),
                                ReplyToComment = null, // default null
                                Status = "Unknown",
                                Topic = new BCF2.CommentTopic() { Guid = issueGuid }, // all referenced to markup's topic
                                VerbalStatus = issue.fields.status == null ? null : issue.fields.status.name,
                                Viewpoint = null
                            });
                        }
                    }

                    // Convert Topic
                    BCF2.Topic bcf2Topic = new BCF2.Topic()
                    {
                        AssignedTo = issue.fields.assignee == null ? null : issue.fields.assignee.displayName,
                        BimSnippet = null,
                        CreationAuthor = issue.fields.creator == null ? null : issue.fields.creator.displayName,
                        CreationDate = issue.fields.created == null ? new DateTime() : DateTime.Parse(issue.fields.created),
                        Description = issue.fields.description == null ? null : issue.fields.description,
                        DocumentReferences = null,
                        Guid = issueGuid,
                        Index = null,
                        Labels = null,
                        ModifiedAuthor = null,
                        ModifiedDate = issue.fields.updated == null ? new DateTime() : DateTime.Parse(issue.fields.updated),
                        Priority = issue.fields.priority == null ? null : issue.fields.priority.name,
                        ReferenceLink = null, 
                        RelatedTopics = null,
                        Title = issue.fields.summary == null ? null : issue.fields.summary,
                        TopicStatus = issue.fields.status == null ? null : issue.fields.status.name,
                        TopicType = issue.fields.issuetype == null ? null : issue.fields.issuetype.name
                    };

                    // Add BCF 2.0 issues/markups
                    bcf2.Issues.Add(new BCF2.Markup()
                    {
                        Header = bcf2Headers,
                        Comment = bcf2Comments,
                        Topic = bcf2Topic,
                        // Viewpoints = bcf2ViewPoints    // use the one saved on Jira
                    });

                    // Save viewpoint and snapshot
                    try
                    {
                        mainPan.saveSnapshotViewpoint(issue.viewpoint, Path.Combine(ReportFolder, issueGuid, "viewpoint.bcfv"));
                        mainPan.saveSnapshotViewpoint(issue.snapshotFull, Path.Combine(ReportFolder, issueGuid, "snapshot.png"));
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Failed to download viewpoint.bcfv and snapshot.png on Jira",
                            "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }                    
                }

                if (errors != 0)
                {
                    MessageBox.Show(errors + " Issue/s were not exported because not formatted correctly.",
                        "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);

                    if (errors == mainPan.jiraPan.issueList.SelectedItems.Count)
                    {
                        mainPan.DeleteDirectory(ReportFolder);
                        return;
                    }
                }

                // Save BCF 2.0 file
                BCF2.BcfContainer.SaveBcfFile(bcf2);
            }
            catch (Exception ex)
            {
                // Get stack trace for the exception with source file information
                var st = new System.Diagnostics.StackTrace(ex, true);
                // Get the top stack frame
                var frame = st.GetFrame(0);
                // Get the line number from the stack frame
                var line = frame.GetFileLineNumber();
                MessageBox.Show("Exception:" + line + "=====" + ex.ToString());
            }
        }

        /// <summary>
        /// Load from BCF 2.0 as BCF 1.0
        /// </summary>
        /// <param name="bcf2">BCF 2.0 file</param>
        /// <returns></returns>
        public static IssueBCF LoadBcf1IssueFromBcf2(BCF2.Markup bcf2Markup, BCF2.VisualizationInfo bcf2Viewpoint)
        {
            // Convert headers            
            List<HeaderFile> bcf1Headers = new List<HeaderFile>();            
            foreach (BCF2.HeaderFile bcf2Header in bcf2Markup.Header)    
            {
                HeaderFile bcf1Header = new HeaderFile()
                {
                    Filename = bcf2Header.Filename,
                    Date = bcf2Header.Date,
                    IfcProject = bcf2Header.IfcProject,
                    IfcSpatialStructureElement = bcf2Header.IfcSpatialStructureElement
                };
                bcf1Headers.Add(bcf1Header);
            }

            // Convert topic
            Topic bcf1Topic = new Topic();
            if (bcf2Markup.Topic != null)
            {
                bcf1Topic.Guid = bcf2Markup.Topic.Guid;
                bcf1Topic.ReferenceLink = bcf2Markup.Topic.ReferenceLink; 
                bcf1Topic.Title = bcf2Markup.Topic.Title;
            };

            // Convert comments
            ObservableCollection<CommentBCF> bcf1Comments = new ObservableCollection<CommentBCF>();
            foreach(BCF2.Comment bcf2Comment in bcf2Markup.Comment)
            {
                CommentBCF bcf1Comment = new CommentBCF()
                {
                    Author = bcf2Comment.Author, 
                    Comment1 = bcf2Comment.Comment1, 
                    Date = bcf2Comment.Date, 
                    Guid = bcf2Comment.Guid, 
                    Status = CommentStatus.Unknown,    // default unknown for now
                    Topic = new CommentTopic() { Guid = bcf2Markup.Topic == null ? Guid.NewGuid().ToString() : bcf2Markup.Topic.Guid }, 
                    VerbalStatus = bcf2Comment.VerbalStatus
                };
                bcf1Comments.Add(bcf1Comment);
            }

            // Convert markups/issues
            Markup bcf1Markup = new Markup()
            {
                Header = bcf1Headers.ToArray(),
                Topic = bcf1Topic,
                Comment = bcf1Comments
            };

            // Convert ClippingPlane
            List<ClippingPlane> bcf1ClippingPlanes = new List<ClippingPlane>();
            if (bcf2Viewpoint.ClippingPlanes != null)
            {                        
                foreach (BCF2.ClippingPlane bcf2ClippingPlane in bcf2Viewpoint.ClippingPlanes)
                {
                    if (bcf2ClippingPlane != null)
                    {
                        bcf1ClippingPlanes.Add(new ClippingPlane()
                        {
                            Direction = new Direction()
                            {
                                X = bcf2ClippingPlane.Direction.X,
                                Y = bcf2ClippingPlane.Direction.Y,
                                Z = bcf2ClippingPlane.Direction.Z
                            },
                            Location = new Point()
                            {
                                X = bcf2ClippingPlane.Location.X,
                                Y = bcf2ClippingPlane.Location.Y,
                                Z = bcf2ClippingPlane.Location.Z
                            }
                        });
                    }
                }
            }

            // Convert Components
            List<Component> bcf1Components = new List<Component>();
            if (bcf2Viewpoint.Components != null)
            {                        
                foreach (BCF2.Component bcf2Component in bcf2Viewpoint.Components)
                {
                    if (bcf2Component != null)
                    {
                        bcf1Components.Add(new Component()
                        {
                            AuthoringToolId = bcf2Component.AuthoringToolId,
                            IfcGuid = bcf2Component.IfcGuid,
                            OriginatingSystem = bcf2Component.OriginatingSystem
                        });
                    }
                }
            }

            // Convert Lines
            List<Line> bcf1Lines = new List<Line>();
            if (bcf2Viewpoint.Lines != null)
            {                        
                foreach (BCF2.Line bcf2Line in bcf2Viewpoint.Lines)
                {
                    if (bcf2Line != null)
                    {
                        bcf1Lines.Add(new Line()
                        {
                            StartPoint = new Point()
                            {
                                X = bcf2Line.StartPoint.X,
                                Y = bcf2Line.StartPoint.Y,
                                Z = bcf2Line.StartPoint.Z
                            },
                            EndPoint = new Point()
                            {
                                X = bcf2Line.EndPoint.X,
                                Y = bcf2Line.EndPoint.Y,
                                Z = bcf2Line.EndPoint.Z
                            }
                        });
                    }
                }
            }

            // Convert viewpoints
            VisualizationInfo bcf1Viewpoint = new VisualizationInfo()
            {
                ClippingPlanes = bcf1ClippingPlanes.ToArray(), 
                Components = bcf1Components.ToArray(), 
                Lines = bcf1Lines.ToArray(),
                OrthogonalCamera = bcf2Viewpoint.OrthogonalCamera == null ? null : new OrthogonalCamera()
                {
                    CameraDirection = new Direction()
                    {
                        X = bcf2Viewpoint.OrthogonalCamera.CameraDirection.X,
                        Y = bcf2Viewpoint.OrthogonalCamera.CameraDirection.Y,
                        Z = bcf2Viewpoint.OrthogonalCamera.CameraDirection.Z
                    },
                    CameraUpVector = new Direction()
                    {
                        X = bcf2Viewpoint.OrthogonalCamera.CameraUpVector.X,
                        Y = bcf2Viewpoint.OrthogonalCamera.CameraUpVector.Y,
                        Z = bcf2Viewpoint.OrthogonalCamera.CameraUpVector.Z
                    },
                    CameraViewPoint = new Point()
                    {
                        X = bcf2Viewpoint.OrthogonalCamera.CameraViewPoint.X,
                        Y = bcf2Viewpoint.OrthogonalCamera.CameraViewPoint.Y,
                        Z = bcf2Viewpoint.OrthogonalCamera.CameraViewPoint.Z
                    },
                    ViewToWorldScale = bcf2Viewpoint.OrthogonalCamera.ViewToWorldScale
                },
                PerspectiveCamera = bcf2Viewpoint.PerspectiveCamera == null ? null : new PerspectiveCamera()
                {
                    CameraDirection = new Direction()
                    {
                        X = bcf2Viewpoint.PerspectiveCamera.CameraDirection.X,
                        Y = bcf2Viewpoint.PerspectiveCamera.CameraDirection.Y,
                        Z = bcf2Viewpoint.PerspectiveCamera.CameraDirection.Z
                    },
                    CameraUpVector = new Direction()
                    {
                        X = bcf2Viewpoint.PerspectiveCamera.CameraUpVector.X,
                        Y = bcf2Viewpoint.PerspectiveCamera.CameraUpVector.Y,
                        Z = bcf2Viewpoint.PerspectiveCamera.CameraUpVector.Z
                    },
                    CameraViewPoint = new Point()
                    {
                        X = bcf2Viewpoint.PerspectiveCamera.CameraViewPoint.X,
                        Y = bcf2Viewpoint.PerspectiveCamera.CameraViewPoint.Y,
                        Z = bcf2Viewpoint.PerspectiveCamera.CameraViewPoint.Z
                    },
                    FieldOfView = bcf2Viewpoint.PerspectiveCamera.FieldOfView
                },
                SheetCamera = bcf2Viewpoint.SheetCamera == null ? null : new SheetCamera()
                {
                    SheetID = bcf2Viewpoint.SheetCamera.SheetID,
                    TopLeft = new Point()
                    {
                        X = bcf2Viewpoint.SheetCamera.TopLeft.X,
                        Y = bcf2Viewpoint.SheetCamera.TopLeft.Y,
                        Z = bcf2Viewpoint.SheetCamera.TopLeft.Z
                    },
                    BottomRight = new Point()
                    {
                        X = bcf2Viewpoint.SheetCamera.BottomRight.X,
                        Y = bcf2Viewpoint.SheetCamera.BottomRight.Y,
                        Z = bcf2Viewpoint.SheetCamera.BottomRight.Z
                    }
                }
            };

            // Create a new BCF 1.0 issue
            IssueBCF bcf1 = new IssueBCF()
            {
                markup = bcf1Markup,
                viewpoint = bcf1Viewpoint
            };

            return bcf1;
        }
    }
}
