<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class SQLCredentialsForm
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.grpSQLConfig = New System.Windows.Forms.GroupBox()
        Me.txtDBName = New System.Windows.Forms.TextBox()
        Me.Label5 = New System.Windows.Forms.Label()
        Me.Label4 = New System.Windows.Forms.Label()
        Me.nudInterval = New System.Windows.Forms.NumericUpDown()
        Me.txtPassword = New System.Windows.Forms.TextBox()
        Me.txtUserName = New System.Windows.Forms.TextBox()
        Me.txtServerName = New System.Windows.Forms.TextBox()
        Me.chkWindowsAuthentication = New System.Windows.Forms.CheckBox()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.btnSave = New System.Windows.Forms.Button()
        Me.btnCancel = New System.Windows.Forms.Button()
        Me.btnTestConnection = New System.Windows.Forms.Button()
        Me.grpSQLConfig.SuspendLayout()
        CType(Me.nudInterval, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'grpSQLConfig
        '
        Me.grpSQLConfig.Controls.Add(Me.txtDBName)
        Me.grpSQLConfig.Controls.Add(Me.Label5)
        Me.grpSQLConfig.Controls.Add(Me.Label4)
        Me.grpSQLConfig.Controls.Add(Me.nudInterval)
        Me.grpSQLConfig.Controls.Add(Me.txtPassword)
        Me.grpSQLConfig.Controls.Add(Me.txtUserName)
        Me.grpSQLConfig.Controls.Add(Me.txtServerName)
        Me.grpSQLConfig.Controls.Add(Me.chkWindowsAuthentication)
        Me.grpSQLConfig.Controls.Add(Me.Label3)
        Me.grpSQLConfig.Controls.Add(Me.Label2)
        Me.grpSQLConfig.Controls.Add(Me.Label1)
        Me.grpSQLConfig.Location = New System.Drawing.Point(13, 13)
        Me.grpSQLConfig.Name = "grpSQLConfig"
        Me.grpSQLConfig.Size = New System.Drawing.Size(303, 157)
        Me.grpSQLConfig.TabIndex = 0
        Me.grpSQLConfig.TabStop = False
        Me.grpSQLConfig.Text = "SQL Server"
        '
        'txtDBName
        '
        Me.txtDBName.Location = New System.Drawing.Point(84, 41)
        Me.txtDBName.MaxLength = 150
        Me.txtDBName.Name = "txtDBName"
        Me.txtDBName.Size = New System.Drawing.Size(202, 20)
        Me.txtDBName.TabIndex = 3
        '
        'Label5
        '
        Me.Label5.AutoSize = True
        Me.Label5.Location = New System.Drawing.Point(22, 45)
        Me.Label5.Name = "Label5"
        Me.Label5.Size = New System.Drawing.Size(56, 13)
        Me.Label5.TabIndex = 2
        Me.Label5.Text = "DB Name:"
        '
        'Label4
        '
        Me.Label4.AutoSize = True
        Me.Label4.Location = New System.Drawing.Point(33, 120)
        Me.Label4.Name = "Label4"
        Me.Label4.Size = New System.Drawing.Size(45, 13)
        Me.Label4.TabIndex = 8
        Me.Label4.Text = "Interval:"
        '
        'nudInterval
        '
        Me.nudInterval.Location = New System.Drawing.Point(84, 116)
        Me.nudInterval.Maximum = New Decimal(New Integer() {120, 0, 0, 0})
        Me.nudInterval.Minimum = New Decimal(New Integer() {30, 0, 0, 0})
        Me.nudInterval.Name = "nudInterval"
        Me.nudInterval.Size = New System.Drawing.Size(39, 20)
        Me.nudInterval.TabIndex = 9
        Me.nudInterval.Value = New Decimal(New Integer() {30, 0, 0, 0})
        '
        'txtPassword
        '
        Me.txtPassword.Location = New System.Drawing.Point(84, 91)
        Me.txtPassword.MaxLength = 150
        Me.txtPassword.Name = "txtPassword"
        Me.txtPassword.PasswordChar = Global.Microsoft.VisualBasic.ChrW(42)
        Me.txtPassword.Size = New System.Drawing.Size(202, 20)
        Me.txtPassword.TabIndex = 7
        Me.txtPassword.UseSystemPasswordChar = True
        '
        'txtUserName
        '
        Me.txtUserName.Location = New System.Drawing.Point(84, 66)
        Me.txtUserName.MaxLength = 150
        Me.txtUserName.Name = "txtUserName"
        Me.txtUserName.Size = New System.Drawing.Size(202, 20)
        Me.txtUserName.TabIndex = 5
        '
        'txtServerName
        '
        Me.txtServerName.Location = New System.Drawing.Point(84, 16)
        Me.txtServerName.MaxLength = 150
        Me.txtServerName.Name = "txtServerName"
        Me.txtServerName.Size = New System.Drawing.Size(202, 20)
        Me.txtServerName.TabIndex = 1
        '
        'chkWindowsAuthentication
        '
        Me.chkWindowsAuthentication.AutoSize = True
        Me.chkWindowsAuthentication.Location = New System.Drawing.Point(129, 119)
        Me.chkWindowsAuthentication.Name = "chkWindowsAuthentication"
        Me.chkWindowsAuthentication.Size = New System.Drawing.Size(163, 17)
        Me.chkWindowsAuthentication.TabIndex = 10
        Me.chkWindowsAuthentication.Text = "Use Windows Authentication"
        Me.chkWindowsAuthentication.UseVisualStyleBackColor = True
        '
        'Label3
        '
        Me.Label3.AutoSize = True
        Me.Label3.Location = New System.Drawing.Point(22, 95)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(56, 13)
        Me.Label3.TabIndex = 6
        Me.Label3.Text = "Password:"
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Location = New System.Drawing.Point(15, 70)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(63, 13)
        Me.Label2.TabIndex = 4
        Me.Label2.Text = "User Name:"
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(6, 20)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(72, 13)
        Me.Label1.TabIndex = 0
        Me.Label1.Text = "Server Name:"
        '
        'btnSave
        '
        Me.btnSave.Location = New System.Drawing.Point(143, 176)
        Me.btnSave.Name = "btnSave"
        Me.btnSave.Size = New System.Drawing.Size(75, 23)
        Me.btnSave.TabIndex = 1
        Me.btnSave.Text = "&Save"
        Me.btnSave.UseVisualStyleBackColor = True
        '
        'btnCancel
        '
        Me.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.btnCancel.Location = New System.Drawing.Point(224, 176)
        Me.btnCancel.Name = "btnCancel"
        Me.btnCancel.Size = New System.Drawing.Size(75, 23)
        Me.btnCancel.TabIndex = 2
        Me.btnCancel.Text = "&Cancel"
        Me.btnCancel.UseVisualStyleBackColor = True
        '
        'btnTestConnection
        '
        Me.btnTestConnection.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.btnTestConnection.Location = New System.Drawing.Point(22, 176)
        Me.btnTestConnection.Name = "btnTestConnection"
        Me.btnTestConnection.Size = New System.Drawing.Size(52, 23)
        Me.btnTestConnection.TabIndex = 3
        Me.btnTestConnection.Text = "Test"
        Me.btnTestConnection.UseVisualStyleBackColor = True
        '
        'SQLCredentialsForm
        '
        Me.AcceptButton = Me.btnSave
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.CancelButton = Me.btnCancel
        Me.ClientSize = New System.Drawing.Size(330, 214)
        Me.Controls.Add(Me.btnTestConnection)
        Me.Controls.Add(Me.btnCancel)
        Me.Controls.Add(Me.btnSave)
        Me.Controls.Add(Me.grpSQLConfig)
        Me.Name = "SQLCredentialsForm"
        Me.ShowIcon = False
        Me.ShowInTaskbar = False
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "Configure SQL Server Connection"
        Me.TopMost = True
        Me.grpSQLConfig.ResumeLayout(False)
        Me.grpSQLConfig.PerformLayout()
        CType(Me.nudInterval, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents grpSQLConfig As System.Windows.Forms.GroupBox
    Friend WithEvents txtPassword As System.Windows.Forms.TextBox
    Friend WithEvents txtUserName As System.Windows.Forms.TextBox
    Friend WithEvents txtServerName As System.Windows.Forms.TextBox
    Friend WithEvents chkWindowsAuthentication As System.Windows.Forms.CheckBox
    Friend WithEvents Label3 As System.Windows.Forms.Label
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents btnSave As System.Windows.Forms.Button
    Friend WithEvents btnCancel As System.Windows.Forms.Button
    Friend WithEvents Label4 As System.Windows.Forms.Label
    Friend WithEvents nudInterval As System.Windows.Forms.NumericUpDown
    Friend WithEvents txtDBName As System.Windows.Forms.TextBox
    Friend WithEvents Label5 As System.Windows.Forms.Label
    Friend WithEvents btnTestConnection As System.Windows.Forms.Button
End Class
