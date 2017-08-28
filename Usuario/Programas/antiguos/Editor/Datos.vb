Public Class Datos

    Private Structure MB
        Public Estado As Byte  ' 4 bit idc 4 bit npos
        Public Indices() As UInt16
    End Structure
    Private Structure MEj_xyrz
        Public Mouse As Byte
        Public nEje As Byte
        Public Sensibilidad() As Byte
        Public Bandas() As Byte
        Public Indices() As UInt16
    End Structure
    Private Structure MEj_xyl
        Public Mouse As Byte
        Public nEje As Byte
        Public Bandas() As Byte
        Public Indices() As UInt16
    End Structure
    Private Structure MEj_mxmy
        Public Mouse As Byte
        Public nEje As Byte
    End Structure

    Private tipoHotas As Byte
    Private tickRaton As Byte = 30
    Private pinkie_modosDefecto As Byte '3 bits (pikie,modo,aux), 3 bit vacios, 2 bit (defecto pinkie,modos)
    Private mapaBotones(1, 2, 2, 25) As MB
    Private mapaSetas(1, 2, 2, 31) As MB
    Private mapaEjes1(1, 2, 2, 3) As MEj_xyrz
    Private mapaEjes2(1, 2, 2, 2) As MEj_xyl
    Private mapaEjes3(1, 2, 2, 1) As MEj_mxmy
    Private nombreModo(15, 6) As Byte
    Private macros As New ArrayList

    Private estaModificado As Boolean = False

