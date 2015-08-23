Public Class EditorBandas

    Private padre As Principal
    Private modop As Byte
    Private modom As Byte
    Private modoa As Byte
    Private eje As Byte
    Private modificando As Byte = False

    Public Sub New(ByVal p As Principal)
        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        padre = p
        Vista.GetModos(modop, modom, modoa, eje)
    End Sub

    Private Sub Form1_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        Cargar()
        Traducir()
        Cambiar(0)
    End Sub

    Private Sub ButtonGuardar_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonG.Click
        Guardar()
    End Sub

    Private Sub Traducir()
        Me.Text = Traduce.Txt("edit_bands")
        Label1.Text = Traduce.Txt("bands")
        ButtonG.Text = Traduce.Txt("accept")
        ButtonC.Text = Traduce.Txt("cancel")
    End Sub

    Private Sub Cargar()
        modificando = True
        NumericUpDown1.Value = padre.datos.GetMapaEjes_Bandas(modop, modom, modoa, eje, 0)
        NumericUpDown2.Value = padre.datos.GetMapaEjes_Bandas(modop, modom, modoa, eje, 1)
        NumericUpDown3.Value = padre.datos.GetMapaEjes_Bandas(modop, modom, modoa, eje, 2)
        NumericUpDown4.Value = padre.datos.GetMapaEjes_Bandas(modop, modom, modoa, eje, 3)
        NumericUpDown5.Value = padre.datos.GetMapaEjes_Bandas(modop, modom, modoa, eje, 4)
        NumericUpDown6.Value = padre.datos.GetMapaEjes_Bandas(modop, modom, modoa, eje, 5)
        NumericUpDown7.Value = padre.datos.GetMapaEjes_Bandas(modop, modom, modoa, eje, 6)
        NumericUpDown8.Value = padre.datos.GetMapaEjes_Bandas(modop, modom, modoa, eje, 7)
        NumericUpDown9.Value = padre.datos.GetMapaEjes_Bandas(modop, modom, modoa, eje, 8)
        NumericUpDown10.Value = padre.datos.GetMapaEjes_Bandas(modop, modom, modoa, eje, 9)
        NumericUpDown11.Value = padre.datos.GetMapaEjes_Bandas(modop, modom, modoa, eje, 10)
        NumericUpDown12.Value = padre.datos.GetMapaEjes_Bandas(modop, modom, modoa, eje, 11)
        NumericUpDown13.Value = padre.datos.GetMapaEjes_Bandas(modop, modom, modoa, eje, 12)
        NumericUpDown14.Value = padre.datos.GetMapaEjes_Bandas(modop, modom, modoa, eje, 13)
        NumericUpDown15.Value = padre.datos.GetMapaEjes_Bandas(modop, modom, modoa, eje, 14)
        modificando = False
    End Sub
    Private Sub Guardar()
        Dim num() As NumericUpDown = {NumericUpDown1, NumericUpDown2, NumericUpDown3, NumericUpDown4, NumericUpDown5, NumericUpDown6, NumericUpDown7, NumericUpDown8, NumericUpDown9, NumericUpDown10, NumericUpDown11, NumericUpDown12, NumericUpDown13, NumericUpDown14, NumericUpDown15}
        For i As Byte = 0 To 14
            padre.datos.SetMapaEjes_Bandas(modop, modom, modoa, eje, i, num(i).Value)
        Next
        Me.Close()
    End Sub

#Region "Eventos up down"
    Private Sub NumericUpDown1_ValueChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles NumericUpDown1.ValueChanged
        If Not modificando Then Cambiar(0)
    End Sub
    Private Sub NumericUpDown2_ValueChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles NumericUpDown2.ValueChanged
        If Not modificando Then Cambiar(1)
    End Sub
    Private Sub NumericUpDown3_ValueChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles NumericUpDown3.ValueChanged
        If Not modificando Then Cambiar(2)
    End Sub
    Private Sub NumericUpDown4_ValueChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles NumericUpDown4.ValueChanged
        If Not modificando Then Cambiar(3)
    End Sub
    Private Sub NumericUpDown5_ValueChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles NumericUpDown5.ValueChanged
        If Not modificando Then Cambiar(4)
    End Sub
    Private Sub NumericUpDown6_ValueChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles NumericUpDown6.ValueChanged
        If Not modificando Then Cambiar(5)
    End Sub
    Private Sub NumericUpDown7_ValueChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles NumericUpDown7.ValueChanged
        If Not modificando Then Cambiar(6)
    End Sub
    Private Sub NumericUpDown8_ValueChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles NumericUpDown8.ValueChanged
        If Not modificando Then Cambiar(7)
    End Sub
    Private Sub NumericUpDown9_ValueChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles NumericUpDown9.ValueChanged
        If Not modificando Then Cambiar(8)
    End Sub
    Private Sub NumericUpDown10_ValueChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles NumericUpDown10.ValueChanged
        If Not modificando Then Cambiar(9)
    End Sub
    Private Sub NumericUpDown11_ValueChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles NumericUpDown11.ValueChanged
        If Not modificando Then Cambiar(10)
    End Sub
    Private Sub NumericUpDown12_ValueChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles NumericUpDown12.ValueChanged
        If Not modificando Then Cambiar(11)
    End Sub
    Private Sub NumericUpDown13_ValueChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles NumericUpDown13.ValueChanged
        If Not modificando Then Cambiar(12)
    End Sub
    Private Sub NumericUpDown14_ValueChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles NumericUpDown14.ValueChanged
        If Not modificando Then Cambiar(13)
    End Sub
    Private Sub NumericUpDown15_ValueChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles NumericUpDown15.ValueChanged
        If Not modificando Then Cambiar(14)
    End Sub
