Public Class EditorMacros
    Private padre As Principal
    Private indicep As Integer
    Private reps As Integer = 0
    Public Shared ultimaPlantilla As Integer = 0
    Private macro As New ArrayList
    Private Declare Auto Function WideCharToMultiByte Lib "kernel32.dll" (ByVal codepage As UInteger, ByVal flags As UInt32, ByVal wstr As String, ByVal wstr_tam As Integer, ByVal mb_str() As Byte, ByVal mbstr_tam As Integer, ByRef defchar As Byte, ByRef bdefchar As Byte) As Integer

    Public Sub New(ByVal i As Integer, ByVal p As Principal)
        InitializeComponent()
        padre = p
        indicep = i
        TextBoxTecla.Tag = New ArrayList
    End Sub

    Private Sub EditorMacros_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
#If LITE = 1 Then
        Me.RadioButtonAvanzado.Visible = False
        Me.RadioButtonBasico.Visible = False
#End If
        Traducir()
        CargarPlantillas()
        If ComboBox1.Items.Count = 0 Then Exit Sub
        If padre.datos.GetTipoHotas() < 3 Then
            Me.CheckBox1.Enabled = False
            Me.PanelX52.Enabled = False
        End If
        If indicep > -1 Then
            TextBoxNombre.Text = padre.ComboBoxMacro.Items.Item(indicep)
            Dim antigua As ArrayList = padre.datos.GetMacro(indicep)
            For i As Integer = 0 To antigua.Count - 1
                macro.Add(antigua(i))
            Next
        End If
        If indicep > -1 Then
            ' Comprobar el check de enviar el nombre al mfd
            Dim idc As Boolean = False
            If macro.Count >= 2 Then
                If macro(0) = (24 + (3 << 8)) Then idc = True
            End If
            If idc Then
                Dim nombreOk As Boolean = True
                Dim comando As UInt16
                Dim stb(16) As Byte
                Dim st As String = TextBoxNombre.Text.Trim()

                If st.Length > 16 Then st = st.Substring(0, 16)
                WideCharToMultiByte(20127, 0, st, st.Length, stb, 17, 0, 0)
                If macro.Count >= (st.Length + 2) Then
                    For i As Integer = 0 To st.Length - 1
                        comando = 24 + (CInt(stb(i)) << 8)
                        If comando <> macro(i + 1) Then
                            nombreOk = False
                            Exit For
                        End If
                    Next
                    If macro(st.Length + 1) = 56 And nombreOk Then
                        For i As Integer = 0 To st.Length + 1
                            macro.RemoveAt(0)
                        Next
                        CheckBox1.Checked = True
                    End If
                End If
            End If
            Cargar()
        Else
            Me.CheckBox1.Checked = True
        End If
        PasarABasico()
    End Sub

    Protected Overrides Function ProcessDialogKey(ByVal keyData As Keys) As Boolean
        If TextBoxTecla.Focused Then Return True Else Return False
        'Select Case keyData And 255
        '    Case Keys.F4
        '        Return True
        '    Case Keys.LMenu
        '        Return True
        '    Case Keys.RMenu
        '        Return True
        '    Case Keys.Return
        '        Return True
        '    Case Keys.Escape
        '        Return True
        '    Case Else
        '        Return False
        'End Select
    End Function

