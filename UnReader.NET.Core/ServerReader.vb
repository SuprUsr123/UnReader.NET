Imports System.Net.Http
Imports System.Net.Http.Headers
Imports System.Net.WebSockets
Imports System.Text
Imports System.Text.Json
Imports System.Text.Json.Serialization
Imports System.Threading
Imports System.Threading.Tasks
Imports System.Collections.Generic
Imports System.IO

' ---------------------------------------------------------------------------
' Shared, persistent settings for all UnReader.NET desktop clients.
' Stored in %APPDATA%/UnReader.NET/settings.json (roaming profile).
' ---------------------------------------------------------------------------
Public Class AppSettings
    Public Property ServerUrl As String = "http://localhost:10000"
    Public Property Username As String = ""
    Public Property AutoReconnect As Boolean = True
    Public Property SystemMemory As String = ""

    Private Shared _current As AppSettings

    Public Shared ReadOnly Property Current As AppSettings
        Get
            If _current Is Nothing Then _current = Load()
            Return _current
        End Get
    End Property

    Public Shared Function SettingsPath() As String
        Return Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "UnReader.NET",
            "settings.json")
    End Function

    Public Shared Function Load() As AppSettings
        Try
            Dim p = SettingsPath()
            If File.Exists(p) Then
                Dim json = File.ReadAllText(p)
                Dim loaded = JsonSerializer.Deserialize(Of AppSettings)(json, New JsonSerializerOptions With {.PropertyNameCaseInsensitive = True})
                If loaded IsNot Nothing AndAlso Not String.IsNullOrWhiteSpace(loaded.ServerUrl) Then
                    _current = loaded
                    Return loaded
                End If
            End If
        Catch
        End Try
        Return New AppSettings()
    End Function

    Public Sub Save()
        Try
            Dim dir = Path.GetDirectoryName(SettingsPath())
            If Not String.IsNullOrWhiteSpace(dir) Then Directory.CreateDirectory(dir)
            File.WriteAllText(SettingsPath(), JsonSerializer.Serialize(Me, New JsonSerializerOptions With {.WriteIndented = True}))
        Catch
        End Try
    End Sub
End Class

' ---------------------------------------------------------------------------
' API/DTO models
' ---------------------------------------------------------------------------
Public Class AuthResponse
    <JsonPropertyName("token")> Public Property Token As String
    <JsonPropertyName("username")> Public Property Username As String
    Public Property ErrorMessage As String

    Public ReadOnly Property IsSuccess As Boolean
        Get
            Return String.IsNullOrEmpty(ErrorMessage)
        End Get
    End Property
End Class

Public Class ChatRole
    <JsonPropertyName("role")> Public Property RoleName As String
    <JsonPropertyName("prefix")> Public Property Prefix As String
    <JsonPropertyName("style")> Public Property Style As String
    <JsonPropertyName("class")> Public Property CssClass As String

    Public ReadOnly Property IsStaff As Boolean
        Get
            Return RoleName = "admin" OrElse RoleName = "moderator"
        End Get
    End Property

    Public ReadOnly Property IsAdmin As Boolean
        Get
            Return RoleName = "admin"
        End Get
    End Property
End Class

Public Class UserRolesPayload
    <JsonPropertyName("role")> Public Property Role As ChatRole
    <JsonPropertyName("is_banned")> Public Property IsBanned As Boolean
    <JsonPropertyName("timeout_until")> Public Property TimeoutUntil As Long

    Public ReadOnly Property IsStaff As Boolean
        Get
            Return Role IsNot Nothing AndAlso Role.IsStaff
        End Get
    End Property

    Public ReadOnly Property IsAdmin As Boolean
        Get
            Return Role IsNot Nothing AndAlso Role.IsAdmin
        End Get
    End Property
End Class

Public Class RosterUser
    Public Property Username As String
    Public Property Mode As String
    Public Property Target As String
    Public Property Role As ChatRole

    Public ReadOnly Property DisplayText As String
        Get
            Dim prefix = If(Not String.IsNullOrEmpty(Role?.Prefix), $"[{Role.Prefix}] ", "")
            Dim context = If(Mode = "public", "(Global)", If(Mode = "dm", $"(DM @{Target})", "(Neighborhood)"))
            Return $"{prefix}@{Username} {context}"
        End Get
    End Property
End Class

Public Class TopicRoom
    <JsonPropertyName("slug")> Public Property Slug As String
    <JsonPropertyName("title")> Public Property Title As String
End Class

Public Class ChatMessage
    <JsonPropertyName("id")> Public Property Id As Integer
    <JsonPropertyName("username")> Public Property Username As String
    <JsonPropertyName("sender")> Public Property Sender As String
    <JsonPropertyName("timestamp")> Public Property Timestamp As String
    <JsonPropertyName("content")> Public Property Content As String
    <JsonPropertyName("is_deleted")> Public Property IsDeleted As Boolean
    <JsonPropertyName("deleted_by")> Public Property DeletedBy As String
    <JsonPropertyName("role")> Public Property Role As ChatRole

    Public ReadOnly Property DisplayHeader As String
        Get
            Dim effectiveUser = If(String.IsNullOrEmpty(Username), Sender, Username)
            Dim prefix = If(Not String.IsNullOrEmpty(Role?.Prefix), $"[{Role.Prefix}] ", "")
            Return $"{prefix}@{effectiveUser}"
        End Get
    End Property

    Public ReadOnly Property FormattedText As String
        Get
            If IsDeleted Then Return $"{DisplayHeader}: [MESSAGE PURGED]"
            Return $"{DisplayHeader}: {Content}"
        End Get
    End Property
