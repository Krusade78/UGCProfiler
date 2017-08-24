Public Class EditorSensibilidad
    Private padre As Principal
    Private Sub EditorSensibilidad_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        padre = Me.Owner
        Me.Text = Traduce.Txt("sensibility_curve")
        Me.Button1.Text = Traduce.Txt("accept")
        Me.Button2.Text = Traduce.Txt("cancel")
        Dim p, m, a, ej As Byte
        Vista.GetModos(p, m, a, ej)
        TrackBar1.Value = padre.datos.GetMapaEjes_Sensibilidad(p, m, a, ej, 0)
        TrackBar2.Value = padre.datos.GetMapaEjes_Sensibilidad(p, m, a, ej, 1)
        TrackBar3.Value = padre.datos.GetMapaEjes_Sensibilidad(p, m, a, ej, 2)
        TrackBar4.Value = padre.datos.GetMapaEjes_Sensibilidad(p, m, a, ej, 3)
        TrackBar5.Value = padre.datos.GetMapaEjes_Sensibilidad(p, m, a, ej, 4)
        TrackBar6.Value = padre.datos.GetMapaEjes_Sensibilidad(p, m, a, ej, 5)
        TrackBar7.Value = padre.datos.GetMapaEjes_Sensibilidad(p, m, a, ej, 6)
        TrackBar8.Value = padre.datos.GetMapaEjes_Sensibilidad(p, m, a, ej, 7)
        TrackBar9.Value = padre.datos.GetMapaEjes_Sensibilidad(p, m, a, ej, 8)
        TrackBar10.Value = padre.datos.GetMapaEjes_Sensibilidad(p, m, a, ej, 9)
        NumericUpDown1.Value = TrackBar1.Value
        NumericUpDown2.Value = TrackBar2.Value
        NumericUpDown3.Value = TrackBar3.Value
        NumericUpDown4.Value = TrackBar4.Value
        NumericUpDown5.Value = TrackBar5.Value
        NumericUpDown6.Value = TrackBar6.Value
        NumericUpDown7.Value = TrackBar7.Value
        NumericUpDown8.Value = TrackBar8.Value
        NumericUpDown9.Value = TrackBar9.Value
        NumericUpDown10.Value = TrackBar10.Value
    End Sub

    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click
        Dim p, m, a, ej As Byte
        Vista.GetModos(p, m, a, ej)
        padre.datos.SetMapaEjes_Sensibilidad(p, m, a, ej, 0, TrackBar1.Value)
        padre.datos.SetMapaEjes_Sensibilidad(p, m, a, ej, 1, TrackBar2.Value)
        padre.datos.SetMapaEjes_Sensibilidad(p, m, a, ej, 2, TrackBar3.Value)
        padre.datos.SetMapaEjes_Sensibilidad(p, m, a, ej, 3, TrackBar4.Value)
        padre.datos.SetMapaEjes_Sensibilidad(p, m, a, ej, 4, TrackBar5.Value)
        padre.datos.SetMapaEjes_Sensibilidad(p, m, a, ej, 5, TrackBar6.Value)
        padre.datos.SetMapaEjes_Sensibilidad(p, m, a, ej, 6, TrackBar7.Value)
        padre.datos.SetMapaEjes_Sensibilidad(p, m, a, ej, 7, TrackBar8.Value)
        padre.datos.SetMapaEjes_Sensibilidad(p, m, a, ej, 8, TrackBar9.Value)
        padre.datos.SetMapaEjes_Sensibilidad(p, m, a, ej, 9, TrackBar10.Value)
        Me.Close()
    End Sub

    Private Sub Button2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button2.Click
        Me.Close()
    End Sub

#Region "Eventos"
    Private Sub NumericUpDown1ValueChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles NumericUpDown1.ValueChanged
        TrackBar1.Value = NumericUpDown1.Value
    End Sub
    Private Sub TrackBar1_Scroll(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TrackBar1.Scroll
        NumericUpDown1.Value = TrackBar1.Value
    End Sub

    Private Sub NumericUpDown2_ValueChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles NumericUpDown2.ValueChanged
        TrackBar2.Value = NumericUpDown2.Value
    End Sub
    Private Sub TrackBar2_Scroll(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TrackBar2.Scroll
        NumericUpDown2.Value = TrackBar2.Value
    End Sub

    Private Sub NumericUpDown3_ValueChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles NumericUpDown3.ValueChanged
        TrackBar3.Value = NumericUpDown3.Value
    End Sub
    Private Sub TrackBar3_Scroll(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TrackBar3.Scroll
        NumericUpDown3.Value = TrackBar3.Value
    End Sub

    Private Sub NumericUpDown4_ValueChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles NumericUpDown4.ValueChanged
        TrackBar4.Value = NumericUpDown4.Value
    End Sub
    Private Sub TrackBar4_Scroll(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TrackBar4.Scroll
        NumericUpDown4.Value = TrackBar4.Value
    End Sub

    Private Sub NumericUpDown5_ValueChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles NumericUpDown5.ValueChanged
        TrackBar5.Value = NumericUpDown5.Value
    End Sub
    Private Sub TrackBar5_Scroll(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TrackBar5.Scroll
        NumericUpDown5.Value = TrackBar5.Value
    End Sub

    Private Sub NumericUpDown6_ValueChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles NumericUpDown6.ValueChanged
        TrackBar6.Value = NumericUpDown6.Value
    End Sub
    Private Sub TrackBar6_Scroll(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TrackBar6.Scroll
        NumericUpDown6.Value = TrackBar6.Value
    End Sub

    Private Sub NumericUpDown7_ValueChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles NumericUpDown7.ValueChanged
        TrackBar7.Value = NumericUpDown7.Value
    End Sub
    Private Sub TrackBar7_Scroll(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TrackBar7.Scroll
        NumericUpDown7.Value = TrackBar7.Value
    End Sub

    Private Sub NumericUpDown8_ValueChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles NumericUpDown8.ValueChanged
        TrackBar8.Value = NumericUpDown8.Value
    End Sub
    Private Sub TrackBar8_Scroll(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TrackBar8.Scroll
        NumericUpDown8.Value = TrackBar8.Value
    End Sub

    Private Sub NumericUpDown9_ValueChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles NumericUpDown9.ValueChanged
        TrackBar9.Value = NumericUpDown9.Value
    End Sub
    Private Sub TrackBar9_Scroll(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TrackBar9.Scroll
        NumericUpDown9.Value = TrackBar9.Value
    End Sub

    Private Sub NumericUpDown10_ValueChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles NumericUpDown10.ValueChanged
        TrackBar10.Value = NumericUpDown10.Value
    End Sub
    Private Sub TrackBar10_Scroll(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TrackBar10.Scroll
        NumericUpDown10.Value = TrackBar10.Value
    End Sub
#End Region
End Class