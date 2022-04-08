' ****************************************************************************************
' ** Class Name:    MySQLDatabaseClass                                                  **
' ** Author:        Jerel Tengan                                                        **
' ** Date:          October 26, 2005                                                    **
' ** Purpose:       Handles all OleDb database access                                   **
' ****************************************************************************************

Option Strict On
Option Explicit On

Imports MySql.Data.MySqlClient
Imports System.IO
Imports System.Text

Public NotInheritable Class MySQLDatabaseClass

#Region " Private Declarations "

    Private _connectionString As String
    Private _connection As MySqlConnection
    Private _transactionBegan As Boolean             ' Flag for whether a transaction has begun (BeginTransaction)
    Private _transactionCommand As MySqlCommand
    Private Shared _commandTimeout As Integer = 300

#End Region

#Region " Properties "

    Public ReadOnly Property ConnectionString() As String

        Get
            Return _connectionString
        End Get

    End Property

    Public ReadOnly Property GetIdentitySqlString() As String

        Get
            Return " SELECT SCOPE_IDENTITY() AS 'NewID'"
        End Get

    End Property

    Public ReadOnly Property TransactionBegan() As Boolean
        Get
            Return _transactionBegan
        End Get
    End Property

    Public ReadOnly Property Connection() As MySqlConnection
        Get
            Return _connection
        End Get
    End Property

    Public Property CommandTimeout() As Integer
        Get
            Return _commandTimeout
        End Get
        Set(ByVal value As Integer)
            _commandTimeout = value
        End Set
    End Property

#End Region