End Class

Public Class SystemChatTurn
    Public Property Role As String
    Public Property Content As String
End Class

Public Class NeighborhoodComment
    <JsonPropertyName("id")> Public Property Id As Integer
    <JsonPropertyName("username")> Public Property Username As String
    <JsonPropertyName("content")> Public Property Content As String
    <JsonPropertyName("is_deleted")> Public Property IsDeleted As Boolean
    <JsonPropertyName("role")> Public Property Role As ChatRole

    Public ReadOnly Property FormattedText As String
        Get
            Dim prefix = If(Not String.IsNullOrEmpty(Role?.Prefix), $"[{Role.Prefix}] ", "")
            If IsDeleted Then Return $"  >> {prefix}@{Username}: [Comment Purged]"
            Return $"  >> {prefix}@{Username}: {Content}"
        End Get
    End Property
End Class

Public Class NeighborhoodPost
    <JsonPropertyName("id")> Public Property Id As Integer
    <JsonPropertyName("username")> Public Property Username As String
    <JsonPropertyName("title")> Public Property Title As String
    <JsonPropertyName("content")> Public Property Content As String
    <JsonPropertyName("is_deleted")> Public Property IsDeleted As Boolean
    <JsonPropertyName("role")> Public Property Role As ChatRole
    <JsonPropertyName("comments")> Public Property Comments As List(Of NeighborhoodComment)

    Public ReadOnly Property DisplayHeader As String
        Get
            Dim prefix = If(Not String.IsNullOrEmpty(Role?.Prefix), $"[{Role.Prefix}] ", "")
            Return $"{prefix}@{Username}"
        End Get
    End Property

    Public Overrides Function ToString() As String
        Return If(IsDeleted, $"[PURGED] {Title}", Title)
    End Function
End Class

Public Class ModLog
    <JsonPropertyName("id")> Public Property Id As Integer
    <JsonPropertyName("mod_username")> Public Property ModUsername As String
    <JsonPropertyName("action_type")> Public Property ActionType As String
    <JsonPropertyName("target_username")> Public Property TargetUsername As String
    <JsonPropertyName("reason")> Public Property Reason As String
    <JsonPropertyName("timestamp")> Public Property Timestamp As Long

    Public ReadOnly Property FormattedText As String
        Get
            Dim ts = DateTimeOffset.FromUnixTimeMilliseconds(Timestamp).LocalDateTime.ToString("yyyy-MM-dd HH:mm")
            Return $"[{ts}] {ModUsername} → {ActionType.ToUpper()} @{TargetUsername} | {Reason}"
        End Get
    End Property
End Class

Public Class UserProfile
    <JsonPropertyName("username")> Public Property Username As String
    <JsonPropertyName("bio")> Public Property Bio As String
    <JsonPropertyName("location")> Public Property Location As String
    <JsonPropertyName("avatar_emoji")> Public Property AvatarEmoji As String
    <JsonPropertyName("is_admin")> Public Property IsAdmin As Boolean
    <JsonPropertyName("is_moderator")> Public Property IsModerator As Boolean
    <JsonPropertyName("is_banned")> Public Property IsBanned As Boolean
    <JsonPropertyName("timeout_until")> Public Property TimeoutUntil As Long
    <JsonPropertyName("role")> Public Property Role As ChatRole

    Public ReadOnly Property IsStaff As Boolean
        Get
            Return IsAdmin OrElse IsModerator OrElse (Role IsNot Nothing AndAlso Role.IsStaff)
        End Get
    End Property

    Public ReadOnly Property DisplayRole As String
        Get
            If Role IsNot Nothing AndAlso Not String.IsNullOrWhiteSpace(Role.RoleName) Then Return Role.RoleName
            Return If(IsAdmin, "admin", If(IsModerator, "moderator", "member"))
        End Get
    End Property

    Public ReadOnly Property StatusSummary As String
        Get
            Dim sb As New StringBuilder()
            sb.AppendLine($"USERNAME : @{Username}")
            sb.AppendLine($"ROLE     : {DisplayRole}")
            sb.AppendLine($"BIO      : {If(Not String.IsNullOrEmpty(Bio), Bio, "(none)")}")
            sb.AppendLine($"LOCATION : {If(Not String.IsNullOrEmpty(Location), Location, "(none)")}")
            sb.AppendLine($"AVATAR   : {If(Not String.IsNullOrEmpty(AvatarEmoji), AvatarEmoji, "(none)")}")
            sb.AppendLine($"BANNED   : {IsBanned}")
            If TimeoutUntil > 0 Then
                Dim untilDt = DateTimeOffset.FromUnixTimeMilliseconds(TimeoutUntil).LocalDateTime
                If untilDt > DateTime.Now Then
                    sb.AppendLine($"TIMEOUT  : Until {untilDt:yyyy-MM-dd HH:mm}")
                Else
                    sb.AppendLine("TIMEOUT  : Expired")
                End If
            End If
            Return sb.ToString().TrimEnd()
        End Get
    End Property
End Class

