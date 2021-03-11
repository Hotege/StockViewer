<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class StockViewer
    Inherits System.Windows.Forms.Form

    'Form 重写 Dispose，以清理组件列表。
    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    Protected Overrides ReadOnly Property CreateParams() As System.Windows.Forms.CreateParams
        Get
            Dim cp As System.Windows.Forms.CreateParams
            cp = MyBase.CreateParams
            cp.ExStyle = cp.ExStyle Xor &H80
            Return cp
        End Get
    End Property

    'Windows 窗体设计器所必需的
    Private components As System.ComponentModel.IContainer

    '注意: 以下过程是 Windows 窗体设计器所必需的
    '可以使用 Windows 窗体设计器修改它。  
    '不要使用代码编辑器修改它。
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Me.LabelClose = New System.Windows.Forms.Label()
        Me.SuspendLayout()
        '
        'LabelClose
        '
        Me.LabelClose.AutoSize = True
        Me.LabelClose.BackColor = System.Drawing.Color.Gray
        Me.LabelClose.ForeColor = System.Drawing.Color.Black
        Me.LabelClose.Location = New System.Drawing.Point(104, 3)
        Me.LabelClose.Name = "LabelClose"
        Me.LabelClose.Size = New System.Drawing.Size(11, 12)
        Me.LabelClose.TabIndex = 0
        Me.LabelClose.Text = "X"
        '
        'StockViewer
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.BackColor = System.Drawing.Color.Black
        Me.ClientSize = New System.Drawing.Size(160, 16)
        Me.Controls.Add(Me.LabelClose)
        Me.DoubleBuffered = True
        Me.Font = New System.Drawing.Font("宋体", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None
        Me.KeyPreview = True
        Me.Name = "StockViewer"
        Me.Opacity = 0.25R
        Me.ShowIcon = False
        Me.ShowInTaskbar = False
        Me.Text = "StockViewer"
        Me.TopMost = True
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents LabelClose As Label
End Class
