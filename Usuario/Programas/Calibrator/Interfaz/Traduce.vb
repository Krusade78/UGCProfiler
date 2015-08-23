Public Class Traduce
    Private Shared datos As System.Data.DataSet = Nothing

    Public Shared Function Msg(ByVal id1 As String, ByVal tipo As Microsoft.VisualBasic.MsgBoxStyle) As Microsoft.VisualBasic.MsgBoxResult
        Dim texto1 As String = ""
        Traducir(id1, texto1)
        Return MsgBox(texto1, tipo, "XCalibrator")
    End Function

    Public Shared Function Txt(ByVal id As String) As String
        Dim texto As String = ""
        Traducir(id, texto)
        Txt = texto
    End Function

    Private Shared Sub Traducir(ByVal ID As String, ByRef texto As String)
        Dim nombre As String = ""
        Dim ruta As String = ""
        CogerNombreIdioma(nombre)
        If datos Is Nothing Then
            If Not BuscarArchivo(nombre, ruta) Then Exit Sub
            datos = New System.Data.DataSet()
            Try
                datos.ReadXml(ruta)
            Catch
                datos.Dispose()
                datos = Nothing
                Exit Sub
            End Try
        End If
        If Not ExtraerCadena(ID, texto) Then texto = ""
    End Sub

    Private Shared Sub CogerNombreIdioma(ByRef nombre As String)
        Select Case (System.Globalization.CultureInfo.CurrentCulture.LCID And &HFF)
            Case &HC
                nombre = "french.xml"
            Case &H10
                nombre = "italian.xml"
            Case &HA
                nombre = "spanish.xml"
            Case &H7
                nombre = "german.xml"
            Case Else
                nombre = "english.xml"
        End Select
    End Sub

    Private Shared Function BuscarArchivo(ByVal idioma As String, ByRef ruta As String) As Boolean
        Dim arch As System.IO.FileInfo
        Dim pordefecto As Boolean = False

da:
        Try
            arch = New System.IO.FileInfo(idioma)
        Catch
            arch = Nothing
            BuscarArchivo = False
            Exit Function
        End Try
        If arch.Exists Then
            ruta = arch.FullName
            BuscarArchivo = True
        Else
            If Not pordefecto Then
                idioma = "english.xml"
                pordefecto = True
                arch = Nothing
                GoTo da
            Else
                BuscarArchivo = False
            End If
        End If
        arch = Nothing
    End Function

    Private Shared Function ExtraerCadena(ByVal ID As String, ByRef texto As String) As Boolean
        Dim app As Integer = datos.Tables.IndexOf("Calibrator")
        If app = -1 Then Return False
        If datos.Tables(app).Columns.IndexOf(ID) > -1 Then
            texto = datos.Tables(app).Rows(0).Item(datos.Tables(app).Columns.IndexOf(ID))
            Return True
        Else
            Return False
        End If
    End Function

End Class
