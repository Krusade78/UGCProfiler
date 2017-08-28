Public Class Driver
    Private Declare Unicode Function CargarMapa Lib "Comsbn.dll" (ByVal archivo As String) As Byte

    Public Shared Function Lanzar(ByVal archivo As String) As Boolean
        Select Case CargarMapa(archivo)
            Case 1
                Traduce.Msg("error_open_xhp", "warning", MsgBoxStyle.Exclamation)
                Return False
            Case 2
                Traduce.Msg("error_reading_file", "warning", MsgBoxStyle.Exclamation)
                Return False
            Case 3
                Traduce.Msg("error_opening_device", "warning", MsgBoxStyle.Exclamation)
                Return False
            Case 4
                Traduce.Msg("error_accessing_device", "warning", MsgBoxStyle.Exclamation)
                Return False
            Case Else
                Return True
        End Select
    End Function
End Class