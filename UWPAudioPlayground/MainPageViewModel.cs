using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Media.Audio;
using Windows.Media.Render;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace UWPAudioPlayground
{
    public class MainPageViewModel : ViewModelBase
    {
        public DelegateCommand PlayCommand { get; set; }
        public DelegateCommand StopCommand { get; set; }
        private AudioGraph authGraph;
        private DeviceInformation _selectedDevice;
        public ObservableCollection<DeviceInformation> Devices { get; }

        public DeviceInformation SelectedDevice
        {
            get => _selectedDevice;
            set => _selectedDevice = value;
        }

        public MainPageViewModel()
        {
            PlayCommand = new DelegateCommand(Play);
            StopCommand = new DelegateCommand(Stop);
            Devices = new ObservableCollection<DeviceInformation>();
        }

        public async Task InitializeAsync()
        {
            var outputDevices = await DeviceInformation.FindAllAsync(DeviceClass.AudioRender);
            foreach (var device in outputDevices.Where(d => d.IsEnabled))
            {
                Devices.Add(device);
            }

            SelectedDevice = Devices.FirstOrDefault(d => d.IsDefault);
        }


        private async void Play()
        {
            if (authGraph == null)
            {
                var settings = new AudioGraphSettings(AudioRenderCategory.Media);
                settings.PrimaryRenderDevice = SelectedDevice;
                var createResult = await AudioGraph.CreateAsync(settings);
                if(createResult.Status != AudioGraphCreationStatus.Success) return;
                authGraph = createResult.Graph;
                var deviceResult = await authGraph.CreateDeviceOutputNodeAsync();
                if(deviceResult.Status != AudioDeviceNodeCreationStatus.Success) return;
                var outputNode = deviceResult.DeviceOutputNode;
                var file = await SelectPlaybackFile();
                if(file == null) return;
                var fileResult = await authGraph.CreateFileInputNodeAsync(file);
                if(fileResult.Status != AudioFileNodeCreationStatus.Success) return;
                var fileInputNode = fileResult.FileInputNode;
                fileInputNode.AddOutgoingConnection(outputNode);
            }
            authGraph.Start();
        }

        private async Task<IStorageFile> SelectPlaybackFile()
        {
            var picker = new FileOpenPicker();
            picker.ViewMode = PickerViewMode.List;
            picker.SuggestedStartLocation = PickerLocationId.Desktop;
            picker.FileTypeFilter.Add(".mp3");
            picker.FileTypeFilter.Add(".aac");
            picker.FileTypeFilter.Add(".wav");
            var file = await picker.PickSingleFileAsync();
            return file;
        }
        private void Stop()
        {
            authGraph?.Stop();
        }

    }
}
