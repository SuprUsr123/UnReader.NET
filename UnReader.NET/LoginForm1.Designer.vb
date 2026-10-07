<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
<Global.System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1726")> _
Partial Class LoginForm1
    Inherits System.Windows.Forms.Form


    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub
    Friend WithEvents UsernameLabel As System.Windows.Forms.Label
    Friend WithEvents PasswordLabel As System.Windows.Forms.Label
    Friend WithEvents UsernameTextBox As System.Windows.Forms.TextBox
    Friend WithEvents PasswordTextBox As System.Windows.Forms.TextBox
    Friend WithEvents OK As System.Windows.Forms.Button
    Friend WithEvents Cancel As System.Windows.Forms.Button
    Friend WithEvents ServerLabel As System.Windows.Forms.Label
    Friend WithEvents ServerTextBox As System.Windows.Forms.TextBox
    Friend WithEvents RegisterButton As System.Windows.Forms.Button


    Private components As System.ComponentModel.IContainer




    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        UsernameLabel = New Label()
        PasswordLabel = New Label()
        UsernameTextBox = New TextBox()
        PasswordTextBox = New TextBox()
        OK = New Button()
        Cancel = New Button()
        ServerLabel = New Label()
        ServerTextBox = New TextBox()
        RegisterButton = New Button()
        SuspendLayout()



        ServerLabel.Location = New Point(12, 8)
        ServerLabel.Name = "ServerLabel"
        ServerLabel.Size = New Size(220, 15)
        ServerLabel.TabIndex = 6
        ServerLabel.Text = "&Server URL"
        ServerLabel.TextAlign = ContentAlignment.MiddleLeft



        ServerTextBox.Location = New Point(14, 24)
        ServerTextBox.Name = "ServerTextBox"
        ServerTextBox.Size = New Size(220, 23)
        ServerTextBox.TabIndex = 0
        ServerTextBox.PlaceholderText = "http://localhost:10000"



        UsernameLabel.Location = New Point(12, 50)
        UsernameLabel.Name = "UsernameLabel"
        UsernameLabel.Size = New Size(220, 15)
        UsernameLabel.TabIndex = 4
        UsernameLabel.Text = "&User name"
        UsernameLabel.TextAlign = ContentAlignment.MiddleLeft



        UsernameTextBox.Location = New Point(14, 66)
        UsernameTextBox.Name = "UsernameTextBox"
        UsernameTextBox.Size = New Size(220, 23)
        UsernameTextBox.TabIndex = 1



        PasswordLabel.Location = New Point(12, 92)
        PasswordLabel.Name = "PasswordLabel"
        PasswordLabel.Size = New Size(220, 15)
        PasswordLabel.TabIndex = 5
        PasswordLabel.Text = "&Password"
        PasswordLabel.TextAlign = ContentAlignment.MiddleLeft



        PasswordTextBox.Location = New Point(14, 108)
        PasswordTextBox.Name = "PasswordTextBox"
        PasswordTextBox.PasswordChar = "*"c
        PasswordTextBox.Size = New Size(220, 23)
        PasswordTextBox.TabIndex = 2



        OK.Location = New Point(14, 150)
        OK.Name = "OK"
        OK.Size = New Size(108, 26)
        OK.TabIndex = 3
        OK.Text = "&OK"



        RegisterButton.Location = New Point(126, 150)
        RegisterButton.Name = "RegisterButton"
        RegisterButton.Size = New Size(108, 26)
        RegisterButton.TabIndex = 4
        RegisterButton.Text = "&Register"



        Cancel.DialogResult = DialogResult.Cancel
        Cancel.Location = New Point(14, 180)
        Cancel.Name = "Cancel"
        Cancel.Size = New Size(220, 23)
        Cancel.TabIndex = 5
        Cancel.Text = "&Cancel"



        AcceptButton = OK
        AutoScaleDimensions = New SizeF(7F, 15F)
        AutoScaleMode = AutoScaleMode.Font
        CancelButton = Cancel
        ClientSize = New Size(255, 212)
        Controls.Add(Cancel)
        Controls.Add(RegisterButton)
        Controls.Add(OK)
        Controls.Add(PasswordTextBox)
        Controls.Add(UsernameTextBox)
        Controls.Add(PasswordLabel)
        Controls.Add(UsernameLabel)
        Controls.Add(ServerTextBox)
        Controls.Add(ServerLabel)
        FormBorderStyle = FormBorderStyle.FixedDialog
        MaximizeBox = False
        MinimizeBox = False
        Name = "LoginForm1"
        SizeGripStyle = SizeGripStyle.Hide
        StartPosition = FormStartPosition.CenterParent
        Text = "Sign Into UnReader"
        ResumeLayout(False)
        PerformLayout()

    End Sub

End Class