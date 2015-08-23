Public Class X36F
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
        'Elementos
        LabelPOV1.Text = Traduce.Txt("pov1")
        LabelPOV2.Text = Traduce.Txt("pov2")
        ButtonX.Text = Traduce.Txt("axisx")
        ButtonY.Text = Traduce.Txt("axisy")
        ButtonLaunch.Text = Traduce.Txt("buttonlaunch")
        ButtonA.Text = Traduce.Txt("buttona")
        ButtonB.Text = Traduce.Txt("buttonb")
        ButtonC.Text = Traduce.Txt("buttonc")
        ButtonPinkie.Text = Traduce.Txt("pinkie")
        ButtonTrigger.Text = Traduce.Txt("trigger")
        Buttonp11.Tag = Traduce.Txt("pov1_n")
        Buttonp12.Tag = Traduce.Txt("pov1_ne")
        Buttonp13.Tag = Traduce.Txt("pov1_e")
        Buttonp14.Tag = Traduce.Txt("pov1_se")
        Buttonp15.Tag = Traduce.Txt("pov1_s")
        Buttonp16.Tag = Traduce.Txt("pov1_sw")
        Buttonp17.Tag = Traduce.Txt("pov1_w")
        Buttonp18.Tag = Traduce.Txt("pov1_nw")
        Buttonp21.Tag = Traduce.Txt("pov2_n")
        Buttonp22.Tag = Traduce.Txt("pov2_ne")
        Buttonp23.Tag = Traduce.Txt("pov2_e")
        Buttonp24.Tag = Traduce.Txt("pov2_se")
        Buttonp25.Tag = Traduce.Txt("pov2_s")
        Buttonp26.Tag = Traduce.Txt("pov2_sw")
        Buttonp27.Tag = Traduce.Txt("pov2_w")
        Buttonp28.Tag = Traduce.Txt("pov2_nw")
    End Sub

#Region "Elementos X36"
    Private Sub Buttonp11_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Buttonp11.Click
        Vista.Ver(100, Buttonp11.Tag)
    End Sub
    Private Sub Buttonp12_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Buttonp12.Click
        Vista.Ver(101, Buttonp12.Tag)
    End Sub
    Private Sub Buttonp13_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Buttonp13.Click
        Vista.Ver(102, Buttonp13.Tag)
    End Sub
    Private Sub Buttonp14_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Buttonp14.Click
        Vista.Ver(103, Buttonp14.Tag)
    End Sub
    Private Sub Buttonp15_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Buttonp15.Click
        Vista.Ver(104, Buttonp15.Tag)
    End Sub
    Private Sub Buttonp16_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Buttonp16.Click
        Vista.Ver(105, Buttonp16.Tag)
    End Sub
    Private Sub Buttonp17_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Buttonp17.Click
        Vista.Ver(106, Buttonp17.Tag)
    End Sub
    Private Sub Buttonp18_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Buttonp18.Click
        Vista.Ver(107, Buttonp18.Tag)
    End Sub

    Private Sub Buttonp21_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Buttonp21.Click
        Vista.Ver(108, Buttonp21.Tag)
    End Sub
    Private Sub Buttonp22_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Buttonp22.Click
        Vista.Ver(109, Buttonp22.Tag)
    End Sub
    Private Sub Buttonp23_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Buttonp23.Click
        Vista.Ver(110, Buttonp23.Tag)
    End Sub
    Private Sub Buttonp24_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Buttonp24.Click
        Vista.Ver(111, Buttonp24.Tag)
    End Sub
    Private Sub Buttonp25_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Buttonp25.Click
        Vista.Ver(112, Buttonp25.Tag)
    End Sub
    Private Sub Buttonp26_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Buttonp26.Click
        Vista.Ver(113, Buttonp26.Tag)
    End Sub
    Private Sub Buttonp27_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Buttonp27.Click
        Vista.Ver(114, Buttonp27.Tag)
    End Sub
    Private Sub Buttonp28_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Buttonp28.Click
        Vista.Ver(115, Buttonp28.Tag)
    End Sub

    Private Sub ButtonA_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonA.Click
        Vista.Ver(1, ButtonA.Text)
    End Sub
    Private Sub ButtonB_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonB.Click
        Vista.Ver(2, ButtonB.Text)
    End Sub
    Private Sub ButtonC_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonC.Click
        Vista.Ver(7, ButtonC.Text)
    End Sub
    Private Sub ButtonLaunch_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonLaunch.Click
        Vista.Ver(3, ButtonLaunch.Text)
    End Sub
    Private Sub ButtonTrigger_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonTrigger.Click
        Vista.Ver(0, ButtonTrigger.Text)
    End Sub

    Private Sub ButtonPinkie_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonPinkie.Click
        Vista.Ver(6, ButtonPinkie.Text)
    End Sub

    Private Sub ButtonX_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonX.Click
        Vista.Ver(64, ButtonX.Text)
    End Sub
    Private Sub ButtonY_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonY.Click
        Vista.Ver(65, ButtonY.Text)
    End Sub
#End Region

    Private Sub X36F_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Vista.Ver(64, ButtonX.Text)
    End Sub
End Class
