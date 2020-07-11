﻿using Microsoft.Win32;
using MRecipeStructure.Classes.MRecipeStructure;
using MRecipeStructure.DialogsDialogs.DialogViewModel;
using MSolvLib.Classes.ProcessConfiguration;
using SharpGL.WPF;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace MRecipeStructure.Dialogs
{
    /// <summary>
    /// Interaction logic for ImportPatternDialog.xaml
    /// </summary>
    public partial class ImportPatternDialog : Window
    {
        private ImportPatternDialogViewModel _viewModel;

        public MRecipe Recipe { get; private set; }

        public ImportPatternDialog(IEnumerable<IProcessConfiguration> availableProcessConfigurations, string defaultPatternDirectory, string defaultRecipeDirectory)
        {
            InitializeComponent();
            _viewModel = new ImportPatternDialogViewModel(availableProcessConfigurations, defaultPatternDirectory, defaultRecipeDirectory);
            DataContext = _viewModel;
        }

        #region Section: Dialog Result Validation and Callbacks

        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog();
            dialog.AddExtension = true;
            dialog.Title = "Save Recipe";
            dialog.Filter = MRecipe.FileFilter;
            dialog.DefaultExt = MRecipe.DefaultFileExtension;
            dialog.FileName = Path.GetFileNameWithoutExtension(_viewModel.PatternFilePath);
            dialog.InitialDirectory = _viewModel.DefaultRecipeDirectory;

            if (dialog.ShowDialog() != true)
                return;

            try
            {
                Recipe = _viewModel.GenerateRecipe(dialog.FileName);

                if (Recipe == null)
                    throw new Exception("Missing or Invalid Parameters");

                DialogResult = true;
            }
            catch (Exception exp)
            {
                MessageBox.Show(
                    exp.Message, "Error"
                );
            }
        }

        #endregion

        #region Section: Open GL Control

        private void OpenGLControl_OpenGLDraw(object sender, SharpGL.SceneGraph.OpenGLEventArgs args)
        {
            _viewModel.MShader.Render(args.OpenGL);
        }

        private void OpenGLControl_OpenGLInitialized(object sender, SharpGL.SceneGraph.OpenGLEventArgs args)
        {
            _viewModel.MShader.OnInitialised(args.OpenGL);
        }

        private void OpenGLControl_Resized(object sender, SharpGL.SceneGraph.OpenGLEventArgs args)
        {
            var reference = (sender as OpenGLControl);
            _viewModel.MShader.OnResize(args.OpenGL, reference.ActualWidth, reference.ActualHeight);
        }

        private void OpenGLControl_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            _viewModel.MShader.Zoom(e.Delta);
        }

        private void OpenGLControl_MouseMove(object sender, MouseEventArgs e)
        {
            var reference = (sender as OpenGLControl);
            var pos = e.GetPosition(reference);
            var smallestSize = Math.Min(reference.ActualWidth, reference.ActualHeight);
            _viewModel.MShader.UpdateMouse(pos.X / smallestSize, pos.Y / smallestSize);
            _viewModel.UpdateMousePosition();
        } 

        #endregion
    }
}
