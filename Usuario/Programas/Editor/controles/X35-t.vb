Public Class X35T
    Public Sub New()
        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        Me.Top = 32
        Me.Left = 15
        Me.Width = 434
        Me.Height = 462
        Traducir()
    End Sub

    Private Sub Traducir()
        LabelPOV1.Text = Traduce.Txt("pov3")
        LabelPOV2.Text = Traduce.Txt("pov4")
        Buttonp11.Tag = Traduce.Txt("pov3_n")
        Buttonp12.Tag = Traduce.Txt("pov3_ne")
        Buttonp13.Tag = Traduce.Txt("pov3_e")
        Buttonp14.Tag = Traduce.Txt("pov3_se")
        Buttonp15.Tag = Traduce.Txt("pov3_s")
        Buttonp16.Tag = Traduce.Txt("pov3_sw")
        Buttonp17.Tag = Traduce.Txt("pov3_w")
        Buttonp18.Tag = Traduce.Txt("pov3_nw")
        Buttonp21.Tag = Traduce.Txt("pov4_n")
        Buttonp22.Tag = Traduce.Txt("pov4_ne")
        Buttonp23.Tag = Traduce.Txt("pov4_e")
        Buttonp24.Tag = Traduce.Txt("pov4_se")
        Buttonp25.Tag = Traduce.Txt("pov4_s")
        Buttonp26.Tag = Traduce.Txt("pov4_sw")
        Buttonp27.Tag = Traduce.Txt("pov4_w")
        Buttonp28.Tag = Traduce.Txt("pov4_nw")
        Buttond.Text = Traduce.Txt("buttond")
        ButtonMouse.Text = Traduce.Txt("buttonm")
        ButtonAux1.Text = Traduce.Txt("aux1")
        ButtonAux2.Text = Traduce.Txt("aux2")
        ButtonAux3.Text = Traduce.Txt("aux3")
        ButtonMode1.Text = Traduce.Txt("mode1")
        ButtonMode2.Text = Traduce.Txt("mode2")
        ButtonMode3.Text = Traduce.Txt("mode3")
        Buttonr.Text = Traduce.Txt("axisr")
        Buttonz.Text = Traduce.Txt("axisz")
        ButtonMinix.Text = Traduce.Txt("ministickx")
        ButtonMiniy.Text = Traduce.Txt("ministicky")
        ButtonRx.Text = Traduce.Txt("axisrx")
        ButtonRy.Text = Traduce.Txt("axisry")
    End Sub
#Region "Elementos X35"
    Private Sub Buttonp11_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Buttonp11.Click
        Vista.Ver(116, Buttonp11.Tag)
    End Sub
    Private Sub Buttonp12_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Buttonp12.Click
        Vista.Ver(117, Buttonp12.Tag)
    End Sub
    Private Sub Buttonp13_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Buttonp13.Click
        Vista.Ver(118, Buttonp13.Tag)
    End Sub
    Private Sub Buttonp14_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Buttonp14.Click
        Vista.Ver(119, Buttonp14.Tag)
    End Sub
    Private Sub Buttonp15_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Buttonp15.Click
        Vista.Ver(120, Buttonp15.Tag)
    End Sub
    Private Sub Buttonp16_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Buttonp16.Click
        Vista.Ver(121, Buttonp16.Tag)
    End Sub
    Private Sub Buttonp17_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Buttonp17.Click
        Vista.Ver(122, Buttonp17.Tag)
    End Sub
    Private Sub Buttonp18_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Buttonp18.Click
        Vista.Ver(123, Buttonp18.Tag)
    End Sub

    Private Sub Buttonp21_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Buttonp21.Click
        Vista.Ver(124, Buttonp21.Tag)
    End Sub
    Private Sub Buttonp22_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Buttonp22.Click
        Vista.Ver(125, Buttonp22.Tag)
    End Sub
    Private Sub Buttonp23_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Buttonp23.Click
        Vista.Ver(126, Buttonp23.Tag)
    End Sub
    Private Sub Buttonp24_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Buttonp24.Click
        Vista.Ver(127, Buttonp24.Tag)
    End Sub
    Private Sub Buttonp25_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Buttonp25.Click
        Vista.Ver(128, Buttonp25.Tag)
    End Sub
    Private Sub Buttonp26_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Buttonp26.Click
        Vista.Ver(129, Buttonp26.Tag)
    End Sub
    Private Sub Buttonp27_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Buttonp27.Click
        Vista.Ver(130, Buttonp27.Tag)
    End Sub
    Private Sub Buttonp28_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Buttonp28.Click
        Vista.Ver(131, Buttonp28.Tag)
    End Sub

    Private Sub Buttond_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Buttond.Click
        Vista.Ver(4, Buttond.Text)
    End Sub
    Private Sub ButtonMouse_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonMouse.Click
        Vista.Ver(5, ButtonMouse.Text)
    End Sub

    Private Sub ButtonAux1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonAux1.Click
        Vista.Ver(11, ButtonAux1.Text)
    End Sub
    Private Sub ButtonAux2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonAux2.Click
        Vista.Ver(12, ButtonAux2.Text)
    End Sub
    Private Sub ButtonAux3_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonAux3.Click
        Vista.Ver(13, ButtonAux3.Text)
    End Sub
    Private Sub ButtonMode1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonMode1.Click
        Vista.Ver(8, ButtonMode1.Text)
    End Sub
    Private Sub ButtonMode2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonMode2.Click
        Vista.Ver(9, ButtonMode2.Text)
    End Sub
    Private Sub ButtonMode3_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonMode3.Click
        Vista.Ver(10, ButtonMode3.Text)
    End Sub

    Private Sub ButtonRx_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonRx.Click
        Vista.Ver(68, ButtonRx.Text)
    End Sub
    Private Sub ButtonRy_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonRy.Click
        Vista.Ver(69, ButtonRy.Text)
    End Sub
    Private Sub ButtonMinix_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonMinix.Click
        Vista.Ver(71, ButtonMinix.Text)
    End Sub
    Private Sub ButtonMiniy_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonMiniy.Click
        Vista.Ver(72, ButtonMiniy.Text)
    End Sub
    Private Sub Buttonz_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Buttonz.Click
        Vista.Ver(67, Buttonz.Text)
    End Sub
    Private Sub Buttonr_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Buttonr.Click
        Vista.Ver(66, Buttonr.Text)
    End Sub
#End Region

    Private Sub X35T_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Vista.Ver(67, Buttonz.Text)
    End Sub
End Class
