Imports System.Net.Sockets
Imports System.ComponentModel

Public Class ListenerClass
    Private listenThread As System.Threading.Thread
    Private listener As System.Net.Sockets.TcpListener
    Private clients As New List(Of ClientClass)
    Private channels As New List(Of ChannelClass)

    Private lngPort As Long = 1000
    Private intStatus As Integer = 0
    Private strServerName As String = "server.name.com"
    Private strServerEmail As String = "hyperion.nz@me.com"
    Private strServerDay As Day = Now.Day
    Private strServerDate As String = Now.Date.ToString
    Private strServerTime As String = Now.TimeOfDay.ToString
    Private strMOTD As String = ""

    Public Event onListeningStart()
    Public Event onListeningStop()

    Public Sub New()
        listenThread = New System.Threading.Thread(AddressOf Listen)
        listenThread.IsBackground = True
    End Sub

    Public Property port() As Long
        Get
            Return lngPort
        End Get
        Set(value As Long)
            lngPort = value
        End Set
    End Property

    Public Property ServerName() As String
        Get
            Return strServerName
        End Get
        Set(value As String)
            strServerName = value
        End Set
    End Property

    Public Property ServerEmail() As String
        Get
            Return strServerEmail
        End Get
        Set(value As String)
            strServerEmail = value
        End Set
    End Property

    Public ReadOnly Property status() As Integer
        Get
            Return intStatus
        End Get
    End Property

    Private Sub Listen()
        Dim newClient As System.Net.Sockets.TcpClient

        Do
            newClient = listener.AcceptTcpClient

            Dim connClient As New ClientClass(newClient)

            AddHandler connClient.dataReceived, AddressOf messageReceived

            clients.Add(connClient)
        Loop
    End Sub

    Public Sub startListening()
        listener = New System.Net.Sockets.TcpListener(System.Net.IPAddress.Any, port)
        listener.Start()
        listenThread.Start()
        RaiseEvent onListeningStart()
    End Sub

    Public Sub stopListening()
        listener.Stop()
        RaiseEvent onListeningStop()
        Try
            SyncLock listenThread
                listenThread.Abort()
            End SyncLock
        Catch ex As Threading.ThreadAbortException
        End Try
    End Sub

    Sub Raise(ByVal [event] As [Delegate], ParamArray data As Object())
        'If the event has no handlers just exit the method call.
        If [event] Is Nothing Then Return

        'Enumerates through the list of handlers.
        For Each D As [Delegate] In [event].GetInvocationList()
            'Casts the handler's parent instance to ISynchronizeInvoke.
            Dim T As ISynchronizeInvoke = DirectCast(D.Target, ISynchronizeInvoke)

            'If an invoke is required (working on a seperate thread) then invoke it
            'on the parent thread, otherwise we can invoke it directly.
            If T.InvokeRequired Then T.Invoke(D, data) Else D.DynamicInvoke(data)
        Next
    End Sub

    Private Sub messageReceived(ByVal sender As ClientClass, ByVal message As String)
        Debug.Print(message)
        Dim index As Integer = clients.IndexOf(sender)
        Dim data() As String = message.Split(" "c)

        Select Case data(0).Replace(vbLf, "").ToUpper
            Case "JOIN"
                Join(sender, data(1))
            Case "LIST"
                List(sender)
            Case "MODE"
                If data.Length > 2 Then Mode(sender, data(1), data(2))
            Case "NAMES"
                Names(sender, data(1))
            Case "NICK"
                Nick(sender, data(1))
            Case "PING"
                Ping(sender, data(1))
            Case "PRIVMSG"
                PrivMsg(sender, data(1), message.Split(":"c)(1))
            Case "QUIT"
                removeClient(sender)
            Case "USER"
                User(sender, data(1) & "@" & data(3), message.Split(":"c)(1))
            Case "WHO"
                'Who(sender, data(1))
        End Select
    End Sub

    Private Function GetClientByName(ByVal name As String) As ClientClass
        For Each cc As ClientClass In clients
            If cc.Username = name Then
                Return cc
            End If
        Next

        Return Nothing
    End Function

    Public Sub removeClient(ByVal client As ClientClass)
        If clients.Contains(client) Then
            clients.Remove(client)
            client.Disconnect()
        End If
    End Sub

    Private Sub Nick(ByVal client As ClientClass, ByVal strNewNick As String)
        Dim strOldNick As String = client.Username
        Dim blnFound As Boolean = False
        If Left(strNewNick, 1) = ":" Then strNewNick = Mid(strNewNick, 2)
        If Not strOldNick = strNewNick Then
            Dim c As ClientClass
            If Not strOldNick = Nothing Then
                For Each c In clients
                    If c.Username.ToLower = strNewNick.ToLower Then
                        blnFound = True
                        Exit For
                    End If
                Next
            Else
                client.Username = strNewNick
            End If

            If blnFound = False Then
                client.Username = strNewNick
                For Each c In clients

                Next
            End If
        End If
    End Sub

    Private Sub User(ByVal client As ClientClass, ByVal strEmail As String, ByVal strRealName As String)
        client.Realname = strRealName
        client.Email = strEmail
        client.Hostname = strEmail.Split("@"c)(1)
        client.Connected = True

        client.SendMessage(":" & ServerName & " 001 " & client.Username & " :Welcome to the Cabral Internet Relay Chat Network " & client.Username & vbCrLf)
        client.SendMessage(":" & strServerName & " 002 " & client.Username & " :Your host is " & strServerName & ", running version Cabral IRCD alpha" & vbCrLf)
        client.SendMessage("NOTICE " & client.Username & " :*** Your host is " & strServerName & ", running version Cabral IRCD alpha" & vbCrLf)
        client.SendMessage(":" & strServerName & " 003 " & client.Username & " :This server was created " & strServerDate & " at " & strServerTime.Split("."c)(0) & vbCrLf)
        client.SendMessage(":" & strServerName & " 004 " & client.Username & " " & strServerName & " Cabral IRCD alpha iofcklmnqrv + winsock " & vbCrLf)
        client.SendMessage(":" & strServerName & " 005 " & client.Username & " SPOOF+NO_NUKE+UNABOMBER+CABRAL+" & strServerEmail & vbCrLf)
        MOTD(client)
    End Sub

    Private Sub MOTD(ByVal client As ClientClass)
        client.SendMessage(":" & strServerName & " 375 " & client.Username & " :- " & strServerName & " Message of the Day -" & vbCrLf)
        client.SendMessage(":" & strServerName & " 372 " & client.Username & " :-" & strMOTD & vbCrLf)
        client.SendMessage(":" & strServerName & " 376 " & client.Username & " :End of Message of the Day" & vbCrLf)
    End Sub

    Private Sub Ping(ByVal client As ClientClass, ByVal str As String)
        client.SendMessage("PONG " & str & vbCrLf)
    End Sub

    Private Sub Join(ByVal client As ClientClass, ByVal strChannel As String)
        If Not Left(strChannel, 1) = "#" Then strChannel = "#" & strChannel
        client.SendMessage(":" & client.Username & "!" & client.Email & "@" & client.Hostname & " JOIN :" & strChannel & vbCrLf)
        client.SendMessage(":" & strServerName & " 324 " & client.Username & " " & strChannel & " +" & vbCrLf)

        Dim blnFound As Boolean = False
        Dim c As ChannelClass
        For Each c In channels
            If c.Name = strChannel Then
                blnFound = True
                Exit For
            End If
        Next

        If blnFound = True Then
            c.addUser(client)
        Else
            Dim chan As ChannelClass = New ChannelClass(strChannel)
            client.Admin = True
            chan.addUser(client)
            channels.Add(chan)
        End If

        Names(client, strChannel)
    End Sub

    Private Sub Names(ByVal client As ClientClass, ByVal strChannel As String)
        If strChannel = "" Then
            client.SendMessage(":" & strServerName & " NOTICE " & client.Username & " *** Sorry, you did not specify a channel." & vbCrLf)
            client.SendMessage(":" & strServerName & " NOTICE " & client.Username & " *** NAMES usage: /names <#channel>" & vbCrLf)
        Else
            Dim c As ChannelClass
            Dim str As String = ""
            For Each c In channels
                If c.Name = strChannel Then
                    Dim u As ClientClass
                    For Each u In c.getUsers
                        If str = "" Then
                            If u.Admin = True Then str = "@" & u.Username
                            If u.Admin = False Then str = u.Username
                        Else
                            If u.Admin = True Then str = str & " @" & u.Username
                            If u.Admin = False Then str = str & " " & u.Username
                        End If
                    Next
                    Exit For
                End If
            Next
            client.SendMessage(":" & strServerName & " 353 " & client.Username & " = " & strChannel & " :" & str & vbCrLf)
            client.SendMessage(":" & strServerName & " 366 " & client.Username & " " & strChannel & " :end of /NAMES list." & vbCrLf)
                End If
    End Sub

    Private Sub Mode(ByVal client As ClientClass, ByVal strTarget As String, ByVal strModes As String)
        If strTarget.ToLower = client.Username.ToLower Then
            client.SendMessage(":" & strTarget.ToUpper & " " & "MODE" & " " & strTarget.ToUpper & " :" & strModes & vbCrLf)
        End If
    End Sub

    Private Sub PrivMsg(ByVal client As ClientClass, ByVal strTarget As String, ByVal strMessage As String)
        If Left(strTarget, 1) = "#" Then
            Dim c As ChannelClass
            For Each c In channels
                If c.Name = strTarget Then
                    Names(client, strTarget)
                    Dim u As ClientClass
                    For Each u In c.getUsers
                        If Not client.Username = u.Username Then
                            u.SendMessage(":" & client.Username & "!" & client.Email & "@" & client.Hostname & " PRIVMSG " & strTarget & " " & strMessage & vbCrLf)
                        End If
                    Next
                    Exit For
                End If
            Next
        Else
            Dim blnFound As Boolean = False
            Dim f As ClientClass
            Dim c As ChannelClass
            For Each c In channels
                Dim u As ClientClass
                For Each u In c.getUsers
                    If u.Username = strTarget Then
                        f = u
                        blnFound = True
                        Exit For
                    End If
                Next
                If blnFound = True Then Exit For
            Next

            If blnFound = True Then
                f.SendMessage(":" & client.Username & "!" & client.Email & "@" & client.Hostname & " PRIVMSG " & strTarget & " " & strMessage & vbCrLf)
            Else
                client.SendMessage(":" & client.Username & "!" & client.Email & "@" & client.Hostname & " NOTICE " & strTarget & " ***" & strTarget & " is not online!!!" & vbCrLf)
            End If
        End If
    End Sub

    Sub List(ByVal client As ClientClass)
        Debug.Print("Starting List")
        client.SendMessage(":" & strServerName & " 321 " & client.Username & " Channel :User Names" & vbCrLf)

        Dim c As ChannelClass
        For Each c In channels
            Dim strTopic As String = "NO TOPIC"
            If Not c.Topic = Nothing Then strTopic = c.Topic
            client.SendMessage(":" & strServerName & " 322 " & client.Username & " " & c.Name & " " & c.getUsers.Count & " " & ":" & strTopic & vbCrLf)
        Next

        client.SendMessage(":" & strServerName & " 323 " & client.Username & " :End of /LIST" & vbCrLf)
    End Sub
End Class