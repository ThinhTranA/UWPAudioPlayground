using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Media.Audio;
using Windows.Media.Render;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;

namespace UWPAudioPlayground
{
    public class MainPageViewModel : ViewModelBase
    {
        public DelegateCommand PlayCommand { get; set; }
        public DelegateCommand StopCommand { get; set; }
        private AudioGraph authGraph;
        private DeviceInformation _selectedDevice;
        private AudioFileInputNode fileInputNode;
        private DispatcherTimer timer;
        private bool updatingPosition;
        public ObservableCollection<DeviceInformation> Devices { get; }

        public DeviceInformation SelectedDevice
        {
            get => _selectedDevice;
            set => _selectedDevice = value;
        }

        private TimeSpan duration;

        public TimeSpan Duration
        {
            get { return duration; }
            set {
                if (value.Equals(duration)) return;
                duration = value;
                OnPropertyChanged();
            }
        }

        private double volume;

        public double Volume
        {
            get { return volume; }
            set 
            {
                if(value.Equals(volume)) return;
                volume = value;
                OnPropertyChanged();
                if (fileInputNode != null)
                    fileInputNode.OutgoingGain = value / 100.0;
            }
        }

        private double playbackSpeed;

        public double PlaybackSpeed
        {
            get { return playbackSpeed; }
            set 
            {
                if(value.Equals(playbackSpeed)) return;
                playbackSpeed = value; 
                OnPropertyChanged();
                if (fileInputNode != null)
                    fileInputNode.PlaybackSpeedFactor = value / 100.0;
            }
        }

        private TimeSpan position;

        public TimeSpan Position
        {
            get { return position; }
            set 
            { 
                if(position.Equals(value)) return;
                position = value;
                OnPropertyChanged();
                if (!updatingPosition)
                {
                    fileInputNode?.Seek(position);
                }
            }
        }



        public MainPageViewModel()
        {
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(500);
            timer.Start();
            timer.Tick += TickOnTimer;
            PlaybackSpeed = 50;
            Volume = 50;

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

        private void TickOnTimer(object sender, object o)
        {
            try
            {
                updatingPosition = true;
                if(fileInputNode != null)
                {
                    Position = fileInputNode.Position;
                }
            } finally
            {
                updatingPosition = false;
            }
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
                fileInputNode = fileResult.FileInputNode;
                fileInputNode.AddOutgoingConnection(outputNode);
                Duration = fileInputNode.Duration;
                fileInputNode.PlaybackSpeedFactor = PlaybackSpeed / 100.0;
                fileInputNode.OutgoingGain = Volume / 100.0;
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
