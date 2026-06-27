Imports System.ComponentModel
Imports System.Threading.Tasks

Public Class LoginForm1

    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property Username As String
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property Password As String

    Private Async Sub OK_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles OK.Click
        ' Basic validation
        If String.IsNullOrWhiteSpace(UsernameTextBox.Text) OrElse String.IsNullOrWhiteSpace(PasswordTextBox.Text) Then
            MessageBox.Show("Empty Username/Password!")
            Return
        End If

        ' Prevent spamming while the network request is processing
        OK.Enabled = False
        OK.Text = "Authenticating..."

        ' 1. Attempt HTTP Login via the API instance on Form1
        Dim authResult = Await Form1.api.LoginAsync(UsernameTextBox.Text, PasswordTextBox.Text)

        If authResult.IsSuccess Then
            ' 2. Start the constant real-time WebSocket reader
            Dim connectionSuccess = Await Form1.api.ConnectRealtimeAsync()

            If connectionSuccess Then
                ' Authentication and Server Stream established. Transition to Main Form.
                Username = UsernameTextBox.Text
                Password = PasswordTextBox.Text

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

    Private Sub ResetLoginUI()
        OK.Enabled = True
        OK.Text = "&OK"
    End Sub

    Private Sub Cancel_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Cancel.Click
        Me.Close()
    End Sub

End Class