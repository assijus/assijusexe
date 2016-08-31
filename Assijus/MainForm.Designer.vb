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
        Me.components = New System.ComponentModel.Container()
        Me.grid = New System.Windows.Forms.DataGridView()
        Me.TableLayoutPanel1 = New System.Windows.Forms.TableLayoutPanel()
        Me.FlowLayoutPanel1 = New System.Windows.Forms.FlowLayoutPanel()
        Me.butList = New System.Windows.Forms.Button()
        Me.butSelectAll = New System.Windows.Forms.Button()
        Me.butDeselectAll = New System.Windows.Forms.Button()
        Me.butCancel = New System.Windows.Forms.Button()
        Me.butSign = New System.Windows.Forms.Button()
        Me.StatusStrip = New System.Windows.Forms.StatusStrip()
        Me.ToolStripStatusLabel1 = New System.Windows.Forms.ToolStripStatusLabel()
        Me.ToolStripProgressBar1 = New System.Windows.Forms.ToolStripProgressBar()
        Me.ToolStripStatusLabel2 = New System.Windows.Forms.ToolStripStatusLabel()
        Me.wrkSign = New System.ComponentModel.BackgroundWorker()
        Me.wkrList = New System.ComponentModel.BackgroundWorker()
        Me.ToolTip1 = New System.Windows.Forms.ToolTip(Me.components)
        CType(Me.grid, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.FlowLayoutPanel1.SuspendLayout()
        Me.StatusStrip.SuspendLayout()
        Me.SuspendLayout()
        '
        'grid
        '
        Me.grid.AllowUserToAddRows = False
        Me.grid.AllowUserToDeleteRows = False
        Me.grid.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill
        Me.grid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.grid.Dock = System.Windows.Forms.DockStyle.Fill
        Me.grid.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically
        Me.grid.ImeMode = System.Windows.Forms.ImeMode.Off
        Me.grid.Location = New System.Drawing.Point(0, 0)
        Me.grid.MultiSelect = False
        Me.grid.Name = "grid"
        Me.grid.ReadOnly = True
        Me.grid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect
        Me.grid.ShowEditingIcon = False
        Me.grid.Size = New System.Drawing.Size(1108, 525)
        Me.grid.TabIndex = 2
        Me.grid.VirtualMode = True
        '
        'TableLayoutPanel1
        '
        Me.TableLayoutPanel1.AutoSize = True
        Me.TableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
        Me.TableLayoutPanel1.ColumnCount = 2
        Me.TableLayoutPanel1.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50.0!))
        Me.TableLayoutPanel1.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50.0!))
        Me.TableLayoutPanel1.Location = New System.Drawing.Point(2, 2)
        Me.TableLayoutPanel1.Name = "TableLayoutPanel1"
        Me.TableLayoutPanel1.RowCount = 2
        Me.TableLayoutPanel1.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50.0!))
        Me.TableLayoutPanel1.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50.0!))
        Me.TableLayoutPanel1.Size = New System.Drawing.Size(0, 0)
        Me.TableLayoutPanel1.TabIndex = 3
        '
        'FlowLayoutPanel1
        '
        Me.FlowLayoutPanel1.AutoSize = True
        Me.FlowLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
        Me.FlowLayoutPanel1.Controls.Add(Me.butList)
        Me.FlowLayoutPanel1.Controls.Add(Me.butSelectAll)
        Me.FlowLayoutPanel1.Controls.Add(Me.butDeselectAll)
        Me.FlowLayoutPanel1.Controls.Add(Me.butCancel)
        Me.FlowLayoutPanel1.Controls.Add(Me.butSign)
        Me.FlowLayoutPanel1.Controls.Add(Me.StatusStrip)
        Me.FlowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Bottom
        Me.FlowLayoutPanel1.Location = New System.Drawing.Point(0, 496)
        Me.FlowLayoutPanel1.Name = "FlowLayoutPanel1"
        Me.FlowLayoutPanel1.Size = New System.Drawing.Size(1108, 29)
        Me.FlowLayoutPanel1.TabIndex = 4
        '
        'butList
        '
        Me.butList.AutoSize = True
        Me.butList.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
        Me.butList.Location = New System.Drawing.Point(3, 3)
        Me.butList.Name = "butList"
        Me.butList.Size = New System.Drawing.Size(57, 23)
        Me.butList.TabIndex = 1
        Me.butList.Text = "Atualizar"
        Me.butList.UseVisualStyleBackColor = True
        '
        'butSelectAll
        '
        Me.butSelectAll.AutoSize = True
        Me.butSelectAll.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
        Me.butSelectAll.Enabled = False
        Me.butSelectAll.Location = New System.Drawing.Point(66, 3)
        Me.butSelectAll.Name = "butSelectAll"
        Me.butSelectAll.Size = New System.Drawing.Size(83, 23)
        Me.butSelectAll.TabIndex = 2
        Me.butSelectAll.Text = "Marcar Todos"
        Me.butSelectAll.UseVisualStyleBackColor = True
        '
        'butDeselectAll
        '
        Me.butDeselectAll.AutoSize = True
        Me.butDeselectAll.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
        Me.butDeselectAll.Enabled = False
        Me.butDeselectAll.Location = New System.Drawing.Point(155, 3)
        Me.butDeselectAll.Name = "butDeselectAll"
        Me.butDeselectAll.Size = New System.Drawing.Size(101, 23)
        Me.butDeselectAll.TabIndex = 3
        Me.butDeselectAll.Text = "Desmarcar Todos"
        Me.butDeselectAll.UseVisualStyleBackColor = True
        '
        'butCancel
        '
        Me.butCancel.AutoSize = True
        Me.butCancel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
        Me.butCancel.Location = New System.Drawing.Point(262, 3)
        Me.butCancel.Name = "butCancel"
        Me.butCancel.Size = New System.Drawing.Size(218, 23)
        Me.butCancel.TabIndex = 5
        Me.butCancel.Text = "Interromper Processamento de Assinaturas"
        Me.butCancel.UseVisualStyleBackColor = True
        Me.butCancel.Visible = False
        '
        'butSign
        '
        Me.butSign.AutoSize = True
        Me.butSign.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
        Me.butSign.Enabled = False
        Me.butSign.Location = New System.Drawing.Point(486, 3)
        Me.butSign.Name = "butSign"
        Me.butSign.Size = New System.Drawing.Size(51, 23)
        Me.butSign.TabIndex = 6
        Me.butSign.Text = "Assinar"
        Me.butSign.UseVisualStyleBackColor = True
        '
        'StatusStrip
        '
        Me.StatusStrip.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None
        Me.StatusStrip.Dock = System.Windows.Forms.DockStyle.Fill
        Me.StatusStrip.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.ToolStripStatusLabel1, Me.ToolStripProgressBar1, Me.ToolStripStatusLabel2})
        Me.StatusStrip.Location = New System.Drawing.Point(540, 0)
        Me.StatusStrip.Name = "StatusStrip"
        Me.StatusStrip.Size = New System.Drawing.Size(17, 29)
        Me.StatusStrip.SizingGrip = False
        Me.StatusStrip.TabIndex = 4
        Me.StatusStrip.Text = "StatusStrip"
        '
        'ToolStripStatusLabel1
        '
        Me.ToolStripStatusLabel1.Name = "ToolStripStatusLabel1"
        Me.ToolStripStatusLabel1.Size = New System.Drawing.Size(0, 24)
        '
        'ToolStripProgressBar1
        '
        Me.ToolStripProgressBar1.Name = "ToolStripProgressBar1"
        Me.ToolStripProgressBar1.Size = New System.Drawing.Size(100, 23)
        Me.ToolStripProgressBar1.Visible = False
        '
        'ToolStripStatusLabel2
        '
        Me.ToolStripStatusLabel2.Name = "ToolStripStatusLabel2"
        Me.ToolStripStatusLabel2.Size = New System.Drawing.Size(0, 24)
        '
        'wrkSign
        '
        Me.wrkSign.WorkerReportsProgress = True
        Me.wrkSign.WorkerSupportsCancellation = True
        '
        'wkrList
        '
        Me.wkrList.WorkerReportsProgress = True
        '
        'ToolTip1
        '
        Me.ToolTip1.IsBalloon = True
        '
        'MainForm
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(1108, 525)
        Me.Controls.Add(Me.FlowLayoutPanel1)
        Me.Controls.Add(Me.TableLayoutPanel1)
        Me.Controls.Add(Me.grid)
        Me.Name = "MainForm"
        Me.Text = "Assijus Windows v0.1"
        Me.WindowState = System.Windows.Forms.FormWindowState.Minimized
        CType(Me.grid, System.ComponentModel.ISupportInitialize).EndInit()
        Me.FlowLayoutPanel1.ResumeLayout(False)
        Me.FlowLayoutPanel1.PerformLayout()
        Me.StatusStrip.ResumeLayout(False)
        Me.StatusStrip.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents grid As DataGridView
    Friend WithEvents TableLayoutPanel1 As TableLayoutPanel
    Friend WithEvents FlowLayoutPanel1 As FlowLayoutPanel
    Friend WithEvents butList As Button
    Friend WithEvents butSelectAll As Button
    Friend WithEvents butDeselectAll As Button
    Friend WithEvents StatusStrip As StatusStrip
    Friend WithEvents ToolStripStatusLabel1 As ToolStripStatusLabel
    Friend WithEvents ToolStripProgressBar1 As ToolStripProgressBar
    Friend WithEvents ToolStripStatusLabel2 As ToolStripStatusLabel
    Friend WithEvents wrkSign As System.ComponentModel.BackgroundWorker
    Friend WithEvents butCancel As Button
    Friend WithEvents wkrList As System.ComponentModel.BackgroundWorker
    Friend WithEvents butSign As Button
    Friend WithEvents ToolTip1 As ToolTip
End Class
