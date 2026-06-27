<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class Form1
    Inherits System.Windows.Forms.Form
    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    Private components As System.ComponentModel.IContainer

    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        components = New ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(Form1))
        MainTabControl = New TabControl()
        ChatTab = New TabPage()
        lblGlobalOnline = New Label()
        btnGlobalSend = New Button()
        txtGlobalInput = New TextBox()
        lstGlobalOnline = New ListBox()
        lstGlobalChat = New ListBox()
        DMsTab = New TabPage()
        txtDmTargetInput = New TextBox()
        btnDmStart = New Button()
        lblDmConversations = New Label()
        btnDmSend = New Button()
        txtDmInput = New TextBox()
        lstDmChat = New ListBox()
        lstDmConversations = New ListBox()
        Topics = New TabPage()
        txtTopicTargetInput = New TextBox()
        btnTopicCreate = New Button()
        lblTopicsList = New Label()
        btnTopicSend = New Button()
        txtTopicInput = New TextBox()
        lstTopicChat = New ListBox()
        lstTopicsList = New ListBox()
        NeighbourhoodTab = New TabPage()
        lblNeighbourhoodComments = New Label()
        lblNeighbourhoodPosts = New Label()
        btnSubmitComment = New Button()
        btnCreatePost = New Button()
        txtCommentInput = New TextBox()
        txtPostView = New TextBox()
        treeNeighbourhoodComments = New TreeView()
        lstNeighbourhoodPosts = New ListBox()
        ModerationTab = New TabPage()
        txtModLogs = New TextBox()
        btnModRefreshLogs = New Button()
        lblModLogs = New Label()
        txtModUserStatus = New TextBox()
        grpModActions = New GroupBox()
        btnModBan = New Button()
        btnModPardon = New Button()
        btnModSetRole = New Button()
        btnModBanIP = New Button()
        btnModKick = New Button()
        numModTimeout = New NumericUpDown()
        btnModTimeout = New Button()
        lblModReason = New Label()
        txtModReason = New TextBox()
        btnModSearch = New Button()
        txtModSearchUser = New TextBox()
        lblModSearch = New Label()
        MainPageOnlineTab = New TabPage()
        UnReaderWeb = New Microsoft.Web.WebView2.WinForms.WebView2()
        ContextMenuStrip1 = New ContextMenuStrip(components)
        MenuStrip1 = New MenuStrip()
        FileToolStripMenuItem = New ToolStripMenuItem()
        AboutToolStripMenuItem = New ToolStripMenuItem()
        ToolStripSeparator1 = New ToolStripSeparator()
        ExitToolStripMenuItem = New ToolStripMenuItem()
        btnPageNewer = New Button()
        btnPageOlder = New Button()
        lblPageIndex = New Label()
        MainTabControl.SuspendLayout()
        ChatTab.SuspendLayout()
        DMsTab.SuspendLayout()
        Topics.SuspendLayout()
        NeighbourhoodTab.SuspendLayout()
        ModerationTab.SuspendLayout()
        grpModActions.SuspendLayout()
        CType(numModTimeout, ComponentModel.ISupportInitialize).BeginInit()
        MainPageOnlineTab.SuspendLayout()
        CType(UnReaderWeb, ComponentModel.ISupportInitialize).BeginInit()
        MenuStrip1.SuspendLayout()
        SuspendLayout()
        ' 
        ' MainTabControl
        ' 
        MainTabControl.Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
        MainTabControl.Controls.Add(ChatTab)
        MainTabControl.Controls.Add(DMsTab)
        MainTabControl.Controls.Add(Topics)
        MainTabControl.Controls.Add(NeighbourhoodTab)
        MainTabControl.Controls.Add(ModerationTab)
        MainTabControl.Controls.Add(MainPageOnlineTab)
        MainTabControl.Font = New Font("Courier New", 9.75F, FontStyle.Bold)
        MainTabControl.Location = New Point(0, 27)
        MainTabControl.Name = "MainTabControl"
        MainTabControl.SelectedIndex = 0
        MainTabControl.Size = New Size(984, 534)
        MainTabControl.TabIndex = 0
        ' 
        ' ChatTab
        ' 
        ChatTab.Controls.Add(lblGlobalOnline)
        ChatTab.Controls.Add(btnGlobalSend)
        ChatTab.Controls.Add(txtGlobalInput)
        ChatTab.Controls.Add(lstGlobalOnline)
        ChatTab.Controls.Add(lstGlobalChat)
        ChatTab.Location = New Point(4, 25)
        ChatTab.Name = "ChatTab"
        ChatTab.Padding = New Padding(3)
        ChatTab.Size = New Size(976, 505)
        ChatTab.TabIndex = 0
        ChatTab.Text = "GLOBAL CHAT"
        ChatTab.UseVisualStyleBackColor = True
        ' 
        ' lblGlobalOnline
        ' 
        lblGlobalOnline.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        lblGlobalOnline.Location = New Point(760, 10)
        lblGlobalOnline.Name = "lblGlobalOnline"
        lblGlobalOnline.Size = New Size(205, 16)
        lblGlobalOnline.TabIndex = 0
        lblGlobalOnline.Text = "ACTIVE USERS:"
        ' 
        ' btnGlobalSend
        ' 
        btnGlobalSend.Anchor = AnchorStyles.Bottom Or AnchorStyles.Right
        btnGlobalSend.FlatAppearance.BorderColor = Color.Black
        btnGlobalSend.FlatAppearance.BorderSize = 2
        btnGlobalSend.FlatStyle = FlatStyle.Flat
        btnGlobalSend.Location = New Point(850, 467)
        btnGlobalSend.Name = "btnGlobalSend"
        btnGlobalSend.Size = New Size(115, 26)
        btnGlobalSend.TabIndex = 3
        btnGlobalSend.Text = "SEND"
        ' 
        ' txtGlobalInput
        ' 
        txtGlobalInput.Anchor = AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
        txtGlobalInput.BorderStyle = BorderStyle.FixedSingle
        txtGlobalInput.Location = New Point(10, 469)
        txtGlobalInput.Name = "txtGlobalInput"
        txtGlobalInput.Size = New Size(830, 22)
        txtGlobalInput.TabIndex = 2
        ' 
        ' lstGlobalOnline
        ' 
        lstGlobalOnline.Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Right
        lstGlobalOnline.BorderStyle = BorderStyle.FixedSingle
        lstGlobalOnline.Location = New Point(760, 30)
        lstGlobalOnline.Name = "lstGlobalOnline"
        lstGlobalOnline.Size = New Size(205, 418)
        lstGlobalOnline.TabIndex = 1
        ' 
        ' lstGlobalChat
        ' 
        lstGlobalChat.Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
        lstGlobalChat.BorderStyle = BorderStyle.FixedSingle
        lstGlobalChat.Location = New Point(10, 10)
        lstGlobalChat.Name = "lstGlobalChat"
        lstGlobalChat.Size = New Size(740, 434)
        lstGlobalChat.TabIndex = 0
        ' 
        ' DMsTab
        ' 
        DMsTab.Controls.Add(txtDmTargetInput)
        DMsTab.Controls.Add(btnDmStart)
        DMsTab.Controls.Add(lblDmConversations)
        DMsTab.Controls.Add(btnDmSend)
        DMsTab.Controls.Add(txtDmInput)
        DMsTab.Controls.Add(lstDmChat)
        DMsTab.Controls.Add(lstDmConversations)
        DMsTab.Location = New Point(4, 25)
        DMsTab.Name = "DMsTab"
        DMsTab.Padding = New Padding(3)
        DMsTab.Size = New Size(976, 505)
        DMsTab.TabIndex = 5
        DMsTab.Text = "DIRECT MESSAGES"
        DMsTab.UseVisualStyleBackColor = True
        ' 
        ' txtDmTargetInput
        ' 
        txtDmTargetInput.Anchor = AnchorStyles.Bottom Or AnchorStyles.Left
        txtDmTargetInput.BorderStyle = BorderStyle.FixedSingle
        txtDmTargetInput.Location = New Point(10, 435)
        txtDmTargetInput.Name = "txtDmTargetInput"
        txtDmTargetInput.Size = New Size(125, 22)
        txtDmTargetInput.TabIndex = 0
        ' 
        ' btnDmStart
        ' 
        btnDmStart.Anchor = AnchorStyles.Bottom Or AnchorStyles.Left
        btnDmStart.FlatAppearance.BorderColor = Color.Black
        btnDmStart.FlatAppearance.BorderSize = 2
        btnDmStart.FlatStyle = FlatStyle.Flat
        btnDmStart.Location = New Point(140, 433)
        btnDmStart.Name = "btnDmStart"
        btnDmStart.Size = New Size(70, 26)
        btnDmStart.TabIndex = 1
        btnDmStart.Text = "ADD"
        ' 
        ' lblDmConversations
        ' 
        lblDmConversations.Location = New Point(10, 10)
        lblDmConversations.Name = "lblDmConversations"
        lblDmConversations.Size = New Size(200, 16)
        lblDmConversations.TabIndex = 2
        lblDmConversations.Text = "CONTACTS:"
        ' 
        ' btnDmSend
        ' 
        btnDmSend.Anchor = AnchorStyles.Bottom Or AnchorStyles.Right
        btnDmSend.FlatAppearance.BorderColor = Color.Black
        btnDmSend.FlatAppearance.BorderSize = 2
        btnDmSend.FlatStyle = FlatStyle.Flat
        btnDmSend.Location = New Point(850, 467)
        btnDmSend.Name = "btnDmSend"
        btnDmSend.Size = New Size(115, 26)
        btnDmSend.TabIndex = 3
        btnDmSend.Text = "SEND"
        ' 
        ' txtDmInput
        ' 
        txtDmInput.Anchor = AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
        txtDmInput.BorderStyle = BorderStyle.FixedSingle
        txtDmInput.Location = New Point(10, 469)
        txtDmInput.Name = "txtDmInput"
        txtDmInput.Size = New Size(830, 22)
        txtDmInput.TabIndex = 2
        ' 
        ' lstDmChat
        ' 
        lstDmChat.Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
        lstDmChat.BorderStyle = BorderStyle.FixedSingle
        lstDmChat.Location = New Point(220, 10)
        lstDmChat.Name = "lstDmChat"
        lstDmChat.Size = New Size(745, 434)
        lstDmChat.TabIndex = 1
        ' 
        ' lstDmConversations
        ' 
        lstDmConversations.Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left
        lstDmConversations.BorderStyle = BorderStyle.FixedSingle
        lstDmConversations.Location = New Point(10, 30)
        lstDmConversations.Name = "lstDmConversations"
        lstDmConversations.Size = New Size(200, 386)
        lstDmConversations.TabIndex = 0
        ' 
        ' Topics
        ' 
        Topics.Controls.Add(txtTopicTargetInput)
        Topics.Controls.Add(btnTopicCreate)
        Topics.Controls.Add(lblTopicsList)
        Topics.Controls.Add(btnTopicSend)
        Topics.Controls.Add(txtTopicInput)
        Topics.Controls.Add(lstTopicChat)
        Topics.Controls.Add(lstTopicsList)
        Topics.Location = New Point(4, 25)
        Topics.Name = "Topics"
        Topics.Padding = New Padding(3)
        Topics.Size = New Size(976, 505)
        Topics.TabIndex = 1
        Topics.Text = "TOPICS"
        Topics.UseVisualStyleBackColor = True
        ' 
        ' txtTopicTargetInput
        ' 
        txtTopicTargetInput.Anchor = AnchorStyles.Bottom Or AnchorStyles.Left
        txtTopicTargetInput.BorderStyle = BorderStyle.FixedSingle
        txtTopicTargetInput.Location = New Point(10, 435)
        txtTopicTargetInput.Name = "txtTopicTargetInput"
        txtTopicTargetInput.Size = New Size(125, 22)
        txtTopicTargetInput.TabIndex = 0
        ' 
        ' btnTopicCreate
        ' 
        btnTopicCreate.Anchor = AnchorStyles.Bottom Or AnchorStyles.Left
        btnTopicCreate.FlatAppearance.BorderColor = Color.Black
        btnTopicCreate.FlatAppearance.BorderSize = 2
        btnTopicCreate.FlatStyle = FlatStyle.Flat
        btnTopicCreate.Location = New Point(140, 433)
        btnTopicCreate.Name = "btnTopicCreate"
        btnTopicCreate.Size = New Size(70, 26)
        btnTopicCreate.TabIndex = 1
        btnTopicCreate.Text = "NEW"
        ' 
        ' lblTopicsList
        ' 
        lblTopicsList.Location = New Point(10, 10)
        lblTopicsList.Name = "lblTopicsList"
        lblTopicsList.Size = New Size(200, 16)
        lblTopicsList.TabIndex = 2
        lblTopicsList.Text = "CHANNELS:"
        ' 
        ' btnTopicSend
        ' 
        btnTopicSend.Anchor = AnchorStyles.Bottom Or AnchorStyles.Right
        btnTopicSend.FlatAppearance.BorderColor = Color.Black
        btnTopicSend.FlatAppearance.BorderSize = 2
        btnTopicSend.FlatStyle = FlatStyle.Flat
        btnTopicSend.Location = New Point(850, 467)
        btnTopicSend.Name = "btnTopicSend"
        btnTopicSend.Size = New Size(115, 26)
        btnTopicSend.TabIndex = 3
        btnTopicSend.Text = "SEND"
        ' 
        ' txtTopicInput
        ' 
        txtTopicInput.Anchor = AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
        txtTopicInput.BorderStyle = BorderStyle.FixedSingle
        txtTopicInput.Location = New Point(10, 469)
        txtTopicInput.Name = "txtTopicInput"
        txtTopicInput.Size = New Size(830, 22)
        txtTopicInput.TabIndex = 2
        ' 
        ' lstTopicChat
        ' 
        lstTopicChat.Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
        lstTopicChat.BorderStyle = BorderStyle.FixedSingle
        lstTopicChat.Location = New Point(220, 10)
        lstTopicChat.Name = "lstTopicChat"
        lstTopicChat.Size = New Size(745, 434)
        lstTopicChat.TabIndex = 1
        ' 
        ' lstTopicsList
        ' 
        lstTopicsList.Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left
        lstTopicsList.BorderStyle = BorderStyle.FixedSingle
        lstTopicsList.Location = New Point(10, 30)
        lstTopicsList.Name = "lstTopicsList"
        lstTopicsList.Size = New Size(200, 386)
        lstTopicsList.TabIndex = 0
        ' 
        ' NeighbourhoodTab
        ' 
        NeighbourhoodTab.Controls.Add(lblNeighbourhoodComments)
        NeighbourhoodTab.Controls.Add(lblNeighbourhoodPosts)
        NeighbourhoodTab.Controls.Add(btnSubmitComment)
        NeighbourhoodTab.Controls.Add(btnCreatePost)
        NeighbourhoodTab.Controls.Add(txtCommentInput)
        NeighbourhoodTab.Controls.Add(txtPostView)
        NeighbourhoodTab.Controls.Add(treeNeighbourhoodComments)
        NeighbourhoodTab.Controls.Add(lstNeighbourhoodPosts)
        NeighbourhoodTab.Location = New Point(4, 25)
        NeighbourhoodTab.Name = "NeighbourhoodTab"
        NeighbourhoodTab.Padding = New Padding(3)
        NeighbourhoodTab.Size = New Size(976, 505)
        NeighbourhoodTab.TabIndex = 2
        NeighbourhoodTab.Text = "NEIGHBOURHOOD"
        NeighbourhoodTab.UseVisualStyleBackColor = True
        ' 
        ' lblNeighbourhoodComments
        ' 
        lblNeighbourhoodComments.Location = New Point(420, 10)
        lblNeighbourhoodComments.Name = "lblNeighbourhoodComments"
        lblNeighbourhoodComments.Size = New Size(545, 16)
        lblNeighbourhoodComments.TabIndex = 0
        lblNeighbourhoodComments.Text = "NESTED COMMENT TREE:"
        ' 
        ' lblNeighbourhoodPosts
        ' 
        lblNeighbourhoodPosts.Location = New Point(10, 10)
        lblNeighbourhoodPosts.Name = "lblNeighbourhoodPosts"
        lblNeighbourhoodPosts.Size = New Size(400, 16)
        lblNeighbourhoodPosts.TabIndex = 1
        lblNeighbourhoodPosts.Text = "POST BOARD ROOTS:"
        ' 
        ' btnSubmitComment
        ' 
        btnSubmitComment.Anchor = AnchorStyles.Bottom Or AnchorStyles.Right
        btnSubmitComment.FlatAppearance.BorderColor = Color.Black
        btnSubmitComment.FlatAppearance.BorderSize = 2
        btnSubmitComment.FlatStyle = FlatStyle.Flat
        btnSubmitComment.Location = New Point(850, 467)
        btnSubmitComment.Name = "btnSubmitComment"
        btnSubmitComment.Size = New Size(115, 26)
        btnSubmitComment.TabIndex = 5
        btnSubmitComment.Text = "REPLY"
        ' 
        ' btnCreatePost
        ' 
        btnCreatePost.Anchor = AnchorStyles.Bottom Or AnchorStyles.Left
        btnCreatePost.FlatAppearance.BorderColor = Color.Black
        btnCreatePost.FlatAppearance.BorderSize = 2
        btnCreatePost.FlatStyle = FlatStyle.Flat
        btnCreatePost.Location = New Point(10, 225)
        btnCreatePost.Name = "btnCreatePost"
        btnCreatePost.Size = New Size(400, 30)
        btnCreatePost.TabIndex = 1
        btnCreatePost.Text = "[+] CREATE NEW THREAD"
        ' 
        ' txtCommentInput
        ' 
        txtCommentInput.Anchor = AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
        txtCommentInput.BorderStyle = BorderStyle.FixedSingle
        txtCommentInput.Location = New Point(420, 469)
        txtCommentInput.Name = "txtCommentInput"
        txtCommentInput.Size = New Size(420, 22)
        txtCommentInput.TabIndex = 4
        ' 
        ' txtPostView
        ' 
        txtPostView.Anchor = AnchorStyles.Bottom Or AnchorStyles.Left
        txtPostView.BorderStyle = BorderStyle.FixedSingle
        txtPostView.Location = New Point(10, 265)
        txtPostView.Multiline = True
        txtPostView.Name = "txtPostView"
        txtPostView.ReadOnly = True
        txtPostView.ScrollBars = ScrollBars.Vertical
        txtPostView.Size = New Size(400, 230)
        txtPostView.TabIndex = 2
        ' 
        ' treeNeighbourhoodComments
        ' 
        treeNeighbourhoodComments.Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
        treeNeighbourhoodComments.BorderStyle = BorderStyle.FixedSingle
        treeNeighbourhoodComments.Location = New Point(420, 30)
        treeNeighbourhoodComments.Name = "treeNeighbourhoodComments"
        treeNeighbourhoodComments.Size = New Size(545, 425)
        treeNeighbourhoodComments.TabIndex = 3
        ' 
        ' lstNeighbourhoodPosts
        ' 
        lstNeighbourhoodPosts.Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left
        lstNeighbourhoodPosts.BorderStyle = BorderStyle.FixedSingle
        lstNeighbourhoodPosts.Location = New Point(10, 30)
        lstNeighbourhoodPosts.Name = "lstNeighbourhoodPosts"
        lstNeighbourhoodPosts.Size = New Size(400, 178)
        lstNeighbourhoodPosts.TabIndex = 0
        ' 
        ' ModerationTab
        ' 
        ModerationTab.Controls.Add(txtModLogs)
        ModerationTab.Controls.Add(btnModRefreshLogs)
        ModerationTab.Controls.Add(lblModLogs)
        ModerationTab.Controls.Add(txtModUserStatus)
        ModerationTab.Controls.Add(grpModActions)
        ModerationTab.Controls.Add(btnModSearch)
        ModerationTab.Controls.Add(txtModSearchUser)
        ModerationTab.Controls.Add(lblModSearch)
        ModerationTab.Location = New Point(4, 25)
        ModerationTab.Name = "ModerationTab"
        ModerationTab.Padding = New Padding(3)
        ModerationTab.Size = New Size(976, 505)
        ModerationTab.TabIndex = 3
        ModerationTab.Text = "MODERATION"
        ModerationTab.UseVisualStyleBackColor = True
        ' 
        ' txtModLogs
        ' 
        txtModLogs.Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
        txtModLogs.BorderStyle = BorderStyle.FixedSingle
        txtModLogs.Location = New Point(445, 45)
        txtModLogs.Multiline = True
        txtModLogs.Name = "txtModLogs"
        txtModLogs.ReadOnly = True
        txtModLogs.ScrollBars = ScrollBars.Vertical
        txtModLogs.Size = New Size(520, 450)
        txtModLogs.TabIndex = 4
        ' 
        ' btnModRefreshLogs
        ' 
        btnModRefreshLogs.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        btnModRefreshLogs.FlatAppearance.BorderColor = Color.Black
        btnModRefreshLogs.FlatAppearance.BorderSize = 2
        btnModRefreshLogs.FlatStyle = FlatStyle.Flat
        btnModRefreshLogs.Location = New Point(850, 10)
        btnModRefreshLogs.Name = "btnModRefreshLogs"
        btnModRefreshLogs.Size = New Size(115, 26)
        btnModRefreshLogs.TabIndex = 5
        btnModRefreshLogs.Text = "REFRESH"
        ' 
        ' lblModLogs
        ' 
        lblModLogs.Location = New Point(445, 15)
        lblModLogs.Name = "lblModLogs"
        lblModLogs.Size = New Size(385, 16)
        lblModLogs.TabIndex = 6
        lblModLogs.Text = "SYSTEM AUDIT LOGS:"
        ' 
        ' txtModUserStatus
        ' 
        txtModUserStatus.Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left
        txtModUserStatus.BorderStyle = BorderStyle.FixedSingle
        txtModUserStatus.Location = New Point(10, 290)
        txtModUserStatus.Multiline = True
        txtModUserStatus.Name = "txtModUserStatus"
        txtModUserStatus.ReadOnly = True
        txtModUserStatus.ScrollBars = ScrollBars.Vertical
        txtModUserStatus.Size = New Size(425, 205)
        txtModUserStatus.TabIndex = 3
        ' 
        ' grpModActions
        ' 
        grpModActions.Controls.Add(btnModBan)
        grpModActions.Controls.Add(btnModPardon)
        grpModActions.Controls.Add(btnModSetRole)
        grpModActions.Controls.Add(btnModBanIP)
        grpModActions.Controls.Add(btnModKick)
        grpModActions.Controls.Add(numModTimeout)
        grpModActions.Controls.Add(btnModTimeout)
        grpModActions.Controls.Add(lblModReason)
        grpModActions.Controls.Add(txtModReason)
        grpModActions.Location = New Point(10, 45)
        grpModActions.Name = "grpModActions"
        grpModActions.Size = New Size(425, 230)
        grpModActions.TabIndex = 2
        grpModActions.TabStop = False
        grpModActions.Text = "COMMAND MATRIX"
        ' 
        ' btnModBan
        ' 
        btnModBan.FlatAppearance.BorderColor = Color.Black
        btnModBan.FlatAppearance.BorderSize = 2
        btnModBan.FlatStyle = FlatStyle.Flat
        btnModBan.Location = New Point(15, 25)
        btnModBan.Name = "btnModBan"
        btnModBan.Size = New Size(190, 35)
        btnModBan.TabIndex = 0
        btnModBan.Text = "BAN TARGET"
        ' 
        ' btnModPardon
        ' 
        btnModPardon.FlatAppearance.BorderColor = Color.Black
        btnModPardon.FlatAppearance.BorderSize = 2
        btnModPardon.FlatStyle = FlatStyle.Flat
        btnModPardon.Location = New Point(215, 25)
        btnModPardon.Name = "btnModPardon"
        btnModPardon.Size = New Size(190, 35)
        btnModPardon.TabIndex = 1
        btnModPardon.Text = "PARDON ACCOUNT"
        ' 
        ' btnModSetRole
        ' 
        btnModSetRole.FlatAppearance.BorderColor = Color.Black
        btnModSetRole.FlatAppearance.BorderSize = 2
        btnModSetRole.FlatStyle = FlatStyle.Flat
        btnModSetRole.Location = New Point(15, 70)
        btnModSetRole.Name = "btnModSetRole"
        btnModSetRole.Size = New Size(190, 35)
        btnModSetRole.TabIndex = 2
        btnModSetRole.Text = "TOGGLE PROMOTION"
        ' 
        ' btnModBanIP
        ' 
        btnModBanIP.FlatAppearance.BorderColor = Color.Black
        btnModBanIP.FlatAppearance.BorderSize = 2
        btnModBanIP.FlatStyle = FlatStyle.Flat
        btnModBanIP.Location = New Point(215, 70)
        btnModBanIP.Name = "btnModBanIP"
        btnModBanIP.Size = New Size(190, 35)
        btnModBanIP.TabIndex = 3
        btnModBanIP.Text = "PURGE NETWORK IP"
        ' 
        ' btnModKick
        ' 
        btnModKick.BackColor = Color.FromArgb(CByte(237), CByte(66), CByte(69))
        btnModKick.FlatAppearance.BorderColor = Color.Black
        btnModKick.FlatAppearance.BorderSize = 2
        btnModKick.FlatStyle = FlatStyle.Flat
        btnModKick.ForeColor = Color.White
        btnModKick.Location = New Point(15, 115)
        btnModKick.Name = "btnModKick"
        btnModKick.Size = New Size(190, 35)
        btnModKick.TabIndex = 4
        btnModKick.Text = "KICK USER"
        btnModKick.UseVisualStyleBackColor = False
        ' 
        ' numModTimeout
        ' 
        numModTimeout.BorderStyle = BorderStyle.FixedSingle
        numModTimeout.Location = New Point(215, 122)
        numModTimeout.Maximum = New Decimal(New Integer() {43200, 0, 0, 0})
        numModTimeout.Minimum = New Decimal(New Integer() {1, 0, 0, 0})
        numModTimeout.Name = "numModTimeout"
        numModTimeout.Size = New Size(50, 22)
        numModTimeout.TabIndex = 5
        numModTimeout.Value = New Decimal(New Integer() {60, 0, 0, 0})
        ' 
        ' btnModTimeout
        ' 
        btnModTimeout.FlatAppearance.BorderColor = Color.Black
        btnModTimeout.FlatAppearance.BorderSize = 2
        btnModTimeout.FlatStyle = FlatStyle.Flat
        btnModTimeout.Location = New Point(275, 115)
        btnModTimeout.Name = "btnModTimeout"
        btnModTimeout.Size = New Size(130, 35)
        btnModTimeout.TabIndex = 5
        btnModTimeout.Text = "TIMEOUT (MINS)"
        ' 
        ' lblModReason
        ' 
        lblModReason.Location = New Point(15, 165)
        lblModReason.Name = "lblModReason"
        lblModReason.Size = New Size(390, 16)
        lblModReason.TabIndex = 6
        lblModReason.Text = "OPERATION LOG REASON:"
        ' 
        ' txtModReason
        ' 
        txtModReason.BorderStyle = BorderStyle.FixedSingle
        txtModReason.Location = New Point(15, 185)
        txtModReason.Name = "txtModReason"
        txtModReason.Size = New Size(390, 22)
        txtModReason.TabIndex = 6
        ' 
        ' btnModSearch
        ' 
        btnModSearch.FlatAppearance.BorderColor = Color.Black
        btnModSearch.FlatAppearance.BorderSize = 2
        btnModSearch.FlatStyle = FlatStyle.Flat
        btnModSearch.Location = New Point(340, 10)
        btnModSearch.Name = "btnModSearch"
        btnModSearch.Size = New Size(95, 26)
        btnModSearch.TabIndex = 1
        btnModSearch.Text = "QUERY"
        ' 
        ' txtModSearchUser
        ' 
        txtModSearchUser.BorderStyle = BorderStyle.FixedSingle
        txtModSearchUser.Location = New Point(140, 12)
        txtModSearchUser.Name = "txtModSearchUser"
        txtModSearchUser.Size = New Size(190, 22)
        txtModSearchUser.TabIndex = 0
        ' 
        ' lblModSearch
        ' 
        lblModSearch.Location = New Point(10, 15)
        lblModSearch.Name = "lblModSearch"
        lblModSearch.Size = New Size(130, 16)
        lblModSearch.TabIndex = 7
        lblModSearch.Text = "TARGET HANDLE:"
        ' 
        ' MainPageOnlineTab
        ' 
        MainPageOnlineTab.Controls.Add(UnReaderWeb)
        MainPageOnlineTab.Location = New Point(4, 25)
        MainPageOnlineTab.Name = "MainPageOnlineTab"
        MainPageOnlineTab.Padding = New Padding(3)
        MainPageOnlineTab.Size = New Size(976, 505)
        MainPageOnlineTab.TabIndex = 4
        MainPageOnlineTab.Text = "MAIN PAGE"
        MainPageOnlineTab.UseVisualStyleBackColor = True
        ' 
        ' UnReaderWeb
        ' 
        UnReaderWeb.AllowExternalDrop = True
        UnReaderWeb.CreationProperties = Nothing
        UnReaderWeb.DefaultBackgroundColor = Color.White
        UnReaderWeb.Dock = DockStyle.Fill
        UnReaderWeb.Location = New Point(3, 3)
        UnReaderWeb.Name = "UnReaderWeb"
        UnReaderWeb.Size = New Size(970, 499)
        UnReaderWeb.TabIndex = 0
        UnReaderWeb.ZoomFactor = 1.0R
        ' 
        ' ContextMenuStrip1
        ' 
        ContextMenuStrip1.Name = "ContextMenuStrip1"
        ContextMenuStrip1.Size = New Size(61, 4)
        ' 
        ' MenuStrip1
        ' 
        MenuStrip1.Items.AddRange(New ToolStripItem() {FileToolStripMenuItem})
        MenuStrip1.Location = New Point(0, 0)
        MenuStrip1.Name = "MenuStrip1"
        MenuStrip1.Size = New Size(984, 24)
        MenuStrip1.TabIndex = 3
        MenuStrip1.Text = "MenuStrip1"
        ' 
        ' FileToolStripMenuItem
        ' 
        FileToolStripMenuItem.DropDownItems.AddRange(New ToolStripItem() {AboutToolStripMenuItem, ToolStripSeparator1, ExitToolStripMenuItem})
        FileToolStripMenuItem.Name = "FileToolStripMenuItem"
        FileToolStripMenuItem.Size = New Size(37, 20)
        FileToolStripMenuItem.Text = "File"
        ' 
        ' AboutToolStripMenuItem
        ' 
        AboutToolStripMenuItem.Name = "AboutToolStripMenuItem"
        AboutToolStripMenuItem.Size = New Size(107, 22)
        AboutToolStripMenuItem.Text = "About"
        ' 
        ' ToolStripSeparator1
        ' 
        ToolStripSeparator1.Name = "ToolStripSeparator1"
        ToolStripSeparator1.Size = New Size(104, 6)
        ' 
        ' ExitToolStripMenuItem
        ' 
        ExitToolStripMenuItem.Name = "ExitToolStripMenuItem"
        ExitToolStripMenuItem.Size = New Size(107, 22)
        ExitToolStripMenuItem.Text = "Exit"
        ' 
        ' btnPageNewer
        ' 
        btnPageNewer.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        btnPageNewer.FlatStyle = FlatStyle.Flat
        btnPageNewer.Location = New Point(680, 2)
        btnPageNewer.Name = "btnPageNewer"
        btnPageNewer.Size = New Size(90, 22)
        btnPageNewer.TabIndex = 1
        btnPageNewer.Text = "< NEWER"
        ' 
        ' btnPageOlder
        ' 
        btnPageOlder.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        btnPageOlder.FlatStyle = FlatStyle.Flat
        btnPageOlder.Location = New Point(880, 2)
        btnPageOlder.Name = "btnPageOlder"
        btnPageOlder.Size = New Size(90, 22)
        btnPageOlder.TabIndex = 3
        btnPageOlder.Text = "OLDER >"
        ' 
        ' lblPageIndex
        ' 
        lblPageIndex.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        lblPageIndex.Location = New Point(775, 5)
        lblPageIndex.Name = "lblPageIndex"
        lblPageIndex.Size = New Size(100, 16)
        lblPageIndex.TabIndex = 2
        lblPageIndex.Text = "PAGE: 0"
        lblPageIndex.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' Form1
        ' 
        AutoScaleDimensions = New SizeF(7.0F, 15.0F)
        AutoScaleMode = AutoScaleMode.Font
        BackColor = Color.White
        ClientSize = New Size(984, 561)
        Controls.Add(btnPageNewer)
        Controls.Add(lblPageIndex)
        Controls.Add(btnPageOlder)
        Controls.Add(MenuStrip1)
        Controls.Add(MainTabControl)
        Icon = CType(resources.GetObject("$this.Icon"), Icon)
        MainMenuStrip = MenuStrip1
        MinimumSize = New Size(800, 500)
        Name = "Form1"
        Text = "UnReader.NET"
        MainTabControl.ResumeLayout(False)
        ChatTab.ResumeLayout(False)
        ChatTab.PerformLayout()
        DMsTab.ResumeLayout(False)
        DMsTab.PerformLayout()
        Topics.ResumeLayout(False)
        Topics.PerformLayout()
        NeighbourhoodTab.ResumeLayout(False)
        NeighbourhoodTab.PerformLayout()
        ModerationTab.ResumeLayout(False)
        ModerationTab.PerformLayout()
        grpModActions.ResumeLayout(False)
        grpModActions.PerformLayout()
        CType(numModTimeout, ComponentModel.ISupportInitialize).EndInit()
        MainPageOnlineTab.ResumeLayout(False)
        CType(UnReaderWeb, ComponentModel.ISupportInitialize).EndInit()
        MenuStrip1.ResumeLayout(False)
        MenuStrip1.PerformLayout()
        ResumeLayout(False)
        PerformLayout()
    End Sub

    Friend WithEvents MainTabControl As TabControl
    Friend WithEvents ChatTab As TabPage
    Friend WithEvents Topics As TabPage
    Friend WithEvents NeighbourhoodTab As TabPage
    Friend WithEvents ModerationTab As TabPage
    Friend WithEvents MainPageOnlineTab As TabPage
    Friend WithEvents DMsTab As TabPage
    Friend WithEvents UnReaderWeb As Microsoft.Web.WebView2.WinForms.WebView2
    Friend WithEvents ContextMenuStrip1 As ContextMenuStrip
    Friend WithEvents MenuStrip1 As MenuStrip
    Friend WithEvents FileToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents AboutToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents ToolStripSeparator1 As ToolStripSeparator
    Friend WithEvents ExitToolStripMenuItem As ToolStripMenuItem

    Friend WithEvents btnPageNewer As Button
    Friend WithEvents btnPageOlder As Button
    Friend WithEvents lblPageIndex As Label

    Friend WithEvents lstGlobalChat As ListBox
    Friend WithEvents lstGlobalOnline As ListBox
    Friend WithEvents txtGlobalInput As TextBox
    Friend WithEvents btnGlobalSend As Button
    Friend WithEvents lblGlobalOnline As Label

    Friend WithEvents lstDmConversations As ListBox
    Friend WithEvents lstDmChat As ListBox
    Friend WithEvents txtDmInput As TextBox
    Friend WithEvents btnDmSend As Button
    Friend WithEvents lblDmConversations As Label
    Friend WithEvents txtDmTargetInput As TextBox
    Friend WithEvents btnDmStart As Button

    Friend WithEvents lstTopicsList As ListBox
    Friend WithEvents lstTopicChat As ListBox
    Friend WithEvents txtTopicInput As TextBox
    Friend WithEvents btnTopicSend As Button
    Friend WithEvents lblTopicsList As Label
    Friend WithEvents txtTopicTargetInput As TextBox
    Friend WithEvents btnTopicCreate As Button

    Friend WithEvents lstNeighbourhoodPosts As ListBox
    Friend WithEvents treeNeighbourhoodComments As TreeView
    Friend WithEvents txtPostView As TextBox
    Friend WithEvents txtCommentInput As TextBox
    Friend WithEvents btnCreatePost As Button
    Friend WithEvents btnSubmitComment As Button
    Friend WithEvents lblNeighbourhoodPosts As Label
    Friend WithEvents lblNeighbourhoodComments As Label

    Friend WithEvents lblModSearch As Label
    Friend WithEvents txtModSearchUser As TextBox
    Friend WithEvents btnModSearch As Button
    Friend WithEvents grpModActions As GroupBox
    Friend WithEvents btnModBan As Button
    Friend WithEvents btnModPardon As Button
    Friend WithEvents btnModSetRole As Button
    Friend WithEvents btnModBanIP As Button
    Friend WithEvents btnModKick As Button
    Friend WithEvents numModTimeout As NumericUpDown
    Friend WithEvents btnModTimeout As Button
    Friend WithEvents txtModReason As TextBox
    Friend WithEvents lblModReason As Label
    Friend WithEvents txtModUserStatus As TextBox
    Friend WithEvents lblModLogs As Label
    Friend WithEvents txtModLogs As TextBox
    Friend WithEvents btnModRefreshLogs As Button
End Class