Imports System
Imports System.Diagnostics
Imports System.IO
Imports System.Runtime.InteropServices
Imports System.Text

' Uses the current OS credential store. Password values are never written to
' AppSettings or placed in process arguments.
Public Module PlatformCredentialStore
    Public ReadOnly Property IsSupported As Boolean
        Get
            If OperatingSystem.IsWindows() Then Return True
            If OperatingSystem.IsLinux() Then
                Dim searchPath = Environment.GetEnvironmentVariable("PATH")
                If String.IsNullOrWhiteSpace(searchPath) Then Return False
                For Each directory In searchPath.Split(System.IO.Path.PathSeparator)
                    If Not String.IsNullOrWhiteSpace(directory) AndAlso File.Exists(System.IO.Path.Combine(directory, "secret-tool")) Then Return True
                Next
            End If
            Return False
        End Get
    End Property

    Public Function ReadPassword(server As String, username As String, Optional purpose As String = "login-password") As String
        If OperatingSystem.IsWindows() Then Return ReadWindowsPassword(server, username, purpose)
        If OperatingSystem.IsLinux() Then Return RunSecretTool("lookup", server, username, Nothing, purpose)
        Return String.Empty
    End Function

    Public Function StorePassword(server As String, username As String, password As String, Optional purpose As String = "login-password") As Boolean
        If OperatingSystem.IsWindows() Then Return StoreWindowsPassword(server, username, password, purpose)
        If OperatingSystem.IsLinux() Then Return RunSecretTool("store", server, username, password, purpose) IsNot Nothing
        Return False
    End Function

    Public Sub ClearPassword(server As String, username As String, Optional purpose As String = "login-password")
        If OperatingSystem.IsWindows() Then
            DeleteWindowsPassword(server, username, purpose)
        ElseIf OperatingSystem.IsLinux() Then
            RunSecretTool("clear", server, username, Nothing, purpose)
        End If
    End Sub

    Private Function RunSecretTool(operation As String, server As String, username As String, password As String, purpose As String) As String
        Try
            Dim startInfo As New ProcessStartInfo("secret-tool") With {.UseShellExecute = False, .CreateNoWindow = True}
            startInfo.ArgumentList.Add(operation)
            If operation = "store" Then startInfo.ArgumentList.Add("--label=UnReader.NET credential")
            startInfo.ArgumentList.Add("application")
            startInfo.ArgumentList.Add("UnReader.NET")
            startInfo.ArgumentList.Add("purpose")
            startInfo.ArgumentList.Add(purpose)
            startInfo.ArgumentList.Add("server-url")
            startInfo.ArgumentList.Add(server)
            startInfo.ArgumentList.Add("username")
            startInfo.ArgumentList.Add(username)
            startInfo.RedirectStandardInput = operation = "store"
            startInfo.RedirectStandardOutput = operation = "lookup"

            Using process As Process = Process.Start(startInfo)
                If process Is Nothing Then Return Nothing
                If operation = "store" Then
                    process.StandardInput.Write(password)
                    process.StandardInput.Close()
                ElseIf operation = "lookup" Then
                    Dim value = process.StandardOutput.ReadToEnd()
                    process.WaitForExit()
                    If process.ExitCode <> 0 Then Return String.Empty
                    Return value.TrimEnd(ControlChars.Cr, ControlChars.Lf)
                End If
                process.WaitForExit()
                Return If(process.ExitCode = 0, String.Empty, Nothing)
            End Using
        Catch
            Return Nothing
        End Try
    End Function

    Private Function WindowsTargetName(server As String, username As String, purpose As String) As String
        If purpose = "login-password" Then Return $"UnReader.NET/login/{server}/{username}"
        Return $"UnReader.NET/{purpose}/{server}/{username}"
    End Function

    Private Function ReadWindowsPassword(server As String, username As String, purpose As String) As String
        Dim credentialPointer As IntPtr = IntPtr.Zero
        Try
            If Not CredRead(WindowsTargetName(server, username, purpose), 1UI, 0UI, credentialPointer) Then Return String.Empty
            Dim credential = Marshal.PtrToStructure(Of NativeCredential)(credentialPointer)
            If credential.CredentialBlob = IntPtr.Zero OrElse credential.CredentialBlobSize = 0 Then Return String.Empty
            Dim bytes(CInt(credential.CredentialBlobSize) - 1) As Byte
            Marshal.Copy(credential.CredentialBlob, bytes, 0, bytes.Length)
            Return Encoding.UTF8.GetString(bytes)
        Catch
            Return String.Empty
        Finally
            If credentialPointer <> IntPtr.Zero Then CredFree(credentialPointer)
        End Try
    End Function

    Private Function StoreWindowsPassword(server As String, username As String, password As String, purpose As String) As Boolean
        Dim secretBytes = Encoding.UTF8.GetBytes(password)
        Dim blob As IntPtr = IntPtr.Zero
        Try
            blob = Marshal.AllocHGlobal(secretBytes.Length)
            Marshal.Copy(secretBytes, 0, blob, secretBytes.Length)
            Dim credential As New NativeCredential With {
                .Type = 1UI,
                .TargetName = WindowsTargetName(server, username, purpose),
                .CredentialBlobSize = CUInt(secretBytes.Length),
                .CredentialBlob = blob,
                .Persist = 2UI,
                .UserName = username
            }
            Return CredWrite(credential, 0UI)
        Catch
            Return False
        Finally
            If blob <> IntPtr.Zero Then Marshal.FreeHGlobal(blob)
            Array.Clear(secretBytes, 0, secretBytes.Length)
        End Try
    End Function

    Private Sub DeleteWindowsPassword(server As String, username As String, purpose As String)
        Try
            CredDelete(WindowsTargetName(server, username, purpose), 1UI, 0UI)
        Catch
        End Try
    End Sub

    <StructLayout(LayoutKind.Sequential, CharSet:=CharSet.Unicode)>
    Private Structure NativeCredential
        Public Flags As UInteger
        Public Type As UInteger
        <MarshalAs(UnmanagedType.LPWStr)> Public TargetName As String
        <MarshalAs(UnmanagedType.LPWStr)> Public Comment As String
        Public LastWrittenLow As UInteger
        Public LastWrittenHigh As UInteger
        Public CredentialBlobSize As UInteger
        Public CredentialBlob As IntPtr
        Public Persist As UInteger
        Public AttributeCount As UInteger
        Public Attributes As IntPtr
        <MarshalAs(UnmanagedType.LPWStr)> Public TargetAlias As String
        <MarshalAs(UnmanagedType.LPWStr)> Public UserName As String
    End Structure

    <DllImport("advapi32.dll", EntryPoint:="CredReadW", CharSet:=CharSet.Unicode, SetLastError:=True)>
    Private Function CredRead(target As String, credentialType As UInteger, flags As UInteger, ByRef credential As IntPtr) As Boolean
    End Function

    <DllImport("advapi32.dll", EntryPoint:="CredWriteW", CharSet:=CharSet.Unicode, SetLastError:=True)>
    Private Function CredWrite(ByRef credential As NativeCredential, flags As UInteger) As Boolean
    End Function

    <DllImport("advapi32.dll", EntryPoint:="CredDeleteW", CharSet:=CharSet.Unicode, SetLastError:=True)>
    Private Function CredDelete(target As String, credentialType As UInteger, flags As UInteger) As Boolean
    End Function

    <DllImport("advapi32.dll", EntryPoint:="CredFree")>
    Private Sub CredFree(buffer As IntPtr)
    End Sub
End Module