#Region "Acceso a datos"
    Public Function GetModificado() As Boolean
        Return estaModificado
    End Function

    Public Sub SetMapaEjes_Incremental(ByVal p As Byte, ByVal m As Byte, ByVal a As Byte, ByVal e As Byte, ByVal inc As Byte)
        estaModificado = True
        If e < 4 Then
            mapaEjes1(p, m, a, e).nEje = (mapaEjes1(p, m, a, e).nEje And 127) Or (inc * 128)
            mapaEjes1(p, m, a, e).Indices(0) = 0
            mapaEjes1(p, m, a, e).Indices(1) = 0
            mapaEjes1(p, m, a, e).Indices(2) = 0
            mapaEjes1(p, m, a, e).Indices(3) = 0
        Else
            mapaEjes2(p, m, a, e - 4).nEje = (mapaEjes2(p, m, a, e - 4).nEje And 127) Or (inc * 128)
            mapaEjes2(p, m, a, e - 4).Indices(0) = 0
            mapaEjes2(p, m, a, e - 4).Indices(1) = 0
            mapaEjes2(p, m, a, e - 4).Indices(2) = 0
            mapaEjes2(p, m, a, e - 4).Indices(3) = 0
        End If
    End Sub
    Public Sub SetMapaEjes_MSensibilidad(ByVal p As Byte, ByVal m As Byte, ByVal a As Byte, ByVal e As Byte, ByVal sens As Byte)
        estaModificado = True
        If e < 4 Then
            mapaEjes1(p, m, a, e).Mouse = sens
        ElseIf e < 7 Then
            mapaEjes2(p, m, a, e - 4).Mouse = sens
        Else
            mapaEjes3(p, m, a, e - 7).Mouse = sens
        End If
    End Sub
    Public Sub SetMapaEjes_nEje(ByVal p As Byte, ByVal m As Byte, ByVal a As Byte, ByVal e As Byte, ByVal ej As Byte)
        estaModificado = True
        If e < 4 Then
            mapaEjes1(p, m, a, e).nEje = (mapaEjes1(p, m, a, e).nEje And 128) Or ej
        ElseIf e < 7 Then
            mapaEjes2(p, m, a, e - 4).nEje = (mapaEjes2(p, m, a, e - 4).nEje And 128) Or ej
        Else
            mapaEjes3(p, m, a, e - 7).nEje = (mapaEjes3(p, m, a, e - 7).nEje And 128) Or ej
        End If
    End Sub
    Public Sub SetMapaEjes_Sensibilidad(ByVal p As Byte, ByVal m As Byte, ByVal a As Byte, ByVal e As Byte, ByVal s As Byte, ByVal sens As Byte)
        estaModificado = True
        mapaEjes1(p, m, a, e).Sensibilidad(s) = sens
    End Sub
    Public Sub SetMapaEjes_Indices(ByVal p As Byte, ByVal m As Byte, ByVal a As Byte, ByVal e As Byte, ByVal t As Byte, ByVal id As Integer)
        estaModificado = True
        If e < 4 Then mapaEjes1(p, m, a, e).Indices(t) = id Else mapaEjes2(p, m, a, e - 4).Indices(t) = id
    End Sub
    Public Sub SetMapaEjes_SensibilidadInc(ByVal p As Byte, ByVal m As Byte, ByVal a As Byte, ByVal e As Byte, ByVal s As Byte)
        estaModificado = True
        If e < 4 Then mapaEjes1(p, m, a, e).Indices(2) = s Else mapaEjes2(p, m, a, e - 4).Indices(2) = s
    End Sub
    Public Sub SetMapaEjes_SensibilidadDec(ByVal p As Byte, ByVal m As Byte, ByVal a As Byte, ByVal e As Byte, ByVal s As Byte)
        estaModificado = True
        If e < 4 Then mapaEjes1(p, m, a, e).Indices(3) = s Else mapaEjes2(p, m, a, e - 4).Indices(3) = s
    End Sub
    Public Sub SetMapaEjes_Bandas(ByVal p As Byte, ByVal m As Byte, ByVal a As Byte, ByVal e As Byte, ByVal b As Byte, ByVal id As Integer)
        estaModificado = True
        If e < 4 Then mapaEjes1(p, m, a, e).Bandas(b) = id Else mapaEjes2(p, m, a, e - 4).Bandas(b) = id
    End Sub

    Public Function GetMapaEjes_Incremental(ByVal p As Byte, ByVal m As Byte, ByVal a As Byte, ByVal e As Byte) As Byte
        If e < 4 Then
            Return mapaEjes1(p, m, a, e).nEje >> 7
        Else
            Return mapaEjes2(p, m, a, e - 4).nEje >> 7
        End If
    End Function
    Public Function GetMapaEjes_MSensibilidad(ByVal p As Byte, ByVal m As Byte, ByVal a As Byte, ByVal e As Byte) As Byte
        If e < 4 Then
            Return mapaEjes1(p, m, a, e).Mouse
        ElseIf e < 7 Then
            Return mapaEjes2(p, m, a, e - 4).Mouse
        Else
            Return mapaEjes3(p, m, a, e - 7).Mouse
        End If
    End Function
    Public Function GetMapaEjes_nEje(ByVal p As Byte, ByVal m As Byte, ByVal a As Byte, ByVal e As Byte) As Byte
        If e < 4 Then
            Return mapaEjes1(p, m, a, e).nEje And 127
        ElseIf e < 7 Then
            Return mapaEjes2(p, m, a, e - 4).nEje And 127
        Else
            Return mapaEjes3(p, m, a, e - 7).nEje And 127
        End If
    End Function
    Public Function GetMapaEjes_Sensibilidad(ByVal p As Byte, ByVal m As Byte, ByVal a As Byte, ByVal e As Byte, ByVal s As Byte) As Byte
        Return mapaEjes1(p, m, a, e).Sensibilidad(s)
    End Function
    Public Function GetMapaEjes_SensibilidadInc(ByVal p As Byte, ByVal m As Byte, ByVal a As Byte, ByVal e As Byte) As Byte
        If e < 4 Then
            Return mapaEjes1(p, m, a, e).Indices(2)
        Else
            Return mapaEjes2(p, m, a, e - 4).Indices(2)
        End If
    End Function
    Public Function GetMapaEjes_SensibilidadDec(ByVal p As Byte, ByVal m As Byte, ByVal a As Byte, ByVal e As Byte) As Byte
        If e < 4 Then
            Return mapaEjes1(p, m, a, e).Indices(3)
        Else
            Return mapaEjes2(p, m, a, e - 4).Indices(3)
        End If
    End Function
    Public Function GetMapaEjes_Indices(ByVal p As Byte, ByVal m As Byte, ByVal a As Byte, ByVal e As Byte, ByVal t As Byte) As Integer
        If e < 4 Then
            Return mapaEjes1(p, m, a, e).Indices(t)
        Else
            Return mapaEjes2(p, m, a, e - 4).Indices(t)
        End If
    End Function
    Public Function GetMapaEjes_Bandas(ByVal p As Byte, ByVal m As Byte, ByVal a As Byte, ByVal e As Byte, ByVal b As Byte) As Integer
        If e < 4 Then
            Return mapaEjes1(p, m, a, e).Bandas(b)
        Else
            Return mapaEjes2(p, m, a, e - 4).Bandas(b)
        End If
    End Function

    Public Function GetMapaBotones_Estado(ByVal p As Byte, ByVal m As Byte, ByVal a As Byte, ByVal b As Byte) As Byte
        Return mapaBotones(p, m, a, b).Estado
    End Function
    Public Function GetMapaBotones_Indices(ByVal p As Byte, ByVal m As Byte, ByVal a As Byte, ByVal b As Byte, ByVal i As Byte) As Integer
        Return mapaBotones(p, m, a, b).Indices(i)
    End Function
    Public Function GetMapaSetas_Estado(ByVal p As Byte, ByVal m As Byte, ByVal a As Byte, ByVal b As Byte) As Byte
        Return mapaSetas(p, m, a, b).Estado
    End Function
    Public Function GetMapaSetas_Indices(ByVal p As Byte, ByVal m As Byte, ByVal a As Byte, ByVal b As Byte, ByVal i As Byte) As Integer
        Return mapaSetas(p, m, a, b).Indices(i)
    End Function

    Public Sub SetMapaBotones_Estado(ByVal p As Byte, ByVal m As Byte, ByVal a As Byte, ByVal b As Byte, ByVal st As Byte)
        estaModificado = True
        mapaBotones(p, m, a, b).Estado = st
    End Sub
    Public Sub SetMapaBotones_Indices(ByVal p As Byte, ByVal m As Byte, ByVal a As Byte, ByVal b As Byte, ByVal t As Byte, ByVal id As Integer)
        estaModificado = True
        mapaBotones(p, m, a, b).Indices(t) = id
    End Sub
    Public Sub SetMapaSetas_Estado(ByVal p As Byte, ByVal m As Byte, ByVal a As Byte, ByVal b As Byte, ByVal st As Byte)
        estaModificado = True
        mapaSetas(p, m, a, b).Estado = st
    End Sub
    Public Sub SetMapaSetas_Indices(ByVal p As Byte, ByVal m As Byte, ByVal a As Byte, ByVal b As Byte, ByVal t As Byte, ByVal id As Integer)
        estaModificado = True
        mapaSetas(p, m, a, b).Indices(t) = id
    End Sub

    Public Function GetTipoHotas() As Byte
        Return tipoHotas
    End Function
    Public Function GetRefrescoRaton() As Byte
        Return tickRaton
    End Function
    Public Function GetModosDefecto() As Byte
        Return pinkie_modosDefecto
    End Function
    Public Sub GetNombreModo(ByVal b As Byte, ByRef st() As Byte)
        For i As Byte = 0 To 15
            st(i) = nombreModo(i, b)
        Next
    End Sub

    Public Sub SetTipoHotas(ByVal t As Byte)
        estaModificado = True
        tipoHotas = t
    End Sub
    Public Sub SetRefrescoRaton(ByVal r As Byte)
        estaModificado = True
        tickRaton = r
    End Sub
    Public Sub SetModosDefecto(ByVal b As Byte)
        estaModificado = True
        pinkie_modosDefecto = b
    End Sub
    Public Sub SetNombreModo(ByVal b As Byte, ByVal st() As Byte)
        estaModificado = True
        For i As Int16 = 0 To st.Length - 1
            nombreModo(i, b) = st(i)
        Next
    End Sub
