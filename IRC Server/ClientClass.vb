Public Class ClientClass
    Private mClient As System.Net.Sockets.TcpClient
    Private readThread As System.Threading.Thread

    Private mUsername As String
    Private mRealName As String
    Private mAdmin As Boolean
    Private mEmail As String
    Private mHost As String
    Private mConnected As Boolean
    Private Const MESSAGE_DELIMITER As Char = ControlChars.Cr

    Public Event dataReceived(ByVal sender As ClientClass, ByVal message As String)

    Sub New(ByVal client As System.Net.Sockets.TcpClient)
        mClient = client
        readThread = New System.Threading.Thread(AddressOf doRead)
        readThread.IsBackground = True
        readThread.Start()
    End Sub

    Public Property Admin() As Boolean
        Get
            Return mAdmin
        End Get

        Set(ByVal value As Boolean)
            mAdmin = value
        End Set
    End Property

    Public Property Connected() As Boolean
        Get
            Return mConnected
        End Get

        Set(ByVal value As Boolean)
            mConnected = value
        End Set
    End Property

    Public Property Email() As String
        Get
            Return mEmail
        End Get

        Set(ByVal value As String)
            mEmail = value
        End Set
    End Property

    Public Property Username() As String
        Get
            Return mUsername
        End Get

        Set(ByVal value As String)
            mUsername = value
        End Set
    End Property

    Public Property Realname() As String
        Get
            Return mRealName
        End Get

        Set(ByVal value As String)
            mRealName = value
        End Set
    End Property

    Public Property Hostname() As String
        Get
            Return mHost
        End Get

        Set(ByVal value As String)
            mHost = value
        End Set
    End Property

    Private Sub doRead()
        Const BYTES_TO_READ As Integer = 255

        Dim readBuffer(BYTES_TO_READ) As Byte
        Dim bytesRead As Integer
        Dim sBuilder As New System.Text.StringBuilder

        Do
            bytesRead = mClient.GetStream.Read(readBuffer, 0, BYTES_TO_READ)
            If (bytesRead > 0) Then
                Dim message As String = System.Text.Encoding.UTF8.GetString(readBuffer, 0, bytesRead)

                If (message.IndexOf(MESSAGE_DELIMITER) > -1) Then
                    Dim subMessages() As String = message.Split(MESSAGE_DELIMITER)

                    sBuilder.Append(subMessages(0))
                    RaiseEvent dataReceived(Me, sBuilder.ToString)
                    sBuilder = New System.Text.StringBuilder
                    If subMessages.Length = 2 Then
                        sBuilder.Append(subMessages(1))
                    Else
                        For i As Integer = 1 To subMessages.GetUpperBound(0) - 1
                            RaiseEvent dataReceived(Me, subMessages(i))
                        Next
                        sBuilder.Append(subMessages(subMessages.GetUpperBound(0)))
                    End If
                Else
                    sBuilder.Append(message)
                End If
            End If
        Loop
    End Sub

    Public Sub SendMessage(ByVal msg As String)
        Dim sw As IO.StreamWriter

        Try
            SyncLock mClient.GetStream
                sw = New IO.StreamWriter(mClient.GetStream)
                sw.Write(msg)
                sw.Flush()
            End SyncLock
        Catch ex As Exception
            MessageBox.Show(ex.ToString)
        End Try
    End Sub

    Public Sub Disconnect()
        mClient.Close()
        readThread.Abort()
        Try
            SyncLock readThread
                readThread.Abort()
            End SyncLock
        Catch ex As Exception
        End Try
    End Sub
End Class