#End Region

    Private Sub Cambiar(ByVal n As Byte)
        Dim num() As NumericUpDown = {NumericUpDown1, NumericUpDown2, NumericUpDown3, NumericUpDown4, NumericUpDown5, NumericUpDown6, NumericUpDown7, NumericUpDown8, NumericUpDown9, NumericUpDown10, NumericUpDown11, NumericUpDown12, NumericUpDown13, NumericUpDown14, NumericUpDown15}
        Dim split() As PictureBox = {PictureBox1, PictureBox2, PictureBox3, PictureBox4, PictureBox5, PictureBox6, PictureBox7, PictureBox8, PictureBox9, PictureBox10, PictureBox11, PictureBox12, PictureBox13, PictureBox14, PictureBox15}
        Dim label() As Button = {Button1, Button2, Button3, Button4, Button5, Button6, Button7, Button8, Button9, Button10, Button11, Button12, Button13, Button14, Button15}
        modificando = True
        'For i As Byte = n To 14
        '    split(i).Visible = False
        'Next
        'Panel1.Visible = False
        For i As Byte = n To 14
            If num(i).Value = 0 Then
                If i = 0 Then
                    split(i).Height = 512
                Else
                    split(i).Height = 512 - ((512 * num(i - 1).Value) / 100) '- i - 1
                End If
                For j As Byte = i + 1 To 14
                    num(j).Value = 0
                    num(j).Enabled = False
                    split(j).Height = 0
                    label(j).Enabled = False
                Next
                Exit For
            ElseIf num(i).Value = 100 Then
                If i = 0 Then
                    split(i).Height = (512 * num(i).Value) / 100
                Else
                    split(i).Height = (512 * (num(i).Value - num(i - 1).Value)) / 100
                End If
                For j As Byte = i + 1 To 14
                    num(j).Value = 0
                    num(j).Enabled = False
                    label(j).Enabled = False
                    split(j).Height = 0
                Next
                If i < 14 Then split(i + 1).Height = 2
                Exit For
            Else
                If i < 14 Then
                    num(i + 1).Enabled = True
                    label(i + 1).Enabled = True
                End If
                If i > 0 Then
                    If num(i).Value <= num(i - 1).Value Then
                        num(i).Value = num(i - 1).Value + 1
                        If num(i).Value = 100 And i < 14 Then
                            num(i + 1).Value = 0
                            num(i + 1).Enabled = False
                        End If
                    End If
                    split(i).Height = (512 * (num(i).Value - num(i - 1).Value)) / 100
                Else
                    split(i).Height = (512 * num(i).Value) / 100
                End If
            End If
        Next
        'For i As Byte = n To 14
        '    split(i).Visible = True
        'Next
        'Panel1.Visible = True
        modificando = False
    End Sub

#Region "Eventos reset"
    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click
        NumericUpDown1.Value = 0
    End Sub
    Private Sub Button2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button2.Click
        NumericUpDown2.Value = 0
    End Sub
    Private Sub Button3_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button3.Click
        NumericUpDown3.Value = 0
    End Sub
    Private Sub Button4_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button4.Click
        NumericUpDown4.Value = 0
    End Sub
    Private Sub Button5_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button5.Click
        NumericUpDown5.Value = 0
    End Sub
    Private Sub Button6_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button6.Click
        NumericUpDown6.Value = 0
    End Sub
    Private Sub Button7_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button7.Click
        NumericUpDown7.Value = 0
    End Sub
    Private Sub Button8_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button8.Click
        NumericUpDown8.Value = 0
    End Sub
    Private Sub Button9_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button9.Click
        NumericUpDown9.Value = 0
    End Sub
    Private Sub Button10_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button10.Click
        NumericUpDown10.Value = 0
    End Sub
    Private Sub Button11_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button11.Click
        NumericUpDown11.Value = 0
    End Sub
    Private Sub Button12_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button12.Click
        NumericUpDown12.Value = 0
    End Sub
    Private Sub Button13_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button13.Click
        NumericUpDown13.Value = 0
    End Sub
    Private Sub Button14_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button14.Click
        NumericUpDown14.Value = 0
    End Sub
    Private Sub Button15_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button15.Click
        NumericUpDown15.Value = 0
    End Sub
#End Region
End Class