#End Region

    Public Sub New()
        For i1 As Byte = 0 To 1
            For i2 As Byte = 0 To 2
                For i3 As Byte = 0 To 2
                    For i5 As Byte = 0 To 25
                        ReDim mapaBotones(i1, i2, i3, i5).Indices(14)
                    Next
                    For i5 As Byte = 0 To 31
                        ReDim mapaSetas(i1, i2, i3, i5).Indices(14)
                    Next
                Next
            Next
        Next
        For i1 As Byte = 0 To 1
            For i2 As Byte = 0 To 2
                For i3 As Byte = 0 To 2
                    For i4 As Byte = 0 To 3
                        ReDim mapaEjes1(i1, i2, i3, i4).Sensibilidad(9)
                        For i5 As Byte = 0 To 9
                            mapaEjes1(i1, i2, i3, i4).Sensibilidad(i5) = (i5 + 1) * 10
                        Next
                        ReDim mapaEjes1(i1, i2, i3, i4).Bandas(14)
                        ReDim mapaEjes1(i1, i2, i3, i4).Indices(15)
                        mapaEjes1(i1, i2, i3, i4).Mouse = 1
                    Next
                    For i4 As Byte = 0 To 2
                        ReDim mapaEjes2(i1, i2, i3, i4).Bandas(14)
                        ReDim mapaEjes2(i1, i2, i3, i4).Indices(15)
                        mapaEjes2(i1, i2, i3, i4).Mouse = 1
                    Next
                    mapaEjes3(i1, i2, i3, 0).Mouse = 1
                    mapaEjes3(i1, i2, i3, 1).Mouse = 1
                Next
            Next
        Next
    End Sub
