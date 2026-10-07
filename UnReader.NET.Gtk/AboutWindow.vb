Imports Gtk
Imports System.IO
Imports System.Reflection

Public Class AboutWindow
    Inherits Dialog

    Public Sub New()
        MyBase.New("About", Nothing, DialogFlags.Modal)
        SetDefaultSize(620, 340)
        Resizable = False
        Title = "About"

        AddButton("OK", ResponseType.Ok)

        Dim topBox As New HBox(False, 10)

        Dim logo As Widget
        Try
            Dim logoFile As String = System.IO.Path.Combine(AppContext.BaseDirectory, "Resources", "testbanner.png")
            If File.Exists(logoFile) Then
                logo = New Image(logoFile)
            Else
                logo = New Image("gtk-info")
            End If
        Catch
            logo = New Image("gtk-info")
        End Try
        logo.SetSizeRequest(190, 120)
        topBox.PackStart(logo, False, False, 0)

        Dim rightBox As New VBox(False, 6)
        Dim titleLabel As New Label("<b>UnReader.NET</b>") With {
            .UseMarkup = True,
            .Xalign = 0
        }
        rightBox.PackStart(titleLabel, False, False, 0)

        Dim appVersion = GetType(AboutWindow).Assembly.GetCustomAttribute(Of System.Reflection.AssemblyInformationalVersionAttribute)()?.InformationalVersion
        Dim version As New Label($"Version {If(String.IsNullOrWhiteSpace(appVersion), "1.0.0", appVersion)}") With {.Xalign = 0}
        rightBox.PackStart(version, False, False, 0)

        Dim copyrightLabel As New Label("Custom Frontend by sudo") With {.Xalign = 0}
        rightBox.PackStart(copyrightLabel, False, False, 0)

        topBox.PackStart(rightBox, True, True, 0)

        Dim desc As New TextView() With {
            .WrapMode = WrapMode.Word,
            .Editable = False,
            .CursorVisible = False
        }
        desc.Buffer.Text = "Credits:" & vbLf & "@tock-dev (CandyQAZ) (original owner of unreader)" & vbLf & "@HackerAUG (AugustineJames) (Contrib)" & vbLf & "@KodiGamingYT (CodyIsBlack) (Contrib)"

        Dim sc As New ScrolledWindow()
        sc.SetSizeRequest(0, 140)
        sc.Add(desc)

        Dim targetArea As Box = TryCast(Me.ContentArea, Box)
        If targetArea IsNot Nothing Then
            targetArea.Spacing = 10
            targetArea.BorderWidth = 12
            targetArea.PackStart(topBox, False, False, 0)
            targetArea.PackStart(sc, True, True, 0)
        Else
            Dim contentArea As New VBox(False, 10)
            contentArea.BorderWidth = 12
            contentArea.PackStart(topBox, False, False, 0)
            contentArea.PackStart(sc, True, True, 0)
            Add(contentArea)
        End If

        ShowAll()
    End Sub

    Protected Overrides Sub OnResponse(response_id As ResponseType)
        MyBase.OnResponse(response_id)

        Try
            Me.Destroy()
        Catch
        End Try
    End Sub
End Class
