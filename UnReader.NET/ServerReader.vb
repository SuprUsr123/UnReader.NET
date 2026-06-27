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
End Class

Public Class UserRolesPayload
    <JsonPropertyName("role")> Public Property Role As ChatRole
    <JsonPropertyName("is_banned")> Public Property IsBanned As Boolean
    <JsonPropertyName("timeout_until")> Public Property TimeoutUntil As Long
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

    Public ReadOnly Property StatusSummary As String
        Get
            Dim sb As New System.Text.StringBuilder()
            sb.AppendLine($"USERNAME : @{Username}")
            sb.AppendLine($"ROLE     : {If(Role IsNot Nothing, Role.RoleName, "member")}")
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

Public Class ServerReader

    Public Property ServerUrl As String
    Public Property JwtToken As String

    Private Shared ReadOnly Client As New HttpClient()
    Private ReadOnly JsonOpts As New JsonSerializerOptions With {
        .PropertyNameCaseInsensitive = True
    }

    Public Event OnRosterUpdated(users As List(Of RosterUser))
    Public Event OnFeedRefreshed()
    Public Event OnTopicsUpdated(topics As List(Of TopicRoom))
    Public Event OnDisconnected(reason As String)
    Public Event OnErrorAlert(message As String)

    Private _ws As ClientWebSocket
    Private _cts As CancellationTokenSource

    Public Sub New(Optional serverUrl As String = "http://localhost:10000", Optional jwtToken As String = "")
        Me.ServerUrl = serverUrl
        Me.JwtToken = jwtToken
    End Sub

    Private Function CreateAuthRequest(method As HttpMethod, endpoint As String) As HttpRequestMessage
        Dim fullUrl As String = ServerUrl.TrimEnd("/"c) & "/" & endpoint.TrimStart("/"c)
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

    Private Async Function PostJsonAsync(Of T)(endpoint As String, payload As Object) As Task(Of T)
        Using req As HttpRequestMessage = CreateAuthRequest(HttpMethod.Post, endpoint)
            Dim jsonString As String = JsonSerializer.Serialize(payload)
            req.Content = New StringContent(jsonString, Encoding.UTF8, "application/json")
            Return Await SendRequestAsync(Of T)(req)
        End Using
    End Function

    Public Async Function ConnectRealtimeAsync() As Task(Of Boolean)
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

            _cts = New CancellationTokenSource()
            _ws = New ClientWebSocket()

            If Not String.IsNullOrWhiteSpace(JwtToken) Then
                _ws.Options.SetRequestHeader("Authorization", "Bearer " & JwtToken)
            End If

            Dim wsUrl As String = ServerUrl.Replace("http://", "ws://").Replace("https://", "wss://")
            Dim uri As New Uri(wsUrl.TrimEnd("/"c) & "/ws")

            Await _ws.ConnectAsync(uri, _cts.Token)

            Await SendWsMessageAsync(New With {.type = "auth", .token = JwtToken})

            Task.Run(Async Function() As Task
                         Await ReceiveLoopAsync()
                     End Function).ConfigureAwait(False)

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
                                        Dim userRolesProp As JsonElement = Nothing
                                        Dim oldRoleProp As JsonElement = Nothing

                                        If item.TryGetProperty("userRoles", userRolesProp) AndAlso userRolesProp.ValueKind = JsonValueKind.Object Then
                                            Dim roleProp As JsonElement = Nothing
                                            If userRolesProp.TryGetProperty("role", roleProp) AndAlso roleProp.ValueKind = JsonValueKind.Object Then
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
                        RaiseEvent OnTopicsUpdated(topicsList)

                End Select
            End Using
        Catch ex As Exception
        End Try
    End Sub

    Public Sub DisconnectRealtime()
        If _cts IsNot Nothing Then _cts.Cancel()
        If _ws IsNot Nothing AndAlso _ws.State = WebSocketState.Open Then
            _ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client closed", CancellationToken.None)
        End If
    End Sub

    Public Async Function SendWsMessageAsync(payload As Object) As Task
        If _ws IsNot Nothing AndAlso _ws.State = WebSocketState.Open Then
            Dim json = JsonSerializer.Serialize(payload)
            Dim bytes = Encoding.UTF8.GetBytes(json)
            Await _ws.SendAsync(New ArraySegment(Of Byte)(bytes), WebSocketMessageType.Text, True, _cts.Token)
        End If
    End Function

    Public Async Function LoginAsync(username As String, password As String) As Task(Of AuthResponse)
        Try
            Dim res = Await PostJsonAsync(Of AuthResponse)("/api/login", New With {.username = username, .password = password})
            Me.JwtToken = res.Token
            Return res
        Catch ex As Exception
            Return New AuthResponse With {.ErrorMessage = ex.Message}
        End Try
    End Function

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

    Public Async Function SetRoleAsync(targetUsername As String) As Task
        Await PostJsonAsync(Of Object)("/api/admin/set-role", New With {.target = targetUsername})
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

    Public Async Function ModDeleteMessageAsync(messageId As Integer, context As String) As Task
        Await SendWsMessageAsync(New With {.type = "mod_delete", .id = messageId, .context = context})
    End Function

    Public Async Function ModRestoreMessageAsync(messageId As Integer, context As String) As Task
        Await SendWsMessageAsync(New With {.type = "mod_restore", .id = messageId, .context = context})
    End Function

    Public Async Function PostNeighborhoodThreadAsync(title As String, content As String) As Task
        Await SendWsMessageAsync(New With {.type = "neighborhood_post", .title = title, .content = content})
    End Function

    Public Async Function PostNeighborhoodCommentAsync(postId As Integer, content As String) As Task
        Await SendWsMessageAsync(New With {.type = "neighborhood_comment", .post_id = postId, .content = content})
    End Function

End Class