Option Explicit On
Option Strict On

Imports System.Xml
Imports System.IO

Public Class SettingsClass

    Public Property File As String
    Public Const FILE_NAME As String = "iPCRCADModule.config"

    Public Sub New(filename As String)

        Me.File = filename

    End Sub

    Public Function ReadValue(ByVal section As String, ByVal entry As String, ByVal defaultValue As String) As String

        Dim reader As XmlTextReader = Nothing
        Dim fileName As String = Me.File

        If Not System.IO.File.Exists(fileName) Then Return defaultValue.Trim

        Try
            reader = New XmlTextReader(fileName)
            reader.WhitespaceHandling = WhitespaceHandling.None

            While reader.Read()

                ' Look for the <Configuration> section. This lets us add several pieces in the config file.
                If reader.Name.ToUpper = "Configuration".ToUpper Then

                    ' We are in the correct space, now for the requested section.
                    While reader.Read()

                        If reader.NodeType = XmlNodeType.Element AndAlso reader.Name = "Section" AndAlso reader.HasAttributes Then

                            ' See if the attribute is the section we are looking for.
                            reader.MoveToFirstAttribute()

                            If reader.Name = "Name" And reader.Value = section Then

                                ' Section found. Now look for the correct entry (key) item.
                                While reader.Read()

                                    If reader.NodeType = XmlNodeType.Element AndAlso reader.HasAttributes AndAlso reader.Name.ToString() = "Key" Then

                                        ' Walk and look at all elements.
                                        reader.MoveToFirstAttribute()

                                        If reader.Name = "Name" AndAlso reader.Value = entry Then

                                            ' Get the next attribute.
                                            reader.MoveToNextAttribute()

                                            If reader.Name = "Value" Then
                                                Return reader.Value.Trim
                                            End If

                                        End If

                                    End If

                                End While

                            End If

                        End If

                    End While

                End If

            End While

            ' If we get here, nothing was found.
            Return defaultValue.Trim

        Catch ex As Exception
            Err.Raise(Err.Number, Err.Source, Err.Description)
            Return defaultValue.Trim

        Finally
            reader.Close()

        End Try

    End Function

    Public Function WriteValue(ByVal section As String, ByVal key As String, ByVal dataValue As String, _
 Optional ByVal removeElement As Boolean = False) As Boolean

        Dim xmlDoc As New XmlDocument
        Dim foundSection As Boolean = False
        Dim foundNode As Boolean = False
        Dim fileName As String = Me.File

        'Check to see if the file exists.
        Try
            xmlDoc.Load(fileName)

            Dim docElement As XmlElement = xmlDoc.DocumentElement

            ' Check the name to see if the node is <Configuration>.
            If docElement.Name.ToUpper = "Configuration".ToUpper Then

                foundNode = True

                ' Each child node is a 'Section' marker.
                For Each childElement As XmlElement In docElement.ChildNodes

                    ' Make certain of the structure of the file.
                    If childElement.Name = "Section" Then

                        ' Check the attribute of the section.
                        If childElement.Attributes(0).Name = "Name" And childElement.Attributes(0).Value = section Then

                            Dim keyFound As Boolean = False

                            For Each element As XmlElement In childElement.ChildNodes

                                ' Make certain of the structure of the file.
                                If element.Name = "Key" Then

                                    ' Check the attribute of the key.
                                    If element.Attributes(0).Name = "Name" And element.Attributes(0).Value = key Then

                                        ' Update the value.
                                        If removeElement Then
                                            childElement.RemoveChild(element)
                                        Else
                                            element.SetAttribute("Value", dataValue.Trim())
                                            childElement.AppendChild(element)
                                        End If

                                        keyFound = True

                                        Exit For
                                    End If
                                End If

                            Next

                            ' If key not found, create the key.
                            If Not keyFound And Not removeElement Then

                                Dim element As XmlElement = xmlDoc.CreateElement("Key")

                                element.SetAttribute("Name", key)
                                element.SetAttribute("Value", dataValue.Trim())

                                childElement.AppendChild(element)

                            End If

                            foundSection = True
                            Exit For

                        End If

                    End If

                Next

                ' If section not found, create the section and the key.
                If Not foundSection And Not removeElement Then

                    Dim sectionElement As XmlElement = xmlDoc.CreateElement("Section")
                    sectionElement.SetAttribute("Name", section)

                    Dim keyElement As XmlElement = xmlDoc.CreateElement("Key")
                    keyElement.SetAttribute("Name", key)
                    keyElement.SetAttribute("Value", dataValue.Trim())
                    sectionElement.AppendChild(keyElement)

                    docElement.AppendChild(sectionElement)
                End If

            End If

            ' Write the document back to disk.
            xmlDoc.Save(fileName)

        Catch ex As FileNotFoundException 'XmlException

            If Not removeElement Then

                ' File does not exist.
                Dim newDoc As New XmlDocument
                Dim docElement As XmlElement
                docElement = newDoc.CreateElement("Configuration")
                Dim sectionElement As XmlElement = newDoc.CreateElement("Section")
                sectionElement.SetAttribute("Name", section)

                Dim keyElement As XmlElement = newDoc.CreateElement("Key")
                keyElement.SetAttribute("Name", section)
                keyElement.SetAttribute("Value", dataValue.Trim())
                sectionElement.AppendChild(keyElement)

                docElement.AppendChild(sectionElement)
                newDoc.AppendChild(docElement)

                newDoc.Save(fileName)

            End If

        Catch ex As Exception

            Throw

        End Try

    End Function

End Class
