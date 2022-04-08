Option Explicit On
Option Strict On

Imports System.IO
Imports System.Collections.Specialized
Imports System.Net
Imports System.Reflection
Imports System.Security.Cryptography.X509Certificates
Imports System.Net.Security
Imports Newtonsoft.Json.Linq

Public Class TextExportFileClass

#Region "  Declarations  "

    Private Const NUMBER_OF_TRIES As Integer = 2
    Private Const DEV_UPLOAD_URL As String = "https://10.10.10.32/CAD.php"

#End Region

#Region "  Auto-Implemented Properties  "

    Public Property File As String
    Public Property eventLog As EventLog
    Public Property AccountNumber As String
    Public Property UploadUrl As String
    Public Property CloudLog As CloudLogClass

#End Region

#Region "  Public Methods  "

    Public Sub UploadFile()

        UploadFile(File, "CAD")

    End Sub

    Public Sub New()

        '' Hookup the callback to accept all certificates
        ServicePointManager.ServerCertificateValidationCallback = New RemoteCertificateValidationCallback(AddressOf CertificateValidationCallBack)

    End Sub

#End Region

#Region "  Private Methods  "

    Private Function UploadFile(fileName As String, fileType As String) As Boolean

        Dim returnValue As Boolean = False
        Dim strResponse As String = String.Empty

        For i = 1 To NUMBER_OF_TRIES

            Try

                Dim nvc As New NameValueCollection()
                nvc.Add("Type", fileType)
                nvc.Add("Account", AccountNumber)
                Dim submitScriptURL As String = UploadUrl

#If DEBUG Then
                submitScriptURL = DEV_UPLOAD_URL
