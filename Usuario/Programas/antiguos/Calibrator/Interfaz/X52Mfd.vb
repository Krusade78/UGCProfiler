Public Class X52Mfd
    Private luzMFD As Byte = 1
    Private luz As Byte = 1

    Private Sub X52Mfd_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        Traducir()
        LeerRegistro()
    End Sub

    Private Sub Traducir()
        Label4.Text = Traduce.Txt("note")
        Label5.Text = Traduce.Txt("note_text")
        GroupBox1.Text = Traduce.Txt("button_light")
        GroupBox2.Text = Traduce.Txt("mfd_light")
        RadioButton1.Text = Traduce.Txt("none")
        RadioButton2.Text = Traduce.Txt("low")
        RadioButton3.Text = Traduce.Txt("high")
        RadioButton4.Text = Traduce.Txt("none")
        RadioButton5.Text = Traduce.Txt("low")
        RadioButton6.Text = Traduce.Txt("high")
        Label1.Text = Traduce.Txt("first_clock")
        Label2.Text = Traduce.Txt("second_clock")
        Label3.Text = Traduce.Txt("third_clock")
        Button1.Text = Traduce.Txt("apply")
    End Sub

    Private Sub LeerRegistro()
        Dim reg As Microsoft.Win32.RegistryKey = Nothing
        Dim hora As Int32
        Dim buff(2) As Byte
        Try
            reg = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("SOFTWARE\XHOTAS\Calibrado", False)
            If reg IsNot Nothing Then
                luzMFD = reg.GetValue("LuzMFD", New Byte(0) {1})(0)
                luz = reg.GetValue("Luz", New Byte(0) {1})(0)
                buff = reg.GetValue("Hora1", New Byte(2) {0, 0, 0})
                If buff(0) = 1 Then CheckBox1.Checked = True Else CheckBox1.Checked = False
                hora = (CInt(buff(2)) << 8) + buff(1)
                If hora > 32767 Then hora = hora - 65536
                NumericUpDown1.Value = hora \ 60
                NumericUpDown2.Value = hora Mod 60

                buff = reg.GetValue("Hora2", New Byte(2) {0, 0, 0})
                If buff(0) = 1 Then CheckBox1.Checked = True Else CheckBox1.Checked = False
                If buff(1) > 3 Then
                    NumericUpDown3.Value = -(((buff(1) - 4) * 256) + buff(2)) \ 60
                    NumericUpDown4.Value = (((buff(1) - 4) * 256) + buff(2)) Mod 60
                Else
                    NumericUpDown3.Value = ((buff(1) * 256) + buff(2)) \ 60
                    NumericUpDown4.Value = ((buff(1) * 256) + buff(2)) Mod 60
                End If

                buff = reg.GetValue("Hora3", New Byte(2) {0, 0, 0})
                If buff(0) = 1 Then CheckBox1.Checked = True Else CheckBox1.Checked = False
                If buff(1) > 3 Then
                    NumericUpDown5.Value = -(((buff(1) - 4) * 256) + buff(2)) \ 60
                    NumericUpDown6.Value = (((buff(1) - 4) * 256) + buff(2)) Mod 60
                Else
                    NumericUpDown5.Value = ((buff(1) * 256) + buff(2)) \ 60
                    NumericUpDown6.Value = ((buff(1) * 256) + buff(2)) Mod 60
                End If
                reg.Close()
            End If
        Catch ex As Exception
            If reg IsNot Nothing Then reg.Close()
            MsgBox(ex.Message, MsgBoxStyle.Exclamation, "XCalibrator")
            Exit Sub
        End Try

        Dim checks() As RadioButton = {RadioButton1, RadioButton2, RadioButton3}
        Dim imgs() As Image = {My.Resources.b0, My.Resources.b1, My.Resources.b2}
        checks(luz).Checked = True
        PictureBox1.Image = imgs(luz)
        Dim checks2() As RadioButton = {RadioButton4, RadioButton5, RadioButton6}
        Dim imgs2() As Image = {My.Resources.mfd1, My.Resources.mfd2, My.Resources.mfd3}
        checks2(luzMFD).Checked = True
        PictureBox2.Image = imgs2(luzMFD)
    End Sub

    Private Sub GuardarRegistro()
        Dim reg As Microsoft.Win32.RegistryKey = Nothing
        Dim buff(2) As Byte
        Dim b(0) As Byte
        Dim hora As Int16
        Try
            Microsoft.Win32.Registry.CurrentUser.CreateSubKey("SOFTWARE\XHOTAS\Calibrado")
            reg = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("SOFTWARE\XHOTAS\Calibrado", True)
            If reg IsNot Nothing Then
                b(0) = luzMFD
                reg.SetValue("LuzMFD", b, Microsoft.Win32.RegistryValueKind.Binary)
                b(0) = luz
                reg.SetValue("Luz", b, Microsoft.Win32.RegistryValueKind.Binary)

                If CheckBox1.Checked Then buff(0) = 1 Else buff(0) = 0
                If NumericUpDown1.Value < 0 Then hora = (NumericUpDown1.Value * 60) - NumericUpDown2.Value Else hora = (NumericUpDown1.Value * 60) + NumericUpDown2.Value
                buff(2) = (hora >> 8) And 255
                buff(1) = hora And 255
                reg.SetValue("Hora1", buff, Microsoft.Win32.RegistryValueKind.Binary)

                If CheckBox2.Checked Then buff(0) = 1 Else buff(0) = 0
                If NumericUpDown3.Value < 0 Then
                    buff(1) = (((-NumericUpDown3.Value * 60) + NumericUpDown4.Value) \ 256) + 4
                    buff(2) = (((-NumericUpDown3.Value * 60) + NumericUpDown4.Value) Mod 256)
                Else
                    buff(1) = ((NumericUpDown3.Value * 60) + NumericUpDown4.Value) \ 256
                    buff(2) = ((NumericUpDown3.Value * 60) + NumericUpDown4.Value) Mod 256
                End If
                reg.SetValue("Hora2", buff, Microsoft.Win32.RegistryValueKind.Binary)

                If CheckBox3.Checked Then buff(0) = 1 Else buff(0) = 0
                If NumericUpDown5.Value < 0 Then
                    buff(1) = (((-NumericUpDown5.Value * 60) + NumericUpDown6.Value) \ 256) + 4
                    buff(2) = (((-NumericUpDown5.Value * 60) + NumericUpDown6.Value) Mod 256)
                Else
                    buff(1) = ((NumericUpDown5.Value * 60) + NumericUpDown6.Value) \ 256
                    buff(2) = ((NumericUpDown5.Value * 60) + NumericUpDown6.Value) Mod 256
                End If
                reg.SetValue("Hora3", buff, Microsoft.Win32.RegistryValueKind.Binary)

                reg.Close()
            End If
        Catch ex As Exception
            If reg IsNot Nothing Then reg.Close()
            MsgBox(ex.Message, MsgBoxStyle.Exclamation, "XCalibrator")
            Exit Sub
        End Try
    End Sub

    Private Sub RadioButton1_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles RadioButton1.CheckedChanged
        If RadioButton1.Checked Then luz = 0 : PictureBox1.Image = My.Resources.b0
    End Sub

    Private Sub RadioButton2_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles RadioButton2.CheckedChanged
        If RadioButton2.Checked Then luz = 1 : PictureBox1.Image = My.Resources.b1
    End Sub

    Private Sub RadioButton3_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles RadioButton3.CheckedChanged
        If RadioButton3.Checked Then luz = 2 : PictureBox1.Image = My.Resources.b2
    End Sub

    Private Sub RadioButton4_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles RadioButton4.CheckedChanged
        If RadioButton4.Checked Then luzMFD = 0 : PictureBox2.Image = My.Resources.mfd1
    End Sub

    Private Sub RadioButton5_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles RadioButton5.CheckedChanged
        If RadioButton5.Checked Then luzMFD = 1 : PictureBox2.Image = My.Resources.mfd2
    End Sub

    Private Sub RadioButton6_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles RadioButton6.CheckedChanged
        If RadioButton6.Checked Then luzMFD = 2 : PictureBox2.Image = My.Resources.mfd3
    End Sub

    Private Declare Auto Sub EnviarConfiguracionX52 Lib "directinput" ()
    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click
        GuardarRegistro()
        EnviarConfiguracionX52()
    End Sub
End Class
