Public Class EditorModos

    Private padre As Principal

    Private Sub EditorModos_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        padre = Me.Owner
#If LITE = 1 Then
        CheckBox3.Enabled = False
#End If
        If ((padre.datos.GetModosDefecto() >> 5) And 1) = 1 Then CheckBox1.Checked = True
        If ((padre.datos.GetModosDefecto() >> 5) And 2) = 2 Then CheckBox2.Checked = True
        If ((padre.datos.GetModosDefecto() >> 5) And 4) = 4 Then CheckBox3.Checked = True
        Traducir()
    End Sub

    Private Sub Traducir()
        Me.Text = Traduce.Txt("modes_configuration")
        CheckBox1.Text = Traduce.Txt("pinkie_activate")
        Label1.Text = Traduce.Txt("pinkie_name")
        If padre.ToolStripSplitPinkie.DropDownItems.Count = 2 Then
            TextBox1.Text = padre.ToolStripSplitPinkie.Text.Substring(0, padre.ToolStripSplitPinkie.Text.Length - 3)
        Else
            TextBox1.Text = Traduce.Txt("pinkie_default")
        End If
        GroupBox2.Text = Traduce.Txt("modes")
        CheckBox2.Text = Traduce.Txt("modes_activate")
        Label2.Text = Traduce.Txt("mode1_name")
        If padre.ToolStripSplitModo.DropDownItems.Count = 3 Then
            TextBox2.Text = padre.ToolStripSplitModo.DropDownItems.Item(0).Text
        Else
            TextBox2.Text = Traduce.Txt("mode1_default")
        End If
        Label3.Text = Traduce.Txt("mode2_name")
        If padre.ToolStripSplitModo.DropDownItems.Count = 3 Then
            TextBox3.Text = padre.ToolStripSplitModo.DropDownItems.Item(1).Text
        Else
            TextBox3.Text = Traduce.Txt("mode2_default")
        End If
        Label4.Text = Traduce.Txt("mode3_name")
        If padre.ToolStripSplitModo.DropDownItems.Count = 3 Then
            TextBox4.Text = padre.ToolStripSplitModo.DropDownItems.Item(2).Text
        Else
            TextBox4.Text = Traduce.Txt("mode3_default")
        End If
        GroupBox3.Text = Traduce.Txt("aux_modes")
        CheckBox3.Text = Traduce.Txt("aux_activate")
        Label5.Text = Traduce.Txt("aux1_name")
        If padre.ToolStripSplitAux.DropDownItems.Count = 3 Then
            TextBox5.Text = padre.ToolStripSplitAux.DropDownItems.Item(0).Text
        Else
            TextBox5.Text = Traduce.Txt("aux1_default")
        End If
        Label6.Text = Traduce.Txt("aux2_name")
        If padre.ToolStripSplitAux.DropDownItems.Count = 3 Then
            TextBox6.Text = padre.ToolStripSplitAux.DropDownItems.Item(1).Text
        Else
            TextBox6.Text = Traduce.Txt("aux2_default")
        End If
        Label7.Text = Traduce.Txt("aux3_name")
        If padre.ToolStripSplitAux.DropDownItems.Count = 3 Then
            TextBox7.Text = padre.ToolStripSplitAux.DropDownItems.Item(2).Text
        Else
            TextBox7.Text = Traduce.Txt("aux3_default")
        End If
        Me.ButtonC.Text = Traduce.Txt("cancel")
        Me.ButtonG.Text = Traduce.Txt("accept")
    End Sub

    Private Sub CheckBox1_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CheckBox1.CheckedChanged
        If CheckBox1.Checked Then
            Label1.Enabled = True
            TextBox1.Enabled = True
        Else
            Label1.Enabled = False
            TextBox1.Enabled = False
        End If
    End Sub

    Private Sub CheckBox2_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CheckBox2.CheckedChanged
        If CheckBox2.Checked Then
            Label2.Enabled = True
            TextBox2.Enabled = True
            Label3.Enabled = True
            TextBox3.Enabled = True
            Label4.Enabled = True
            TextBox4.Enabled = True
        Else
            Label2.Enabled = False
            TextBox2.Enabled = False
            Label3.Enabled = False
            TextBox3.Enabled = False
            Label4.Enabled = False
            TextBox4.Enabled = False
        End If
    End Sub

    Private Sub CheckBox3_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CheckBox3.CheckedChanged
        If CheckBox3.Checked Then
            Label5.Enabled = True
            TextBox5.Enabled = True
            Label6.Enabled = True
            TextBox6.Enabled = True
            Label7.Enabled = True
            TextBox7.Enabled = True
        Else
            Label5.Enabled = False
            TextBox5.Enabled = False
            Label6.Enabled = False
            TextBox6.Enabled = False
            Label7.Enabled = False
            TextBox7.Enabled = False
        End If
    End Sub

    Private Sub ButtonAceptar_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonG.Click
        Dim b As Byte = padre.datos.GetModosDefecto()
        b = b And 3
        If CheckBox1.Checked Then
            b = b Or 32
            If TextBox1.Text.Trim() = "" Then TextBox1.Text = Traduce.Txt("pinkie_default")
            If TextBox1.Text.Trim() <> TextBox2.Text.Trim() And TextBox1.Text.Trim() <> TextBox3.Text.Trim _
            And TextBox1.Text.Trim() <> TextBox4.Text.Trim() And TextBox1.Text.Trim() <> TextBox5.Text.Trim _
            And TextBox1.Text.Trim() <> TextBox6.Text.Trim() And TextBox1.Text.Trim() <> TextBox7.Text.Trim Then
                padre.datos.SetNombreModo(0, System.Text.ASCIIEncoding.ASCII.GetBytes(TextBox1.Text.Trim()))
            Else
                TextBox1.SelectAll()
                Exit Sub
            End If
        End If
        If CheckBox2.Checked Then
            b = b Or 64
            If TextBox2.Text.Trim() = "" Then TextBox1.Text = Traduce.Txt("mode1_default")
            If TextBox1.Text.Trim() <> TextBox2.Text.Trim() And TextBox2.Text.Trim() <> TextBox3.Text.Trim _
            And TextBox2.Text.Trim() <> TextBox4.Text.Trim() And TextBox2.Text.Trim() <> TextBox5.Text.Trim _
            And TextBox2.Text.Trim() <> TextBox6.Text.Trim() And TextBox2.Text.Trim() <> TextBox7.Text.Trim Then
                padre.datos.SetNombreModo(1, System.Text.ASCIIEncoding.ASCII.GetBytes(TextBox2.Text.Trim()))
            Else
                TextBox2.SelectAll()
                Exit Sub
            End If
            If TextBox3.Text.Trim() = "" Then TextBox2.Text = Traduce.Txt("mode2_default")
            If TextBox3.Text.Trim() <> TextBox2.Text.Trim() And TextBox1.Text.Trim() <> TextBox3.Text.Trim _
            And TextBox3.Text.Trim() <> TextBox4.Text.Trim() And TextBox3.Text.Trim() <> TextBox5.Text.Trim _
            And TextBox3.Text.Trim() <> TextBox6.Text.Trim() And TextBox3.Text.Trim() <> TextBox7.Text.Trim Then
                padre.datos.SetNombreModo(2, System.Text.ASCIIEncoding.ASCII.GetBytes(TextBox3.Text.Trim()))
            Else
                TextBox3.SelectAll()
                Exit Sub
            End If
            If TextBox4.Text.Trim() = "" Then TextBox3.Text = Traduce.Txt("mode3_default")
            If TextBox4.Text.Trim() <> TextBox2.Text.Trim() And TextBox4.Text.Trim() <> TextBox3.Text.Trim _
            And TextBox1.Text.Trim() <> TextBox4.Text.Trim() And TextBox4.Text.Trim() <> TextBox5.Text.Trim _
            And TextBox4.Text.Trim() <> TextBox6.Text.Trim() And TextBox4.Text.Trim() <> TextBox7.Text.Trim Then
                padre.datos.SetNombreModo(3, System.Text.ASCIIEncoding.ASCII.GetBytes(TextBox4.Text.Trim()))
            Else
                TextBox4.SelectAll()
                Exit Sub
            End If
        End If
        If CheckBox3.Checked Then
            b = b Or 128
            If TextBox5.Text.Trim() = "" Then TextBox1.Text = Traduce.Txt("aux1_default")
            If TextBox5.Text.Trim() <> TextBox2.Text.Trim() And TextBox5.Text.Trim() <> TextBox3.Text.Trim _
            And TextBox5.Text.Trim() <> TextBox4.Text.Trim() And TextBox1.Text.Trim() <> TextBox5.Text.Trim _
            And TextBox5.Text.Trim() <> TextBox6.Text.Trim() And TextBox5.Text.Trim() <> TextBox7.Text.Trim Then
                padre.datos.SetNombreModo(4, System.Text.ASCIIEncoding.ASCII.GetBytes(TextBox5.Text.Trim()))
            Else
                TextBox5.SelectAll()
                Exit Sub
            End If
            If TextBox6.Text.Trim() = "" Then TextBox1.Text = Traduce.Txt("aux2_default")
            If TextBox6.Text.Trim() <> TextBox2.Text.Trim() And TextBox6.Text.Trim() <> TextBox3.Text.Trim _
            And TextBox6.Text.Trim() <> TextBox4.Text.Trim() And TextBox6.Text.Trim() <> TextBox5.Text.Trim _
            And TextBox1.Text.Trim() <> TextBox6.Text.Trim() And TextBox6.Text.Trim() <> TextBox7.Text.Trim Then
                padre.datos.SetNombreModo(5, System.Text.ASCIIEncoding.ASCII.GetBytes(TextBox6.Text.Trim()))
            Else
                TextBox6.SelectAll()
                Exit Sub
            End If
            If TextBox7.Text.Trim() = "" Then TextBox1.Text = Traduce.Txt("aux3_default")
            If TextBox7.Text.Trim() <> TextBox2.Text.Trim() And TextBox7.Text.Trim() <> TextBox3.Text.Trim _
            And TextBox7.Text.Trim() <> TextBox4.Text.Trim() And TextBox7.Text.Trim() <> TextBox5.Text.Trim _
            And TextBox7.Text.Trim() <> TextBox6.Text.Trim() And TextBox1.Text.Trim() <> TextBox7.Text.Trim Then
                padre.datos.SetNombreModo(6, System.Text.ASCIIEncoding.ASCII.GetBytes(TextBox7.Text.Trim()))
            Else
                TextBox7.SelectAll()
                Exit Sub
            End If
        End If
        padre.datos.SetModosDefecto(b)
        Me.Close()
    End Sub

    Private Sub ButtonCancelar_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonC.Click
        Me.Close()
    End Sub
End Class