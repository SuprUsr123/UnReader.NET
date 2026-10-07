Imports Gtk
Imports System
Imports System.Text
Imports UnReader.NET.Core

Module Program
    Sub Main(args As String())
        Application.Init()

        Dim envServerUrl = Environment.GetEnvironmentVariable("UNREADER_SERVER_URL")
        Dim serverUrl = If(String.IsNullOrWhiteSpace(envServerUrl), AppSettings.Current.ServerUrl, envServerUrl)

        Dim loginWindow As New LoginWindow(serverUrl)
        Application.Run()
    End Sub
End Module
