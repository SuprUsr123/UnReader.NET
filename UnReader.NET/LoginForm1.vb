Imports System.ComponentModel
Imports System.Threading.Tasks
Imports UnReader.NET.Core

Public Class LoginForm1

    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property Username As String
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property Password As String

    Private ServerReaderInstance As ServerReader

    Protected Overrides Sub OnLoad(e As EventArgs)
        MyBase.OnLoad(e)

        ' Pre-fill from persisted settings.
        ServerTextBox.Text = AppSettings.Current.ServerUrl
        UsernameTextBox.Text = AppSettings.Current.Username

        ServerReaderInstance = New ServerReader(AppSettings.Current.ServerUrl)
        Form1.api = ServerReaderInstance
    End Sub

    Private Async Sub OK_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles OK.Click

        If String.IsNullOrWhiteSpace(UsernameTextBox.Text) OrElse String.IsNullOrWhiteSpace(PasswordTextBox.Text) Then
            MessageBox.Show("Empty Username/Password!")
            Return
        End If

        If String.IsNullOrWhiteSpace(ServerTextBox.Text) Then
            MessageBox.Show("Server URL is required.")
            Return
        End If

        OK.Enabled = False
        OK.Text = "Authenticating..."

        ' Persist the chosen server + username so future launches remember it.
        AppSettings.Current.ServerUrl = ServerTextBox.Text.Trim()
        AppSettings.Current.Username = UsernameTextBox.Text.Trim()
        AppSettings.Current.Save()

        ServerReaderInstance = New ServerReader(AppSettings.Current.ServerUrl, Form1.api?.JwtToken)
        Form1.api = ServerReaderInstance

        Dim authResult = Await ServerReaderInstance.LoginAsync(UsernameTextBox.Text, PasswordTextBox.Text)

        If authResult.IsSuccess Then

            Dim connectionSuccess = Await ServerReaderInstance.ConnectRealtimeAsync()

            If connectionSuccess Then

                Username = UsernameTextBox.Text
                Password = PasswordTextBox.Text

                ' Load identity/staff flags from the server (safe fallback built in).
                Await ServerReaderInstance.GetMeAsync()

                Me.Hide()
                Form1.Show()
            Else
                MessageBox.Show("Authentication successful, but failed to connect to the real-time server stream.")
                ResetLoginUI()
            End If
        Else
            MessageBox.Show("Authentication Failed: " & authResult.ErrorMessage)
            ResetLoginUI()
        End If
    End Sub

    Private Async Sub RegisterButton_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles RegisterButton.Click
        If String.IsNullOrWhiteSpace(UsernameTextBox.Text) OrElse String.IsNullOrWhiteSpace(PasswordTextBox.Text) Then
            MessageBox.Show("Enter a username and password to register.")
            Return
        End If

        AppSettings.Current.ServerUrl = ServerTextBox.Text.Trim()
        AppSettings.Current.Save()

        ServerReaderInstance = New ServerReader(AppSettings.Current.ServerUrl, Form1.api?.JwtToken)
        Form1.api = ServerReaderInstance

        RegisterButton.Enabled = False
        Try
            Dim result = Await ServerReaderInstance.RegisterAsync(UsernameTextBox.Text, PasswordTextBox.Text)
            If result.IsSuccess Then
                MessageBox.Show("Account created. You are now signed in.")
                Dim connectionSuccess = Await ServerReaderInstance.ConnectRealtimeAsync()
                If connectionSuccess Then
                    Username = UsernameTextBox.Text
                    Password = PasswordTextBox.Text
                    Await ServerReaderInstance.GetMeAsync()
                    Me.Hide()
                    Form1.Show()
                Else
                    MessageBox.Show("Account created, but realtime connection failed.")
                    ResetLoginUI()
                End If
            Else
                MessageBox.Show("Registration failed: " & result.ErrorMessage)
            End If
        Catch ex As Exception
            MessageBox.Show("Registration error: " & ex.Message)
        Finally
            RegisterButton.Enabled = True
        End Try
    End Sub

    Private Sub ResetLoginUI()
        OK.Enabled = True
        OK.Text = "&OK"
    End Sub

    Private Sub Cancel_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Cancel.Click
        Me.Close()
    End Sub

End Class