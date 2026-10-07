Imports Gtk
Imports System
Imports System.Threading.Tasks
Imports UnReader.NET.Core

Public Class LoginWindow
    Inherits Window

    Private serverUrl As String
    Private serverUrlEntry As Entry
    Private usernameEntry As Entry
    Private passwordEntry As Entry
    Private rememberPasswordCheck As CheckButton
    Private loginButton As Button
    Private statusLabel As Label

    Public Sub New(defaultServerUrl As String)
        MyBase.New("UnReader.NET Login")

        Dim savedSettings = AppSettings.Current
        serverUrl = defaultServerUrl

        SetDefaultSize(460, 280)
        BorderWidth = 14
        Resizable = False
        WindowPosition = WindowPosition.Center

        AddHandler DeleteEvent, AddressOf OnWindowClosed

        Dim grid = New Grid() With {
            .RowSpacing = 10,
            .ColumnSpacing = 10,
            .ColumnHomogeneous = False
        }

        Add(grid)

        grid.Attach(New Label("Server URL:") With {.Xalign = 0}, 0, 0, 1, 1)
        serverUrlEntry = New Entry() With {.Text = serverUrl}
        grid.Attach(serverUrlEntry, 1, 0, 1, 1)

        grid.Attach(New Label("Username:") With {.Xalign = 0}, 0, 1, 1, 1)
        usernameEntry = New Entry()
        grid.Attach(usernameEntry, 1, 1, 1, 1)

        grid.Attach(New Label("Password:") With {.Xalign = 0}, 0, 2, 1, 1)
        passwordEntry = New Entry() With {.Visibility = False}
        grid.Attach(passwordEntry, 1, 2, 1, 1)

        rememberPasswordCheck = New CheckButton("Remember password in system keyring")
        rememberPasswordCheck.Label = "Remember password securely on this device"
        rememberPasswordCheck.Sensitive = PlatformCredentialStore.IsSupported
        If Not rememberPasswordCheck.Sensitive Then
            rememberPasswordCheck.TooltipText = "Password saving is unavailable on this platform."
        End If
        grid.Attach(rememberPasswordCheck, 1, 3, 1, 1)
        usernameEntry.Text = savedSettings.Username
        If rememberPasswordCheck.Sensitive AndAlso Not String.IsNullOrWhiteSpace(usernameEntry.Text) Then
            passwordEntry.Text = PlatformCredentialStore.ReadPassword(serverUrl, usernameEntry.Text)
            rememberPasswordCheck.Active = Not String.IsNullOrEmpty(passwordEntry.Text)
        End If

        statusLabel = New Label(String.Empty) With {
            .Xalign = 0,
            .Ellipsize = Pango.EllipsizeMode.End
        }
        grid.Attach(statusLabel, 0, 4, 2, 1)

        Dim buttons = New ButtonBox(Orientation.Horizontal) With {
            .Layout = ButtonBoxStyle.End,
            .Spacing = 10
        }
        loginButton = New Button("Login")
        AddHandler loginButton.Clicked, AddressOf OnLoginClicked
        buttons.Add(loginButton)

        Dim cancelButton = New Button("Cancel")
        AddHandler cancelButton.Clicked, Sub(sender, e) Application.Quit()
        buttons.Add(cancelButton)

        grid.Attach(buttons, 0, 5, 2, 1)

        ShowAll()
    End Sub

    Private Sub OnWindowClosed(sender As Object, e As DeleteEventArgs)
        Application.Quit()
    End Sub

    Private Async Sub OnLoginClicked(sender As Object, e As EventArgs)
        loginButton.Sensitive = False
        statusLabel.Text = "Authenticating..."

        Dim username = usernameEntry.Text.Trim()
        Dim password = passwordEntry.Text
        serverUrl = serverUrlEntry.Text.Trim()

        If String.IsNullOrWhiteSpace(serverUrl) OrElse String.IsNullOrWhiteSpace(username) OrElse String.IsNullOrWhiteSpace(password) Then
            statusLabel.Text = "Server URL, username and password are required."
            loginButton.Sensitive = True
            Return
        End If

        Dim reader As New ServerReader(serverUrl)
        Try
            Dim auth = Await reader.LoginAsync(username, password)
            If Not auth.IsSuccess Then
                statusLabel.Text = $"Login failed: {auth.ErrorMessage}"
                loginButton.Sensitive = True
                Return
            End If

            AppSettings.Current.ServerUrl = serverUrl
            AppSettings.Current.Username = username
            AppSettings.Current.Save()
            If rememberPasswordCheck.Active Then
                If Not PlatformCredentialStore.StorePassword(serverUrl, username, password) Then
                    statusLabel.Text = "Login succeeded; the system credential store is unavailable, so the password was not saved."
                End If
            Else
                PlatformCredentialStore.ClearPassword(serverUrl, username)
            End If

            ' Load role flags before creating the role-specific navigation.
            Await reader.GetMeAsync()

            statusLabel.Text = "Login successful. Establishing realtime connection..."
            Dim mainWin = New MainWindow(reader)
            Try
                Await mainWin.InitializeAsync()
                mainWin.ShowAll()
                Hide()
            Catch ex As Exception
                statusLabel.Text = $"Connection failed: {ex.Message}"
                loginButton.Sensitive = True
            End Try
        Catch ex As Exception
            statusLabel.Text = $"Login error: {ex.Message}"
            loginButton.Sensitive = True
        End Try
    End Sub

End Class
