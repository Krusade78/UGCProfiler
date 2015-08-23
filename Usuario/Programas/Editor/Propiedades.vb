Module Vista

    Private eventos As Boolean = False
    Private padre As Principal
    Private actual As Byte = 255
    Private togglePos As Byte = 1
#Region "Variables principales"
    Public Sub SetPadre(ByRef p As Principal)
        padre = p
    End Sub
    Public Function GetEventos() As Boolean
        Return eventos
    End Function
#End Region
#Region "Modos"
    Public Sub GetModos(ByRef p As Byte, ByRef m As Byte, ByRef a As Byte)
        p = padre.ToolStripSplitPinkie.Tag
        m = padre.ToolStripSplitModo.Tag
        a = padre.ToolStripSplitAux.Tag
    End Sub
    Public Sub GetModos(ByRef p As Byte, ByRef m As Byte, ByRef a As Byte, ByRef e As Byte)
        p = padre.ToolStripSplitPinkie.Tag
        m = padre.ToolStripSplitModo.Tag
        a = padre.ToolStripSplitAux.Tag
        If (actual > 63) And (actual < 100) Then e = actual - 64 Else e = actual
    End Sub

    Private Sub CargarModos()
        padre.ToolStripSplitPinkie.DropDownItems.Clear()
        padre.ToolStripSplitModo.DropDownItems.Clear()
        padre.ToolStripSplitAux.DropDownItems.Clear()
        padre.ToolStripSplitPinkie.Tag = 0
        padre.ToolStripSplitModo.Tag = 0
        padre.ToolStripSplitAux.Tag = 0

        Dim buf(15) As Byte
        If ((padre.datos.GetModosDefecto() >> 5) And 1) = 1 Then
            padre.datos.GetNombreModo(0, buf)
            If System.Text.ASCIIEncoding.ASCII.GetString(buf).IndexOf(Chr(0)) > -1 Then
                padre.ToolStripSplitPinkie.DropDownItems.Add(System.Text.ASCIIEncoding.ASCII.GetString(buf).Substring(0, System.Text.ASCIIEncoding.ASCII.GetString(buf).IndexOf(Chr(0))) & " off")
                padre.ToolStripSplitPinkie.DropDownItems.Add(System.Text.ASCIIEncoding.ASCII.GetString(buf).Substring(0, System.Text.ASCIIEncoding.ASCII.GetString(buf).IndexOf(Chr(0))) & " on ")
            Else
                padre.ToolStripSplitPinkie.DropDownItems.Add(System.Text.ASCIIEncoding.ASCII.GetString(buf) & " off")
                padre.ToolStripSplitPinkie.DropDownItems.Add(System.Text.ASCIIEncoding.ASCII.GetString(buf) & " on ")
            End If
            padre.ToolStripSplitPinkie.Text = padre.ToolStripSplitPinkie.DropDownItems.Item(0).Text
        Else
            padre.ToolStripSplitPinkie.Text = ""
        End If
        If ((padre.datos.GetModosDefecto() >> 5) And 2) = 2 Then
            padre.datos.GetNombreModo(1, buf)
            If System.Text.ASCIIEncoding.ASCII.GetString(buf).IndexOf(Chr(0)) > -1 Then
                padre.ToolStripSplitModo.DropDownItems.Add(System.Text.ASCIIEncoding.ASCII.GetString(buf).Substring(0, System.Text.ASCIIEncoding.ASCII.GetString(buf).IndexOf(Chr(0))))
            Else
                padre.ToolStripSplitModo.DropDownItems.Add(System.Text.ASCIIEncoding.ASCII.GetString(buf))
            End If
            padre.datos.GetNombreModo(2, buf)
            If System.Text.ASCIIEncoding.ASCII.GetString(buf).IndexOf(Chr(0)) > -1 Then
                padre.ToolStripSplitModo.DropDownItems.Add(System.Text.ASCIIEncoding.ASCII.GetString(buf).Substring(0, System.Text.ASCIIEncoding.ASCII.GetString(buf).IndexOf(Chr(0))))
            Else
                padre.ToolStripSplitModo.DropDownItems.Add(System.Text.ASCIIEncoding.ASCII.GetString(buf))
            End If
            padre.datos.GetNombreModo(3, buf)
            If System.Text.ASCIIEncoding.ASCII.GetString(buf).IndexOf(Chr(0)) > -1 Then
                padre.ToolStripSplitModo.DropDownItems.Add(System.Text.ASCIIEncoding.ASCII.GetString(buf).Substring(0, System.Text.ASCIIEncoding.ASCII.GetString(buf).IndexOf(Chr(0))))
            Else
                padre.ToolStripSplitModo.DropDownItems.Add(System.Text.ASCIIEncoding.ASCII.GetString(buf))
            End If
            padre.ToolStripSplitModo.Text = padre.ToolStripSplitModo.DropDownItems.Item(0).Text
        Else
            padre.ToolStripSplitModo.Text = ""
        End If
        If ((padre.datos.GetModosDefecto() >> 5) And 4) = 4 Then
            padre.datos.GetNombreModo(4, buf)
            If System.Text.ASCIIEncoding.ASCII.GetString(buf).IndexOf(Chr(0)) > -1 Then
                padre.ToolStripSplitAux.DropDownItems.Add(System.Text.ASCIIEncoding.ASCII.GetString(buf).Substring(0, System.Text.ASCIIEncoding.ASCII.GetString(buf).IndexOf(Chr(0))))
            Else
                padre.ToolStripSplitAux.DropDownItems.Add(System.Text.ASCIIEncoding.ASCII.GetString(buf))
            End If
            padre.datos.GetNombreModo(5, buf)
            If System.Text.ASCIIEncoding.ASCII.GetString(buf).IndexOf(Chr(0)) > -1 Then
                padre.ToolStripSplitAux.DropDownItems.Add(System.Text.ASCIIEncoding.ASCII.GetString(buf).Substring(0, System.Text.ASCIIEncoding.ASCII.GetString(buf).IndexOf(Chr(0))))
            Else
                padre.ToolStripSplitAux.DropDownItems.Add(System.Text.ASCIIEncoding.ASCII.GetString(buf))
            End If
            padre.datos.GetNombreModo(6, buf)
            If System.Text.ASCIIEncoding.ASCII.GetString(buf).IndexOf(Chr(0)) > -1 Then
                padre.ToolStripSplitAux.DropDownItems.Add(System.Text.ASCIIEncoding.ASCII.GetString(buf).Substring(0, System.Text.ASCIIEncoding.ASCII.GetString(buf).IndexOf(Chr(0))))
            Else
                padre.ToolStripSplitAux.DropDownItems.Add(System.Text.ASCIIEncoding.ASCII.GetString(buf))
            End If
            padre.ToolStripSplitAux.Text = padre.ToolStripSplitAux.DropDownItems.Item(0).Text
        Else
            padre.ToolStripSplitAux.Text = ""
        End If

        If (padre.datos.GetModosDefecto() >> 5) = 0 Then
            padre.ToolStripSplitPinkie.Text = Traduce.Txt("no_modes")
        End If
    End Sub