#Region " Public Methods "

    Public Sub New(ByVal connectionString As String)

        _connectionString = connectionString

    End Sub

    Public Sub OpenConnection()

        _connection = New MySqlConnection(_connectionString)
        _connection.Open()

    End Sub

    Public Sub CloseConnection()

        If _connection.State <> ConnectionState.Closed Then _connection.Close()

    End Sub

    Public Function ExecuteSql(ByRef SQL As String) As Boolean

        If Not ConnectionStringSet() Then Return False

        Try

            If _transactionBegan Then

                _transactionCommand.CommandText = SQL
                _transactionCommand.ExecuteNonQuery()

            Else

                _connection = New MySqlConnection(_connectionString)
                _connection.Open()
                Dim cmd As MySqlCommand = _connection.CreateCommand

                cmd.CommandTimeout = _commandTimeout
                cmd.CommandText = SQL
                cmd.ExecuteNonQuery()

            End If

            Return True

        Catch oleex As MySqlException
            Throw

        Catch e As Exception
            Err.Raise(1002, "DatabaseOleDbClass.ExecuteSQL", e.Message)

        Finally

            If Not _transactionBegan Then
                If _connection.State <> ConnectionState.Closed Then _connection.Close()
            End If

        End Try

    End Function

    Public Function ExecuteSqlArray(ByRef SQL() As String) As Boolean

        If Not ConnectionStringSet() Then Exit Function

        Dim item As String
        Dim trans As MySqlTransaction = Nothing

        Try
            _connection = New MySqlConnection(_connectionString)
            _connection.Open()

            Dim cmd As MySqlCommand = _connection.CreateCommand

            ' Start a local transaction
            trans = _connection.BeginTransaction()
            cmd.Transaction = trans

            For Each item In SQL

                cmd.CommandTimeout = _commandTimeout
                cmd.CommandText = item
                cmd.ExecuteNonQuery()

            Next

            ' Commit transaction
            trans.Commit()
            Return True

        Catch oleex As MySqlException

            Throw
            trans.Rollback()

        Catch ex As Exception

            Err.Raise(1002, "DatabaseOleDbClass.ExecuteSqlArray", ex.Message)
            trans.Rollback()

        Finally
            If _connection.State <> ConnectionState.Closed Then _connection.Close()
        End Try

    End Function

    Public Function ReturnDataReader(ByRef SQL As String) As MySqlDataReader

        If Not ConnectionStringSet() Then Return Nothing

        Dim reader As MySqlDataReader = Nothing

        Try
            If _transactionBegan Then

                _transactionCommand.CommandText = SQL
                reader = _transactionCommand.ExecuteReader(CommandBehavior.Default)

            Else
                _connection = New MySqlConnection(_connectionString)
                _connection.Open()

                Dim cmd As MySqlCommand = _connection.CreateCommand
                cmd.CommandTimeout = _commandTimeout
                cmd.CommandText = SQL
                reader = cmd.ExecuteReader(CommandBehavior.CloseConnection)

            End If

        Catch oleex As MySqlException
            Throw

        Catch e As Exception
            Err.Raise(1003, "Database.ReturnDataReader", e.Message)

        End Try

        Return reader

    End Function

    Public Function ReturnDataTable(ByRef SQL As String) As DataTable

        If Not ConnectionStringSet() Then Return Nothing

        Dim command As MySqlCommand
        Dim table As New DataTable
        Dim adapter As MySqlDataAdapter

        Try
            If _transactionBegan Then

                _transactionCommand.CommandText = SQL
                adapter = New MySqlDataAdapter(_transactionCommand)
                adapter.Fill(table)

            Else

                _connection = New MySqlConnection(_connectionString)
                _connection.Open()
                command = _connection.CreateCommand
                command.CommandTimeout = _commandTimeout
                command.CommandText = SQL
                adapter = New MySqlDataAdapter(command)
                adapter.Fill(table)

            End If

            'Catch oleex As MySqlException
            '    Throw
            '    Err.Raise(oleex.Number, "Database.ReturnDataTable", oleex.Message)

        Catch e As Exception
            'Throw 
            Err.Raise(1000, "DatabaseOleDbClass.ReturnDataTable", e.Message)

        Finally

            If Not _transactionBegan Then
                If _connection.State <> ConnectionState.Closed Then _connection.Close()
            End If

        End Try

        Return table

    End Function

    Public Function ReturnDataAdapter(ByRef SQL As String) As MySqlDataAdapter

        If Not ConnectionStringSet() Then Return Nothing

        Dim command As MySqlCommand
        Dim adapter As MySqlDataAdapter

        Try
            If _transactionBegan Then

                _transactionCommand.CommandText = SQL
                adapter = New MySqlDataAdapter(_transactionCommand)

            Else

                _connection = New MySqlConnection(_connectionString)
                _connection.Open()
                command = _connection.CreateCommand
                command.CommandTimeout = _commandTimeout
                command.CommandText = SQL
                adapter = New MySqlDataAdapter(command)

            End If

        Catch oleex As MySqlException
            Throw '(oleex.Number, oleex.Source, oleex.Message)

        Catch e As Exception
            Throw 'Err.Raise(1004, "Database.ReturnDataTable", e.Message)

        Finally

            If Not _transactionBegan Then
                If _connection.State <> ConnectionState.Closed Then _connection.Close()
            End If

        End Try

        Return adapter

    End Function

    Public Sub UpdateDataSet(ByRef adapter As MySqlDataAdapter, ByRef SQL As String)

        If Not ConnectionStringSet() Then Exit Sub

        Dim command As MySqlCommand
        Dim table As New DataTable

        Try

            If _transactionBegan Then

                _transactionCommand.CommandText = SQL
                adapter = New MySqlDataAdapter(_transactionCommand)
                adapter.Fill(table)

            Else

                _connection = New MySqlConnection(_connectionString)
                _connection.Open()
                command = _connection.CreateCommand
                command.CommandText = SQL
                adapter = New MySqlDataAdapter(command)
                adapter.Fill(table)

            End If

        Catch oleex As MySqlException
            Throw '(oleex.Number, oleex.Source, oleex.Message)

        Catch e As Exception
            Throw 'Err.Raise(1004, "Database.ReturnDataTable", e.Message)

        Finally

            If Not _transactionBegan Then
                If _connection.State <> ConnectionState.Closed Then _connection.Close()
            End If

        End Try

    End Sub

    Public Function Null() As String

        Return "NULL"

    End Function

    Public Function BeginTransaction() As Boolean

        If Not ConnectionStringSet() Then Return False

        Dim trans As MySqlTransaction

        Try
            _connection = New MySqlConnection(_connectionString)
            _connection.Open()

            _transactionCommand = _connection.CreateCommand

            ' Start a local transaction
            trans = _connection.BeginTransaction()
            _transactionCommand.Transaction = trans

            _transactionBegan = True

            Return True

        Catch oleex As MySqlException

            _transactionBegan = False
            Throw oleex

        Catch ex As Exception

            _transactionBegan = False
            Err.Raise(1002, "DatabaseOleDbClass.BeginTransaction", ex.Message)

        End Try

    End Function

    Public Function CommitTransaction() As Boolean


        If Not _transactionBegan Then Return False

        ' set flag 
        _transactionBegan = False

        ' execute array of sql statements
        _transactionCommand.Transaction.Commit()
        If _transactionCommand.Connection.State <> ConnectionState.Closed Then _transactionCommand.Connection.Close()

        Return True

    End Function

    Public Function RollbackTransaction() As Boolean

        If Not _transactionBegan Then Return False

        ' set flag 
        _transactionBegan = False

        ' execute array of sql statements
        _transactionCommand.Transaction.Rollback()
        If _transactionCommand.Connection.State <> ConnectionState.Closed Then _transactionCommand.Connection.Close()

        Return True

    End Function

#End Region

#Region " Private Methods "

    Private Function ConnectionStringSet() As Boolean

        If _connectionString = String.Empty Then
            Err.Raise(1001, "DatabaseOleDbClass", "ConnectionString property not set.")
        Else
            Return True
        End If

    End Function

#End Region

#Region " Shared Methods "

    Public Shared Function ScrubString(value As String) As String

        Dim newValue = "'" & value.Replace("'", "''") & "'"

        Return newValue

    End Function

#End Region

End Class

