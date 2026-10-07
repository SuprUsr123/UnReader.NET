Imports Microsoft.Web.WebView2.Core
Imports System.Collections.Generic
Imports System.Linq
Imports System.Threading.Tasks
Imports System.Windows.Forms
Imports System
Imports UnReader.NET.Core

Public Class Form1
    ' Shared API instance; configured (server URL + token) at login time
    ' from AppSettings. Kept as a form-wide instance for the legacy call sites.
    Public Shared api As ServerReader

    Private hasConnected As Boolean = False
    Private isRefreshingFeed As Boolean = False
    Private pendingRefresh As Boolean = False

    Private currentContext As String = "public"
    Private currentTarget As String = ""
    Private activeNeighborhoodPostId As Integer = -1
    Private isWebViewInitialized As Boolean = False
    Private currentHistoryIndex As Integer = 0
    Private suggestionPageIndex As Integer = 0
    Private isSuggestionsLoaded As Boolean = False
    Private loadedSuggestions As New List(Of Suggestion)
    Private currentFeedMessages As New List(Of ChatMessage)

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        If api Is Nothing Then
            api = New ServerReader(AppSettings.Current.ServerUrl)
        End If

        AddHandler api.OnRosterUpdated, AddressOf HandleRosterUpdate
        AddHandler api.OnDisconnected, AddressOf HandleDisconnect
        AddHandler api.OnErrorAlert, AddressOf HandleErrorAlert
        AddHandler api.OnFeedRefreshed, AddressOf HandleFeedRefresh
        AddHandler api.OnTopicsUpdated, AddressOf HandleTopicsUpdated
        AddHandler api.OnSuggestionsUpdated, AddressOf HandleSuggestionsUpdated
        ApplyStaffVisibility()
    End Sub

    Public Sub ApplyStaffVisibility()
        ' The server remains the authority; this only keeps irrelevant tabs and
        ' controls out of each role's way.
        Dim staff = api IsNot Nothing AndAlso api.IsStaff
        Dim admin = api IsNot Nothing AndAlso api.IsAdmin
        If api IsNot Nothing AndAlso api.CurrentUser IsNot Nothing Then
            Text = $"UnReader.NET — {api.CurrentUser.DisplayRole}"
        End If
        btnGlobalPurge.Visible = staff
        btnDmPurge.Visible = staff
        btnTopicPurge.Visible = staff
        ' The server permits users to delete their own suggestions.
        btnSuggestionDelete.Visible = True
        btnSuggestionDelete.Enabled = False

        btnModBan.Visible = admin
        btnModPardon.Visible = admin
        btnModSetRole.Visible = admin
        btnModBanIP.Visible = admin
        btnModKick.Visible = staff
        btnModTimeout.Visible = staff
        ' Add or remove the Moderation tab entirely, depending on staff status.
        If staff AndAlso Not MainTabControl.TabPages.Contains(ModerationTab) Then
            MainTabControl.TabPages.Add(ModerationTab)
        ElseIf Not staff AndAlso MainTabControl.TabPages.Contains(ModerationTab) Then
            MainTabControl.TabPages.Remove(ModerationTab)
        End If
    End Sub

    Private Async Sub Form1_VisibleChanged(sender As Object, e As EventArgs) Handles Me.VisibleChanged
        If Me.Visible AndAlso Not hasConnected Then
            hasConnected = True
            Await api.ConnectRealtimeAsync()
            ApplyStaffVisibility()
            Await LoadInitialDataAsync()
            Await InitializeWebEngineAsync()
        End If
    End Sub

    Private Async Function LoadInitialDataAsync() As Task
        Try
            Await RefreshActiveFeedAsync()
            Await FetchDmsAndTopicsListsAsync()
        Catch ex As Exception
            MessageBox.Show($"Failed to load initial data: {ex.Message}", "Initialization Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Function

    Private Async Function InitializeWebEngineAsync() As Task
        If isWebViewInitialized Then Return

        Try
            Await UnReaderWeb.EnsureCoreWebView2Async()
            UnReaderWeb.CoreWebView2.Settings.AreBrowserAcceleratorKeysEnabled = False
            UnReaderWeb.CoreWebView2.Settings.AreDevToolsEnabled = False
            UnReaderWeb.CoreWebView2.Settings.AreDefaultContextMenusEnabled = False

            AddHandler UnReaderWeb.CoreWebView2.NewWindowRequested, AddressOf OnNewWindowRequested
            AddHandler UnReaderWeb.CoreWebView2.NavigationStarting, AddressOf OnNavigationStarting

            UnReaderWeb.Source = New Uri(api.ServerUrl)
            isWebViewInitialized = True
        Catch ex As Exception
            MessageBox.Show($"WebView2 failed to initialize: {ex.Message}", "Engine Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End Try
    End Function

    Private Async Sub MainTabControl_SelectedIndexChanged(sender As Object, e As EventArgs) Handles MainTabControl.SelectedIndexChanged
        Dim previousContext As String = currentContext
        Dim previousTarget As String = currentTarget

        Select Case MainTabControl.SelectedTab.Name
            Case "ChatTab"
                currentContext = "public"
                currentTarget = ""
            Case "DMsTab"
                currentContext = "dm"
                currentTarget = If(lstDmConversations.SelectedItem IsNot Nothing, lstDmConversations.SelectedItem.ToString(), "")
            Case "Topics"
                currentContext = "topic"
                currentTarget = If(lstTopicsList.SelectedItem IsNot Nothing, lstTopicsList.SelectedItem.ToString(), "")
            Case "NeighbourhoodTab"
                currentContext = "neighborhood"
                currentTarget = ""
            Case "ModerationTab"
                currentContext = "moderation"
                currentTarget = ""
            Case "SuggestionsTab"
                If Not isSuggestionsLoaded Then
                    isSuggestionsLoaded = True
                    Call RefreshSuggestionsAsync()
                End If
        End Select

        If currentContext = previousContext AndAlso currentTarget = previousTarget Then Return

        currentHistoryIndex = 0

        If currentContext <> "neighborhood" AndAlso currentContext <> "moderation" Then
            Await api.SwitchContextAsync(currentContext, currentTarget)
        End If

        Await RefreshActiveFeedAsync()
    End Sub

    Private Sub HandleRosterUpdate(users As List(Of RosterUser))
        Me.BeginInvoke(Sub() UpdateRosterUI(users))
    End Sub

    Private Sub UpdateRosterUI(users As List(Of RosterUser))
        lstGlobalOnline.Items.Clear()
        For Each u In users
            If Not String.Equals(u.Username, "system", StringComparison.OrdinalIgnoreCase) Then
                lstGlobalOnline.Items.Add(u.DisplayText)
            End If
        Next
    End Sub

    Private Sub HandleTopicsUpdated(topics As List(Of TopicRoom))
        Me.Invoke(Sub()
                      lstTopicsList.Items.Clear()
                      For Each t In topics
                          lstTopicsList.Items.Add(t.Slug)
                      Next
                  End Sub)
    End Sub

    Private Sub HandleFeedRefresh()
        Me.BeginInvoke(Sub()
                           Call RefreshActiveFeedAsync()
                       End Sub)
    End Sub

    Private Sub HandleDisconnect(reason As String)
        Me.Invoke(Sub()
                      MessageBox.Show($"Disconnected from server: {reason}", "Connection Lost", MessageBoxButtons.OK, MessageBoxIcon.Error)
                      Me.Hide()
                  End Sub)
    End Sub

    Private Sub HandleErrorAlert(message As String)
        Me.Invoke(Sub()
                      MessageBox.Show($"Server Alert: {message}", "Notice", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                  End Sub)
    End Sub

    Private Sub HandleSuggestionsUpdated()
        Me.BeginInvoke(Sub()
                           If MainTabControl.SelectedTab Is SuggestionsTab Then
                               Call RefreshSuggestionsAsync()
                           End If
                       End Sub)
    End Sub

    Private Async Function RefreshActiveFeedAsync() As Task
        If isRefreshingFeed Then
            pendingRefresh = True
            Return
        End If

        isRefreshingFeed = True

        Try
            Do
                pendingRefresh = False
                lblPageIndex.Text = $"PAGE: {currentHistoryIndex}"

                Select Case currentContext
                    Case "public"
                        Dim msgs = Await api.GetHistoryAsync(currentHistoryIndex)
                        UpdateChatBox(lstGlobalChat, msgs)

                    Case "dm"
                        If Not String.IsNullOrEmpty(currentTarget) Then
                            Dim msgs = Await api.GetDMHistoryAsync(currentTarget, currentHistoryIndex)
                            UpdateChatBox(lstDmChat, msgs)
                        Else
                            lstDmChat.Items.Clear()
                        End If

                    Case "topic"
                        If Not String.IsNullOrEmpty(currentTarget) Then
                            Dim msgs = Await api.GetTopicHistoryAsync(currentTarget, currentHistoryIndex)
                            UpdateChatBox(lstTopicChat, msgs)
                        Else
                            lstTopicChat.Items.Clear()
                        End If

                    Case "neighborhood"
                        Await RefreshNeighborhoodAsync()
                End Select

            Loop While pendingRefresh

        Catch ex As Exception
        Finally
            isRefreshingFeed = False
        End Try
    End Function

    Private Sub UpdateChatBox(listBox As ListBox, messages As IEnumerable(Of ChatMessage))
        currentFeedMessages = messages.ToList()
        listBox.BeginUpdate()
        listBox.Items.Clear()
        For Each msg In messages
            listBox.Items.Add(msg.FormattedText)
        Next
        If listBox.Items.Count > 0 Then listBox.TopIndex = listBox.Items.Count - 1
        listBox.EndUpdate()
    End Sub

    Private Async Function FetchDmsAndTopicsListsAsync() As Task
        Try
            Dim dms = Await api.GetDMContactsAsync()
            lstDmConversations.Items.Clear()
            For Each contact In dms
                lstDmConversations.Items.Add(contact)
            Next
        Catch ex As Exception
        End Try
    End Function

    Private Sub btnPageNewer_Click(sender As Object, e As EventArgs) Handles btnPageNewer.Click
        If currentHistoryIndex > 0 Then
            currentHistoryIndex -= 1
            Call RefreshActiveFeedAsync()
        End If
    End Sub

    Private Sub btnPageOlder_Click(sender As Object, e As EventArgs) Handles btnPageOlder.Click
        currentHistoryIndex += 1
        Call RefreshActiveFeedAsync()
    End Sub

    Private Async Sub btnGlobalSend_Click(sender As Object, e As EventArgs) Handles btnGlobalSend.Click
        If String.IsNullOrWhiteSpace(txtGlobalInput.Text) Then Return
        btnGlobalSend.Enabled = False
        Await api.SendPublicMessageAsync(txtGlobalInput.Text)
        txtGlobalInput.Clear()
        btnGlobalSend.Enabled = True
    End Sub

    Private Async Sub btnDmSend_Click(sender As Object, e As EventArgs) Handles btnDmSend.Click
        If String.IsNullOrWhiteSpace(txtDmInput.Text) OrElse String.IsNullOrWhiteSpace(currentTarget) Then Return
        btnDmSend.Enabled = False
        Await api.SendDirectMessageAsync(currentTarget, txtDmInput.Text)
        txtDmInput.Clear()
        btnDmSend.Enabled = True
    End Sub

    Private Async Sub btnTopicSend_Click(sender As Object, e As EventArgs) Handles btnTopicSend.Click
        If String.IsNullOrWhiteSpace(txtTopicInput.Text) OrElse String.IsNullOrWhiteSpace(currentTarget) Then Return
        btnTopicSend.Enabled = False
        Await api.SendTopicMessageAsync(currentTarget, txtTopicInput.Text)
        txtTopicInput.Clear()
        btnTopicSend.Enabled = True
    End Sub

    Private Sub Inputs_KeyDown(sender As Object, e As KeyEventArgs) Handles txtGlobalInput.KeyDown, txtDmInput.KeyDown, txtTopicInput.KeyDown
        If e.KeyCode = Keys.Enter Then
            e.SuppressKeyPress = True
            If sender Is txtGlobalInput Then btnGlobalSend.PerformClick()
            If sender Is txtDmInput Then btnDmSend.PerformClick()
            If sender Is txtTopicInput Then btnTopicSend.PerformClick()
        End If
    End Sub

    Private Async Sub lstDmConversations_SelectedIndexChanged(sender As Object, e As EventArgs) Handles lstDmConversations.SelectedIndexChanged
        If lstDmConversations.SelectedItem IsNot Nothing Then
            currentTarget = lstDmConversations.SelectedItem.ToString()
            currentHistoryIndex = 0
            Await api.SwitchContextAsync("dm", currentTarget)
            Await RefreshActiveFeedAsync()
        End If
    End Sub

    Private Async Sub lstTopicsList_SelectedIndexChanged(sender As Object, e As EventArgs) Handles lstTopicsList.SelectedIndexChanged
        If lstTopicsList.SelectedItem IsNot Nothing Then
            currentTarget = lstTopicsList.SelectedItem.ToString()
            currentHistoryIndex = 0
            Await api.SwitchContextAsync("topic", currentTarget)
            Await RefreshActiveFeedAsync()
        End If
    End Sub

    Private Sub btnDmStart_Click(sender As Object, e As EventArgs) Handles btnDmStart.Click
        Dim target = txtDmTargetInput.Text.Trim()
        If Not String.IsNullOrEmpty(target) AndAlso Not lstDmConversations.Items.Contains(target) Then
            lstDmConversations.Items.Add(target)
            lstDmConversations.SelectedItem = target
            txtDmTargetInput.Clear()
        End If
    End Sub

    Private Async Sub btnTopicCreate_Click(sender As Object, e As EventArgs) Handles btnTopicCreate.Click
        If Not String.IsNullOrWhiteSpace(txtTopicTargetInput.Text) Then
            Await api.CreateTopicAsync(txtTopicTargetInput.Text)
            txtTopicTargetInput.Clear()
        End If
    End Sub

    Private Async Function RefreshNeighborhoodAsync() As Task
        Dim posts = Await api.GetNeighborhoodHistoryAsync(currentHistoryIndex)
        lstNeighbourhoodPosts.BeginUpdate()
        lstNeighbourhoodPosts.Items.Clear()
        For Each post In posts
            lstNeighbourhoodPosts.Items.Add(post)
        Next
        lstNeighbourhoodPosts.EndUpdate()
    End Function

    Private Sub lstNeighbourhoodPosts_SelectedIndexChanged(sender As Object, e As EventArgs) Handles lstNeighbourhoodPosts.SelectedIndexChanged
        If lstNeighbourhoodPosts.SelectedItem Is Nothing Then Return

        Dim post As NeighborhoodPost = TryCast(lstNeighbourhoodPosts.SelectedItem, NeighborhoodPost)
        If post Is Nothing Then Return

        activeNeighborhoodPostId = post.Id

        Dim sb As New System.Text.StringBuilder()
        sb.AppendLine($"[ {post.DisplayHeader} ]")
        sb.AppendLine($"TITLE : {post.Title}")
        sb.AppendLine()
        sb.AppendLine(If(post.IsDeleted, "[POST PURGED]", post.Content))
        txtPostView.Text = sb.ToString()

        treeNeighbourhoodComments.Nodes.Clear()
        If post.Comments IsNot Nothing Then
            For Each comment In post.Comments
                treeNeighbourhoodComments.Nodes.Add(comment.FormattedText)
            Next
        End If
    End Sub

    Private Async Sub btnCreatePost_Click(sender As Object, e As EventArgs) Handles btnCreatePost.Click
        Using dlg As New NewPostDialog()
            If dlg.ShowDialog(Me) = DialogResult.OK Then
                Await api.PostNeighborhoodThreadAsync(dlg.PostTitle, dlg.PostContent)
                Await RefreshNeighborhoodAsync()
            End If
        End Using
    End Sub

    Private Async Sub btnSubmitComment_Click(sender As Object, e As EventArgs) Handles btnSubmitComment.Click
        If activeNeighborhoodPostId = -1 Then
            MessageBox.Show("Select a post from the board first.", "No Post Selected", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If
        If String.IsNullOrWhiteSpace(txtCommentInput.Text) Then Return

        btnSubmitComment.Enabled = False
        Await api.PostNeighborhoodCommentAsync(activeNeighborhoodPostId, txtCommentInput.Text)
        txtCommentInput.Clear()
        Await RefreshNeighborhoodAsync()
        btnSubmitComment.Enabled = True
    End Sub

    Private Async Sub btnModSearch_Click(sender As Object, e As EventArgs) Handles btnModSearch.Click
        Dim target = txtModSearchUser.Text.Trim()
        If String.IsNullOrEmpty(target) Then Return

        btnModSearch.Enabled = False
        txtModUserStatus.Text = "Querying target…"

        Dim profile = Await api.FindUserAsync(target)
        txtModUserStatus.Text = profile.StatusSummary

        btnModSearch.Enabled = True
    End Sub

    Private Async Sub btnModRefreshLogs_Click(sender As Object, e As EventArgs) Handles btnModRefreshLogs.Click
        btnModRefreshLogs.Enabled = False
        txtModLogs.Text = "Loading…"

        Dim logs = Await api.GetModLogsAsync()
        Dim sb As New System.Text.StringBuilder()
        For Each entry In logs
            sb.AppendLine(entry.FormattedText)
        Next
        txtModLogs.Text = If(sb.Length > 0, sb.ToString(), "(No log entries found)")

        btnModRefreshLogs.Enabled = True
    End Sub

    Private Async Sub btnModBan_Click(sender As Object, e As EventArgs) Handles btnModBan.Click
        Dim target = txtModSearchUser.Text.Trim()
        If String.IsNullOrEmpty(target) Then Return

        Dim reason = If(String.IsNullOrWhiteSpace(txtModReason.Text), "No reason given", txtModReason.Text)
        If MessageBox.Show($"Permanently BAN @{target}?{vbCrLf}Reason: {reason}", "Confirm BAN",
                           MessageBoxButtons.YesNo, MessageBoxIcon.Warning) = DialogResult.Yes Then
            Await api.ModBanAsync(target, reason)
        End If
    End Sub

    Private Async Sub btnModPardon_Click(sender As Object, e As EventArgs) Handles btnModPardon.Click
        Dim target = txtModSearchUser.Text.Trim()
        If String.IsNullOrEmpty(target) Then Return

        If MessageBox.Show($"PARDON @{target}? (Lifts ban + timeout)", "Confirm PARDON",
                           MessageBoxButtons.YesNo, MessageBoxIcon.Question) = DialogResult.Yes Then
            Await api.ModPardonAsync(target)
        End If
    End Sub

    Private Async Sub btnModSetRole_Click(sender As Object, e As EventArgs) Handles btnModSetRole.Click
        Dim target = txtModSearchUser.Text.Trim()
        If String.IsNullOrEmpty(target) Then Return

        If MessageBox.Show($"Toggle MOD role for @{target}?", "Confirm Role Toggle",
                           MessageBoxButtons.YesNo, MessageBoxIcon.Question) = DialogResult.Yes Then
            Try
                Await api.SetRoleAsync(target)
            Catch ex As Exception
                MessageBox.Show($"Role change failed: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            End Try
        End If
    End Sub

    Private Async Sub btnModBanIP_Click(sender As Object, e As EventArgs) Handles btnModBanIP.Click
        Dim target = txtModSearchUser.Text.Trim()
        If String.IsNullOrEmpty(target) Then Return

        Dim reason = If(String.IsNullOrWhiteSpace(txtModReason.Text), "Network-level block", txtModReason.Text)
        If MessageBox.Show($"Purge NETWORK IP for @{target}?{vbCrLf}This affects all accounts on that IP.",
                           "Confirm IP BAN", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) = DialogResult.Yes Then
            Try
                Await api.BanIpAsync(target, reason)
            Catch ex As Exception
                MessageBox.Show($"IP ban failed: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            End Try
        End If
    End Sub

    Private Async Sub btnModKick_Click(sender As Object, e As EventArgs) Handles btnModKick.Click
        Dim target = txtModSearchUser.Text.Trim()
        If String.IsNullOrEmpty(target) Then Return

        If MessageBox.Show($"KICK @{target} from the server?", "Confirm KICK",
                           MessageBoxButtons.YesNo, MessageBoxIcon.Warning) = DialogResult.Yes Then
            Await api.ModKickAsync(target)
        End If
    End Sub

    Private Async Sub btnModTimeout_Click(sender As Object, e As EventArgs) Handles btnModTimeout.Click
        Dim target = txtModSearchUser.Text.Trim()
        If String.IsNullOrEmpty(target) Then Return

        Dim minutes = CInt(numModTimeout.Value)
        Dim reason = If(String.IsNullOrWhiteSpace(txtModReason.Text), "Timed out by moderator", txtModReason.Text)

        If MessageBox.Show($"TIMEOUT @{target} for {minutes} minute(s)?{vbCrLf}Reason: {reason}", "Confirm TIMEOUT",
                           MessageBoxButtons.YesNo, MessageBoxIcon.Warning) = DialogResult.Yes Then
            Await api.ModTimeoutAsync(target, minutes, reason)
        End If
    End Sub

    ' -----------------------------------------------------------------
    ' Suggestions board (newer server API)
    ' -----------------------------------------------------------------
    Private Async Function RefreshSuggestionsAsync() As Task
        Try
            Dim items = Await api.GetSuggestionsAsync(suggestionPageIndex)
            loadedSuggestions = items
            lstSuggestions.BeginUpdate()
            lstSuggestions.Items.Clear()
            For Each s In items
                lstSuggestions.Items.Add(s)
            Next
            lstSuggestions.EndUpdate()
            lblSuggestionPage.Text = $"PAGE: {suggestionPageIndex}"
        Catch ex As Exception
        End Try
    End Function

    Private Sub lstSuggestions_SelectedIndexChanged(sender As Object, e As EventArgs) Handles lstSuggestions.SelectedIndexChanged
        Dim s As Suggestion = TryCast(lstSuggestions.SelectedItem, Suggestion)
        If s Is Nothing Then
            btnSuggestionDelete.Enabled = False
            Return
        End If

        Dim isOwner = api IsNot Nothing AndAlso api.CurrentUser IsNot Nothing AndAlso
            String.Equals(s.Username, api.CurrentUser.Username, StringComparison.OrdinalIgnoreCase)
        btnSuggestionDelete.Enabled = api IsNot Nothing AndAlso (api.IsStaff OrElse isOwner)

        MessageBox.Show(
            $"{s.Title}{vbCrLf}{vbCrLf}{s.Description}{vbCrLf}{vbCrLf}" &
            $"By @{s.Username} ({s.DateText}){vbCrLf}" &
            If(s.Completed, "[COMPLETED]", "") & If(Not String.IsNullOrWhiteSpace(s.StatusTag), $"  [{s.StatusTag}]", "") & vbCrLf &
            $"#ID {s.Id}",
            "Suggestion Details",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information)
    End Sub

    Private Async Sub btnSuggestionSubmit_Click(sender As Object, e As EventArgs) Handles btnSuggestionSubmit.Click
        If String.IsNullOrWhiteSpace(txtSuggestionTitle.Text) OrElse String.IsNullOrWhiteSpace(txtSuggestionBody.Text) Then
            MessageBox.Show("Both a title and description are required.")
            Return
        End If
        btnSuggestionSubmit.Enabled = False
        Dim err = Await api.PostSuggestionAsync(txtSuggestionTitle.Text.Trim(), txtSuggestionBody.Text.Trim())
        If err Is Nothing Then
            txtSuggestionTitle.Clear()
            txtSuggestionBody.Clear()
            suggestionPageIndex = 0
            Await RefreshSuggestionsAsync()
        Else
            MessageBox.Show($"Failed to post suggestion: {err}")
        End If
        btnSuggestionSubmit.Enabled = True
    End Sub

    Private Async Sub btnSuggestionDelete_Click(sender As Object, e As EventArgs) Handles btnSuggestionDelete.Click
        If lstSuggestions.SelectedItem Is Nothing Then Return
        Dim s As Suggestion = TryCast(lstSuggestions.SelectedItem, Suggestion)
        If s Is Nothing Then Return
        If MessageBox.Show($"Delete suggestion #{s.Id} by @{s.Username}?", "Confirm Delete",
                           MessageBoxButtons.YesNo, MessageBoxIcon.Warning) = DialogResult.Yes Then
            Dim err = Await api.DeleteSuggestionAsync(s.Id)
            If err Is Nothing Then
                Await RefreshSuggestionsAsync()
            Else
                MessageBox.Show($"Failed to delete suggestion: {err}")
            End If
        End If
    End Sub

    Private Sub btnSuggestionNewer_Click(sender As Object, e As EventArgs) Handles btnSuggestionNewer.Click
        If suggestionPageIndex > 0 Then
            suggestionPageIndex -= 1
            Call RefreshSuggestionsAsync()
        End If
    End Sub

    Private Sub btnSuggestionOlder_Click(sender As Object, e As EventArgs) Handles btnSuggestionOlder.Click
        suggestionPageIndex += 1
        Call RefreshSuggestionsAsync()
    End Sub

    Private Sub btnSuggestionRefresh_Click(sender As Object, e As EventArgs) Handles btnSuggestionRefresh.Click
        Call RefreshSuggestionsAsync()
    End Sub

    ' -----------------------------------------------------------------
    ' Staff tools: purge (delete) / restore a message from the active feed.
    ' -----------------------------------------------------------------
    Private Async Function PurgeSelectedMessageAsync(listBox As ListBox, channel As String) As Task
        If listBox.SelectedItem Is Nothing Then Return
        Dim msg As ChatMessage = Nothing
        Dim selIndex = listBox.SelectedIndex
        If selIndex >= 0 AndAlso selIndex < currentFeedMessages.Count Then
            msg = currentFeedMessages(selIndex)
        End If
        If msg Is Nothing Then Return

        Dim reason = If(String.IsNullOrWhiteSpace(txtModReason.Text), "Deleted from desktop client", txtModReason.Text)
        If MessageBox.Show($"Purge message #{msg.Id} by @{If(String.IsNullOrEmpty(msg.Username), msg.Sender, msg.Username)}?" & vbCrLf & $"Channel: {channel}", "Confirm Purge",
                           MessageBoxButtons.YesNo, MessageBoxIcon.Warning) = DialogResult.Yes Then
            Await api.ModDeleteMessageAsync(msg.Id, channel, reason)
            Await RefreshActiveFeedAsync()
        End If
    End Function

    Private Async Sub btnGlobalPurge_Click(sender As Object, e As EventArgs) Handles btnGlobalPurge.Click
        Await PurgeSelectedMessageAsync(lstGlobalChat, "public")
    End Sub

    Private Async Sub btnDmPurge_Click(sender As Object, e As EventArgs) Handles btnDmPurge.Click
        Await PurgeSelectedMessageAsync(lstDmChat, "dm")
    End Sub

    Private Async Sub btnTopicPurge_Click(sender As Object, e As EventArgs) Handles btnTopicPurge.Click
        Await PurgeSelectedMessageAsync(lstTopicChat, "topic")
    End Sub

    ' -----------------------------------------------------------------
    ' Change password (menu item)
    ' -----------------------------------------------------------------
    Private Async Sub ChangePasswordToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ChangePasswordToolStripMenuItem.Click
        Using dlg As New ChangePasswordDialog()
            If dlg.ShowDialog(Me) = DialogResult.OK Then
                Dim err = Await api.ChangePasswordAsync(dlg.CurrentPassword, dlg.NewPassword)
                If err Is Nothing Then
                    MessageBox.Show("Password changed successfully.", "Change Password", MessageBoxButtons.OK, MessageBoxIcon.Information)
                Else
                    MessageBox.Show($"Password change failed: {err}", "Change Password", MessageBoxButtons.OK, MessageBoxIcon.Error)
                End If
            End If
        End Using
    End Sub

    Private Sub MainForm_FormClosed(sender As Object, e As FormClosedEventArgs) Handles Me.FormClosed
        api.DisconnectRealtime()
        Application.Exit()
    End Sub

    Private Sub OnNewWindowRequested(sender As Object, e As CoreWebView2NewWindowRequestedEventArgs)
        e.Handled = True
        MessageBox.Show("Action Blocked: Popups are disabled.", "Security Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning)
    End Sub

    Private Sub OnNavigationStarting(sender As Object, e As CoreWebView2NavigationStartingEventArgs)
        Dim AllowedUri As New Uri(api.ServerUrl)
        Dim Target As New Uri(e.Uri)

        If Not String.Equals(Target.Host, AllowedUri.Host, StringComparison.OrdinalIgnoreCase) OrElse
           Not Target.AbsolutePath.StartsWith(AllowedUri.AbsolutePath, StringComparison.OrdinalIgnoreCase) Then
            e.Cancel = True
            MessageBox.Show("External Navigation Blocked.", "Security Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End If
    End Sub

    Private Sub AboutToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles AboutToolStripMenuItem.Click
        AboutBox1.Show()
    End Sub

    Private Sub ExitToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ExitToolStripMenuItem.Click
        Me.Close()
    End Sub

End Class

Public Class ChangePasswordDialog
    Inherits Form

    Public ReadOnly Property CurrentPassword As String
        Get
            Return _txtCurrent.Text
        End Get
    End Property

    Public ReadOnly Property NewPassword As String
        Get
            Return _txtNew.Text
        End Get
    End Property

    Private _txtCurrent As New TextBox() With {
        .Location = New Drawing.Point(10, 30),
        .Size = New Drawing.Size(460, 22),
        .BorderStyle = BorderStyle.FixedSingle,
        .UseSystemPasswordChar = True,
        .PlaceholderText = "Current password..."
    }

    Private _txtNew As New TextBox() With {
        .Location = New Drawing.Point(10, 80),
        .Size = New Drawing.Size(460, 22),
        .BorderStyle = BorderStyle.FixedSingle,
        .UseSystemPasswordChar = True,
        .PlaceholderText = "New password..."
    }

    Private _btnOk As New Button() With {
        .Text = "CHANGE",
        .DialogResult = DialogResult.OK,
        .Location = New Drawing.Point(310, 115),
        .Size = New Drawing.Size(80, 28),
        .FlatStyle = FlatStyle.Flat
    }

    Private _btnCancel As New Button() With {
        .Text = "CANCEL",
        .DialogResult = DialogResult.Cancel,
        .Location = New Drawing.Point(395, 115),
        .Size = New Drawing.Size(75, 28),
        .FlatStyle = FlatStyle.Flat
    }

    Public Sub New()
        Text = "CHANGE PASSWORD"
        ClientSize = New Drawing.Size(480, 155)
        FormBorderStyle = FormBorderStyle.FixedDialog
        MaximizeBox = False
        MinimizeBox = False
        StartPosition = FormStartPosition.CenterParent
        AcceptButton = _btnOk
        CancelButton = _btnCancel
        Controls.AddRange({_txtCurrent, _txtNew, _btnOk, _btnCancel})
    End Sub

    Protected Overrides Sub OnFormClosing(e As FormClosingEventArgs)
        If DialogResult = DialogResult.OK Then
            If String.IsNullOrWhiteSpace(_txtCurrent.Text) OrElse String.IsNullOrWhiteSpace(_txtNew.Text) Then
                MessageBox.Show("Both passwords are required.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                e.Cancel = True
            ElseIf _txtNew.Text.Length < 6 Then
                MessageBox.Show("New password must be at least 6 characters.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                e.Cancel = True
            End If
        End If
        MyBase.OnFormClosing(e)
    End Sub

End Class

Public Class NewPostDialog
    Inherits Form

    Public ReadOnly Property PostTitle As String
        Get
            Return _txtTitle.Text.Trim()
        End Get
    End Property

    Public ReadOnly Property PostContent As String
        Get
            Return _txtContent.Text.Trim()
        End Get
    End Property

    Private _txtTitle As New TextBox() With {
        .Location = New Drawing.Point(10, 30),
        .Size = New Drawing.Size(460, 22),
        .BorderStyle = BorderStyle.FixedSingle,
        .PlaceholderText = "Thread title…"
    }

    Private _txtContent As New TextBox() With {
        .Location = New Drawing.Point(10, 80),
        .Size = New Drawing.Size(460, 120),
        .BorderStyle = BorderStyle.FixedSingle,
        .Multiline = True,
        .ScrollBars = ScrollBars.Vertical,
        .PlaceholderText = "Post body…"
    }

    Private _lblTitle As New Label() With {
        .Text = "TITLE:",
        .Location = New Drawing.Point(10, 10),
        .Size = New Drawing.Size(200, 16)
    }

    Private _lblContent As New Label() With {
        .Text = "CONTENT:",
        .Location = New Drawing.Point(10, 60),
        .Size = New Drawing.Size(200, 16)
    }

    Private _btnOk As New Button() With {
        .Text = "POST",
        .DialogResult = DialogResult.OK,
        .Location = New Drawing.Point(310, 215),
        .Size = New Drawing.Size(80, 28),
        .FlatStyle = FlatStyle.Flat
    }

    Private _btnCancel As New Button() With {
        .Text = "CANCEL",
        .DialogResult = DialogResult.Cancel,
        .Location = New Drawing.Point(395, 215),
        .Size = New Drawing.Size(75, 28),
        .FlatStyle = FlatStyle.Flat
    }

    Public Sub New()
        Text = "CREATE THREAD"
        ClientSize = New Drawing.Size(480, 255)
        FormBorderStyle = FormBorderStyle.FixedDialog
        MaximizeBox = False
        MinimizeBox = False
        StartPosition = FormStartPosition.CenterParent
        AcceptButton = _btnOk
        CancelButton = _btnCancel
        Controls.AddRange({_lblTitle, _txtTitle, _lblContent, _txtContent, _btnOk, _btnCancel})
    End Sub

    Protected Overrides Sub OnFormClosing(e As FormClosingEventArgs)
        If DialogResult = DialogResult.OK Then
            If String.IsNullOrWhiteSpace(_txtTitle.Text) OrElse String.IsNullOrWhiteSpace(_txtContent.Text) Then
                MessageBox.Show("Title and content are required.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                e.Cancel = True
            End If
        End If
        MyBase.OnFormClosing(e)
    End Sub

End Class
