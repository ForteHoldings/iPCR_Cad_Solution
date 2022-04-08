Option Explicit On
Option Strict On

Imports System.Net
Imports System.Threading
Imports System.ComponentModel
Imports System.Text
Imports System.IO
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq

Public Class CloudLogClass

#Region "  Declarations  "

    Friend WithEvents mBgWorker As BackgroundWorker
    Private mEventLog As EventLog
    Private mAccountNo As String

    Private Const SERVER_ERROR_URL As String = "http://data.ipcrems.com/CADerrorreporting.php"

#End Region

#Region "  Public Methods  "

    Public Sub New(AccountNo As String, EventLog As EventLog)
        mAccountNo = AccountNo
        mEventLog = EventLog
    End Sub

    Public Sub WriteToCloud(Message As String)

        Try
            '' Setup our worker
            mBgWorker = New BackgroundWorker() With {.WorkerReportsProgress = False, .WorkerSupportsCancellation = False}

            '' Serialize our object and pass it as an argument to the worker
            mBgWorker.RunWorkerAsync(JsonConvert.SerializeObject(New CadErrorMessage With {.account = mAccountNo, .message = Message}))

        Catch ex As Exception
            ' Nothing, we don't want anything bubbling up from logging to our server.

        End Try

    End Sub

#End Region

#Region "  Private Methods  "

    Private Sub LogInEventViewer(Message As String, Optional ByVal Type As EventLogEntryType = EventLogEntryType.Information)
        If mEventLog Is Nothing OrElse String.IsNullOrEmpty(Message) Then Exit Sub

        mEventLog.WriteEntry(Message, Type)

    End Sub

    Private Function PostCadMessageToCloud(JsonMessage As String) As String

        Dim postByteData() As Byte
        Dim webRequest As WebRequest
        Dim streamReader As StreamReader
        Dim reqStream, respStream As System.IO.Stream

        Try

            postByteData = UTF8Encoding.UTF8.GetBytes(JsonMessage)

            webRequest = webRequest.Create(SERVER_ERROR_URL)
            With webRequest
                .Method = "POST"
                .ContentType = "application/json"
                .CachePolicy = New Cache.RequestCachePolicy(Cache.RequestCacheLevel.NoCacheNoStore)
                .ContentLength = postByteData.Length
            End With

            reqStream = webRequest.GetRequestStream()
            reqStream.Write(postByteData, 0, postByteData.Length)

            respStream = webRequest.GetResponse().GetResponseStream()
            streamReader = New StreamReader(respStream)

            Return streamReader.ReadToEnd()

        Catch ex As Exception
            LogInEventViewer("Exception in PostCadMessageToCloud: " & ex.Message)

        Finally
            If reqStream IsNot Nothing Then reqStream.Close()
            If streamReader IsNot Nothing Then streamReader.Close()
            If respStream IsNot Nothing Then respStream.Close()

        End Try

    End Function

    Private Sub LogPostResponse(ResponseText As String)

        Dim strType, strMessage As String

        Try
            If String.IsNullOrEmpty(ResponseText) Then
                LogInEventViewer("Post response came back null or empty.", EventLogEntryType.Warning)
                Exit Sub
            End If

            Dim respObj As JObject = JObject.Parse(ResponseText)
            strType = respObj.GetValue("Type", StringComparison.CurrentCultureIgnoreCase).ToString
            strMessage = respObj.GetValue("Message", StringComparison.CurrentCultureIgnoreCase).ToString

            '' If it's not a success, then log the message
            If Not strType.Equals("Success", StringComparison.CurrentCultureIgnoreCase) Then

                LogInEventViewer("CAD Post Failure, response message: " & strMessage, EventLogEntryType.Error)

            End If

        Catch ex As Exception
            ' nothing to do if we can't even log

        End Try

    End Sub

#End Region

#Region "  Event Handlers  "

    Private Sub bgWorker_DoWork(sender As Object, e As DoWorkEventArgs) Handles mBgWorker.DoWork

        Try

            e.Result = PostCadMessageToCloud(e.Argument.ToString)

        Catch ex As Exception
            LogInEventViewer("Error posting to server in background thread.")

        End Try

    End Sub

    Private Sub bgWorker_RunWorkerCompleted(sender As Object, e As RunWorkerCompletedEventArgs) Handles mBgWorker.RunWorkerCompleted

        If e.Error IsNot Nothing Then

            LogInEventViewer("Error reported in background worker: " & e.Error.Message, EventLogEntryType.Error)

        Else
            '' No errors occured, we should have a good response
            LogPostResponse(e.Result.ToString)

        End If

    End Sub

#End Region

    Private Class CadErrorMessage

        Property account As String
        Property message As String

        Public Sub New()
            account = String.Empty
            message = String.Empty
        End Sub

    End Class

End Class
