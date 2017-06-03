using Orbital7.Extensions;
using Orbital7.Extensions.NETFramework.WPF;
using Orbital7.MyGames;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace DesktopApp
{
    /// <summary>
    /// Interaction logic for EditPlatformConfigDialog.xaml
    /// </summary>
    public partial class EditPlatformConfigDialog : Window
    {
        public PlatformConfig PlatformConfig { get; private set; }

        public EditPlatformConfigDialog(PlatformConfig platformConfig, string action)
        {
            InitializeComponent();
            App.SetWindowFont(this);
            this.Title = action + " Platform Config";

            this.PlatformConfig = XMLSerializationHelper.CloneObject<PlatformConfig>(platformConfig);
            WPFHelper.FillComboBox(comboPlatform, EnumHelper.EnumToList<Platform>(),
                this.PlatformConfig.Platform);
            this.DataContext = this.PlatformConfig;

            UpdateGameConfigs();
            UpdateEmulators();
        }

        private void UpdateGameConfigs()
        {
            WPFHelper.FillListBox(listGameConfigs, this.PlatformConfig.GameConfigs, false);
            ValidateGameConfigsToolbar();
        }

        private void UpdateEmulators()
        {
            WPFHelper.FillListBox(listEmulators, this.PlatformConfig.Emulators, false);
            ValidateEmulatorsToolbar();
        }

        private void ValidateGameConfigsToolbar()
        {
            buttonDeleteGameConfig.IsEnabled = listGameConfigs.SelectedItem != null;
        }

        private void ValidateEmulatorsToolbar()
        {
            buttonDeleteEmulator.IsEnabled = listEmulators.SelectedItem != null;
        }

        private void listGameConfigs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ValidateGameConfigsToolbar();
        }

        private void buttonAddGameConfig_Click(object sender, RoutedEventArgs e)
        {
            string gameConfig = CommonDialogsHelper.ShowInputTextBoxDialog(this, "Enter Game Config File Name:", "Add Game Config", "TODO.config");
            if (!String.IsNullOrEmpty(gameConfig))
            {
                this.PlatformConfig.GameConfigs.Add(gameConfig);
                UpdateGameConfigs();
                listGameConfigs.SelectedItem = gameConfig;
            }
        }
        
        private void buttonDeleteGameConfig_Click(object sender, RoutedEventArgs e)
        {
            string gameConfig = listGameConfigs.SelectedItem as string;
            if (gameConfig != null)
            {
                if (MessageBoxHelper.AskQuestion(this, "Are you sure you want to delete the Game Config \"" + gameConfig + "\"?"))
                {
                    this.PlatformConfig.GameConfigs.Remove(gameConfig);
                    UpdateGameConfigs();
                }
            }
        }

        private void listEmulators_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ValidateEmulatorsToolbar();
        }

        private void buttonAddEmulator_Click(object sender, RoutedEventArgs e)
        {
            string emulator = CommonDialogsHelper.ShowInputTextBoxDialog(this, "Enter Emulator Name:", "Add Game Config");
            if (!String.IsNullOrEmpty(emulator))
            {
                this.PlatformConfig.Emulators.Add(emulator);
                UpdateEmulators();
                listEmulators.SelectedItem = emulator;
            }
        }

        private void buttonDeleteEmulator_Click(object sender, RoutedEventArgs e)
        {
            string emulator = listEmulators.SelectedItem as string;
            if (emulator != null)
            {
                if (MessageBoxHelper.AskQuestion(this, "Are you sure you want to delete the Emulator \"" + emulator + "\"?"))
                {
                    this.PlatformConfig.Emulators.Remove(emulator);
                    UpdateEmulators();
                }
            }
        }

        private void buttonDone_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void comboPlatform_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.PlatformConfig.Platform = (Platform)comboPlatform.SelectedItem;
        }
    }
}
