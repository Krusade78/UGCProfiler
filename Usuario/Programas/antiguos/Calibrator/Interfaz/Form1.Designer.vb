<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Form1
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(Form1))
        Me.RadioButton1 = New System.Windows.Forms.RadioButton
        Me.RadioButton2 = New System.Windows.Forms.RadioButton
        Me.RadioButton3 = New System.Windows.Forms.RadioButton
        Me.RadioButton4 = New System.Windows.Forms.RadioButton
        Me.Label1 = New System.Windows.Forms.Label
        Me.SuspendLayout()
        '
        'RadioButton1
        '
        Me.RadioButton1.Appearance = System.Windows.Forms.Appearance.Button
        Me.RadioButton1.BackColor = System.Drawing.Color.DarkSlateGray
        Me.RadioButton1.Checked = True
        Me.RadioButton1.FlatAppearance.BorderSize = 0
        Me.RadioButton1.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.RadioButton1.ForeColor = System.Drawing.Color.GreenYellow
        Me.RadioButton1.Location = New System.Drawing.Point(150, 1)
        Me.RadioButton1.Name = "RadioButton1"
        Me.RadioButton1.Size = New System.Drawing.Size(95, 23)
        Me.RadioButton1.TabIndex = 0
        Me.RadioButton1.TabStop = True
        Me.RadioButton1.Text = "Joystick test"
        Me.RadioButton1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        Me.RadioButton1.UseVisualStyleBackColor = False
        '
        'RadioButton2
        '
        Me.RadioButton2.Appearance = System.Windows.Forms.Appearance.Button
        Me.RadioButton2.BackColor = System.Drawing.Color.DarkSlateGray
        Me.RadioButton2.FlatAppearance.BorderSize = 0
        Me.RadioButton2.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.RadioButton2.ForeColor = System.Drawing.Color.GreenYellow
        Me.RadioButton2.Location = New System.Drawing.Point(250, 1)
        Me.RadioButton2.Name = "RadioButton2"
        Me.RadioButton2.Size = New System.Drawing.Size(95, 23)
        Me.RadioButton2.TabIndex = 1
        Me.RadioButton2.Text = "DX Calibration"
        Me.RadioButton2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        Me.RadioButton2.UseVisualStyleBackColor = False
        '
        'RadioButton3
        '
        Me.RadioButton3.Appearance = System.Windows.Forms.Appearance.Button
        Me.RadioButton3.BackColor = System.Drawing.Color.DarkSlateGray
        Me.RadioButton3.FlatAppearance.BorderSize = 0
        Me.RadioButton3.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.RadioButton3.ForeColor = System.Drawing.Color.GreenYellow
        Me.RadioButton3.Location = New System.Drawing.Point(350, 1)
        Me.RadioButton3.Name = "RadioButton3"
        Me.RadioButton3.Size = New System.Drawing.Size(155, 23)
        Me.RadioButton3.TabIndex = 2
        Me.RadioButton3.Text = "X36 Gameport calibration"
        Me.RadioButton3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        Me.RadioButton3.UseVisualStyleBackColor = False
        Me.RadioButton3.Visible = False
        '
        'RadioButton4
        '
        Me.RadioButton4.Appearance = System.Windows.Forms.Appearance.Button
        Me.RadioButton4.BackColor = System.Drawing.Color.DarkSlateGray
        Me.RadioButton4.FlatAppearance.BorderSize = 0
        Me.RadioButton4.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.RadioButton4.ForeColor = System.Drawing.Color.GreenYellow
        Me.RadioButton4.Location = New System.Drawing.Point(350, 1)
        Me.RadioButton4.Name = "RadioButton4"
        Me.RadioButton4.Size = New System.Drawing.Size(126, 23)
        Me.RadioButton4.TabIndex = 2
        Me.RadioButton4.Text = "X52 Setup"
        Me.RadioButton4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        Me.RadioButton4.UseVisualStyleBackColor = False
        Me.RadioButton4.Visible = False
        '
        'Label1
        '
        Me.Label1.BackColor = System.Drawing.Color.Black
        Me.Label1.Dock = System.Windows.Forms.DockStyle.Top
        Me.Label1.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label1.ForeColor = System.Drawing.Color.White
        Me.Label1.Location = New System.Drawing.Point(0, 0)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(517, 25)
        Me.Label1.TabIndex = 3
        Me.Label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'Form1
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackColor = System.Drawing.Color.MidnightBlue
        Me.BackgroundImage = Global.Calibrator.My.Resources.Resources.CalibrationGrid
        Me.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch
        Me.ClientSize = New System.Drawing.Size(517, 582)
        Me.Controls.Add(Me.RadioButton4)
        Me.Controls.Add(Me.RadioButton3)
        Me.Controls.Add(Me.RadioButton2)
        Me.Controls.Add(Me.RadioButton1)
        Me.Controls.Add(Me.Label1)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.Name = "Form1"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "XHOTAS Calibrator"
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents RadioButton1 As System.Windows.Forms.RadioButton
    Friend WithEvents RadioButton2 As System.Windows.Forms.RadioButton
    Friend WithEvents RadioButton3 As System.Windows.Forms.RadioButton
    Friend WithEvents RadioButton4 As System.Windows.Forms.RadioButton
    Friend WithEvents Label1 As System.Windows.Forms.Label

End Class
