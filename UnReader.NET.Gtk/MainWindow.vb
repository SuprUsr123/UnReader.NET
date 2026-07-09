Imports Gtk
Imports System
Imports System.Diagnostics
Imports System.Text
Imports System.Threading.Tasks
Imports UnReader.NET.Core

Public Class MainWindow
    Inherits Window

    Private ReadOnly reader As ServerReader
    Private currentContext As String = "public"
    Private currentTarget As String = String.Empty
    Private currentHistoryIndex As Integer = 0
    Private activeNeighborhoodPostId As Integer = -1
    Private isRefreshingFeed As Boolean = False
    Private pendingRefresh As Boolean = False

    Private lblPageIndex As Label
    Private statusLabel As Label

    Private globalChatView As TextView
    Private globalUsersList As ListBox
    Private globalInput As Entry
    Private globalSendButton As Button

    Private dmContactsList As ListBox
    Private dmChatView As TextView
    Private dmTargetInput As Entry
    Private dmAddButton As Button
    Private dmInput As Entry
    Private dmSendButton As Button

    Private topicList As ListBox
    Private topicChatView As TextView
    Private topicCreateInput As Entry
    Private topicCreateButton As Button
    Private topicInput As Entry
    Private topicSendButton As Button

    Private nbPostsList As ListBox
    Private nbPostView As TextView
    Private nbCommentsView As TextView
    Private nbCommentInput As Entry
    Private nbCreatePostButton As Button
    Private nbSubmitCommentButton As Button

    Private modSearchEntry As Entry
    Private modSearchButton As Button
    Private modUserStatusView As TextView
    Private modReasonInput As Entry
    Private modLogsView As TextView
    Private modRefreshLogsButton As Button
    Private modBanButton As Button
    Private modPardonButton As Button
    Private modSetRoleButton As Button
    Private modBanIPButton As Button
    Private modKickButton As Button
    Private modTimeoutAdjustment As Adjustment
    Private modTimeoutButton As Button

    Private openBrowserButton As Button
    Private notebookControl As Notebook

    Public Sub New(reader As ServerReader)
        MyBase.New("UnReader.NET")
        Me.reader = reader

        SetDefaultSize(1000, 600)
        Resizable = False
        BorderWidth = 10
        WindowPosition = WindowPosition.Center

        AddHandler DeleteEvent, AddressOf OnWindowClosed
        WindowPosition = WindowPosition.Center

        Dim rootVBox = New VBox(False, 8)
        Add(rootVBox)

        Dim menuBar As New MenuBar()
        Dim fileMenuItem As New MenuItem("File")
        Dim fileMenu As New Menu()
        Dim aboutMenuItem As New MenuItem("About")
        Dim exitMenuItem As New MenuItem("Exit")

        AddHandler aboutMenuItem.Activated, AddressOf OnAboutClicked
        AddHandler exitMenuItem.Activated, AddressOf OnExitClicked

        fileMenu.Append(aboutMenuItem)
        fileMenu.Append(exitMenuItem)
        fileMenuItem.Submenu = fileMenu
        menuBar.Append(fileMenuItem)
        rootVBox.PackStart(menuBar, False, False, 0)


        Try
            Dim geom As New Gdk.Geometry()
            geom.MinWidth = 1000
            geom.MinHeight = 600
            geom.MaxWidth = 1000
            geom.MaxHeight = 600
            Me.SetGeometryHints(Nothing, geom, Gdk.WindowHints.MaxSize Or Gdk.WindowHints.MinSize)
        Catch
        End Try

        Dim toolbar = New HBox(False, 6)
        lblPageIndex = New Label("PAGE: 0") With {.Xalign = 0}
        Dim newerButton = New Button("< NEWER")
        Dim olderButton = New Button("OLDER >")
        AddHandler newerButton.Clicked, AddressOf OnPageNewerClicked
        AddHandler olderButton.Clicked, AddressOf OnPageOlderClicked
        Dim aboutButton = New Button("About")
        AddHandler aboutButton.Clicked, AddressOf OnAboutClicked
        toolbar.PackStart(lblPageIndex, True, True, 0)
        toolbar.PackStart(newerButton, False, False, 0)
        toolbar.PackStart(olderButton, False, False, 0)
        toolbar.PackStart(aboutButton, False, False, 0)
        rootVBox.PackStart(toolbar, False, False, 0)

        statusLabel = New Label("Ready") With {.Xalign = 0}
        rootVBox.PackStart(statusLabel, False, False, 0)

        notebookControl = New Notebook()
        notebookControl.SetSizeRequest(980, 500)
        rootVBox.PackStart(notebookControl, True, True, 0)

        notebookControl.AppendPage(CreateGlobalChatPage(), New Label("GLOBAL CHAT"))
        notebookControl.AppendPage(CreateDmPage(), New Label("DIRECT MESSAGES"))
        notebookControl.AppendPage(CreateTopicPage(), New Label("TOPICS"))
        notebookControl.AppendPage(CreateNeighborhoodPage(), New Label("NEIGHBORHOOD"))
        notebookControl.AppendPage(CreateModerationPage(), New Label("MODERATION"))
        notebookControl.AppendPage(CreateMainPage(), New Label("MAIN PAGE"))

        AddHandler notebookControl.SwitchPage, AddressOf OnNotebookSwitched

        ShowAll()
    End Sub

    Public Async Function InitializeAsync() As Task
        AddHandler reader.OnRosterUpdated, AddressOf HandleRosterUpdate
        AddHandler reader.OnDisconnected, AddressOf HandleDisconnect
        AddHandler reader.OnErrorAlert, AddressOf HandleErrorAlert
        AddHandler reader.OnFeedRefreshed, AddressOf HandleFeedRefresh
        AddHandler reader.OnTopicsUpdated, AddressOf HandleTopicsUpdated

        statusLabel.Text = "Connecting to realtime server..."
        Dim connected = Await reader.ConnectRealtimeAsync()
        If Not connected Then
            statusLabel.Text = "Realtime connection failed."
            Throw New Exception("Failed to connect to realtime server.")
        End If

        statusLabel.Text = "Connected. Loading data..."
        Await LoadInitialDataAsync()
        statusLabel.Text = "Ready"
    End Function

    Private Function CreateGlobalChatPage() As Widget
        Dim grid = New Grid() With {
            .RowSpacing = 8,
            .ColumnSpacing = 8,
            .ColumnHomogeneous = False
        }

        globalUsersList = New ListBox()
        globalUsersList.SetSizeRequest(240, 300)
        grid.Attach(globalUsersList, 1, 0, 1, 3)

        globalChatView = New TextView() With {
            .Editable = False,
            .WrapMode = WrapMode.Word
        }
        Dim chatScroll = New ScrolledWindow()
        chatScroll.Add(globalChatView)
        chatScroll.SetSizeRequest(640, 300)
        grid.Attach(chatScroll, 0, 0, 1, 2)

        globalInput = New Entry()
        globalSendButton = New Button("SEND")
        AddHandler globalSendButton.Clicked, AddressOf OnGlobalSendClicked
        AddHandler globalInput.Activated, AddressOf OnGlobalSendClicked

        Dim inputBox = New HBox(False, 6)
        inputBox.PackStart(globalInput, True, True, 0)
        inputBox.PackStart(globalSendButton, False, False, 0)
        grid.Attach(inputBox, 0, 2, 1, 1)

        Return grid
    End Function

    Private Function CreateDmPage() As Widget
        Dim hbox = New HBox(False, 10)

        Dim leftBox = New VBox(False, 8)
        leftBox.SetSizeRequest(240, 0)
        leftBox.PackStart(New Label("CONTACTS:"), False, False, 0)
        dmContactsList = New ListBox()
        dmContactsList.SetSizeRequest(240, 300)
        AddHandler dmContactsList.RowSelected, AddressOf OnDmContactSelected
        leftBox.PackStart(dmContactsList, True, True, 0)

        dmTargetInput = New Entry()
        dmTargetInput.PlaceholderText = "Username..."
        dmAddButton = New Button("ADD")
        AddHandler dmAddButton.Clicked, AddressOf OnDmAddClicked
        Dim addBox = New HBox(False, 6)
        addBox.PackStart(dmTargetInput, True, True, 0)
        addBox.PackStart(dmAddButton, False, False, 0)
        leftBox.PackStart(addBox, False, False, 0)

        hbox.PackStart(leftBox, False, False, 0)

        Dim rightBox = New VBox(False, 8)
        dmChatView = New TextView() With {.Editable = False, .WrapMode = WrapMode.Word}
        Dim dmScroll = New ScrolledWindow()
        dmScroll.Add(dmChatView)
        dmScroll.SetSizeRequest(640, 300)
        rightBox.PackStart(dmScroll, True, True, 0)

        dmInput = New Entry()
        dmSendButton = New Button("SEND")
        AddHandler dmSendButton.Clicked, AddressOf OnDmSendClicked
        AddHandler dmInput.Activated, AddressOf OnDmSendClicked
        Dim dmInputBox = New HBox(False, 6)
        dmInputBox.PackStart(dmInput, True, True, 0)
        dmInputBox.PackStart(dmSendButton, False, False, 0)
        rightBox.PackStart(dmInputBox, False, False, 0)

        hbox.PackStart(rightBox, True, True, 0)
        Return hbox
    End Function

    Private Function CreateTopicPage() As Widget
        Dim hbox = New HBox(False, 10)

        Dim leftBox = New VBox(False, 8)
        leftBox.SetSizeRequest(240, 0)
        leftBox.PackStart(New Label("TOPICS:"), False, False, 0)
        topicList = New ListBox()
        topicList.SetSizeRequest(240, 300)
        AddHandler topicList.RowSelected, AddressOf OnTopicSelected
        Dim topicListScroll As New ScrolledWindow()
        topicListScroll.Add(topicList)
        topicListScroll.SetSizeRequest(240, 300)
        leftBox.PackStart(topicListScroll, False, False, 0)

        topicCreateInput = New Entry()
        topicCreateInput.PlaceholderText = "New topic slug..."
        topicCreateButton = New Button("CREATE")
        AddHandler topicCreateButton.Clicked, AddressOf OnTopicCreateClicked
        Dim createBox = New HBox(False, 6)
        createBox.PackStart(topicCreateInput, True, True, 0)
        createBox.PackStart(topicCreateButton, False, False, 0)
        leftBox.PackStart(createBox, False, False, 0)

        hbox.PackStart(leftBox, False, False, 0)

        Dim rightBox = New VBox(False, 8)
        topicChatView = New TextView() With {.Editable = False, .WrapMode = WrapMode.Word}
        Dim topicScroll = New ScrolledWindow()
        topicScroll.Add(topicChatView)
        topicScroll.SetSizeRequest(640, 300)
        rightBox.PackStart(topicScroll, True, True, 0)

        topicInput = New Entry()
        topicSendButton = New Button("SEND")
        AddHandler topicSendButton.Clicked, AddressOf OnTopicSendClicked
        AddHandler topicInput.Activated, AddressOf OnTopicSendClicked
        Dim sendBox = New HBox(False, 6)
        sendBox.PackStart(topicInput, True, True, 0)
        sendBox.PackStart(topicSendButton, False, False, 0)
        rightBox.PackStart(sendBox, False, False, 0)

        hbox.PackStart(rightBox, True, True, 0)
        Return hbox
    End Function

    Private Function CreateNeighborhoodPage() As Widget
        Dim hbox = New HBox(False, 10)

        Dim leftBox = New VBox(False, 8)
        leftBox.SetSizeRequest(260, 0)
        leftBox.PackStart(New Label("NEIGHBORHOOD POSTS:"), False, False, 0)
        nbPostsList = New ListBox()
        nbPostsList.SetSizeRequest(260, 300)
        AddHandler nbPostsList.RowSelected, AddressOf OnNeighborhoodPostSelected
        leftBox.PackStart(nbPostsList, True, True, 0)

        nbCreatePostButton = New Button("CREATE POST")
        AddHandler nbCreatePostButton.Clicked, AddressOf OnCreatePostClicked
        leftBox.PackStart(nbCreatePostButton, False, False, 0)
        hbox.PackStart(leftBox, False, False, 0)

        Dim rightBox = New VBox(False, 8)
        nbPostView = New TextView() With {.Editable = False, .WrapMode = WrapMode.Word}
        Dim postScroll = New ScrolledWindow()
        postScroll.Add(nbPostView)
        postScroll.SetSizeRequest(0, 200)
        rightBox.PackStart(postScroll, True, True, 0)

        nbCommentsView = New TextView() With {.Editable = False, .WrapMode = WrapMode.Word}
        Dim commentScroll = New ScrolledWindow()
        commentScroll.Add(nbCommentsView)
        commentScroll.SetSizeRequest(0, 150)
        rightBox.PackStart(commentScroll, True, True, 0)

        nbCommentInput = New Entry() With {.PlaceholderText = "Write a comment..."}
        nbSubmitCommentButton = New Button("SUBMIT COMMENT")
        AddHandler nbSubmitCommentButton.Clicked, AddressOf OnSubmitCommentClicked
        Dim commentBox = New HBox(False, 6)
        commentBox.PackStart(nbCommentInput, True, True, 0)
        commentBox.PackStart(nbSubmitCommentButton, False, False, 0)
        rightBox.PackStart(commentBox, False, False, 0)

        hbox.PackStart(rightBox, True, True, 0)
        Return hbox
    End Function

    Private Function CreateModerationPage() As Widget
        Dim vbox = New VBox(False, 8)

        Dim searchBox = New HBox(False, 6)
        modSearchEntry = New Entry() With {.PlaceholderText = "Username..."}
        modSearchButton = New Button("SEARCH")
        AddHandler modSearchButton.Clicked, AddressOf OnModSearchClicked
        searchBox.PackStart(New Label("MOD SEARCH:"), False, False, 0)
        searchBox.PackStart(modSearchEntry, True, True, 0)
        searchBox.PackStart(modSearchButton, False, False, 0)
        vbox.PackStart(searchBox, False, False, 0)

        modUserStatusView = New TextView() With {.Editable = False, .WrapMode = WrapMode.Word}
        Dim userStatusScroll = New ScrolledWindow()
        userStatusScroll.Add(modUserStatusView)
        userStatusScroll.SetSizeRequest(0, 100)
        vbox.PackStart(userStatusScroll, False, False, 0)

        modReasonInput = New Entry() With {.PlaceholderText = "Reason..."}
        modRefreshLogsButton = New Button("REFRESH LOGS")
        AddHandler modRefreshLogsButton.Clicked, AddressOf OnModRefreshLogsClicked
        Dim actionsBox = New HBox(False, 6)
        actionsBox.PackStart(modReasonInput, True, True, 0)
        actionsBox.PackStart(modRefreshLogsButton, False, False, 0)
        vbox.PackStart(actionsBox, False, False, 0)

        modLogsView = New TextView() With {.Editable = False, .WrapMode = WrapMode.Word}
        Dim logsScroll = New ScrolledWindow()
        logsScroll.Add(modLogsView)
        logsScroll.SetSizeRequest(0, 200)
        vbox.PackStart(logsScroll, True, True, 0)

        Dim buttonGrid = New Grid() With {.RowSpacing = 8, .ColumnSpacing = 8}
        modBanButton = New Button("BAN")
        modPardonButton = New Button("PARDON")
        modSetRoleButton = New Button("SET ROLE")
        modBanIPButton = New Button("BAN IP")
        modKickButton = New Button("KICK")
        modTimeoutAdjustment = New Adjustment(1, 1, 1440, 1, 10, 0)
        Dim timeoutSpin = New SpinButton(modTimeoutAdjustment, 1, 0)
        modTimeoutButton = New Button("TIMEOUT")

        AddHandler modBanButton.Clicked, AddressOf OnModBanClicked
        AddHandler modPardonButton.Clicked, AddressOf OnModPardonClicked
        AddHandler modSetRoleButton.Clicked, AddressOf OnModSetRoleClicked
        AddHandler modBanIPButton.Clicked, AddressOf OnModBanIPClicked
        AddHandler modKickButton.Clicked, AddressOf OnModKickClicked
        AddHandler modTimeoutButton.Clicked, AddressOf OnModTimeoutClicked

        buttonGrid.Attach(modBanButton, 0, 0, 1, 1)
        buttonGrid.Attach(modPardonButton, 1, 0, 1, 1)
        buttonGrid.Attach(modSetRoleButton, 2, 0, 1, 1)
        buttonGrid.Attach(modBanIPButton, 0, 1, 1, 1)
        buttonGrid.Attach(modKickButton, 1, 1, 1, 1)
        buttonGrid.Attach(timeoutSpin, 2, 1, 1, 1)
        buttonGrid.Attach(modTimeoutButton, 3, 1, 1, 1)

        vbox.PackStart(buttonGrid, False, False, 0)
        Return vbox
    End Function

    Private Function CreateMainPage() As Widget
        Dim vbox = New VBox(False, 10)
        vbox.PackStart(New Label("This page shows your server landing page. GTK# cannot embed WebView2 on Linux."), False, False, 0)
        openBrowserButton = New Button("Open in Browser")
        AddHandler openBrowserButton.Clicked, AddressOf OnOpenBrowserClicked
        vbox.PackStart(openBrowserButton, False, False, 0)
        Return vbox
    End Function

    Private Async Function LoadInitialDataAsync() As Task
        Try
            Await RefreshActiveFeedAsync()
            Await FetchDmsAndTopicsListsAsync()
        Catch ex As Exception
            statusLabel.Text = $"Initialization failed: {ex.Message}"
        End Try
    End Function

    Private Async Function RefreshActiveFeedAsync() As Task
        If isRefreshingFeed Then
            pendingRefresh = True
            Return
        End If

        isRefreshingFeed = True
        Try
            Do
                pendingRefresh = False
                InvokeOnMainThread(Sub() lblPageIndex.Text = $"PAGE: {currentHistoryIndex}")

                Select Case currentContext
                    Case "public"
                        Dim msgs = Await reader.GetHistoryAsync(currentHistoryIndex)
                        UpdateChatView(globalChatView, msgs)
                    Case "dm"
                        If Not String.IsNullOrEmpty(currentTarget) Then
                            Dim msgs = Await reader.GetDMHistoryAsync(currentTarget, currentHistoryIndex)
                            UpdateChatView(dmChatView, msgs)
                        Else
                            InvokeOnMainThread(Sub() dmChatView.Buffer.Text = String.Empty)
                        End If
                    Case "topic"
                        If Not String.IsNullOrEmpty(currentTarget) Then
                            Dim msgs = Await reader.GetTopicHistoryAsync(currentTarget, currentHistoryIndex)
                            UpdateChatView(topicChatView, msgs)
                        Else
                            InvokeOnMainThread(Sub() topicChatView.Buffer.Text = String.Empty)
                        End If
                    Case "neighborhood"
                        Await RefreshNeighborhoodAsync()
                End Select
            Loop While pendingRefresh
        Catch ex As Exception
            statusLabel.Text = $"Refresh error: {ex.Message}"
        Finally
            isRefreshingFeed = False
        End Try
    End Function

    Private Sub UpdateChatView(view As TextView, messages As IEnumerable(Of ChatMessage))
        Dim sb As New StringBuilder()
        For Each msg In messages
            sb.AppendLine(msg.FormattedText)
        Next
        InvokeOnMainThread(Sub()
                               view.Buffer.Text = sb.ToString()
                               view.ScrollToIter(view.Buffer.EndIter, 0, False, 0, 0)
                           End Sub)
    End Sub

    Private Function CreateLabel(text As String) As Label
        Return New Label(text) With {.Xalign = 0, .Ellipsize = Pango.EllipsizeMode.End}
    End Function

    Private Sub ClearListBox(listBox As ListBox)
        For Each row As Widget In listBox.AllChildren
            listBox.Remove(row)
        Next
    End Sub

    Private Sub UpdateListBox(listBox As ListBox, items As IEnumerable(Of String))
        InvokeOnMainThread(Sub()
                               ClearListBox(listBox)
                               For Each labelText In items
                                   Dim lbl As New Label()
                                   lbl.Text = labelText
                                   lbl.Xalign = 0
                                   lbl.Ellipsize = Pango.EllipsizeMode.End
                                   listBox.Add(lbl)
                               Next
                               listBox.ShowAll()
                           End Sub)
    End Sub

    Private Async Function FetchDmsAndTopicsListsAsync() As Task
        Try
            Dim dms = Await reader.GetDMContactsAsync()
            UpdateListBox(dmContactsList, dms)

            Dim topics = Await reader.GetModLogsAsync()

        Catch ex As Exception
            statusLabel.Text = $"List fetch failed: {ex.Message}"
        End Try
    End Function

    Private Async Function RefreshNeighborhoodAsync() As Task
        Try
            Dim posts = Await reader.GetNeighborhoodHistoryAsync(currentHistoryIndex)
            InvokeOnMainThread(Sub()
                                           ClearListBox(nbPostsList)
                                           For Each post In posts
                                               Dim row As New Label(If(post.IsDeleted, $"[PURGED] {post.Title}", post.Title))
                                               row.Xalign = 0
                                               row.Ellipsize = Pango.EllipsizeMode.End
                                               nbPostsList.Add(row)
                                           Next
                                           nbPostsList.ShowAll()
                                   End Sub)
        Catch ex As Exception
            statusLabel.Text = $"Neighborhood refresh failed: {ex.Message}"
        End Try
    End Function

    Private Sub HandleRosterUpdate(users As List(Of RosterUser))
        InvokeOnMainThread(Sub()
                               ClearListBox(globalUsersList)
                               For Each u In users
                                   If Not String.Equals(u.Username, "system", StringComparison.OrdinalIgnoreCase) Then
                                       Dim row As New Label()
                                       row.Text = u.DisplayText
                                       row.Xalign = 0
                                       row.Ellipsize = Pango.EllipsizeMode.End
                                       globalUsersList.Add(row)
                                   End If
                               Next
                               globalUsersList.ShowAll()
                           End Sub)
    End Sub

    Private Sub HandleTopicsUpdated(topics As List(Of TopicRoom))
        InvokeOnMainThread(Sub()
                               ClearListBox(topicList)
                               For Each t In topics
                                   Dim row As New Label()
                                   row.Text = t.Slug
                                   row.Xalign = 0
                                   row.Ellipsize = Pango.EllipsizeMode.End
                                   topicList.Add(row)
                               Next
                               topicList.ShowAll()
                           End Sub)
    End Sub

    Private Sub HandleFeedRefresh()
        Task.Run(Async Function()
                     Await RefreshActiveFeedAsync()
                 End Function)
    End Sub

    Private Sub OnWindowClosed(sender As Object, e As DeleteEventArgs)
        Application.Quit()
    End Sub

    Private Sub HandleDisconnect(reason As String)
        ShowMessage("Disconnected", $"Disconnected from server: {reason}", MessageType.Error)
        Application.Quit()
    End Sub

    Private Sub HandleErrorAlert(message As String)
        ShowMessage("Server Alert", message, MessageType.Warning)
    End Sub

    Private Sub InvokeOnMainThread(action As System.Action)
        GLib.Idle.Add(Function()
                         action()
                         Return False
                     End Function)
    End Sub

    Private Sub ShowMessage(title As String, message As String, msgType As MessageType)
        InvokeOnMainThread(Sub()
                               Using dlg As New MessageDialog(Me, DialogFlags.Modal, msgType, ButtonsType.Ok, message)
                                   dlg.Text = title
                                   dlg.Run()
                               End Using
                           End Sub)
    End Sub

    Private Async Sub OnGlobalSendClicked(sender As Object, e As EventArgs)
        Dim text = globalInput.Text.Trim()
        If String.IsNullOrEmpty(text) Then Return

        globalSendButton.Sensitive = False
        Await reader.SendWsMessageAsync(New With {.type = "message", .content = text, .target = String.Empty})
        globalInput.Text = String.Empty
        globalSendButton.Sensitive = True
    End Sub

    Private Sub OnDmContactSelected(sender As Object, e As EventArgs)
        Dim row = dmContactsList.SelectedRow
        If row Is Nothing OrElse row.Child Is Nothing Then Return

        currentTarget = CType(row.Child, Label).Text
        currentContext = "dm"
        currentHistoryIndex = 0
        Task.Run(Async Function()
                     Await reader.SendWsMessageAsync(New With {.type = "switch_context", .mode = currentContext, .target = currentTarget})
                     Await RefreshActiveFeedAsync()
                 End Function)
    End Sub

    Private Async Sub OnDmAddClicked(sender As Object, e As EventArgs)
        Dim target = dmTargetInput.Text.Trim()
        If String.IsNullOrEmpty(target) Then Return
        Dim nm As New Label(target) With {.Xalign = 0}
        nm.Ellipsize = Pango.EllipsizeMode.End
        dmContactsList.Add(nm)
        dmContactsList.ShowAll()
        dmTargetInput.Text = String.Empty
    End Sub

    Private Async Sub OnDmSendClicked(sender As Object, e As EventArgs)
        If String.IsNullOrWhiteSpace(dmInput.Text) OrElse String.IsNullOrWhiteSpace(currentTarget) Then Return
        dmSendButton.Sensitive = False
        Await reader.SendWsMessageAsync(New With {.type = "dm", .content = dmInput.Text, .target = currentTarget})
        dmInput.Text = String.Empty
        dmSendButton.Sensitive = True
    End Sub

    Private Sub OnTopicSelected(sender As Object, e As EventArgs)
        Dim row = topicList.SelectedRow
        If row Is Nothing OrElse row.Child Is Nothing Then Return

        currentTarget = CType(row.Child, Label).Text
        currentContext = "topic"
        currentHistoryIndex = 0
        Task.Run(Async Function()
                     Await reader.SendWsMessageAsync(New With {.type = "switch_context", .mode = currentContext, .target = currentTarget})
                     Await RefreshActiveFeedAsync()
                 End Function)
    End Sub

    Private Async Sub OnTopicCreateClicked(sender As Object, e As EventArgs)
        If String.IsNullOrWhiteSpace(topicCreateInput.Text) Then Return
        Await reader.SendWsMessageAsync(New With {.type = "create_topic", .title = topicCreateInput.Text})
        topicCreateInput.Text = String.Empty
    End Sub

    Private Async Sub OnTopicSendClicked(sender As Object, e As EventArgs)
        If String.IsNullOrWhiteSpace(topicInput.Text) OrElse String.IsNullOrWhiteSpace(currentTarget) Then Return
        topicSendButton.Sensitive = False
        Await reader.SendWsMessageAsync(New With {.type = "topic_message", .content = topicInput.Text, .target = currentTarget})
        topicInput.Text = String.Empty
        topicSendButton.Sensitive = True
    End Sub

    Private Sub OnNeighborhoodPostSelected(sender As Object, e As EventArgs)
        Dim row = nbPostsList.SelectedRow
        If row Is Nothing OrElse row.Child Is Nothing Then Return

        Dim title = CType(row.Child, Label).Text
        Task.Run(Async Function()
                     Dim posts = Await reader.GetNeighborhoodHistoryAsync(currentHistoryIndex)
                     Dim selectedPost = posts.Find(Function(p) If(p.IsDeleted, $"[PURGED] {p.Title}", p.Title) = title)
                     If selectedPost IsNot Nothing Then
                         activeNeighborhoodPostId = selectedPost.Id
                         InvokeOnMainThread(Sub()
                                                nbPostView.Buffer.Text = BuildPostText(selectedPost)
                                                nbCommentsView.Buffer.Text = BuildCommentsText(selectedPost.Comments)
                                            End Sub)
                     End If
                 End Function)
    End Sub

    Private Function BuildPostText(post As NeighborhoodPost) As String
        Dim sb As New StringBuilder()
        sb.AppendLine($"[ {post.DisplayHeader} ]")
        sb.AppendLine($"TITLE : {post.Title}")
        sb.AppendLine()
        sb.AppendLine(If(post.IsDeleted, "[POST PURGED]", post.Content))
        Return sb.ToString()
    End Function

    Private Function BuildCommentsText(comments As List(Of NeighborhoodComment)) As String
        Dim sb As New StringBuilder()
        If comments IsNot Nothing Then
            For Each comment In comments
                sb.AppendLine(comment.FormattedText)
            Next
        End If
        Return sb.ToString()
    End Function

    Private Async Sub OnCreatePostClicked(sender As Object, e As EventArgs)
        Using dialog = New NewPostDialog()
            If dialog.Run() = ResponseType.Ok Then
                Await reader.PostNeighborhoodThreadAsync(dialog.PostTitle, dialog.PostContent)
                Await RefreshNeighborhoodAsync()
            End If
        End Using
    End Sub

    Private Async Sub OnSubmitCommentClicked(sender As Object, e As EventArgs)
        If activeNeighborhoodPostId = -1 Then
            ShowMessage("No Post Selected", "Select a post from the board first.", MessageType.Info)
            Return
        End If
        If String.IsNullOrWhiteSpace(nbCommentInput.Text) Then Return

        nbSubmitCommentButton.Sensitive = False
        Await reader.PostNeighborhoodCommentAsync(activeNeighborhoodPostId, nbCommentInput.Text)
        nbCommentInput.Text = String.Empty
        Await RefreshNeighborhoodAsync()
        nbSubmitCommentButton.Sensitive = True
    End Sub

    Private Async Sub OnModSearchClicked(sender As Object, e As EventArgs)
        Dim target = modSearchEntry.Text.Trim()
        If String.IsNullOrEmpty(target) Then Return

        modSearchButton.Sensitive = False
        modUserStatusView.Buffer.Text = "Querying target…"
        Dim profile = Await reader.FindUserAsync(target)
        InvokeOnMainThread(Sub() modUserStatusView.Buffer.Text = profile.StatusSummary)
        modSearchButton.Sensitive = True
    End Sub

    Private Async Sub OnModRefreshLogsClicked(sender As Object, e As EventArgs)
        modRefreshLogsButton.Sensitive = False
        InvokeOnMainThread(Sub() modLogsView.Buffer.Text = "Loading…")
        Dim logs = Await reader.GetModLogsAsync()
        Dim sb As New StringBuilder()
        For Each entry In logs
            sb.AppendLine(entry.FormattedText)
        Next
        InvokeOnMainThread(Sub() modLogsView.Buffer.Text = If(sb.Length > 0, sb.ToString(), "(No log entries found)"))
        modRefreshLogsButton.Sensitive = True
    End Sub

    Private Async Sub OnModBanClicked(sender As Object, e As EventArgs)
        Dim target = modSearchEntry.Text.Trim()
        If String.IsNullOrEmpty(target) Then Return
        Dim reason = If(String.IsNullOrWhiteSpace(modReasonInput.Text), "No reason given", modReasonInput.Text)
        If ConfirmAction($"Permanently BAN @{target}?\nReason: {reason}") Then
            Await reader.ModBanAsync(target, reason)
        End If
    End Sub

    Private Async Sub OnModPardonClicked(sender As Object, e As EventArgs)
        Dim target = modSearchEntry.Text.Trim()
        If String.IsNullOrEmpty(target) Then Return
        If ConfirmAction($"PARDON @{target}? (Lifts ban + timeout)") Then
            Await reader.ModPardonAsync(target)
        End If
    End Sub

    Private Async Sub OnModSetRoleClicked(sender As Object, e As EventArgs)
        Dim target = modSearchEntry.Text.Trim()
        If String.IsNullOrEmpty(target) Then Return
        If ConfirmAction($"Toggle MOD role for @{target}?") Then
            Try
                Await reader.SetRoleAsync(target)
            Catch ex As Exception
                ShowMessage("Error", $"Role change failed: {ex.Message}", MessageType.Error)
            End Try
        End If
    End Sub

    Private Async Sub OnModBanIPClicked(sender As Object, e As EventArgs)
        Dim target = modSearchEntry.Text.Trim()
        If String.IsNullOrEmpty(target) Then Return
        Dim reason = If(String.IsNullOrWhiteSpace(modReasonInput.Text), "Network-level block", modReasonInput.Text)
        If ConfirmAction($"Purge NETWORK IP for @{target}?\nThis affects all accounts on that IP.") Then
            Try
                Await reader.BanIpAsync(target, reason)
            Catch ex As Exception
                ShowMessage("Error", $"IP ban failed: {ex.Message}", MessageType.Error)
            End Try
        End If
    End Sub

    Private Async Sub OnModKickClicked(sender As Object, e As EventArgs)
        Dim target = modSearchEntry.Text.Trim()
        If String.IsNullOrEmpty(target) Then Return
        If ConfirmAction($"KICK @{target} from the server?") Then
            Await reader.ModKickAsync(target)
        End If
    End Sub

    Private Async Sub OnModTimeoutClicked(sender As Object, e As EventArgs)
        Dim target = modSearchEntry.Text.Trim()
        If String.IsNullOrEmpty(target) Then Return
        Dim minutes = CInt(modTimeoutAdjustment.Value)
        Dim reason = If(String.IsNullOrWhiteSpace(modReasonInput.Text), "Timed out by moderator", modReasonInput.Text)
        If ConfirmAction($"TIMEOUT @{target} for {minutes} minute(s)?\nReason: {reason}") Then
            Await reader.ModTimeoutAsync(target, minutes, reason)
        End If
    End Sub

    Private Sub OnOpenBrowserClicked(sender As Object, e As EventArgs)
        Dim url = reader.ServerUrl
        Try
            Process.Start("xdg-open", url)
        Catch
            ShowMessage("Open Browser", "Unable to open browser. Please visit: " & url, MessageType.Info)
        End Try
    End Sub

    Private Sub OnAboutClicked(sender As Object, e As EventArgs)
        Using about As New AboutWindow()
            about.Run()
        End Using
    End Sub

    Private Sub OnExitClicked(sender As Object, e As EventArgs)
        Application.Quit()
    End Sub

    Private Sub OnPageNewerClicked(sender As Object, e As EventArgs)
        If currentHistoryIndex > 0 Then
            currentHistoryIndex -= 1
            Task.Run(Async Function()
                         Await RefreshActiveFeedAsync()
                     End Function)
        End If
    End Sub

    Private Sub OnPageOlderClicked(sender As Object, e As EventArgs)
        currentHistoryIndex += 1
        Task.Run(Async Function()
                     Await RefreshActiveFeedAsync()
                 End Function)
    End Sub

    Private Sub OnNotebookSwitched(sender As Object, e As SwitchPageArgs)
        Dim pageIndex = notebookControl.CurrentPage
        Select Case pageIndex
            Case 0
                currentContext = "public"
                currentTarget = String.Empty
            Case 1
                currentContext = "dm"
            Case 2
                currentContext = "topic"
            Case 3
                currentContext = "neighborhood"
            Case Else
                currentContext = "public"
        End Select

        Task.Run(Async Function()
                     Try
                         Await reader.SendWsMessageAsync(New With {.type = "switch_context", .mode = currentContext, .target = currentTarget})
                         Await RefreshActiveFeedAsync()
                     Catch
                     End Try
                 End Function)
    End Sub

    Private Function ConfirmAction(message As String) As Boolean
        Dim result As Boolean = False
        InvokeOnMainThread(Sub()
                               Using dlg As New MessageDialog(Me, DialogFlags.Modal, MessageType.Question, ButtonsType.YesNo, message)
                                   dlg.Text = "Confirm Action"
                                   result = dlg.Run() = ResponseType.Yes
                               End Using
                           End Sub)
        Return result
    End Function
End Class
