using AudioSender.Record;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace AudioSender.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _recordStatus = "Start Record";
    [ObservableProperty]
    private string _saveFolderPath = Assembly.GetExecutingAssembly().Location;
    [ObservableProperty]
    private string _saveFileName = DateTime.Now.ToString();
    [ObservableProperty]
    private List<string>? _fileFormats = new()
    {
        "WAV", "MP3",
    };

    [ObservableProperty]
    private string _selectFormat = "MP3";

    private AudioRecorder? _recorder;
    private UserControl? _userControl;

    public MainViewModel()
    {

    }

    public MainViewModel(UserControl view)
    {
        _userControl = view;
    }

    [RelayCommand]
    public void StartRecordCommand()
    {
        if (RecordStatus == "Start Record")
        {
            RecordStatus = "Stop Record";
            _recorder = new AudioRecorder();
            switch (_selectFormat)
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
            RecordStatus = "Start Record";
        }
    }

    [RelayCommand]
    public async void SelectFolder()
    {
        var topLevel = TopLevel.GetTopLevel(_userControl);
        FolderPickerOpenOptions options = new FolderPickerOpenOptions() { AllowMultiple = false };
        var folder = await topLevel.StorageProvider.OpenFolderPickerAsync(options);

        if (folder.Count != 0)
        {
            SaveFolderPath = folder[0].Path.LocalPath;
        }
    }
}




//IClassicDesktopStyleApplicationLifetime w = App.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;