using System.Media;
using NAudio.Wave;

namespace PacmanGameProject.Game.Services;

public class GameAudioService
{
    private SoundPlayer _eatSound;
    private bool _isChomping = false;
    private DateTime _lastPelletTime = DateTime.MinValue;
    private const int AUDIO_TIMEOUT_MS = 250;
    private IWavePlayer _bgmOutput;
    private AudioFileReader _bgmReader;

     public void SetupAudio()
     { 
         string baseDir = AppContext.BaseDirectory;
         string soundPath = Path.Combine(baseDir, "Assets", "sounds", "pacman_chomp.wav");
    
         if (File.Exists(soundPath))
         { 
             _eatSound = new SoundPlayer(soundPath);
             _eatSound.Load();
         }
    }

     public void SetupBackgroundMusic()
     {
         try
         {
             string bgmPath = Path.Combine(AppContext.BaseDirectory, "Assets", "sounds", "pacman-soundtrack.mp3");

             if (File.Exists(bgmPath))
             {
                 _bgmReader = new AudioFileReader(bgmPath);
                 _bgmReader.Volume = 0.15f; // 15% de volume sonoro

                 _bgmOutput = new WaveOutEvent();
                 _bgmOutput.Init(_bgmReader);

                 // Lógica de Loop: Quando acabar, volta pro início e toca de novo
                 _bgmOutput.PlaybackStopped += (sender, args) =>
                 {
                     if (_bgmReader != null)
                     {
                         _bgmReader.Position = 0;
                         _bgmOutput.Play();
                     }
                 };

                 _bgmOutput.Play();
             }
             else
             {
                 System.Diagnostics.Debug.WriteLine($"Música não encontrada em: {bgmPath}");
             }
         }
         catch (Exception ex)
         {
             System.Diagnostics.Debug.WriteLine($"Erro no NAudio: {ex.Message}");
         }
     }

      public void PelletEaten()
         {
             _lastPelletTime = DateTime.Now;
     
             if (!_isChomping && _eatSound != null)
             {
                 _isChomping = true;
                 _eatSound.PlayLooping();
             }
         }
     
     public void Update()
     {
         if (!_isChomping) return;

         var delta = DateTime.Now - _lastPelletTime;
         if (delta.TotalMilliseconds > AUDIO_TIMEOUT_MS)
         {
             _eatSound?.Stop();
             _isChomping = false;
         }
     }

     public void StopAll()
     {
         _bgmOutput?.Stop();
         _eatSound?.Stop();
     }

     public void Dispose()
     {
         _bgmOutput?.Dispose();
         _bgmReader?.Dispose();
     }
}

