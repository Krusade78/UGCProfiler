Public Class Principal

    Public datos As Datos
    Private esNuevo As Boolean = True
    Private parteJoy As Object
    Private report As Object

#Region "Eventos principales"

    Private Sub Form1_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
#If LITE = 1 Then
        ButtonSensibility.Visible = False
        RadioButtonToggle.Visible = False
#End If
        Traducir()
        Vista.SetPadre(Me)
        datos = New Datos()
        If My.Application.CommandLineArgs.Count = 1 Then
            Abrir(My.Application.CommandLineArgs(0))
        End If
    End Sub
    Private Sub Principal_FormClosing(ByVal sender As Object, ByVal e As System.Windows.Forms.FormClosingEventArgs) Handles Me.FormClosing
        If datos.GetModificado() Then
            Dim r As Microsoft.VisualBasic.MsgBoxResult = Traduce.Msg("save_changes", "warning", MsgBoxStyle.YesNoCancel Or MsgBoxStyle.Exclamation)
            If r = MsgBoxResult.Cancel Then
                e.Cancel = True
                Exit Sub
            Else
                If r = MsgBoxResult.Yes Then
                    If Not Guardar() Then e.Cancel = True : Exit Sub
                End If
            End If
        End If
    End Sub

    Private Sub MenuNew_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MenuNew.Click
        Nuevo()
    End Sub
    Private Sub MenuOpen_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MenuOpen.Click
        Abrir()
    End Sub
    Private Sub MenuSave_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MenuSave.Click
        Guardar()
    End Sub
    Private Sub MenuSaveAs_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MenuSaveAs.Click
        GuardarComo()
    End Sub

    Private Sub MenuPrint_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MenuPrint.Click
        Dim im As New Imprimir(Me)
        im.Ver()
    End Sub
    Private Sub MenuPrintPreview_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MenuPrintPreview.Click
        Dim im As New Imprimir(Me)
        im.PreVer()
    End Sub
    Private Sub MenuExit_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MenuExit.Click
        Me.Close()
    End Sub
    Private Sub MenuMouse_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MenuMouse.Click
        Dim m As New EditorRaton()
        m.ShowDialog(Me)
    End Sub
    Private Sub MenuMode_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MenuMode.Click
        Dim b As Byte
        Dim old As Byte = datos.GetModosDefecto()
        Dim m As New EditorModos()
        m.ShowDialog(Me)
        b = datos.GetModosDefecto()
        If b <> old Then Vista.RefrescarModos()
    End Sub

    Private Sub MenuLaunch_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MenuLaunch.Click
        Lanzar()
    End Sub
    Private Sub MenuAbout_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MenuAbout.Click
        Dim about As New AcercaDe
        Me.AddOwnedForm(about)
        about.Show()
    End Sub

    Private Sub ToolBar1_ItemClicked(ByVal sender As System.Object, ByVal e As System.Windows.Forms.ToolStripItemClickedEventArgs) Handles ToolBar1.ItemClicked
        Select Case e.ClickedItem.Tag.ToString
            Case "Nuevo"
                Nuevo()
            Case "Abrir"
                Abrir()
            Case "Guardar"
                Guardar()
            Case "Imprimir"
                Dim im As New Imprimir(Me)
                im.Ver()
            Case "Lanzar"
                Lanzar()
            Case "X36F"
                If report IsNot Nothing Then
                    Fondo.Visible = True
                    Me.Controls.Remove(report)
                    report = Nothing
                End If
                Label1.Text = "X36-F && X35-T"
                Label1.Visible = True
                RadioButton13.Visible = True
                RadioButton14.Visible = True
                datos.SetTipoHotas(0)
                If RadioButton13.Checked Then RadioButton14.Checked = True
                RadioButton13.Checked = True
            Case "X36"
                If report IsNot Nothing Then
                    Fondo.Visible = True
                    Me.Controls.Remove(report)
                    report = Nothing
                End If
                Label1.Text = "X36 USB HOTAS"
                Label1.Visible = True
                RadioButton13.Visible = True
                RadioButton14.Visible = True
                datos.SetTipoHotas(1)
                If RadioButton13.Checked Then RadioButton14.Checked = True
                RadioButton13.Checked = True
            Case "X45"
                If report IsNot Nothing Then
                    Fondo.Visible = True
                    Me.Controls.Remove(report)
                    report = Nothing
                End If
                Label1.Text = "X45 HOTAS"
                Label1.Visible = True
                RadioButton13.Visible = True
                RadioButton14.Visible = True
                datos.SetTipoHotas(2)
                If RadioButton13.Checked Then RadioButton14.Checked = True
                RadioButton13.Checked = True
            Case "X52"
                If report IsNot Nothing Then
                    Fondo.Visible = True
                    Me.Controls.Remove(report)
                    report = Nothing
                End If
                Label1.Text = "X52 HOTAS"
                Label1.Visible = True
                RadioButton13.Visible = True
                RadioButton14.Visible = True
                datos.SetTipoHotas(3)
                If RadioButton13.Checked Then RadioButton14.Checked = True
                RadioButton13.Checked = True
            Case "Mouse"
                Dim m As New EditorRaton()
                m.ShowDialog(Me)
            Case "Modos"
                Dim b As Byte
                Dim old As Byte = datos.GetModosDefecto()
                Dim m As New EditorModos()
                m.ShowDialog(Me)
                b = datos.GetModosDefecto()
                If b <> old Then Vista.RefrescarModos()
            Case "editar"
                If report IsNot Nothing Then
                    Fondo.Visible = True
                    Me.Controls.Remove(report)
                    report = Nothing
                End If
            Case "listar"
                If report Is Nothing Then
                    report = New VistaReport
                    Me.Controls.Add(report)
                    Fondo.Visible = False
                End If
        End Select
    End Sub

    Private Sub ToolStripSplitPinkie_DropDownItemClicked(ByVal sender As Object, ByVal e As System.Windows.Forms.ToolStripItemClickedEventArgs) Handles ToolStripSplitPinkie.DropDownItemClicked
        ToolStripSplitPinkie.Tag = ToolStripSplitPinkie.DropDownItems.IndexOf(e.ClickedItem)
        ToolStripSplitPinkie.Text = e.ClickedItem.Text
        Vista.Refrescar()
    End Sub
    Private Sub ToolStripSplitModo_DropDownItemClicked(ByVal sender As Object, ByVal e As System.Windows.Forms.ToolStripItemClickedEventArgs) Handles ToolStripSplitModo.DropDownItemClicked
        ToolStripSplitModo.Tag = ToolStripSplitModo.DropDownItems.IndexOf(e.ClickedItem)
        ToolStripSplitModo.Text = e.ClickedItem.Text
        Vista.Refrescar()
    End Sub
    Private Sub ToolStripSplitAux_DropDownItemClicked(ByVal sender As Object, ByVal e As System.Windows.Forms.ToolStripItemClickedEventArgs) Handles ToolStripSplitAux.DropDownItemClicked
        ToolStripSplitAux.Tag = ToolStripSplitAux.DropDownItems.IndexOf(e.ClickedItem)
        ToolStripSplitAux.Text = e.ClickedItem.Text
        Vista.Refrescar()
    End Sub

    Private Sub RadioButton13_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles RadioButton13.CheckedChanged
        If RadioButton13.Checked = False Then Exit Sub
        Select Case datos.GetTipoHotas()
            Case 0
                If parteJoy IsNot Nothing Then Fondo.Controls.Remove(parteJoy)
                parteJoy = New X36F
                Fondo.Controls.Add(parteJoy)
            Case 1
                If parteJoy IsNot Nothing Then Fondo.Controls.Remove(parteJoy)
                parteJoy = New X36F
                Fondo.Controls.Add(parteJoy)
            Case 2
                If parteJoy IsNot Nothing Then Fondo.Controls.Remove(parteJoy)
                parteJoy = New X45J
                Fondo.Controls.Add(parteJoy)
            Case 3
                If parteJoy IsNot Nothing Then Fondo.Controls.Remove(parteJoy)
                parteJoy = New X52J
                Fondo.Controls.Add(parteJoy)
        End Select
        If Me.Propiedades.Enabled = False Then Me.Propiedades.Enabled = True
    End Sub

    Private Sub RadioButton14_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles RadioButton14.CheckedChanged
        If RadioButton14.Checked = False Then Exit Sub
        Select Case datos.GetTipoHotas()
            Case 0
                If parteJoy IsNot Nothing Then Fondo.Controls.Remove(parteJoy)
                parteJoy = New X35T
                Fondo.Controls.Add(parteJoy)
            Case 1
                If parteJoy IsNot Nothing Then Fondo.Controls.Remove(parteJoy)
                parteJoy = New X35T
                Fondo.Controls.Add(parteJoy)
            Case 2
                If parteJoy IsNot Nothing Then Fondo.Controls.Remove(parteJoy)
                parteJoy = New X45T
                Fondo.Controls.Add(parteJoy)
            Case 3
                If parteJoy IsNot Nothing Then Fondo.Controls.Remove(parteJoy)
                parteJoy = New X52T
                Fondo.Controls.Add(parteJoy)
        End Select
        If Me.Propiedades.Enabled = False Then Me.Propiedades.Enabled = True
    End Sub