#Region "metodos"
    Private Sub Traducir()
        Me.Text = Traduce.Txt("edit_macro")
        Me.LabelNombre.Text = Traduce.Txt("name")
        Me.ButtonAcepta.Text = Traduce.Txt("accept")
        Me.ButtonCancel.Text = Traduce.Txt("cancel")
        Me.CheckBox1.Text = Traduce.Txt("use_in_mfd")
        Me.RadioButtonAvanzado.Text = Traduce.Txt("advanced_mode")
        Me.RadioButtonBasico.Text = Traduce.Txt("easy_mode")
        Me.LabelTeclado.Text = Traduce.Txt("keyboard")
        Me.LabelPantilla.Text = Traduce.Txt("template")
        Me.LabelTecla.Text = Traduce.Txt("key")
        Me.ButtonPresionar.Text = Traduce.Txt("press")
        Me.ButtonSoltar.Text = Traduce.Txt("release")
        Me.LabelInsercion.Text = Traduce.Txt("easy_config")
        Me.ButtonNormal.Text = Traduce.Txt("normal")
        Me.ButtonTeclasRepetir.Text = Traduce.Txt("repeat")
        Me.LabelModos.Text = Traduce.Txt("modes")
        Me.ButtonModo1.Text = Traduce.Txt("mode1")
        Me.ButtonModo2.Text = Traduce.Txt("mode2")
        Me.ButtonModo3.Text = Traduce.Txt("mode3")
        Me.ButtonAux1.Text = Traduce.Txt("auxm1")
        Me.ButtonAux2.Text = Traduce.Txt("auxm2")
        Me.ButtonAux3.Text = Traduce.Txt("auxm3")
        Me.ButtonPinkieOff.Text = Traduce.Txt("pinkie") & "off"
        Me.ButtonPinkieOn.Text = Traduce.Txt("pinkie") & "on"
        Me.GroupBox1.Text = Traduce.Txt("commands")
        'Me.ButtonBorrar.Text = Traduce.Txt("delete_command")
        Me.LabelEspecial.Text = Traduce.Txt("special")
        Me.ButtonMantener.Text = Traduce.Txt("hold")
        Me.ButtonRepetir.Text = Traduce.Txt("repeat")
        'Me.ButtonFinRepetir.Text = Traduce.Txt("repeat_end")
        Me.ButtonPausa.Text = Traduce.Txt("pause")
        Me.ButtonRepetirN.Text = Traduce.Txt("repeatn")
        'Me.ButtonfinRepetirN.Text = Traduce.Txt("repeatn_end")
        Me.LabelRaton.Text = Traduce.Txt("mouse")
        Me.LabelBotonesRaton.Text = Traduce.Txt("buttons")
        Me.ButtonIzquierdoOn.Text = Mid(Traduce.Txt("left"), 1, 5)
        Me.ButtonIzquierdoOff.Text = Mid(Traduce.Txt("left"), 1, 5)
        Me.ButtonCentroOff.Text = Traduce.Txt("center")
        Me.ButtonCentroOn.Text = Traduce.Txt("center")
        Me.ButtonDerechoOff.Text = Mid(Traduce.Txt("right"), 1, 5)
        Me.ButtonDerechoOn.Text = Mid(Traduce.Txt("right"), 1, 5)
        Me.LabelRueda.Text = Traduce.Txt("wheel")
        Me.ButtonArribaOff.Text = Traduce.Txt("up")
        Me.ButtonArribaOn.Text = Traduce.Txt("up")
        Me.ButtonAbajoOff.Text = Traduce.Txt("down")
        Me.ButtonAbajoOn.Text = Traduce.Txt("down")
        Me.LabelMovimiento.Text = Traduce.Txt("movement")
        Me.LabelSensibilidad.Text = Traduce.Txt("sensibility")
        Me.ButtonMovAbajo.Text = Traduce.Txt("down")
        Me.ButtonMovArriba.Text = Traduce.Txt("up")
        Me.ButtonMovDerecha.Text = Traduce.Txt("right")
        Me.ButtonMovIzquierda.Text = Traduce.Txt("left")
        Me.LabelBotonesDX.Text = Traduce.Txt("buttons")
        Me.LabelBoton.Text = Traduce.Txt("button")
        Me.LabelPov1.Text = Traduce.Txt("povs")
        Me.LabelPov2.Text = Traduce.Txt("pov")
        Me.LabelPosicion.Text = Traduce.Txt("position")
        Me.LabelLight.Text = Traduce.Txt("light")
        Me.LabelTexto.Text = Traduce.Txt("text")
        Me.LabelLinea.Text = Traduce.Txt("line")
        Me.ButtonLinea.Text = Traduce.Txt("text_line")
        Me.LabelHora.Text = Traduce.Txt("hour")
        Me.LabelMinuto.Text = Traduce.Txt("minute")
        Me.LabelReloj.Text = Traduce.Txt("clock")
        'Me.ButtonX52PinkieOff.Text = Traduce.Txt("pinkie") & "off"
        'Me.ButtonX52PinkieOn.Text = Traduce.Txt("pinkie") & "on"
        Me.ButtonFecha1.Text = Traduce.Txt("date") & " 1"
        Me.ButtonFecha2.Text = Traduce.Txt("date") & " 2"
        Me.ButtonFecha3.Text = Traduce.Txt("date") & " 3"
        Me.LabelDigito.Text = Traduce.Txt("digit")
        'Me.ButtonInfoOff.Text = Traduce.Txt("info") & " off"
        'Me.ButtonInfoOn.Text = Traduce.Txt("info") & " on"
        Me.ButtonLuzMfd.Text = Traduce.Txt("mfd_light")
        Me.ButtonLuzBotones.Text = Traduce.Txt("button_light")
        Me.LabelNivel.Text = Traduce.Txt("level")
    End Sub

    Private Sub Cargar()
        Dim soltar As Boolean
        Dim tipo As Byte
        Dim dato As Byte
        ListBox1.Items.Clear()
        For i As Integer = 0 To macro.Count - 1
            tipo = macro(i) And &HFF
            dato = macro(i) >> 8
            If tipo > 30 Then soltar = True : tipo = tipo - 32 Else soltar = False
            Select Case tipo
                Case 0
                    If soltar Then
                        ListBox1.Items.Add(Traduce.Txt("release") & " " & CType(ComboBox1.Items(dato), String).Remove(0, CType(ComboBox1.Items(dato), String).IndexOf(" ")))
                    Else
                        ListBox1.Items.Add(Traduce.Txt("press") & " " & CType(ComboBox1.Items(dato), String).Remove(0, CType(ComboBox1.Items(dato), String).IndexOf(" ")))
                    End If
                Case 1
                    If soltar Then
                        ListBox1.Items.Add(Traduce.Txt("mouse") & " " & Traduce.Txt("button") & " 1 Off")
                    Else
                        ListBox1.Items.Add(Traduce.Txt("mouse") & " " & Traduce.Txt("button") & " 1 On")
                    End If
                Case 2
                    If soltar Then
                        ListBox1.Items.Add(Traduce.Txt("mouse") & " " & Traduce.Txt("button") & " 2 Off")
                    Else
                        ListBox1.Items.Add(Traduce.Txt("mouse") & " " & Traduce.Txt("button") & " 2 On")
                    End If
                Case 3
                    If soltar Then
                        ListBox1.Items.Add(Traduce.Txt("mouse") & " " & Traduce.Txt("button") & " 3 Off")
                    Else
                        ListBox1.Items.Add(Traduce.Txt("mouse") & " " & Traduce.Txt("button") & " 3 On")
                    End If
                Case 4
                    If soltar Then
                        ListBox1.Items.Add(Traduce.Txt("mouse") & " " & Traduce.Txt("left") & " 0")
                    Else
                        ListBox1.Items.Add(Traduce.Txt("mouse") & " " & Traduce.Txt("left") & " " & dato)
                    End If
                Case 5
                    If soltar Then
                        ListBox1.Items.Add(Traduce.Txt("mouse") & " " & Traduce.Txt("right") & " 0")
                    Else
                        ListBox1.Items.Add(Traduce.Txt("mouse") & " " & Traduce.Txt("right") & " " & dato)
                    End If
                Case 6
                    If soltar Then
                        ListBox1.Items.Add(Traduce.Txt("mouse") & " " & Traduce.Txt("down") & " 0")
                    Else
                        ListBox1.Items.Add(Traduce.Txt("mouse") & " " & Traduce.Txt("down") & " " & dato)
                    End If
                Case 7
                    If soltar Then
                        ListBox1.Items.Add(Traduce.Txt("mouse") & " " & Traduce.Txt("up") & " 0")
                    Else
                        ListBox1.Items.Add(Traduce.Txt("mouse") & " " & Traduce.Txt("up") & " " & dato)
                    End If
                Case 8
                    If soltar Then
                        ListBox1.Items.Add(Traduce.Txt("wheel") & " " & Traduce.Txt("up") & " Off")
                    Else
                        ListBox1.Items.Add(Traduce.Txt("wheel") & " " & Traduce.Txt("up") & " On")
                    End If
                Case 9
                    If soltar Then
                        ListBox1.Items.Add(Traduce.Txt("wheel") & " " & Traduce.Txt("down") & " Off")
                    Else
                        ListBox1.Items.Add(Traduce.Txt("wheel") & " " & Traduce.Txt("down") & " On")
                    End If
                Case 10
                    ListBox1.Items.Add(Traduce.Txt("pause") & " " & dato)
                Case 11
                    ListBox1.Items.Add(Traduce.Txt("hold"))
                Case 12
                    If soltar Then ListBox1.Items.Add(Traduce.Txt("repeat_end")) Else ListBox1.Items.Add(Traduce.Txt("repeat"))
                Case 13
                    If soltar Then ListBox1.Items.Add(Traduce.Txt("repeatn_end")) Else ListBox1.Items.Add(Traduce.Txt("repeatn") & " " & dato)
                Case 14
                    ListBox1.Items.Add(Traduce.Txt("mode" & (dato + 1)))
                Case 15
                    ListBox1.Items.Add(Traduce.Txt("aux" & (dato + 1)))
                Case 16
                    If dato = 0 Then ListBox1.Items.Add(Traduce.Txt("pinkie") & " Off") Else ListBox1.Items.Add(Traduce.Txt("pinkie") & " On")
                Case 18
                    If soltar Then
                        ListBox1.Items.Add("DX " & Traduce.Txt("button") & " " & dato + 1 & " Off")
                    Else
                        ListBox1.Items.Add("DX " & Traduce.Txt("button") & " " & dato + 1 & " On")
                    End If
                Case 19
                    If soltar Then
                        ListBox1.Items.Add("DX " & Traduce.Txt("pov") & "_" & ((dato \ 8) + 1) & ((dato Mod 8) + 1) & " Off")
                    Else
                        ListBox1.Items.Add("DX " & Traduce.Txt("pov") & "_" & ((dato \ 8) + 1) & ((dato Mod 8) + 1) & " On")
                    End If
                Case 20
                    ListBox1.Items.Add(Traduce.Txt("mfd_light") & " " & dato)
                Case 21
                    ListBox1.Items.Add(Traduce.Txt("button_light") & " " & dato)
                Case 22
                    If dato = 0 Then ListBox1.Items.Add(Traduce.Txt("light") & " Info Off") Else ListBox1.Items.Add(Traduce.Txt("light") & " Info On")
                Case 23
                    If dato = 0 Then ListBox1.Items.Add("MFD " & Traduce.Txt("pinkie") & " Off") Else ListBox1.Items.Add("MFD " & Traduce.Txt("pinkie") & " On")
                Case 24
                    Dim texto As String = Traduce.Txt("text_line") & "_" & dato
                    Dim ascii(15) As Byte
                    Dim j As Byte = 0
                    While macro(i + 1) <> 56
                        i = i + 1
                        ascii(j) = macro(i) >> 8
                        j = j + 1
                    End While
                    i = i + 1
                    texto = texto & "  " & System.Text.ASCIIEncoding.ASCII.GetString(ascii)
                    ListBox1.Items.Add(texto)
                Case 25
                    If dato = 1 Then
                        ListBox1.Items.Add("(AM/PM) " & Traduce.Txt("hour") & "_" & dato & " " & (macro(i + 1) >> 8) & ":" & (macro(i + 2) >> 8))
                    Else
                        ListBox1.Items.Add("(AM/PM) " & Traduce.Txt("hour") & "_" & dato & " " & ((((macro(i + 1) >> 8) * 256) + (macro(i + 2) >> 8)) \ 60) & ":" & ((((macro(i + 1) >> 8) * 256) + (macro(i + 2) >> 8)) Mod 60))
                    End If
                    i = i + 2
                Case 26
                    If dato = 1 Then
                        ListBox1.Items.Add("(24H) " & Traduce.Txt("hour") & "_" & dato & " " & (macro(i + 1) >> 8) & ":" & (macro(i + 2) >> 8))
                    Else
                        ListBox1.Items.Add("(24H) " & Traduce.Txt("hour") & "_" & dato & " " & ((((macro(i + 1) >> 8) * 256) + (macro(i + 2) >> 8)) \ 60) & ":" & ((((macro(i + 1) >> 8) * 256) + (macro(i + 2) >> 8)) Mod 60))
                    End If
                    i = i + 2
                Case 27
                    ListBox1.Items.Add(Traduce.Txt("date") & "_" & dato & " " & (macro(i + 1) >> 8))
                    i = i + 1
            End Select
        Next
    End Sub

    Private Sub PasarABasico()
        PanelX52.Enabled = False
        LabelPov1.Enabled = False
        LabelPov2.Enabled = False
        NumericUpDownPov.Enabled = False
        NumericUpDownPosicion.Enabled = False
        ButtonPovOff.Enabled = False
        ButtonPovOn.Enabled = False
        ButtonDXOff.Enabled = False
        ButtonMovIzquierda.Enabled = False
        ButtonMovDerecha.Enabled = False
        ButtonMovAbajo.Enabled = False
        ButtonMovArriba.Enabled = False
        LabelSensibilidad.Enabled = False
        NumericUpDownSensibilidad.Enabled = False
        LabelMovimiento.Enabled = False
        ButtonArribaOff.Enabled = False
        ButtonAbajoOff.Enabled = False
        ButtonIzquierdoOff.Enabled = False
        ButtonCentroOff.Enabled = False
        ButtonDerechoOff.Enabled = False
        PanelEspecial.Enabled = False
        PanelModos.Enabled = False
        LabelPantilla.Enabled = False
        vtSelPlantilla.Enabled = False
        LabelTecla.Enabled = False
        ComboBox1.Enabled = False
        ButtonPresionar.Enabled = False
        ButtonSoltar.Enabled = False
        ButtonSubir.Enabled = False
        ButtonBajar.Enabled = False
        ButtonTeclasRepetir.Font = New Font(ButtonTeclasRepetir.Font, FontStyle.Bold)
        ButtonNormal.Font = New Font(ButtonNormal.Font, FontStyle.Bold)
        ButtonDXOn.Font = New Font(ButtonDXOn.Font, FontStyle.Bold)
        ButtonIzquierdoOn.Font = New Font(ButtonIzquierdoOn.Font, FontStyle.Bold)
        ButtonCentroOn.Font = New Font(ButtonCentroOn.Font, FontStyle.Bold)
        ButtonDerechoOn.Font = New Font(ButtonDerechoOn.Font, FontStyle.Bold)
        ButtonArribaOn.Font = New Font(ButtonArribaOn.Font, FontStyle.Bold)
        ButtonAbajoOn.Font = New Font(ButtonAbajoOn.Font, FontStyle.Bold)
        LabelBoton.Font = New Font(LabelBoton.Font, FontStyle.Bold)
        NumericUpDown1.Font = New Font(NumericUpDown1.Font, FontStyle.Bold)
    End Sub

    Private Sub PasarAAvanzado()
        If padre.datos.GetTipoHotas() = 3 Then PanelX52.Enabled = True
        LabelPov1.Enabled = True
        LabelPov2.Enabled = True
        NumericUpDownPov.Enabled = True
        NumericUpDownPosicion.Enabled = True
        ButtonPovOff.Enabled = True
        ButtonPovOn.Enabled = True
        ButtonDXOff.Enabled = True
        ButtonMovIzquierda.Enabled = True
        ButtonMovDerecha.Enabled = True
        ButtonMovAbajo.Enabled = True
        ButtonMovArriba.Enabled = True
        LabelSensibilidad.Enabled = True
        NumericUpDownSensibilidad.Enabled = True
        LabelMovimiento.Enabled = True
        ButtonArribaOff.Enabled = True
        ButtonAbajoOff.Enabled = True
        ButtonIzquierdoOff.Enabled = True
        ButtonCentroOff.Enabled = True
        ButtonDerechoOff.Enabled = True
        PanelEspecial.Enabled = True
        PanelModos.Enabled = True
        LabelPantilla.Enabled = True
        vtSelPlantilla.Enabled = True
        LabelTecla.Enabled = True
        ComboBox1.Enabled = True
        ButtonPresionar.Enabled = True
        ButtonSoltar.Enabled = True
        ButtonSubir.Enabled = True
        ButtonBajar.Enabled = True
        ButtonTeclasRepetir.Font = New Font(ButtonTeclasRepetir.Font, FontStyle.Regular)
        ButtonNormal.Font = New Font(ButtonNormal.Font, FontStyle.Regular)
        ButtonDXOn.Font = New Font(ButtonDXOn.Font, FontStyle.Regular)
        ButtonIzquierdoOn.Font = New Font(ButtonIzquierdoOn.Font, FontStyle.Regular)
        ButtonCentroOn.Font = New Font(ButtonCentroOn.Font, FontStyle.Regular)
        ButtonDerechoOn.Font = New Font(ButtonDerechoOn.Font, FontStyle.Regular)
        ButtonArribaOn.Font = New Font(ButtonArribaOn.Font, FontStyle.Regular)
        ButtonAbajoOn.Font = New Font(ButtonAbajoOn.Font, FontStyle.Regular)
        LabelBoton.Font = New Font(LabelBoton.Font, FontStyle.Regular)
        NumericUpDown1.Font = New Font(NumericUpDown1.Font, FontStyle.Regular)
    End Sub

