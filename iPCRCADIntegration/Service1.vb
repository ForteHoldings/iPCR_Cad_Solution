Option Explicit On
Option Strict On

Imports System.IO
Imports System.Configuration
Imports System.Threading
Imports System.Diagnostics
Imports System.Reflection
Imports System.Data.SqlClient
Imports System.Net
Imports System.Net.Security
Imports System.Security.Cryptography.X509Certificates

Public Class Service1

#Region "  Declarations  "

    Private mFileSystemWatcher As FileSystemWatcher
    Private mTimerSql As System.Timers.Timer = Nothing
    Private mAccount As String
    Private mLastFileBytes() As Byte
    Private mLastRunIdImported As Integer = Integer.MinValue
    Private mIsSqlImportRunning As Boolean = False
    Private mMaxRunQuery As String = String.Empty
    Private mCadRecordQuery As String = String.Empty
    Private mUploadUrl As String = String.Empty
    Private mCloudLog As CloudLogClass

    Private Const SQL_SETTINGS As String = "SQL"
    Private Const CONFIG_FILE As String = "iPCRCADModule.config"
    Private Const EVENTLOG_SOURCE As String = "iPCRCADModule"
    Private Const EVENTLOG_LOG As String = "iPCRCADModuleLog"


    '' Account Codes for testing
    'Private Const MRESCAD_PRODUCT_CODE As String = "droupiewlup5asoephie"
    'Private Const ARMSCAD_PRODUCT_CODE As String = "vous5ouphleqo0phousw"  

#End Region

#Region "  Overridden Methods  "

    Protected Overrides Sub OnStart(ByVal args() As String)

        objEventLog.Source = EVENTLOG_SOURCE
        objEventLog.Log = EVENTLOG_LOG

        ' Allows service to be run in debug mode
#If DEBUG Then

        While Not Debugger.IsAttached
            ' Waiting until debugger is attached
            RequestAdditionalTime(1000)     ' Prevents the service from timeout
            Thread.Sleep(1000)              ' Gives you time to attach the debugger
        End While
        RequestAdditionalTime(20000)

#End If
        ConfigureTls()

        Dim settings As New SettingsClass(System.IO.Path.Combine(GetApplicationPath(), CONFIG_FILE))

        '' Set member mAccount for both integrations here
        mAccount = settings.ReadValue("Main", "AccountNumber", "")

        '' Set member url for both integrations (where to upload files to)
        mUploadUrl = settings.ReadValue("Main", "UploadUrl", "")

        If String.IsNullOrEmpty(mUploadUrl) Then

            Logger.WriteLogEntry("Unable to start service, URL is empty", objEventLog)
            Me.Stop()

        End If

        '' Setup our cloud logger
        mCloudLog = New CloudLogClass(mAccount, objEventLog)

        If settings.ReadValue(SQL_SETTINGS, "ServerName", String.Empty) <> String.Empty AndAlso
            settings.ReadValue(SQL_SETTINGS, "DBName", String.Empty) <> String.Empty AndAlso
            settings.ReadValue(SQL_SETTINGS, "CADQueries", String.Empty) <> String.Empty Then

            HandleSqlBasedIntegration()

        Else

            HandleFileBasedIntegration()

        End If

    End Sub

    Private Sub ConfigureTls()
        ServicePointManager.Expect100Continue = True
        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12
        ServicePointManager.ServerCertificateValidationCallback = New RemoteCertificateValidationCallback(AddressOf CertificateValidationCallBack)
    End Sub

    Function CertificateValidationCallBack(ByVal sender As Object, ByVal certificate As X509Certificate, ByVal chain As X509Chain, ByVal sslPolicyErrors As Security.SslPolicyErrors) As Boolean
        Return True
    End Function

    Protected Overrides Sub OnStop()
        ' Add code here to perform any tear-down necessary to stop your service.

        Try

            Logger.WriteLogEntry("Service Stopped", objEventLog)
            mCloudLog.WriteToCloud("Service Stopped")

            '' stop timer if need be
            If Not mTimerSql Is Nothing Then
                If mTimerSql.Enabled = True Then
                    mTimerSql.Stop()
                End If
            End If

        Catch ex As Exception
            ' do nothing

        End Try

        Try

            RemoveHandler mFileSystemWatcher.Deleted, New FileSystemEventHandler(AddressOf File_Deleted)
            RemoveHandler mFileSystemWatcher.Changed, New FileSystemEventHandler(AddressOf File_Modified)
            RemoveHandler mTimerSql.Elapsed, New System.Timers.ElapsedEventHandler(AddressOf ImportCADSQL)

        Catch ex As Exception
            ' Ignore

        End Try

    End Sub

