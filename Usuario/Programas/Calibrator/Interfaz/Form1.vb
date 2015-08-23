Public Class Form1
    Public tipo As Byte = 255


    Private Sub RadioButton1_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles RadioButton1.CheckedChanged
        If tipo = 255 Then Exit Sub
        If Me.RadioButton1.Checked Then
            Dim c As New Info()
            c.Top = 30
            Me.Controls.Add(c)
        Else
            Me.Controls(Me.Controls.Count - 1).Dispose()
            'Me.Controls.RemoveAt(Me.Controls.Count - 1)
        End If
    End Sub

    Private Sub RadioButton2_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles RadioButton2.CheckedChanged
        If Me.RadioButton2.Checked Then
            Dim c As New HIDCal()
            c.Top = 30
            Me.Controls.Add(c)
        Else
            Me.Controls(Me.Controls.Count - 1).Dispose()
            'Me.Controls.RemoveAt(Me.Controls.Count - 1)
        End If
    End Sub

    Private Sub RadioButton3_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles RadioButton3.CheckedChanged
        If Me.RadioButton3.Checked Then
            Dim c As New X36Cal()
            c.Top = 30
            Me.Controls.Add(c)
        Else
            Me.Controls(Me.Controls.Count - 1).Dispose()
            'Me.Controls.RemoveAt(Me.Controls.Count - 1)
        End If
    End Sub

    Private Sub RadioButton4_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles RadioButton4.CheckedChanged
        If Me.RadioButton4.Checked Then
            Dim c As New X52Mfd()
            c.Top = 30
            Me.Controls.Add(c)
        Else
            Me.Controls(Me.Controls.Count - 1).Dispose()
            'Me.Controls.RemoveAt(Me.Controls.Count - 1)
        End If
    End Sub

    Private Declare Auto Function GetTipoDirectInput Lib "directinput.dll" () As Byte
    Private Sub Form1_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Traducir()
        Select Case GetTipoDirectInput()
            Case 1
                tipo = 1
                Label1.Text = "   Saitek X36"
            Case 2
                tipo = 1
                Label1.Text = "   Saitek X36+X35"
            Case 3
                tipo = 0
                Label1.Text = "   Saitek X36 USB"
            Case 4
                tipo = 0
                Label1.Text = "   Saitek X45"
            Case 5
                tipo = 2
                Label1.Text = "   Saitek X52"
            Case Else
                Me.Close()
                Exit Sub
        End Select
        'Dim applicationDevice As DirectInput.Device = Nothing
        'For Each instance As DirectInput.DeviceInstance In DirectInput.Manager.GetDevices(DirectInput.DeviceClass.GameControl, DirectInput.EnumDevicesFlags.AttachedOnly)
        '    If instance.InstanceName = "Saitek X36 gameport" Or instance.InstanceName = "Saitek X36+X35 gameport" Then
        '        tipo = 1
        '        Label1.Text = "   " & instance.InstanceName.Replace(" gameport", "")
        '        applicationDevice = New DirectInput.Device(instance.InstanceGuid)
        '        Exit For
        '    ElseIf instance.InstanceName = "Saitek X36 USB" Or instance.InstanceName = "Saitek X45" Then
        '        tipo = 0
        '        Label1.Text = "   " & instance.InstanceName
        '        applicationDevice = New DirectInput.Device(instance.InstanceGuid)
        '        Exit For
        '    ElseIf instance.InstanceName = "Saitek X52" Or instance.InstanceName = "Saitek X52 Flight Control System" Then
        '        tipo = 2
        '        Label1.Text = "   " & instance.InstanceName
        '        applicationDevice = New DirectInput.Device(instance.InstanceGuid)
        '        Exit For
        '    End If
        'Next instance

        If tipo = 1 Then Me.RadioButton3.Visible = True Else If tipo = 2 Then RadioButton4.Visible = True
        Dim c As New Info()
        c.Top = 30
        Me.Controls.Add(c)
        Me.ActiveControl = c

    End Sub

    Private Sub Traducir()
        RadioButton1.Text = Traduce.Txt("joystick_text")
        RadioButton2.Text = Traduce.Txt("dx_calibration")
        RadioButton3.Text = Traduce.Txt("x36_calibration")
        RadioButton4.Text = Traduce.Txt("x52_setup")
    End Sub

End Class
