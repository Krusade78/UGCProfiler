﻿using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;


namespace Editor
{
    internal class CtlNumUpDown : TextBox
    {
        #region "Propiedades"
        private int _Valor = 1;
        public int Valor
        {
            get { return _Valor; }
            set
            {
                this.Background = System.Windows.Media.Brushes.Black;
                _Valor = value;
                this.Text = _Valor.ToString(); //SetValue(TextProperty, value.ToString());
                ValueChanged?.Invoke(this, null);
            }
        }
        public double Minimum { get; set; } = int.MinValue;
        public double Maximum { get; set; } = int.MaxValue;
        #endregion

        private bool eventosOn = true;
        public event EventHandler ValueChanged;

        static CtlNumUpDown()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CtlNumUpDown), new FrameworkPropertyMetadata(typeof(CtlNumUpDown)));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            base.Text = _Valor.ToString();
            base.KeyUp += TextBox1_KeyPress;
            base.KeyDown += TextBox1_KeyPress;
            base.PreviewKeyDown += TextBox1_KeyPress;
            base.PreviewKeyUp += TextBox1_KeyPress;
            base.PreviewTextInput += FtxtBox1_PreviewTextInput;
            base.TextChanged += CtlNumUpDown_TextChanged;
            ((RepeatButton)base.GetTemplateChild("btUp")).Click += CtlNumUp_Click;
            ((RepeatButton)base.GetTemplateChild("btDown")).Click += CtlNumDown_Click;
        }

        #region "eventos botones"
        private void CtlNumUp_Click(object sender, RoutedEventArgs e)
        {
            if (Valor < Maximum)
            {
                eventosOn = false;
                Valor++;
                eventosOn = true;
            }
        }

        private void CtlNumDown_Click(object sender, RoutedEventArgs e)
        {
            if (Valor > Minimum)
            {
                eventosOn = false;
                Valor--;
                eventosOn = true;
            }
        }
        #endregion

        #region "Eventos TextBox"
        private void FtxtBox1_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            foreach (char c in e.Text)
            {
                if ((c < '0') || (c > '9'))
                {
                    e.Handled = true;
                    return;
                }
            }
        }
        private void TextBox1_KeyPress(Object sender, KeyEventArgs e)
        {
            //'sólo números, signos, y borrar
            if ((e.KeyboardDevice.Modifiers != ModifierKeys.None) || (e.Key == Key.Space))
            {
                e.Handled = true;
                return;
            }
        }

        private void CtlNumUpDown_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!eventosOn || !this.IsLoaded)
                return;

            if (((TextBox)e.OriginalSource).Text.Trim() == "")
                return;

            try
            {
                int d = int.Parse(((TextBox)e.OriginalSource).Text);
            }
            catch
            {
                this.Background = System.Windows.Media.Brushes.DarkRed;
                return;
            }

            if ((int.Parse(((TextBox)e.OriginalSource).Text) >= Minimum) && (int.Parse(((TextBox)e.OriginalSource).Text) <= Maximum))
            {
                Valor = int.Parse(((TextBox)e.OriginalSource).Text);
            }
            else
                this.Background = System.Windows.Media.Brushes.DarkRed;
        }
        #endregion
    }
}
