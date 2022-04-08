Option Explicit On
Option Strict On

Imports System.ServiceProcess
Imports System.Text
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq
Imports System.Net
Imports System.Security.Cryptography.X509Certificates
Imports System.Net.Security

Public Class MainForm

#Region "  Declarations  "

    Private mChangesDetected As Boolean = False
    Private mProductCodeChanged As Boolean = False

    Private Const SQL_SETTINGS As String = "SQL"
    Private Const DEV_REGISTER_URL As String = "http://10.10.10.30/registration/cadregister.php"
    'Private Const PRODUCTION_REGISTER_URL As String = "https://data.ipcrems.com/registration/registercad.php"
    Private Const PRODUCTION_REGISTER_URL As String = "http://data.ipcrems.com/registration/registercad.php" ' Per MW, dropped 's

    'Private Const MRESCAD_PRODUCT_CODE As String = "droupiewlup5asoephie"
    'Private Const ARMSCAD_PRODUCT_CODE As String = "vous5ouphleqo0phousw"

#End Region

#Region "  Event Handlers  "

    Private Sub btnClose_Click(sender As System.Object, e As System.EventArgs) Handles btnClose.Click

        Me.Close()

    End Sub

    Private Sub MainForm_FormClosing(sender As Object, e As System.Windows.Forms.FormClosingEventArgs) Handles Me.FormClosing

        If mChangesDetected Then

            Select Case MessageBox.Show("Changes were detected to your settings. Do you want to save them now?", Me.Text, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button3)

                Case Windows.Forms.DialogResult.Yes
                    If Not SaveSettings() Then

                        e.Cancel = True

                    End If

                Case Windows.Forms.DialogResult.No
                    ' do nothing

                Case Windows.Forms.DialogResult.Cancel
                    e.Cancel = True

            End Select

        End If

    End Sub

    Private Sub btnSave_Click(sender As Object, e As System.EventArgs) Handles btnSave.Click

        Me.Cursor = Cursors.WaitCursor

        Try
            If SaveSettings() Then

                If MessageBox.Show("The service must be started/restarted for these settings to take effect. Do you want to restart the service now?", Me.Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) = Windows.Forms.DialogResult.Yes Then

                    RestartService()

                End If

            End If

        Catch ex As Exception
            MessageBox.Show(ex.ToString)

        Finally
            Me.Cursor = Cursors.Default

        End Try

    End Sub

    Function CertificateValidationCallBack(ByVal sender As Object, ByVal certificate As X509Certificate, ByVal chain As X509Chain, ByVal sslPolicyErrors As Security.SslPolicyErrors) As Boolean

        Return True

    End Function

    Private Sub btnRestartService_Click(sender As Object, e As System.EventArgs) Handles btnRestartService.Click

        Me.Cursor = Cursors.WaitCursor
        Dim cancel As Boolean = False

        Try
            If mChangesDetected Then

                Select Case MessageBox.Show("Changes were detected to your settings. Do you want to save them before restarting the service?", Me.Text, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button3)

                    Case Windows.Forms.DialogResult.Yes
                        If Not SaveSettings() Then

                            cancel = True

                        End If

                    Case Windows.Forms.DialogResult.No
                        ' do nothing

                    Case Windows.Forms.DialogResult.Cancel
                        cancel = True

                End Select

            End If

            If Not cancel Then RestartService()

        Catch ex As Exception
            MessageBox.Show(ex.ToString)

        Finally
            Me.Cursor = Cursors.Default

        End Try

    End Sub

    Private Sub cboCADProgram_SelectedIndexChanged(sender As Object, e As System.EventArgs) Handles cboCADProgram.SelectedIndexChanged

        Me.mChangesDetected = True

    End Sub

    Private Sub txtExportDirectory_TextChanged(sender As Object, e As System.EventArgs) Handles txtExportDirectory.TextChanged

        Me.mChangesDetected = True

    End Sub

    Private Sub MainForm_Load(sender As Object, e As System.EventArgs) Handles Me.Load

        Try

            Me.cboCADProgram.Items.Clear()
            Me.cboCADProgram.Items.Add("RightCAD")

            Me.LoadSettings()

            ServicePointManager.Expect100Continue = True
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12
            ServicePointManager.ServerCertificateValidationCallback = New RemoteCertificateValidationCallback(AddressOf CertificateValidationCallBack)


        Catch ex As Exception
            MessageBox.Show(ex.ToString, Me.Text, MessageBoxButtons.OK, MessageBoxIcon.Error)

        End Try

    End Sub

    Private Sub btnBrowseExportDirectory_Click(sender As Object, e As System.EventArgs) Handles btnBrowseExportDirectory.Click

        If System.IO.Directory.Exists(Me.txtExportDirectory.Text) Then
            Me.FolderBrowserDialog1.SelectedPath = Me.txtExportDirectory.Text
        End If

        If Me.FolderBrowserDialog1.ShowDialog(Me) = Windows.Forms.DialogResult.OK Then
            Me.txtExportDirectory.Text = Me.FolderBrowserDialog1.SelectedPath
        End If

    End Sub

    Private Sub txtProductCode_TextChanged(sender As Object, e As System.EventArgs) Handles txtProductCode.TextChanged

        mChangesDetected = True
        mProductCodeChanged = True

    End Sub

    Private Sub btnConfigureSQL_Click(sender As System.Object, e As System.EventArgs) Handles btnConfigureSQL.Click

        Dim sqlCredentials As New SQLCredentialsForm
        sqlCredentials.ShowDialog()

    End Sub

