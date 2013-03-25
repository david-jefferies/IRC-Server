Public Class frmMain
    Public WithEvents socket As ListenerClass

    Public Sub New()
        InitializeComponent()

        socket = New ListenerClass
        socket.port = 1234
        socket.startListening()
    End Sub

    Private Sub socket_onListeningStart() Handles socket.onListeningStart
        txtLog.AppendText("Started Listening")
    End Sub

    Private Sub socket_onListeningStop() Handles socket.onListeningStop
        txtLog.AppendText("Stopped Listening")
    End Sub
End Class
