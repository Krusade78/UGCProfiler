Public Class EditorPinkieModos
    Private padre As Principal
    Private pinkie As Boolean

    Public Sub New(ByVal pk As Boolean)
        InitializeComponent()
        pinkie = pk
    End Sub

    Private Sub EditorPinkieModos_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        If pinkie Then
            Me.Text = Traduce.Txt("assign_to_pinkie")
            Label1.Text = Traduce.Txt("pinkie_text")
        Else
            Me.Text = Traduce.Txt("assign_to_modes")
            Label1.Text = Traduce.Txt("modes_text")
        End If
        Button1.Text = Traduce.Txt("accept")
        Button2.Text = Traduce.Txt("cancel")
        padre = Me.Owner
    End Sub

    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click
        If pinkie Then GuardarPinkie() Else GuardarModos()
        Me.DialogResult = Windows.Forms.DialogResult.OK
        Me.Close()
    End Sub

    Private Sub Button2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button2.Click
        Me.DialogResult = Windows.Forms.DialogResult.Cancel
        Me.Close()
    End Sub

    Private Sub GuardarPinkie()
        Dim idx As Integer

        'on
        idx = padre.ComboBoxAssigned.Items.IndexOf("<" & padre.ToolStripSplitPinkie.DropDownItems(1).Text & ">")
        If idx = -1 Then
            idx = padre.ComboBoxAssigned.Items.Add("<" & padre.ToolStripSplitPinkie.DropDownItems(1).Text & ">")
            padre.ComboBoxMacro.Items.Add("<" & padre.ToolStripSplitPinkie.DropDownItems(1).Text & ">")
        End If

        Dim macro As New ArrayList
        Dim comando As UInt16
        comando = 16 + (1 << 8)
        macro.Add(comando)
        comando = 23 + (1 << 8)
        macro.Add(comando)
        padre.datos.AñadirIndice(idx - 1, macro)
        'For p As Byte = 0 To 1
        For m As Byte = 0 To 2
            For a As Byte = 0 To 2
                padre.datos.SetMapaBotones_Estado(0, m, a, 6, 0)
                padre.datos.SetMapaBotones_Indices(0, m, a, 6, 0, idx)
                padre.datos.SetMapaBotones_Estado(0, m, a, 6, 0)
                padre.datos.SetMapaBotones_Indices(0, m, a, 6, 1, 0)
            Next
        Next
        'Next

        'off
        idx = padre.ComboBoxAssigned.Items.IndexOf("<" & padre.ToolStripSplitPinkie.DropDownItems(0).Text & ">")
        If idx = -1 Then
            idx = padre.ComboBoxAssigned.Items.Add("<" & padre.ToolStripSplitPinkie.DropDownItems(0).Text & ">")
            padre.ComboBoxMacro.Items.Add("<" & padre.ToolStripSplitPinkie.DropDownItems(0).Text & ">")
        End If

        macro = New ArrayList
        comando = 16
        macro.Add(comando)
        comando = 23
        macro.Add(comando)
        padre.datos.AñadirIndice(idx - 1, macro)
        'For p As Byte = 0 To 1
        For m As Byte = 0 To 2
            For a As Byte = 0 To 2
                padre.datos.SetMapaBotones_Estado(1, m, a, 6, 0)
                padre.datos.SetMapaBotones_Indices(1, m, a, 6, 0, 0)
                padre.datos.SetMapaBotones_Estado(1, m, a, 6, 0)
                padre.datos.SetMapaBotones_Indices(1, m, a, 6, 1, idx)
            Next
        Next
        'Next
    End Sub
    Private Sub GuardarModos()
        Dim idx As Integer

        For modo As Byte = 1 To 3
            idx = padre.ComboBoxAssigned.Items.IndexOf("<" & padre.ToolStripSplitModo.DropDownItems(modo - 1).Text & ">")
            If idx = -1 Then
                idx = padre.ComboBoxAssigned.Items.Add("<" & padre.ToolStripSplitModo.DropDownItems(modo - 1).Text & ">")
                padre.ComboBoxMacro.Items.Add("<" & padre.ToolStripSplitModo.DropDownItems(modo - 1).Text & ">")
            End If

            Dim macro As New ArrayList
            Dim comando As UInt16
            'texto x52
            comando = 24 + (1 << 8)
            macro.Add(comando)
            Dim st As String = padre.ToolStripSplitModo.DropDownItems(modo - 1).Text
            For j As Byte = 0 To st.Length - 1
                comando = 24 + (CInt(System.Text.ASCIIEncoding.ASCII.GetBytes(st)(j)) << 8)
                macro.Add(comando)
            Next
            comando = 56
            macro.Add(comando)
            '------------
            comando = 14 + ((modo - 1) << 8)
            macro.Add(comando)
            padre.datos.AñadirIndice(idx - 1, macro)
            For p As Byte = 0 To 1
                For m As Byte = 0 To 2
                    For a As Byte = 0 To 2
                        padre.datos.SetMapaBotones_Estado(p, m, a, 7 + modo, 0)
                        padre.datos.SetMapaBotones_Indices(p, m, a, 7 + modo, 0, idx)
                        padre.datos.SetMapaBotones_Indices(p, m, a, 7 + modo, 1, 0)
                    Next
                Next
            Next
        Next
    End Sub
End Class