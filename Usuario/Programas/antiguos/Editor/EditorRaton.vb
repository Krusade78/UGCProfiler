Public Class EditorRaton

    Private Sub EditorRaton_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        Me.Text = Traduce.Txt("mouse_configuration")
        Label1.Text = Traduce.Txt("mouse_text")
        Button1.Text = Traduce.Txt("accept")
        Button2.Text = Traduce.Txt("cancel")
        NumericUpDown1.Value = CType(Me.Owner, Principal).datos.getRefrescoRaton()
    End Sub

    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click
        CType(Me.Owner, Principal).datos.SetRefrescoRaton(NumericUpDown1.Value)
        Me.Close()
    End Sub

    Private Sub Button2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button2.Click
        Me.Close()
    End Sub
End Class