#End Region
#Region "Cargar inicial"
    Public Sub Clear(ByVal abriendo As Boolean)
        padre.ToolStripSplitPinkie.DropDownItems.Clear()
        padre.ToolStripSplitModo.DropDownItems.Clear()
        padre.ToolStripSplitAux.DropDownItems.Clear()
        padre.ToolStripSplitPinkie.Text = Traduce.Txt("no_modes")
        padre.ToolStripSplitModo.Text = ""
        padre.ToolStripSplitAux.Text = ""
        padre.Label2.Text = ""
        actual = 255
        If abriendo Then CargarModos()
    End Sub
    Public Sub RefrescarModos()
        CargarModos()
        If actual < 255 Then Ver(actual)
    End Sub
#End Region

#Region "Cambiar vista"
    Public Sub Refrescar()
        If actual < 255 Then Ver(actual)
    End Sub
    Public Sub Ver(ByVal id As Byte, Optional ByVal titulo As String = "")
        If titulo <> "" Then padre.Label2.Text = titulo
        If actual <> id Then togglePos = 1
        actual = id
        'padre.Propiedades.Visible = False
        If id > 99 Then
            Boton(id - 100, True)
        ElseIf id < 64 Then
            Boton(id)
        ElseIf id < 71 Then
            Eje(id - 64)
        Else
            MiniStick(id - 64)
        End If
        'padre.Propiedades.Visible = True
    End Sub
    Private Sub Boton(ByVal b As Byte, Optional ByVal seta As Boolean = False)
        eventos = False
        'reset
        If Not padre.RadioButton1.Checked Then padre.RadioButton1.Checked = True
        padre.NumericUpDownPosition.Maximum = 15
        padre.NumericUpDownPosition.Value = 1
        '/--------

        Dim p, m, a, st As Byte
        GetModos(p, m, a)
        If seta Then
            st = padre.datos.GetMapaSetas_Estado(p, m, a, b)
        Else
            st = padre.datos.GetMapaBotones_Estado(p, m, a, b)
        End If

        If st > 0 Then
            padre.RadioButtonToggle.Checked = True
            padre.LabelPositions.Visible = True
            padre.NumericUpDownPositions.Visible = True
            padre.NumericUpDownPositions.Value = st
            padre.ButtonAssignPOV.Visible = False
            padre.ButtonAssignPinkie.Visible = False
            padre.ButtonAssignModes.Visible = False
            'macros
            padre.RadioButton1.Visible = False
            padre.RadioButton2.Visible = False
            padre.LabelPosition.Visible = True
            padre.NumericUpDownPosition.Visible = True
            'padre.LabelPosition.Enabled = True
            'padre.NumericUpDownPosition.Enabled = True
        Else
            padre.RadioButtonUpDown.Checked = True
            padre.LabelPositions.Visible = False
            padre.NumericUpDownPositions.Visible = False
            If seta Then
                padre.ButtonAssignPOV.Visible = True
                padre.ButtonAssignPinkie.Visible = False
                padre.ButtonAssignModes.Visible = False
            Else
                padre.ButtonAssignPOV.Visible = False
                If b = 6 Then
                    padre.ButtonAssignModes.Visible = False
                    If padre.ToolStripSplitPinkie.DropDownItems.Count = 2 Then padre.ButtonAssignPinkie.Visible = True Else padre.ButtonAssignPinkie.Visible = False
                Else
                    padre.ButtonAssignPinkie.Visible = False
                    If b > 7 And b < 11 Then
                        If padre.ToolStripSplitModo.DropDownItems.Count = 3 Then padre.ButtonAssignModes.Visible = True Else padre.ButtonAssignModes.Visible = False
                    Else
                        padre.ButtonAssignModes.Visible = False
                    End If
                End If
            End If
            'macros
            padre.LabelPosition.Visible = False
            padre.NumericUpDownPosition.Visible = False
            padre.RadioButton1.Visible = True
            padre.RadioButton1.Text = Traduce.Txt("pressed")
