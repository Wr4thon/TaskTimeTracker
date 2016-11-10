﻿using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

using TaskTimeTracker.Client.Contract;
using TaskTimeTracker.Client.Contract.Configuration;
using TaskTimeTracker.Client.Ui.Commands;
using TaskTimeTracker.Client.Ui.ConfigurationWindow;
using TaskTimeTracker.Client.Ui.Inbox;

namespace TaskTimeTracker.Client.Ui.MainWindow {
  internal class MainWindowViewModel {
    private ObservableCollection<Task> _tasks;
    private Visibility _mainWindowVisibility;
    private IConfigurationWindowViewModel _configViewModel;
    private IConfigurationController _configurationController;

    public Task SelectedTask { get; set; }

    public ObservableCollection<Task> Tasks {
      get { return this._tasks; }
      set {
        this._tasks = value;
        OnPropertyChanged();
      }
    }

    public Visibility MainWindowVisibility {
      get { return this._mainWindowVisibility; }
      set {
        this._mainWindowVisibility = value;
        OnPropertyChanged();
      }
    }

    /// <summary>
    /// Adds a TaskStamp
    /// </summary>
    public ICommand AddCommand { get; set; }

    /// <summary>
    /// Removes a TaskStamp
    /// </summary>
    public ICommand RemoveCommand { get; set; }

    /// <summary>
    /// Command to open the ConfigurationWindow
    /// </summary>
    public ICommand ConfigCommand { get; set; }

    /// <summary>
    /// Dem Configuration
    /// </summary>
    public IConfiguration Configuration { get; set; }

    public ICommand MouseDoubleClick { get; set; }

    public MainWindowViewModel(IConfigurationController configurationController) {
      this.Tasks = new ObservableCollection<Task>();
      this.AddCommand = new RelayCommand(AddExecute);
      this.RemoveCommand = new RelayCommand(RemoveExecute);
      this.ConfigCommand = new RelayCommand(ConfigExecute);
      this.MouseDoubleClick = new RelayCommand(this.MouseDoubleClickExecute);
      this.MainWindowVisibility = Visibility.Visible;
      this._configurationController = configurationController;
      this.Configuration = this._configurationController.Configuration;
    }

    private void MouseDoubleClickExecute(object o) {
      if (this.SelectedTask == null || this.SelectedTask.EditMode) {
        return;
      }

      this.SelectedTask.EnterEditMode();
    }

    private void ConfigExecute(object obj) {
      ConfigurationWindow.ConfigurationWindow configWindow = new ConfigurationWindow.ConfigurationWindow();
      configWindow.ConfigurationController = this._configurationController;
      ConfigurationViewModelController configurationViewModelController = new ConfigurationViewModelController(configWindow);
      this._configViewModel = configurationViewModelController.FromConfiguration(this.Configuration);
      configWindow.ViewModel = this._configViewModel;
      configWindow.ShowDialog();
      if (this.Configuration.CompareTo(this._configurationController.Configuration) != 0) {
        this._configurationController.Save();
      }
    }

    private void RemoveExecute(object obj) {
      if (MessageBox.Show("Sure wanna delete?", "check", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.No) != MessageBoxResult.Yes) {
        return;
      }

      this.Tasks.Remove(obj as Task);
    }

    private void AddExecute(object o) {
      Inbox.Inbox inbox = new Inbox.Inbox();
      InboxViewModel vm = new InboxViewModel(inbox);
      inbox.DataContext = vm;
      bool? b = inbox.ShowDialog();

      if (!b.HasValue || !b.Value) { return; }

      string text = vm.Text;

      DateTime dateTime = DateTime.Now;
      this.Tasks.Add(new Task(dateTime, text));
    }


    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
      this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
  }
}