#Region "Plantillas"
    Private Sub CargarPlantillas()
        Dim d As New System.IO.DirectoryInfo(".")
        For Each f As System.IO.FileInfo In d.GetFiles("*.kbp")
            vtSelPlantilla.Items.Add(Mid(f.Name, 1, f.Name.Length - 4))
        Next
        If vtSelPlantilla.Items.Count > 0 Then
            If vtSelPlantilla.Items.Count > EditorMacros.ultimaPlantilla Then
                vtSelPlantilla.SelectedIndex = EditorMacros.ultimaPlantilla
                CargarPlantilla(EditorMacros.ultimaPlantilla)
            Else
                Dim idx As Integer = 0
                Select Case (System.Globalization.CultureInfo.CurrentCulture.LCID And &HFF)
                    Case &HC
                        If vtSelPlantilla.Items.IndexOf("french") <> -1 Then idx = vtSelPlantilla.Items.IndexOf("french")
                    Case &H10
                        If vtSelPlantilla.Items.IndexOf("italian") <> -1 Then idx = vtSelPlantilla.Items.IndexOf("italian")
                    Case &HA
                        If vtSelPlantilla.Items.IndexOf("spanish-es") <> -1 Then idx = vtSelPlantilla.Items.IndexOf("spanish-es")
                    Case &H7
                        If vtSelPlantilla.Items.IndexOf("german") <> -1 Then idx = vtSelPlantilla.Items.IndexOf("german")
                    Case Else
                        If vtSelPlantilla.Items.IndexOf("english-us") <> -1 Then idx = vtSelPlantilla.Items.IndexOf("english-us")
                End Select
                vtSelPlantilla.SelectedIndex = idx
                CargarPlantilla(idx)
            End If
        Else
            ButtonPresionar.Enabled = False
            ButtonSoltar.Enabled = False
        End If
    End Sub
    Private Sub CargarPlantilla(ByVal p As Byte)
        ComboBox1.Items.Clear()
        Dim f As System.IO.StreamReader
        Try
            f = New System.IO.StreamReader(CType(vtSelPlantilla.Items.Item(vtSelPlantilla.SelectedIndex), String) & ".kbp")
        Catch
            Exit Sub
        End Try
        Try
            Do While f.Peek() >= 0
                ComboBox1.Items.Add(f.ReadLine())
            Loop
            ComboBox1.SelectedIndex = 0
        Catch
            Try
                f.Close()
            Catch
            End Try
            Exit Sub
        End Try
        Try
            f.Close()
        Catch
        End Try
    End Sub
