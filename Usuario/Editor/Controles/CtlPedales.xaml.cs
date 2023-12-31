﻿using System;
using System.Windows;
using System.Windows.Controls;

namespace Editor
{
    /// <summary>
    /// Lógica de interacción para CtlPedales.xaml
    /// </summary>
    internal partial class CtlPedales : UserControl
    {
        private readonly CtlPropiedades Vista;

        public CtlPedales(CtlEditar padre)
        {
            InitializeComponent();
            Vista = padre.ctlPropiedades;
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            Vista.Ver(5, CEnums.Tipo.Eje, (String)EjeR.Content);
        }

        #region "ejes"
        private void PedalDer_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(0, CEnums.Tipo.Eje, (string)((Button)sender).Content);
        }

        private void PedalIzq_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(1, CEnums.Tipo.Eje, (string)((Button)sender).Content);
        }

        private void EjeR_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(5, CEnums.Tipo.Eje, (string)EjeR.Content);
        }
        #endregion
    }
}