#End If

                strResponse = HttpUploadFile(submitScriptURL, fileName, "uploaded_file", "text/plain", nvc, eventLog)

                Dim objResponse As JObject = JObject.Parse(strResponse)

                If objResponse Is Nothing OrElse Not objResponse.GetValue("Upload").ToString.Contains("Success") Then

                    If Not eventLog Is Nothing Then

                        Try

                            Logger.WriteLogEntry(String.Format("Upload failed for file {0}: Attempt {1} of {2}" & vbCrLf & strResponse, fileName, i.ToString, NUMBER_OF_TRIES.ToString), eventLog)
                            CloudLog.WriteToCloud(String.Format("Upload failed for file {0}: Attempt {1} of {2}" & vbCrLf & strResponse, fileName, i.ToString, NUMBER_OF_TRIES.ToString))

                        Catch ex As Exception
                            ' do nothing

                        End Try

                    End If

                Else

                    'Log Error Object
                    LogResponse(objResponse)

                    '' Success
                    If System.IO.File.Exists(fileName) Then

                        Try
                            System.IO.File.Delete(fileName)
                        Catch ex As Exception
                            ' do nothing
                        End Try

                    End If

                    returnValue = True
                    Exit For


                End If

            Catch jex As Newtonsoft.Json.JsonException
                Logger.WriteLogEntry("Error Parsing Json Response: " & jex.Message & vbCrLf & "Payload = " & strResponse, eventLog)

            Catch WebEx As System.Net.WebException
                Logger.WriteLogEntry("Web exception: " & WebEx.Message, eventLog)

            Catch ex As Exception
                Logger.WriteLogEntry(String.Format("Error sending file {0}: attempt {1} of {2}", fileName, i.ToString, NUMBER_OF_TRIES.ToString), eventLog)
                CloudLog.WriteToCloud("Error sending file: " & ex.Message)

            End Try

        Next

        Return returnValue

    End Function

    Private Function GetJsonPropString(PropertyName As String, ByRef JsonObject As JObject) As String
        If IsNothing(JsonObject) Then Return String.Empty

        Dim value As String = String.Empty

        Try

            value = JsonObject.GetValue(PropertyName, StringComparison.CurrentCultureIgnoreCase).ToString

            ' server returns "null" for actual null values, we want an empty string for our logic
            If value.Equals("null", StringComparison.CurrentCultureIgnoreCase) Then value = String.Empty

        Catch ex As Exception
            ' No action, returning the empty string

        End Try

        Return value

    End Function

    Private Function ApplicationPath() As String

        Return Path.GetDirectoryName([Assembly].GetEntryAssembly().Location)

    End Function

    Function CertificateValidationCallBack(ByVal sender As Object, ByVal certificate As X509Certificate, ByVal chain As X509Chain, ByVal sslPolicyErrors As Security.SslPolicyErrors) As Boolean

        Return True

    End Function

    Private Shared Function HttpUploadFile(url As String, file As String, paramName As String, contentType As String, nvc As NameValueCollection, eventLog As System.Diagnostics.EventLog) As String

        Dim boundary As String = "---------------------------" & DateTime.Now.Ticks.ToString("x")
        Dim boundarybytes As Byte() = System.Text.Encoding.ASCII.GetBytes(vbCr & vbLf & "--" & boundary & vbCr & vbLf)

        Dim wr As HttpWebRequest = DirectCast(WebRequest.Create(url), HttpWebRequest)
        wr.ContentType = "multipart/form-data; boundary=" & boundary
        wr.Method = "POST"
        wr.KeepAlive = True
        wr.Credentials = System.Net.CredentialCache.DefaultCredentials
        wr.Proxy = Nothing

        Dim rs As Stream = wr.GetRequestStream()

        Dim formdataTemplate As String = "Content-Disposition: form-data; name=""{0}""" & vbCr & vbLf & vbCr & vbLf & "{1}"
        For Each key As String In nvc.Keys
            rs.Write(boundarybytes, 0, boundarybytes.Length)
            Dim formitem As String = String.Format(formdataTemplate, key, nvc(key))
            Dim formitembytes As Byte() = System.Text.Encoding.UTF8.GetBytes(formitem)
            rs.Write(formitembytes, 0, formitembytes.Length)
        Next
        rs.Write(boundarybytes, 0, boundarybytes.Length)

        Dim headerTemplate As String = "Content-Disposition: form-data; name=""{0}""; filename=""{1}""" & vbCr & vbLf & "Content-Type: {2}" & vbCr & vbLf & vbCr & vbLf
        Dim header As String = String.Format(headerTemplate, paramName, file, contentType)
        Dim headerbytes As Byte() = System.Text.Encoding.UTF8.GetBytes(header)
        rs.Write(headerbytes, 0, headerbytes.Length)

        Dim fileStream As New FileStream(file, FileMode.Open, FileAccess.Read)
        Dim buffer As Byte() = New Byte(4095) {}
        Dim bytesRead As Integer = 0
        While (InlineAssignHelper(bytesRead, fileStream.Read(buffer, 0, buffer.Length))) <> 0
            rs.Write(buffer, 0, bytesRead)
        End While
        fileStream.Close()

        Dim trailer As Byte() = System.Text.Encoding.ASCII.GetBytes(vbCr & vbLf & "--" & boundary & "--" & vbCr & vbLf)
        rs.Write(trailer, 0, trailer.Length)
        rs.Close()

        Dim wresp As WebResponse = Nothing
        Try
            wresp = wr.GetResponse()
            Dim stream2 As Stream = wresp.GetResponseStream()
            Dim reader2 As New StreamReader(stream2)
            Return reader2.ReadToEnd

        Catch ex As Exception

            Logger.WriteLogEntry(String.Format("Error uploading file: {0}", ex.ToString), eventLog)


            If wresp IsNot Nothing Then
                wresp.Close()
                wresp = Nothing
            End If
        Finally
            wr = Nothing
        End Try

        Return ""

    End Function

    Private Shared Function InlineAssignHelper(Of T)(ByRef target As T, ByVal value As T) As T

        target = value : Return value

    End Function

    Private Sub LogResponse(objResponse As JObject)

        Try
            Dim sb As New System.Text.StringBuilder

            With sb
                .Append("File Upload Response: " & vbCrLf & vbCrLf)

                .Append("UPLOAD" & vbCrLf)
                .AppendFormat("Type: {0}{1}{2}", vbTab & vbTab, objResponse.GetValue("Upload", StringComparison.CurrentCultureIgnoreCase)("Type").ToString, vbCrLf)
                .AppendFormat("Message: {0}{1}{2}", vbTab, objResponse.GetValue("Upload", StringComparison.CurrentCultureIgnoreCase)("message").ToString, vbCrLf)
                .AppendFormat("Filesize: {0}{1}{2}", vbTab & vbTab, objResponse.GetValue("Upload", StringComparison.CurrentCultureIgnoreCase)("Filesize").ToString, vbCrLf)
                .AppendFormat("MD5: {0}{1}{2}", vbTab, objResponse.GetValue("Upload", StringComparison.CurrentCultureIgnoreCase)("MD5").ToString, vbCrLf)

                .Append(vbCrLf & vbCrLf)

                .AppendFormat("PROCESSING" & vbCrLf)
                .AppendFormat("Type: {0}{1}{2}", vbTab & vbTab, objResponse.GetValue("Processing", StringComparison.CurrentCultureIgnoreCase)("Type").ToString, vbCrLf)
                .AppendFormat("Message: {0}{1}{2}", vbTab, objResponse.GetValue("Processing", StringComparison.CurrentCultureIgnoreCase)("message").ToString, vbCrLf)

            End With

            Logger.WriteLogEntry(sb.ToString, eventLog)

        Catch ex As Exception

        End Try

    End Sub


#End Region

End Class
