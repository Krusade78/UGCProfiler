Public Class VistaReport
    Private padre As Principal

    Private Sub VistaReport_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Me.padre = CType(Me.Parent, Principal)
        Me.Top = padre.Fondo.Top
        Me.Left = 0
        Traducir()
        Cargar()
    End Sub

    Private Sub Traducir()
        'modos
        If padre.ToolStripSplitAux.DropDownItems.Count = 3 Then ' hay aux
            For a As Byte = 0 To 2
                Dim sta As String = padre.ToolStripSplitAux.DropDownItems.Item(a).Text
                If padre.ToolStripSplitModo.DropDownItems.Count = 3 Then ' hay modos
                    For m As Byte = 0 To 2
                        Dim stm As String = sta & " + " & padre.ToolStripSplitModo.DropDownItems.Item(m).Text
                        ListView1.Columns.Add(stm, 100)
                        If padre.ToolStripSplitPinkie.DropDownItems.Count = 2 Then 'hay pk
                            ListView1.Columns.Add(stm & " + " & padre.ToolStripSplitPinkie.DropDownItems.Item(0).Text.Substring(0, padre.ToolStripSplitPinkie.Text.Length - 3), 100)
                        End If
                    Next
                Else
                    ListView1.Columns.Add(sta, 100)
                    If padre.ToolStripSplitPinkie.DropDownItems.Count = 2 Then 'hay pk
                        ListView1.Columns.Add(sta & " + " & padre.ToolStripSplitPinkie.DropDownItems.Item(0).Text.Substring(0, padre.ToolStripSplitPinkie.Text.Length - 3), 100)
                    End If
                End If
            Next
        Else
            If padre.ToolStripSplitModo.DropDownItems.Count = 3 Then ' hay modos
                For m As Byte = 0 To 2
                    Dim stm As String = padre.ToolStripSplitModo.DropDownItems.Item(m).Text
                    ListView1.Columns.Add(stm, 95)
                    If padre.ToolStripSplitPinkie.DropDownItems.Count = 2 Then 'hay pk
                        ListView1.Columns.Add(stm & " + " & padre.ToolStripSplitPinkie.DropDownItems.Item(0).Text.Substring(0, padre.ToolStripSplitPinkie.Text.Length - 3), 100)
                    End If
                Next
            Else
                If padre.ToolStripSplitPinkie.DropDownItems.Count = 2 Then 'hay pk
                    ListView1.Columns.Add(padre.ToolStripSplitPinkie.DropDownItems.Item(0).Text, 315)
                    ListView1.Columns.Add(padre.ToolStripSplitPinkie.DropDownItems.Item(1).Text, 315)
                Else
                    ListView1.Columns.Add(Traduce.Txt("no_modes"), 640)
                End If
            End If
        End If
        'joy
        ListView1.Items.Add(Traduce.Txt("joystick")).BackColor = Color.DarkSlateGray
        ListView1.Items.Add(Traduce.Txt("axisx"))
        ListView1.Items.Add(Traduce.Txt("axisy"))
        If (padre.datos.GetTipoHotas() = 3) Then
            ListView1.Items.Add(Traduce.Txt("axisr"))
            ListView1.Items.Add(Traduce.Txt("trigger1"))
            ListView1.Items.Add(Traduce.Txt("trigger2"))
        Else
            ListView1.Items.Add(Traduce.Txt("trigger"))
        End If
        ListView1.Items.Add(Traduce.Txt("buttonlaunch"))
        ListView1.Items.Add(Traduce.Txt("buttona"))
        ListView1.Items.Add(Traduce.Txt("buttonb"))
        ListView1.Items.Add(Traduce.Txt("buttonc"))
        ListView1.Items.Add(Traduce.Txt("pinkie"))
        ListView1.Items.Add(Traduce.Txt("pov1_n"))
        ListView1.Items.Add(Traduce.Txt("pov1_ne"))
        ListView1.Items.Add(Traduce.Txt("pov1_e"))
        ListView1.Items.Add(Traduce.Txt("pov1_se"))
        ListView1.Items.Add(Traduce.Txt("pov1_s"))
        ListView1.Items.Add(Traduce.Txt("pov1_sw"))
        ListView1.Items.Add(Traduce.Txt("pov1_w"))
        ListView1.Items.Add(Traduce.Txt("pov1_nw"))
        ListView1.Items.Add(Traduce.Txt("pov2_n"))
        ListView1.Items.Add(Traduce.Txt("pov2_ne"))
        ListView1.Items.Add(Traduce.Txt("pov2_e"))
        ListView1.Items.Add(Traduce.Txt("pov2_se"))
        ListView1.Items.Add(Traduce.Txt("pov2_s"))
        ListView1.Items.Add(Traduce.Txt("pov2_sw"))
        ListView1.Items.Add(Traduce.Txt("pov2_w"))
        ListView1.Items.Add(Traduce.Txt("pov2_nw"))
        If (padre.datos.GetTipoHotas() = 3) Then
            ListView1.Items.Add(Traduce.Txt("toggle1"))
            ListView1.Items.Add(Traduce.Txt("toggle2"))
            ListView1.Items.Add(Traduce.Txt("toggle3"))
            ListView1.Items.Add(Traduce.Txt("toggle4"))
            ListView1.Items.Add(Traduce.Txt("toggle5"))
            ListView1.Items.Add(Traduce.Txt("toggle6"))
            ListView1.Items.Add(Traduce.Txt("mode1"))
            ListView1.Items.Add(Traduce.Txt("mode2"))
            ListView1.Items.Add(Traduce.Txt("mode3"))
        End If
        For i As Byte = 1 To ListView1.Items.Count - 1
            ListView1.Items(i).ForeColor = Color.Yellow
            ListView1.Items(i).UseItemStyleForSubItems = False
        Next
        'acelerador
        ListView1.Items.Add(Traduce.Txt("throttle")).BackColor = Color.DarkSlateGray
        ListView1.Items.Add(Traduce.Txt("axisz"))
        If (padre.datos.GetTipoHotas() = 3) Then
            ListView1.Items.Add(Traduce.Txt("slider"))
        Else
            ListView1.Items.Add(Traduce.Txt("axisr"))
        End If
        ListView1.Items.Add(Traduce.Txt("axisrx"))
        ListView1.Items.Add(Traduce.Txt("axisry"))
        ListView1.Items.Add(Traduce.Txt("ministickx"))
        ListView1.Items.Add(Traduce.Txt("ministicky"))
        ListView1.Items.Add(Traduce.Txt("buttond"))
        If (padre.datos.GetTipoHotas() = 3) Then
            ListView1.Items.Add(Traduce.Txt("buttone"))
            ListView1.Items.Add(Traduce.Txt("buttoni"))
        End If
        ListView1.Items.Add(Traduce.Txt("buttonm"))
        ListView1.Items.Add(Traduce.Txt("pov3_n"))
        ListView1.Items.Add(Traduce.Txt("pov3_ne"))
        ListView1.Items.Add(Traduce.Txt("pov3_e"))
        ListView1.Items.Add(Traduce.Txt("pov3_se"))
        ListView1.Items.Add(Traduce.Txt("pov3_s"))
        ListView1.Items.Add(Traduce.Txt("pov3_sw"))
        ListView1.Items.Add(Traduce.Txt("pov3_w"))
        ListView1.Items.Add(Traduce.Txt("pov3_nw"))
        ListView1.Items.Add(Traduce.Txt("pov4_n"))
        ListView1.Items.Add(Traduce.Txt("pov4_ne"))
        ListView1.Items.Add(Traduce.Txt("pov4_e"))
        ListView1.Items.Add(Traduce.Txt("pov4_se"))
        ListView1.Items.Add(Traduce.Txt("pov4_s"))
        ListView1.Items.Add(Traduce.Txt("pov4_sw"))
        ListView1.Items.Add(Traduce.Txt("pov4_w"))
        ListView1.Items.Add(Traduce.Txt("pov4_nw"))
        If (padre.datos.GetTipoHotas() = 3) Then
            ListView1.Items.Add(Traduce.Txt("wheel_button"))
            ListView1.Items.Add(Traduce.Txt("wheel_up"))
            ListView1.Items.Add(Traduce.Txt("wheel_down"))
            ListView1.Items.Add(Traduce.Txt("mfd1"))
            ListView1.Items.Add(Traduce.Txt("mfd2"))
            ListView1.Items.Add(Traduce.Txt("mfd3"))
        Else
            ListView1.Items.Add(Traduce.Txt("mode1"))
            ListView1.Items.Add(Traduce.Txt("mode2"))
            ListView1.Items.Add(Traduce.Txt("mode3"))
            ListView1.Items.Add(Traduce.Txt("aux1"))
            ListView1.Items.Add(Traduce.Txt("aux2"))
            ListView1.Items.Add(Traduce.Txt("aux3"))
        End If

        For i As Byte = ListView1.FindItemWithText(Traduce.Txt("throttle"), False, 10).Index + 1 To ListView1.Items.Count - 1
            ListView1.Items(i).ForeColor = Color.Yellow
            ListView1.Items(i).UseItemStyleForSubItems = False
        Next
        ListView1.Columns.Item(0).Width = -1
    End Sub

    Private Sub Cargar()
        For i As Integer = 1 To ListView1.Items.Count - 1
            If padre.ToolStripSplitAux.DropDownItems.Count = 3 Then ' hay aux
                Dim col As Integer = 1
                For a As Byte = 0 To 2
                    If padre.ToolStripSplitModo.DropDownItems.Count = 3 Then ' hay modos
                        For m As Byte = 0 To 2
                            CogerDatos(col, 0, m, a, i)
                            col = col + 1
                            If padre.ToolStripSplitPinkie.DropDownItems.Count = 2 Then 'hay pk
                                CogerDatos(col, 1, m, a, i)
                                col = col + 1
                            End If
                        Next
                    Else
                        CogerDatos(col, 0, 0, a, i)
                        col = col + 1
                        If padre.ToolStripSplitPinkie.DropDownItems.Count = 2 Then 'hay pk
                            CogerDatos(col, 1, 0, a, i)
                            col = col + 1
                        End If
                    End If
                Next
            Else
                Dim col As Integer = 1
                If padre.ToolStripSplitModo.DropDownItems.Count = 3 Then ' hay modos
                    For m As Byte = 0 To 2
                        CogerDatos(col, 0, m, 0, i)
                        col = col + 1
                        If padre.ToolStripSplitPinkie.DropDownItems.Count = 2 Then 'hay pk
                            CogerDatos(col, 1, m, 0, i)
                            col = col + 1
                        End If
                    Next
                Else
                    CogerDatos(col, 0, 0, 0, i)
                    col = col + 1
                    If padre.ToolStripSplitPinkie.DropDownItems.Count = 2 Then 'hay pk
                        CogerDatos(col, 1, 0, 0, i)
                        col = col + 1
                    End If
                End If
            End If
        Next
    End Sub

    Private Sub CogerDatos(ByVal col As Integer, ByVal p As Byte, ByVal m As Byte, ByVal a As Byte, ByVal idc As Integer)
        Dim id As Byte = Mapear(idc)
        If id = 255 Then Return

        Dim nombre As String
        If id > 99 Then
            nombre = Boton(p, m, a, id - 100, True)
        ElseIf id < 64 Then
            nombre = Boton(p, m, a, id)
        ElseIf id < 71 Then
            nombre = Eje(p, m, a, id - 64)
        Else
            nombre = MiniStick(p, m, a, id - 64)
        End If

        Me.ListView1.Items(idc).SubItems.Add(nombre)
    End Sub

    Private Function Boton(ByVal p As Byte, ByVal m As Byte, ByVal a As Byte, ByVal b As Byte, Optional ByVal seta As Boolean = False) As String
        Dim n As String = ""
        If seta Then
            If padre.datos.GetMapaSetas_Indices(p, m, a, b, 0) > 0 Then n = padre.ComboBoxMacro.Items(padre.datos.GetMapaSetas_Indices(p, m, a, b, 0) - 1)
            If padre.datos.GetMapaSetas_Estado(p, m, a, b) > 0 Then
                For i As Integer = 1 To padre.datos.GetMapaSetas_Estado(p, m, a, b) - 1
                    If padre.datos.GetMapaSetas_Indices(p, m, a, b, i) > 0 Then n = n & ", " & padre.ComboBoxMacro.Items(padre.datos.GetMapaSetas_Indices(p, m, a, b, i) - 1)
                Next
            Else
                If padre.datos.GetMapaSetas_Indices(p, m, a, b, 1) > 0 Then n = n & " / " & padre.ComboBoxMacro.Items(padre.datos.GetMapaSetas_Indices(p, m, a, b, 1) - 1)
            End If
        Else
            If padre.datos.GetMapaBotones_Indices(p, m, a, b, 0) > 0 Then n = padre.ComboBoxMacro.Items(padre.datos.GetMapaBotones_Indices(p, m, a, b, 0) - 1)
            If padre.datos.GetMapaBotones_Estado(p, m, a, b) > 0 Then
                For i As Integer = 1 To padre.datos.GetMapaBotones_Estado(p, m, a, b) - 1
                    If padre.datos.GetMapaBotones_Indices(p, m, a, b, i) > 0 Then n = n & ", " & padre.ComboBoxMacro.Items(padre.datos.GetMapaBotones_Indices(p, m, a, b, i) - 1)
                Next
            Else
                If padre.datos.GetMapaBotones_Indices(p, m, a, b, 1) > 0 Then n = n & " / " & padre.ComboBoxMacro.Items(padre.datos.GetMapaBotones_Indices(p, m, a, b, 1) - 1)
            End If
        End If
        Return n
    End Function

    Private Function Eje(ByVal p As Byte, ByVal m As Byte, ByVal a As Byte, ByVal e As Byte) As String
        Dim n As String = ""
        Dim neje As Byte = padre.datos.GetMapaEjes_nEje(p, m, a, e)
        If neje > 19 Then n = "-" : neje = neje - 20

        n = n & padre.ComboBoxAxes.Items(neje) & " & "

        If padre.datos.GetMapaEjes_Incremental(p, m, a, e) Then
            If padre.datos.GetMapaEjes_Indices(p, m, a, e, 0) > 0 Then n = n & "+" & padre.ComboBoxMacro.Items(padre.datos.GetMapaEjes_Indices(p, m, a, e, 0) - 1) Else n = n & " "
            If padre.datos.GetMapaEjes_Indices(p, m, a, e, 1) > 0 Then n = n & "/-" & padre.ComboBoxMacro.Items(padre.datos.GetMapaEjes_Indices(p, m, a, e, 1) - 1) Else n = n & "/ "
        Else
            If padre.datos.GetMapaEjes_Indices(p, m, a, e, 0) > 0 Then
                n = n & padre.ComboBoxMacro.Items(padre.datos.GetMapaEjes_Indices(p, m, a, e, 0) - 1)
            End If
            For i As Byte = 1 To 15
                If padre.datos.GetMapaEjes_Indices(p, m, a, e, i) = 0 Then
                    n = n & ", "
                Else
                    n = n & ", " & padre.ComboBoxMacro.Items(padre.datos.GetMapaEjes_Indices(p, m, a, e, i) - 1)
                End If
            Next
        End If

        Return n
    End Function
    Private Function MiniStick(ByVal p As Byte, ByVal m As Byte, ByVal a As Byte, ByVal id As Byte) As String
        Dim n As String = ""
        Dim neje As Byte = padre.datos.GetMapaEjes_nEje(p, m, a, id)
        If neje > 19 Then n = "-" : neje = neje - 20

        Return n & padre.ComboBoxAxes.Items(neje)
    End Function

    Private Function Mapear(ByVal idc As Integer) As Integer
        If padre.datos.GetTipoHotas() = 3 Then
            Dim m() As Byte = {255, 64, 65, 66, 22, 0, 3, 1, 2, 7, 6, 100, 101, 102, 103, 104, 105, 106, 107, 108, 109, 110, 111, 112, 113, 114, 115, 16, 17, 18, 19, 20, 21, 8, 9, 10, 255, 67, 70, 68, 69, 71, 72, 4, 15, 14, 5, 116, 117, 118, 119, 120, 121, 122, 123, 124, 125, 126, 127, 128, 129, 130, 131, 23, 24, 25, 11, 12, 13}
            Return m(idc)
        Else
            Dim m() As Byte = {255, 64, 65, 0, 3, 1, 2, 7, 6, 100, 101, 102, 103, 104, 105, 106, 107, 108, 109, 110, 111, 112, 113, 114, 115, 255, 67, 66, 68, 69, 71, 72, 4, 5, 116, 117, 118, 119, 120, 121, 122, 123, 124, 125, 126, 127, 128, 129, 130, 131, 8, 9, 10, 11, 12, 13}
            Return m(idc)
        End If
    End Function

End Class