#If LITE = 1 Then
            padre.RadioButton2.Visible = False
#Else
            padre.RadioButton2.Visible = True
#End If
            padre.RadioButton2.Text = Traduce.Txt("raised")
            padre.RadioButton1.Enabled = True
            padre.RadioButton2.Enabled = True
        End If

        padre.PanelButton.Enabled = True
        padre.PanelMapaEjes.Enabled = False
        padre.ComboBoxAssigned.Enabled = True

        eventos = True
        CargarIndice()
    End Sub
    Private Sub Eje(ByVal e As Byte)
        eventos = False
        ResetEje()

        Dim p, m, a As Byte
        GetModos(p, m, a)
        Dim neje As Byte = padre.datos.GetMapaEjes_nEje(p, m, a, e)
        If neje > 19 Then padre.CheckBoxInverted.Checked = True : neje = neje - 20 Else padre.CheckBoxInverted.Checked = False

        padre.ComboBoxAxes.SelectedIndex = neje
        If e < 4 Then padre.ButtonSensibility.Enabled = True Else padre.ButtonSensibility.Enabled = False

        If padre.ComboBoxAxes.SelectedIndex > 9 Then
            padre.LabelMSensibility.Enabled = True
            padre.NumericUpDownMSensibility.Enabled = True
            padre.NumericUpDownMSensibility.Value = padre.datos.GetMapaEjes_MSensibilidad(p, m, a, e)
        Else
            padre.LabelMSensibility.Enabled = False
            padre.NumericUpDownMSensibility.Enabled = False
        End If

        padre.LabelDigital.Enabled = True
        padre.RadioButtonIncremental.Enabled = True
        padre.RadioButtonBands.Enabled = True

        padre.RadioButtonBands.Enabled = True
        padre.RadioButtonIncremental.Enabled = True
        If padre.datos.GetMapaEjes_Incremental(p, m, a, e) Then
            padre.RadioButtonIncremental.Checked = True
            padre.ButtonEditBands.Visible = False
            padre.LabelResistanceInc.Visible = True
            padre.LabelResistanceDec.Visible = True
            padre.NumericUpDownResistanceInc.Visible = True
            padre.NumericUpDownResistanceDec.Visible = True
            padre.NumericUpDownResistanceInc.Value = padre.datos.GetMapaEjes_SensibilidadInc(p, m, a, e)
            padre.NumericUpDownResistanceDec.Value = padre.datos.GetMapaEjes_SensibilidadDec(p, m, a, e)
            'macros
            padre.LabelPosition.Visible = False
            padre.NumericUpDownPosition.Visible = False
            padre.RadioButton1.Visible = True
            padre.RadioButton1.Text = Traduce.Txt("increment")
            padre.RadioButton2.Visible = True
            padre.RadioButton2.Text = Traduce.Txt("decrement")
            padre.RadioButton1.Enabled = True
            padre.RadioButton2.Enabled = True
        Else
            padre.RadioButtonBands.Checked = True
            padre.ButtonEditBands.Enabled = True
            'macros
            padre.RadioButton1.Visible = False
            padre.RadioButton2.Visible = False
            padre.LabelPosition.Visible = True
            padre.NumericUpDownPosition.Visible = True
            'padre.LabelPosition.Enabled = True
            'padre.NumericUpDownPosition.Enabled = True
        End If

        padre.PanelMapaEjes.Enabled = True
        padre.PanelButton.Enabled = False
        padre.ComboBoxAssigned.Enabled = True

        eventos = True
        CargarIndice()
    End Sub
    Private Sub MiniStick(ByVal id As Byte)
        eventos = False
        ResetEje()

        Dim p, m, a As Byte
        GetModos(p, m, a)
        Dim neje As Byte = padre.datos.GetMapaEjes_nEje(p, m, a, id)
        If neje > 19 Then padre.CheckBoxInverted.Checked = True : neje = neje - 20 Else padre.CheckBoxInverted.Checked = False

        padre.ComboBoxAxes.SelectedIndex = neje
        If padre.ComboBoxAxes.SelectedIndex > 9 Then
            padre.LabelMSensibility.Enabled = True
            padre.NumericUpDownMSensibility.Enabled = True
            padre.NumericUpDownMSensibility.Value = padre.datos.GetMapaEjes_MSensibilidad(p, m, a, id)
        Else
            padre.LabelMSensibility.Enabled = False
            padre.NumericUpDownMSensibility.Enabled = False
        End If

        padre.LabelDigital.Enabled = False
        padre.ButtonSensibility.Enabled = False
        padre.RadioButtonIncremental.Enabled = False
        padre.RadioButtonBands.Enabled = False


        padre.LabelPosition.Visible = False
        padre.NumericUpDownPosition.Visible = False
        padre.RadioButton1.Visible = True
        padre.RadioButton2.Visible = True
        padre.RadioButton1.Enabled = False
        padre.RadioButton2.Enabled = False
        padre.ComboBoxAssigned.Text = "</------- " & Traduce.Txt("none") & " -------/>"
        padre.ComboBoxAssigned.Enabled = False

        padre.PanelMapaEjes.Enabled = True
        padre.PanelButton.Enabled = False

        eventos = True
    End Sub

    Private Sub ResetEje()
        padre.NumericUpDownPosition.Maximum = 16
        ' Ejes
        If padre.LabelResistanceInc.Visible Then padre.LabelResistanceInc.Visible = False
        If padre.LabelResistanceDec.Visible Then padre.LabelResistanceDec.Visible = False
        If padre.NumericUpDownResistanceInc.Visible Then padre.NumericUpDownResistanceInc.Visible = False
        If padre.NumericUpDownResistanceDec.Visible Then padre.NumericUpDownResistanceDec.Visible = False
        If Not padre.ButtonEditBands.Visible Then padre.ButtonEditBands.Visible = True
        If padre.RadioButtonIncremental.Checked Then padre.RadioButtonIncremental.Checked = False
        If padre.RadioButtonBands.Checked Then padre.RadioButtonBands.Checked = False
        If padre.ButtonEditBands.Enabled Then padre.ButtonEditBands.Enabled = False
        ' Macros
        If Not padre.RadioButton1.Checked Then padre.RadioButton1.Checked = True
        padre.NumericUpDownPosition.Value = 1
    End Sub

