Option Explicit On
Option Strict On

Public Class SQLCredentialsForm

#Region "  Private Declarations  "

    Private mIsChanged As Boolean = False

    Private Const SQL_SETTINGS As String = "SQL"
    Private Const CONFIG_FILE As String = "iPCRCADModule.config"

#End Region

#Region "  Private Methods  "

    Private Function SaveSettings() As Boolean

        If ValidSettings() = True Then

            Dim settings As New SettingsClass(System.IO.Path.Combine(Application.StartupPath, CONFIG_FILE))

            With settings

                '' First call to write IsSQLBased creates the section
                .WriteValue(SQL_SETTINGS, "IsSqlBased", "True")
                .WriteValue(SQL_SETTINGS, "ServerName", Me.txtServerName.Text.Trim)
                .WriteValue(SQL_SETTINGS, "UserName", Me.txtUserName.Text.Trim)
                .WriteValue(SQL_SETTINGS, "DBName", Me.txtDBName.Text.Trim)
                .WriteValue(SQL_SETTINGS, "Password", Me.txtPassword.Text) ' Do not trim password
                .WriteValue(SQL_SETTINGS, "WindowsAuth", Me.chkWindowsAuthentication.Checked.ToString)
                .WriteValue(SQL_SETTINGS, "Interval", Me.nudInterval.Value.ToString)

            End With

            mIsChanged = False
            Return True
        Else

            Return False

        End If

    End Function

    Private Sub LoadSettings()

        Dim settings As New SettingsClass(System.IO.Path.Combine(Application.StartupPath, CONFIG_FILE))

        With settings

            Me.txtServerName.Text = .ReadValue(SQL_SETTINGS, "ServerName", String.Empty)
            Me.txtUserName.Text = .ReadValue(SQL_SETTINGS, "UserName", String.Empty)
            Me.txtPassword.Text = .ReadValue(SQL_SETTINGS, "Password", String.Empty)
            Me.txtDBName.Text = .ReadValue(SQL_SETTINGS, "DBName", String.Empty)
            Me.chkWindowsAuthentication.Checked = CBool(.ReadValue(SQL_SETTINGS, "WindowsAuth", "False"))
            Me.nudInterval.Value = CDec(.ReadValue(SQL_SETTINGS, "Interval", "30"))

        End With

        Me.mIsChanged = False

    End Sub

    Private Function ValidSettings() As Boolean

        Dim result As Boolean = True
        Dim message As String = String.Empty

        Try

            If txtServerName.Text.Trim.Length = 0 Then
                message &= "Server Name cannot be empty." & Environment.NewLine
            End If

            If txtDBName.Text.Trim.Length = 0 Then
                message &= "DB Name cannot be empty." & Environment.NewLine
            End If

            If Not Me.chkWindowsAuthentication.Checked And txtUserName.Text.Trim.Length = 0 Then
                message &= "User Name cannot be empty." & Environment.NewLine
            End If

            If Not Me.chkWindowsAuthentication.Checked And txtPassword.Text.Trim.Length = 0 Then
                message &= "Password cannot be empty." & Environment.NewLine
            End If

            If message <> String.Empty Then

                message = "Please address the following item(s):" & Environment.NewLine & message
                MessageBox.Show(message, "Invalid Settings", MessageBoxButtons.OK, MessageBoxIcon.Information)

                result = False

            End If

            Return result

        Catch ex As Exception
            '' didn't pass
            Return False

        End Try

    End Function

    Private Function CanConnectToSql() As Boolean
        Dim result As Boolean = False

        Dim connString As String = String.Empty
        Dim server As String = String.Empty
        Dim dbName As String = String.Empty
        Dim userName As String = String.Empty
        Dim password As String = String.Empty
        Dim useWindowsAuth As Boolean = False

        server = Me.txtServerName.Text.Trim
        dbName = Me.txtDBName.Text.Trim
        userName = Me.txtUserName.Text.Trim
        password = Me.txtPassword.Text
        useWindowsAuth = Me.chkWindowsAuthentication.Checked

        If useWindowsAuth = True Then
            '' Use trusted conx
            connString = String.Format("Server={0};Database={1};Trusted_Connection=True;", server, dbName)

        Else
            '' populate username/password
            connString = String.Format("Server={0};Database={1};User Id={2};Password={3};", server, dbName, userName, password)

        End If

        Try
            Dim sqlConnection As New System.Data.SqlClient.SqlConnection(connString)

            sqlConnection.Open() '' Will throw an exception if it can't connect and return false below
            sqlConnection.Close()

            Return True

        Catch ex As Exception
            Return False

        End Try

    End Function

#End Region

#Region "  Event Handlers  "

    Private Sub btnCancel_Click(sender As System.Object, e As System.EventArgs) Handles btnCancel.Click

        If mIsChanged Then

            Select Case MessageBox.Show("Changes were detected to your SQL configuration. Do you want to save them now?", Me.Text, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button3)

                Case Windows.Forms.DialogResult.Yes

                    '' Save settings and close
                    If SaveSettings() Then

                        MessageBox.Show("Settings Saved.", "SQL Configuration", MessageBoxButtons.OK, MessageBoxIcon.Information)
                        Me.Close()

                    End If

                Case Windows.Forms.DialogResult.No
                    Me.Close()

                Case Windows.Forms.DialogResult.Cancel
                    '' Cancelled, do nothing
                    Exit Sub

            End Select

            ' no changes detected, just close
        Else
            Me.Close()

        End If

    End Sub

    Private Sub txtServerName_TextChanged(sender As System.Object, e As System.EventArgs) Handles txtUserName.TextChanged,
                                                                                                    txtServerName.TextChanged,
                                                                                                    txtPassword.TextChanged,
                                                                                                    txtDBName.TextChanged,
                                                                                                    chkWindowsAuthentication.CheckStateChanged
        mIsChanged = True

    End Sub

    Private Sub btnSave_Click(sender As System.Object, e As System.EventArgs) Handles btnSave.Click

        If SaveSettings() Then

            Me.DialogResult = Windows.Forms.DialogResult.OK
            Me.Close()

        End If

    End Sub

    Private Sub SQLCredentialsForm_Load(sender As Object, e As System.EventArgs) Handles Me.Load

        LoadSettings()

    End Sub

    Private Sub chkWindowsAuthentication_CheckedChanged(sender As System.Object, e As System.EventArgs) Handles chkWindowsAuthentication.CheckedChanged

        If chkWindowsAuthentication.Checked Then
            Me.txtUserName.Enabled = False
            Me.txtPassword.Enabled = False
        Else
            Me.txtUserName.Enabled = True
            Me.txtPassword.Enabled = True
        End If

    End Sub

    Private Sub btnTestConnection_Click(sender As System.Object, e As System.EventArgs) Handles btnTestConnection.Click
        If CanConnectToSql() Then

            SaveSettings()

            MessageBox.Show("Successfully Connected." & Environment.NewLine & _
                            "Settings Saved.", "Success!", MessageBoxButtons.OK, MessageBoxIcon.Information)
        Else
            MessageBox.Show("Unable to connect." & Environment.NewLine & _
                            "Please check your settings and try again.", "Unable to connect", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
        End If

    End Sub

#End Region

End Class