#End Region

#Region "Metodos"

    Private Sub Traducir()
        Me.Text = Traduce.Txt("profile_editor") & " - [" & Traduce.Txt("untitled") & "]"
        'Menus
        MenuFile.Text = Traduce.Txt("mfile")
        MenuNew.Text = Traduce.Txt("mnew")
        MenuOpen.Text = Traduce.Txt("mopen")
        MenuSave.Text = Traduce.Txt("msave")
        MenuSaveAs.Text = Traduce.Txt("msave_as")
        MenuPrint.Text = Traduce.Txt("mprint")
        MenuPrintPreview.Text = Traduce.Txt("mprint_preview")
        MenuExit.Text = Traduce.Txt("mexit")
        MenuProfile.Text = Traduce.Txt("mprofile")
        MenuMouse.Text = Traduce.Txt("mmouse_configuration")
        MenuMode.Text = Traduce.Txt("mmode_configuration")
        MenuLaunch.Text = Traduce.Txt("mlaunch")
        MenuView.Text = Traduce.Txt("mview")
        MenuEditV.Text = Traduce.Txt("medit_view")
        MenuListV.Text = Traduce.Txt("mlist_view")
        MenuHelp.Text = Traduce.Txt("mhelp")
        MenuAbout.Text = Traduce.Txt("mabout")
        ' Dialogos
        OpenFileDialog1.Filter = Traduce.Txt("profile_file")
        SaveFileDialog1.Filter = Traduce.Txt("profile_file")
        OpenFileDialog1.FileName = Traduce.Txt("profile_name")
        SaveFileDialog1.FileName = Traduce.Txt("profile_name")
        ' Barra botones
        ToolBar1.Items(0).ToolTipText = Traduce.Txt("new")
        ToolBar1.Items(1).ToolTipText = Traduce.Txt("open")
        ToolBar1.Items(2).ToolTipText = Traduce.Txt("save")
        ToolBar1.Items(3).ToolTipText = Traduce.Txt("print")
        ToolBar1.Items(5).ToolTipText = Traduce.Txt("launch")
        ToolBar1.Items(7).ToolTipText = Traduce.Txt("x36_gameport")
        ToolBar1.Items(12).ToolTipText = Traduce.Txt("mouse_configuration")
        ToolBar1.Items(19).ToolTipText = Traduce.Txt("modes_configuration")
        ToolBar1.Items(14).ToolTipText = Traduce.Txt("edit_view")
        ToolBar1.Items(15).ToolTipText = Traduce.Txt("list_view")
        ToolStripLabel2.Text = Traduce.Txt("current_mode")
        ToolStripSplitPinkie.Text = Traduce.Txt("no_modes")
        'Barra interna
        RadioButton13.Text = Traduce.Txt("joystick")
        RadioButton14.Text = Traduce.Txt("throttle")
        'Propiedades
        LabelAxis.Text = Traduce.Txt("axis_configuration")
        LabelWorkAs.Text = Traduce.Txt("work_as")
        ComboBoxAxes.Items.AddRange(New String() {Traduce.Txt("none"), "X", "Y", "R", "Z", "Rot. X", "Rot. Y", Traduce.Txt("slider"), "MiniStick X", "MiniStick Y", Traduce.Txt("mousex"), Traduce.Txt("mousey")})
        ComboBoxAxes.SelectedIndex = 0
        CheckBoxInverted.Text = Traduce.Txt("inverted")
        ButtonSensibility.Text = Traduce.Txt("sensibility_curve")
        LabelMSensibility.Text = Traduce.Txt("mouse_sensibility")
        LabelDigital.Text = Traduce.Txt("digital_function")
        RadioButtonIncremental.Text = Traduce.Txt("incremental")
        RadioButtonBands.Text = Traduce.Txt("bands")
        LabelResistanceInc.Text = Traduce.Txt("resistance_inc")
        LabelResistanceDec.Text = Traduce.Txt("resistance_dec")
        ButtonEditBands.Text = Traduce.Txt("edit_bands")
        LabelButton.Text = Traduce.Txt("button_configuration")
        RadioButtonUpDown.Text = Traduce.Txt("updown")
        RadioButtonToggle.Text = Traduce.Txt("toggle")
        ButtonAssignPOV.Text = Traduce.Txt("assign_to_pov")
        ButtonAssignPinkie.Text = Traduce.Txt("assign_to_pinkie")
        ButtonAssignModes.Text = Traduce.Txt("assign_to_modes")
        LabelPositions.Text = Traduce.Txt("positions")
        LabelAssigned.Text = Traduce.Txt("assigned_macros")
        RadioButton1.Text = Traduce.Txt("pressed")
        RadioButton2.Text = Traduce.Txt("raised")
        LabelPosition.Text = Traduce.Txt("position")
        ComboBoxAssigned.Items.Add(Chr(1) & "</------- " & Traduce.Txt("none") & " -------/>")
        ComboBoxAssigned.SelectedIndex = 0
        LabelMacro.Text = Traduce.Txt("macro_configuration")
        ButtonAdd.Text = Traduce.Txt("Add")
        ButtonEdit.Text = Traduce.Txt("Edit")
        ButtonDelete.Text = Traduce.Txt("Delete")
    End Sub

    Private Sub Nuevo()
        If datos.GetModificado() Then
            Dim r As Microsoft.VisualBasic.MsgBoxResult = Traduce.Msg("save_changes", "warning", MsgBoxStyle.YesNoCancel Or MsgBoxStyle.Exclamation)
            If r = MsgBoxResult.Cancel Then
                Exit Sub
            Else
                If r = MsgBoxResult.Yes Then
                    If Not Guardar() Then Exit Sub
                End If
            End If
        End If
        Label1.Visible = False
        RadioButton13.Visible = False
        RadioButton14.Visible = False
        ComboBoxMacro.Items.Clear()
        ComboBoxAssigned.Items.Clear()
        ComboBoxAssigned.Items.Add("</------- " & Traduce.Txt("none") & " -------/>")
        ComboBoxAssigned.SelectedIndex = 0
        datos = Nothing
        GC.Collect()
        datos = New Datos()
        If parteJoy IsNot Nothing Then Fondo.Controls.Remove(parteJoy) : Me.Propiedades.Enabled = False
        Vista.Clear(False)
        Me.Text = Traduce.Txt("profile_editor") & " - [" & Traduce.Txt("untitled") & "]"
        esNuevo = True
        If report IsNot Nothing Then
            Fondo.Visible = True
            Me.Controls.Remove(report)
        End If
    End Sub

    Private Sub Abrir(Optional ByVal archivo As String = Nothing)
        If archivo Is Nothing Then
            If datos.GetModificado() Then
                Dim r As Microsoft.VisualBasic.MsgBoxResult = Traduce.Msg("save_changes", "warning", MsgBoxStyle.YesNoCancel Or MsgBoxStyle.Exclamation)
                If r = MsgBoxResult.Cancel Then
                    Exit Sub
                Else
                    If r = MsgBoxResult.Yes Then
                        If Not Guardar() Then Exit Sub
                    End If
                End If
            End If
            If OpenFileDialog1.ShowDialog() = Windows.Forms.DialogResult.OK Then
                archivo = OpenFileDialog1.FileName
            End If
        End If
        If Not archivo Is Nothing Then
            ComboBoxMacro.Items.Clear()
            ComboBoxAssigned.Items.Clear()
            ComboBoxAssigned.Items.Add("</------- " & Traduce.Txt("none") & " -------/>")
            ComboBoxAssigned.SelectedIndex = 0
            datos = Nothing
            GC.Collect()
            datos = New Datos()
            If datos.CargarArchivo(archivo, ComboBoxAssigned, ComboBoxMacro) Then
                If report IsNot Nothing Then 'a vista normal
                    Fondo.Visible = True
                    Me.Controls.Remove(report)
                End If
                Select Case datos.GetTipoHotas()
                    Case 0
                        Label1.Text = "X36-F && X35-T"
                    Case 1
                        Label1.Text = "X36 USB HOTAS"
                    Case 2
                        Label1.Text = "X45 HOTAS"
                    Case 3
                        Label1.Text = "X52 HOTAS"
                End Select
                Label1.Visible = True
                RadioButton13.Visible = True
                RadioButton14.Visible = True
                If RadioButton13.Checked Then RadioButton14.Checked = True
                Vista.Clear(True)
                RadioButton13.Checked = True
                Me.Text = Traduce.Txt("profile_editor") & " - [" & archivo.Remove(0, archivo.LastIndexOf("\") + 1) & "]"
                esNuevo = False
            Else
                Label1.Visible = False
                RadioButton13.Visible = False
                RadioButton14.Visible = False
                ComboBoxMacro.Items.Clear()
                ComboBoxAssigned.Items.Clear()
                ComboBoxAssigned.Items.Add("</------- " & Traduce.Txt("none") & " -------/>")
                ComboBoxAssigned.SelectedIndex = 0
                datos = Nothing
                GC.Collect()
                datos = New Datos()
                If parteJoy IsNot Nothing Then Fondo.Controls.Remove(parteJoy) : Me.Propiedades.Enabled = False
                Vista.Clear(False)
                Me.Text = Traduce.Txt("profile_editor") & " - [" & Traduce.Txt("untitled") & "]"
                esNuevo = True
            End If
        End If
    End Sub

    Private Function Guardar() As Boolean
        If Fondo.Controls.Count = 5 Then
            If esNuevo Then
                Return GuardarComo()
            Else
                Return datos.Guardar(ComboBoxMacro)
            End If
        End If
    End Function

    Private Function GuardarComo() As Boolean
        If Fondo.Controls.Count = 5 Then
            SaveFileDialog1.FileName = Traduce.Txt("profile_name")
            If SaveFileDialog1.ShowDialog() = Windows.Forms.DialogResult.OK Then
                If datos.Guardar(SaveFileDialog1.FileName, ComboBoxMacro) Then
                    esNuevo = False
                    Me.Text = Traduce.Txt("profile_editor") & " - [" & SaveFileDialog1.FileName.Remove(0, SaveFileDialog1.FileName.LastIndexOf("\") + 1) & "]"
                    Return True
                Else
                    Return False
                End If
            Else
                Return False
            End If
        End If
    End Function

    Private Sub Lanzar()
        If datos.GetModificado() Then
            Dim r As Microsoft.VisualBasic.MsgBoxResult = Traduce.Msg("save_before_launch", "warning", MsgBoxStyle.OkCancel Or MsgBoxStyle.Exclamation)
            If r = MsgBoxResult.Cancel Then
                Exit Sub
            Else
                If Not Guardar() Then Exit Sub
            End If
        End If
        If Driver.Lanzar(datos.GetRutaPerfil()) Then
            Traduce.Msg("load_ok", "ok", MsgBoxStyle.Information)
        End If
    End Sub

#End Region

#Region "Eventos propiedades"

#Region "Eventos panel ejes"
    Private Sub ButtonSensibility_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonSensibility.Click
        Dim s As New EditorSensibilidad()
        s.ShowDialog(Me)
    End Sub
    Private Sub SetEje()
        Dim p, m, a, ej As Byte
        Vista.GetModos(p, m, a, ej)
        If CheckBoxInverted.Checked Then
            datos.SetMapaEjes_nEje(p, m, a, ej, (datos.GetMapaEjes_nEje(p, m, a, ej) And 128) Or (ComboBoxAxes.SelectedIndex + 20))
        Else
            datos.SetMapaEjes_nEje(p, m, a, ej, (datos.GetMapaEjes_nEje(p, m, a, ej) And 128) Or ComboBoxAxes.SelectedIndex)
        End If
        Vista.Refrescar()
    End Sub
    Private Sub CheckBoxInverted_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CheckBoxInverted.CheckedChanged
        If Vista.GetEventos() Then SetEje()
    End Sub
    Private Sub ComboBoxAxes_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ComboBoxAxes.SelectedIndexChanged
        If Vista.GetEventos() Then SetEje()
    End Sub
    Private Sub NumericUpDownMSensibility_ValueChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles NumericUpDownMSensibility.ValueChanged
        If Vista.GetEventos() Then
            Dim p, m, a, ej As Byte
            Vista.GetModos(p, m, a, ej)
            datos.SetMapaEjes_MSensibilidad(p, m, a, ej, NumericUpDownMSensibility.Value)
        End If
    End Sub
    Private Sub RadioButtonIncremental_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles RadioButtonIncremental.CheckedChanged
        If Vista.GetEventos() Then
            If RadioButtonIncremental.Checked Then
                Dim p, m, a, ej As Byte
                Vista.GetModos(p, m, a, ej)
                datos.SetMapaEjes_Incremental(p, m, a, ej, 1)
                Vista.Refrescar()
            End If
        End If
    End Sub
    Private Sub RadioButtonBands_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles RadioButtonBands.CheckedChanged
        If Vista.GetEventos() Then
            If RadioButtonBands.Checked Then
                Dim p, m, a, ej As Byte
                Vista.GetModos(p, m, a, ej)
                datos.SetMapaEjes_Incremental(p, m, a, ej, 0)
                Vista.Refrescar()
            End If
        End If
    End Sub
    Private Sub NumericUpDownResistanceInc_ValueChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles NumericUpDownResistanceInc.ValueChanged
        If Vista.GetEventos() Then
            Dim p, m, a, ej As Byte
            Vista.GetModos(p, m, a, ej)
            datos.SetMapaEjes_SensibilidadInc(p, m, a, ej, NumericUpDownResistanceInc.Value)
        End If
    End Sub
    Private Sub NumericUpDownResistanceDec_ValueChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles NumericUpDownResistanceDec.ValueChanged
        If Vista.GetEventos() Then
            Dim p, m, a, ej As Byte
            Vista.GetModos(p, m, a, ej)
            datos.SetMapaEjes_SensibilidadDec(p, m, a, ej, NumericUpDownResistanceDec.Value)
        End If
    End Sub
    Private Sub ButtonEditBands_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonEditBands.Click
        Dim b As New EditorBandas(Me)
        b.ShowDialog(Me)
    End Sub

#End Region

#Region "Eventos panel botones"
    Private Sub RadioButtonUpDown_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles RadioButtonUpDown.CheckedChanged
        If Vista.GetEventos() Then
            Dim p, m, a, b As Byte
            Vista.GetModos(p, m, a, b)
            If b > 99 Then
                datos.SetMapaSetas_Estado(p, m, a, b - 100, 0)
            Else
                datos.SetMapaBotones_Estado(p, m, a, b, 0)
            End If
            Vista.Refrescar()
        End If
    End Sub

    Private Sub RadioButtonToggle_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles RadioButtonToggle.CheckedChanged
        If Vista.GetEventos() Then
            Dim p, m, a, b As Byte
            Vista.GetModos(p, m, a, b)
            If b > 99 Then
                datos.SetMapaSetas_Estado(p, m, a, b - 100, 1)
            Else
                datos.SetMapaBotones_Estado(p, m, a, b, 1)
            End If
            Vista.Refrescar()
        End If
    End Sub

    Private Sub NumericUpDownPositions_ValueChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles NumericUpDownPositions.ValueChanged
        If Vista.GetEventos() Then
            Dim p, m, a, b As Byte
            Vista.GetModos(p, m, a, b)
            If b > 99 Then
                datos.SetMapaSetas_Estado(p, m, a, b - 100, NumericUpDownPositions.Value)
            Else
                datos.SetMapaBotones_Estado(p, m, a, b, NumericUpDownPositions.Value)
            End If
            Vista.Refrescar()
        End If
    End Sub
    Private Sub ButtonAssignPOV_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonAssignPOV.Click
        Dim s As New EditorPOV()
        If s.ShowDialog(Me) = Windows.Forms.DialogResult.OK Then
            Vista.Refrescar()
        End If
    End Sub
    Private Sub ButtonAssignPinkie_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonAssignPinkie.Click
        Dim s As New EditorPinkieModos(True)
        If s.ShowDialog(Me) = Windows.Forms.DialogResult.OK Then
            Vista.Refrescar()
        End If
    End Sub
    Private Sub ButtonAssignModes_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonAssignModes.Click
        Dim s As New EditorPinkieModos(False)
        If s.ShowDialog(Me) = Windows.Forms.DialogResult.OK Then
            Vista.Refrescar()
        End If
    End Sub
#End Region

#Region "Eventos panel macros"
    Private Sub ComboBoxAssigned_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ComboBoxAssigned.SelectedIndexChanged
        If ComboBoxAssigned.SelectedIndex <> -1 And Vista.GetEventos() Then Vista.CambiarIndice(ComboBoxAssigned.SelectedIndex)
    End Sub
    Private Sub NumericUpDownPosition_ValueChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles NumericUpDownPosition.ValueChanged
        If Vista.GetEventos() Then Vista.CargarIndice()
    End Sub
    Private Sub RadioButton2_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles RadioButton2.CheckedChanged
        If Vista.GetEventos() Then Vista.CargarIndice()
    End Sub
    Private Sub RadioButton1_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles RadioButton1.CheckedChanged
        If Vista.GetEventos() Then Vista.CargarIndice()
    End Sub

    Private Sub ButtonAdd_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonAdd.Click
        Dim m As New EditorMacros(-1, Me)
        If m.ShowDialog(Me) <> Windows.Forms.DialogResult.Cancel Then ComboBoxMacro.SelectedIndex = m.Tag
    End Sub
    Private Sub ButtonEdit_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonEdit.Click
        If ComboBoxMacro.SelectedIndex > -1 Then
            Dim m As New EditorMacros(ComboBoxMacro.SelectedIndex, Me)
            If ComboBoxAssigned.SelectedIndex = (ComboBoxMacro.SelectedIndex + 1) Then
                Dim r As DialogResult = m.ShowDialog(Me)
                If r <> Windows.Forms.DialogResult.Cancel Then ComboBoxMacro.SelectedIndex = m.Tag
                If r = Windows.Forms.DialogResult.OK Then Vista.CargarIndice()
            Else
                Dim r As DialogResult = m.ShowDialog(Me)
                If r <> Windows.Forms.DialogResult.Cancel Then ComboBoxMacro.SelectedIndex = m.Tag
            End If
        End If
    End Sub
    Private Sub ButtonDelete_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonDelete.Click
        If ComboBoxMacro.SelectedIndex > -1 Then
            Dim s As Boolean = (ComboBoxMacro.SelectedIndex = ComboBoxAssigned.SelectedIndex - 1)
            datos.QuitarIndice(ComboBoxMacro.SelectedIndex)
            ComboBoxAssigned.Items.RemoveAt(ComboBoxMacro.SelectedIndex + 1)
            ComboBoxMacro.Items.RemoveAt(ComboBoxMacro.SelectedIndex)
            If ComboBoxMacro.Items.Count > 0 Then ComboBoxMacro.SelectedIndex = 0
            If s Then Vista.CargarIndice()
        End If
    End Sub
#End Region

#End Region

End Class