Public Class Suggestion
    <JsonPropertyName("id")> Public Property Id As Integer
    <JsonPropertyName("date")> Public Property DateText As String
    <JsonPropertyName("suggestions")> Public Property Body As String
    <JsonPropertyName("username")> Public Property Username As String
    <JsonPropertyName("admin_filter")> Public Property AdminFilter As String
    <JsonPropertyName("completion")> Public Property Completion As Boolean?

    Public ReadOnly Property Title As String
        Get
            If String.IsNullOrEmpty(Body) Then Return "Untitled Concept"
            Dim idx = Body.IndexOf(ControlChars.Lf)
            If idx < 0 Then Return Body.Trim()
            Return Body.Substring(0, idx).Trim()
        End Get
    End Property

    Public ReadOnly Property Description As String
        Get
            If String.IsNullOrEmpty(Body) Then Return ""
            Dim idx = Body.IndexOf(ControlChars.Lf)
            If idx < 0 OrElse idx + 1 >= Body.Length Then Return ""
            Return Body.Substring(idx + 1).Trim()
        End Get
    End Property

    Public ReadOnly Property Completed As Boolean
        Get
            Return Completion.GetValueOrDefault()
        End Get
    End Property

    Public ReadOnly Property StatusTag As String
        Get
            Return If(String.IsNullOrWhiteSpace(AdminFilter), "", AdminFilter.Trim())
        End Get
    End Property

    Public ReadOnly Property DisplaySummary As String
        Get
            Dim tag = If(Completed, "[DONE] ", "")
            Dim status = If(Not String.IsNullOrWhiteSpace(StatusTag), $"[{StatusTag.Trim()}] ", "")
            Return $"{tag}{status}#{Id} @{Username} ({DateText}) — {Title}"
        End Get
    End Property
End Class

' Portal 2D online pack DTOs. Pack data intentionally remains JSON so newer
' game formats can pass through the desktop client without losing fields.
Public Class Portal2DViewer
    Public Property Username As String
    Public Property IsAdmin As Boolean
    Public Property IsStaff As Boolean
End Class

Public Class Portal2DLevelSummary
    Public Property Id As Integer
    Public Property Name As String
    Public Property Username As String
    Public Property CanDelete As Boolean
    Public Property IsOwner As Boolean
End Class

Public Class Portal2DLevelPage
    Public Property Levels As List(Of Portal2DLevelSummary) = New List(Of Portal2DLevelSummary)()
    Public Property HasMore As Boolean
    Public Property Viewer As Portal2DViewer
End Class

Public Class Portal2DOnlinePack
    Public Property Id As Integer
    Public Property Name As String
    Public Property Username As String
    Public Property Data As JsonElement
End Class

Public Class Portal2DActionResult
    Public Property Success As Boolean
    Public Property Id As Integer
End Class

