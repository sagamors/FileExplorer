﻿using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace FileExplorer.Attached
{
    public class ControlDoubleClick : DependencyObject
    {
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.RegisterAttached("Command", typeof(ICommand), typeof(ControlDoubleClick), new PropertyMetadata(OnChangedCommand));

        public static ICommand GetCommand(Control target)
        {
            return (ICommand)target.GetValue(CommandProperty);
        }

        public static void SetCommand(Control target, ICommand value)
        {
            target.SetValue(CommandProperty, value);
        }

        private static void OnChangedCommand(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            Control control = o as Control;
            control.PreviewMouseDoubleClick += Element_PreviewMouseDoubleClick;
        }

        private static void Element_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Control control = sender as Control;
            ICommand command = GetCommand(control);

            if (command.CanExecute(null))
            {
                command.Execute(null);
                e.Handled = true;
            }
        }
    }
}
