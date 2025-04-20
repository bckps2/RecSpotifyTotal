// See https://aka.ms/new-console-template for more information
using ConsoleApp1.RecordingSporitfy;

Console.WriteLine("Hello, World!");

var spotify = new SpotifyRecord();

await spotify.StartRecording();
