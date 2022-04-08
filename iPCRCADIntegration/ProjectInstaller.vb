Imports System.ComponentModel
Imports System.Configuration.Install

Public Class ProjectInstaller

    Public Sub New()
        MyBase.New()

        'This call is required by the Component Designer.
        InitializeComponent()

        'Add initialization code after the call to InitializeComponent
        If Not EventLog.SourceExists("iPCRCADModule") Then
            EventLog.CreateEventSource("iPCRCADModule", "iPCRCADModuleLog")
        End If

    End Sub

End Class