#End Region

    Private Sub Guardar()
        If Me.TextBoxNombre.Text.Trim() = "" Then Exit Sub
        If indicep = -1 Then
            Dim idx As Integer = padre.ComboBoxMacro.Items.IndexOf(Me.TextBoxNombre.Text.Trim())
            If idx = -1 Then 'no existe
                If CheckBox1.Checked Then 'texto x52
                    Dim st As String = Me.TextBoxNombre.Text.Trim()
                    Dim stb(32) As Byte
                    WideCharToMultiByte(20127, 0, st, st.Length, stb, 33, 0, 0)
                    macro.Insert(0, 56)
                    If st.Length > 16 Then
                        For j As Short = 15 To 0 Step -1
                            macro.Insert(0, 24 + (CInt(stb(j)) << 8))
                        Next
                    Else
                        For j As Short = st.Length - 1 To 0 Step -1
                            macro.Insert(0, 24 + (CInt(stb(j)) << 8))
                        Next
                    End If
                    macro.Insert(0, 24 + (3 << 8))
                End If
                idx = padre.ComboBoxAssigned.Items.Add(Me.TextBoxNombre.Text.Trim())
                padre.ComboBoxMacro.Items.Add(Me.TextBoxNombre.Text.Trim())
                padre.datos.AñadirIndice(idx - 1, macro)
                Me.Tag = idx - 1
                Me.DialogResult = Windows.Forms.DialogResult.Ignore
            Else
                Traduce.Msg("name_repeated", "warning", MsgBoxStyle.Exclamation)
                Exit Sub
            End If
        Else
            If CheckBox1.Checked Then 'texto x52
                Dim st As String = Me.TextBoxNombre.Text.Trim()
                Dim stb(32) As Byte
                WideCharToMultiByte(20127, 0, st, st.Length, stb, 33, 0, 0)
                macro.Insert(0, 56)
                If st.Length > 16 Then
                    For j As Short = 15 To 0 Step -1
                        macro.Insert(0, 24 + (CInt(stb(j)) << 8))
                    Next
                Else
                    For j As Short = st.Length - 1 To 0 Step -1
                        macro.Insert(0, 24 + (CInt(stb(j)) << 8))
                    Next
                End If
                macro.Insert(0, 24 + (3 << 8))
            End If
            Dim idx As Integer = padre.ComboBoxAssigned.Items.Add(Me.TextBoxNombre.Text.Trim())
            padre.ComboBoxMacro.Items.Add(Me.TextBoxNombre.Text.Trim())
            If idx <> (indicep + 1) Then
                padre.datos.AñadirIndice(idx - 1, macro)
                padre.datos.IntercambiarIndices(idx, indicep + 1, padre.ComboBoxAssigned, padre.ComboBoxMacro)
            Else
                padre.ComboBoxAssigned.Items.RemoveAt(idx + 1)
                padre.ComboBoxMacro.Items.RemoveAt(idx)
                padre.datos.GetMacro(indicep).Clear()
                For i As Integer = 0 To macro.Count - 1
                    padre.datos.GetMacro(indicep).Add(macro(i))
                Next
            End If
            If idx <= (indicep + 1) Then
                Me.Tag = idx - 1
            Else
                Me.Tag = idx - 2
            End If
            Me.DialogResult = Windows.Forms.DialogResult.OK
        End If

        Me.Close()
    End Sub

    Private Function GetIndice() As Integer
        If (ListBox1.SelectedIndex = -1) Or (ListBox1.SelectedIndex = (ListBox1.Items.Count - 1)) Then
            Return macro.Count
        Else
            Dim real As Integer = 0
            Dim ficticio As Integer = 0
            Dim tipo As Byte
            For real = 0 To macro.Count - 1
                If ficticio = ListBox1.SelectedIndex Then Exit For
                tipo = macro(real) And &HFF
                If tipo > 31 Then tipo = tipo - 32
                If tipo < 24 Then
                    ficticio = ficticio + 1
                ElseIf tipo = 24 Then
                    If macro(real) = 56 Then
                        ficticio = ficticio + 1
                    End If
                ElseIf tipo < 27 Then
                    ficticio = ficticio + 1
                    real = real + 2
                Else
                    ficticio = ficticio + 1
                    real = real + 1
                End If
            Next
            Return real
        End If
    End Function

#End Region

    Private Sub ButtonAcepta_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonAcepta.Click
        Guardar()
    End Sub

    Private Sub ButtonCancel_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonCancel.Click
        Me.DialogResult = Windows.Forms.DialogResult.Cancel
        Me.Close()
    End Sub
    Private Sub RadioButtonBasico_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles RadioButtonBasico.CheckedChanged
        If RadioButtonBasico.Checked Then
            PasarABasico()
        End If
    End Sub
    Private Sub RadioButtonAvanzado_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles RadioButtonAvanzado.CheckedChanged
        If RadioButtonAvanzado.Checked Then
            PasarAAvanzado()
        End If
    End Sub

#Region "comandos"
    Private Sub ButtonBorrar_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonBorrar.Click
        Borrar(False)
    End Sub
    Private Sub Borrar(ByVal noRepeats As Boolean)
        If ListBox1.SelectedIndex > -1 Then
            If RadioButtonAvanzado.Checked Then
                Dim idc As Integer = GetIndice()
                If ListBox1.SelectedIndex = (ListBox1.Items.Count - 1) Then
                    idc = idc - 1
                    If macro(idc) = 56 Then
                        Dim i As Integer
                        For i = idc - 1 To 0 Step -1
                            If (macro(i) And &HFF) <> 24 Then
                                idc = i + 1
                                Exit For
                            End If
                        Next
                        If i = -1 Then idc = 0
                    ElseIf (idc - 1) > -1 Then
                        If (macro(idc - 1) And &HFF) = 27 Then
                            idc = idc - 1
                        ElseIf (idc - 2) > -1 Then
                            If ((macro(idc - 2) And &HFF) = 26) Or ((macro(idc - 2) And &HFF) = 25) Then idc = idc - 2
                        End If
                    End If
                End If
                Dim tipo As Byte = macro(idc) And &HFF
                If Not noRepeats Then
                    If tipo = 12 Then
                        For i As Integer = idc To macro.Count - 1
                            If macro(i) = 44 Then macro.RemoveAt(i) : Exit For
                        Next
                    ElseIf tipo = 13 Then
                        For i As Integer = idc To macro.Count - 1
                            If macro(i) = 45 Then macro.RemoveAt(i) : Exit For
                        Next
                    ElseIf tipo = 44 Then
                        macro.RemoveAt(idc)
                        For i As Integer = idc - 1 To 0 Step -1
                            If macro(i) = 12 Then macro.RemoveAt(i) : Exit For
                        Next
                        Cargar()
                        Exit Sub
                    ElseIf tipo = 45 Then
                        macro.RemoveAt(idc)
                        For i As Integer = idc - 1 To 0 Step -1
                            If macro(i) = 13 Then macro.RemoveAt(i) : Exit For
                        Next
                        Cargar()
                        Exit Sub
                    End If
                End If
                If tipo > 31 Then tipo = tipo - 32
                If tipo < 24 Then
                    macro.RemoveAt(idc)
                ElseIf tipo = 24 Then
                    macro.RemoveAt(idc)
                    For i As Byte = 0 To 16
                        If macro(idc) = 56 Then Exit For
                        macro.RemoveAt(idc)
                    Next
                    macro.RemoveAt(idc)
                ElseIf tipo < 27 Then
                    macro.RemoveAt(idc)
                    macro.RemoveAt(idc)
                    macro.RemoveAt(idc)
                Else
                    macro.RemoveAt(idc)
                    macro.RemoveAt(idc)
                End If
                Cargar()
            Else
                macro.Clear()
                ListBox1.Items.Clear()
            End If
        Else
            If RadioButtonBasico.Checked Then
                macro.Clear()
                ListBox1.Items.Clear()
            End If
        End If
    End Sub
    Private Sub ButtonSubir_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonSubir.Click
        If (ListBox1.SelectedIndex > -1) And (ListBox1.SelectedIndex > 0) Then
            Dim idc1 As Integer = GetIndice()
            Dim sel As Integer = ListBox1.SelectedIndex
            If ListBox1.SelectedIndex = (ListBox1.Items.Count - 1) Then
                idc1 = idc1 - 1
                If macro(idc1) = 56 Then
                    Dim i As Integer
                    For i = idc1 - 1 To 0 Step -1
                        If (macro(i) And &HFF) <> 24 Then
                            idc1 = i + 1
                            Exit For
                        End If
                    Next
                ElseIf (idc1 - 1) > -1 Then
                    If (macro(idc1 - 1) And &HFF) = 27 Then
                        idc1 = idc1 - 1
                    ElseIf (idc1 - 2) > -1 Then
                        If ((macro(idc1 - 2) And &HFF) = 26) Or ((macro(idc1 - 2) And &HFF) = 25) Then idc1 = idc1 - 2
                    End If
                End If
            End If
            ListBox1.SelectedIndex = sel - 1
            Dim idc2 As Integer = GetIndice()
            If (macro(idc1) = 11 And macro(idc2) = 45) Or (macro(idc1) = 12 And macro(idc2) = 45) _
            Or (macro(idc1) = 13 And macro(idc2) = 11) Or (macro(idc1) = 13 And macro(idc2) = 12) Then
                Exit Sub
            End If
            If (macro(idc1) And 31) < 24 Then
                macro.Insert(idc2, macro(idc1))
            ElseIf (macro(idc1) And 31) = 24 Then
                For i As Byte = 0 To 17
                    macro.Insert(idc2 + i, macro(idc1 + (i * 2)))
                    If macro(idc2 + i) = 56 Then Exit For
                Next
            ElseIf (macro(idc1) And 31) < 27 Then
                macro.Insert(idc2, macro(idc1))
                macro.Insert(idc2 + 1, macro(idc1 + 2))
                macro.Insert(idc2 + 2, macro(idc1 + 4))
            Else
                macro.Insert(idc2, macro(idc1))
                macro.Insert(idc2 + 1, macro(idc1 + 2))
            End If
            Cargar()
            ListBox1.SelectedIndex = sel + 1
            Borrar(True)
        End If
    End Sub
    Private Sub ButtonBajar_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonBajar.Click
        If (ListBox1.SelectedIndex > -1) And (ListBox1.SelectedIndex < ListBox1.Items.Count - 1) Then
            ListBox1.SelectedIndex = ListBox1.SelectedIndex + 1
            ButtonSubir.PerformClick()
        End If
    End Sub
#End Region