#Region "macros"
    Public Function GetMacro(ByVal idx As Integer) As ArrayList
        estaModificado = True
        Return macros(idx)
    End Function
    Public Sub QuitarIndice(ByVal i As Integer)
        i = i + 1
        For i1 As Byte = 0 To 1
            For i2 As Byte = 0 To 2
                For i3 As Byte = 0 To 2
                    For i6 As Byte = 0 To 14
                        For i4 As Byte = 0 To 25
                            If mapaBotones(i1, i2, i3, i4).Indices(i6) > i Then
                                mapaBotones(i1, i2, i3, i4).Indices(i6) = mapaBotones(i1, i2, i3, i4).Indices(i6) - 1
                            ElseIf mapaBotones(i1, i2, i3, i4).Indices(i6) = i Then
                                mapaBotones(i1, i2, i3, i4).Indices(i6) = 0
                            End If
                        Next
                        For i4 As Byte = 0 To 31
                            If mapaSetas(i1, i2, i3, i4).Indices(i6) > i Then
                                mapaSetas(i1, i2, i3, i4).Indices(i6) = mapaSetas(i1, i2, i3, i4).Indices(i6) - 1
                            ElseIf mapaSetas(i1, i2, i3, i4).Indices(i6) = i Then
                                mapaSetas(i1, i2, i3, i4).Indices(i6) = 0
                            End If
                        Next
                        For i4 As Byte = 0 To 3
                            If mapaEjes1(i1, i2, i3, i4).Indices(i6) > i Then
                                mapaEjes1(i1, i2, i3, i4).Indices(i6) = mapaEjes1(i1, i2, i3, i4).Indices(i6) - 1
                            ElseIf mapaEjes1(i1, i2, i3, i4).Indices(i6) = i Then
                                mapaEjes1(i1, i2, i3, i4).Indices(i6) = 0
                            End If
                        Next
                        For i4 As Byte = 0 To 2
                            If mapaEjes2(i1, i2, i3, i4).Indices(i6) > i Then
                                mapaEjes2(i1, i2, i3, i4).Indices(i6) = mapaEjes2(i1, i2, i3, i4).Indices(i6) - 1
                            ElseIf mapaEjes2(i1, i2, i3, i4).Indices(i6) = i Then
                                mapaEjes2(i1, i2, i3, i4).Indices(i6) = 0
                            End If
                        Next
                    Next
                    For i4 As Byte = 0 To 3
                        If mapaEjes1(i1, i2, i3, i4).Indices(15) > i Then
                            mapaEjes1(i1, i2, i3, i4).Indices(15) = mapaEjes1(i1, i2, i3, i4).Indices(15) - 1
                        ElseIf mapaEjes1(i1, i2, i3, i4).Indices(15) = i Then
                            mapaEjes1(i1, i2, i3, i4).Indices(15) = 0
                        End If
                    Next
                    For i4 As Byte = 0 To 2
                        If mapaEjes2(i1, i2, i3, i4).Indices(15) > i Then
                            mapaEjes2(i1, i2, i3, i4).Indices(15) = mapaEjes2(i1, i2, i3, i4).Indices(15) - 1
                        ElseIf mapaEjes2(i1, i2, i3, i4).Indices(15) = i Then
                            mapaEjes2(i1, i2, i3, i4).Indices(15) = 0
                        End If
                    Next
                Next
            Next
        Next
        CType(macros.Item(i - 1), ArrayList).Clear()
        macros.RemoveAt(i - 1)
        estaModificado = True
    End Sub
    Public Sub AñadirIndice(ByVal idx As Integer, ByRef comando As ArrayList)
        macros.Insert(idx, comando)
        idx = idx + 1
        For i1 As Byte = 0 To 1
            For i2 As Byte = 0 To 2
                For i3 As Byte = 0 To 2
                    For i6 As Byte = 0 To 14
                        For i4 As Byte = 0 To 25
                            If mapaBotones(i1, i2, i3, i4).Indices(i6) >= idx Then
                                mapaBotones(i1, i2, i3, i4).Indices(i6) = mapaBotones(i1, i2, i3, i4).Indices(i6) + 1
                            End If
                        Next
                        For i4 As Byte = 0 To 31
                            If mapaSetas(i1, i2, i3, i4).Indices(i6) >= idx Then
                                mapaSetas(i1, i2, i3, i4).Indices(i6) = mapaSetas(i1, i2, i3, i4).Indices(i6) + 1
                            End If
                        Next
                        For i4 As Byte = 0 To 3
                            If mapaEjes1(i1, i2, i3, i4).Indices(i6) >= idx Then
                                mapaEjes1(i1, i2, i3, i4).Indices(i6) = mapaEjes1(i1, i2, i3, i4).Indices(i6) + 1
                            End If
                        Next
                        For i4 As Byte = 0 To 2
                            If mapaEjes2(i1, i2, i3, i4).Indices(i6) >= idx Then
                                mapaEjes2(i1, i2, i3, i4).Indices(i6) = mapaEjes2(i1, i2, i3, i4).Indices(i6) + 1
                            End If
                        Next
                    Next
                    For i4 As Byte = 0 To 3
                        If mapaEjes1(i1, i2, i3, i4).Indices(15) >= idx Then
                            mapaEjes1(i1, i2, i3, i4).Indices(15) = mapaEjes1(i1, i2, i3, i4).Indices(15) + 1
                        End If
                    Next
                    For i4 As Byte = 0 To 2
                        If mapaEjes2(i1, i2, i3, i4).Indices(15) >= idx Then
                            mapaEjes2(i1, i2, i3, i4).Indices(15) = mapaEjes2(i1, i2, i3, i4).Indices(15) + 1
                        End If
                    Next
                Next
            Next
        Next
        estaModificado = True
    End Sub
    Public Sub IntercambiarIndices(ByVal nuevo As Integer, ByVal viejo As Integer, ByVal assigned As ComboBox, ByVal cmacro As ComboBox)
        For i1 As Byte = 0 To 1
            For i2 As Byte = 0 To 2
                For i3 As Byte = 0 To 2
                    For i6 As Byte = 0 To 14
                        For i4 As Byte = 0 To 25
                            If mapaBotones(i1, i2, i3, i4).Indices(i6) = viejo Then
                                mapaBotones(i1, i2, i3, i4).Indices(i6) = nuevo
                            End If
                        Next
                        For i4 As Byte = 0 To 31
                            If mapaSetas(i1, i2, i3, i4).Indices(i6) = viejo Then
                                mapaSetas(i1, i2, i3, i4).Indices(i6) = nuevo
                            End If
                        Next
                        For i4 As Byte = 0 To 3
                            If mapaEjes1(i1, i2, i3, i4).Indices(i6) = viejo Then
                                mapaEjes1(i1, i2, i3, i4).Indices(i6) = nuevo
                            End If
                        Next
                        For i4 As Byte = 0 To 2
                            If mapaEjes2(i1, i2, i3, i4).Indices(i6) = viejo Then
                                mapaEjes2(i1, i2, i3, i4).Indices(i6) = nuevo
                            End If
                        Next
                    Next
                    For i4 As Byte = 0 To 3
                        If mapaEjes1(i1, i2, i3, i4).Indices(15) = viejo Then
                            mapaEjes1(i1, i2, i3, i4).Indices(15) = nuevo
                        End If
                    Next
                    For i4 As Byte = 0 To 2
                        If mapaEjes2(i1, i2, i3, i4).Indices(15) = viejo Then
                            mapaEjes2(i1, i2, i3, i4).Indices(15) = nuevo
                        End If
                    Next
                Next
            Next
        Next
        If viejo < nuevo Then
            QuitarIndice(viejo - 1)
            assigned.Items.RemoveAt(viejo)
            cmacro.Items.RemoveAt(viejo - 1)
        Else
            QuitarIndice(viejo)
            assigned.Items.RemoveAt(viejo + 1)
            cmacro.Items.RemoveAt(viejo)
        End If
    End Sub
