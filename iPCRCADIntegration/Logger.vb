Option Explicit On
Option Strict On

Imports System.Text

Public Module Logger

    Public Sub WriteLogEntry(Text As String, TargetEventLog As EventLog)

        Try

            ' write to log
            TargetEventLog.WriteEntry(Text)

        Catch subEx As Exception

            Dim logsDirectory As String = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs")
            System.IO.Directory.CreateDirectory(logsDirectory)

            Dim stream As New System.IO.FileStream(System.IO.Path.Combine(logsDirectory, "iPCRCADModule.log"), System.IO.FileMode.Append, System.IO.FileAccess.Write)
            Dim writer As New System.IO.StreamWriter(stream)

            Try

                writer.WriteLine(Text)
                writer.WriteLine(ControlChars.CrLf)

            Catch ex As Exception
                'Not doing anything here

            Finally
                writer.Close()

            End Try

        End Try

    End Sub

End Module