#End Region

#Region "Cargar/Guardar"
    Public Sub CambiarIndice(ByVal idx As Integer)
        Dim p, m, a As Byte
        GetModos(p, m, a)
        If actual > 99 Then
            If padre.RadioButton1.Visible = True Then
                If padre.RadioButton1.Checked Then padre.datos.SetMapaSetas_Indices(p, m, a, actual - 100, 0, idx) Else padre.datos.SetMapaSetas_Indices(p, m, a, actual - 100, 1, idx)
            Else
                padre.datos.SetMapaSetas_Indices(p, m, a, actual - 100, padre.NumericUpDownPosition.Value - 1, idx)
            End If
        ElseIf actual < 64 Then
            If padre.RadioButton1.Visible = True Then
                If padre.RadioButton1.Checked Then padre.datos.SetMapaBotones_Indices(p, m, a, actual, 0, idx) Else padre.datos.SetMapaBotones_Indices(p, m, a, actual, 1, idx)
            Else
                padre.datos.SetMapaBotones_Indices(p, m, a, actual, padre.NumericUpDownPosition.Value - 1, idx)
            End If
        Else
            If padre.RadioButton1.Visible = True Then
                If padre.RadioButton1.Checked Then padre.datos.SetMapaEjes_Indices(p, m, a, actual - 64, 0, idx) Else padre.datos.SetMapaEjes_Indices(p, m, a, actual - 64, 1, idx)
            Else
                padre.datos.SetMapaEjes_Indices(p, m, a, actual - 64, padre.NumericUpDownPosition.Value - 1, idx)
            End If
        End If
    End Sub

    Public Sub CargarIndice()
        eventos = False
        Dim p, m, a As Byte
        GetModos(p, m, a)
        If actual > 99 Then
            If padre.RadioButton1.Visible Then
                If padre.RadioButton1.Checked Then padre.ComboBoxAssigned.SelectedIndex = padre.datos.GetMapaSetas_Indices(p, m, a, actual - 100, 0) Else padre.ComboBoxAssigned.SelectedIndex = padre.datos.GetMapaSetas_Indices(p, m, a, actual - 100, 1)
            Else
                padre.ComboBoxAssigned.SelectedIndex = padre.datos.GetMapaSetas_Indices(p, m, a, actual - 100, padre.NumericUpDownPosition.Value - 1)
            End If
        ElseIf actual < 64 Then
            If padre.RadioButton1.Visible Then
                If padre.RadioButton1.Checked Then padre.ComboBoxAssigned.SelectedIndex = padre.datos.GetMapaBotones_Indices(p, m, a, actual, 0) Else padre.ComboBoxAssigned.SelectedIndex = padre.datos.GetMapaBotones_Indices(p, m, a, actual, 1)
            Else
                padre.ComboBoxAssigned.SelectedIndex = padre.datos.GetMapaBotones_Indices(p, m, a, actual, padre.NumericUpDownPosition.Value - 1)
            End If
        Else
            If padre.RadioButton1.Visible Then
                If padre.RadioButton1.Checked Then padre.ComboBoxAssigned.SelectedIndex = padre.datos.GetMapaEjes_Indices(p, m, a, actual - 64, 0) Else padre.ComboBoxAssigned.SelectedIndex = padre.datos.GetMapaEjes_Indices(p, m, a, actual - 64, 1)
            Else
                padre.ComboBoxAssigned.SelectedIndex = padre.datos.GetMapaEjes_Indices(p, m, a, actual - 64, padre.NumericUpDownPosition.Value - 1)
            End If
        End If

        eventos = True
    End Sub

#End Region

End Module