#End Region

#Region "  Private Methods  "

    Private Sub HandleFileBasedIntegration()

        WriteLogEntry("File Based Integration Started", objEventLog)
        mCloudLog.WriteToCloud("File Based Integration Started")

        mFileSystemWatcher = New FileSystemWatcher()

        Dim settings As New SettingsClass(System.IO.Path.Combine(GetApplicationPath(), "iPCRCADModule.config"))

        Dim folder As String = settings.ReadValue("Main", "ExportDirectory", "")

        If folder.Trim.Length = 0 Or String.Compare(folder, "NOT USED FOR THIS INTEGRATION", True) = 0 Or mAccount.Trim.Length = 0 Then

            WriteLogEntry("File Based Integration stopped.", objEventLog)
            mCloudLog.WriteToCloud("File based integration stopped, Empty Directory/Account or looks like this should be SQL.")
            Me.Stop()

        End If

        If folder.Length > 0 Then

            mFileSystemWatcher.Path = folder
            mFileSystemWatcher.IncludeSubdirectories = False

            ' B55325 changed event not fired when Firehouse creates export file - jtengan 011717
            Dim fileSystemWatcherEvent As String = settings.ReadValue("Main", "FileSystemWatcherEvent", "Changed")

            AddHandler mFileSystemWatcher.Deleted, New FileSystemEventHandler(AddressOf File_Deleted)

            If fileSystemWatcherEvent.Trim.ToUpper = "CREATED" Then
                AddHandler mFileSystemWatcher.Created, New FileSystemEventHandler(AddressOf File_Modified)
            Else
                AddHandler mFileSystemWatcher.Changed, New FileSystemEventHandler(AddressOf File_Modified)
            End If
            ' test
            mFileSystemWatcher.EnableRaisingEvents = True

        End If

    End Sub

    Private Sub HandleSqlBasedIntegration()

        WriteLogEntry("SQL Based Integration Started", objEventLog)
        mCloudLog.WriteToCloud("SQL Based Integration Started")

        If CanConnectToSQL() Then

            Dim settings As New SettingsClass(System.IO.Path.Combine(GetApplicationPath(), CONFIG_FILE))

            '' Instantiate the timer using the interval specified by the user * 1000 to get the seconds value
            mTimerSql = New System.Timers.Timer(CDbl(settings.ReadValue(SQL_SETTINGS, "Interval", "30")) * 1000)

            '' Set the cad queries (member vars)
            ParseCompositeQuery()

            '' hook up the event
            AddHandler mTimerSql.Elapsed, AddressOf ImportCADSQL

            '' Populate the member id place holder, will be integer.minvalue if there are any errors
            SetCurrentMaxRunID()

            '' Check that we have a valid value for the last run
            If mLastRunIdImported = Integer.MinValue Then

                WriteLogEntry("There was a problem obtaining the last RunId to import.", objEventLog)
                mCloudLog.WriteToCloud("There was a problem obtaining the last RunId to import.")
                Me.Stop()

            End If

            '' Start the timer that checks the Callsheet for new calls at N interval (user selected between 30 and 120 second interval)
            mTimerSql.Start()

        Else

            WriteLogEntry("Unable to connect to the SQL Server Database." & Environment.NewLine & _
                          "Please check your SQL Server Settings and try again.", objEventLog)
            mCloudLog.WriteToCloud("Unable to connect to SQL Server Database, service stopping.")

            '' Stop the service
            Me.Stop()

        End If

    End Sub

    Private Sub ProcessFile(file As String)

        Try

            LogTheFile(file)

            Me.StoreLastFileBytes(file)

            Dim textFile As New TextExportFileClass
            textFile.File = file
            textFile.UploadUrl = mUploadUrl
            textFile.eventLog = objEventLog
            textFile.AccountNumber = Me.mAccount
            textFile.CloudLog = mCloudLog

            Dim t As New Thread(AddressOf textFile.UploadFile)
            t.IsBackground = True
            t.Start()

        Catch ex As Exception

            Try

                Logger.WriteLogEntry(String.Format("Error Encountered: {0}", ex.ToString), objEventLog)
                mCloudLog.WriteToCloud("Error in CAD Service ProcessFile: " & ex.ToString)

            Catch subEx As Exception

            End Try

        End Try

    End Sub

    Private Sub LogTheFile(fileName As String)

        Dim sbLog As Text.StringBuilder = New Text.StringBuilder

        Try

            sbLog.AppendFormat("Uploading File: {0}", fileName)
            sbLog.AppendLine().AppendLine()
            sbLog.AppendLine("File Contents Begin")
            sbLog.AppendLine(My.Computer.FileSystem.ReadAllText(fileName))
            sbLog.AppendLine("File Contents End")

            Logger.WriteLogEntry(sbLog.ToString(), objEventLog)

        Catch ex As Exception

            Dim strExMessage As String = "Unable to log the file contents, exception occurred: " & ex.Message
            Logger.WriteLogEntry(strExMessage, objEventLog)
            mCloudLog.WriteToCloud(strExMessage)

        End Try

    End Sub


    Private Function GetApplicationPath() As String

        Return Path.GetDirectoryName([Assembly].GetEntryAssembly().Location)

    End Function

    Private Function FileBytesSame(filePathOne As String) As Boolean

        Dim fileOneByte As Integer
        Dim fileTwoByte As Integer

        Dim fileOneStream As FileStream

        ' Open a FileStream for each file.
        Try

            fileOneStream = New FileStream(filePathOne, FileMode.Open)

        Catch ex As Exception
            'EventLog1.WriteEntry(ex.ToString)
            Logger.WriteLogEntry(String.Format("Error Encountered comparing files: {0}", ex.ToString), objEventLog)

            Return False

        End Try

        ' If the files are not the same length...
        If (fileOneStream.Length <> Me.mLastFileBytes.Length) Then
            fileOneStream.Close()
            ' File's are not equal.
            Return False
        End If

        Dim areFilesEqual As Boolean = True

        ' Loop through bytes in the files until
        '  a byte in file one <> a byte in file two
        ' OR
        '  end of the file one is reached.
        Dim index As Integer = 0

        Dim s As MemoryStream = New MemoryStream(Me.mLastFileBytes)

        Do
            ' Read one byte from each file.
            fileOneByte = fileOneStream.ReadByte()
            fileTwoByte = s.ReadByte

            If fileOneByte <> fileTwoByte Then
                ' Files are not equal; byte in file one <> byte in file two.
                areFilesEqual = False
                Exit Do
            End If

            index += 1

        Loop While (fileOneByte <> -1)

        ' Close the FileStreams.
        fileOneStream.Close()
        s.Close()

        Return areFilesEqual

    End Function

    Private Function ShouldProcessFile(file As String) As Boolean

        If Me.mLastFileBytes Is Nothing Then Return True

        If Me.mLastFileBytes.Length = 0 Then Return True

        If Not Me.FileBytesSame(file) Then Return True

        Return False

    End Function

    Private Sub StoreLastFileBytes(file As String)

        Dim position As Integer = 0
        Dim bufferSize As Integer = 4096

        Using fsOpen As FileStream = New FileStream(file, FileMode.Open, FileAccess.Read)

            ReDim Me.mLastFileBytes(CInt(fsOpen.Length - 1))
            fsOpen.Read(Me.mLastFileBytes, 0, CInt(fsOpen.Length))
            fsOpen.Close()

        End Using

    End Sub

    ''' <summary>
    ''' Reads in the values stored by the SQLCredentialsForm and attempts to make a connection to the database.
    ''' </summary>
    ''' <returns>False if the open() sub throws an exception (unable to connect).</returns>
    Private Function CanConnectToSQL() As Boolean
        Dim result As Boolean = False

        Try

            Dim sqlConnection As New SqlConnection(ReturnSQLConnString())

            sqlConnection.Open() '' Will throw an exception if it can't connect and return false below
            sqlConnection.Close()

            Return True

        Catch ex As Exception
            WriteLogEntry("Unable To connect to sql server.", objEventLog)
            Return False

        End Try

    End Function

    Private Function ReturnSQLConnString() As String
        Dim connString As String = String.Empty
        Dim settings As New SettingsClass(System.IO.Path.Combine(GetApplicationPath(), CONFIG_FILE))
        Dim server As String = String.Empty
        Dim dbName As String = String.Empty
        Dim userName As String = String.Empty
        Dim password As String = String.Empty
        Dim useWindowsAuth As Boolean = False

        With settings

            server = .ReadValue(SQL_SETTINGS, "ServerName", String.Empty)
            dbName = .ReadValue(SQL_SETTINGS, "DBName", String.Empty)
            userName = .ReadValue(SQL_SETTINGS, "UserName", String.Empty)
            password = .ReadValue(SQL_SETTINGS, "Password", String.Empty)
            useWindowsAuth = CBool(.ReadValue(SQL_SETTINGS, "WindowsAuth", "False"))

        End With

        If useWindowsAuth = True Then
            '' Use trusted conx
            connString = String.Format("Server={0};Database={1};Trusted_Connection=True;", server, dbName)

        Else
            '' populate username/password
            connString = String.Format("Server={0};Database={1};User Id={2};Password={3};", server, dbName, userName, password)

        End If

        Return connString

    End Function

    Private Function ImportCADRuns() As Integer

        Dim dAdapter As SqlDataAdapter

        Dim strTempFileName As String = String.Empty
        Dim runIDImported As Integer = mLastRunIdImported

        Try

            '' Instantiate connection from settings values that were tested by CanConnectToSQL()
            Using conn As SqlConnection = New SqlConnection(ReturnSQLConnString())

                Using DT As DataTable = New DataTable("CadRecord")

                    dAdapter = New SqlDataAdapter With {.SelectCommand = New SqlCommand(mCadRecordQuery & mLastRunIdImported.ToString, conn)}
                    dAdapter.Fill(DT)

                    If DT.Rows.Count > 0 Then

                        '' Grab the row and store the Run ID
                        If DT.Columns.Contains("CurrentRecordNo") Then
                            runIDImported = Convert.ToInt32(DT.Rows(0)("CurrentRecordNo")) '' Advised cad queries should have CurrentRecordNo alias to run id

                        ElseIf DT.Columns.Contains("Run") Then

                            runIDImported = Convert.ToInt32(DT.Rows(0)("Run"))

                        Else
                            ' Looking for CurrentRecordNo or Run
                            Dim strColumns As String = String.Empty

                            For Each column As DataColumn In DT.Columns
                                strColumns &= column.ColumnName & vbCrLf
                            Next

                            Throw New ArgumentException(String.Format("Service could not find a valid CurrentRecordNo/Run id to import, found columns: {0}", strColumns))

                        End If

                        runIDImported = Convert.ToInt32(DT.Rows(0)("CurrentRecordNo")) '' Cad query will always have a currentRecordNo aliased column

                        '' Create a unique filename to store the xml (tempPath \ GUID.xml)
                        strTempFileName = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") & ".xml")

                        '' Write out the dataTable to this file as xml
                        DT.WriteXml(strTempFileName)

                        '' Process, Upload, and Delete this file
                        ProcessAndUploadfile(strTempFileName)
                        Return runIDImported

                    Else
                        ' Nothing to send up - runIdImported remains the same
                        Return runIDImported

                    End If

                End Using

            End Using

        Catch ex As Exception
            WriteLogEntry("Error importing CAD Run: " & ex.Message, objEventLog)
            mCloudLog.WriteToCloud("Error importing CAD Run: " & ex.Message)

        Finally
            If Not IsNothing(dAdapter) Then dAdapter.Dispose()

        End Try

        Return runIDImported

    End Function

    ''' <summary>
    ''' Checks to see if it should process this file by comparing the byte values, this ensures the same record is not sent up twice.
    ''' If the file should be processed, it's bytes are stored for later comparison, the file is uploaded to the web, and finally deleted.
    ''' If the file should not be processed, it is deleted.
    ''' </summary>
    ''' <param name="fileForUpload">Fully qualified filename for comparison, upload, and deletion.</param>
    Private Sub ProcessAndUploadfile(fileForUpload As String)

        If ShouldProcessFile(fileForUpload) Then

            ProcessFile(fileForUpload)

        Else

            Try
                My.Computer.FileSystem.DeleteFile(fileForUpload)
            Catch ex As Exception
                '' Nothing
            End Try

        End If

    End Sub

    Private Sub SetCurrentMaxRunID()
        Dim currentMaxID As Integer = Integer.MinValue
        Dim sql As String = mMaxRunQuery

        Using conn As New SqlConnection(ReturnSQLConnString())
            Dim cmd As New SqlCommand(sql, conn)

            Try

                conn.Open()
                currentMaxID = Convert.ToInt32(cmd.ExecuteScalar())
                conn.Close()

            Catch ex As Exception
                currentMaxID = Integer.MinValue

            Finally
                mLastRunIdImported = currentMaxID

            End Try

        End Using

    End Sub

    Private Sub ParseCompositeQuery()

        Dim settings As New SettingsClass(System.IO.Path.Combine(GetApplicationPath(), CONFIG_FILE))
        Dim compositeQueries As String = settings.ReadValue(SQL_SETTINGS, "CADQueries", String.Empty)

        If String.IsNullOrEmpty(compositeQueries) Then

            WriteLogEntry("The composite query is null or empty.", objEventLog)
            mCloudLog.WriteToCloud("The composite query is null or empty.")
            Exit Sub

        End If


        Try
            Dim queryArr As String() = compositeQueries.Split("^"c)

            '' Set the members to the appropriate query string based on the presence of SELECT MAX.  The trim and concatenation 
            If queryArr(0).IndexOf("SELECT MAX") = -1 Then
                mMaxRunQuery = queryArr(1).Trim & " "
                mCadRecordQuery = queryArr(0).Trim & " "
            Else
                mMaxRunQuery = queryArr(0).Trim & " "
                mCadRecordQuery = queryArr(1).Trim & " "
            End If

            WriteLogEntry(String.Format("MaxRunQuery: {0}{1}CadRecordQuery:{2}", mMaxRunQuery, Environment.NewLine, mCadRecordQuery), objEventLog)

        Catch ex As Exception
            WriteLogEntry("There was an error setting the composite queries.", objEventLog)
            mCloudLog.WriteToCloud("Error parsing SQL Query: " & ex.Message)

        End Try

    End Sub