' ---------------------------------------------------------------------------
' ServerReader: HTTP + WebSocket client for the unreader BBS.
' ---------------------------------------------------------------------------
Public Class ServerReader

    Public Property ServerUrl As String
    Public Property JwtToken As String
    Public Property CurrentUser As UserProfile
    Public Property LastUserRoles As UserRolesPayload

    Private Shared ReadOnly Client As New HttpClient()
    Private ReadOnly JsonOpts As New JsonSerializerOptions With {
        .PropertyNameCaseInsensitive = True
        }

    Public Event OnRosterUpdated(users As List(Of RosterUser))
    Public Event OnFeedRefreshed()
    Public Event OnTopicsUpdated(topics As List(Of TopicRoom))
    Public Event OnDisconnected(reason As String)
    Public Event OnErrorAlert(message As String)
    Public Event OnSuggestionsUpdated()
    ' The full server packet is exposed for client-specific live rendering.
    Public Event OnLivePacket(json As String)

    Private _ws As ClientWebSocket
    Private _cts As CancellationTokenSource
    Private _lastTopics As New List(Of TopicRoom)()

    Public Sub New(Optional serverUrl As String = "http://localhost:10000", Optional jwtToken As String = "")
        Me.ServerUrl = serverUrl.TrimEnd("/"c)
        Me.JwtToken = jwtToken
    End Sub

    Public ReadOnly Property IsStaff As Boolean
        Get
            If CurrentUser IsNot Nothing Then Return CurrentUser.IsStaff
            If LastUserRoles IsNot Nothing Then Return LastUserRoles.IsStaff
            Return False
        End Get
    End Property

    Public ReadOnly Property IsAdmin As Boolean
        Get
            If CurrentUser IsNot Nothing Then Return CurrentUser.IsAdmin OrElse (CurrentUser.Role IsNot Nothing AndAlso CurrentUser.Role.IsAdmin)
            If LastUserRoles IsNot Nothing Then Return LastUserRoles.IsAdmin
            Return False
        End Get
    End Property

    Private Function CreateAuthRequest(method As HttpMethod, endpoint As String) As HttpRequestMessage
        Dim fullUrl As String = ServerUrl & "/" & endpoint.TrimStart("/"c)
        Dim req As New HttpRequestMessage(method, fullUrl)

        If Not String.IsNullOrWhiteSpace(JwtToken) Then
            req.Headers.Authorization = New AuthenticationHeaderValue("Bearer", JwtToken)
        End If

        Return req
    End Function

    Private Async Function SendRequestAsync(Of T)(req As HttpRequestMessage) As Task(Of T)
        Using res As HttpResponseMessage = Await Client.SendAsync(req)
            Dim rawContent As String = Await res.Content.ReadAsStringAsync()
            If Not res.IsSuccessStatusCode Then Throw New Exception($"HTTP {res.StatusCode}: {rawContent}")
            Return JsonSerializer.Deserialize(Of T)(rawContent, JsonOpts)
        End Using
    End Function

    Private Async Function SendRequestAndGetRawAsync(req As HttpRequestMessage) As Task(Of String)
        Using res As HttpResponseMessage = Await Client.SendAsync(req)
            Dim rawContent As String = Await res.Content.ReadAsStringAsync()
            If Not res.IsSuccessStatusCode Then Throw New Exception($"HTTP {res.StatusCode}: {rawContent}")
            Return rawContent
        End Using
    End Function

    Private Async Function PostJsonAsync(Of T)(endpoint As String, payload As Object) As Task(Of T)
        Using req As HttpRequestMessage = CreateAuthRequest(HttpMethod.Post, endpoint)
            Dim jsonString As String = JsonSerializer.Serialize(payload)
            req.Content = New StringContent(jsonString, Encoding.UTF8, "application/json")
            Return Await SendRequestAsync(Of T)(req)
        End Using
    End Function

    Private Async Function PutJsonAsync(Of T)(endpoint As String, payload As Object) As Task(Of T)
        Using req As HttpRequestMessage = CreateAuthRequest(HttpMethod.Put, endpoint)
            Dim jsonString As String = JsonSerializer.Serialize(payload)
            req.Content = New StringContent(jsonString, Encoding.UTF8, "application/json")
            Return Await SendRequestAsync(Of T)(req)
        End Using
    End Function

    Private Async Function DeleteAsync(Of T)(endpoint As String) As Task(Of T)
        Using req As HttpRequestMessage = CreateAuthRequest(HttpMethod.Delete, endpoint)
            Return Await SendRequestAsync(Of T)(req)
        End Using
    End Function

    ' ------------------------------------------------------------------
    ' WebSocket realtime
    ' ------------------------------------------------------------------
    Public Async Function ConnectRealtimeAsync(Optional authToken As String = Nothing) As Task(Of Boolean)
        Try
            If _ws IsNot Nothing Then
                Try
                    If _cts IsNot Nothing Then _cts.Cancel()

                    If _ws.State = WebSocketState.Open Then
                        Await _ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Reconnecting", CancellationToken.None)
                    End If
                    _ws.Dispose()
                Catch
                End Try
            End If

            If Not String.IsNullOrWhiteSpace(authToken) Then JwtToken = authToken

            _cts = New CancellationTokenSource()
            _ws = New ClientWebSocket()

            If Not String.IsNullOrWhiteSpace(JwtToken) Then
                _ws.Options.SetRequestHeader("Authorization", "Bearer " & JwtToken)
            End If

            Dim wsUrl As String = ServerUrl.Replace("http://", "ws://").Replace("https://", "wss://")
            Dim uri As New Uri(wsUrl & "/ws")

            Await _ws.ConnectAsync(uri, _cts.Token)

            Await SendWsMessageAsync(New With {.type = "auth", .token = JwtToken})

            ' Fire-and-forget receive loop; lifetime follows the connection.
            Dim receiveTask As Task = Task.Run(Async Function() As Task
                                                   Await ReceiveLoopAsync()
                                               End Function)
            receiveTask.ConfigureAwait(False)

            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function

    Private Async Function ReceiveLoopAsync() As Task
        Dim buffer(8192) As Byte
        Try
            While _ws.State = WebSocketState.Open AndAlso Not _cts.IsCancellationRequested
                Using ms As New MemoryStream()
                    Dim result As WebSocketReceiveResult
                    Do
                        result = Await _ws.ReceiveAsync(New ArraySegment(Of Byte)(buffer), _cts.Token)
                        ms.Write(buffer, 0, result.Count)
                    Loop While Not result.EndOfMessage

                    If result.MessageType = WebSocketMessageType.Close Then
                        RaiseEvent OnDisconnected("Server closed the connection.")
                        Exit While
                    End If

                    If result.MessageType = WebSocketMessageType.Text Then
                        Dim jsonStr = Encoding.UTF8.GetString(ms.ToArray())
                        If Not String.IsNullOrWhiteSpace(jsonStr) Then
                            ProcessWebSocketPacket(jsonStr)
                        End If
                    End If
                End Using
            End While
        Catch ex As OperationCanceledException
        Catch ex As Exception
            RaiseEvent OnDisconnected("Connection lost: " & ex.Message)
        End Try
    End Function

    Private Sub ProcessWebSocketPacket(json As String)
        Try
            Using doc = JsonDocument.Parse(json)
                Dim root = doc.RootElement

                Dim typeProp As JsonElement
                If Not root.TryGetProperty("type", typeProp) Then Return

                Select Case typeProp.GetString()
                    Case "terminated"
                        Dim reasonEl As JsonElement
                        Dim reason = If(root.TryGetProperty("reason", reasonEl), reasonEl.GetString(), "Session expired")
                        RaiseEvent OnDisconnected(reason)

                    Case "error_alert"
                        Dim msgProp As JsonElement
                        If root.TryGetProperty("message", msgProp) Then
                            RaiseEvent OnErrorAlert(msgProp.GetString())
                        End If

                    Case "refresh_feed"
                        RaiseEvent OnLivePacket(json)
                        RaiseEvent OnFeedRefreshed()

                    Case "suggestions_update"
                        RaiseEvent OnLivePacket(json)
                        RaiseEvent OnSuggestionsUpdated()

                    Case "auth_success"
                        Dim userRolesProp As JsonElement
                        If root.TryGetProperty("userRoles", userRolesProp) AndAlso userRolesProp.ValueKind = JsonValueKind.Object Then
                            LastUserRoles = JsonSerializer.Deserialize(Of UserRolesPayload)(userRolesProp.GetRawText(), JsonOpts)
                        End If

                    Case "live_message", "live_neighborhood_post"
                        RaiseEvent OnLivePacket(json)
                        RaiseEvent OnFeedRefreshed()

                    Case "roster_update"
                        Dim usersList As New List(Of RosterUser)
                        Dim arrayProp As JsonElement

                        If root.TryGetProperty("users", arrayProp) OrElse root.TryGetProperty("roster", arrayProp) Then
                            If arrayProp.ValueKind = JsonValueKind.Array Then
                                For Each item In arrayProp.EnumerateArray()
                                    Try
                                        Dim ru As New RosterUser()

                                        If item.ValueKind = JsonValueKind.String Then
                                            ru.Username = item.GetString()
                                            ru.Mode = "public"
                                            usersList.Add(ru)
                                            Continue For
                                        End If

                                        Dim uProp As JsonElement = Nothing
                                        If item.TryGetProperty("username", uProp) AndAlso uProp.ValueKind = JsonValueKind.String Then
                                            ru.Username = uProp.GetString()
                                        Else
                                            ru.Username = "Unknown"
                                        End If

                                        Dim mProp As JsonElement = Nothing
                                        If item.TryGetProperty("mode", mProp) AndAlso mProp.ValueKind = JsonValueKind.String Then
                                            ru.Mode = mProp.GetString()
                                        Else
                                            ru.Mode = "public"
                                        End If

                                        Dim tProp As JsonElement = Nothing
                                        If item.TryGetProperty("target", tProp) AndAlso tProp.ValueKind = JsonValueKind.String Then
                                            ru.Target = tProp.GetString()
                                        Else
                                            ru.Target = ""
                                        End If

                                        Dim prefix As String = ""
                                        Dim userRolesProp2 As JsonElement = Nothing
                                        Dim oldRoleProp As JsonElement = Nothing

                                        If item.TryGetProperty("userRoles", userRolesProp2) AndAlso userRolesProp2.ValueKind = JsonValueKind.Object Then
                                            Dim roleProp As JsonElement = Nothing
                                            If userRolesProp2.TryGetProperty("role", roleProp) AndAlso roleProp.ValueKind = JsonValueKind.Object Then
                                                Dim prefixProp As JsonElement = Nothing
                                                If roleProp.TryGetProperty("prefix", prefixProp) AndAlso prefixProp.ValueKind = JsonValueKind.String Then
                                                    prefix = prefixProp.GetString()
                                                End If
                                            End If
                                        ElseIf item.TryGetProperty("role", oldRoleProp) Then
                                            If oldRoleProp.ValueKind = JsonValueKind.Object Then
                                                Dim prefixProp As JsonElement = Nothing
                                                If oldRoleProp.TryGetProperty("prefix", prefixProp) AndAlso prefixProp.ValueKind = JsonValueKind.String Then
                                                    prefix = prefixProp.GetString()
                                                End If
                                            ElseIf oldRoleProp.ValueKind = JsonValueKind.String Then
                                                prefix = oldRoleProp.GetString()
                                            End If
                                        End If

                                        ru.Role = New ChatRole With {.Prefix = prefix}
                                        usersList.Add(ru)
                                    Catch
                                    End Try
                                Next
                            End If
                        End If
                        RaiseEvent OnRosterUpdated(usersList)

                    Case "topics_update"
                        Dim topicsList As New List(Of TopicRoom)
                        Dim topicsProp As JsonElement

                        If root.TryGetProperty("topics", topicsProp) AndAlso topicsProp.ValueKind = JsonValueKind.Array Then
                            For Each item In topicsProp.EnumerateArray()
                                Try
                                    Dim tr As New TopicRoom()

                                    If item.ValueKind = JsonValueKind.String Then
                                        tr.Slug = item.GetString()
                                        tr.Title = item.GetString()
                                    Else
                                        Dim sProp As JsonElement = Nothing
                                        Dim tProp As JsonElement = Nothing
                                        tr.Slug = If(item.TryGetProperty("slug", sProp), sProp.GetString(), "")
                                        tr.Title = If(item.TryGetProperty("title", tProp), tProp.GetString(), tr.Slug)
                                    End If

                                    If Not String.IsNullOrEmpty(tr.Slug) Then topicsList.Add(tr)
                                Catch
                                End Try
                            Next
                        End If
                        _lastTopics = topicsList
                        RaiseEvent OnTopicsUpdated(topicsList)

                End Select
            End Using
        Catch ex As Exception
        End Try
    End Sub

    Public Sub DisconnectRealtime()
        Try
            If _cts IsNot Nothing Then _cts.Cancel()
            If _ws IsNot Nothing AndAlso _ws.State = WebSocketState.Open Then
                _ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client closed", CancellationToken.None).GetAwaiter().GetResult()
            End If
        Catch
        End Try
    End Sub

    Public Async Function SendWsMessageAsync(payload As Object) As Task
        If _ws IsNot Nothing AndAlso _ws.State = WebSocketState.Open AndAlso _cts IsNot Nothing Then
            Dim json = JsonSerializer.Serialize(payload)
            Dim bytes = Encoding.UTF8.GetBytes(json)
            Await _ws.SendAsync(New ArraySegment(Of Byte)(bytes), WebSocketMessageType.Text, True, _cts.Token)
        End If
    End Function

    Public Function SwitchContextAsync(mode As String, Optional target As String = "") As Task
        Return SendWsMessageAsync(New With {.type = "switch_context", .mode = mode, .target = target})
    End Function

    Public Function SendPublicMessageAsync(content As String) As Task
        Return SendWsMessageAsync(New With {.type = "message", .content = content})
    End Function

    Public Function SendDirectMessageAsync(target As String, content As String) As Task
        Return SendWsMessageAsync(New With {.type = "dm", .target = target, .content = content})
    End Function

    Public Function SendTopicMessageAsync(slug As String, content As String) As Task
        Return SendWsMessageAsync(New With {.type = "topic_message", .target = slug, .content = content})
    End Function

    Public Async Function GetSystemChatReplyAsync(query As String, history As IEnumerable(Of SystemChatTurn), apiKey As String, systemPrompt As String) As Task(Of String)
        If String.IsNullOrWhiteSpace(apiKey) Then Throw New InvalidOperationException("Add your Groq API key in File > Settings to use @system.")

        Dim messages As New List(Of Object) From {
            New With {.role = "system", .content = "You are system, the chat assistant for UnReader.NET. Answer clearly and concisely in plain text. Do not claim to perform actions."}
        }
        For Each turn In If(history, Enumerable.Empty(Of SystemChatTurn)()).TakeLast(20)
            If turn IsNot Nothing AndAlso (turn.Role = "user" OrElse turn.Role = "assistant") AndAlso Not String.IsNullOrWhiteSpace(turn.Content) Then
                messages.Add(New With {.role = turn.Role, .content = turn.Content})
            End If
        Next
        messages.Add(New With {.role = "user", .content = query})

        Dim lastError As Exception = Nothing
        For Each model In {"openai/gpt-oss-120b", "qwen/qwen3.8-27b"}
            Try
                messages(0) = New With {.role = "system", .content = systemPrompt.Replace("{model}", model)}
                Using request As New HttpRequestMessage(HttpMethod.Post, "https://api.groq.com/openai/v1/chat/completions")
                    request.Headers.Authorization = New AuthenticationHeaderValue("Bearer", apiKey)
                    request.Content = New StringContent(JsonSerializer.Serialize(New With {
                        .model = model,
                        .messages = messages,
                        .max_completion_tokens = 4096,
                        .temperature = 1,
                        .top_p = 1,
                        .stream = False
                    }), Encoding.UTF8, "application/json")
                    Using response = Await Client.SendAsync(request)
                        If Not response.IsSuccessStatusCode Then
                            lastError = New HttpRequestException($"Groq request failed (HTTP {CInt(response.StatusCode)}).")
                            Continue For
                        End If
                        Using document = JsonDocument.Parse(Await response.Content.ReadAsStringAsync())
                            Dim reply = document.RootElement.GetProperty("choices")(0).GetProperty("message").GetProperty("content").GetString()
                            If Not String.IsNullOrWhiteSpace(reply) Then Return reply.Trim()
                        End Using
                    End Using
                End Using
            Catch ex As Exception
                lastError = ex
            End Try
        Next
        Throw New InvalidOperationException("The system assistant could not complete the request.", lastError)
    End Function

    Public Function CreateTopicAsync(title As String) As Task
        Return SendWsMessageAsync(New With {.type = "create_topic", .title = title})
    End Function

    ' ------------------------------------------------------------------
    ' Auth / identity
    ' ------------------------------------------------------------------
    Public Async Function LoginAsync(username As String, password As String) As Task(Of AuthResponse)
        Try
            Dim res = Await PostJsonAsync(Of AuthResponse)("/api/login", New With {.username = username, .password = password})
            Me.JwtToken = res.Token
            If Not String.IsNullOrWhiteSpace(res.Username) Then
                CurrentUser = New UserProfile With {.Username = res.Username}
            End If
            Return res
        Catch ex As Exception
            Return New AuthResponse With {.ErrorMessage = ex.Message}
        End Try
    End Function

    Public Async Function RegisterAsync(username As String, password As String) As Task(Of AuthResponse)
        Try
            Dim res = Await PostJsonAsync(Of AuthResponse)("/api/register", New With {.username = username, .password = password})
            Me.JwtToken = res.Token
            CurrentUser = New UserProfile With {.Username = res.Username}
            Return res
        Catch ex As Exception
            Return New AuthResponse With {.ErrorMessage = ex.Message}
        End Try
    End Function

    ' /api/me returns full identity including staff flags. Falls back to a
    ' profile lookup so the client keeps working on older servers.
    Public Async Function GetMeAsync() As Task(Of UserProfile)
        Dim raw As String = Nothing
        Dim fetched As Boolean = False
        Try
            Using req = CreateAuthRequest(HttpMethod.Get, "/api/me")
                raw = Await SendRequestAndGetRawAsync(req)
                fetched = True
            End Using
        Catch
        End Try

        If fetched Then
            CurrentUser = JsonSerializer.Deserialize(Of UserProfile)(raw, JsonOpts)
            Return CurrentUser
        End If

        ' Fallback path (outside the Try/Catch so Await is permitted).
        If CurrentUser IsNot Nothing AndAlso Not String.IsNullOrWhiteSpace(CurrentUser.Username) Then
            Dim profile = Await GetProfileAsync(CurrentUser.Username)
            If profile IsNot Nothing AndAlso Not String.IsNullOrWhiteSpace(profile.Username) Then
                CurrentUser = profile
                Return profile
            End If
        End If
        Return New UserProfile()
    End Function

    Public Async Function ChangePasswordAsync(currentPassword As String, newPassword As String) As Task(Of String)
        ' Returns Nothing on success, error text otherwise.
        Try
            Await PostJsonAsync(Of Object)("/api/change-password", New With {.currentPassword = currentPassword, .newPassword = newPassword})
            Return Nothing
        Catch ex As Exception
            Return ex.Message
        End Try
    End Function

    Public Async Function UpdateProfileAsync(bio As String, location As String, avatarEmoji As String) As Task(Of String)
        Try
            Await PostJsonAsync(Of Object)("/api/profile", New With {.bio = bio, .location = location, .avatar_emoji = avatarEmoji})
            Return Nothing
        Catch ex As Exception
            Return ex.Message
        End Try
    End Function

    ' ------------------------------------------------------------------
    ' History feeds
    ' ------------------------------------------------------------------
    Public Async Function GetHistoryAsync(index As Integer) As Task(Of List(Of ChatMessage))
        Using req = CreateAuthRequest(HttpMethod.Get, $"/history?index={index}")
            Return Await SendRequestAsync(Of List(Of ChatMessage))(req)
        End Using
    End Function

    Public Async Function GetDMHistoryAsync(target As String, index As Integer) As Task(Of List(Of ChatMessage))
        Using req = CreateAuthRequest(HttpMethod.Get, $"/dm-history?target={Uri.EscapeDataString(target)}&index={index}")
            Return Await SendRequestAsync(Of List(Of ChatMessage))(req)
        End Using
    End Function

    Public Async Function GetTopicHistoryAsync(slug As String, index As Integer) As Task(Of List(Of ChatMessage))
        Using req = CreateAuthRequest(HttpMethod.Get, $"/topic-history?slug={Uri.EscapeDataString(slug)}&index={index}")
            Return Await SendRequestAsync(Of List(Of ChatMessage))(req)
        End Using
    End Function

    Public Async Function GetDMContactsAsync() As Task(Of List(Of String))
        Using req = CreateAuthRequest(HttpMethod.Get, "/dm-contacts")
            Return Await SendRequestAsync(Of List(Of String))(req)
        End Using
    End Function

    Public Async Function GetNeighborhoodHistoryAsync(index As Integer) As Task(Of List(Of NeighborhoodPost))
        Using req = CreateAuthRequest(HttpMethod.Get, $"/neighborhood-history?index={index}")
            Return Await SendRequestAsync(Of List(Of NeighborhoodPost))(req)
        End Using
    End Function

    ' ------------------------------------------------------------------
    ' Suggestions board (newer server API)
    ' ------------------------------------------------------------------
    Public Async Function GetSuggestionsAsync(index As Integer) As Task(Of List(Of Suggestion))
        Using req = CreateAuthRequest(HttpMethod.Get, $"/suggestions-history?index={index}")
            Return Await SendRequestAsync(Of List(Of Suggestion))(req)
        End Using
    End Function

    Public Async Function PostSuggestionAsync(title As String, body As String) As Task(Of String)
        Try
            Await PostJsonAsync(Of Object)("/api/suggestions", New With {.title = title, .body = body})
            Return Nothing
        Catch ex As Exception
            Return ex.Message
        End Try
    End Function

    ' Staff tools: tag a suggestion with a status (empty clears), mark done/undone.
    Public Async Function StaffSetSuggestionTagAsync(id As Integer, tag As String) As Task(Of String)
        Try
            Await PutJsonAsync(Of Object)($"/api/suggestions/{id}", New With {.admin_filter = tag})
            Return Nothing
        Catch ex As Exception
            Return ex.Message
        End Try
    End Function

    Public Async Function StaffSetSuggestionDoneAsync(id As Integer, done As Boolean) As Task(Of String)
        Try
            Await PutJsonAsync(Of Object)($"/api/suggestions/{id}", New With {.completion = done})
            Return Nothing
        Catch ex As Exception
            Return ex.Message
        End Try
    End Function

    Public Async Function StaffClearSuggestionTagsAsync(id As Integer) As Task(Of String)
        Try
            Await PutJsonAsync(Of Object)($"/api/suggestions/{id}", New With {.admin_filter = Nothing, .completion = Nothing})
            Return Nothing
        Catch ex As Exception
            Return ex.Message
        End Try
    End Function

    Public Async Function DeleteSuggestionAsync(id As Integer) As Task(Of String)
        Try
            Await DeleteAsync(Of Object)($"/api/suggestions/{id}")
            Return Nothing
        Catch ex As Exception
            Return ex.Message
        End Try
    End Function

    ' ------------------------------------------------------------------
    ' Profiles / moderation
    ' ------------------------------------------------------------------
    Public Async Function GetProfileAsync(username As String) As Task(Of UserProfile)
        Try
            Using req = CreateAuthRequest(HttpMethod.Get, $"/api/profile/{Uri.EscapeDataString(username)}")
                Return Await SendRequestAsync(Of UserProfile)(req)
            End Using
        Catch ex As Exception
            Return New UserProfile With {.Username = username, .Bio = $"Error: {ex.Message}"}
        End Try
    End Function

    Public Async Function FindUserAsync(username As String) As Task(Of UserProfile)
        Try
            Using req = CreateAuthRequest(HttpMethod.Get, $"/api/admin/find-user/{Uri.EscapeDataString(username)}")
                Return Await SendRequestAsync(Of UserProfile)(req)
            End Using
        Catch ex As Exception
            Return New UserProfile With {.Username = username, .Bio = $"Error: {ex.Message}"}
        End Try
    End Function

    Public Async Function GetModLogsAsync() As Task(Of List(Of ModLog))
        Try
            Using req = CreateAuthRequest(HttpMethod.Get, "/api/mod-logs")
                Return Await SendRequestAsync(Of List(Of ModLog))(req)
            End Using
        Catch ex As Exception
            Return New List(Of ModLog)()
        End Try
    End Function

    Public Async Function GetTopicsAsync() As Task(Of List(Of TopicRoom))
        ' Topics are delivered in the initial WebSocket handshake, not REST.
        Return Await Task.FromResult(New List(Of TopicRoom)(_lastTopics))
    End Function

    Public Async Function SetRoleAsync(targetUsername As String, Optional isModerator As Boolean = True) As Task
        Await PostJsonAsync(Of Object)("/api/admin/set-role", New With {.target = targetUsername, .is_moderator = isModerator})
    End Function

    Public Async Function BanIpAsync(targetUsername As String, reason As String) As Task
        Await PostJsonAsync(Of Object)("/api/admin/ban-ip", New With {.target = targetUsername, .reason = reason})
    End Function

    Public Async Function ModBanAsync(target As String, reason As String) As Task
        Await SendWsMessageAsync(New With {.type = "mod_ban", .target = target, .reason = reason})
    End Function

    Public Async Function ModPardonAsync(target As String) As Task
        Await SendWsMessageAsync(New With {.type = "mod_pardon", .target = target})
    End Function

    Public Async Function ModKickAsync(target As String) As Task
        Await SendWsMessageAsync(New With {.type = "mod_kick", .target = target})
    End Function

    Public Async Function ModTimeoutAsync(target As String, minutes As Integer, reason As String) As Task
        Await SendWsMessageAsync(New With {.type = "mod_timeout", .target = target, .duration = minutes, .reason = reason})
    End Function

    ' Server expects { type, channel, id, reason }; channel is the string key
    ' from ALLOWED_CHANNELS (public|topic|neighborhood|dm|comment).
    Public Async Function ModDeleteMessageAsync(messageId As Integer, channel As String, reason As String) As Task
        Await SendWsMessageAsync(New With {.type = "mod_delete", .channel = channel, .id = messageId, .reason = reason})
    End Function

    Public Async Function ModRestoreMessageAsync(messageId As Integer, channel As String) As Task
        Await SendWsMessageAsync(New With {.type = "mod_restore", .channel = channel, .id = messageId})
    End Function

    Public Async Function PostNeighborhoodThreadAsync(title As String, content As String) As Task
        Await SendWsMessageAsync(New With {.type = "neighborhood_post", .title = title, .content = content})
    End Function

    Public Async Function PostNeighborhoodCommentAsync(postId As Integer, content As String) As Task
        Await SendWsMessageAsync(New With {.type = "neighborhood_comment", .post_id = postId, .content = content})
    End Function

    ' ------------------------------------------------------------------
    ' Portal 2D online packs. These methods match the browser game's API.
    ' ------------------------------------------------------------------
    Public Async Function GetPortal2DViewerAsync() As Task(Of Portal2DViewer)
        Using req = CreateAuthRequest(HttpMethod.Get, "/api/portal2d/me")
            Return Await SendRequestAsync(Of Portal2DViewer)(req)
        End Using
    End Function

    Public Async Function BrowsePortal2DLevelsAsync(index As Integer, Optional mine As Boolean = False) As Task(Of Portal2DLevelPage)
        Dim page = Math.Max(0, index)
        Dim mineQuery = If(mine, "&mine=1", "")
        Using req = CreateAuthRequest(HttpMethod.Get, $"/api/portal2d/levels?index={page}{mineQuery}")
            Return Await SendRequestAsync(Of Portal2DLevelPage)(req)
        End Using
    End Function

    Public Async Function GetPortal2DLevelAsync(id As Integer) As Task(Of Portal2DOnlinePack)
        Using req = CreateAuthRequest(HttpMethod.Get, $"/api/portal2d/levels/{id}")
            Return Await SendRequestAsync(Of Portal2DOnlinePack)(req)
        End Using
    End Function

    Public Async Function PublishPortal2DLevelAsync(name As String, data As JsonElement) As Task(Of Portal2DActionResult)
        Return Await PostJsonAsync(Of Portal2DActionResult)("/api/portal2d/levels", New With {.name = name, .data = data})
    End Function

    Public Async Function PublishPortal2DLevelAsync(name As String, dataJson As String) As Task(Of Portal2DActionResult)
        Using doc = JsonDocument.Parse(dataJson)
            Return Await PublishPortal2DLevelAsync(name, doc.RootElement.Clone())
        End Using
    End Function

    Public Async Function UpdatePortal2DLevelAsync(id As Integer, name As String, data As JsonElement) As Task(Of Portal2DActionResult)
        Return Await PutJsonAsync(Of Portal2DActionResult)($"/api/portal2d/levels/{id}", New With {.name = name, .data = data})
    End Function

    Public Async Function UpdatePortal2DLevelAsync(id As Integer, name As String, dataJson As String) As Task(Of Portal2DActionResult)
        Using doc = JsonDocument.Parse(dataJson)
            Return Await UpdatePortal2DLevelAsync(id, name, doc.RootElement.Clone())
        End Using
    End Function

    Public Async Function DeletePortal2DLevelAsync(id As Integer) As Task
        Await DeleteAsync(Of Object)($"/api/portal2d/levels/{id}")
    End Function

    Public Async Function GetPortal2DOfficialPackAsync() As Task(Of Portal2DOnlinePack)
        Using req = CreateAuthRequest(HttpMethod.Get, "/api/portal2d/official")
            Return Await SendRequestAsync(Of Portal2DOnlinePack)(req)
        End Using
    End Function

End Class
