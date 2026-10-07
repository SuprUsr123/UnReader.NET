Public NotInheritable Class AboutBox1
    Private Sub AboutBox1_Load(ByVal sender As Object, ByVal e As EventArgs) Handles MyBase.Load
        Dim versionAttribute = System.Reflection.CustomAttributeExtensions.GetCustomAttribute(Of System.Reflection.AssemblyInformationalVersionAttribute)(GetType(AboutBox1).Assembly)
        LabelVersion.Text = $"Version {If(versionAttribute?.InformationalVersion, "1.0.0") }"
    End Sub

    Private Sub OKButton_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles OKButton.Click
        Me.Close()
    End Sub

End Class
