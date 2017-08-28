
Public Class Info
    Private Shared js As HIDData
    Private Shared mux As New Threading.Mutex()
    Private Shared salir As Boolean = False
    Private wu As Byte = 0
    Private wd As Byte = 0
    Private pBotones() As PictureBox
    Dim buffGraphics As BufferedGraphics

    Private Declare Auto Function DescalibrarHID Lib "directinput.dll" () As Byte
    Private Declare Auto Function CalibrarHID Lib "directinput.dll" () As Byte

    Private Sub Info_Disposed(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Disposed
        Timer1.Stop()
        CalibrarHID()
        salir = True
        While (salir)
            Threading.Thread.Sleep(25)
        End While
        js.Cerrar()
        buffGraphics.Dispose()
    End Sub

    Private Sub Info_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        Traducir()
        js = New HIDData(Me.ParentForm)
        Me.Focus()
        Dim b() As PictureBox = {PictureBox1, PictureBox2, PictureBox3, PictureBox4, PictureBox5, PictureBox6, PictureBox7, PictureBox8, PictureBox9, PictureBox10, PictureBox11, PictureBox12, PictureBox13, PictureBox14, PictureBox15, PictureBox16, PictureBox17, PictureBox18, PictureBox19, PictureBox20, PictureBox21, PictureBox22, PictureBox23, PictureBox24, PictureBox25, PictureBox26, PictureBox27, PictureBox28, PictureBox29, PictureBox30, PictureBox31, PictureBox32, PictureBox33, PictureBox34, PictureBox35, PictureBox36, PictureBox37, PictureBox38, PictureBox39, PictureBox40, PictureBox41, PictureBox42, PictureBox43, PictureBox44, PictureBox45, PictureBox46, PictureBox47, PictureBox48, PictureBox49, PictureBox50, PictureBox51, PictureBox52, PictureBox53, PictureBox54, PictureBox55, PictureBox56}
        pBotones = b
        Dim buffContext As BufferedGraphicsContext = BufferedGraphicsManager.Current
        buffGraphics = buffContext.Allocate(PictureBox57.CreateGraphics(), PictureBox57.DisplayRectangle)
        buffGraphics.Graphics.TranslateTransform(-20, -22)
        buffGraphics.Graphics.DrawImage(My.Resources.CalibrationGrid, -224, -30, Me.ParentForm.ClientRectangle.Width, Me.ParentForm.ClientRectangle.Height)
        Timer1.Start()
        Dim th As New Threading.Thread(AddressOf Polling)
        th.Priority = Threading.ThreadPriority.AboveNormal
        th.Start()
    End Sub

    Private Sub Traducir()
        GroupBox3.Text = Traduce.Txt("axes")
        GroupBox1.Text = Traduce.Txt("buttons")
        GroupBox2.Text = Traduce.Txt("povs")
        GroupBox4.Text = Traduce.Txt("keyboard")
        GroupBox5.Text = Traduce.Txt("mouse")
        Label14.Text = Traduce.Txt("wheelup")
        Label15.Text = Traduce.Txt("wheeldw")
        CheckBox1.Text = Traduce.Txt("show_raw_data")
    End Sub

    Private Shared Sub Polling()
        While (Not salir)
            mux.WaitOne()
            js.Poll()
            mux.ReleaseMutex()
            Threading.Thread.Sleep(50)
        End While
        salir = False
    End Sub
    Private Sub Timer1_Tick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Timer1.Tick
        'Label12.Text = "X: " & Me.MousePosition.X
        'Label13.Text = "Y: " & Me.MousePosition.Y
        'If (Me.MouseButtons And Windows.Forms.MouseButtons.Left) = Windows.Forms.MouseButtons.Left Then
        '    Label16.BackColor = Color.Red
        'Else
        '    Label16.BackColor = Color.RosyBrown
        'End If
        'If (Me.MouseButtons And Windows.Forms.MouseButtons.Middle) = Windows.Forms.MouseButtons.Middle Then
        '    Label17.BackColor = Color.Red
        'Else
        '    Label17.BackColor = Color.RosyBrown
        'End If
        'If (Me.MouseButtons And Windows.Forms.MouseButtons.Right) = Windows.Forms.MouseButtons.Right Then
        '    Label18.BackColor = Color.Red
        'Else
        '    Label18.BackColor = Color.RosyBrown
        'End If
        'If wu = 5 Then
        '    Label14.BackColor = Color.RosyBrown
        '    wu = 0
        'Else
        '    If wu > 0 Then wu = wu + 1
        'End If
        'If wd = 5 Then
        '    Label15.BackColor = Color.RosyBrown
        '    wd = 0
        'Else
        '    If wd > 0 Then wd = wd + 1
        'End If
        'LeerTeclado()
        'js.Poll()

        'PintarBotones()
        'PintarPOVs()
        'PintarEjes()
        'PintarRuedas()
    End Sub

    Private Sub CheckBox1_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CheckBox1.CheckedChanged
        If CheckBox1.Checked Then
            Dim b As Byte = DescalibrarHID() 'Descalibrar
            If b = 1 Then Traduce.Msg("error_opening_device", MsgBoxStyle.Exclamation)
            If b = 2 Then Traduce.Msg("error_accessing_device", MsgBoxStyle.Exclamation)
        Else
            CalibrarHID()
        End If
        Me.Focus()
    End Sub


#Region "Ruedas"
    Private Sub PintarRuedas()
        PictureBoxrx.Refresh()
        PictureBoxry.Refresh()
    End Sub
    Private Sub PictureBoxrx_Paint(ByVal sender As Object, ByVal e As System.Windows.Forms.PaintEventArgs) Handles PictureBoxrx.Paint
        e.Graphics.DrawEllipse(Pens.RosyBrown, 0, 0, 49, 49)
        mux.WaitOne()
        e.Graphics.FillPie(Brushes.Red, 0, 0, 49, 49, 270, CInt(1.412 * js.Rx))
        Labelrx.Text = "Rx: " & js.Rx
        mux.ReleaseMutex()
    End Sub
    Private Sub PictureBoxry_Paint(ByVal sender As Object, ByVal e As System.Windows.Forms.PaintEventArgs) Handles PictureBoxry.Paint
        e.Graphics.DrawEllipse(Pens.RosyBrown, 0, 0, 49, 49)
        mux.WaitOne()
        e.Graphics.FillPie(Brushes.Red, 0, 0, 49, 49, 270, CInt(1.412 * js.Ry))
        Labelry.Text = "Ry: " & js.Ry
        mux.ReleaseMutex()
    End Sub
#End Region

#Region "Botones"

    Private Sub PintarBotones()
        For i As Byte = 0 To 55
            mux.WaitOne()
            If i < 10 Then
                PintaBoton(js.GetBoton(i), "0" & i, buffGraphics.Graphics, pBotones(i).Left, pBotones(i).Top)
            Else
                PintaBoton(js.GetBoton(i), i, buffGraphics.Graphics, pBotones(i).Left, pBotones(i).Top)
            End If
            mux.ReleaseMutex()
        Next
        buffGraphics.Render()
    End Sub
    Private Sub PintaBoton(ByVal i As Boolean, ByVal s As String, ByVal g As Graphics, ByVal x As Integer, ByVal y As Integer)
        If i Then g.FillEllipse(Brushes.Red, x + 2, y + 2, 25, 25) Else g.FillEllipse(Brushes.RosyBrown, x + 2, y + 2, 25, 25)
        g.DrawString(s, New Font(FontFamily.GenericMonospace, 10, FontStyle.Bold, GraphicsUnit.Point), Brushes.WhiteSmoke, x + 5, y + 7)
    End Sub
#End Region

#Region "POVS"
    Private Sub PintarPOVs()
        PictureBoxp1.Refresh()
        PictureBoxp2.Refresh()
        PictureBoxp3.Refresh()
        PictureBoxp4.Refresh()
    End Sub
    Private Sub PictureBoxp1_Paint(ByVal sender As Object, ByVal e As System.Windows.Forms.PaintEventArgs) Handles PictureBoxp1.Paint
        e.Graphics.DrawEllipse(Pens.RosyBrown, 0, 0, 77, 77)
        If js.POVs(0) > -1 Then
            e.Graphics.TranslateTransform(39, 39)
            mux.WaitOne()
            e.Graphics.RotateTransform(js.POVs(0) / 100)
            mux.ReleaseMutex()
            e.Graphics.FillEllipse(Brushes.Red, -6, -13, 12, -26)
        End If
    End Sub
    Private Sub PictureBoxp2_Paint(ByVal sender As Object, ByVal e As System.Windows.Forms.PaintEventArgs) Handles PictureBoxp2.Paint
        e.Graphics.DrawEllipse(Pens.RosyBrown, 0, 0, 77, 77)
        If js.POVs(1) > -1 Then
            e.Graphics.TranslateTransform(39, 39)
            mux.WaitOne()
            e.Graphics.RotateTransform(js.POVs(1) / 100)
            mux.ReleaseMutex()
            e.Graphics.FillEllipse(Brushes.Red, -6, -13, 12, -26)
        End If
    End Sub
    Private Sub PictureBoxp3_Paint(ByVal sender As Object, ByVal e As System.Windows.Forms.PaintEventArgs) Handles PictureBoxp3.Paint
        e.Graphics.DrawEllipse(Pens.RosyBrown, 0, 0, 77, 77)
        If js.POVs(2) > -1 Then
            e.Graphics.TranslateTransform(39, 39)
            mux.WaitOne()
            e.Graphics.RotateTransform(js.POVs(2) / 100)
            mux.ReleaseMutex()
            e.Graphics.FillEllipse(Brushes.Red, -6, -13, 12, -26)
        End If
    End Sub
    Private Sub PictureBoxp4_Paint(ByVal sender As Object, ByVal e As System.Windows.Forms.PaintEventArgs) Handles PictureBoxp4.Paint
        e.Graphics.DrawEllipse(Pens.RosyBrown, 0, 0, 77, 77)
        If js.POVs(3) > -1 Then
            e.Graphics.TranslateTransform(39, 39)
            mux.WaitOne()
            e.Graphics.RotateTransform(js.POVs(3) / 100)
            mux.ReleaseMutex()
            e.Graphics.FillEllipse(Brushes.Red, -6, -13, 12, -26)
        End If
    End Sub
#End Region

#Region "Ejes"
    Private Sub PintarEjes()
        PictureBoxsl2.Refresh()
        PictureBoxsl.Refresh()
        PictureBoxz.Refresh()
        PictureBoxr.Refresh()
        PictureBoxxy.Refresh()
        PictureBoxmxy.Refresh()
    End Sub
    Private Sub PictureBoxsl2_Paint(ByVal sender As Object, ByVal e As System.Windows.Forms.PaintEventArgs) Handles PictureBoxsl2.Paint
        mux.WaitOne()
        e.Graphics.FillRectangle(Brushes.Red, 0, 0, js.Slider2 \ 2, 20)
        Labelsl2.Text = "Freno:" & js.Slider2
        mux.ReleaseMutex()
    End Sub
    Private Sub PictureBoxsl_Paint(ByVal sender As Object, ByVal e As System.Windows.Forms.PaintEventArgs) Handles PictureBoxsl.Paint
        mux.WaitOne()
        e.Graphics.FillRectangle(Brushes.Red, 0, 0, js.Slider \ 2, 20)
        Labelsl.Text = Traduce.Txt("slider") & js.Slider
        mux.ReleaseMutex()
    End Sub

    Private Sub PictureBoxz_Paint(ByVal sender As Object, ByVal e As System.Windows.Forms.PaintEventArgs) Handles PictureBoxz.Paint
        mux.WaitOne()
        e.Graphics.FillRectangle(Brushes.Red, 0, 127 - (js.Z \ 2), 20, 127)
        Labelz.Text = "Z: " & js.Z
        mux.ReleaseMutex()
    End Sub

    Private Sub PictureBoxr_Paint(ByVal sender As Object, ByVal e As System.Windows.Forms.PaintEventArgs) Handles PictureBoxr.Paint
        mux.WaitOne()
        e.Graphics.FillRectangle(Brushes.Red, 0, 0, (js.R * 127) \ js.limiteR, 20)
        Labelr.Text = "R: " & js.R
        mux.ReleaseMutex()
    End Sub

    Private Sub PictureBoxxy_Paint(ByVal sender As Object, ByVal e As System.Windows.Forms.PaintEventArgs) Handles PictureBoxxy.Paint
        'Dim a, b, c, d As Integer
        mux.WaitOne()
        Dim x As Integer = ((js.X * 121) / js.limitexy)
        Dim y As Integer = ((js.Y * 121) / js.limitexy)
        mux.ReleaseMutex()
        'y = 121 - y
        e.Graphics.DrawLine(Pens.Red, x, y + 3, x + 6, y + 3)
        e.Graphics.DrawLine(Pens.Red, x + 3, y, x + 3, y + 6)
        mux.WaitOne()
        Labelxy.Text = "X: " & js.X & " # Y:" & js.Y
        mux.ReleaseMutex()
    End Sub

    Private Sub PictureBoxmxy_Paint(ByVal sender As Object, ByVal e As System.Windows.Forms.PaintEventArgs) Handles PictureBoxmxy.Paint
        'Dim a, b, c, d As Integer
        mux.WaitOne()
        Dim x As Integer = ((js.mjX * 59) / 15)
        Dim y As Integer = ((js.mjY * 59) / 15)
        mux.ReleaseMutex()
        'y = 59 - y
        e.Graphics.DrawLine(Pens.Red, x, y + 2, x + 4, y + 2)
        e.Graphics.DrawLine(Pens.Red, x + 2, y, x + 2, y + 4)
        mux.WaitOne()
        Labelmxy.Text = "Ministick X: " & js.mjX & " # Y:" & js.mjY
        mux.ReleaseMutex()
    End Sub
#End Region

#Region "Raton"
    Private Sub Info_MouseWheel(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles Me.MouseWheel, GroupBox5.MouseWheel, GroupBox1.MouseWheel, GroupBox2.MouseWheel, GroupBox3.MouseWheel, GroupBox4.MouseWheel
        If e.Delta > 0 Then
            wu = 1
            Label14.BackColor = Color.Red
        ElseIf e.Delta < 0 Then
            wd = 1
            Label15.BackColor = Color.Red
        End If
    End Sub
#End Region

#Region "Teclado"
    Private Declare Auto Function GetKeyboardState Lib "user32.dll" (ByVal buf() As Byte) As Byte
    'Public Overrides Function PreProcessMessage(ByRef msg As Message) As Boolean
    Private Sub LeerTeclado()
        Dim buf(255) As Byte
        Dim s As String = ""
        If GetKeyboardState(buf) = 1 Then
            If (buf(16) And 128) And (Not (buf(&HA0) And 128)) And (Not (buf(&HA1) And 128)) Then
                s = "Shift"
            End If
            If (buf(17) And 128) And (Not (buf(&HA2) And 128)) And (Not (buf(&HA3) And 128)) Then
                If s = "" Then s = "Control" Else s = s & " + Control"
            End If
            If (buf(18) And 128) And (Not (buf(&HA4) And 128)) And (Not (buf(&HA5) And 128)) Then
                If s = "" Then s = "Alt" Else s = s & " + Alt"
            End If
            If buf(&HA0) And 128 Then
                If s = "" Then s = "LShift" Else s = s & " + LShift"
            End If
            If buf(&HA1) And 128 Then
                If s = "" Then s = "RShift" Else s = s & " + RShift"
            End If
            If buf(&HA2) And 128 Then
                If s = "" Then s = "LControl" Else s = s & " + LControl"
            End If
            If buf(&HA3) And 128 Then
                If s = "" Then s = "RControl" Else s = s & " + RControl"
            End If
            If buf(&HA4) And 128 Then
                If s = "" Then s = "LAlt" Else s = s & " + LAlt"
            End If
            If buf(&HA5) And 128 Then
                If s = "" Then s = "RAlt" Else s = s & " + RAlt"
            End If
            If buf(&H5B) And 128 Then
                If s = "" Then s = "LWin" Else s = s & " + LWin"
            End If
            If buf(&H5C) And 128 Then
                If s = "" Then s = "RWin" Else s = s & " + RWin"
            End If
            For i As UInt16 = 7 To 255
                If (i < 16 Or i > 18) And i <> &H5B And i <> &H5C And (i < &HA0 Or i > &HA5) Then
                    If (buf(i) And 128) Then
                        If s = "" Then s = CType(i, Keys).ToString() Else s = s & " + " & CType(i, Keys).ToString()
                    End If
                End If
            Next
            Label19.Text = s
        End If
        'Return True 'MyBase.PreProcessMessage(msg)
    End Sub
#End Region

End Class