#End Region

#Region "File"
    Private rutaPerfil As String
    Public Sub SetRutaPerfil(ByVal ruta As String)
        rutaPerfil = ruta
    End Sub
    Public Function GetRutaPerfil() As String
        Return rutaPerfil
    End Function

    Public Function Guardar(ByVal combo As ComboBox) As Boolean
        Return Guardar(rutaPerfil, combo)
    End Function
    Public Function Guardar(ByVal archivo As String, ByVal combo As ComboBox) As Boolean
        Dim f As System.IO.FileStream
        Dim gch As Runtime.InteropServices.GCHandle
        Dim buff() As Byte
        Try
            If IO.File.Exists(archivo) Then f = New System.IO.FileStream(archivo, IO.FileMode.Truncate) Else f = New System.IO.FileStream(archivo, IO.FileMode.Create)
        Catch
            Traduce.Msg("error_opening_file", "warning", MsgBoxStyle.Exclamation)
            Return False
        End Try

        Try
            f.WriteByte(Me.tipoHotas)
            f.WriteByte(Me.tickRaton)

            gch = Runtime.InteropServices.GCHandle.Alloc(nombreModo, Runtime.InteropServices.GCHandleType.Pinned)
            ReDim buff(111)
            Runtime.InteropServices.Marshal.Copy(gch.AddrOfPinnedObject, buff, 0, 112)
            gch.Free()
            f.Write(buff, 0, 112)
            f.WriteByte(Me.pinkie_modosDefecto)

            For i1 As Byte = 0 To 1
                For i2 As Byte = 0 To 2
                    For i3 As Byte = 0 To 2
                        For i4 As Byte = 0 To 3
                            f.WriteByte(mapaEjes1(i1, i2, i3, i4).Mouse)
                            f.WriteByte(mapaEjes1(i1, i2, i3, i4).nEje)
                            f.Write(mapaEjes1(i1, i2, i3, i4).Sensibilidad, 0, 10)
                            f.Write(mapaEjes1(i1, i2, i3, i4).Bandas, 0, 15)
                            f.WriteByte(0)
                            For i5 As Byte = 0 To 15
                                Dim b(1) As Byte
                                b(0) = mapaEjes1(i1, i2, i3, i4).Indices(i5) And &HFF
                                b(1) = mapaEjes1(i1, i2, i3, i4).Indices(i5) >> 8
                                f.Write(b, 0, 2)
                            Next
                        Next
                    Next
                Next
            Next
            For i1 As Byte = 0 To 1
                For i2 As Byte = 0 To 2
                    For i3 As Byte = 0 To 2
                        For i4 As Byte = 0 To 2
                            f.WriteByte(mapaEjes2(i1, i2, i3, i4).Mouse)
                            f.WriteByte(mapaEjes2(i1, i2, i3, i4).nEje)
                            f.Write(mapaEjes2(i1, i2, i3, i4).Bandas, 0, 15)
                            f.WriteByte(0)
                            For i5 As Byte = 0 To 15
                                Dim b(1) As Byte
                                b(0) = mapaEjes2(i1, i2, i3, i4).Indices(i5) And &HFF
                                b(1) = mapaEjes2(i1, i2, i3, i4).Indices(i5) >> 8
                                f.Write(b, 0, 2)
                            Next
                        Next
                    Next
                Next
            Next
            For i1 As Byte = 0 To 1
                For i2 As Byte = 0 To 2
                    For i3 As Byte = 0 To 2
                        f.WriteByte(mapaEjes3(i1, i2, i3, 0).Mouse)
                        f.WriteByte(mapaEjes3(i1, i2, i3, 0).nEje)
                        f.WriteByte(mapaEjes3(i1, i2, i3, 1).Mouse)
                        f.WriteByte(mapaEjes3(i1, i2, i3, 1).nEje)
                    Next
                Next
            Next
            For i1 As Byte = 0 To 1
                For i2 As Byte = 0 To 2
                    For i3 As Byte = 0 To 2
                        For i4 As Byte = 0 To 25
                            f.WriteByte(mapaBotones(i1, i2, i3, i4).Estado)
                            f.WriteByte(0)
                            For i5 As Byte = 0 To 14
                                Dim b(1) As Byte
                                b(0) = mapaBotones(i1, i2, i3, i4).Indices(i5) And &HFF
                                b(1) = mapaBotones(i1, i2, i3, i4).Indices(i5) >> 8
                                f.Write(b, 0, 2)
                            Next
                        Next
                    Next
                Next
            Next
            For i1 As Byte = 0 To 1
                For i2 As Byte = 0 To 2
                    For i3 As Byte = 0 To 2
                        For i4 As Byte = 0 To 31
                            f.WriteByte(mapaSetas(i1, i2, i3, i4).Estado)
                            f.WriteByte(0)
                            For i5 As Byte = 0 To 14
                                Dim b(1) As Byte
                                b(0) = mapaSetas(i1, i2, i3, i4).Indices(i5) And &HFF
                                b(1) = mapaSetas(i1, i2, i3, i4).Indices(i5) >> 8
                                f.Write(b, 0, 2)
                            Next
                        Next
                    Next
                Next
            Next


            ' Macros

            ' cuenta
            f.WriteByte(macros.Count And &HFF)
            f.WriteByte(macros.Count >> 8)

            For i As Integer = 0 To macros.Count - 1
                f.WriteByte(combo.Items(i).ToString().Length)
                'nombre
                f.Write(System.Text.UnicodeEncoding.Unicode.GetBytes(combo.Items(i).ToString()), 0, System.Text.UnicodeEncoding.Unicode.GetBytes(combo.Items(i).ToString()).Length)
                f.WriteByte(CType(macros.Item(i), ArrayList).Count)
                ' comandos
                For j As Integer = 0 To CType(macros.Item(i), ArrayList).Count - 1
                    Dim c As UInt16
                    c = CType(CType(macros.Item(i), ArrayList).Item(j), UInt16)
                    f.WriteByte(c And &HFF)
                    f.WriteByte(c >> 8)
                Next
            Next
        Catch ex As Exception
            f.Close()
            Try
                System.IO.File.Delete(archivo)
            Catch
            End Try
            Traduce.Msg("error_writing_file", "warning", MsgBoxStyle.Exclamation)
            Return False
        End Try

        f.Close()
        estaModificado = False
        rutaPerfil = archivo
        Return True
    End Function

    Public Function CargarArchivo(ByVal archivo As String, ByVal comboa As ComboBox, ByVal combom As ComboBox) As Boolean
        Dim f As System.IO.FileStream
        Try
            f = New System.IO.FileStream(archivo, IO.FileMode.Open)
        Catch
            Traduce.Msg("error_opening_file", "warning", MsgBoxStyle.Exclamation)
            Return False
        End Try
        Try
            CargarMapa(f)
        Catch
            f.Close()
            Traduce.Msg("error_reading_file", "warning", MsgBoxStyle.Exclamation)
            Return False
        End Try
        Try
            CargarMacros(f, comboa, combom)
        Catch
            f.Close()
            Return False
        End Try
        f.Close()

        estaModificado = False
        rutaPerfil = archivo
        Return True
    End Function

    Private Sub CargarMacros(ByVal archivo As System.IO.FileStream, ByVal comboa As ComboBox, ByVal combom As ComboBox)
        ' cuenta
        Dim total As Integer
        total = archivo.ReadByte()
        total = total + (archivo.ReadByte() << 8)

        For i As Integer = 0 To total - 1
            'nombre
            Dim nombre() As Byte
            Dim tam As Integer
            Dim comandos As New ArrayList()
            ReDim nombre((archivo.ReadByte() * 2) - 1)
            archivo.Read(nombre, 0, nombre.Length)
            combom.Items.Add(System.Text.UnicodeEncoding.Unicode.GetString(nombre))
            comboa.Items.Add(System.Text.UnicodeEncoding.Unicode.GetString(nombre))
            tam = archivo.ReadByte()
            ' comandos
            macros.Add(comandos)
            For j As Integer = 0 To tam - 1
                Dim c As UInt16
                c = archivo.ReadByte()
                c = c + (archivo.ReadByte() << 8)
                comandos.Add(c)
            Next
        Next
        If total > 0 Then combom.SelectedIndex = 0
    End Sub

    Private Sub CargarMapa(ByVal f As System.IO.FileStream)
        Dim buff() As Byte
        Dim gch As Runtime.InteropServices.GCHandle

        Me.tipoHotas = f.ReadByte()
        Me.tickRaton = f.ReadByte()

        gch = Runtime.InteropServices.GCHandle.Alloc(nombreModo, Runtime.InteropServices.GCHandleType.Pinned)
        ReDim buff(111)
        f.Read(buff, 0, 112)
        Runtime.InteropServices.Marshal.Copy(buff, 0, gch.AddrOfPinnedObject, 112)
        gch.Free()

        Me.pinkie_modosDefecto = f.ReadByte()

        For i1 As Byte = 0 To 1
            For i2 As Byte = 0 To 2
                For i3 As Byte = 0 To 2
                    For i4 As Byte = 0 To 3
                        mapaEjes1(i1, i2, i3, i4).Mouse = f.ReadByte()
                        mapaEjes1(i1, i2, i3, i4).nEje = f.ReadByte()
                        f.Read(mapaEjes1(i1, i2, i3, i4).Sensibilidad, 0, 10)
                        f.Read(mapaEjes1(i1, i2, i3, i4).Bandas, 0, 15)
                        f.ReadByte()
                        For i5 As Byte = 0 To 15
                            mapaEjes1(i1, i2, i3, i4).Indices(i5) = f.ReadByte()
                            mapaEjes1(i1, i2, i3, i4).Indices(i5) = mapaEjes1(i1, i2, i3, i4).Indices(i5) + (f.ReadByte() << 8)
                        Next
                    Next
                Next
            Next
        Next
        For i1 As Byte = 0 To 1
            For i2 As Byte = 0 To 2
                For i3 As Byte = 0 To 2
                    For i4 As Byte = 0 To 2
                        mapaEjes2(i1, i2, i3, i4).Mouse = f.ReadByte()
                        mapaEjes2(i1, i2, i3, i4).nEje = f.ReadByte()
                        f.Read(mapaEjes2(i1, i2, i3, i4).Bandas, 0, 15)
                        f.ReadByte()
                        For i5 As Byte = 0 To 15
                            mapaEjes2(i1, i2, i3, i4).Indices(i5) = f.ReadByte()
                            mapaEjes2(i1, i2, i3, i4).Indices(i5) = mapaEjes2(i1, i2, i3, i4).Indices(i5) + (f.ReadByte() << 8)
                        Next
                    Next
                Next
            Next
        Next
        For i1 As Byte = 0 To 1
            For i2 As Byte = 0 To 2
                For i3 As Byte = 0 To 2
                    mapaEjes3(i1, i2, i3, 0).Mouse = f.ReadByte()
                    mapaEjes3(i1, i2, i3, 0).nEje = f.ReadByte()
                    mapaEjes3(i1, i2, i3, 1).Mouse = f.ReadByte()
                    mapaEjes3(i1, i2, i3, 1).nEje = f.ReadByte()
                Next
            Next
        Next
        For i1 As Byte = 0 To 1
            For i2 As Byte = 0 To 2
                For i3 As Byte = 0 To 2
                    For i4 As Byte = 0 To 25
                        mapaBotones(i1, i2, i3, i4).Estado = f.ReadByte()
                        f.ReadByte()
                        For i5 As Byte = 0 To 14
                            mapaBotones(i1, i2, i3, i4).Indices(i5) = f.ReadByte()
                            mapaBotones(i1, i2, i3, i4).Indices(i5) = mapaBotones(i1, i2, i3, i4).Indices(i5) + (f.ReadByte() << 8)
                        Next
                    Next
                Next
            Next
        Next
        For i1 As Byte = 0 To 1
            For i2 As Byte = 0 To 2
                For i3 As Byte = 0 To 2
                    For i4 As Byte = 0 To 31
                        mapaSetas(i1, i2, i3, i4).Estado = f.ReadByte()
                        f.ReadByte()
                        For i5 As Byte = 0 To 14
                            mapaSetas(i1, i2, i3, i4).Indices(i5) = f.ReadByte()
                            mapaSetas(i1, i2, i3, i4).Indices(i5) = mapaSetas(i1, i2, i3, i4).Indices(i5) + (f.ReadByte() << 8)
                        Next
                    Next
                Next
            Next
        Next
    End Sub
#End Region
End Class


