using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Media.Audio;
using Windows.Media.Render;

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
            }
            authGraph.Start();
        }
        private void Stop()
        {
            authGraph?.Stop();
        }
    }
}
