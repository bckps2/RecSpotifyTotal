using ConsoleApp1.InformationTracks;
using NAudio.Lame;
using NAudio.Wave;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace ConsoleApp1.RecordingSporitfy
{
    public partial class SpotifyRecord
    {
        private readonly DeezerApi dezzer;
        private readonly Process spotifyProcess;

        public SpotifyRecord()
        {
            dezzer = new DeezerApi();
            spotifyProcess = Process.GetProcessesByName("Spotify")[0];
        }

        const int WM_APPCOMMAND = 0x0319;
        const int APPCOMMAND_MEDIA_PLAY_PAUSE = 0xE0000;
        string newFolder = string.Empty;
        int songNumbers = 0;
        [DllImport("user32.dll")]
        public static extern IntPtr SendMessageW(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern int GetWindowText(IntPtr hWnd, IntPtr lpString, int nMaxCount);

        public async Task StartRecording()
        {
            FolderToSave();
            SetNumbersSongs();
            await RecMusic();
        }

        private void SetNumbersSongs() 
        {
            while (songNumbers == 0) 
            {
                Console.WriteLine("Por favor indique el numero de canciones");
                var number = Console.ReadLine();
                if (int.TryParse(number, out songNumbers))
                {
                    Console.WriteLine("Vamos!");
                }
                else
                {
                    Console.WriteLine("Intentalo de nuevo!");
                }
            }
        }

        private void FolderToSave()
        {
            Console.WriteLine("Por favor introducir nombre de la lista => ");
            newFolder = Console.ReadLine();

            if (!Directory.Exists(newFolder))
            {
                Directory.CreateDirectory(newFolder);
                Console.WriteLine("La carpeta se creó correctamente.");
            }
            else
            {
                Console.WriteLine("La carpeta ya existe.");
            }
        }

        private async Task RecMusic()
        {
            IntPtr spotifyHandle = spotifyProcess.MainWindowHandle;
            WaveFormat waveFormat = new(44100, 16, 2);
            string currentTitle = string.Empty;

            for (int i = 0; i < songNumbers; i++)
            {
                try
                {
                    PlayPause();
                    string newTitle = GetWindowTitle(spotifyHandle);

                    while (string.IsNullOrEmpty(newTitle))
                    {
                        newTitle = GetWindowTitle(spotifyHandle);
                    }

                    var titleSong = MyRegex().Replace(newTitle, "");
                    string outputFile = @$"{newFolder}/{titleSong}.mp3";
                    currentTitle = newTitle;

                    WaveInEvent waveIn = new()
                    {
                        WaveFormat = waveFormat
                    };

                    LameMP3FileWriter writer = new(outputFile, waveIn.WaveFormat, 320);

                    waveIn.DataAvailable += (sender, e) =>
                    {
                        writer.Write(e.Buffer, 0, e.BytesRecorded);
                    };

                    Console.WriteLine("Grabando..." + newTitle);
                    waveIn.StartRecording();

                    while (newTitle == currentTitle)
                    {
                        currentTitle = GetWindowTitle(spotifyHandle);
                        Thread.Sleep(500);
                    }

                    waveIn.StopRecording();
                    writer.Dispose();
                    waveIn.Dispose();

                    PlayPause();
                    Console.WriteLine("Grabación finalizada. Audio guardado en: " + titleSong);
                    await SaveMetaData(outputFile, newTitle);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Grabación no finalizada con exito en la cancion => " + currentTitle, "With error => " + ex.Message);
                    PlayPause();
                }  
            }
        }

        private void PlayPause()
        {
            Process[] processes = Process.GetProcessesByName("Spotify");
            Process spotifyProcess = processes[0];
            IntPtr spotifyHandle = spotifyProcess.MainWindowHandle;
            SendMessageW(spotifyHandle, WM_APPCOMMAND, spotifyHandle, (IntPtr)APPCOMMAND_MEDIA_PLAY_PAUSE);
            Thread.Sleep(500);
        }

        static string GetWindowTitle(IntPtr hWnd)
        {
            const int nChars = 256;
            IntPtr buffer = IntPtr.Zero;

            try
            {
                if (hWnd == IntPtr.Zero)
                {
                    return string.Empty; // Or throw an exception, depending on your requirements
                }

                buffer = Marshal.AllocHGlobal(nChars * 2);

                if (buffer == IntPtr.Zero)
                {
                    throw new OutOfMemoryException("Failed to allocate memory for window title buffer.");
                }

                if (GetWindowText(hWnd, buffer, nChars) == 0)
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }

                return Marshal.PtrToStringUni(buffer) ?? string.Empty;
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
        }

        private async Task SaveMetaData(string file, string song)
        {
            var archivo = TagLib.File.Create(file);
            var artist = song.Split('-')[0];
            var nameSong = song.Split('-')[1];
            var information = await dezzer.GetInformationTrack(artist, nameSong);

            archivo.Tag.Title = nameSong;
            archivo.Tag.AlbumArtists = new[] { artist };

            if(information != null) 
            {
                archivo.Tag.Album = information.album.title;
                archivo.Tag.Track = (uint)information.track_position;
                archivo.Tag.BeatsPerMinute = (uint)information.bpm;
                archivo.Tag.Performers = information.contributors.Select(c => c.name).ToArray();
                archivo.Tag.Year = (uint)DateTime.Parse(information.release_date).Year;
            }

            // Guardar los cambios
            archivo.Save();
            archivo.Dispose();

            Console.WriteLine("Metadatos agregados correctamente al archivo MP3.");
        }

        [GeneratedRegex(@"[^\p{L}\d\s-]+")]
        private static partial Regex MyRegex();
    }
}
