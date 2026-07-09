Imports Gtk
Imports System
Imports System.Text
Imports UnReader.NET.Core

Module Program
    Sub Main(args As String())
        Application.Init()

        'Edit!
        Dim serverUrl As String = If(Environment.GetEnvironmentVariable("UNREADER_SERVER_URL"), String.Empty)
        Dim jwtSecret As String = If(Environment.GetEnvironmentVariable("UNREADER_JWT_SECRET"), String.Empty)
        'End of edit.

        Dim loginWindow As New LoginWindow(serverUrl, jwtSecret)
        Application.Run()
    End Sub
End Module
