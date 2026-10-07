Imports System
Imports System.Text
Imports System.Threading.Tasks
Imports UnReader.NET.Core

Module Program
    Sub Main(args As String())
        MainAsync(args).GetAwaiter().GetResult()
    End Sub

    Private Async Function MainAsync(args As String()) As Task
        Dim serverUrl As String = If(Environment.GetEnvironmentVariable("UNREADER_SERVER_URL"), "http://localhost:10000")
        Dim reader As New ServerReader(serverUrl)

        Console.WriteLine("UnReader.NET Linux console client")
        Console.Write("Username: ")
        Dim username As String = If(Console.ReadLine(), String.Empty).Trim()
        Console.Write("Password: ")
        Dim password As String = ReadPassword()

        If String.IsNullOrWhiteSpace(username) OrElse String.IsNullOrWhiteSpace(password) Then
            Console.WriteLine("Username and password are required.")
            Return
        End If

        Dim auth = Await reader.LoginAsync(username, password)
        If Not auth.IsSuccess Then
            Console.WriteLine($"Login failed: {auth.ErrorMessage}")
            Return
        End If

        Console.WriteLine($"Authenticated as {auth.Username}")

        Dim connected As Boolean = Await reader.ConnectRealtimeAsync()
        If Not connected Then
            Console.WriteLine("Failed to connect to realtime server.")
            Return
        End If

        Console.WriteLine("Connected. Type /help for commands.")

        While True
            Console.Write("> ")
            Dim input As String = If(Console.ReadLine(), String.Empty).Trim()
            If String.IsNullOrEmpty(input) Then Continue While

            Select Case input.ToLowerInvariant()
                Case "/quit", "/exit"
                    reader.DisconnectRealtime()
                    Return
                Case "/help"
                    Console.WriteLine("Commands: /quit, /exit, /help, /history, /dms, /topics")
                Case "/history"
                    Dim msgs = Await reader.GetHistoryAsync(0)
                    For Each m As ChatMessage In msgs
                        Console.WriteLine(m.FormattedText)
                    Next
                Case "/dms"
                    Dim dms = Await reader.GetDMContactsAsync()
                    For Each dm As String In dms
                        Console.WriteLine(dm)
                    Next
                Case "/topics"
                    Dim topics = Await reader.GetTopicsAsync()
                    For Each t As TopicRoom In topics
                        Console.WriteLine(t.Slug)
                    Next
                Case Else
                    Console.WriteLine("Unknown command. Use /help")
            End Select
        End While
    End Function

    Private Function ReadPassword() As String
        Dim pw As New StringBuilder()
        While True
            Dim key = Console.ReadKey(intercept:=True)
            If key.Key = ConsoleKey.Enter Then
                Console.WriteLine()
                Exit While
            ElseIf key.Key = ConsoleKey.Backspace AndAlso pw.Length > 0 Then
                pw.Length -= 1
                Console.Write("\b \b")
            ElseIf Not Char.IsControl(key.KeyChar) Then
                pw.Append(key.KeyChar)
                Console.Write("*")
            End If
        End While
        Return pw.ToString()
    End Function
End Module
