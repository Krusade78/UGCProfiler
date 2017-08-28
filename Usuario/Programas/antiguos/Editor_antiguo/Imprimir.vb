Public Class Imprimir
    Private padre As Principal
    Private maxPaginas As Byte
    Private pagina As Byte = 0
    Private elemento As Byte = 0
    Private pk As Byte = 0
    Private mo As Byte = 0
    Private au As Byte = 0
    Private WithEvents pdoc As New Printing.PrintDocument

    Public Sub New(ByVal p As Principal)
        padre = p
        maxPaginas = GetPaginas()
        pdoc.DefaultPageSettings.Landscape = True
    End Sub

    Public Sub Ver()
        padre.PrintDialog1.Document = pdoc
        If padre.PrintDialog1.ShowDialog() = DialogResult.OK Then
            pdoc.DefaultPageSettings.Landscape = True
            pdoc.Print()
        End If
    End Sub

    Public Sub PreVer()
        padre.PrintPreviewDialog1.Document = pdoc
        padre.PrintPreviewDialog1.StartPosition = FormStartPosition.CenterScreen
        padre.PrintPreviewDialog1.WindowState = FormWindowState.Maximized
        padre.PrintPreviewDialog1.ShowDialog(padre)
    End Sub

    Private Function GetPaginas() As Byte
        Dim p As Byte = 1
        If padre.ToolStripSplitAux.DropDownItems.Count = 3 Then ' hay aux
            p = 3
            If padre.ToolStripSplitModo.DropDownItems.Count = 3 Then ' hay modos
                p = p * 3
                If padre.ToolStripSplitPinkie.DropDownItems.Count = 2 Then 'hay pk
                    p = p * 2
                End If
            Else
                If padre.ToolStripSplitPinkie.DropDownItems.Count = 2 Then 'hay pk
                    p = p * 2
                End If
            End If
        Else
            If padre.ToolStripSplitModo.DropDownItems.Count = 3 Then ' hay modos
                p = 3
                If padre.ToolStripSplitPinkie.DropDownItems.Count = 2 Then 'hay pk
                    p = p * 2
                End If
            Else
                If padre.ToolStripSplitPinkie.DropDownItems.Count = 2 Then 'hay pk
                    p = 2
                End If
            End If
        End If
        Return p
    End Function

    Private Sub document_PrintPage(ByVal sender As Object, ByVal e As System.Drawing.Printing.PrintPageEventArgs) Handles pdoc.PrintPage
        Dim x As Integer = 45
        Dim y As Integer = 40
        Dim maxy As Integer = y
        e.Graphics.Clear(Color.White)

        ImprimirTitulo(e.Graphics, pagina)
        y = 70
        While True
            If padre.datos.GetTipoHotas() = 3 Then
                If elemento = 67 Then
                    pagina = pagina + 1
                    Exit While
                End If
            Else
                If elemento = 54 Then
                    pagina = pagina + 1
                    Exit While
                End If
            End If
            If ImprimirElemento(e.Graphics, elemento, x, y, maxy) Then
                elemento = elemento + 1
            Else
                e.HasMorePages = True
                Exit Sub
            End If
        End While

        If pagina = maxPaginas Then
            e.HasMorePages = False
            pagina = 0
            elemento = 0
        Else
            e.HasMorePages = True
            elemento = 0
        End If
    End Sub

    Private Sub ImprimirTitulo(ByVal g As Graphics, ByVal p As Byte)
        Dim titulo As String = ""
        Dim pag As Byte = 0
        If padre.ToolStripSplitAux.DropDownItems.Count = 3 Then ' hay aux
            For a As Byte = 0 To 2
                pag = pag + 1

                Dim sta As String = padre.ToolStripSplitAux.DropDownItems.Item(a).Text
                If padre.ToolStripSplitModo.DropDownItems.Count = 3 Then ' hay modos
                    For m As Byte = 0 To 2
                        pag = pag + 1
                        Dim stm As String = sta & " + " & padre.ToolStripSplitModo.DropDownItems.Item(m).Text
                        If (p = pag) Then
                            titulo = stm
                            au = a
                            mo = m
                            pk = 0
                        End If
                        If padre.ToolStripSplitPinkie.DropDownItems.Count = 2 Then 'hay pk
                            pag = pag + 1
                            If (p = pag) Then
                                titulo = stm & " + " & padre.ToolStripSplitPinkie.DropDownItems.Item(0).Text.Substring(0, padre.ToolStripSplitPinkie.Text.Length - 3)
                                au = a
                                mo = m
                                pk = 1
                            End If
                        End If
                    Next
                Else
                    If p = pag Then
                        titulo = sta
                        au = a
                        mo = 0
                        pk = 0
                    End If
                    If padre.ToolStripSplitPinkie.DropDownItems.Count = 2 Then 'hay pk
                        pag = pag + 1
                        If p = pag Then
                            titulo = sta & " + " & padre.ToolStripSplitPinkie.DropDownItems.Item(0).Text.Substring(0, padre.ToolStripSplitPinkie.Text.Length - 3)
                            au = a
                            mo = 0
                            pk = 1
                        End If
                    End If
                End If
            Next
        Else
            If padre.ToolStripSplitModo.DropDownItems.Count = 3 Then ' hay modos
                For m As Byte = 0 To 2
                    pag = pag + 1
                    Dim stm As String = padre.ToolStripSplitModo.DropDownItems.Item(m).Text
                    If p = pag Then
                        titulo = stm
                        au = 0
                        mo = m
                        pk = 0
                    End If
                    If padre.ToolStripSplitPinkie.DropDownItems.Count = 2 Then 'hay pk
                        pag = pag + 1
                        If p = pag Then
                            titulo = stm & " + " & padre.ToolStripSplitPinkie.DropDownItems.Item(0).Text.Substring(0, padre.ToolStripSplitPinkie.Text.Length - 3)
                            au = 0
                            mo = m
                            pk = 1
                        End If
                    End If
                Next
            Else
                If padre.ToolStripSplitPinkie.DropDownItems.Count = 2 Then 'hay pk
                    If p = 1 Then
                        titulo = padre.ToolStripSplitPinkie.DropDownItems.Item(0).Text
                        au = 0
                        mo = 0
                        pk = 0
                    Else
                        titulo = padre.ToolStripSplitPinkie.DropDownItems.Item(1).Text
                        au = 0
                        mo = 0
                        pk = 1
                    End If
                Else
                    titulo = Traduce.Txt("no_modes")
                    au = 0
                    mo = 0
                    pk = 0
                End If
            End If
        End If

        If padre.datos.GetRutaPerfil() = Nothing Then
            titulo = titulo.Insert(0, " <" & Traduce.Txt("untitled") & ">  ")
        Else
            Dim nombre As String = padre.datos.GetRutaPerfil().Remove(padre.datos.GetRutaPerfil().Length - 4, 4)
            nombre = nombre.Remove(0, nombre.LastIndexOf("\") + 1)
            titulo = titulo.Insert(0, " <" & nombre & ">  ")
        End If
        If padre.datos.GetTipoHotas() = 0 Then
            titulo = titulo.Insert(0, "Saitek X36-F & X35-T ")
        Else
            titulo = titulo.Insert(0, padre.Label1.Text)
        End If
        Dim sz As SizeF = g.MeasureString(titulo, New Font(FontFamily.GenericSansSerif, 0.2, FontStyle.Bold, GraphicsUnit.Inch), 1000)
        g.DrawString(titulo, New Font(FontFamily.GenericSansSerif, 0.2, FontStyle.Bold, GraphicsUnit.Inch), Brushes.RoyalBlue, (1169 - sz.Width) / 2, 40)
    End Sub

    Private Function ImprimirElemento(ByVal g As Graphics, ByVal el As Byte, ByRef x As Integer, ByRef y As Integer, ByRef maxy As Integer) As Boolean
inicio:
        If (y + 20) > 786 Then Return False ' se sale de la pagina si o si

        If (x + 106) > 1118 Then 'linea nueva
            y = maxy + 20
            x = 45
            GoTo inicio
        End If

        Dim id As Byte = 0
        Dim titulo As String = Mapear(el, id)
        Dim ret As Boolean
        Dim yprevio As Integer = y

        If id > 99 Then
            ret = Boton(g, x, y, titulo, id - 100, True)
        ElseIf id < 64 Then
            ret = Boton(g, x, y, titulo, id)
        ElseIf id < 71 Then
            ret = Eje(g, x, y, titulo, id - 64)
        Else
            ret = MiniStick(g, x, y, titulo, id - 64)
        End If

        If ret Then
            If y > maxy Then maxy = y
            y = yprevio
        End If

        Return ret
    End Function

    Private Sub PintarCadenaCentrada(ByVal g As Graphics, ByVal st As String, ByVal f As Font, ByVal color As Brush, ByVal x As Integer, ByVal y As Integer)
        Dim sz As SizeF = g.MeasureString(st, f, 106)
        g.DrawString(st, f, color, New RectangleF(x + ((106 - sz.Width) / 2), y + 4, sz.Width, 20))
    End Sub

    Private Function Boton(ByVal g As Graphics, ByRef x As Integer, ByRef y As Integer, ByVal titulo As String, ByVal b As Byte, Optional ByVal seta As Boolean = False) As Boolean
        If seta Then
            If padre.datos.GetMapaSetas_Estado(pk, mo, au, b) > 0 Then
                If ((padre.datos.GetMapaSetas_Estado(pk, mo, au, b) * 20) + 20 + y) > 786 Then Return False
            Else
                Dim n As Byte = 0
                If padre.datos.GetMapaSetas_Indices(pk, mo, au, b, 0) > 0 Then n = n + 1
                If padre.datos.GetMapaSetas_Indices(pk, mo, au, b, 1) > 0 Then n = n + 1
                If ((n * 20) + 20 + y) > 786 Then Return False
            End If


            g.DrawRectangle(Pens.Black, x, y, 106, 20)
            PintarCadenaCentrada(g, titulo, New Font(FontFamily.GenericSansSerif, 8, FontStyle.Bold, GraphicsUnit.Point), Brushes.DarkRed, x, y)
            y = y + 20

            If padre.datos.GetMapaSetas_Estado(pk, mo, au, b) > 0 Then
                For i As Integer = 0 To padre.datos.GetMapaSetas_Estado(pk, mo, au, b) - 1
                    g.DrawRectangle(Pens.Black, x, y, 106, 20)
                    If padre.datos.GetMapaSetas_Indices(pk, mo, au, b, i) > 0 Then
                        g.DrawString(padre.ComboBoxMacro.Items(padre.datos.GetMapaSetas_Indices(pk, mo, au, b, i) - 1), _
                                        New Font(FontFamily.GenericSansSerif, 8, FontStyle.Regular, GraphicsUnit.Point), _
                                        Brushes.Black, New RectangleF(x + 1, y + 4, 105, 15))
                    End If
                    y = y + 20
                Next
            Else
                If padre.datos.GetMapaSetas_Indices(pk, mo, au, b, 0) > 0 Then
                    g.DrawRectangle(Pens.Black, x, y, 106, 20)
                    g.DrawString(padre.ComboBoxMacro.Items(padre.datos.GetMapaSetas_Indices(pk, mo, au, b, 0) - 1), _
                                    New Font(FontFamily.GenericSansSerif, 8, FontStyle.Regular, GraphicsUnit.Point), _
                                    Brushes.Black, New RectangleF(x + 1, y + 4, 105, 15))
                    y = y + 20
                End If
                If padre.datos.GetMapaSetas_Indices(pk, mo, au, b, 1) > 0 Then
                    g.DrawRectangle(Pens.Black, x, y, 106, 20)
                    g.DrawString(padre.ComboBoxMacro.Items(padre.datos.GetMapaSetas_Indices(pk, mo, au, b, 1) - 1), _
                                    New Font(FontFamily.GenericSansSerif, 8, FontStyle.Regular, GraphicsUnit.Point), _
                                    Brushes.MidnightBlue, New RectangleF(x + 1, y + 4, 105, 15))
                    y = y + 20
                End If
            End If
        Else
            If padre.datos.GetMapaBotones_Estado(pk, mo, au, b) > 0 Then
                If ((padre.datos.GetMapaBotones_Estado(pk, mo, au, b) * 20) + 20 + y) > 786 Then Return False
            Else
                Dim n As Byte = 0
                If padre.datos.GetMapaBotones_Indices(pk, mo, au, b, 0) > 0 Then n = n + 1
                If padre.datos.GetMapaBotones_Indices(pk, mo, au, b, 1) > 0 Then n = n + 1
                If ((n * 20) + 20 + y) > 786 Then Return False
            End If

            g.DrawRectangle(Pens.Black, x, y, 106, 20)
            PintarCadenaCentrada(g, titulo, New Font(FontFamily.GenericSansSerif, 8, FontStyle.Bold, GraphicsUnit.Point), Brushes.DarkRed, x, y)
        y = y + 20

        If padre.datos.GetMapaBotones_Estado(pk, mo, au, b) > 0 Then
                For i As Integer = 0 To padre.datos.GetMapaBotones_Estado(pk, mo, au, b) - 1
                    g.DrawRectangle(Pens.Black, x, y, 106, 20)
                    If padre.datos.GetMapaBotones_Indices(pk, mo, au, b, i) > 0 Then
                        g.DrawString(padre.ComboBoxMacro.Items(padre.datos.GetMapaBotones_Indices(pk, mo, au, b, i) - 1), _
                                        New Font(FontFamily.GenericSansSerif, 8, FontStyle.Regular, GraphicsUnit.Point), _
                                        Brushes.Black, New RectangleF(x + 1, y + 4, 105, 15))
                    End If
                    y = y + 20
                Next
        Else
            If padre.datos.GetMapaBotones_Indices(pk, mo, au, b, 0) > 0 Then
                    g.DrawRectangle(Pens.Black, x, y, 106, 20)
                    g.DrawString(padre.ComboBoxMacro.Items(padre.datos.GetMapaBotones_Indices(pk, mo, au, b, 0) - 1), _
                                    New Font(FontFamily.GenericSansSerif, 8, FontStyle.Regular, GraphicsUnit.Point), _
                                    Brushes.Black, New RectangleF(x + 1, y + 4, 105, 15))
                y = y + 20
            End If
            If padre.datos.GetMapaBotones_Indices(pk, mo, au, b, 1) > 0 Then
                    g.DrawRectangle(Pens.Black, x, y, 106, 20)
                    g.DrawString(padre.ComboBoxMacro.Items(padre.datos.GetMapaBotones_Indices(pk, mo, au, b, 1) - 1), _
                                    New Font(FontFamily.GenericSansSerif, 8, FontStyle.Regular, GraphicsUnit.Point), _
                                    Brushes.MidnightBlue, New RectangleF(x + 1, y + 4, 105, 15))
                y = y + 20
            End If
        End If
            End If

        x = x + 106
        Return True
    End Function

    Private Function Eje(ByVal g As Graphics, ByRef x As Integer, ByRef y As Integer, ByVal titulo As String, ByVal e As Byte) As Boolean
        Dim ejest As String = ""
        Dim neje As Byte = padre.datos.GetMapaEjes_nEje(pk, mo, au, e)
        If neje > 19 Then ejest = "-" : neje = neje - 20

        If padre.datos.GetMapaEjes_Incremental(pk, mo, au, e) Then
            Dim c As Byte = 0
            If padre.datos.GetMapaEjes_Indices(pk, mo, au, e, 0) > 0 Then c = c + 1
            If padre.datos.GetMapaEjes_Indices(pk, mo, au, e, 1) > 0 Then c = c + 1
            If ((c * 20) + 40 + y) > 786 Then Return False
        Else
            If ((padre.datos.GetMapaEjes_Indices(pk, mo, au, e, 0) * 20) + 40 + y) > 786 Then Return False
        End If

        g.DrawRectangle(Pens.Black, x, y, 106, 20)
        PintarCadenaCentrada(g, titulo, New Font(FontFamily.GenericSansSerif, 8, FontStyle.Bold, GraphicsUnit.Point), Brushes.DarkRed, x, y)
        y = y + 20
        g.DrawRectangle(Pens.Black, x, y, 106, 20)
        PintarCadenaCentrada(g, ejest & padre.ComboBoxAxes.Items(neje), New Font(FontFamily.GenericSansSerif, 8, FontStyle.Italic, GraphicsUnit.Point), Brushes.Green, x, y)
        y = y + 20

        If padre.datos.GetMapaEjes_Incremental(pk, mo, au, e) Then
            g.DrawRectangle(Pens.Black, x, y, 106, 20)
            If padre.datos.GetMapaEjes_Indices(pk, mo, au, e, 0) > 0 Then
                g.DrawString(padre.ComboBoxMacro.Items(padre.datos.GetMapaEjes_Indices(pk, mo, au, e, 0) - 1), _
                                New Font(FontFamily.GenericSansSerif, 8, FontStyle.Regular, GraphicsUnit.Point), _
                                Brushes.Black, New RectangleF(x + 1, y + 4, 105, 15))
            End If
            y = y + 20
            g.DrawRectangle(Pens.Black, x, y, 106, 20)
            If padre.datos.GetMapaEjes_Indices(pk, mo, au, e, 1) > 0 Then
                g.DrawString(padre.ComboBoxMacro.Items(padre.datos.GetMapaEjes_Indices(pk, mo, au, e, 1) - 1), _
                                New Font(FontFamily.GenericSansSerif, 8, FontStyle.Regular, GraphicsUnit.Point), _
                                Brushes.MidnightBlue, New RectangleF(x + 1, y + 4, 105, 15))
            End If
            y = y + 20
        Else
            If padre.datos.GetMapaEjes_Bandas(pk, mo, au, e, 0) > 0 Then
                For i As Int16 = 0 To 14
                    g.DrawRectangle(Pens.Black, x, y, 106, 20)
                    If padre.datos.GetMapaEjes_Indices(pk, mo, au, e, i) > 0 Then
                        g.DrawString(padre.ComboBoxMacro.Items(padre.datos.GetMapaEjes_Indices(pk, mo, au, e, i) - 1), _
                                        New Font(FontFamily.GenericSansSerif, 8, FontStyle.Regular, GraphicsUnit.Point), _
                                        Brushes.Black, New RectangleF(x + 1, y + 4, 105, 15))
                    End If
                    y = y + 20
                    If padre.datos.GetMapaEjes_Bandas(pk, mo, au, e, i) = 0 Then Exit For
                Next
            End If
        End If

        x = x + 106
        Return True
    End Function

    Private Function MiniStick(ByVal g As Graphics, ByRef x As Integer, ByRef y As Integer, ByVal titulo As String, ByVal id As Byte) As Boolean
        Dim ejest As String = ""
        Dim neje As Byte = padre.datos.GetMapaEjes_nEje(pk, mo, au, id)
        If neje > 19 Then ejest = "-" : neje = neje - 20

        If (40 + y) > 786 Then Return False

        g.DrawRectangle(Pens.Black, x, y, 106, 20)
        PintarCadenaCentrada(g, titulo, New Font(FontFamily.GenericSansSerif, 8, FontStyle.Bold, GraphicsUnit.Point), Brushes.DarkRed, x, y)
        y = y + 20
        g.DrawRectangle(Pens.Black, x, y, 106, 20)
        PintarCadenaCentrada(g, ejest & padre.ComboBoxAxes.Items(neje), New Font(FontFamily.GenericSansSerif, 8, FontStyle.Italic, GraphicsUnit.Point), Brushes.Green, x, y)
        y = y + 20

        x = x + 106
        Return True
    End Function

    Private Function Mapear(ByVal idc As Byte, ByRef map As Byte) As String
        Dim str As New ArrayList
        str.Add(Traduce.Txt("axisx"))
        str.Add(Traduce.Txt("axisy"))
        If (padre.datos.GetTipoHotas() = 3) Then
            str.Add(Traduce.Txt("axisr"))
            str.Add(Traduce.Txt("trigger1"))
            str.Add(Traduce.Txt("trigger2"))
        Else
            str.Add(Traduce.Txt("trigger"))
        End If
        str.Add(Traduce.Txt("buttonlaunch"))
        str.Add(Traduce.Txt("buttona"))
        str.Add(Traduce.Txt("buttonb"))
        str.Add(Traduce.Txt("buttonc"))
        str.Add(Traduce.Txt("pinkie"))
        str.Add(Traduce.Txt("pov1_n"))
        str.Add(Traduce.Txt("pov1_ne"))
        str.Add(Traduce.Txt("pov1_e"))
        str.Add(Traduce.Txt("pov1_se"))
        str.Add(Traduce.Txt("pov1_s"))
        str.Add(Traduce.Txt("pov1_sw"))
        str.Add(Traduce.Txt("pov1_w"))
        str.Add(Traduce.Txt("pov1_nw"))
        str.Add(Traduce.Txt("pov2_n"))
        str.Add(Traduce.Txt("pov2_ne"))
        str.Add(Traduce.Txt("pov2_e"))
        str.Add(Traduce.Txt("pov2_se"))
        str.Add(Traduce.Txt("pov2_s"))
        str.Add(Traduce.Txt("pov2_sw"))
        str.Add(Traduce.Txt("pov2_w"))
        str.Add(Traduce.Txt("pov2_nw"))
        If (padre.datos.GetTipoHotas() = 3) Then
            str.Add(Traduce.Txt("toggle1"))
            str.Add(Traduce.Txt("toggle2"))
            str.Add(Traduce.Txt("toggle3"))
            str.Add(Traduce.Txt("toggle4"))
            str.Add(Traduce.Txt("toggle5"))
            str.Add(Traduce.Txt("toggle6"))
            str.Add(Traduce.Txt("mode1"))
            str.Add(Traduce.Txt("mode2"))
            str.Add(Traduce.Txt("mode3"))
        End If
        'acelerador
        str.Add(Traduce.Txt("axisz"))
        If (padre.datos.GetTipoHotas() = 3) Then
            str.Add(Traduce.Txt("slider"))
        Else
            str.Add(Traduce.Txt("axisr"))
        End If
        str.Add(Traduce.Txt("axisrx"))
        str.Add(Traduce.Txt("axisry"))
        str.Add(Traduce.Txt("ministickx"))
        str.Add(Traduce.Txt("ministicky"))
        str.Add(Traduce.Txt("buttond"))
        If (padre.datos.GetTipoHotas() = 3) Then
            str.Add(Traduce.Txt("buttone"))
            str.Add(Traduce.Txt("buttoni"))
        End If
        str.Add(Traduce.Txt("buttonm"))
        str.Add(Traduce.Txt("pov3_n"))
        str.Add(Traduce.Txt("pov3_ne"))
        str.Add(Traduce.Txt("pov3_e"))
        str.Add(Traduce.Txt("pov3_se"))
        str.Add(Traduce.Txt("pov3_s"))
        str.Add(Traduce.Txt("pov3_sw"))
        str.Add(Traduce.Txt("pov3_w"))
        str.Add(Traduce.Txt("pov3_nw"))
        str.Add(Traduce.Txt("pov4_n"))
        str.Add(Traduce.Txt("pov4_ne"))
        str.Add(Traduce.Txt("pov4_e"))
        str.Add(Traduce.Txt("pov4_se"))
        str.Add(Traduce.Txt("pov4_s"))
        str.Add(Traduce.Txt("pov4_sw"))
        str.Add(Traduce.Txt("pov4_w"))
        str.Add(Traduce.Txt("pov4_nw"))
        If (padre.datos.GetTipoHotas() = 3) Then
            str.Add(Traduce.Txt("wheel_button"))
            str.Add(Traduce.Txt("wheel_up"))
            str.Add(Traduce.Txt("wheel_down"))
            str.Add(Traduce.Txt("mfd1"))
            str.Add(Traduce.Txt("mfd2"))
            str.Add(Traduce.Txt("mfd3"))
        Else
            str.Add(Traduce.Txt("mode1"))
            str.Add(Traduce.Txt("mode2"))
            str.Add(Traduce.Txt("mode3"))
            str.Add(Traduce.Txt("aux1"))
            str.Add(Traduce.Txt("aux2"))
            str.Add(Traduce.Txt("aux3"))
        End If
        If padre.datos.GetTipoHotas() = 3 Then
            Dim m() As Byte = {64, 65, 66, 22, 0, 3, 1, 2, 7, 6, 100, 101, 102, 103, 104, 105, 106, 107, 108, 109, 110, 111, 112, 113, 114, 115, 16, 17, 18, 19, 20, 21, 8, 9, 10, 67, 70, 68, 69, 71, 72, 4, 15, 14, 5, 116, 117, 118, 119, 120, 121, 122, 123, 124, 125, 126, 127, 128, 129, 130, 131, 23, 24, 25, 11, 12, 13}
            map = m(idc)
            Return str(idc)
        Else
            Dim m() As Byte = {64, 65, 0, 3, 1, 2, 7, 6, 100, 101, 102, 103, 104, 105, 106, 107, 108, 109, 110, 111, 112, 113, 114, 115, 67, 66, 68, 69, 71, 72, 4, 5, 116, 117, 118, 119, 120, 121, 122, 123, 124, 125, 126, 127, 128, 129, 130, 131, 8, 9, 10, 11, 12, 13}
            map = m(idc)
            Return str(idc)
        End If
    End Function
End Class