#End Region

#Region "  Private Methods  "

    Private Sub LoadSettings()

        Dim settings As New SettingsClass(System.IO.Path.Combine(Application.StartupPath, "iPCRCADModule.config"))

        Me.txtProductCode.Text = settings.ReadValue("Main", "ProductCode", "")
        Me.txtExportDirectory.Text = settings.ReadValue("Main", "ExportDirectory", "")

        Me.mChangesDetected = False
        Me.mProductCodeChanged = False

        Dim fileSystemWatcherEvent = settings.ReadValue("Main", "FileSystemWatcherEvent", "Changed")

        If fileSystemWatcherEvent.Trim.ToUpper = "CREATED" Then
            chkOnCreated.CheckState = CheckState.Checked
        Else
            chkOnCreated.CheckState = CheckState.Unchecked
        End If

    End Sub

    Private Function SaveSettings() As Boolean

        Dim accountNumber As String = String.Empty
        Dim compositeCadQuery As String = String.Empty
        Dim uploadUrl As String = String.Empty

        If Me.txtProductCode.Text.Trim.Length = 0 Then

            MessageBox.Show("A product code is required.", "Invalid Product Code", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return False

        Else

            'If Me._productCodeChanged Then
            '' Removed condition, have it hit the web and query every time
            Dim names() As String = {"key"}
            Dim values() As String = {Me.txtProductCode.Text.Trim}
            Dim targetUrl As String = PRODUCTION_REGISTER_URL

#If DEBUG Then
            targetUrl = DEV_REGISTER_URL
#End If
            Dim strResponse As String = Me.FormPost(targetUrl, names, values)

            'Dim errorText As String = String.Empty
            'Dim doc As New Xml.XmlDocument
            'doc.LoadXml(strResponse)

            'For Each node As Xml.XmlNode In doc.ChildNodes(1).ChildNodes

            '    Select Case node.Name.ToUpper

            '        Case "ACCOUNTNUMBER"
            '            accountNumber = node.InnerText

            '        Case "CADQUERY"
            '            compositeCadQuery = ReturnCompositeQueryFromBase64(node.InnerText)
            '            Me.txtExportDirectory.Enabled = False

            '        Case "ERRORTEXT"
            '            errorText = node.InnerText
            '            MessageBox.Show("The product code that you entered is invalid. Please verify your product code and try again.", "Invalid Product Code", MessageBoxButtons.OK, MessageBoxIcon.Error)
            '            Return False

            '    End Select

            'Next

            '' Switched to JSON instead of XML above
            Dim regResponseObj As JObject = JObject.Parse(strResponse)

            If GetJsonPropString("type", regResponseObj).Equals("failure") Then

                Dim strFailureMsg As String = GetJsonPropString("message", regResponseObj)

                If String.IsNullOrEmpty(strFailureMsg) Then

                    '' Existing error message for any error text
                    MessageBox.Show("The product code that you entered is invalid. Please verify your product code and try again.", "Invalid Product Code", MessageBoxButtons.OK, MessageBoxIcon.Error)

                Else

                    MessageBox.Show("The server responded:" & vbCrLf & strFailureMsg, "Unable To Register", MessageBoxButtons.OK, MessageBoxIcon.Error)
                    Return False

                End If

            End If

            ' Made it here, success with the server
            accountNumber = GetJsonPropString("account", regResponseObj)
            compositeCadQuery = GetJsonPropString("query", regResponseObj)
            uploadUrl = GetJsonPropString("url", regResponseObj)

        End If

        '' Only need to check the directory if this isn't a cad based integration.  IF the query field is blank, then no need to verify the directory.
        If String.IsNullOrEmpty(compositeCadQuery) Then

            If Not System.IO.Directory.Exists(Me.txtExportDirectory.Text) Then

                MessageBox.Show(String.Format("The export directory {0} is invalid.", Me.txtExportDirectory.Text), "Invalid Export Directory", MessageBoxButtons.OK, MessageBoxIcon.Error)
                Return False
            End If


        End If

        If String.IsNullOrEmpty(uploadUrl) Then

            MessageBox.Show("The upload path was not found.  Please contact technical support.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return False

        End If

        Dim fileSystemWatcherEvent As String

        If chkOnCreated.CheckState = CheckState.Checked Then
            fileSystemWatcherEvent = "Created"
        Else
            fileSystemWatcherEvent = "Changed"
        End If

        Dim settings As New SettingsClass(System.IO.Path.Combine(Application.StartupPath, "iPCRCADModule.config"))

        If Me.mProductCodeChanged Then
            settings.WriteValue("Main", "ProductCode", Me.txtProductCode.Text.Trim)
            settings.WriteValue("Main", "AccountNumber", accountNumber)
        End If

        settings.WriteValue("Main", "ExportDirectory", Me.txtExportDirectory.Text.Trim)
        settings.WriteValue("Main", "UploadUrl", uploadUrl.Trim)
        settings.WriteValue(SQL_SETTINGS, "CADQueries", compositeCadQuery)
        settings.WriteValue("Main", "FileSystemWatcherEvent", fileSystemWatcherEvent)

        mChangesDetected = False
        mProductCodeChanged = False

        Return True

    End Function

    Private Sub RestartService()

        Dim sc1 As ServiceController

        Try

            Dim millisec1 As Integer = Environment.TickCount
            Dim timeout As TimeSpan = TimeSpan.FromMilliseconds(30000)

            sc1 = New ServiceController("iPCRCADModuleService")

            If sc1.CanStop Then

                sc1.Stop()
                sc1.WaitForStatus(ServiceControllerStatus.Stopped, timeout)

            End If

            sc1.Start()
            sc1.WaitForStatus(ServiceControllerStatus.Running, timeout)

        Catch ex As Exception
            Throw

        End Try

    End Sub

    Private Function FormPost(ByVal url As String, ByVal valueNames() As String, ByVal valueValues() As String) As String

        ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3
        Dim myWebClient As New System.Net.WebClient With {.Proxy = Nothing}
        Dim myNameValueCollection As New System.Collections.Specialized.NameValueCollection
        Dim value As String = String.Empty

        For i As Integer = 0 To valueNames.Length - 1
            myNameValueCollection.Add(valueNames(i), valueValues(i))
        Next

        Try

            Dim responseArray As Byte() = myWebClient.UploadValues(url, "post", myNameValueCollection)
            value = Encoding.ASCII.GetString(responseArray)

        Catch ex As Exception
            Throw

        End Try

        Return value

    End Function

    Private Function ReturnCompositeQueryFromBase64(ByVal strBase64 As String) As String

        Return System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(strBase64))

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

#End Region

End Class
