Imports Gtk
Imports System
Imports System.Threading.Tasks
Imports UnReader.NET.Core

Public Class LoginWindow
    Inherits Window

    Private ReadOnly serverUrl As String
    Private ReadOnly jwtSecret As String
    Private usernameEntry As Entry
    Private passwordEntry As Entry
    Private loginButton As Button
    Private statusLabel As Label

    Public Sub New(serverUrl As String, jwtSecret As String)
        MyBase.New("UnReader.NET Login")

        Me.serverUrl = serverUrl
        Me.jwtSecret = jwtSecret

        SetDefaultSize(460, 240)
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
        Dim serverLabel = New Label(serverUrl) With {.Xalign = 0}
        grid.Attach(serverLabel, 1, 0, 1, 1)

        grid.Attach(New Label("Username:") With {.Xalign = 0}, 0, 1, 1, 1)
        usernameEntry = New Entry()
        grid.Attach(usernameEntry, 1, 1, 1, 1)

        grid.Attach(New Label("Password:") With {.Xalign = 0}, 0, 2, 1, 1)
        passwordEntry = New Entry() With {.Visibility = False}
        grid.Attach(passwordEntry, 1, 2, 1, 1)

        statusLabel = New Label(String.Empty) With {
            .Xalign = 0,
            .Ellipsize = Pango.EllipsizeMode.End
        }
        grid.Attach(statusLabel, 0, 3, 2, 1)

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

        grid.Attach(buttons, 0, 4, 2, 1)

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

        If String.IsNullOrWhiteSpace(username) OrElse String.IsNullOrWhiteSpace(password) Then
            statusLabel.Text = "Username and password are required."
            loginButton.Sensitive = True
            Return
        End If

        Dim reader As New ServerReader(serverUrl, jwtSecret)
        Try
            Dim auth = Await reader.LoginAsync(username, password)
            If Not auth.IsSuccess Then
                statusLabel.Text = $"Login failed: {auth.ErrorMessage}"
                loginButton.Sensitive = True
                Return
            End If

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
