Public Class ChannelClass
    Private mName As String
    Private mTopic As String
    Private users As New List(Of ClientClass)

    Sub New(ByVal _Name As String)
        mName = _Name
    End Sub

    Public Sub addUser(ByVal client As ClientClass)
        'Dim user As UserClass = New UserClass
        'user.Email = client.Email
        'user.Hostname = client.Hostname
        'user.Realname = client.Realname
        'user.Username = client.Username
        users.Add(client)
    End Sub

    Public Sub removeUser(ByVal client As ClientClass)
        Dim user As UserClass = New UserClass
        user.Email = client.Email
        user.Hostname = client.Hostname
        user.Realname = client.Realname
        user.Username = client.Username
        If users.Contains(client) Then
            users.Remove(client)
        End If
    End Sub

    Public ReadOnly Property Name() As String
        Get
            Return mName
        End Get
    End Property

    Public Property Topic() As String
        Get
            Return mTopic
        End Get
        Set(value As String)
            mTopic = value
        End Set
    End Property

    Public ReadOnly Property getUsers() As List(Of ClientClass)
        Get
            Return users
        End Get
    End Property
End Class

Public Class UserClass
    Private mUsername As String
    Private mEmail As String
    Private mHostname As String
    Private mRealname As String
    Private mAdmin As Boolean

    Sub New()
    End Sub

    Public Property Admin() As Boolean
        Get
            Return mAdmin
        End Get

        Set(ByVal value As Boolean)
            mAdmin = value
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

    Public Property Hostname() As String
        Get
            Return mHostname
        End Get

        Set(ByVal value As String)
            mHostname = value
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
            Return mRealname
        End Get

        Set(ByVal value As String)
            mRealname = value
        End Set
    End Property
End Class