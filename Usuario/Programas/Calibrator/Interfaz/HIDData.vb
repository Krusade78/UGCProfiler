'Public Structure DIJOYSTATE2
'    Public lX As Int32
'    Public lY As Int32
'    Public lZ As Int32
'    Public lRx As Int32
'    Public lRy As Int32
'    Public lRz As Int32
'    Public rglSlider() As Int32
'    Public rgdwPOV() As UInt32
'    Public rgbButtons() As Byte
'    Public lVX As Int32
'    Public lVY As Int32
'    Public lVZ As Int32
'    Public lVRx As Int32
'    Public lVRy As Int32
'    Public lVRz As Int32
'    Public rglVSlider() As Int32
'    Public lAX As Int32
'    Public lAY As Int32
'    Public lAZ As Int32
'    Public lARx As Int32
'    Public lARy As Int32
'    Public lARz As Int32
'    Public rglASlider() As Int32
'    Public lFX As Int32
'    Public lFY As Int32
'    Public lFZ As Int32
'    Public lFRx As Int32
'    Public lFRy As Int32
'    Public lFRz As Int32
'    Public rglFSlider() As Int32
'End Structure

Public Class HIDData

    Private botones(55) As Byte
    Public X As Integer = 0
    Public Y As Integer = 0
    Public mjX As Integer = 0
    Public mjY As Integer = 0
    Public Z As Integer = 0
    Public R As Integer = 0
    Public Rx As Integer = 0
    Public Ry As Integer = 0
    Public Slider As Integer = 0
    Public Slider2 As Integer = 0
    Public Rudder As Integer = 0
    Public POVs() As Integer = {-1, -1, -1, -1}

    Public limiteR As Integer = 255
    Public limitexy As Integer = 4095


    Private Declare Ansi Function AbrirDirectInput Lib "DirectInput.dll" (ByVal hwnd As IntPtr) As Byte
    Private Declare Auto Function GetTipoDirectInput Lib "directinput.dll" () As Byte
    Public Sub New(ByVal p As Form1)
        Dim t As Byte = GetTipoDirectInput()
        If t = 0 Then Exit Sub
        If t = 5 Then
            limitexy = 2047
            limiteR = 1023
        End If
        Dim h As New Threading.Thread(AddressOf Apertura)
        h.Start(p.Handle)
    End Sub
    Private Shared Sub Apertura(ByVal h As Object)
        If AbrirDirectInput(CType(h, IntPtr)) = 0 Then
            Traduce.Msg("directx_opening_failed", MsgBoxStyle.Exclamation)
        End If
    End Sub

    Private Declare Ansi Sub CerrarDirectInput Lib "DirectInput.dll" ()
    Public Sub Cerrar()
        CerrarDirectInput()
    End Sub

    Private Declare Ansi Function PollDirectInput Lib "directInput.dll" (ByVal buf() As Byte) As Byte
    Public Sub Poll()
        'Dim js As DIJOYSTATE2
        'ReDim js.rglSlider(1)
        'ReDim js.rgdwPOV(3)
        'ReDim js.rgbButtons(127)
        'ReDim js.rglVSlider(1)
        'ReDim js.rglASlider(1)
        'ReDim js.rglFSlider(1)
        Dim b() As Byte
        ReDim b(272)
        If PollDirectInput(b) = 1 Then
            'js = Runtime.InteropServices.Marshal.PtrToStructure(Ptr, js.GetType())
            'Runtime.InteropServices.Marshal.FreeHGlobal(Ptr)
            Dim Ptr As IntPtr = Runtime.InteropServices.Marshal.AllocHGlobal(272)
            Runtime.InteropServices.Marshal.Copy(b, 0, Ptr, 272)
            Me.X = Runtime.InteropServices.Marshal.ReadInt32(Ptr, 0) 'js.lX
            Me.Y = Runtime.InteropServices.Marshal.ReadInt32(Ptr, 4) 'js.lY
            Me.Z = Runtime.InteropServices.Marshal.ReadInt32(Ptr, 8) 'js.lZ
            Me.Rx = Runtime.InteropServices.Marshal.ReadInt32(Ptr, 12) 'js.lRx
            Me.Ry = Runtime.InteropServices.Marshal.ReadInt32(Ptr, 16) 'js.lRy
            Me.R = Runtime.InteropServices.Marshal.ReadInt32(Ptr, 20) 'js.lRz
            Me.Slider = Runtime.InteropServices.Marshal.ReadInt32(Ptr, 24) 'js.rglSlider(0)
            Me.Slider2 = Runtime.InteropServices.Marshal.ReadInt32(Ptr, 28) 'js.rglSlider(1)
            Me.POVs(0) = Runtime.InteropServices.Marshal.ReadInt32(Ptr, 32) 'js.rgdwPOV(0)
            Me.POVs(1) = Runtime.InteropServices.Marshal.ReadInt32(Ptr, 36) 'js.rgdwPOV(1)
            Me.POVs(2) = Runtime.InteropServices.Marshal.ReadInt32(Ptr, 40) 'js.rgdwPOV(2)
            Me.POVs(3) = Runtime.InteropServices.Marshal.ReadInt32(Ptr, 44) 'js.rgdwPOV(3)
            For i As Byte = 0 To 55
                botones(i) = Runtime.InteropServices.Marshal.ReadByte(Ptr, 48 + i) 'js.rgbButtons(i)
            Next
            Me.mjX = Runtime.InteropServices.Marshal.ReadInt32(Ptr, 176)
            Me.mjY = Runtime.InteropServices.Marshal.ReadInt32(Ptr, 180)
            Me.Rudder = Runtime.InteropServices.Marshal.ReadInt32(Ptr, 184)
            Runtime.InteropServices.Marshal.FreeHGlobal(Ptr)
        End If
    End Sub
    Public Function GetBoton(ByVal b As Integer) As Boolean
        Return botones(b)
    End Function

    Private Declare Auto Function CalibrarDirectInput Lib "directinput.dll" (ByVal b As Byte) As Byte
    Public Function Calibrar() As Byte
        If (limitexy = 2047) Then
            Return CalibrarDirectInput(1)
        Else
            Return CalibrarDirectInput(0)
        End If
    End Function

End Class