#Region "Teclado"
    Private Sub vtSelPlantilla_SelectedItemChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles vtSelPlantilla.SelectedItemChanged
        CargarPlantilla(vtSelPlantilla.SelectedIndex)
    End Sub
    Private Sub ButtonPresionar_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonPresionar.Click
        If macro.Count > 237 Then Exit Sub
        macro.Insert(GetIndice(), ComboBox1.SelectedIndex << 8)
        Cargar()
    End Sub
    Private Sub ButtonSoltar_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonSoltar.Click
        If macro.Count > 237 Then Exit Sub
        macro.Insert(GetIndice(), 32 + (ComboBox1.SelectedIndex << 8))
        Cargar()
    End Sub
    Private Sub ButtonNormal_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonNormal.Click
        If CType(TextBoxTecla.Tag, ArrayList).Count = 0 Then Exit Sub
        If (macro.Count + (CType(TextBoxTecla.Tag, ArrayList).Count * 2)) > 237 Then Exit Sub

        If RadioButtonBasico.Checked Then
            macro.Clear()
            ListBox1.Items.Clear()
        End If

        Dim idc As Integer = GetIndice()
        Dim i As Byte
        For i = 0 To CType(TextBoxTecla.Tag, ArrayList).Count - 1
            Dim k As Integer = MapKey(CInt(CType(TextBoxTecla.Tag, ArrayList)(i)))
            If k > -1 Then macro.Insert(idc + i, k << 8)
        Next
        For j As Integer = CType(TextBoxTecla.Tag, ArrayList).Count - 1 To 0 Step -1
            Dim k As Integer = MapKey(CInt(CType(TextBoxTecla.Tag, ArrayList)(j)))
            If k > -1 Then
                macro.Insert(idc + i, 32 + (k << 8))
                i = i + 1
            End If
        Next
        Cargar()
    End Sub
    Private Sub ButtonTeclasRepetir_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonTeclasRepetir.Click
        If CType(TextBoxTecla.Tag, ArrayList).Count = 0 Then Exit Sub
        If (macro.Count + 1 + (CType(TextBoxTecla.Tag, ArrayList).Count * 2)) > 237 Then Exit Sub

        If RadioButtonBasico.Checked Then
            macro.Clear()
            ListBox1.Items.Clear()
        End If

        Dim idc As Integer = GetIndice()
        Dim i As Byte
        For i = 0 To CType(TextBoxTecla.Tag, ArrayList).Count - 1
            macro.Insert(idc + i, MapKey(CInt(CType(TextBoxTecla.Tag, ArrayList)(i))) << 8)
        Next
        macro.Insert(idc + i, 11)
        For j As Integer = CType(TextBoxTecla.Tag, ArrayList).Count - 1 To 0 Step -1
            i = i + 1
            macro.Insert(idc + i, 32 + (MapKey(CInt(CType(TextBoxTecla.Tag, ArrayList)(j))) << 8))
        Next
        Cargar()
    End Sub

