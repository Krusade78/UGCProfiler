Public Class EditorPOV
    Private padre As Principal

    Private Sub EditorRaton_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        Me.Text = Traduce.Txt("assign_to_pov")
        Label1.Text = Traduce.Txt("pov_text")
        Label2.Text = Traduce.Txt("directx_pov")
        Button1.Text = Traduce.Txt("accept")
        Button2.Text = Traduce.Txt("cancel")
        padre = Me.Owner
    End Sub

    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click
        Guardar()
        Me.DialogResult = Windows.Forms.DialogResult.OK
        Me.Close()
    End Sub

    Private Sub Button2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button2.Click
        Me.DialogResult = Windows.Forms.DialogResult.Cancel
        Me.Close()
    End Sub

    Private Sub Guardar()
        Dim idx As Integer
        Dim st() As String = {"pov1_n", "pov1_nw", "pov1_w", "pov1_sw", "pov1_s", "pov1_se", "pov1_e", "pov1_ne"}
        Dim p, m, a, pov As Byte
        Vista.GetModos(p, m, a, pov)
        pov = (pov - 100) \ 8
        Select Case NumericUpDown1.Value
            Case 2
                For i As Byte = 0 To 7
                    st(i) = st(i).Replace("1", "2")
                Next
            Case 3
                For i As Byte = 0 To 7
                    st(i) = st(i).Replace("1", "3")
                Next
            Case 4
                For i As Byte = 0 To 7
                    st(i) = st(i).Replace("1", "4")
                Next
        End Select
        For i As Byte = 0 To 7
            idx = padre.ComboBoxAssigned.Items.IndexOf("<DX " & Traduce.Txt(st(i)) & ">")
            If idx = -1 Then
                idx = padre.ComboBoxAssigned.Items.Add("<DX " & Traduce.Txt(st(i)) & ">")
                padre.ComboBoxMacro.Items.Add("<DX " & Traduce.Txt(st(i)) & ">")
                Dim macro As New ArrayList
                Dim comando As UInt16
                'texto x52
                comando = 24 + (3 << 8)
                macro.Add(comando)
                st(i) = Traduce.Txt(st(i))
                If st(i).Length > 16 Then st(i) = st(i).Substring(0, 16)
                For j As Byte = 0 To st(i).Length - 1
                    comando = 24 + (CInt(System.Text.ASCIIEncoding.ASCII.GetBytes(st(i))(j)) << 8)
                    macro.Add(comando)
                Next
                comando = 56
                macro.Add(comando)
                'resto
                comando = 19 + ((((NumericUpDown1.Value - 1) * 8) + i) << 8)
                macro.Add(comando)
                comando = 11
                macro.Add(comando)
                comando = 51 + ((((NumericUpDown1.Value - 1) * 8) + i) << 8)
                macro.Add(comando)
                padre.datos.AñadirIndice(idx - 1, macro)
            End If
            padre.datos.SetMapaSetas_Estado(p, m, a, (pov * 8) + i, 0)
            padre.datos.SetMapaSetas_Indices(p, m, a, (pov * 8) + i, 0, idx)
            padre.datos.SetMapaSetas_Indices(p, m, a, (pov * 8) + i, 1, 0)
        Next
    End Sub
End Class