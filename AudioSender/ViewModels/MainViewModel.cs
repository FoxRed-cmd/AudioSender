using AudioSender.Record;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;

namespace AudioSender.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    #region propertys and fields
    [ObservableProperty]
    private string _recordStatus = "Start Recording";
    [ObservableProperty]
    private string _saveFolderPath = Assembly.GetExecutingAssembly().Location;
    [ObservableProperty]
    private string _saveFileName = DateTime.Now.ToString("dd_MM_yyyy_HH_mm_ss");
    [ObservableProperty]
    private List<string>? _fileFormats = new()
    {
        "WAV", "MP3",
    };

    [ObservableProperty]
    private string _selectFormat = "MP3";

    [ObservableProperty]
    private string _sendStatus = "Start Sending";
    [ObservableProperty]
    private string _ipAddress = string.Empty;
    [ObservableProperty]
    private string _port = string.Empty;
    [ObservableProperty]
    private bool _isValidConnection;

    private AudioRecorder? _recorder;
    #endregion

    #region responding to property changes
    partial void OnIpAddressChanged(string value) => IsValidConnection = IsValidIpAndPort();

    partial void OnPortChanged(string value) => IsValidConnection = IsValidIpAndPort();
    #endregion

    #region commands
    [RelayCommand]
    public void StartRecordingCommand()
    {
        if (RecordStatus == "Start Recording")
        {
            RecordStatus = "Stop Recording";
            _recorder = new AudioRecorder();
            switch (SelectFormat)
            {
                case "WAV":
                    _recorder.StartRecording(SaveFolderPath, SaveFileName, AudioFileTypes.WAV);
                    break;
                case "MP3":
                    _recorder.StartRecording(SaveFolderPath, SaveFileName, AudioFileTypes.MP3);
                    break;
                default:
                    _recorder.StartRecording(SaveFolderPath, SaveFileName, AudioFileTypes.MP3);
                    break;
            }
        }
        else
        {
            _recorder?.StopRecording();
            RecordStatus = "Start Recording";
        }
    }

    [RelayCommand]
    public void StartSendingCommand()
    {
        if (SendStatus == "Start Sending")
        {
            SendStatus = "Stop Sending";
            _recorder = new AudioRecorder();
            _recorder.StartSendingToAnotherDevice(IPAddress.Parse(IpAddress), int.Parse(Port));
        }
        else
        {
            SendStatus = "Start Sending";
            _recorder?.StopRecording();
        }
    }

    [RelayCommand]
    public async void SelectFolderCommand()
    {
        var window = App.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;
        var topLevel = TopLevel.GetTopLevel(window.MainWindow.Content as Visual);
        FolderPickerOpenOptions options = new FolderPickerOpenOptions() { AllowMultiple = false };
        var folder = await topLevel.StorageProvider.OpenFolderPickerAsync(options);

        if (folder.Count != 0)
        {
            SaveFolderPath = folder[0].Path.LocalPath;
        }
    }
    #endregion

    #region private methods
    private bool IsValidIpAndPort()
    {
        if (IpAddress is null || IpAddress == string.Empty || Port is null || Port == string.Empty)
            return false;

        string pattern = @"^(?:[0-9]{1,3}\.){3}[0-9]{1,3}$";

        return Regex.IsMatch(IpAddress, pattern) && IPAddress.TryParse(IpAddress, out _) && int.TryParse(Port, out _);
    }
    #endregion
}