#End Region

#Region "  Event Handlers  "

    Private Sub File_Deleted(ByVal sender As Object, ByVal e As EventArgs)
        ' No implementation
    End Sub

    Private Sub File_Modified(ByVal obj As Object, ByVal e As FileSystemEventArgs)

        Dim file As String = e.FullPath.Trim
        Dim tempFile As String = System.IO.Path.Combine(System.IO.Path.GetTempPath, Guid.NewGuid.ToString & ".txt")

        ' copy file to temp location to process without locking original file
        Dim success As Boolean = False

        ' try 4 times
        For i = 1 To 4

            Try
                System.IO.File.Copy(file, tempFile, True)
                success = True
                Exit For    ' if reached here, success

            Catch ex As Exception

                Try

                    If i = 4 Then

                        Logger.WriteLogEntry(String.Format("Attempted copy of file '{0}' failed. Aborting. Error: {1}", file, ex.ToString), objEventLog)
                        mCloudLog.WriteToCloud(String.Format("Attempted copy of file '{0}' failed. Aborting. Error: {1}", file, ex.ToString))

                    End If

                    Thread.Sleep(1000)

                Catch subEx As Exception
                    ' do nothing?

                End Try

            End Try

        Next

        If success Then

            ' only process if file is not the same contents
            If ShouldProcessFile(tempFile) Then

                ProcessFile(tempFile)

            Else

                'EventLog1.WriteEntry(String.Format("Skipping file '{0}': Duplicate", file))

                Try
                    System.IO.File.Delete(tempFile)
                Catch ex As Exception
                    ' do nothing
                End Try

            End If

        End If

    End Sub

    Private Sub ImportCADSQL(ByVal sender As Object, ByVal e As EventArgs)

        '' Exit if the import is already running
        If mIsSqlImportRunning = True Then Exit Sub

        Try
            mIsSqlImportRunning = True
            mLastRunIdImported = ImportCADRuns()

        Catch ex As Exception
            '' Nothing right now

        Finally
            mIsSqlImportRunning = False

        End Try

    End Sub

#End Region

End Class