#Region "lectura teclas"
    Private Sub TextBoxTecla_GotFocus(ByVal sender As Object, ByVal e As System.EventArgs) Handles TextBoxTecla.GotFocus
        CType(TextBoxTecla.Tag, ArrayList).Clear()
        TextBoxTecla.Text = ""
        TextBoxTecla.BackColor = Color.GreenYellow
        TextBoxTecla.ForeColor = Color.Black
        TextBoxTecla.Font = New Font(FontFamily.GenericSansSerif, 8.25, FontStyle.Bold)
        Timer1.Enabled = True
    End Sub
    Private Sub TextBoxTecla_LostFocus(ByVal sender As Object, ByVal e As System.EventArgs) Handles TextBoxTecla.LostFocus
        TextBoxTecla.Font = New Font(FontFamily.GenericSansSerif, 8.25, FontStyle.Regular)
        TextBoxTecla.BackColor = Color.Black
        TextBoxTecla.ForeColor = Color.GreenYellow
        Timer1.Enabled = False
    End Sub
    Private Sub Timer1_Tick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Timer1.Tick
        LeerTeclado()
    End Sub
    Private Declare Auto Function GetKeyboardState Lib "user32.dll" (ByVal buf() As Byte) As Byte
    Private Sub LeerTeclado()
        Dim buf(255) As Byte
        Dim s As String = ""
        If GetKeyboardState(buf) = 1 Then
            For i As Integer = 0 To CType(TextBoxTecla.Tag, ArrayList).Count - 1
                If buf(CType(TextBoxTecla.Tag, ArrayList)(i)) < 128 Then
                    ButtonNormal.Focus()
                    Exit Sub
                End If
            Next
            CType(TextBoxTecla.Tag, ArrayList).Clear()
            TextBoxTecla.Text = ""
            If (buf(16) And 128) And (Not (buf(&HA0) And 128)) And (Not (buf(&HA1) And 128)) Then
                s = "Shift"
                CType(TextBoxTecla.Tag, ArrayList).Add(16)
            End If
            If (buf(17) And 128) And (Not (buf(&HA2) And 128)) And (Not (buf(&HA3) And 128)) Then
                If s = "" Then s = "Control" Else s = s & " + Control"
                CType(TextBoxTecla.Tag, ArrayList).Add(17)
            End If
            If (buf(18) And 128) And (Not (buf(&HA4) And 128)) And (Not (buf(&HA5) And 128)) Then
                If s = "" Then s = "Alt" Else s = s & " + Alt"
                CType(TextBoxTecla.Tag, ArrayList).Add(18)
            End If
            If buf(&HA0) And 128 Then
                If s = "" Then s = "LShift" Else s = s & " + LShift"
                CType(TextBoxTecla.Tag, ArrayList).Add(&HA0)
            End If
            If buf(&HA1) And 128 Then
                If s = "" Then s = "RShift" Else s = s & " + RShift"
                CType(TextBoxTecla.Tag, ArrayList).Add(&HA1)
            End If
            If buf(&HA2) And 128 Then
                If s = "" Then s = "LControl" Else s = s & " + LControl"
                CType(TextBoxTecla.Tag, ArrayList).Add(&HA2)
            End If
            If buf(&HA3) And 128 Then
                If s = "" Then s = "RControl" Else s = s & " + RControl"
                CType(TextBoxTecla.Tag, ArrayList).Add(&HA3)
            End If
            If buf(&HA4) And 128 Then
                If s = "" Then s = "LAlt" Else s = s & " + LAlt"
                CType(TextBoxTecla.Tag, ArrayList).Add(&HA4)
            End If
            If buf(&HA5) And 128 Then
                If s = "" Then s = "RAlt" Else s = s & " + RAlt"
                CType(TextBoxTecla.Tag, ArrayList).Add(&HA5)
            End If
            If buf(&H5B) And 128 Then
                If s = "" Then s = "LWin" Else s = s & " + LWin"
                CType(TextBoxTecla.Tag, ArrayList).Add(&H5B)
            End If
            If buf(&H5C) And 128 Then
                If s = "" Then s = "RWin" Else s = s & " + RWin"
                CType(TextBoxTecla.Tag, ArrayList).Add(&H5C)
            End If
            For i As UInt16 = 7 To 255
                If (i < 16 Or i > 18) And i <> &H5B And i <> &H5C And (i < &HA0 Or i > &HA5) Then
                    If (buf(i) And 128) Then
                        If s = "" Then s = CType(i, Keys).ToString() Else s = s & " + " & CType(i, Keys).ToString()
                        CType(TextBoxTecla.Tag, ArrayList).Add(i)
                    End If
                End If
            Next
            TextBoxTecla.Text = s
        End If
    End Sub

    Private Declare Auto Function MapVirtualKey Lib "user32.dll" (ByVal key As UInteger, ByVal tipo As UInteger) As UInteger
    Private Function MapKey(ByVal key As Integer) As Integer
        Dim sc As UInteger = MapVirtualKey(key, 0)
        'especiales 0xE0
        Select Case key
            Case 3 ' crtl+break
                Return 72
            Case &H13 'pausa
                Return 72
            Case &H2D 'insert
                Return &H49
            Case &H60 ' kp 0
                Return &H62
            Case &H24 'home
                Return &H4A
            Case &H67 'kp 7
                Return &H5F
            Case &H21 'pg up
                Return &H4B
            Case &H69 'kp 9
                Return &H62
            Case &H2E
                Return &H4C 'delete
            Case &H6E 'kp.
                Return &H63
            Case &H23 'end
                Return &H4D
            Case &H61 'kp 1
                Return &H59
            Case &H22 'pg down
                Return &H4E
            Case &H63 'kp 3
                Return &H5B
            Case &H27 ' derecha
                Return &H4F
            Case &H66 'kp 6
                Return &H5E
            Case &H25 'left
                Return &H50
            Case &H64 'hp 4
                Return &H5C
            Case &H28 'down
                Return &H51
            Case &H62 'kp 2
                Return &H5A
            Case &H26  'up
                Return &H52
            Case &H68 'kp 8
                Return &H60
            Case &H6F 'kp/
                Return &H54
                'Case &HBD ' /? -
                '    Return &H38
            Case &H5D 'app
                Return &H65
            Case &HF9 '??
                Return -1 '&H65
            Case &H5B 'left win
                Return &HE3
            Case &HF1 '???
                Return -1 '&HE3
            Case &H11 ' ctrl
                Return &HE0
            Case &HA2 ' left control
                Return &HE0
            Case &HA3 'right control
                Return &HE4
            Case &H12 'alt
                Return &HE2
            Case &HA4 'left alt
                Return &HE2
            Case &HA5 'right alt
                Return &HE6
            Case &H5C 'right win
                Return &HE7
            Case &HEA '??
                Return -1 '&he7
            Case &H5E 'power
                Return &H66
            Case &HD ' enter
                Return &H28
            Case &H2C 'print
                Return &H46
            Case &H6A ' kp *
                Return &H55
            Case &H91 'scroll lock
                Return 71
        End Select
        If sc = 0 Then
            Return -1
        Else
            Select Case sc
                Case &HFF
                    Return &H1
                Case &HFC
                    Return &H2
                Case &H1E
                    Return 4
                Case &H30
                    Return 5
                Case &H2E
                    Return 6
                Case &H20
                    Return 7
                Case &H12
                    Return 8
                Case &H21
                    Return 9
                Case &H22
                    Return &HA
                Case &H23
                    Return &HB
                Case &H17
                    Return &HC
                Case &H24
                    Return &HD
                Case &H25
                    Return &HE
                Case &H26
                    Return &HF
                Case &H32
                    Return &H10
                Case &H31
                    Return &H11
                Case &H18
                    Return &H12
                Case &H19
                    Return &H13
                Case &H10
                    Return &H14
                Case &H13
                    Return &H15
                Case &H1F
                    Return &H16
                Case &H14
                    Return &H17
                Case &H16
                    Return &H18
                Case &H2F
                    Return &H19
                Case &H11
                    Return &H1A
                Case &H2D
                    Return &H1B
                Case &H15
                    Return &H1C
                Case &H2C
                    Return &H1D
                Case &H2
                    Return &H1E
                Case &H3
                    Return &H1F
                Case &H4
                    Return &H20
                Case &H5
                    Return &H21
                Case &H6
                    Return &H22
                Case &H7
                    Return &H23
                Case &H8
                    Return &H24
                Case &H9
                    Return &H25
                Case &HA
                    Return &H26
                Case &HB
                    Return &H27
                Case &H1C
                    Return &H28
                Case &H1
                    Return &H29
                Case &HE
                    Return &H2A
                Case &HF
                    Return &H2B
                Case &H39
                    Return &H2C
                Case &HC
                    Return &H2D
                Case &HD
                    Return &H2E
                Case &H1A
                    Return &H2F
                Case &H1B
                    Return &H30
                Case &H2B
                    Return &H31
                Case &H2B
                    Return &H32
                Case &H27
                    Return &H33
                Case &H28
                    Return &H34
                Case &H29
                    Return &H35
                Case &H33
                    Return &H36
                Case &H34
                    Return &H37
                Case &H35
                    Return &H38
                Case &H3A
                    Return &H39
                Case &H3B
                    Return &H3A
                Case &H3C
                    Return &H3B
                Case &H3D
                    Return &H3C
                Case &H3E
                    Return &H3D
                Case &H3F
                    Return &H3E
                Case &H40
                    Return &H3F
                Case &H41
                    Return &H40
                Case &H42
                    Return &H41
                Case &H43
                    Return &H42
                Case &H44
                    Return &H43
                Case &H57
                    Return &H44
                Case &H58
                    Return &H45
                Case &H45
                    Return &H53
                Case &H37
                    Return &H55
                Case &H4A
                    Return &H56
                Case &H4E
                    Return &H57
                Case &H4F
                    Return &H59
                Case &H50
                    Return &H5A
                Case &H51
                    Return &H5B
                Case &H4B
                    Return &H5C
                Case &H4C
                    Return &H5D
                Case &H4D
                    Return &H5E
                Case &H47
                    Return &H5F
                Case &H48
                    Return &H60
                Case &H49
                    Return &H61
                Case &H52
                    Return &H62
                Case &H53
                    Return &H63
                Case &H56
                    Return &H64
                Case &H59
                    Return &H67
                Case &H64
                    Return &H68
                Case &H65
                    Return &H69
                Case &H66
                    Return &H6A
                Case &H67
                    Return &H6B
                Case &H68
                    Return &H6C
                Case &H69
                    Return &H6D
                Case &H6A
                    Return &H6E
                Case &H6B
                    Return &H6F
                Case &H6C
                    Return &H70
                Case &H6D
                    Return &H71
                Case &H6E
                    Return &H72
                Case &H76
                    Return &H73
                Case &H73
                    Return &H87
                Case &H70
                    Return &H88
                Case &H7D
                    Return &H89
                Case &H79
                    Return &H8A
                Case &H7B
                    Return &H8B
                Case &H5C
                    Return &H8C
                Case &HF2
                    Return &H90
                Case &HF1
                    Return &H91
                Case &H78
                    Return &H92
                Case &H77
                    Return &H93
                Case &H76
                    Return &H94
                Case &H1D
                    Return &HE0
                Case &H2A
                    Return &HE1
                Case &H38
                    Return &HE2
                Case &H36
                    Return &HE5
                Case Else
                    Return -1
            End Select
            Return -1
        End If
        'Select Case key
        '    Case 8
        '        Return 42
        '    Case 9
        '        Return 43
        '    Case 12
        '        Return 156
        '    Case 13
        '        Return 40 '158
        '    Case 16
        '        Return 225
        '    Case 17
        '        Return 224
        '    Case 18
        '        Return 226
        '    Case 19
        '        Return 72
        '    Case 20
        '        Return 57
        '    Case 27
        '        Return 41
        '    Case 32
        '        Return 44
        '    Case 33
        '        Return 75
        '    Case 34
        '        Return 78
        '    Case 35
        '        Return 77
        '    Case 36
        '        Return 74
        '    Case 37
        '        Return 80
        '    Case 38
        '        Return 82
        '    Case 39
        '        Return 79
        '    Case 40
        '        Return 81
        '    Case 41
        '        Return 119
        '    Case 42
        '        Return -1 'desconocido
        '    Case 43
        '        Return 116
        '    Case 44
        '        Return 70
        '    Case 45
        '        Return 73
        '    Case 46
        '        Return 76
        '    Case 47
        '        Return 117
        '    Case 48
        '        Return 39
        '    Case 49
        '        Return 30
        '    Case 50
        '        Return 31
        '    Case 51
        '        Return 32
        '    Case 52
        '        Return 33
        '    Case 53
        '        Return 34
        '    Case 54
        '        Return 35
        '    Case 55
        '        Return 36
        '    Case 56
        '        Return 37
        '    Case 57
        '        Return 38
        '    Case 65
        '        Return 4
        '    Case 66
        '        Return 5
        '    Case 67
        '        Return 6
        '    Case 68
        '        Return 7
        '    Case 69
        '        Return 8
        '    Case 70
        '        Return 9
        '    Case 71
        '        Return 10
        '    Case 72
        '        Return 11
        '    Case 73
        '        Return 12
        '    Case 74
        '        Return 13
        '    Case 75
        '        Return 14
        '    Case 76
        '        Return 15
        '    Case 77
        '        Return 16
        '    Case 78
        '        Return 17
        '    Case 79
        '        Return 18
        '    Case 80
        '        Return 19
        '    Case 81
        '        Return 20
        '    Case 82
        '        Return 21
        '    Case 83
        '        Return 22
        '    Case 84
        '        Return 23
        '    Case 85
        '        Return 24
        '    Case 86
        '        Return 25
        '    Case 87
        '        Return 26
        '    Case 88
        '        Return 27
        '    Case 89
        '        Return 28
        '    Case 90
        '        Return 29
        '    Case 91
        '        Return 227
        '    Case 92
        '        Return 231
        '    Case 93
        '        Return 101
        '    Case 95
        '        Return 102
        '    Case 96
        '        Return 98
        '    Case 97
        '        Return 89
        '    Case 98
        '        Return 90
        '    Case 99
        '        Return 91
        '    Case 100
        '        Return 92
        '    Case 101
        '        Return 93
        '    Case 102
        '        Return 94
        '    Case 103
        '        Return 95
        '    Case 104
        '        Return 96
        '    Case 105
        '        Return 97
        '    Case 106
        '        Return 85
        '    Case 107
        '        Return 87
        '    Case 108
        '        Return -1 'desconocido
        '    Case 109
        '        Return 86
        '    Case 110
        '        Return 99
        '    Case 111
        '        Return 84
        '    Case 112
        '        Return 58
        '    Case 113
        '        Return 59
        '    Case 114
        '        Return 60
        '    Case 115
        '        Return 61
        '    Case 116
        '        Return 62
        '    Case 117
        '        Return 63
        '    Case 118
        '        Return 64
        '    Case 119
        '        Return 65
        '    Case 120
        '        Return 66
        '    Case 121
        '        Return 67
        '    Case 122
        '        Return 68
        '    Case 123
        '        Return 69
        '    Case 124
        '        Return 104
        '    Case 125
        '        Return 105
        '    Case 126
        '        Return 106
        '    Case 127
        '        Return 107
        '    Case 128
        '        Return 108
        '    Case 129
        '        Return 109
        '    Case 130
        '        Return 110
        '    Case 131
        '        Return 111
        '    Case 132
        '        Return 112
        '    Case 133
        '        Return 113
        '    Case 134
        '        Return 114
        '    Case 135
        '        Return 115
        '    Case 144
        '        Return 83
        '    Case 145
        '        Return 71
        '    Case 146
        '        Return -1 'desconocido
        '    Case 147
        '        Return -1 'desconocido
        '    Case 148
        '        Return -1 'desconocido
        '    Case 149
        '        Return -1 'desconocido
        '    Case 150
        '        Return -1 'desconocido
        '    Case 160
        '        Return 225
        '    Case 161
        '        Return 229
        '    Case 162
        '        Return 224
        '    Case 163
        '        Return 228
        '    Case 164
        '        Return 226
        '    Case 165
        '        Return 230
        '    Case 166
        '        Return -1 'desconocido
        '    Case 167
        '        Return -1 'desconocido
        '    Case 168
        '        Return -1 'desconocido
        '    Case 169
        '        Return -1 'desconocido
        '    Case 170
        '        Return -1 'desconocido
        '    Case 171
        '        Return -1 'desconocido
        '    Case 172
        '        Return -1 'desconocido
        '    Case 173
        '        Return 127
        '    Case 174
        '        Return 129
        '    Case 175
        '        Return 128
        '    Case 176
        '        Return -1 'desconocido
        '    Case 177
        '        Return -1 'desconocido
        '    Case 178
        '        Return -1 'desconocido
        '    Case 179
        '        Return -1 'desconocido
        '    Case 180
        '        Return -1 'desconocido
        '    Case 181
        '        Return -1 'desconocido
        '    Case 182
        '        Return -1 'desconocido
        '    Case 183
        '        Return -1 'desconocido
        '    Case 186
        '        Return 47
        '    Case 187
        '        Return 48
        '    Case 188
        '        Return 54
        '    Case 189
        '        Return 56
        '    Case 190
        '        Return 55
        '    Case 191
        '        Return 49
        '    Case 192
        '        Return 51
        '    Case 219
        '        Return 45
        '    Case 220
        '        Return 53
        '    Case 221
        '        Return 46
        '    Case 222
        '        Return 52
        '    Case 223
        '        Return -1 'desconocido
        '    Case 226
        '        Return 100
        '    Case 246
        '        Return 154
        '    Case 247
        '        Return 163
        '    Case 248
        '        Return 164
        '    Case 249
        '        Return 153
        '    Case Else
        '        Return -1
        'End Select
    End Function
