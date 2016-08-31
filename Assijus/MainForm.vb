Imports System.ComponentModel

Public Class MainForm
    Delegate Sub StartBatchSign()
    Public BatchSignDelegate As StartBatchSign
    Public ctx As AppContext
    Friend docs() As Assijus.Doc
    Dim dicSelected As New Dictionary(Of String, Boolean)
    Dim dicStatus As New Dictionary(Of String, String)
    Dim cAssinados As Integer
    Dim cErros As Integer
    Dim tt1Presented As Boolean

    Friend Sub batchSignMethod()
        Try
            If Not IsNothing(docs) Then
                ToolStripStatusLabel1.Text = docs.Length & " documentos recebidos."
                Me.grid.RowCount = docs.Length
            Else
                ToolStripStatusLabel1.Text = "Nenhum documento recebido."
                Me.grid.RowCount = 0
            End If
            butSelectAll.Enabled = Me.grid.RowCount > 0
            butDeselectAll.Enabled = Me.grid.RowCount > 0

            dicSelected.Clear()
            If Not IsNothing(docs) Then
                For Each doc In docs
                    dicSelected(doc.id) = True
                Next
                produceAuthKey()
                If Not wrkSign.IsBusy Then
                    ' Start the asynchronous operation.
                    wrkSign.RunWorkerAsync()
                End If
            End If
            Me.grid.Invalidate()
            butList.Enabled = True
            updateButSign()
        Catch ex As Exception
            Dim rex As RestException = New RestException("iniciar lote", ex)
            ctx.wsSend("{""kind"":""FAILED"",""response"":" & rex.ToJSON() & "}")
        End Try
    End Sub

    Private Sub mainfrm_Load(sender As Object, e As EventArgs) Handles Me.Load
        grid.VirtualMode = True
        grid.AllowUserToAddRows = False

        Dim chk As New DataGridViewCheckBoxColumn()
        grid.Columns.Add(chk)
        chk.HeaderText = "Sel."
        chk.Name = "Sel."

        grid.ColumnCount = 6
        grid.Columns(1).Name = "Número"
        grid.Columns(2).Name = "Descrição"
        grid.Columns(3).Name = "Tipo"
        grid.Columns(4).Name = "Origem"
        grid.Columns(5).Name = "Status"

        BatchSignDelegate = New StartBatchSign(AddressOf batchSignMethod)
        'Atualizar.PerformClick()
    End Sub


    Private Sub MainForm_Shown(sender As Object, e As EventArgs) Handles Me.Shown
        'If Not tt1Presented Then
        'ToolTip1.Show("Clique aqui para selecionar o certificado e carregar a lista de documentos a serem assinados.", butList)
        'tt1Presented = True
        'End If
    End Sub

    Private Sub MainForm_GotFocus(sender As Object, e As EventArgs) Handles Me.GotFocus

    End Sub


    Private Sub mainfrm_Activated(sender As Object, e As EventArgs) Handles Me.Activated

    End Sub

    Private Sub butList_Click(sender As Object, e As EventArgs) Handles butList.Click
        'Assijus.cert()
        'Assijus.produceToken()
        'docs = Assijus.getList()
        'Me.grid.RowCount = docs.Length
        wkrList.RunWorkerAsync()
    End Sub

    Private Sub wkrList_DoWork(sender As Object, e As DoWorkEventArgs) Handles wkrList.DoWork
        Dim worker As BackgroundWorker = TryCast(sender, BackgroundWorker)
        Try
            worker.ReportProgress(0)
            Assijus.cert()
            worker.ReportProgress(25)
            Assijus.produceAuthKey()
            worker.ReportProgress(50)
            docs = Assijus.getList()
            worker.ReportProgress(100)
        Catch ex As Exception
            MsgBox("Erro durante a atualização da lista: " & ex.Message, MsgBoxStyle.OkOnly Or MsgBoxStyle.Critical)
        End Try
    End Sub

    Private Sub wkrList_ProgressChanged(sender As Object, e As ProgressChangedEventArgs) Handles wkrList.ProgressChanged
        ToolStripProgressBar1.Visible = True
        ToolStripStatusLabel1.Text = "Listando..."
        ToolStripStatusLabel2.Text = ""
        If e.ProgressPercentage = 0 Then
            butList.Enabled = False
            ToolStripStatusLabel2.Text = "Selecionando o certificado..."
        ElseIf e.ProgressPercentage = 25 Then
            ToolStripStatusLabel2.Text = "Autenticando usuário..."
        ElseIf e.ProgressPercentage = 50 Then
            ToolStripStatusLabel2.Text = "Atualizando a lista de documentos..."
        End If
        ToolStripProgressBar1.Value = e.ProgressPercentage
    End Sub


    Private Sub wkrList_RunWorkerCompleted(sender As Object, e As RunWorkerCompletedEventArgs) Handles wkrList.RunWorkerCompleted
        If Not IsNothing(docs) Then
            ToolStripStatusLabel1.Text = docs.Length & " documentos listados."
            Me.grid.RowCount = docs.Length
        Else
            ToolStripStatusLabel1.Text = "Erro na atualização."
            Me.grid.RowCount = 0
        End If
        butSelectAll.Enabled = Me.grid.RowCount > 0
        butDeselectAll.Enabled = Me.grid.RowCount > 0
        ToolStripProgressBar1.Visible = False
        ToolStripStatusLabel2.Text = ""
        Me.grid.Invalidate()
        butList.Enabled = True
        updateButSign()
    End Sub

    Private Function getStatus(id As String) As String
        If dicStatus.ContainsKey(id) Then
            Return dicStatus(id)
        Else
            Return ""
        End If
    End Function

    Private Function isSelected(id As String) As Boolean
        If dicSelected.ContainsKey(id) Then
            Return dicSelected(id) AndAlso Not getStatus(id).StartsWith("OK")
        Else
            Return False
        End If
    End Function

    Private Sub updateButSign()
        If (IsNothing(docs)) Then
            butSign.Text = "Assinar"
            butSign.Enabled = False
            Return
        End If

        Dim i As Integer = 0
        For Each doc In docs
            If isSelected(doc.id) Then
                i = i + 1
            End If
        Next
        butSign.Enabled = i > 0
        If butSign.Enabled Then
            butSign.Text = "Assinar (" & i & ")"
        Else
            butSign.Text = "Assinar"
        End If
    End Sub


    Private Sub grid_CellValueNeeded(sender As Object, e As DataGridViewCellValueEventArgs) Handles grid.CellValueNeeded

        Dim doc As Assijus.Doc = docs(e.RowIndex)

        ' Set the cell value to paint using the Customer object retrieved.
        Select Case Me.grid.Columns(e.ColumnIndex).Name
            Case "Sel."
                e.Value = isSelected(doc.id)
            Case "Número"
                e.Value = doc.code
            Case "Descrição"
                e.Value = doc.descr
            Case "Tipo"
                e.Value = doc.kind
            Case "Origem"
                e.Value = doc.origin
            Case "Status"
                e.Value = getStatus(doc.id)
        End Select
    End Sub

    Private Sub butSelectAll_Click(sender As Object, e As EventArgs) Handles butSelectAll.Click
        For Each doc In docs
            dicSelected(doc.id) = True
        Next
        grid.InvalidateColumn(0)
        updateButSign()
        ToolStripStatusLabel1.Text = ""
    End Sub

    Private Sub butDeselectAll_Click(sender As Object, e As EventArgs) Handles butDeselectAll.Click
        For Each doc In docs
            dicSelected(doc.id) = False
        Next
        grid.InvalidateColumn(0)
        updateButSign()
        ToolStripStatusLabel1.Text = ""
    End Sub

    Private Sub butSign_Click(sender As Object, e As EventArgs) Handles butSign.Click
        If Not wrkSign.IsBusy Then
            ' Start the asynchronous operation.
            wrkSign.RunWorkerAsync()
        End If
    End Sub

    Private Sub wkrSign_DoWork(sender As Object, e As DoWorkEventArgs) Handles wrkSign.DoWork
        Dim worker As BackgroundWorker = TryCast(sender, BackgroundWorker)
        cAssinados = 0
        cErros = 0

        worker.ReportProgress(0)
        Try
            Dim i As Integer = 0
            Dim j As Integer = 0
            For Each doc In docs
                If worker.CancellationPending Then
                    e.Cancel = True
                    Exit For
                Else
                    If isSelected(doc.id) Then
                        Dim saveResp As String = ""
                        Try
                            dicStatus(doc.id) = Assijus.sign(doc, saveResp)
                        Catch ex As RestException
                            saveResp = ex.ToJSON()
                        End Try
                        If (Not saveResp.StartsWith("{")) Then
                            saveResp = """" & saveResp.Replace("""", "'") & """"
                        End If
                        If dicStatus.ContainsKey(doc.id) AndAlso dicStatus(doc.id).StartsWith("OK") Then
                            cAssinados = cAssinados + 1
                        Else
                            cErros = cErros + 1
                        End If
                        worker.ReportProgress((i + 1) / docs.Length * 99.99, doc)
                        ctx.wsSend("{""kind"":""SIGNED"",""id"":""" & doc.id & """,""response"":" & saveResp & "}")
                    End If
                    i = i + 1
                End If
            Next
        Catch ex As Exception
            Dim rex As RestException = New RestException("assinar lote", ex)
            ctx.wsSend("{""kind"":""FAILED"",""response"":" & rex.ToJSON() & "}")
        End Try
        worker.ReportProgress(100)
    End Sub

    Private Sub wrkSign_ProgressChanged(sender As Object, e As ProgressChangedEventArgs) Handles wrkSign.ProgressChanged
        If e.ProgressPercentage = 0 Then
            ToolStripStatusLabel1.Text = "Assinando..."
            ctx.wsSend("{""kind"":""STARTED""}")
            Return
        ElseIf e.ProgressPercentage = 100 Then
            ToolStripStatusLabel1.Text = "Documentos assinados"
            ToolStripStatusLabel2.Text = ""
            ToolStripProgressBar1.Visible = False
            butSign.Visible = True
            butCancel.Visible = False
            ctx.wsSend("{""kind"":""FINISHED""}")
            Return
        End If

        Dim doc As Assijus.Doc = TryCast(e.UserState, Assijus.Doc)
        butSign.Visible = False
        butCancel.Visible = True
        ToolStripProgressBar1.Visible = True
        ToolStripProgressBar1.Value = e.ProgressPercentage
        ToolStripStatusLabel2.Text = doc.code
        Dim i As Integer = 0
        For Each d In docs
            If (d.id = doc.id) Then
                Me.grid.InvalidateRow(i)
                ctx.wsSend("{""kind"":""PROGRESS"",""msg"":""" & doc.code & ": Assinado."",""percentage"":""" & e.ProgressPercentage & """}")
            End If
            i = i + 1
        Next
    End Sub

    Private Sub wrkSign_RunWorkerCompleted(sender As Object, e As RunWorkerCompletedEventArgs) Handles wrkSign.RunWorkerCompleted
        ToolStripStatusLabel1.Text = cAssinados & " documentos assinados / " & cErros & " erros."
        ToolStripStatusLabel2.Text = ""
        ToolStripProgressBar1.Visible = False
        butSign.Visible = True
        butCancel.Visible = False
    End Sub

    Private Sub butCancel_Click(sender As Object, e As EventArgs) Handles butCancel.Click
        wrkSign.CancelAsync()
    End Sub

    Private Sub grid_CellClick(sender As Object, e As DataGridViewCellEventArgs) Handles grid.CellClick
        If e.ColumnIndex = 0 AndAlso Not IsNothing(docs) AndAlso docs.Length > e.RowIndex Then
            Dim id As String = docs(e.RowIndex).id
            dicSelected(id) = Not isSelected(id)
            dicSelected(id) = isSelected(id)
            updateButSign()
            ToolStripStatusLabel1.Text = ""
        End If
    End Sub

    Private Sub MainForm_Closing(sender As Object, e As CancelEventArgs) Handles Me.Closing
        Hide()
        e.Cancel = True
    End Sub
End Class
