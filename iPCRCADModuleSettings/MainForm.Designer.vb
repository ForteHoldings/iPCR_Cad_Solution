<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class MainForm
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(MainForm))
        Me.Label1 = New System.Windows.Forms.Label()
        Me.cboCADProgram = New System.Windows.Forms.ComboBox()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.txtExportDirectory = New System.Windows.Forms.TextBox()
        Me.btnBrowseExportDirectory = New System.Windows.Forms.Button()
        Me.btnRestartService = New System.Windows.Forms.Button()
        Me.btnSave = New System.Windows.Forms.Button()
        Me.btnClose = New System.Windows.Forms.Button()
        Me.txtProductCode = New System.Windows.Forms.TextBox()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.FolderBrowserDialog1 = New System.Windows.Forms.FolderBrowserDialog()
        Me.btnConfigureSQL = New System.Windows.Forms.Button()
        Me.chkOnCreated = New System.Windows.Forms.CheckBox()
        Me.SuspendLayout()
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(12, 67)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(74, 13)
        Me.Label1.TabIndex = 5
        Me.Label1.Text = "CAD Program:"
        Me.Label1.Visible = False
        '
        'cboCADProgram
        '
        Me.cboCADProgram.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cboCADProgram.FormattingEnabled = True
        Me.cboCADProgram.Items.AddRange(New Object() {"RightCAD"})
        Me.cboCADProgram.Location = New System.Drawing.Point(140, 64)
        Me.cboCADProgram.Name = "cboCADProgram"
        Me.cboCADProgram.Size = New System.Drawing.Size(244, 21)
        Me.cboCADProgram.TabIndex = 6
        Me.cboCADProgram.Visible = False
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Location = New System.Drawing.Point(12, 41)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(85, 13)
        Me.Label2.TabIndex = 2
        Me.Label2.Text = "Export Directory:"
        '
        'txtExportDirectory
        '
        Me.txtExportDirectory.Location = New System.Drawing.Point(140, 38)
        Me.txtExportDirectory.Name = "txtExportDirectory"
        Me.txtExportDirectory.Size = New System.Drawing.Size(218, 20)
        Me.txtExportDirectory.TabIndex = 3
        '
        'btnBrowseExportDirectory
        '
        Me.btnBrowseExportDirectory.Location = New System.Drawing.Point(360, 38)
        Me.btnBrowseExportDirectory.Name = "btnBrowseExportDirectory"
        Me.btnBrowseExportDirectory.Size = New System.Drawing.Size(24, 20)
        Me.btnBrowseExportDirectory.TabIndex = 4
        Me.btnBrowseExportDirectory.Text = "..."
        Me.btnBrowseExportDirectory.UseVisualStyleBackColor = True
        '
        'btnRestartService
        '
        Me.btnRestartService.Image = CType(resources.GetObject("btnRestartService.Image"), System.Drawing.Image)
        Me.btnRestartService.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.btnRestartService.Location = New System.Drawing.Point(12, 122)
        Me.btnRestartService.Name = "btnRestartService"
        Me.btnRestartService.Size = New System.Drawing.Size(156, 23)
        Me.btnRestartService.TabIndex = 9
        Me.btnRestartService.Text = "Start/Restart Service"
        Me.btnRestartService.UseVisualStyleBackColor = True
        '
        'btnSave
        '
        Me.btnSave.Location = New System.Drawing.Point(228, 122)
        Me.btnSave.Name = "btnSave"
        Me.btnSave.Size = New System.Drawing.Size(75, 23)
        Me.btnSave.TabIndex = 10
        Me.btnSave.Text = "&Save"
        Me.btnSave.UseVisualStyleBackColor = True
        '
        'btnClose
        '
        Me.btnClose.Location = New System.Drawing.Point(309, 122)
        Me.btnClose.Name = "btnClose"
        Me.btnClose.Size = New System.Drawing.Size(75, 23)
        Me.btnClose.TabIndex = 11
        Me.btnClose.Text = "&Close"
        Me.btnClose.UseVisualStyleBackColor = True
        '
        'txtProductCode
        '
        Me.txtProductCode.Location = New System.Drawing.Point(140, 12)
        Me.txtProductCode.Name = "txtProductCode"
        Me.txtProductCode.Size = New System.Drawing.Size(244, 20)
        Me.txtProductCode.TabIndex = 1
        '
        'Label3
        '
        Me.Label3.AutoSize = True
        Me.Label3.Location = New System.Drawing.Point(12, 15)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(75, 13)
        Me.Label3.TabIndex = 0
        Me.Label3.Text = "Product Code:"
        '
        'btnConfigureSQL
        '
        Me.btnConfigureSQL.Image = CType(resources.GetObject("btnConfigureSQL.Image"), System.Drawing.Image)
        Me.btnConfigureSQL.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.btnConfigureSQL.Location = New System.Drawing.Point(12, 93)
        Me.btnConfigureSQL.Name = "btnConfigureSQL"
        Me.btnConfigureSQL.Size = New System.Drawing.Size(156, 23)
        Me.btnConfigureSQL.TabIndex = 8
        Me.btnConfigureSQL.Text = "Configure SQL Conn."
        Me.btnConfigureSQL.UseVisualStyleBackColor = True
        '
        'chkOnCreated
        '
        Me.chkOnCreated.AutoSize = True
        Me.chkOnCreated.Location = New System.Drawing.Point(215, 91)
        Me.chkOnCreated.Name = "chkOnCreated"
        Me.chkOnCreated.Size = New System.Drawing.Size(169, 17)
        Me.chkOnCreated.TabIndex = 7
        Me.chkOnCreated.Text = "Import Export Files on Creation"
        Me.chkOnCreated.UseVisualStyleBackColor = True
        '
        'MainForm
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(396, 157)
        Me.Controls.Add(Me.chkOnCreated)
        Me.Controls.Add(Me.btnConfigureSQL)
        Me.Controls.Add(Me.txtProductCode)
        Me.Controls.Add(Me.Label3)
        Me.Controls.Add(Me.btnClose)
        Me.Controls.Add(Me.btnSave)
        Me.Controls.Add(Me.btnRestartService)
        Me.Controls.Add(Me.btnBrowseExportDirectory)
        Me.Controls.Add(Me.txtExportDirectory)
        Me.Controls.Add(Me.Label2)
        Me.Controls.Add(Me.cboCADProgram)
        Me.Controls.Add(Me.Label1)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "MainForm"
        Me.Text = "iPCR CAD Module Settings"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents cboCADProgram As System.Windows.Forms.ComboBox
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents txtExportDirectory As System.Windows.Forms.TextBox
    Friend WithEvents btnBrowseExportDirectory As System.Windows.Forms.Button
    Friend WithEvents btnRestartService As System.Windows.Forms.Button
    Friend WithEvents btnSave As System.Windows.Forms.Button
    Friend WithEvents btnClose As System.Windows.Forms.Button
    Friend WithEvents txtProductCode As System.Windows.Forms.TextBox
    Friend WithEvents Label3 As System.Windows.Forms.Label
    Friend WithEvents FolderBrowserDialog1 As System.Windows.Forms.FolderBrowserDialog
    Friend WithEvents btnConfigureSQL As System.Windows.Forms.Button
    Friend WithEvents chkOnCreated As CheckBox
End Class