#End Region
#End Region

#Region "Modos"
    Private Sub ButtonModo1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonModo1.Click
        If macro.Count > 237 Then Exit Sub
        macro.Insert(GetIndice(), 14)
        Cargar()
    End Sub
    Private Sub ButtonModo2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonModo2.Click
        If macro.Count > 237 Then Exit Sub
        macro.Insert(GetIndice(), 14 + (1 << 8))
        Cargar()
    End Sub
    Private Sub ButtonModo3_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonModo3.Click
        If macro.Count > 237 Then Exit Sub
        macro.Insert(GetIndice(), 14 + (2 << 8))
        Cargar()
    End Sub
    Private Sub ButtonPinkieOn_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonPinkieOn.Click
        If macro.Count > 237 Then Exit Sub
        macro.Insert(GetIndice(), 16 + (1 << 8))
        Cargar()
    End Sub
    Private Sub ButtonPinkieOff_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonPinkieOff.Click
        If macro.Count > 237 Then Exit Sub
        macro.Insert(GetIndice(), 16)
        Cargar()
    End Sub
    Private Sub ButtonAux1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonAux1.Click
        If macro.Count > 237 Then Exit Sub
        macro.Insert(GetIndice(), 15)
        Cargar()
    End Sub
    Private Sub ButtonAux2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonAux2.Click
        If macro.Count > 237 Then Exit Sub
        macro.Insert(GetIndice(), 15 + (1 << 8))
        Cargar()
    End Sub
    Private Sub ButtonAux3_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonAux3.Click
        If macro.Count > 237 Then Exit Sub
        macro.Insert(GetIndice(), 15 + (2 << 8))
        Cargar()
    End Sub
#End Region

#Region "especial"
    Private Sub ButtonMantener_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonMantener.Click
        If macro.Count > 237 Then Exit Sub

        Dim idc As Integer = GetIndice()
        Dim reps As Integer = 0
        For i As Integer = 0 To idc - 1
            If macro(i) = 12 Or macro(i) = 11 Then
                Traduce.Msg("hold_and_repeat_in_macro", "warning", MsgBoxStyle.Information)
                Exit Sub
            End If
            If macro(i) = 13 Then reps = reps + 1
            If macro(i) = 45 Then reps = reps - 1
        Next
        If reps = 0 Then
            macro.Insert(idc, 11)
            Cargar()
        Else
            Traduce.Msg("cannot_be_inside_repeatn", "warning", MsgBoxStyle.Information)
        End If
    End Sub
    Private Sub ButtonRepetir_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonRepetir.Click
        If macro.Count > 236 Then Exit Sub

        Dim idc As Integer = GetIndice()
        Dim reps As Integer = 0
        For i As Integer = 0 To idc - 1
            If macro(i) = 12 Or macro(i) = 11 Then
                Traduce.Msg("hold_and_repeat_in_macro", "warning", MsgBoxStyle.Information)
                Exit Sub
            End If
            If macro(i) = 13 Then reps = reps + 1
            If macro(i) = 45 Then reps = reps - 1
        Next
        If reps = 0 Then
            macro.Insert(idc, 12)
            macro.Insert(idc + 1, 44)
            Cargar()
        Else
            Traduce.Msg("cannot_be_inside_repeatn", "warning", MsgBoxStyle.Information)
        End If
    End Sub
    Private Sub ButtonPausa_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonPausa.Click
        If macro.Count > 237 Then Exit Sub
        macro.Insert(GetIndice, 10 + (NumericUpDown6.Value << 8))
        Cargar()
    End Sub
    Private Sub ButtonRepetirN_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonRepetirN.Click
        If macro.Count > 236 Then Exit Sub

        Dim idc As Integer = GetIndice()
        macro.Insert(idc, 13 + (NumericUpDown4.Value << 8))
        macro.Insert(idc + 1, 45)
        Cargar()
    End Sub
#End Region

