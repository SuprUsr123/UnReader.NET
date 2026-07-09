Imports Gtk

Public Class NewPostDialog
    Inherits Dialog

    Private titleEntry As Entry
    Private contentView As TextView

    Public ReadOnly Property PostTitle As String
        Get
            Return titleEntry.Text.Trim()
        End Get
    End Property

    Public ReadOnly Property PostContent As String
        Get
            Return contentView.Buffer.Text.Trim()
        End Get
    End Property

    Public Sub New()
        MyBase.New("Create Thread", Nothing, DialogFlags.Modal)
        SetDefaultSize(520, 340)

        AddButton("Cancel", ResponseType.Cancel)
        AddButton("Post", ResponseType.Ok)

        Dim contentArea As VBox = CType(ContentArea, VBox)
        contentArea.Spacing = 10
        contentArea.BorderWidth = 12

        contentArea.PackStart(New Label("Title:"), False, False, 0)
        titleEntry = New Entry()
        contentArea.PackStart(titleEntry, False, False, 0)

        contentArea.PackStart(New Label("Content:"), False, False, 0)
        contentView = New TextView() With {.WrapMode = WrapMode.Word}
        Dim scroll = New ScrolledWindow()
        scroll.Add(contentView)
        scroll.SetSizeRequest(0, 180)
        contentArea.PackStart(scroll, True, True, 0)

        ShowAll()
    End Sub

    Protected Overrides Sub OnResponse(response_id As ResponseType)
        If response_id = ResponseType.Ok Then
            If String.IsNullOrWhiteSpace(PostTitle) OrElse String.IsNullOrWhiteSpace(PostContent) Then
                Using dlg As New MessageDialog(Me, DialogFlags.Modal, MessageType.Warning, ButtonsType.Ok, "Title and content are required.")
                    dlg.Text = "Validation"
                    dlg.Run()
                End Using
                response_id = ResponseType.None
            End If
        End If
        MyBase.OnResponse(response_id)
    End Sub
End Class