#Region "ratón"
    Private Sub ButtonIzquierdoOn_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonIzquierdoOn.Click
        If RadioButtonBasico.Checked Then
            macro.Clear()
            ListBox1.Items.Clear()
            macro.Add(1)
            macro.Add(11)
            macro.Add(33)
        Else
            If macro.Count > 237 Then Exit Sub
            macro.Insert(GetIndice(), 1)
        End If
        Cargar()
    End Sub
    Private Sub ButtonCentroOn_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonCentroOn.Click
        If RadioButtonBasico.Checked Then
            macro.Clear()
            ListBox1.Items.Clear()
            macro.Add(3)
            macro.Add(11)
            macro.Add(35)
        Else
            If macro.Count > 237 Then Exit Sub
            macro.Insert(GetIndice(), 3)
        End If
        Cargar()
    End Sub
    Private Sub ButtonDerechoOn_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonDerechoOn.Click
        If RadioButtonBasico.Checked Then
            macro.Clear()
            ListBox1.Items.Clear()
            macro.Add(2)
            macro.Add(11)
            macro.Add(34)
        Else
            If macro.Count > 237 Then Exit Sub
            macro.Insert(GetIndice(), 2)
        End If
        Cargar()
    End Sub
    Private Sub ButtonArribaOn_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonArribaOn.Click
        If RadioButtonBasico.Checked Then
            macro.Clear()
            ListBox1.Items.Clear()
            macro.Add(8)
            'macro.Add(11)
            macro.Add(40)
        Else
            If macro.Count > 237 Then Exit Sub
            macro.Insert(GetIndice(), 8)
        End If
        Cargar()
    End Sub
    Private Sub ButtonAbajoOn_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonAbajoOn.Click
        If RadioButtonBasico.Checked Then
            macro.Clear()
            ListBox1.Items.Clear()
            macro.Add(9)
            'macro.Add(11)
            macro.Add(41)
        Else
            If macro.Count > 237 Then Exit Sub
            macro.Insert(GetIndice(), 9)
        End If
        Cargar()
    End Sub
    Private Sub ButtonIzquierdoOff_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonIzquierdoOff.Click
        If macro.Count > 237 Then Exit Sub
        macro.Insert(GetIndice, 33)
        Cargar()
    End Sub
    Private Sub ButtonCentroOff_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonCentroOff.Click
        If macro.Count > 237 Then Exit Sub
        macro.Insert(GetIndice, 35)
        Cargar()
    End Sub
    Private Sub ButtonDerechoOff_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonDerechoOff.Click
        If macro.Count > 237 Then Exit Sub
        macro.Insert(GetIndice, 34)
        Cargar()
    End Sub
    Private Sub ButtonArribaOff_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonArribaOff.Click
        If macro.Count > 237 Then Exit Sub
        macro.Insert(GetIndice, 40)
        Cargar()
    End Sub
    Private Sub ButtonAbajoOff_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonAbajoOff.Click
        If macro.Count > 237 Then Exit Sub
        macro.Insert(GetIndice, 41)
        Cargar()
    End Sub
    Private Sub ButtonMovIzquierda_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonMovIzquierda.Click
        If macro.Count > 237 Then Exit Sub
        macro.Insert(GetIndice(), 4 + (NumericUpDownSensibilidad.Value << 8))
        Cargar()
    End Sub
    Private Sub ButtonMovArriba_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonMovArriba.Click
        If macro.Count > 237 Then Exit Sub
        macro.Insert(GetIndice(), 6 + (NumericUpDownSensibilidad.Value << 8))
        Cargar()
    End Sub
    Private Sub ButtonMovDerecha_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonMovDerecha.Click
        If macro.Count > 237 Then Exit Sub
        macro.Insert(GetIndice(), 5 + (NumericUpDownSensibilidad.Value << 8))
        Cargar()
    End Sub
    Private Sub ButtonMovAbajo_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonMovAbajo.Click
        If macro.Count > 237 Then Exit Sub
        macro.Insert(GetIndice(), 7 + (NumericUpDownSensibilidad.Value << 8))
        Cargar()
    End Sub
#End Region

#Region "Directx"
    Private Sub ButtonDXOn_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonDXOn.Click
        If RadioButtonBasico.Checked Then
            macro.Clear()
            ListBox1.Items.Clear()
            macro.Add(18 + ((NumericUpDown1.Value - 1) << 8))
            macro.Add(11)
            macro.Add(50 + ((NumericUpDown1.Value - 1) << 8))
        Else
            If macro.Count > 237 Then Exit Sub
            macro.Insert(GetIndice(), 18 + ((NumericUpDown1.Value - 1) << 8))
        End If
        Cargar()
    End Sub
    Private Sub ButtonDXOff_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonDXOff.Click
        If macro.Count > 237 Then Exit Sub
        macro.Insert(GetIndice(), 50 + ((NumericUpDown1.Value - 1) << 8))
        Cargar()
    End Sub
    Private Sub ButtonPovOn_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonPovOn.Click
        If macro.Count > 237 Then Exit Sub
        macro.Insert(GetIndice(), 19 + ((((NumericUpDownPov.Value - 1) * 8) + (NumericUpDownPosicion.Value - 1)) << 8))
        Cargar()
    End Sub
    Private Sub ButtonPovOff_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonPovOff.Click
        If macro.Count > 237 Then Exit Sub
        macro.Insert(GetIndice(), 51 + ((((NumericUpDownPov.Value - 1) * 8) + (NumericUpDownPosicion.Value - 1)) << 8))
        Cargar()
    End Sub
#End Region

#Region "X52"
    Private Sub ButtonLinea_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonLinea.Click
        If (macro.Count + 2 + TextBox3.Text.Length) > 237 Then Exit Sub

        Dim idc As Integer = GetIndice()
        macro.Insert(idc, 24 + (NumericUpDown9.Value << 8))
        Dim stb(16) As Byte
        WideCharToMultiByte(20127, 0, TextBox3.Text, TextBox3.Text.Length, stb, 17, 0, 0)
        Dim i As Byte
        For i = 0 To TextBox3.Text.Length - 1
            macro.Insert(idc + i + 1, 24 + (CInt(stb(i)) << 8))
        Next
        macro.Insert(idc + i + 1, 56)
        Cargar()
    End Sub
    Private Sub Button47_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button47.Click
        If macro.Count > 235 Then Exit Sub

        Dim idc As Integer = GetIndice()
        macro.Insert(idc, 26 + (NumericUpDown7.Value << 8))
        If NumericUpDown7.Value = 1 Then
            macro.Insert(idc + 1, 26 + (NumericUpDown10.Value << 8))
            macro.Insert(idc + 2, 26 + (NumericUpDown11.Value << 8))
        Else
            macro.Insert(idc + 1, 26 + ((((NumericUpDown10.Value * 60) + NumericUpDown11.Value) \ 256) << 8))
            macro.Insert(idc + 2, 26 + ((((NumericUpDown10.Value * 60) + NumericUpDown11.Value) Mod 256) << 8))
        End If
        Cargar()
    End Sub
    Private Sub Button48_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button48.Click
        If macro.Count > 235 Then Exit Sub

        Dim idc As Integer = GetIndice()
        macro.Insert(idc, 25 + (NumericUpDown7.Value << 8))
        If NumericUpDown7.Value = 1 Then
            macro.Insert(idc + 1, 25 + (NumericUpDown10.Value << 8))
            macro.Insert(idc + 2, 25 + (NumericUpDown11.Value << 8))
        Else
            macro.Insert(idc + 1, 25 + ((((NumericUpDown10.Value * 60) + NumericUpDown11.Value) \ 256) << 8))
            macro.Insert(idc + 2, 25 + ((((NumericUpDown10.Value * 60) + NumericUpDown11.Value) Mod 256) << 8))
        End If
        Cargar()
    End Sub
    Private Sub ButtonX52PinkieOn_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonX52PinkieOn.Click
        If macro.Count > 237 Then Exit Sub
        macro.Insert(GetIndice(), 23)
        Cargar()
    End Sub
    Private Sub ButtonX52PinkieOff_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonX52PinkieOff.Click
        If macro.Count > 237 Then Exit Sub
        macro.Insert(GetIndice(), 23 + (1 << 8))
        Cargar()
    End Sub
    Private Sub ButtonFecha1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonFecha1.Click
        If macro.Count > 236 Then Exit Sub

        Dim idc As Integer = GetIndice()
        macro.Insert(idc, 27 + (1 << 8))
        macro.Insert(idc + 1, 27 + (NumericUpDown13.Value << 8))
        Cargar()
    End Sub
    Private Sub ButtonFecha2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonFecha2.Click
        If macro.Count > 236 Then Exit Sub

        Dim idc As Integer = GetIndice()
        macro.Insert(idc, 27 + (2 << 8))
        macro.Insert(idc + 1, 27 + (NumericUpDown13.Value << 8))
        Cargar()
    End Sub
    Private Sub ButtonFecha3_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonFecha3.Click
        If macro.Count > 236 Then Exit Sub

        Dim idc As Integer = GetIndice()
        macro.Insert(idc, 27 + (3 << 8))
        macro.Insert(idc + 1, 27 + (NumericUpDown13.Value << 8))
        Cargar()
    End Sub
    Private Sub ButtonInfoOn_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonInfoOn.Click
        If macro.Count > 237 Then Exit Sub
        macro.Insert(GetIndice(), 22)
        Cargar()
    End Sub
    Private Sub ButtonInfoOff_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonInfoOff.Click
        If macro.Count > 237 Then Exit Sub
        macro.Insert(GetIndice(), 22 + (1 << 8))
        Cargar()
    End Sub
    Private Sub ButtonLuzMfd_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonLuzMfd.Click
        If macro.Count > 237 Then Exit Sub
        macro.Insert(GetIndice(), 20 + (NumericUpDownLuzMfd.Value << 8))
        Cargar()
    End Sub
    Private Sub ButtonLuzBotones_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonLuzBotones.Click
        If macro.Count > 237 Then Exit Sub
        macro.Insert(GetIndice(), 21 + (NumericUpDownLuzMfd.Value << 8))
        Cargar()
    End Sub

#End Region

End Class